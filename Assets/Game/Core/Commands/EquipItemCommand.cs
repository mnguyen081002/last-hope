using LastHope.Core.Events;

namespace LastHope.Core.Commands
{
    /// <summary>Equips an item into one of its ItemDefinition.EquipSlot slots (body/feet/hands/
    /// back/tool, S8). The item stays in Items — equipping is just a slot reference, it does not
    /// move or remove the item from the owner's inventory.</summary>
    public sealed class EquipItemCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // item instance id
        public long WorldTime { get; set; }
        public string Slot { get; }

        public EquipItemCommand(string actorId, string itemInstanceId, string slot)
        {
            ActorId = actorId;
            TargetId = itemInstanceId;
            Slot = slot;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory))
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown owner '{ActorId}'.");

            if (!inventory.Items.TryGetValue(TargetId, out var item))
                return CommandResult.Fail(CommandErrorCode.ItemNotFound, $"Item instance '{TargetId}' not found.");

            if (!ctx.Definitions.TryGetItem(item.ItemId, out var def) || string.IsNullOrEmpty(def.EquipSlot))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"'{item.ItemId}' is not equippable.");

            if (def.EquipSlot != Slot)
                return CommandResult.Fail(CommandErrorCode.SlotMismatch, $"'{item.ItemId}' equips to slot '{def.EquipSlot}', not '{Slot}'.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory);
            inventory.EquipmentSlots[Slot] = TargetId;
            ctx.Events.Publish(new EquipmentChanged(ActorId, Slot, TargetId));
        }
    }

    /// <summary>Clears one equipment slot. TargetId is the slot name, not an item instance id.</summary>
    public sealed class UnequipItemCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // slot name
        public long WorldTime { get; set; }

        public UnequipItemCommand(string actorId, string slot)
        {
            ActorId = actorId;
            TargetId = slot;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory))
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown owner '{ActorId}'.");

            if (!inventory.EquipmentSlots.ContainsKey(TargetId))
                return CommandResult.Fail(CommandErrorCode.SlotMismatch, $"Slot '{TargetId}' is already empty.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory);
            inventory.EquipmentSlots.Remove(TargetId);
            ctx.Events.Publish(new EquipmentChanged(ActorId, TargetId, null));
        }
    }
}
