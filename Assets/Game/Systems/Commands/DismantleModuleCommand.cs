using LastHope.Core.Commands;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Tháo Module đã xây — không hoàn vật liệu (dismantle cơ bản, BL-P3-03).</summary>
    public class DismantleModuleCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string PlacementId;

        public DismantleModuleCommand(string placementId) => PlacementId = placementId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Shelter.PlacedModules.ContainsKey(PlacementId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Không tìm thấy Module này.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context) => BuildSystem.DismantleModule(context.World, PlacementId);
    }
}
