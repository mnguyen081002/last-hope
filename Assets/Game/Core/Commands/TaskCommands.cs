using System;
using LastHope.Core.Events;
using LastHope.Core.Logging;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Core.Commands
{
    /// <summary>Generic task creation (S11) — no material reservation (that's StartBuildCommand's
    /// job); for task kinds that don't need it, e.g. a future Repair task. TargetId here is
    /// whatever the task acts on (a build slot, a module instance...), NOT the task's own id — the
    /// task's own id is generated fresh so multiple tasks can target different things concurrently.</summary>
    public sealed class StartTaskCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // what the task acts on
        public long WorldTime { get; set; }
        private readonly TaskKind _kind;

        public StartTaskCommand(string actorId, string targetId, TaskKind kind)
        {
            ActorId = actorId;
            TargetId = targetId;
            _kind = kind;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (string.IsNullOrEmpty(ActorId)) return CommandResult.Fail(CommandErrorCode.InvalidActor);
            if (string.IsNullOrEmpty(TargetId)) return CommandResult.Fail(CommandErrorCode.InvalidTarget);

            foreach (var task in ctx.World.ActiveTasks)
                if (task.TargetId == TargetId) return CommandResult.Fail(CommandErrorCode.AlreadyActive);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            string taskId = Guid.NewGuid().ToString("N");
            ctx.World.ActiveTasks.Add(new ActiveTaskState
            {
                TaskId = taskId,
                Kind = _kind,
                TargetId = TargetId,
                Progress = 0f,
                Status = TaskStatus.Running,
                RequiredWorker = _kind == TaskKind.Active ? ActorId : null,
            });
            ctx.Events.Publish(new TaskStateChanged(taskId));
        }
    }

    /// <summary>Cancels a task by its own TaskId — refunds any reserved materials (owner
    /// "task:&lt;TaskId&gt;") to shelter storage, then removes the task (S11).</summary>
    public sealed class CancelTaskCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // task id
        public long WorldTime { get; set; }

        public CancelTaskCommand(string actorId, string taskId)
        {
            ActorId = actorId;
            TargetId = taskId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            foreach (var task in ctx.World.ActiveTasks)
                if (task.TaskId == TargetId) return CommandResult.Ok();
            return CommandResult.Fail(CommandErrorCode.TaskNotFound);
        }

        public void Execute(GameContext ctx)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            InventoryOwnerResolver.TryResolve(ctx, "task:" + TargetId, out var taskInv);
            InventoryOwnerResolver.TryResolve(ctx, "shelter_storage:" + shelterId, out var storage);

            if (taskInv.Items.Count > 0)
            {
                foreach (var kvp in taskInv.Items) kvp.Value.ContainerId = storage.OwnerId;
                foreach (var instanceId in taskInv.Items.Keys) storage.Items[instanceId] = taskInv.Items[instanceId];
                taskInv.Items.Clear();
                InventoryOps.RecalculateLoad(storage, ctx.Definitions);
                InventoryOps.RecalculateLoad(taskInv, ctx.Definitions);
                ctx.Events.Publish(new InventoryChanged(storage.OwnerId));
            }

            ctx.World.ActiveTasks.RemoveAll(t => t.TaskId == TargetId);
            ctx.Events.Publish(new TaskStateChanged(TargetId));
        }
    }

    public sealed class PauseTaskCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // task id
        public long WorldTime { get; set; }

        public PauseTaskCommand(string actorId, string taskId) { ActorId = actorId; TargetId = taskId; }

        public CommandResult Validate(GameContext ctx)
        {
            var task = ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId);
            if (task == null) return CommandResult.Fail(CommandErrorCode.TaskNotFound);
            if (task.Status != TaskStatus.Running) return CommandResult.Fail(CommandErrorCode.TaskNotRunning);
            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId).Status = TaskStatus.Paused;
            ctx.Events.Publish(new TaskStateChanged(TargetId));
        }
    }

    public sealed class ResumeTaskCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // task id
        public long WorldTime { get; set; }

        public ResumeTaskCommand(string actorId, string taskId) { ActorId = actorId; TargetId = taskId; }

        public CommandResult Validate(GameContext ctx)
        {
            var task = ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId);
            if (task == null) return CommandResult.Fail(CommandErrorCode.TaskNotFound);
            if (task.Status != TaskStatus.Paused) return CommandResult.Fail(CommandErrorCode.TaskNotRunning);
            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId).Status = TaskStatus.Running;
            ctx.Events.Publish(new TaskStateChanged(TargetId));
        }
    }

    /// <summary>
    /// Full body since Sprint 6 (BL-P1-19): validates the player is at one end of the route,
    /// fast-forwards the clock by TravelMinutes scaled by carry-load factor, then switches
    /// CurrentLocationId to the other end. Scene loading reacts to TravelCompleted
    /// (SceneFlowController) — this command only touches simulation state.
    /// </summary>
    public sealed class BeginTravelCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // route id
        public long WorldTime { get; set; }

        public BeginTravelCommand(string actorId, string routeId)
        {
            ActorId = actorId;
            TargetId = routeId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!ctx.Definitions.TryGetRoute(TargetId, out var route))
                return CommandResult.Fail(CommandErrorCode.RouteBlocked, $"Unknown route '{TargetId}'.");

            string current = ctx.World.Player.CurrentLocationId;
            if (current != route.FromLocationId && current != route.ToLocationId)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, $"Player is not on route '{TargetId}'.");

            if (ActorId == ctx.World.Player.ActorId && ConditionOps.IsIncapacitated(ctx.World.Player.Condition))
                return CommandResult.Fail(CommandErrorCode.Incapacitated, "Player is incapacitated and cannot travel.");

            var crossing = EvaluateCrossing(ctx, route);
            if (!crossing.Passable)
                return CommandResult.Fail(CommandErrorCode.RouteBlocked, string.Join(" ", crossing.Warnings));

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Definitions.TryGetRoute(TargetId, out var route);
            string from = ctx.World.Player.CurrentLocationId;
            string to = from == route.FromLocationId ? route.ToLocationId : route.FromLocationId;

            var crossing = EvaluateCrossing(ctx, route);
            float loadFactor = InventoryRules.LoadFactorFor(ctx.World.Player.Inventory.Overload, ctx.Definitions.Balance);
            int minutes = (int)Math.Ceiling(route.TravelMinutes * loadFactor * crossing.TimeFactor);

            ctx.Events.Publish(new TravelStarted(TargetId, from, to, minutes));
            ctx.Clock.FastForward(minutes);
            ctx.World.Player.CurrentLocationId = to;

            ApplyCrossingCost(ctx, crossing);

            ctx.Events.Publish(new TravelCompleted(TargetId, from, to, minutes));
        }

        private static CrossingEvaluation EvaluateCrossing(GameContext ctx, RouteDefinition route)
        {
            var hazard = HazardRules.EvaluateRoute(route, ctx.Definitions.DisasterPhasesSorted, ctx.World.WorldTimeMinutes);
            var equipment = EquipmentRules.ResolveTravelProtection(ctx.World.Player.Inventory, ctx.Definitions);
            return TravelRules.EvaluateCrossing(hazard, ctx.World.Player.Condition, ctx.Definitions.Balance.Hazard, equipment);
        }

        private static void ApplyCrossingCost(GameContext ctx, CrossingEvaluation crossing)
        {
            PlayerConditionState condition = ctx.World.Player.Condition;

            float shortfall = Math.Max(0f, crossing.StaminaCost - condition.Stamina);
            ConditionOps.ApplyStamina(condition, -crossing.StaminaCost);
            if (shortfall > 0f) ConditionOps.ApplyFatigue(condition, shortfall);

            ConditionOps.AddExposure(condition, "black_water", crossing.ExposureGain);
            ConditionOps.ApplyExposureStatusChain(condition, "black_water", ctx.World.WorldTimeMinutes, ctx.Definitions.Balance.Condition);

            float wet = ConditionOps.GetStatusSeverity(condition, ConditionOps.StatusWet);
            ConditionOps.SetStatusSeverity(condition, ConditionOps.StatusWet, wet + crossing.WetGain, ctx.World.WorldTimeMinutes);

            ConditionOps.RecomputeIncapacitation(condition, ctx.Definitions.Balance.Condition);
            ctx.Events.Publish(new ConditionChanged(ctx.World.Player.ActorId));
        }
    }
}
