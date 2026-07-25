using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Applies a chosen response to an Active event (S13). Response effects are a small switch on
    /// response id rather than fully data-driven resolution_rules (event-system-design.md §11) —
    /// S13 ships exactly 3 events with 1 response each; a rules engine isn't worth building until
    /// there's enough content to need one.
    /// </summary>
    public sealed class ResolveEventCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // event instance id
        public long WorldTime { get; set; }
        private readonly string _responseId;

        public ResolveEventCommand(string actorId, string eventInstanceId, string responseId)
        {
            ActorId = actorId;
            TargetId = eventInstanceId;
            _responseId = responseId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            var instance = ctx.World.ActiveEvents.Find(e => e.EventInstanceId == TargetId);
            if (instance == null)
                return CommandResult.Fail(CommandErrorCode.EventNotActive);
            if (instance.State == EventLifecycleState.Undiscovered)
                return CommandResult.Fail(CommandErrorCode.EventNotDiscovered);
            if (instance.State != EventLifecycleState.Active)
                return CommandResult.Fail(CommandErrorCode.EventNotActive);

            if (!ctx.Definitions.TryGetEvent(instance.EventId, out var def) || !def.AvailableResponses.Contains(_responseId))
                return CommandResult.Fail(CommandErrorCode.ResponseUnavailable);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            var instance = ctx.World.ActiveEvents.Find(e => e.EventInstanceId == TargetId);
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];

            switch (_responseId)
            {
                case "reinforce_seal":
                    shelter.EventFlags.Remove(ShelterEventFlags.DrainBackflowActive);
                    ctx.Clock.FastForward(15);
                    break;
                case "clear_pump":
                    shelter.EventFlags.Remove(ShelterEventFlags.PumpJammed);
                    ctx.Clock.FastForward(15);
                    break;
                case "secure_storage":
                    // Best-effort move to safety — S13 doesn't yet simulate the "left unresolved"
                    // failure case (no auto-expire), so resolving is the whole effect for now.
                    break;
            }

            instance.ChosenResponse = _responseId;
            instance.State = EventLifecycleState.Resolved;
            ctx.Events.Publish(new EventResolved(instance.EventInstanceId, instance.EventId, _responseId));
        }
    }
}
