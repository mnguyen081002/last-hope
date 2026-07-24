using System.Collections.Generic;
using System.Linq;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Disaster
{
    /// <summary>
    /// Determines WorldState.CurrentDisasterPhase from DisasterPhaseDefinition.StartMinute
    /// thresholds (BL-P1 S7). Resyncs on construct AND on WorldStateReloaded — a loaded save may
    /// land at a different WorldTimeMinutes than the one this instance was built for, so both the
    /// "current phase" recompute and the future-threshold registration must redo. Threshold
    /// callbacks are idempotent (TransitionTo no-ops if the phase already matches), so re-running
    /// RegisterFutureThresholds after a reload is safe even if a threshold was already registered.
    /// </summary>
    public sealed class DisasterPhaseSystem
    {
        private readonly GameContext _ctx;
        private readonly List<DisasterPhaseDefinition> _phasesByStart;

        public DisasterPhaseSystem(GameContext ctx)
        {
            _ctx = ctx;
            _phasesByStart = ctx.Definitions.DisasterPhases.Values.OrderBy(p => p.StartMinute).ToList();

            ctx.Events.Subscribe<WorldStateReloaded>(_ => Resync());
            Resync();
        }

        private void Resync()
        {
            RecomputeCurrentPhase();
            RegisterFutureThresholds();
        }

        private void RecomputeCurrentPhase()
        {
            DisasterPhaseDefinition current = null;
            foreach (var phase in _phasesByStart)
            {
                if (phase.StartMinute > _ctx.World.WorldTimeMinutes) break;
                current = phase;
            }
            if (current != null) _ctx.World.CurrentDisasterPhase = current.Id;
        }

        private void RegisterFutureThresholds()
        {
            foreach (var phase in _phasesByStart)
            {
                if (phase.StartMinute <= _ctx.World.WorldTimeMinutes) continue;
                _ctx.Clock.RegisterThreshold(phase.StartMinute, _ => TransitionTo(phase));
            }
        }

        private void TransitionTo(DisasterPhaseDefinition phase)
        {
            string from = _ctx.World.CurrentDisasterPhase;
            if (from == phase.Id) return;

            _ctx.World.CurrentDisasterPhase = phase.Id;
            _ctx.Events.Publish(new DisasterPhaseChanged(from, phase.Id));
        }
    }
}
