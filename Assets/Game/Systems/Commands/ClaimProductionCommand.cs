using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Nhận sản phẩm Production đã "Ready to Claim" (BL-P3-03, 2026-07-30) — cộng
    /// packed item vào túi Player qua <see cref="InventorySystem.Add"/>.</summary>
    public class ClaimProductionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ModuleId;

        public ClaimProductionCommand(string moduleId) => ModuleId = moduleId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
                || !location.IsShelter)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ nhận được khi đang ở Shelter.");

            var reason = BuildSystem.CanClaim(context.World, context.Definitions, ModuleId, out _);
            return reason switch
            {
                BuildRejectReason.None => CommandResult.Ok(),
                BuildRejectReason.UnknownModule => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ModuleId),
                BuildRejectReason.NothingToClaim => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, "Không có sản phẩm nào đang chờ nhận."),
                BuildRejectReason.InventoryFull => CommandResult.Fail(
                    CommandErrorCode.NotEnoughCapacity, "Không đủ chỗ trong túi."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            BuildSystem.ClaimProduction(context.World, context.Definitions, ModuleId);
            context.Events?.Publish(new InventoryChanged(InventoryOwner.Player.ToString()));
            context.Events?.Publish(new ProductionClaimed(ModuleId));
        }
    }
}
