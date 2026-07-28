using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;

namespace LastHope.Systems.Commands
{
    public class StartConstructionCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string SlotId;
        public string ModuleId;

        public StartConstructionCommand(string slotId, string moduleId)
        {
            SlotId = slotId;
            ModuleId = moduleId;
        }

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
                || !location.IsShelter)
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ xây được khi đang ở Shelter.");

            var reason = BuildSystem.CanStartConstruction(context.World, context.Definitions, SlotId, ModuleId);
            return reason switch
            {
                BuildRejectReason.None => CommandResult.Ok(),
                BuildRejectReason.UnknownSlot => CommandResult.Fail(CommandErrorCode.UnknownDefinition, SlotId),
                BuildRejectReason.UnknownModule => CommandResult.Fail(CommandErrorCode.UnknownDefinition, ModuleId),
                BuildRejectReason.WrongZone => CommandResult.Fail(
                    CommandErrorCode.InvalidTarget, $"'{ModuleId}' không xây được ở slot này."),
                BuildRejectReason.SlotOccupied => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Slot đã có Module."),
                BuildRejectReason.ConstructionInProgress => CommandResult.Fail(
                    CommandErrorCode.NotAllowedNow, "Đang xây công trình khác — chỉ một construction cùng lúc."),
                BuildRejectReason.NotEnoughMaterials => CommandResult.Fail(
                    CommandErrorCode.ItemNotFound, "Không đủ vật liệu trong kho Shelter."),
                _ => CommandResult.Fail(CommandErrorCode.NotAllowedNow),
            };
        }

        public void Execute(GameContext context)
        {
            BuildSystem.StartConstruction(context.World, context.Definitions, SlotId, ModuleId);
            context.Events?.Publish(
                new InventoryChanged(InventoryOwner.ShelterStorage(ShelterModuleIds.LocationId).ToString()));
        }
    }
}
