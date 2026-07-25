using LastHope.Core.Events;
using LastHope.Core.State;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Brings an NPC into the shelter (S16). Mechanical only — no gate on narrative flags like
    /// "minh_met" (event_minh_intro's "greet" response); whether the player has actually met the
    /// NPC yet is a UI/content concern (S17: a recruit button only appears after the encounter),
    /// not something this command validates. Reusable across every NpcDefinition, not just
    /// Nguyễn Minh.
    /// </summary>
    public sealed class RecruitNpcCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // npc definition id
        public long WorldTime { get; set; }

        public RecruitNpcCommand(string actorId, string npcId)
        {
            ActorId = actorId;
            TargetId = npcId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            if (!ctx.Definitions.TryGetNpc(TargetId, out _))
                return CommandResult.Fail(CommandErrorCode.NpcUnavailable, $"No npc definition '{TargetId}'.");
            if (ctx.World.NpcStates.TryGetValue(TargetId, out var existing) && existing.Recruited)
                return CommandResult.Fail(CommandErrorCode.NpcUnavailable, "Already recruited.");
            if (!ctx.Definitions.TryGetLocation(ctx.World.Player.CurrentLocationId, out var loc) || !loc.IsShelter)
                return CommandResult.Fail(CommandErrorCode.NotAtLocation, "Must be at a shelter to recruit.");

            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Shelter not initialized.");
            if (shelter.Occupants + 1 > shelter.LivingCapacity)
                return CommandResult.Fail(CommandErrorCode.CapacityFull);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            ctx.Definitions.TryGetNpc(TargetId, out var def);
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = ctx.World.ShelterStates[shelterId];

            if (!ctx.World.NpcStates.TryGetValue(TargetId, out var npc))
            {
                npc = new NpcState { Id = TargetId };
                ctx.World.NpcStates[TargetId] = npc;
            }

            npc.Health = NpcHealthState.Healthy;
            npc.Hunger = 0f;
            npc.Thirst = 0f;
            npc.StarvingLongTicks = 0;
            npc.FloodExposureLongTicks = 0;
            npc.Trust = def.StartingTrust;
            npc.Recruited = true;
            npc.LocationId = ctx.World.Player.CurrentLocationId;

            shelter.Occupants++;

            DecisionLog.Append(ctx, "recruit", TargetId);
            ctx.Events.Publish(new NpcRecruited(TargetId));
            ctx.Events.Publish(new NpcStateChanged(TargetId));
        }
    }

    /// <summary>
    /// Assigns a recruited NPC as the RequiredWorker for an Active task (S16) — the first real
    /// consumer of TaskSystem's Active-task branch (declared since S11, unused until now: every
    /// task S11-S15 created is Passive). No content creates an Active task yet, so this command
    /// has no world-visible effect until one does; the mechanism itself is real and tested.
    /// </summary>
    public sealed class AssignNpcTaskCommand : IGameCommand
    {
        public string ActorId { get; } // player
        public string TargetId { get; } // task id
        public long WorldTime { get; set; }
        private readonly string _npcId;

        public AssignNpcTaskCommand(string actorId, string taskId, string npcId)
        {
            ActorId = actorId;
            TargetId = taskId;
            _npcId = npcId;
        }

        public CommandResult Validate(GameContext ctx)
        {
            var task = ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId);
            if (task == null)
                return CommandResult.Fail(CommandErrorCode.TaskNotFound);
            if (task.Kind != TaskKind.Active)
                return CommandResult.Fail(CommandErrorCode.InvalidState, "Only Active tasks take an assigned worker.");
            if (!ctx.World.NpcStates.TryGetValue(_npcId, out var npc) || !npc.Recruited)
                return CommandResult.Fail(CommandErrorCode.NpcNotRecruited);

            return CommandResult.Ok();
        }

        public void Execute(GameContext ctx)
        {
            var task = ctx.World.ActiveTasks.Find(t => t.TaskId == TargetId);
            task.RequiredWorker = _npcId;
            ctx.Events.Publish(new TaskStateChanged(task.TaskId));
        }
    }
}
