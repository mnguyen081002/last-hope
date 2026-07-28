using LastHope.Core.Commands;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Huỷ công trình đang xây — không hoàn vật liệu (chưa có số liệu refund).</summary>
    public class CancelConstructionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SlotId;

        public CancelConstructionCommand(string slotId) => SlotId = slotId;

        public CommandResult Validate(GameContext context)
        {
            var construction = context.World.Shelter.Construction;
            if (construction == null || construction.SlotId != SlotId)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Không có công trình đang xây ở slot này.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context) => BuildSystem.CancelConstruction(context.World, SlotId);
    }
}
