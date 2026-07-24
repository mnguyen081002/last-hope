using System;
using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Starts building a Module into a Build Slot (S11). Reserves materials into a new task-scoped
    /// inventory ("task:&lt;TaskId&gt;") and creates a Passive ActiveTaskState — TaskSystem advances
    /// it every long-tick (even while the player is elsewhere/asleep) and spawns the ModuleState on
    /// completion.
    /// </summary>
    public sealed class StartBuildCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // build slot id
        public long WorldTime { get; set; }
        private readonly string _moduleId;

        public StartBuildCommand(string actorId, string slotId, string moduleId)
        {
            ActorId = actorId;
            TargetId = slotId;
            _moduleId = moduleId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ActorId != ctx.World.Player.ActorId)
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown actor '{ActorId}'.");

            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");

            if (!ctx.Definitions.TryGetModule(_moduleId, out var module))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Unknown module '{_moduleId}'.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Shelter state not initialized.");

            foreach (var task in ctx.World.ActiveTasks)
                if (task.TargetId == TargetId)
                    return CommandResult.Fail(CommandErrorCode.SlotOccupied, $"Slot '{TargetId}' already has a task in progress.");

            PlacementIssue issue = BuildRules.ValidatePlacement(ctx.Definitions.ShelterZones, shelter, TargetId, module);
            switch (issue)
            {
                case PlacementIssue.SlotLocked: return CommandResult.Fail(CommandErrorCode.SlotLocked);
                case PlacementIssue.SlotOccupied: return CommandResult.Fail(CommandErrorCode.SlotOccupied);
                case PlacementIssue.InvalidSlot:
                case PlacementIssue.WrongZone:
                    return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Module '{_moduleId}' cannot be built into slot '{TargetId}'.");
            }

            if (!BuildRules.HasMaterials(ctx.World.Player.Inventory, module.Materials))
                return CommandResult.Fail(CommandErrorCode.MissingMaterials, $"Missing materials for '{_moduleId}'.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Definitions.TryGetModule(_moduleId, out var module);
            string taskId = Guid.NewGuid().ToString("N");

            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var playerInv);
            InventoryOwnerResolver.TryResolve(ctx, "task:" + taskId, out var taskInv);
            MoveMaterials(playerInv, taskInv, module.Materials, ctx.Definitions);

            ctx.World.ActiveTasks.Add(new ActiveTaskState
            {
                TaskId = taskId,
                Kind = TaskKind.Passive,
                TargetId = TargetId,
                ModuleId = _moduleId,
                Progress = 0f,
                Status = TaskStatus.Running,
            });

            ctx.Events.Publish(new InventoryChanged(ActorId));
            ctx.Events.Publish(new InventoryChanged("task:" + taskId));
            ctx.Events.Publish(new TaskStateChanged(taskId));
        }

        /// <summary>Moves total `quantity` of each material id from source to dest, spanning
        /// multiple stacks if needed. Unlike TransferItemCommand (moves one known instance), this
        /// aggregates by item id — materials arrive as a plain id->qty requirement, not a specific
        /// instance the player picked.</summary>
        internal static void MoveMaterials(InventoryState source, InventoryState dest, IReadOnlyDictionary<string, int> materials, DefinitionRegistry defs)
        {
            foreach (var required in materials)
            {
                int remaining = required.Value;
                var instanceIds = new List<string>();
                foreach (var kvp in source.Items)
                    if (kvp.Value.ItemId == required.Key) instanceIds.Add(kvp.Key);

                foreach (string instanceId in instanceIds)
                {
                    if (remaining <= 0) break;
                    var item = source.Items[instanceId];
                    int take = Math.Min(remaining, item.Quantity);

                    if (take >= item.Quantity)
                    {
                        source.Items.Remove(instanceId);
                        item.ContainerId = dest.OwnerId;
                        dest.Items[item.InstanceId] = item;
                    }
                    else
                    {
                        item.Quantity -= take;
                        var moved = new ItemInstanceState
                        {
                            InstanceId = Guid.NewGuid().ToString("N"),
                            ItemId = item.ItemId,
                            Quantity = take,
                            Condition = item.Condition,
                            Durability = item.Durability,
                            Contamination = item.Contamination,
                            Wet = item.Wet,
                            ContainerId = dest.OwnerId,
                        };
                        dest.Items[moved.InstanceId] = moved;
                    }
                    remaining -= take;
                }
            }

            InventoryOps.RecalculateLoad(source, defs);
            InventoryOps.RecalculateLoad(dest, defs);
        }
    }

    /// <summary>Removes a completed Module, refunding 50% of its materials to the player and
    /// freeing the slot (S11).</summary>
    public sealed class DismantleModuleCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // build slot id
        public long WorldTime { get; set; }

        public DismantleModuleCommand(string actorId, string slotId)
        {
            ActorId = actorId;
            TargetId = slotId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (ActorId != ctx.World.Player.ActorId)
                return CommandResult.Fail(CommandErrorCode.InvalidActor, $"Unknown actor '{ActorId}'.");

            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Not at a shelter.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)
                || !shelter.BuildSlots.TryGetValue(TargetId, out var slot)
                || string.IsNullOrEmpty(slot.ModuleInstanceId))
                return CommandResult.Fail(CommandErrorCode.InvalidTarget, $"Slot '{TargetId}' has no module to dismantle.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];
            var slot = shelter.BuildSlots[TargetId];
            var module = shelter.Modules[slot.ModuleInstanceId];
            ctx.Definitions.TryGetModule(module.ModuleId, out var moduleDef);

            shelter.Modules.Remove(slot.ModuleInstanceId);
            slot.ModuleInstanceId = null;

            var refund = BuildRules.DismantleRefund(moduleDef.Materials);
            InventoryOwnerResolver.TryResolve(ctx, ActorId, out var playerInv);
            foreach (var kvp in refund)
            {
                InventoryOps.AddItem(playerInv, ctx.Definitions, kvp.Key, kvp.Value, () => Guid.NewGuid().ToString("N"));
            }

            ctx.Events.Publish(new InventoryChanged(ActorId));
            ctx.Events.Publish(new ModuleCompleted(TargetId, null)); // reused: null instance id = slot freed
        }
    }
}
