using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using LastHope.Systems.Tasks;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class BuildCommandsTests
    {
        private static Dictionary<string, LocationDefinition> Locations() => new Dictionary<string, LocationDefinition>
        {
            ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
        };

        private static Dictionary<string, ShelterZoneDefinition> Zones() => new Dictionary<string, ShelterZoneDefinition>
        {
            ["utility_area"] = new ShelterZoneDefinition { Id = "utility_area", BuildSlotIds = new List<string> { "slot_utility_1" } },
        };

        private static Dictionary<string, ItemDefinition> Items() => new Dictionary<string, ItemDefinition>
        {
            ["item_pump_part"] = new ItemDefinition { Id = "item_pump_part", MaxStackSize = 5 },
            ["item_scrap"] = new ItemDefinition { Id = "item_scrap", MaxStackSize = 20 },
        };

        private static Dictionary<string, ModuleDefinition> Modules() => new Dictionary<string, ModuleDefinition>
        {
            ["module_pump"] = new ModuleDefinition
            {
                Id = "module_pump",
                AllowedZoneIds = new List<string> { "utility_area" },
                Materials = new Dictionary<string, int> { ["item_pump_part"] = 1, ["item_scrap"] = 2 },
                BuildMinutes = 20, // 2 long ticks
                MaxDurability = 100,
                Tags = new List<string> { "pump" },
            },
        };

        private static GameContext BuildContext()
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            var bus = new EventBus();
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), Items(),
                Locations(), new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                shelterZones: Zones(), modules: Modules());
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx); // seeds shelter_main + build slots
            _ = new TaskSystem(ctx);
            return ctx;
        }

        private static void GiveMaterials(GameContext ctx)
        {
            InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_pump_part", 1, () => System.Guid.NewGuid().ToString("N"));
            InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_scrap", 2, () => System.Guid.NewGuid().ToString("N"));
        }

        [Test]
        public void StartBuild_MissingMaterials_FailsMissingMaterials()
        {
            var ctx = BuildContext();
            var result = new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.MissingMaterials, result.Code);
        }

        [Test]
        public void StartBuild_HasMaterials_ReservesThem_CreatesPassiveTask()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);

            var result = new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, ctx.World.Player.Inventory.Items.Count); // materials moved out
            Assert.AreEqual(1, ctx.World.ActiveTasks.Count);
            var task = ctx.World.ActiveTasks[0];
            Assert.AreEqual(TaskKind.Passive, task.Kind);
            Assert.AreEqual("slot_utility_1", task.TargetId);
            Assert.AreEqual("module_pump", task.ModuleId);
        }

        [Test]
        public void StartBuild_SlotAlreadyHasTask_FailsSlotOccupied()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            GiveMaterials(ctx);

            var result = new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.SlotOccupied, result.Code);
        }

        [Test]
        public void PassiveTask_CompletesAfterEnoughLongTicks_SpawnsModule_EvenAwayFromShelter()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));

            ctx.World.Player.CurrentLocationId = "elsewhere"; // player away — Passive task must still run
            ctx.Clock.FastForward(20); // BuildMinutes=20 -> 2 long ticks -> 100%

            var shelter = ctx.World.ShelterStates["shelter_main"];
            Assert.AreEqual(0, ctx.World.ActiveTasks.Count); // task consumed
            Assert.AreEqual(1, shelter.Modules.Count);
            var module = new List<ModuleState>(shelter.Modules.Values)[0];
            Assert.AreEqual("module_pump", module.ModuleId);
            Assert.IsTrue(module.Active);
            Assert.AreEqual(module.InstanceId, shelter.BuildSlots["slot_utility_1"].ModuleInstanceId);
        }

        [Test]
        public void PassiveTask_PartialProgress_PublishesBuildProgressChanged()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));

            float lastProgress = -1f;
            ctx.Events.Subscribe<BuildProgressChanged>(e => lastProgress = e.Progress);
            ctx.Clock.FastForward(10); // 1 of 2 long ticks -> 50%

            Assert.AreEqual(50f, lastProgress, 0.01f);
        }

        [Test]
        public void PauseTask_StopsProgress_ResumeContinues()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            string taskId = ctx.World.ActiveTasks[0].TaskId;

            new CommandProcessor(ctx).Submit(new PauseTaskCommand("player", taskId));
            ctx.Clock.FastForward(10);
            Assert.AreEqual(0f, ctx.World.ActiveTasks[0].Progress);

            new CommandProcessor(ctx).Submit(new ResumeTaskCommand("player", taskId));
            ctx.Clock.FastForward(10);
            Assert.AreEqual(50f, ctx.World.ActiveTasks[0].Progress, 0.01f);
        }

        [Test]
        public void CancelTask_ReturnsMaterialsToShelterStorage()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            string taskId = ctx.World.ActiveTasks[0].TaskId;

            var result = new CommandProcessor(ctx).Submit(new CancelTaskCommand("player", taskId));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, ctx.World.ActiveTasks.Count);
            var storage = ctx.World.ShelterStates["shelter_main"].Storage;
            Assert.IsNotNull(storage);
            int totalScrap = 0;
            foreach (var item in storage.Items.Values) if (item.ItemId == "item_scrap") totalScrap += item.Quantity;
            Assert.AreEqual(2, totalScrap);
        }

        [Test]
        public void DismantleModule_RefundsHalfMaterials_FreesSlot()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            ctx.Clock.FastForward(20); // completes

            var result = new CommandProcessor(ctx).Submit(new DismantleModuleCommand("player", "slot_utility_1"));

            Assert.IsTrue(result.Success);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            Assert.AreEqual(0, shelter.Modules.Count);
            Assert.IsNull(shelter.BuildSlots["slot_utility_1"].ModuleInstanceId);

            int scrapCount = 0; // pump_part 1/2=0 omitted, scrap 2/2=1 refunded
            foreach (var item in ctx.World.Player.Inventory.Items.Values) if (item.ItemId == "item_scrap") scrapCount += item.Quantity;
            Assert.AreEqual(1, scrapCount);
        }

        [Test]
        public void CompletedPump_ReducesWaterIntrusion_ThroughWaterIntrusionSystem()
        {
            var ctx = BuildContext();
            GiveMaterials(ctx);
            new CommandProcessor(ctx).Submit(new StartBuildCommand("player", "slot_utility_1", "module_pump"));
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 50f;

            // tick@10 (-2, building): 48; tick@20 (-2, building completes this tick): 46; tick@30 (pump now active: -2-6): 38
            ctx.Clock.FastForward(30);

            Assert.AreEqual(38f, shelter.WaterIntrusion.Units, 0.01f);
        }
    }
}
