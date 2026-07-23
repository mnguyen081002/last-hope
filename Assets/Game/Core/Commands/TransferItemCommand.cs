using System;
using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Moves an item instance between two known inventory owners. Moves the whole instance
    /// (preserving Condition/Contamination/Wet) when the full quantity is transferred; splits
    /// into a fresh instance only for partial-quantity transfers.
    /// </summary>
    public sealed class TransferItemCommand : IGameCommand
    {
        public string ActorId { get; } // source owner id
        public string TargetId { get; } // item instance id
        public long WorldTime { get; set; }
        public string DestinationOwnerId { get; }
        public int Quantity { get; }

        public TransferItemCommand(string sourceOwnerId, string itemInstanceId, string destinationOwnerId, int quantity)
        {
            ActorId = sourceOwnerId;
            TargetId = itemInstanceId;
            DestinationOwnerId = destinationOwnerId;
            Quantity = quantity;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!InventoryOwnerResolver.TryResolve(ctx, ActorId, out var source))
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown owner '{ActorId}'.");

            if (!InventoryOwnerResolver.TryResolve(ctx, DestinationOwnerId, out _))
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown owner '{DestinationOwnerId}'.");

            if (!source.Items.TryGetValue(TargetId, out var item))
                return CommandResult.Fail(CommandErrorCode.ItemNotFound, $"Item instance '{TargetId}' not found in '{ActorId}'.");

            if (Quantity <= 0 || Quantity > item.Quantity)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Invalid transfer quantity.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var source);
            InventoryOwnerResolver.TryResolve(ctx, DestinationOwnerId, out var destination);
            ItemInstanceState item = source.Items[TargetId];

            if (Quantity >= item.Quantity)
            {
                source.Items.Remove(TargetId);
                item.ContainerId = DestinationOwnerId;
                destination.Items[item.InstanceId] = item;
            }
            else
            {
                item.Quantity -= Quantity;
                var moved = new ItemInstanceState
                {
                    InstanceId = Guid.NewGuid().ToString("N"),
                    ItemId = item.ItemId,
                    Quantity = Quantity,
                    Condition = item.Condition,
                    Durability = item.Durability,
                    Contamination = item.Contamination,
                    Wet = item.Wet,
                    ContainerId = DestinationOwnerId,
                };
                destination.Items[moved.InstanceId] = moved;
            }

            InventoryOps.RecalculateLoad(source, ctx.Definitions);
            InventoryOps.RecalculateLoad(destination, ctx.Definitions);
            ctx.Events.Publish(new InventoryChanged(ActorId));
            ctx.Events.Publish(new InventoryChanged(DestinationOwnerId));
        }
    }
}
