using System;
using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Events
{
    /// <summary>
    /// Evaluates every EventDefinition against current world/shelter state each long-tick and
    /// runs the full instance lifecycle (S13 trigger core + S14 completion,
    /// event-system-design.md §5-6/§14): trigger → Active (or Undiscovered when
    /// RequiresDiscovery), discovery (being at a shelter — radio/NPC sources arrive with S15),
    /// soft deadline warning, hard-deadline expiration with Persistent Consequence flags, and
    /// event chains (NextEventId force-triggered on resolve/expire). Deadlines are checked every
    /// long-tick rather than via TickScheduler.RegisterThreshold — threshold registrations aren't
    /// serialized, so they'd be lost on save/load. Off-scene by default: nothing here reads
    /// player position except the discovery check.
    /// </summary>
    public sealed class EventSystem
    {
        private readonly GameContext _ctx;

        public EventSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
            ctx.Events.Subscribe<EventResolved>(OnEventResolved);
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase);
            bool hasActivePump = HasActiveModuleTag(shelter, "pump");

            foreach (var def in _ctx.Definitions.Events.Values)
            {
                // One live-or-spent instance per event id: Active/Undiscovered block re-trigger,
                // and Expired/PersistentConsequence block it permanently — only Resolved frees the
                // id to trigger again if conditions return.
                if (_ctx.World.ActiveEvents.Exists(e => e.EventId == def.Id && e.State != EventLifecycleState.Resolved))
                    continue;

                int? roll = def.TriggerChancePercentPerLongTick > 0
                    ? _ctx.Rng.GetStream("events").NextInt(0, 100)
                    : (int?)null;

                if (!EventTriggerRules.Evaluate(def, phase, shelter, hasActivePump, roll)) continue;

                Trigger(def, shelter);
            }

            AdvanceLifecycles(minute, shelter);
        }

        private void Trigger(EventDefinition def, ShelterState shelter)
        {
            var instance = new ActiveEventState
            {
                EventInstanceId = Guid.NewGuid().ToString("N"),
                EventId = def.Id,
                State = def.RequiresDiscovery ? EventLifecycleState.Undiscovered : EventLifecycleState.Active,
                TriggeredAtMinute = _ctx.World.WorldTimeMinutes,
            };
            if (!def.RequiresDiscovery) ArmDeadlines(instance, def);
            _ctx.World.ActiveEvents.Add(instance);

            // World effects apply at trigger regardless of discovery — the backflow starts whether
            // or not the player knows about it (off-scene principle).
            if (def.Tags.Contains("drain_backflow"))
                shelter.EventFlags.Add(ShelterEventFlags.DrainBackflowActive);
            else if (def.Tags.Contains("pump_jam"))
                shelter.EventFlags.Add(ShelterEventFlags.PumpJammed);
            // "storage_flood_risk" has no trigger-time state effect — it's a warning the player
            // resolves before the risk becomes real.

            if (instance.State == EventLifecycleState.Active)
                _ctx.Events.Publish(new EventTriggered(instance.EventInstanceId, def.Id));
        }

        /// <summary>Deadlines run from the moment the instance becomes Active — trigger time for
        /// auto-discovered events, discovery time for RequiresDiscovery events.</summary>
        private void ArmDeadlines(ActiveEventState instance, EventDefinition def)
        {
            long now = _ctx.World.WorldTimeMinutes;
            instance.DeadlineMinute = def.HardDeadlineMinutes > 0 ? now + def.HardDeadlineMinutes : (long?)null;
            instance.SoftDeadlineMinute = def.SoftDeadlineMinutes > 0 ? now + def.SoftDeadlineMinutes : (long?)null;
        }

        private void AdvanceLifecycles(long minute, ShelterState shelter)
        {
            bool playerAtShelter = _ctx.Definitions.TryGetLocation(_ctx.World.Player.CurrentLocationId, out var loc) && loc.IsShelter;

            // Snapshot: expiring an event may chain-trigger a new instance mid-iteration.
            var instances = new List<ActiveEventState>(_ctx.World.ActiveEvents);
            foreach (var instance in instances)
            {
                if (!_ctx.Definitions.TryGetEvent(instance.EventId, out var def)) continue;

                if (instance.State == EventLifecycleState.Undiscovered)
                {
                    if (!playerAtShelter) continue;
                    instance.State = EventLifecycleState.Active;
                    ArmDeadlines(instance, def);
                    _ctx.Events.Publish(new EventDiscovered(instance.EventId));
                    continue;
                }

                if (instance.State != EventLifecycleState.Active) continue;

                if (instance.SoftDeadlineMinute.HasValue && !instance.SoftDeadlineNotified && minute >= instance.SoftDeadlineMinute.Value)
                {
                    instance.SoftDeadlineNotified = true;
                    _ctx.Events.Publish(new EventDeadlineApproaching(instance.EventInstanceId, instance.EventId, instance.DeadlineMinute ?? 0));
                }

                if (instance.DeadlineMinute.HasValue && minute >= instance.DeadlineMinute.Value)
                    Expire(instance, def, shelter);
            }
        }

        private void Expire(ActiveEventState instance, EventDefinition def, ShelterState shelter)
        {
            bool persistent = def.ExpirationShelterFlags.Count > 0 || def.ExpirationPersistentFlags.Count > 0;
            foreach (string flag in def.ExpirationShelterFlags) shelter.EventFlags.Add(flag);
            foreach (string flag in def.ExpirationPersistentFlags) _ctx.World.PersistentFlags[flag] = true;

            instance.State = persistent ? EventLifecycleState.PersistentConsequence : EventLifecycleState.Expired;
            _ctx.Events.Publish(new EventExpired(instance.EventInstanceId, instance.EventId));

            ChainNext(def, shelter);
        }

        private void OnEventResolved(EventResolved evt)
        {
            if (!_ctx.Definitions.TryGetEvent(evt.EventId, out var def)) return;

            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            ChainNext(def, shelter);
        }

        /// <summary>Event Chain: the next event is force-triggered, bypassing its own trigger
        /// conditions — the chain IS its trigger. Dedup still applies.</summary>
        private void ChainNext(EventDefinition def, ShelterState shelter)
        {
            if (string.IsNullOrEmpty(def.NextEventId)) return;
            if (!_ctx.Definitions.TryGetEvent(def.NextEventId, out var next)) return;
            if (_ctx.World.ActiveEvents.Exists(e => e.EventId == next.Id && e.State != EventLifecycleState.Resolved)) return;

            Trigger(next, shelter);
        }

        private bool HasActiveModuleTag(ShelterState shelter, string tag)
        {
            foreach (var module in shelter.Modules.Values)
                if (module.Active && _ctx.Definitions.TryGetModule(module.ModuleId, out var def) && def.Tags.Contains(tag))
                    return true;
            return false;
        }
    }
}
