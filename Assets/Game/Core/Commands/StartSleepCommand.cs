namespace LastHope.Core.Commands
{
    /// <summary>
    /// Sleep fast-forward (technical-specification.md mục 9/§9). Interrupt-on-event handling
    /// arrives with the Event System (M3+); for M1 this advances the clock tick-by-tick only.
    /// </summary>
    public sealed class StartSleepCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId => null;
        public long WorldTime { get; set; }
        public int Minutes { get; }

        public StartSleepCommand(string actorId, int minutes)
        {
            ActorId = actorId;
            Minutes = minutes;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ActorId != ctx.World.Player.ActorId)
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown actor '{ActorId}'.");
            if (Minutes <= 0)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Sleep duration must be positive.");
            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Clock.FastForward(Minutes);
        }
    }
}
