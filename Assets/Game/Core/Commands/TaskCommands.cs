using System;
using LastHope.Core.Events;
using LastHope.Core.Logging;
using LastHope.Core.Rules;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    // Validate + minimal state flag only; full Task System behavior (progress, pause/resume,
    // resource reservation) arrives with Shelter Task in Sprint 10+ (technical-specification.md §22).

    public sealed class StartTaskCommand : IGameCommand
    {
        public string ActorId { get; }
        public string TargetId { get; } // task id
        public long WorldTime { get; set; }

        public StartTaskCommand(string actorId, string taskId)
        {
            ActorId = actorId;
            TargetId = taskId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (string.IsNullOrEmpty(ActorId)) return CommandResult.Fail(CommandErrorCode.InvalidActor);
            if (string.IsNullOrEmpty(TargetId)) return CommandResult.Fail(CommandErrorCode.InvalidTarget);

            foreach (var task in ctx.World.ActiveTasks)
                if (task.Id == TargetId) return CommandResult.Fail(CommandErrorCode.AlreadyActive);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.World.ActiveTasks.Add(new ActiveTaskState { Id = TargetId, StatusName = "Active" });
            GameLog.Info(LogCategory.World, $"StartTaskCommand: '{TargetId}' marked Active (full Task System arrives later).");
        }
    }

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
                if (task.Id == TargetId) return CommandResult.Ok();
            return CommandResult.Fail(CommandErrorCode.NotActive);
        }

        public void Execute(GameContext ctx)
        {
            ctx.World.ActiveTasks.RemoveAll(t => t.Id == TargetId);
            GameLog.Info(LogCategory.World, $"CancelTaskCommand: '{TargetId}' removed.");
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

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Definitions.TryGetRoute(TargetId, out var route);
            string from = ctx.World.Player.CurrentLocationId;
            string to = from == route.FromLocationId ? route.ToLocationId : route.FromLocationId;

            float loadFactor = InventoryRules.LoadFactorFor(ctx.World.Player.Inventory.Overload, ctx.Definitions.Balance);
            int minutes = (int)Math.Ceiling(route.TravelMinutes * loadFactor);

            ctx.Events.Publish(new TravelStarted(TargetId, from, to, minutes));
            ctx.Clock.FastForward(minutes);
            ctx.World.Player.CurrentLocationId = to;
            ctx.Events.Publish(new TravelCompleted(TargetId, from, to, minutes));
        }
    }
}
