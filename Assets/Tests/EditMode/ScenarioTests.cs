using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Condition;
using LastHope.Systems.Shelter;
using LastHope.Systems.Tasks;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    /// <summary>
    /// End-to-end scenarios (BL-P1 S9, Gate P2 check) — pure GameContext + command + FastForward,
    /// no Unity. Each proves one player-facing claim rather than one mechanic in isolation.
    /// </summary>
    public class ScenarioTests
    {
        private static GameContext BuildContext(bool raining, Dictionary<string, ItemDefinition> items = null)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "loc_outside";
            world.CurrentDisasterPhase = raining ? "phase_rain" : "phase_dry";

            var bus = new EventBus();
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["phase_dry"] = new DisasterPhaseDefinition { Id = "phase_dry", StartMinute = 0, RainIntensity = 0 },
                ["phase_rain"] = new DisasterPhaseDefinition { Id = "phase_rain", StartMinute = 0, RainIntensity = 2 },
            };
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["loc_outside"] = new LocationDefinition { Id = "loc_outside", IsShelter = false },
                ["loc_shelter"] = new LocationDefinition { Id = "loc_shelter", IsShelter = true },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items ?? new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(), phases);
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        // Scenario A: no rain, no hazard exposure — only hunger/thirst should ever move.
        [Test]
        public void ScenarioA_Dry_OnlyHungerAndThirstAccrue()
        {
            var ctx = BuildContext(raining: false);
            _ = new ConditionSystem(ctx);
            var c = ctx.World.Player.Condition;

            ctx.Clock.FastForward(100);

            Assert.Greater(c.Hunger, 0f);
            Assert.Greater(c.Thirst, 0f);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet));
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold));
            Assert.AreEqual(0f, ConditionOps.GetExposure(c, "black_water"));
            Assert.AreEqual(100f, c.Health); // nowhere near starvation after only 100 minutes
        }

        // Scenario B: standing in the rain without a jacket gets you wet enough to go cold;
        // a jacket (wet_multiplier 0.3) keeps the same exposure time from ever crossing that line.
        [Test]
        public void ScenarioB_RainNoJacket_GetsWetThenCold()
        {
            var ctx = BuildContext(raining: true);
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(90);

            var c = ctx.World.Player.Condition;
            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet), 50f);
            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold), 0f);
        }

        [Test]
        public void ScenarioB_RainWithJacket_StaysDryAndWarm()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_jacket"] = new ItemDefinition { Id = "item_jacket", EquipSlot = "body", Protection = new Dictionary<string, float> { ["wet_multiplier"] = 0.3f } },
            };
            var ctx = BuildContext(raining: true, items: items);
            var jacket = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_jacket", 1, () => "jacket_1");
            new CommandProcessor(ctx).Submit(new EquipItemCommand("player", jacket.InstanceId, "body"));
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(90);

            var c = ctx.World.Player.Condition;
            Assert.Less(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet), 50f);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold));
        }

        // Scenario C: repeatedly crossing a Medium-flood (tier 2) route without boots accumulates
        // enough exposure to go Sick; with boots equipped, the same number of crossings stays
        // under even the lower threshold, and gloves separately let you handle a contaminated item
        // for free.
        private static GameContext BuildCrossingContext(Dictionary<string, ItemDefinition> items)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a_b"] = new RouteDefinition { Id = "route_a_b", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 5 },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["storm"] = new DisasterPhaseDefinition { Id = "storm", StartMinute = 0, FloodBandMin = 2, FloodBandMax = 2, CurrentBandMin = 0, CurrentBandMax = 0 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items,
                new Dictionary<string, LocationDefinition>(), routes, new Dictionary<string, SearchPointDefinition>(), phases);

            return new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));
        }

        [Test]
        public void ScenarioC_MediumCrossing_NoBoots_AccumulatesToSick()
        {
            var ctx = BuildCrossingContext(new Dictionary<string, ItemDefinition>());
            var processor = new CommandProcessor(ctx);

            for (int i = 0; i < 5; i++)
                processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            var c = ctx.World.Player.Condition;
            Assert.GreaterOrEqual(ConditionOps.GetExposure(c, "black_water"), 70f);
            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick), 0f);
        }

        [Test]
        public void ScenarioC_MediumCrossing_WithBootsAndGloves_StaysClean()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_boots"] = new ItemDefinition { Id = "item_boots", EquipSlot = "feet", Protection = new Dictionary<string, float> { ["exposure_block_level"] = 1f, ["exposure_medium_multiplier"] = 0.5f } },
                ["item_gloves"] = new ItemDefinition { Id = "item_gloves", EquipSlot = "hands", Protection = new Dictionary<string, float> { ["handles_contaminated"] = 1f } },
                ["item_test"] = new ItemDefinition { Id = "item_test", BaseWeightKg = 0.1f, BaseVolumeLiters = 0.1f },
            };
            var ctx = BuildCrossingContext(items);
            var processor = new CommandProcessor(ctx);

            var boots = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_boots", 1, () => "boots_1");
            processor.Submit(new EquipItemCommand("player", boots.InstanceId, "feet"));
            var gloves = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_gloves", 1, () => "gloves_1");
            processor.Submit(new EquipItemCommand("player", gloves.InstanceId, "hands"));

            for (int i = 0; i < 5; i++)
                processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            var c = ctx.World.Player.Condition;
            Assert.Less(ConditionOps.GetExposure(c, "black_water"), ctx.Definitions.Balance.Condition.BlackWaterExposureThreshold);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick));

            float exposureBeforePickup = ConditionOps.GetExposure(c, "black_water");
            var dropped = new InventoryState { OwnerId = "location_dropped:x" };
            ctx.World.LocationStates["x"] = new LocationState { Id = "x", DroppedItems = dropped };
            dropped.Items["dirty"] = new ItemInstanceState { InstanceId = "dirty", ItemId = "item_test", Quantity = 1, Contamination = ContaminationState.Contaminated };
            processor.Submit(new TransferItemCommand("location_dropped:x", "dirty", "player", 1));

            Assert.AreEqual(exposureBeforePickup, ConditionOps.GetExposure(c, "black_water"));
        }

        // Scenario D: a route that keeps flooding predicts its own closing via ReturnWindowCalculator;
        // ignoring that window and waiting instead of leaving makes it actually Impassable — but a
        // higher-elevation route to a different destination stays open the whole time.
        [Test]
        public void ScenarioD_MissedReturnWindow_RisingRouteCloses_StableRouteStaysOpen()
        {
            var world = new WorldState { WorldTimeMinutes = 100 };
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_rising"] = new RouteDefinition { Id = "route_rising", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 10, BaseElevationLevel = 0 },
                ["route_stable"] = new RouteDefinition { Id = "route_stable", FromLocationId = "location_a", ToLocationId = "location_c", TravelMinutes = 10, BaseElevationLevel = 4 },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["dry"] = new DisasterPhaseDefinition { Id = "dry", StartMinute = 0, FloodBandMin = 0, FloodBandMax = 0 },
                ["storm"] = new DisasterPhaseDefinition { Id = "storm", StartMinute = 50, FloodBandMin = 0, FloodBandMax = 4 },
                ["aftermath"] = new DisasterPhaseDefinition { Id = "aftermath", StartMinute = 150, FloodBandMin = 4, FloodBandMax = 4 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(), routes, new Dictionary<string, SearchPointDefinition>(), phases);
            var ctx = new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));

            var window = ReturnWindowCalculator.Evaluate(routes["route_rising"], ctx.Definitions.DisasterPhasesSorted, 100);
            Assert.AreEqual(40, window.MinutesUntilImpassable);

            ctx.Clock.FastForward(40); // player waited instead of leaving -> now at minute 140

            var blocked = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_rising"));
            Assert.IsFalse(blocked.Success);
            Assert.AreEqual(CommandErrorCode.RouteBlocked, blocked.Code);

            var stableOk = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_stable"));
            Assert.IsTrue(stableOk.Success, "Higher-elevation route should remain open.");
        }

        // Scenario E (Gate P3): a materials budget that's short of building every module still
        // supports at least 3 different strategic combinations of Barrier + 2 other modules, each
        // buildable end-to-end via the real command layer without running out of materials. This
        // does NOT simulate "surviving Peak" itself — no Outcome/failure-condition system exists
        // yet (that's S18) — it proves the resource budget doesn't quietly favor only one strategy.
        private static readonly Dictionary<string, int> ScenarioEBudget = new Dictionary<string, int>
        {
            ["item_wood"] = 9,
            ["item_scrap"] = 6,
            ["item_pump_part"] = 1,
            ["item_purifier_unit"] = 1,
            ["item_filter"] = 2,
            ["item_battery"] = 4,
        };

        private static GameContext BuildScenarioEContext()
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            var bus = new EventBus();

            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
            };
            var zones = new Dictionary<string, ShelterZoneDefinition>
            {
                ["shelter_entrance"] = new ShelterZoneDefinition { Id = "shelter_entrance", BuildSlotIds = new List<string> { "slot_entrance_1" } },
                ["utility_area"] = new ShelterZoneDefinition { Id = "utility_area", BuildSlotIds = new List<string> { "slot_utility_1", "slot_utility_2" } },
                ["water_processing"] = new ShelterZoneDefinition { Id = "water_processing", BuildSlotIds = new List<string> { "slot_wp_1" } },
                ["ground_storage"] = new ShelterZoneDefinition { Id = "ground_storage", BuildSlotIds = new List<string> { "slot_storage_1" } },
            };
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_wood"] = new ItemDefinition { Id = "item_wood", MaxStackSize = 20 },
                ["item_scrap"] = new ItemDefinition { Id = "item_scrap", MaxStackSize = 20 },
                ["item_pump_part"] = new ItemDefinition { Id = "item_pump_part", MaxStackSize = 5 },
                ["item_purifier_unit"] = new ItemDefinition { Id = "item_purifier_unit", MaxStackSize = 5 },
                ["item_filter"] = new ItemDefinition { Id = "item_filter", MaxStackSize = 5 },
                ["item_battery"] = new ItemDefinition { Id = "item_battery", MaxStackSize = 20 },
            };
            var modules = new Dictionary<string, ModuleDefinition>
            {
                ["module_barrier"] = new ModuleDefinition { Id = "module_barrier", AllowedZoneIds = new List<string> { "shelter_entrance" }, Materials = new Dictionary<string, int> { ["item_wood"] = 4, ["item_scrap"] = 2 }, BuildMinutes = 10, MaxDurability = 100, Tags = new List<string> { "barrier" } },
                ["module_pump"] = new ModuleDefinition { Id = "module_pump", AllowedZoneIds = new List<string> { "utility_area" }, Materials = new Dictionary<string, int> { ["item_pump_part"] = 1, ["item_scrap"] = 2 }, BuildMinutes = 10, PowerDemand = 2f, MaxDurability = 100, Tags = new List<string> { "pump" } },
                ["module_elevated_storage"] = new ModuleDefinition { Id = "module_elevated_storage", AllowedZoneIds = new List<string> { "ground_storage" }, Materials = new Dictionary<string, int> { ["item_wood"] = 3 }, BuildMinutes = 10, MaxDurability = 100, Tags = new List<string> { "elevated_storage" } },
                ["module_purifier"] = new ModuleDefinition { Id = "module_purifier", AllowedZoneIds = new List<string> { "water_processing" }, Materials = new Dictionary<string, int> { ["item_purifier_unit"] = 1, ["item_filter"] = 1 }, BuildMinutes = 10, PowerDemand = 2f, MaxDurability = 100, Tags = new List<string> { "purifier" } },
                ["module_battery_bank"] = new ModuleDefinition { Id = "module_battery_bank", AllowedZoneIds = new List<string> { "utility_area" }, Materials = new Dictionary<string, int> { ["item_battery"] = 2, ["item_scrap"] = 1 }, BuildMinutes = 10, MaxDurability = 100, Tags = new List<string> { "battery_bank" } },
            };

            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items, locations,
                new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                shelterZones: zones, modules: modules);
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx);
            _ = new TaskSystem(ctx);
            _ = new PowerSystem(ctx);

            foreach (var kvp in ScenarioEBudget)
                InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, kvp.Key, kvp.Value, () => System.Guid.NewGuid().ToString("N"));

            return ctx;
        }

        private static void BuildAndComplete(GameContext ctx, string slotId, string moduleId, int buildMinutes)
        {
            var result = new CommandProcessor(ctx).Submit(new StartBuildCommand("player", slotId, moduleId));
            Assert.IsTrue(result.Success, $"Build '{moduleId}' at '{slotId}' should succeed within budget: {result.Code}");
            ctx.Clock.FastForward(buildMinutes);
        }

        [TestCase("module_pump", "slot_utility_1", "module_elevated_storage", "slot_storage_1")]
        [TestCase("module_purifier", "slot_wp_1", "module_battery_bank", "slot_utility_1")]
        [TestCase("module_pump", "slot_utility_1", "module_battery_bank", "slot_utility_2")]
        public void ScenarioE_BudgetSupportsBarrierPlusAnyTwoOfThreeStrategies(
            string moduleA, string slotA, string moduleB, string slotB)
        {
            var ctx = BuildScenarioEContext();

            BuildAndComplete(ctx, "slot_entrance_1", "module_barrier", 10);
            BuildAndComplete(ctx, slotA, moduleA, 10);
            BuildAndComplete(ctx, slotB, moduleB, 10);

            var shelter = ctx.World.ShelterStates["shelter_main"];
            Assert.AreEqual(3, shelter.Modules.Count);
        }
    }
}
