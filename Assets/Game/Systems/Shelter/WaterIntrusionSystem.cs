using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Drives the single Main Shelter's WaterIntrusionState from the world clock (S10,
    /// main-shelter-design.md §21-22). LongTick only — same 10-minute granularity as
    /// hunger/thirst/fatigue accrual. Resyncs (creates the shelter if missing, seeds it from
    /// ShelterBalance/ShelterZoneDefinitions) on construct AND WorldStateReloaded, same pattern as
    /// DisasterPhaseSystem — a loaded save may reference a shelter this instance hasn't seen yet.
    /// </summary>
    public sealed class WaterIntrusionSystem
    {
        public const string FlagLowerFloorPowerLocked = "lower_floor_power_locked";
        public const string FlagGroundFloorLost = "ground_floor_lost";

        private readonly GameContext _ctx;

        public WaterIntrusionSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
            ctx.Events.Subscribe<WorldStateReloaded>(_ => Resync());
            Resync();
        }

        private void Resync()
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
            {
                shelter = new ShelterState { Id = shelterId };
                _ctx.World.ShelterStates[shelterId] = shelter;
            }

            SeedIfNew(shelter);
            EnsureBuildSlots(shelter);
        }

        private void SeedIfNew(ShelterState shelter)
        {
            if (shelter.StructuralIntegrity > 0f) return; // already seeded (or loaded from save)

            ShelterBalance cfg = _ctx.Definitions.Balance.Shelter;
            shelter.StructuralIntegrity = cfg.InitialStructuralIntegrity;
            shelter.LivingCapacity = cfg.InitialLivingCapacity;
            shelter.Occupants = 1; // the player
            shelter.WaterStocks.Clean = cfg.InitialCleanWater;
            shelter.WaterStocks.Untreated = cfg.InitialUntreatedWater;
        }

        private void EnsureBuildSlots(ShelterState shelter)
        {
            foreach (var zone in _ctx.Definitions.ShelterZones.Values)
            {
                foreach (string slotId in zone.BuildSlotIds)
                {
                    if (!shelter.BuildSlots.ContainsKey(slotId))
                        shelter.BuildSlots[slotId] = new BuildSlotState();
                }
            }
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            ShelterBalance cfg = _ctx.Definitions.Balance.Shelter;
            int rainIntensity = _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase)
                ? phase.RainIntensity : 0;
            bool backflowActive = false; // wired to EventFlags by S13's Drain Backflow event

            int activePumpCount = CountActiveModulesByTag(shelter, "pump");
            ModuleState barrier = FindActiveModuleByTag(shelter, "barrier");
            bool hasActiveBarrier = barrier != null;

            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity, backflowActive, activePumpCount, hasActiveBarrier, cfg);
            shelter.WaterIntrusion.Units = WaterIntrusionRules.Clamp01To100(shelter.WaterIntrusion.Units + delta);

            if (barrier != null && shelter.WaterIntrusion.Level > WaterIntrusionLevel.Dry)
                DecayBarrier(barrier, cfg);

            WaterIntrusionLevel newLevel = WaterIntrusionRules.LevelFor(shelter.WaterIntrusion.Units, cfg);
            if (newLevel == shelter.WaterIntrusion.Level) return;

            shelter.WaterIntrusion.Level = newLevel;
            UpdateFlags(shelter, newLevel);
            _ctx.Events.Publish(new ShelterWaterChanged(shelterId, newLevel));
        }

        private void DecayBarrier(ModuleState barrier, ShelterBalance cfg)
        {
            barrier.Durability -= cfg.BarrierDurabilityDecayPerLongTick;
            if (barrier.Durability <= 0f)
            {
                barrier.Durability = 0f;
                barrier.Active = false; // destroyed — no longer blocks inflow
            }
        }

        private int CountActiveModulesByTag(ShelterState shelter, string tag)
        {
            int count = 0;
            foreach (var module in shelter.Modules.Values)
            {
                if (!module.Active) continue;
                if (_ctx.Definitions.TryGetModule(module.ModuleId, out var def) && def.Tags.Contains(tag)) count++;
            }
            return count;
        }

        private ModuleState FindActiveModuleByTag(ShelterState shelter, string tag)
        {
            foreach (var module in shelter.Modules.Values)
            {
                if (!module.Active) continue;
                if (_ctx.Definitions.TryGetModule(module.ModuleId, out var def) && def.Tags.Contains(tag)) return module;
            }
            return null;
        }

        private static void UpdateFlags(ShelterState shelter, WaterIntrusionLevel level)
        {
            if (level >= WaterIntrusionLevel.Deep) shelter.EventFlags.Add(FlagLowerFloorPowerLocked);
            else shelter.EventFlags.Remove(FlagLowerFloorPowerLocked);

            if (level >= WaterIntrusionLevel.Critical) shelter.EventFlags.Add(FlagGroundFloorLost);
            else shelter.EventFlags.Remove(FlagGroundFloorLost);
        }
    }
}
