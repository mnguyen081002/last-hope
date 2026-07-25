using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;

namespace LastHope.Systems.Outcome
{
    /// <summary>
    /// Evaluates the slice ending exactly once (S18): early on an immediate-fail condition
    /// (player Incapacitated), or once the timeline reaches its final DisasterPhase (read
    /// dynamically from DisasterPhasesSorted's last entry — not a hardcoded "phase_p4_end" id, so
    /// this doesn't need editing if the content's phase list changes). Persists the
    /// "outcome_reached" flag so a save/load mid-slice can't re-evaluate or duplicate-publish —
    /// same reasoning as EventSystem avoiding TickScheduler.RegisterThreshold: nothing here can
    /// rely on an unserialized callback still being registered after a load.
    /// </summary>
    public sealed class OutcomeSystem
    {
        private readonly GameContext _ctx;
        private bool _reached;

        public OutcomeSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
            ctx.Events.Subscribe<WorldStateReloaded>(_ => Resync());
            Resync();
        }

        private void Resync()
        {
            _reached = _ctx.World.PersistentFlags.TryGetValue("outcome_reached", out bool r) && r;
        }

        private void OnLongTick(long minute)
        {
            if (_reached) return;

            bool collapsedNow = ConditionOps.IsIncapacitated(_ctx.World.Player.Condition);
            var phases = _ctx.Definitions.DisasterPhasesSorted;
            bool finalPhaseReached = phases.Count > 0 && _ctx.World.CurrentDisasterPhase == phases[phases.Count - 1].Id;

            if (!collapsedNow && !finalPhaseReached) return;

            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            var outcome = OutcomeRules.Evaluate(_ctx.World, shelter, _ctx.Definitions.Balance.Slice);

            _reached = true;
            _ctx.World.PersistentFlags["outcome_reached"] = true;
            _ctx.Events.Publish(new OutcomeReached(outcome));
        }
    }
}
