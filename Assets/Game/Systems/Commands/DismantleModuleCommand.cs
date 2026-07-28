using LastHope.Core.Commands;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Tháo Module đã xây — không hoàn vật liệu (dismantle cơ bản, BL-P3-03).</summary>
    public class DismantleModuleCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SlotId;

        public DismantleModuleCommand(string slotId) => SlotId = slotId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Shelter.BuildSlots.ContainsKey(SlotId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Slot chưa có Module.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context) => BuildSystem.DismantleModule(context.World, SlotId);
    }
}
