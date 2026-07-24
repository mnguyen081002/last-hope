using System;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Events
{
    /// <summary>
    /// Evaluates every EventDefinition against current world/shelter state each long-tick (S13,
    /// event-system-design.md §6) and triggers the ones whose conditions hold. S13 only implements
    /// Dormant→Active (skips Undiscovered/Discovered gating — nothing here does Event Discovery
    /// yet) and Resolved (via ResolveEventCommand); Expired/PersistentConsequence are declared on
    /// ActiveEventState but unused until S14 enforces deadlines.
    /// </summary>
    public sealed class EventSystem
    {
        private readonly GameContext _ctx;

        public EventSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
        }

        private void OnLongTick(long minute)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase);
            bool hasActivePump = HasActiveModuleTag(shelter, "pump");

            foreach (var def in _ctx.Definitions.Events.Values)
            {
                if (_ctx.World.ActiveEvents.Exists(e => e.EventId == def.Id && e.State != EventLifecycleState.Resolved))
                    continue;

                int? roll = def.TriggerChancePercentPerLongTick > 0
                    ? _ctx.Rng.GetStream("events").NextInt(0, 100)
                    : (int?)null;

                if (!EventTriggerRules.Evaluate(def, phase, shelter, hasActivePump, roll)) continue;

                Trigger(def, shelter);
            }
        }

        private void Trigger(EventDefinition def, ShelterState shelter)
        {
            var instance = new ActiveEventState
            {
                EventInstanceId = Guid.NewGuid().ToString("N"),
                EventId = def.Id,
                State = EventLifecycleState.Active,
                TriggeredAtMinute = _ctx.World.WorldTimeMinutes,
                DeadlineMinute = def.HardDeadlineMinutes > 0 ? _ctx.World.WorldTimeMinutes + def.HardDeadlineMinutes : (long?)null,
            };
            _ctx.World.ActiveEvents.Add(instance);

            if (def.Tags.Contains("drain_backflow"))
                shelter.EventFlags.Add(ShelterEventFlags.DrainBackflowActive);
            else if (def.Tags.Contains("pump_jam"))
                shelter.EventFlags.Add(ShelterEventFlags.PumpJammed);
            // "storage_flood_risk" has no trigger-time state effect — it's a warning the player
            // resolves before the risk becomes real; S13 doesn't yet simulate that "real" failure.

            _ctx.Events.Publish(new EventTriggered(instance.EventInstanceId, def.Id));
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
