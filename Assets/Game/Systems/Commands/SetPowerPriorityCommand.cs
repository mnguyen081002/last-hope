using LastHope.Core.Commands;
using LastHope.Core.State;

namespace LastHope.Systems.Commands
{
    public class SetPowerPriorityCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SlotId;
        public PowerPriority Priority;

        public SetPowerPriorityCommand(string slotId, PowerPriority priority)
        {
            SlotId = slotId;
            Priority = priority;
        }

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Shelter.BuildSlots.ContainsKey(SlotId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Slot chưa có Module.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context) =>
            context.World.Shelter.BuildSlots[SlotId].Priority = Priority;
    }
}
