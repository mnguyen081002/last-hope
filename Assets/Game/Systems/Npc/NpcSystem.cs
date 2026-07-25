using System;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Npc
{
    /// <summary>
    /// NPC consumption pressure (S16, npc-framework baseline): every recruited, living NPC's
    /// Hunger/Thirst accrue every long-tick; when a meter hits 100 the NPC tries to feed itself
    /// from shelter resources (same "search inventory for a matching item" pattern
    /// RestAtShelterCommand uses for medical items). Feeding succeeds → meter resets + Trust
    /// rises; fails → Trust drops and a starvation counter climbs, dropping Health a step once it
    /// crosses the threshold. A second, identical counter tracks standing in a Deep+ flooded
    /// shelter. Both share DowngradeHealth — one mechanism, two triggers, not two systems.
    /// </summary>
    public sealed class NpcSystem
    {
        private readonly GameContext _ctx;

        private enum FeedResult { NotNeeded, Fed, Shortage }

        public NpcSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            _ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter);

            NpcBalance cfg = _ctx.Definitions.Balance.Npc;

            foreach (var npc in _ctx.World.NpcStates.Values)
            {
                if (!npc.Recruited || npc.Health == NpcHealthState.Dead) continue;
                Tick(npc, shelter, cfg);
            }
        }

        private void Tick(NpcState npc, ShelterState shelter, NpcBalance cfg)
        {
            npc.Thirst = Math.Min(100f, npc.Thirst + cfg.ThirstPerLongTick);
            npc.Hunger = Math.Min(100f, npc.Hunger + cfg.HungerPerLongTick);

            var water = TryConsumeWater(npc, shelter, cfg);
            var food = TryConsumeFood(npc, shelter, cfg);

            if (water == FeedResult.Fed || food == FeedResult.Fed)
                npc.Trust = Math.Min(100, npc.Trust + cfg.TrustGainOnFed);

            bool shortage = water == FeedResult.Shortage || food == FeedResult.Shortage;
            if (shortage)
            {
                npc.Trust = Math.Max(0, npc.Trust - cfg.TrustLossOnHungry);
                npc.StarvingLongTicks++;
                if (npc.StarvingLongTicks >= cfg.StarvingLongTicksPerHealthDrop)
                {
                    npc.StarvingLongTicks = 0;
                    DowngradeHealth(npc, shelter);
                }
            }
            else
            {
                npc.StarvingLongTicks = 0;
            }

            bool floodedAtShelter = IsAtMainShelter(npc)
                && shelter != null && shelter.WaterIntrusion.Level >= WaterIntrusionLevel.Deep;
            if (floodedAtShelter)
            {
                npc.FloodExposureLongTicks++;
                if (npc.FloodExposureLongTicks >= cfg.FloodLongTicksPerHealthDrop)
                {
                    npc.FloodExposureLongTicks = 0;
                    DowngradeHealth(npc, shelter);
                }
            }
            else
            {
                npc.FloodExposureLongTicks = 0;
            }

            _ctx.Events.Publish(new NpcStateChanged(npc.Id));
        }

        private FeedResult TryConsumeWater(NpcState npc, ShelterState shelter, NpcBalance cfg)
        {
            if (npc.Thirst < 100f) return FeedResult.NotNeeded;
            if (shelter == null || shelter.WaterStocks.Clean < cfg.WaterConsumedPerFeed) return FeedResult.Shortage;

            shelter.WaterStocks.Clean -= cfg.WaterConsumedPerFeed;
            npc.Thirst = 0f;
            _ctx.Events.Publish(new WaterStocksChanged(shelter.Id));
            return FeedResult.Fed;
        }

        private FeedResult TryConsumeFood(NpcState npc, ShelterState shelter, NpcBalance cfg)
        {
            if (npc.Hunger < 100f) return FeedResult.NotNeeded;
            if (shelter?.Storage == null) return FeedResult.Shortage;

            foreach (var item in shelter.Storage.Items.Values)
            {
                if (item.Quantity <= 0 || !_ctx.Definitions.TryGetItem(item.ItemId, out var def) || def.Category != "food") continue;

                item.Quantity -= 1;
                if (item.Quantity <= 0) shelter.Storage.Items.Remove(item.InstanceId);
                InventoryOps.RecalculateLoad(shelter.Storage, _ctx.Definitions);
                _ctx.Events.Publish(new InventoryChanged(shelter.Storage.OwnerId));

                npc.Hunger = 0f;
                return FeedResult.Fed;
            }
            return FeedResult.Shortage;
        }

        /// <summary>Single-shelter assumption (matches WaterIntrusionSystem/PowerSystem, S10-S12)
        /// — any IsShelter location counts as "at the shelter" until S17 introduces a second one.</summary>
        private bool IsAtMainShelter(NpcState npc)
            => _ctx.Definitions.TryGetLocation(npc.LocationId, out var loc) && loc.IsShelter;

        private void DowngradeHealth(NpcState npc, ShelterState shelter)
        {
            if (npc.Health == NpcHealthState.Dead) return;

            npc.Health = (NpcHealthState)((int)npc.Health + 1);
            if (npc.Health == NpcHealthState.Dead)
            {
                if (shelter != null) shelter.Occupants = Math.Max(0, shelter.Occupants - 1);
                _ctx.Events.Publish(new NpcDied(npc.Id));
            }
        }
    }
}
