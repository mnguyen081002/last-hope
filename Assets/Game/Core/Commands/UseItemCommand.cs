using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>Dùng một món trong túi: áp use_effects rồi trừ số lượng.</summary>
    public class UseItemCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public string ItemId;

        public UseItemCommand(string itemId) => ItemId = itemId;

        public CommandResult Validate(GameContext context)
        {
            if (!context.Definitions.TryGetItem(ItemId, out var definition))
                return CommandResult.Fail(CommandErrorCode.UnknownDefinition, ItemId);

            if (definition.UseEffects == null)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"{ItemId} không dùng được");

            var inventory = context.World.Player.Inventory;
            if (InventoryOps.CountOf(inventory, ItemId) <= 0)
                return CommandResult.Fail(CommandErrorCode.ItemNotFound, ItemId);

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            var definition = context.Definitions.GetItem(ItemId);
            var player = context.World.Player;
            var effects = definition.UseEffects;

            // use_effects ghi theo hướng "cộng vào chỉ số": thirst -40 = bớt khát 40.
            player.Thirst = Clamp01To100(player.Thirst + effects.Thirst);
            player.Hunger = Clamp01To100(player.Hunger + effects.Hunger);

            InventoryOps.RemoveItem(player.Inventory, ItemId, 1);

            context.Events?.Publish(new InventoryChanged("player"));
        }

        static float Clamp01To100(float value) =>
            value < 0f ? 0f : value > 100f ? 100f : value;
    }
}
