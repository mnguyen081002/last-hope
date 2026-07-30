using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    /// <summary>Đặt Module đã gói — từ Claim (<see cref="ClaimProductionCommand"/>) hoặc Tháo
    /// (<see cref="DismantleModuleCommand"/>), packed item đều nằm trong túi Player (đổi
    /// 2026-07-30) — tức thì, không qua Construction/BuildMinutes như
    /// <see cref="StartConstructionCommand"/>.</summary>
    public class RedeployModuleCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ZoneId;
        public float PositionX;
        public float PositionY;
        public string ModuleId;
        public int RotationQuarterTurns;

        public RedeployModuleCommand(
            string zoneId, float positionX, float positionY, string moduleId, int rotationQuarterTurns = 0)
        {
            ZoneId = zoneId;
            PositionX = positionX;
            PositionY = positionY;
            ModuleId = moduleId;
            RotationQuarterTurns = rotationQuarterTurns;
        }

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
                || !location.IsShelter)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ đặt lại được khi đang ở Shelter.");

            var reason = BuildSystem.CanRedeployAt(
                context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId,
                RotationQuarterTurns);
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
                    CommandErrorCode.ItemNotFound, "Không có Module đã gói sẵn trong túi."),
                BuildRejectReason.RotationNotAllowed => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, $"'{ModuleId}' không hỗ trợ xoay."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            string placementId = BuildSystem.RedeployModule(
                context.World, context.Definitions, ZoneId, PositionX, PositionY, ModuleId,
                RotationQuarterTurns);
            context.Events?.Publish(new InventoryChanged(InventoryOwner.Player.ToString()));
            context.Events?.Publish(new ModuleRedeployed(placementId, ModuleId));
        }
    }
}
