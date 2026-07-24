using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    public enum RestMode { Rest, TreatExposure, DryOff }

    /// <summary>
    /// Shelter-only recovery actions (BL-P1 S9). Rest just passes time; TreatExposure consumes one
    /// medical item (applying its normal UseEffects too) and grants bonus "black_water" exposure
    /// decay for the session; DryOff is instant — no clock cost — modeling "take off wet clothes
    /// indoors", which passive ConditionSystem shelter-drying would otherwise take many minutes to
    /// achieve on its own.
    /// </summary>
    public sealed class RestAtShelterCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId => Mode.ToString();
        public long WorldTime { get; set; }
        public RestMode Mode { get; }

        public RestAtShelterCommand(string actorId, RestMode mode)
        {
            ActorId = actorId;
            Mode = mode;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ActorId != ctx.World.Player.ActorId)
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown actor '{ActorId}'.");

            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");

            if (Mode == RestMode.TreatExposure && !TryFindMedicalItem(ctx, out _))
                return CommandResult.Fail(CommandErrorCode.NoMedicalItem, "No medical item available to treat exposure.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            switch (Mode)
            {
                case RestMode.Rest: ExecuteRest(ctx); break;
                case RestMode.TreatExposure: ExecuteTreatExposure(ctx); break;
                case RestMode.DryOff: ExecuteDryOff(ctx); break;
            }
        }

        private void ExecuteRest(GameContext ctx)
        {
            ctx.Clock.FastForward(ctx.Definitions.Balance.Condition.ShelterRestMinutes);
        }

        private void ExecuteTreatExposure(GameContext ctx)
        {
            TryFindMedicalItem(ctx, out string instanceId);
            InventoryState inv = ctx.World.Player.Inventory;
            ItemInstanceState item = inv.Items[instanceId];
            ctx.Definitions.TryGetItem(item.ItemId, out var def);

            PlayerConditionState condition = ctx.World.Player.Condition;
            ConditionOps.ApplyItemUseEffects(condition, def.UseEffects);

            item.Quantity -= 1;
            if (item.Quantity <= 0) inv.Items.Remove(instanceId);
            InventoryOps.RecalculateLoad(inv, ctx.Definitions);
            ctx.Events.Publish(new InventoryChanged(ActorId));

            condition.TreatingExposure = true;
            ctx.Clock.FastForward(ctx.Definitions.Balance.Condition.ShelterTreatExposureMinutes);
            condition.TreatingExposure = false;

            ConditionOps.RecomputeIncapacitation(condition, ctx.Definitions.Balance.Condition);
            ctx.Events.Publish(new ConditionChanged(ActorId));
        }

        private void ExecuteDryOff(GameContext ctx)
        {
            PlayerConditionState condition = ctx.World.Player.Condition;
            ConditionOps.SetStatusSeverity(condition, ConditionOps.StatusWet, 0f, ctx.World.WorldTimeMinutes);
            ctx.Events.Publish(new ConditionChanged(ActorId));
        }

        private static bool TryFindMedicalItem(GameContext ctx, out string instanceId)
        {
            foreach (var kvp in ctx.World.Player.Inventory.Items)
            {
                if (ctx.Definitions.TryGetItem(kvp.Value.ItemId, out var def) && def.Tags.Contains("medical"))
                {
                    instanceId = kvp.Key;
                    return true;
                }
            }
            instanceId = null;
            return false;
        }
    }
}
