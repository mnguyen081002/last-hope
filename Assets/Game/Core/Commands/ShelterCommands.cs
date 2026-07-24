using System;
using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>Sets a built module's power priority (S12) — read by PowerSystem next long-tick.</summary>
    public sealed class SetPowerPriorityCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // module instance id
        public long WorldTime { get; set; }
        private readonly PowerPriority _priority;

        public SetPowerPriorityCommand(string actorId, string moduleInstanceId, PowerPriority priority)
        {
            ActorId = actorId;
            TargetId = moduleInstanceId;
            _priority = priority;
        }

        public CommandResult Validate(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter) || !shelter.Modules.ContainsKey(TargetId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"No module '{TargetId}'.");
            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];
            shelter.Power.Priorities[TargetId] = _priority;
            ctx.Events.Publish(new PowerStateChanged(shelterId));
        }
    }

    /// <summary>
    /// Runs one Purify batch on a built, powered Purifier module (S12) — same
    /// validate-then-FastForward shape as RestAtShelterCommand rather than a supervised Active
    /// task; docs only require the batch to finish before Peak, not that the player stand there.
    /// ModuleState.Durability is reused as the module's filter-life meter here (a filter lasts 3
    /// batches) — a different meaning than Barrier's structural durability, documented since
    /// there's no separate FilterState yet.
    /// </summary>
    public sealed class StartPurifyBatchCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // purifier module instance id
        public long WorldTime { get; set; }

        public StartPurifyBatchCommand(string actorId, string moduleInstanceId)
        {
            ActorId = actorId;
            TargetId = moduleInstanceId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter) || !shelter.Modules.TryGetValue(TargetId, out var module))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"No module '{TargetId}'.");

            if (!ctx.Definitions.TryGetModule(module.ModuleId, out var def) || !def.Tags.Contains("purifier"))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Module '{TargetId}' is not a Purifier.");

            if (!module.Active)
                return CommandResult.Fail(CommandErrorCode.NoPower);
            if (module.Durability <= 0f)
                return CommandResult.Fail(CommandErrorCode.NoFilter);

            var cfg = ctx.Definitions.Balance.Water;
            if (shelter.WaterStocks.Untreated < cfg.PurifyBatchSize)
                return CommandResult.Fail(CommandErrorCode.NothingToPurify);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];
            var module = shelter.Modules[TargetId];
            var cfg = ctx.Definitions.Balance.Water;

            ctx.Clock.FastForward(cfg.PurifyBatchMinutes);

            shelter.WaterStocks.Untreated -= cfg.PurifyBatchSize;
            shelter.WaterStocks.Clean += cfg.PurifyBatchSize;
            module.Durability = Math.Max(0f, module.Durability - cfg.FilterWearPerBatch);

            ctx.Events.Publish(new WaterStocksChanged(shelterId));
        }
    }

    /// <summary>Converts abstract shelter Clean Water stock into carryable item_water_bottle
    /// instances (S12) — bridges the shelter-level resource number to the existing P1 item/
    /// inventory system rather than inventing a parallel "drink shelter water" mechanic.</summary>
    public sealed class CollectWaterCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId => "clean_water";
        public long WorldTime { get; set; }
        private readonly int _units;

        public CollectWaterCommand(string actorId, int units)
        {
            ActorId = actorId;
            _units = units;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");
            if (_units <= 0)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Units must be positive.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter) || shelter.WaterStocks.Clean < _units)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Not enough clean water.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];
            shelter.WaterStocks.Clean -= _units;

            InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_water_bottle", _units, () => Guid.NewGuid().ToString("N"));
            InventoryOps.RecalculateLoad(ctx.World.Player.Inventory, ctx.Definitions);

            ctx.Events.Publish(new InventoryChanged(ActorId));
            ctx.Events.Publish(new WaterStocksChanged(shelterId));
        }
    }
}
