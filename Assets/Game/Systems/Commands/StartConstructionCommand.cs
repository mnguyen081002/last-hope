using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Production (BL-P3-03, đổi 2026-07-30) — sản xuất Module tại Shelter Console,
    /// không gắn Zone/vị trí. Xong chuyển "Ready to Claim", đặt vào world qua
    /// <see cref="RedeployModuleCommand"/> sau khi Nhận.</summary>
    public class StartConstructionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ModuleId;

        public StartConstructionCommand(string moduleId) => ModuleId = moduleId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
                || !location.IsShelter)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ sản xuất được khi đang ở Shelter.");

            var reason = BuildSystem.CanStartProduction(context.World, context.Definitions, ModuleId, out _);
            return reason switch
            {
                BuildRejectReason.None => CommandResult.Ok(),
                BuildRejectReason.UnknownModule => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ModuleId),
                BuildRejectReason.ConstructionInProgress => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Đang sản xuất Module khác — chỉ một cái cùng lúc."),
                BuildRejectReason.NotEnoughMaterials => CommandResult.Fail(
                    CommandErrorCode.ItemNotFound, "Không đủ vật liệu trong kho Shelter."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            BuildSystem.StartConstruction(context.World, context.Definitions, ModuleId);
            context.Events?.Publish(
                new InventoryChanged(InventoryOwner.ShelterStorage(ShelterModuleIds.LocationId).ToString()));

            int minutes = context.Definitions.GetModule(ModuleId).BuildMinutes;
            context.Events?.Publish(new ConstructionStarted(ModuleId, minutes));
        }
    }
}
