using System;
using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Tasks
{
    /// <summary>
    /// Advances ActiveTaskState.Progress every long-tick (S11). Passive tasks (Build) always
    /// advance — including during FastForward while asleep/traveling, since LongTick fires
    /// uniformly regardless of what triggered clock advancement. Active tasks additionally require
    /// RequiredWorker to be at the shelter (no Active task type exists yet in S11; the branch is
    /// here so S12's first Active task type — Purify batch — doesn't need this rewritten).
    /// </summary>
    public sealed class TaskSystem
    {
        private readonly GameContext _ctx;

        public TaskSystem(GameContext ctx)
        {
            _ctx = ctx;
            ctx.Clock.SubscribeLong(OnLongTick);
        }

        private void OnLongTick(long minute)
        {
            // Snapshot: completing a task mutates ActiveTasks mid-iteration.
            var tasks = new List<ActiveTaskState>(_ctx.World.ActiveTasks);
            foreach (var task in tasks)
                AdvanceTask(task);
        }

        private void AdvanceTask(ActiveTaskState task)
        {
            if (task.Status != TaskStatus.Running) return;
            if (task.Kind == TaskKind.Active && !IsWorkerAtShelter(task.RequiredWorker)) return;
            if (string.IsNullOrEmpty(task.ModuleId)) return; // no progress formula for non-Build tasks yet

            if (!_ctx.Definitions.TryGetModule(task.ModuleId, out var moduleDef) || moduleDef.BuildMinutes <= 0) return;

            task.Progress = Math.Min(100f, task.Progress + 100f * 10f / moduleDef.BuildMinutes);
            _ctx.Events.Publish(new BuildProgressChanged(task.TargetId, task.Progress));

            if (task.Progress >= 100f) CompleteBuild(task, moduleDef);
        }

        /// <summary>S16: an Active task's worker can be the player or a recruited NPC
        /// (AssignNpcTaskCommand) — both need to be physically at a shelter to advance the task.</summary>
        private bool IsWorkerAtShelter(string workerId)
        {
            if (string.IsNullOrEmpty(workerId)) return false;

            if (workerId == _ctx.World.Player.ActorId)
                return _ctx.Definitions.TryGetLocation(_ctx.World.Player.CurrentLocationId, out var loc) && loc.IsShelter;

            if (_ctx.World.NpcStates.TryGetValue(workerId, out var npc) && npc.Recruited)
                return _ctx.Definitions.TryGetLocation(npc.LocationId, out var npcLoc) && npcLoc.IsShelter;

            return false;
        }

        private void CompleteBuild(ActiveTaskState task, ModuleDefinition moduleDef)
        {
            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = _ctx.World.ShelterStates[shelterId];

            InventoryOwnerResolver.TryResolve(_ctx, "task:" + task.TaskId, out var taskInv);
            taskInv.Items.Clear(); // materials consumed

            string moduleInstanceId = Guid.NewGuid().ToString("N");
            shelter.Modules[moduleInstanceId] = new ModuleState
            {
                InstanceId = moduleInstanceId,
                ModuleId = task.ModuleId,
                SlotId = task.TargetId,
                Durability = moduleDef.MaxDurability,
                Active = true,
            };
            shelter.BuildSlots[task.TargetId].ModuleInstanceId = moduleInstanceId;

            // Power-consuming modules default to Normal priority; PowerSystem's next long-tick
            // decides whether they actually get power (may flip Active back to false).
            if (moduleDef.PowerDemand > 0f)
                shelter.Power.Priorities[moduleInstanceId] = PowerPriority.Normal;

            _ctx.World.ActiveTasks.Remove(task);

            _ctx.Events.Publish(new ModuleCompleted(task.TargetId, moduleInstanceId));
            _ctx.Events.Publish(new TaskStateChanged(task.TaskId));
        }
    }
}
