using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Tháo Module đã xây — thành 1 item "đã gói" vào Storage, đặt lại tức thì qua
    /// <see cref="RedeployModuleCommand"/> (BL-P3-03, đổi 2026-07-29 — trước đó không hoàn gì).</summary>
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

        public void Execute(GameContext context)
        {
            BuildSystem.DismantleModule(context.World, context.Definitions, PlacementId);
            context.Events?.Publish(
                new InventoryChanged(InventoryOwner.ShelterStorage(ShelterModuleIds.LocationId).ToString()));
            context.Events?.Publish(new ModuleDismantled(PlacementId));
        }
    }
}
