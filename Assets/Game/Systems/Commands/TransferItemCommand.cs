using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Systems.Inventory;

namespace LastHope.Systems.Commands
{
    /// <summary>
    /// Chuyển đồ giữa hai owner bất kỳ (player, shelter storage, search point đã mở, đồ
    /// dưới đất). Một command chung phục vụ Take/Take lẻ, Store, Withdraw, Drop, Pick up —
    /// khác nhau chỉ ở owner truyền vào, không cần command riêng cho từng UI action.
    /// </summary>
    public class TransferItemCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public InventoryOwner From;
        public InventoryOwner To;
        public string ItemId;
        public int Quantity;

        public TransferItemCommand(InventoryOwner from, InventoryOwner to, string itemId, int quantity)
        {
            From = from;
            To = to;
            ItemId = itemId;
            Quantity = quantity;
        }

        public CommandResult Validate(GameContext context)
        {
            if (Quantity <= 0)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, "Số lượng phải > 0.");

            if (!context.Definitions.TryGetItem(ItemId, out _))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, ItemId);

            int available = InventoryOwnerOps.CountOf(context.World, context.Definitions, From, ItemId);
            if (available < Quantity)
                return CommandResult.Fail(CommandErrorCode.ItemNotFound,
                    $"{From} chỉ có {available}×{ItemId}, cần {Quantity}.");

            if (!InventoryOwnerOps.CanAdd(context.World, context.Definitions, To, ItemId, Quantity))
                return CommandResult.Fail(CommandErrorCode.NotEnoughCapacity, $"{To} không nhận thêm {ItemId}.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            InventoryOwnerOps.Move(context.World, context.Definitions, From, To, ItemId, Quantity);

            context.Events?.Publish(new InventoryChanged(From.ToString()));
            context.Events?.Publish(new InventoryChanged(To.ToString()));
        }
    }
}
