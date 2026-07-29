using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Free Placement (BL-P3-03) — đặt Module tự do bằng world position trong Zone hợp lệ.</summary>
    public class StartConstructionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ZoneId;
        public float PositionX;
        public float PositionY;
        public string ModuleId;

        public StartConstructionCommand(string zoneId, float positionX, float positionY, string moduleId)
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
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ xây được khi đang ở Shelter.");

            var reason = BuildSystem.CanPlaceAt(
                context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId);
            return reason switch
            {
                BuildRejectReason.None => CommandResult.Ok(),
                BuildRejectReason.UnknownZone => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ZoneId),
                BuildRejectReason.UnknownModule => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ModuleId),
                BuildRejectReason.WrongZone => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, $"'{ModuleId}' không xây được ở zone này."),
                BuildRejectReason.OutOfBounds => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, "Vị trí nằm ngoài biên Zone."),
                BuildRejectReason.Overlapping => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Vị trí chồng lấn Module khác."),
                BuildRejectReason.ConstructionInProgress => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Đang xây công trình khác — chỉ một construction cùng lúc."),
                BuildRejectReason.NotEnoughMaterials => CommandResult.Fail(
                    CommandErrorCode.ItemNotFound, "Không đủ vật liệu trong kho Shelter."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            BuildSystem.StartConstruction(context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId);
            context.Events?.Publish(
                new InventoryChanged(InventoryOwner.ShelterStorage(ShelterModuleIds.LocationId).ToString()));

            int minutes = context.Definitions.GetModule(ModuleId).BuildMinutes;
            context.Events?.Publish(new ConstructionStarted(ZoneId, ModuleId, minutes));
        }
    }
}
