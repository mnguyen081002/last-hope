using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

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

            if (ActorId == ctx.World.Player.ActorId && ConditionOps.IsIncapacitated(ctx.World.Player.Condition))
            {
                bool isMedical = ctx.Definitions.TryGetItem(item.ItemId, out var def) && def.Tags.Contains("medical");
                if (!isMedical)
                    return CommandResult.Fail(CommandErrorCode.Incapacitated, "Player is incapacitated; only medical items can be used.");
            }

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var inventory);
            ItemInstanceState item = inventory.Items[TargetId];

            if (ActorId == ctx.World.Player.ActorId && ctx.Definitions.TryGetItem(item.ItemId, out var def))
                ApplyUseEffects(ctx, def);

            item.Quantity -= Quantity;
            if (item.Quantity <= 0) inventory.Items.Remove(TargetId);

            InventoryOps.RecalculateLoad(inventory, ctx.Definitions);
            ctx.Events.Publish(new InventoryChanged(ActorId));
        }

        private void ApplyUseEffects(GameContext ctx, ItemDefinition def)
        {
            if (def.UseEffects == null || def.UseEffects.Count == 0) return;

            PlayerConditionState condition = ctx.World.Player.Condition;
            bool changed = false;
            foreach (var effect in def.UseEffects)
            {
                switch (effect.Key)
                {
                    case "thirst": ConditionOps.ApplyThirst(condition, effect.Value); changed = true; break;
                    case "hunger": ConditionOps.ApplyHunger(condition, effect.Value); changed = true; break;
                    case "health": ConditionOps.ApplyHealth(condition, effect.Value); changed = true; break;
                    case "stamina": ConditionOps.ApplyStamina(condition, effect.Value); changed = true; break;
                    case "fatigue": ConditionOps.ApplyFatigue(condition, effect.Value); changed = true; break;
                }
            }

            if (!changed) return;
            ConditionOps.RecomputeIncapacitation(condition, ctx.Definitions.Balance.Condition);
            ctx.Events.Publish(new ConditionChanged(ActorId));
        }
    }
}
