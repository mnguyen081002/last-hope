using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    public sealed class UseItemCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // item instance id
        public long WorldTime { get; set; }
        public int Quantity { get; }

        public UseItemCommand(string actorId, string itemInstanceId, int quantity = 1)
        {
            ActorId = actorId;
            TargetId = itemInstanceId;
            Quantity = quantity;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory))
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown owner '{ActorId}'.");

            if (!inventory.Items.TryGetValue(TargetId, out var item))
                return CommandResult.Fail(CommandErrorCode.ItemNotFound, $"Item instance '{TargetId}' not found.");

            if (Quantity <= 0 || Quantity > item.Quantity)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Invalid use quantity.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory);
            ItemInstanceState item = inventory.Items[TargetId];

            item.Quantity -= Quantity;
            if (item.Quantity <= 0) inventory.Items.Remove(TargetId);

            InventoryOps.RecalculateLoad(inventory, ctx.Definitions);
            ctx.Events.Publish(new InventoryChanged(ActorId));
        }
    }
}
