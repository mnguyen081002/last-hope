using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Systems.Commands
{
    public class SetPowerPriorityCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string PlacementId;
        public PowerPriority Priority;

        public SetPowerPriorityCommand(string placementId, PowerPriority priority)
        {
            PlacementId = placementId;
            Priority = priority;
        }

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Shelter.PlacedModules.ContainsKey(PlacementId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Không tìm thấy Module này.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            var module = context.World.Shelter.PlacedModules[PlacementId];
            module.Priority = Priority;
            context.Events?.Publish(new PowerPriorityChanged(PlacementId, module.ModuleId, Priority.ToString()));
        }
    }
}
