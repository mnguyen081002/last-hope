using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>Appends to WorldState.DecisionLog (S18, Causal Outcome Report source) — called
    /// from the 4 commands the plan names as "major decisions": ResolveEventCommand,
    /// StartBuildCommand, RecruitNpcCommand, EvacuateCommand.</summary>
    public static class DecisionLog
    {
        public static void Append(GameContext ctx, string decisionId, string payload)
        {
            ctx.World.DecisionLog.Add(new DecisionLogEntry
            {
                Minute = ctx.World.WorldTimeMinutes,
                DecisionId = decisionId,
                Payload = payload,
            });
        }
    }
}
