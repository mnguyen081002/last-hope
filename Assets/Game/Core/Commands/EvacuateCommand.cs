using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Commits to abandoning the main shelter (S18) — only meaningful once the shelter has
    /// actually failed (GroundFloorLost). Leaves shelter storage behind ("bỏ storage lại" per the
    /// plan) and marks the decision; the player still has to physically travel to safety via the
    /// existing BeginTravelCommand — this command doesn't teleport them or spin up a second
    /// simulated shelter (S17 scope-cut "Temporary Shelter nâng cấp"). OutcomeRules checks both
    /// this flag and the player's final location to decide Forced Evacuation vs. Collapse.
    /// </summary>
    public sealed class EvacuateCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId => "evacuate";
        public long WorldTime { get; set; }

        public EvacuateCommand(string actorId)
        {
            ActorId = actorId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ctx.World.Player.CurrentLocationId != ctx.Definitions.Balance.NewGame.StartLocationId)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Must be at the main shelter to evacuate.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Shelter not initialized.");
            if (!shelter.EventFlags.Contains(ShelterEventFlags.GroundFloorLost))
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Shelter isn't lost — nothing to evacuate from.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];
            shelter.Storage?.Items.Clear();

            ctx.World.PersistentFlags["evacuated"] = true;

            DecisionLog.Append(ctx, "evacuate", ctx.World.Player.CurrentLocationId);
            ctx.Events.Publish(new EvacuationDeclared());
        }
    }
}
