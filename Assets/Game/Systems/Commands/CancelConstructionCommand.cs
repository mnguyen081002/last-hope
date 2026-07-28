using LastHope.Core.Commands;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Huỷ công trình đang xây — không hoàn vật liệu (chưa có số liệu refund). Chỉ một construction cùng lúc.</summary>
    public class CancelConstructionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public CommandResult Validate(GameContext context)
        {
            if (context.World.Shelter.Construction == null)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Không có công trình đang xây.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context) => BuildSystem.CancelConstruction(context.World);
    }
}
