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
    }
}
