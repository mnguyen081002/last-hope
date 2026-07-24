using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Sleep fast-forward with interrupt (S12, technical-specification.md §9). Validates the
    /// player isn't Incapacitated and the shelter isn't Deep+ flooded (main-shelter-design.md
    /// §25 "Lower Floor Lost" is the closest documented "unsafe" threshold — no literal Bed
    /// content exists yet in S11's module set, so NoBedAvailable stays declared-but-unused until
    /// a Bed module is real). Wakes early if the shelter crosses into Deep/Critical flooding
    /// mid-sleep — S14 layers Event-priority wake conditions onto this without changing the shape.
    /// </summary>
    public sealed class StartSleepCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId => "sleep";
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
            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");
            if (ConditionOps.IsIncapacitated(ctx.World.Player.Condition))
                return CommandResult.Fail(CommandErrorCode.Incapacitated);
            if (IsShelterUnsafe(ctx))
                return CommandResult.Fail(CommandErrorCode.UnsafeToSleep, "Shelter is flooded — unsafe to sleep.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Events.Publish(new SleepStarted());

            bool interrupted = false;
            int elapsed = ctx.Clock.FastForward(Minutes, _ =>
            {
                if (!IsShelterUnsafe(ctx)) return false;
                interrupted = true;
                return true;
            });

            if (interrupted) ctx.Events.Publish(new SleepInterrupted(elapsed));
            else ctx.Events.Publish(new SleepEnded());
        }

        private static bool IsShelterUnsafe(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            return ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)
                && shelter.WaterIntrusion.Level >= WaterIntrusionLevel.Deep;
        }
    }
}
