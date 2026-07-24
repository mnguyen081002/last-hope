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
    public class ShelterCommandsTests
    {
        private static Dictionary<string, LocationDefinition> Locations() => new Dictionary<string, LocationDefinition>
        {
            ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
        };

        private static Dictionary<string, ShelterZoneDefinition> Zones() => new Dictionary<string, ShelterZoneDefinition>
        {
            ["water_processing"] = new ShelterZoneDefinition { Id = "water_processing", BuildSlotIds = new List<string> { "slot_wp_1" } },
        };

        private static Dictionary<string, ItemDefinition> Items() => new Dictionary<string, ItemDefinition>
        {
            ["item_water_bottle"] = new ItemDefinition { Id = "item_water_bottle", MaxStackSize = 10 },
        };

        private static Dictionary<string, ModuleDefinition> Modules() => new Dictionary<string, ModuleDefinition>
        {
            ["module_purifier"] = new ModuleDefinition
            {
                Id = "module_purifier",
                AllowedZoneIds = new List<string> { "water_processing" },
                MaxDurability = 100,
                PowerDemand = 2f,
                Tags = new List<string> { "purifier" },
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
            _ = new WaterIntrusionSystem(ctx);
            _ = new PowerSystem(ctx);
            _ = new WaterSystem(ctx);
            return ctx;
        }

        private static ModuleState AddPurifierModule(GameContext ctx, bool active, float durability)
        {
            var shelter = ctx.World.ShelterStates["shelter_main"];
            var module = new ModuleState { InstanceId = "purifier_1", ModuleId = "module_purifier", SlotId = "slot_wp_1", Active = active, Durability = durability };
            shelter.Modules[module.InstanceId] = module;
            shelter.BuildSlots["slot_wp_1"].ModuleInstanceId = module.InstanceId;
            return module;
        }

        [Test]
        public void StartPurifyBatch_NotPowered_FailsNoPower()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: false, durability: 100f);

            var result = new CommandProcessor(ctx).Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NoPower, result.Code);
        }

        [Test]
        public void StartPurifyBatch_NoFilterLeft_FailsNoFilter()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: true, durability: 0f);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Untreated = 10f;

            var result = new CommandProcessor(ctx).Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NoFilter, result.Code);
        }

        [Test]
        public void StartPurifyBatch_NotEnoughUntreated_FailsNothingToPurify()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: true, durability: 100f);
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Untreated = 1f; // batch needs 3

            var result = new CommandProcessor(ctx).Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NothingToPurify, result.Code);
        }

        [Test]
        public void StartPurifyBatch_Success_ConvertsWater_WearsFilter_AdvancesClock()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: true, durability: 100f);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterStocks.Untreated = 10f;
            shelter.WaterStocks.Clean = 0f;
            long before = ctx.World.WorldTimeMinutes;

            var result = new CommandProcessor(ctx).Submit(new StartPurifyBatchCommand("player", "purifier_1"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(7f, shelter.WaterStocks.Untreated, 0.01f);
            Assert.AreEqual(3f, shelter.WaterStocks.Clean, 0.01f);
            Assert.AreEqual(before + 60, ctx.World.WorldTimeMinutes); // PurifyBatchMinutes
            Assert.Less(shelter.Modules["purifier_1"].Durability, 100f);
        }

        [Test]
        public void ThreeBatches_ExhaustsFilter()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: true, durability: 100f);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterStocks.Untreated = 100f;

            var processor = new CommandProcessor(ctx);
            processor.Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            processor.Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            processor.Submit(new StartPurifyBatchCommand("player", "purifier_1"));

            Assert.AreEqual(0f, shelter.Modules["purifier_1"].Durability, 0.01f);
            var fourthAttempt = processor.Submit(new StartPurifyBatchCommand("player", "purifier_1"));
            Assert.AreEqual(CommandErrorCode.NoFilter, fourthAttempt.Code);
        }

        [Test]
        public void CollectWater_MovesFromShelterStockIntoPlayerInventoryAsBottles()
        {
            var ctx = BuildContext();
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 5f;

            var result = new CommandProcessor(ctx).Submit(new CollectWaterCommand("player", 3));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2f, ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean, 0.01f);
            int bottles = 0;
            foreach (var item in ctx.World.Player.Inventory.Items.Values) if (item.ItemId == "item_water_bottle") bottles += item.Quantity;
            Assert.AreEqual(3, bottles);
        }

        [Test]
        public void CollectWater_NotEnoughStock_Fails()
        {
            var ctx = BuildContext();
            ctx.World.ShelterStates["shelter_main"].WaterStocks.Clean = 1f;

            var result = new CommandProcessor(ctx).Submit(new CollectWaterCommand("player", 3));
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void SetPowerPriority_UnknownModule_FailsInvalidTarget()
        {
            var ctx = BuildContext();
            var result = new CommandProcessor(ctx).Submit(new SetPowerPriorityCommand("player", "nonexistent", PowerPriority.Critical));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Code);
        }

        [Test]
        public void SetPowerPriority_ThenPowerSystemLongTick_RespectsPriority()
        {
            var ctx = BuildContext();
            AddPurifierModule(ctx, active: false, durability: 100f);

            new CommandProcessor(ctx).Submit(new SetPowerPriorityCommand("player", "purifier_1", PowerPriority.Critical));
            ctx.Clock.FastForward(10); // PowerSystem long-tick

            Assert.IsTrue(ctx.World.ShelterStates["shelter_main"].Modules["purifier_1"].Active);
        }

        [Test]
        public void WaterSystem_PassiveIntake_OnlyWhenRaining()
        {
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["phase_rain"] = new DisasterPhaseDefinition { Id = "phase_rain", StartMinute = 0, RainIntensity = 1 },
            };
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            world.CurrentDisasterPhase = "phase_rain";
            var bus = new EventBus();
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), Items(), Locations(),
                new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                disasterPhases: phases, shelterZones: Zones(), modules: Modules());
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx);
            _ = new WaterSystem(ctx);

            ctx.Clock.FastForward(60); // 6 long ticks, 1 hour of rain

            Assert.AreEqual(1f, ctx.World.ShelterStates["shelter_main"].WaterStocks.Untreated, 0.01f);
        }
    }
}
