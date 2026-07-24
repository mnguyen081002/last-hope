using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Hazard
{
    /// <summary>
    /// Keeps RouteState in sync with the disaster timeline (BL-P1 S8) — a cached snapshot for
    /// display (World Map). BeginTravelCommand does NOT read this; it recomputes fresh via
    /// HazardRules at the exact moment of travel, since this snapshot can be up to 9 minutes
    /// stale between long ticks.
    /// </summary>
    public sealed class HazardSystem
    {
        private readonly GameContext _ctx;

        public HazardSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(_ => RecomputeAll());
            ctx.Events.Subscribe<WorldStateReloaded>(_ => RecomputeAll());
            RecomputeAll();
        }

        private void RecomputeAll()
        {
            foreach (var route in _ctx.Definitions.Routes.Values)
                Recompute(route);
        }

        private void Recompute(RouteDefinition route)
        {
            if (!_ctx.World.RouteStates.TryGetValue(route.Id, out var state))
            {
                state = new RouteState { Id = route.Id };
                _ctx.World.RouteStates[route.Id] = state;
            }

            var levels = HazardRules.EvaluateRoute(route, _ctx.Definitions.DisasterPhasesSorted, _ctx.World.WorldTimeMinutes);
            bool contamination = _ctx.Definitions.TryGetDisasterPhase(_ctx.World.CurrentDisasterPhase, out var phase) && phase.BlackWater;
            bool closed = levels.FloodLevel >= HazardRules.MaxLevel || levels.CurrentLevel >= HazardRules.MaxLevel;

            bool changed = state.FloodLevel != levels.FloodLevel
                || state.CurrentLevel != levels.CurrentLevel
                || state.Contamination != contamination
                || state.Closed != closed;

            if (!changed) return;

            state.FloodLevel = levels.FloodLevel;
            state.CurrentLevel = levels.CurrentLevel;
            state.Contamination = contamination;
            state.Closed = closed;

            _ctx.Events.Publish(new RouteStateChanged(route.Id));
        }
    }
}
