using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Đặt lại Module đã Tháo (đã gói, xem <see cref="DismantleModuleCommand"/>) —
    /// tức thì, không qua Construction/BuildMinutes như <see cref="StartConstructionCommand"/>.</summary>
    public class RedeployModuleCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ZoneId;
        public float PositionX;
        public float PositionY;
        public string ModuleId;

        public RedeployModuleCommand(string zoneId, float positionX, float positionY, string moduleId)
        {
            ZoneId = zoneId;
            PositionX = positionX;
            PositionY = positionY;
            ModuleId = moduleId;
        }

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
                || !location.IsShelter)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ đặt lại được khi đang ở Shelter.");

            var reason = BuildSystem.CanRedeployAt(
                context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId);
            return reason switch
            {
                BuildRejectReason.None => CommandResult.Ok(),
                BuildRejectReason.UnknownZone => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ZoneId),
                BuildRejectReason.UnknownModule => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ModuleId),
                BuildRejectReason.WrongZone => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, $"'{ModuleId}' không đặt được ở zone này."),
                BuildRejectReason.OutOfBounds => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, "Vị trí nằm ngoài biên Zone."),
                BuildRejectReason.Overlapping => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Vị trí chồng lấn Module khác."),
                BuildRejectReason.NotEnoughPackedModules => CommandResult.Fail(
                    CommandErrorCode.ItemNotFound, "Không có Module đã gói sẵn trong kho Shelter."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            string placementId = BuildSystem.RedeployModule(
                context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId);
            context.Events?.Publish(
                new InventoryChanged(InventoryOwner.ShelterStorage(ShelterModuleIds.LocationId).ToString()));
            context.Events?.Publish(new ModuleRedeployed(placementId, ModuleId));
        }
    }
}
