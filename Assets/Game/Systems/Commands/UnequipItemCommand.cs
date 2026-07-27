using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Data.Definitions;
using LastHope.Systems.Equipment;

namespace LastHope.Systems.Commands
{
    public class UnequipItemCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public EquipSlot Slot;

        public UnequipItemCommand(EquipSlot slot) => Slot = slot;

        public CommandResult Validate(GameContext context)
        {
            if (!context.World.Player.Equipped.ContainsKey(Slot))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Slot {Slot} đang trống.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            if (EquipmentSystem.TryUnequip(context.World.Player, context.Definitions, Slot))
            {
                context.Events?.Publish(new InventoryChanged("player"));
            }
            // TryUnequip false (backpack không đủ chỗ) — không publish, coi như no-op an toàn.
        }
    }
}
