using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Tháo Module đã xây — thành 1 item "đã gói" vào túi Player (đổi 2026-07-30, trước
    /// đó vào Storage), đặt lại tức thì qua <see cref="RedeployModuleCommand"/> (BL-P3-03, đổi
    /// 2026-07-29 — trước đó không hoàn gì).</summary>
    public class DismantleModuleCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string PlacementId;

        public DismantleModuleCommand(string placementId) => PlacementId = placementId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Shelter.PlacedModules.ContainsKey(PlacementId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Không tìm thấy Module này.");

            var reason = BuildSystem.CanDismantle(context.World, context.Definitions, PlacementId);
            if (reason == BuildRejectReason.InventoryFull)
                return CommandResult.Fail(CommandErrorCode.NotEnoughCapacity, "Không đủ chỗ trong túi để tháo.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            BuildSystem.DismantleModule(context.World, context.Definitions, PlacementId);
            context.Events?.Publish(new InventoryChanged(InventoryOwner.Player.ToString()));
            context.Events?.Publish(new ModuleDismantled(PlacementId));
        }
    }
}
