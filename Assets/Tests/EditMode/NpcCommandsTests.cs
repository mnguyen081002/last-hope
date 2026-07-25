using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class NpcCommandsTests
    {
        private const string Shelter = "location_shelter";

        private static Dictionary<string, NpcDefinition> Npcs() => new Dictionary<string, NpcDefinition>
        {
            ["npc_minh"] = new NpcDefinition { Id = "npc_minh", DisplayName = "Nguyễn Minh", StartingTrust = 30 },
        };

        private static GameContext BuildContext(int livingCapacity = 2, int occupants = 1)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = Shelter;
            world.ShelterStates["shelter_main"] = new ShelterState
            {
                Id = "shelter_main",
                LivingCapacity = livingCapacity,
                Occupants = occupants,
            };
            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                [Shelter] = new LocationDefinition { Id = Shelter, IsShelter = true },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                npcs: Npcs());
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        [Test]
        public void Recruit_Success_CreatesNpcState_IncrementsOccupants_PublishesNpcRecruited()
        {
            var ctx = BuildContext();
            string recruitedId = null;
            ctx.Events.Subscribe<NpcRecruited>(e => recruitedId = e.NpcId);

            var result = new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("npc_minh", recruitedId);
            var npc = ctx.World.NpcStates["npc_minh"];
            Assert.IsTrue(npc.Recruited);
            Assert.AreEqual(30, npc.Trust);
            Assert.AreEqual(NpcHealthState.Healthy, npc.Health);
            Assert.AreEqual(Shelter, npc.LocationId);
            Assert.AreEqual(2, ctx.World.ShelterStates["shelter_main"].Occupants);
        }

        [Test]
        public void Recruit_UnknownDefinition_FailsNpcUnavailable()
        {
            var ctx = BuildContext();
            var result = new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_ghost"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NpcUnavailable, result.Code);
        }

        [Test]
        public void Recruit_AlreadyRecruited_FailsNpcUnavailable()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);
            processor.Submit(new RecruitNpcCommand("player", "npc_minh"));

            var result = processor.Submit(new RecruitNpcCommand("player", "npc_minh"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NpcUnavailable, result.Code);
        }

        [Test]
        public void Recruit_NotAtShelter_FailsNotAtLocation()
        {
            var ctx = BuildContext();
            ctx.World.Player.CurrentLocationId = "location_elsewhere";
            var result = new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NotAtLocation, result.Code);
        }

        [Test]
        public void Recruit_OverCapacity_FailsCapacityFull()
        {
            var ctx = BuildContext(livingCapacity: 1, occupants: 1); // already full (just the player)
            var result = new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.CapacityFull, result.Code);
        }

        [Test]
        public void AssignNpcTask_Success_SetsRequiredWorker()
        {
            var ctx = BuildContext();
            new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));
            ctx.World.ActiveTasks.Add(new ActiveTaskState { TaskId = "task_1", Kind = TaskKind.Active, Status = TaskStatus.Running });

            var result = new CommandProcessor(ctx).Submit(new AssignNpcTaskCommand("player", "task_1", "npc_minh"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("npc_minh", ctx.World.ActiveTasks[0].RequiredWorker);
        }

        [Test]
        public void AssignNpcTask_UnrecruitedNpc_FailsNpcNotRecruited()
        {
            var ctx = BuildContext();
            ctx.World.ActiveTasks.Add(new ActiveTaskState { TaskId = "task_1", Kind = TaskKind.Active, Status = TaskStatus.Running });

            var result = new CommandProcessor(ctx).Submit(new AssignNpcTaskCommand("player", "task_1", "npc_minh"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NpcNotRecruited, result.Code);
        }

        [Test]
        public void AssignNpcTask_PassiveTask_FailsInvalidState()
        {
            var ctx = BuildContext();
            new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));
            ctx.World.ActiveTasks.Add(new ActiveTaskState { TaskId = "task_1", Kind = TaskKind.Passive, Status = TaskStatus.Running });

            var result = new CommandProcessor(ctx).Submit(new AssignNpcTaskCommand("player", "task_1", "npc_minh"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.InvalidState, result.Code);
        }

        [Test]
        public void AssignNpcTask_UnknownTask_FailsTaskNotFound()
        {
            var ctx = BuildContext();
            new CommandProcessor(ctx).Submit(new RecruitNpcCommand("player", "npc_minh"));

            var result = new CommandProcessor(ctx).Submit(new AssignNpcTaskCommand("player", "task_missing", "npc_minh"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.TaskNotFound, result.Code);
        }
    }
}
