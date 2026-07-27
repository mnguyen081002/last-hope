using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Systems.Equipment;

namespace LastHope.Systems.Commands
{
    public class EquipItemCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ItemId;

        public EquipItemCommand(string itemId) => ItemId = itemId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetItem(ItemId, out var item))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, ItemId);

            if (!item.IsEquipment)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"{ItemId} không phải equipment.");

            if (InventoryOps.CountOf(context.World.Player.Inventory, ItemId) <= 0)
                return CommandResult.Fail(CommandErrorCode.ItemNotFound, ItemId);

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            if (EquipmentSystem.TryEquip(context.World.Player, context.Definitions, ItemId))
            {
                context.Events?.Publish(new InventoryChanged("player"));
            }
        }
    }
}
