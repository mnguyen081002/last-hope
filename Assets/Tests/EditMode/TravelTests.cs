using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class TravelTests
    {
        private static GameContext BuildContext()
        {
            var world = new WorldState { RandomSeed = 1 };
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a_b"] = new RouteDefinition { Id = "route_a_b", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 20 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(), routes, new Dictionary<string, SearchPointDefinition>());

            return new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));
        }

        [Test]
        public void AdjacentRoute_ChangesLocation_NormalLoad()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);
            long before = ctx.World.WorldTimeMinutes;

            var result = processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("location_b", ctx.World.Player.CurrentLocationId);
            Assert.AreEqual(before + 20, ctx.World.WorldTimeMinutes); // LoadFactorNormal = 1.0
        }

        [Test]
        public void HeavyOverload_ScalesTravelTime()
        {
            var ctx = BuildContext();
            ctx.World.Player.Inventory.Overload = OverloadState.Heavy; // LoadFactorHeavy = 1.5 by default
            var processor = new CommandProcessor(ctx);
            long before = ctx.World.WorldTimeMinutes;

            processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.AreEqual(before + 30, ctx.World.WorldTimeMinutes); // ceil(20 * 1.5) = 30
        }

        [Test]
        public void ReverseDirection_AlsoWorks()
        {
            var ctx = BuildContext();
            ctx.World.Player.CurrentLocationId = "location_b";
            var processor = new CommandProcessor(ctx);

            var result = processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual("location_a", ctx.World.Player.CurrentLocationId);
        }

        [Test]
        public void NotOnRoute_FailsNotAtLocation()
        {
            var ctx = BuildContext();
            ctx.World.Player.CurrentLocationId = "location_c";
            var processor = new CommandProcessor(ctx);

            var result = processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NotAtLocation, result.Code);
        }

        [Test]
        public void TravelCompleted_PublishedWithCorrectMinutes()
        {
            var ctx = BuildContext();
            var processor = new CommandProcessor(ctx);
            TravelCompleted? received = null;
            ctx.Events.Subscribe<TravelCompleted>(e => received = e);

            processor.Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsTrue(received.HasValue);
            Assert.AreEqual("route_a_b", received.Value.RouteId);
            Assert.AreEqual("location_a", received.Value.FromLocationId);
            Assert.AreEqual("location_b", received.Value.ToLocationId);
            Assert.AreEqual(20, received.Value.MinutesSpent);
        }

        private static GameContext BuildHazardContext(int floodBandMax)
        {
            var world = new WorldState { RandomSeed = 1 };
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a_b"] = new RouteDefinition { Id = "route_a_b", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 20 },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["storm"] = new DisasterPhaseDefinition { Id = "storm", StartMinute = 0, FloodBandMin = floodBandMax, FloodBandMax = floodBandMax, CurrentBandMin = 0, CurrentBandMax = 0 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(), routes, new Dictionary<string, SearchPointDefinition>(), phases);

            return new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));
        }

        [Test]
        public void FloodAtMaxLevel_BlocksTravel()
        {
            var ctx = BuildHazardContext(floodBandMax: 4);
            var result = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.RouteBlocked, result.Code);
            Assert.AreEqual("location_a", ctx.World.Player.CurrentLocationId); // never moved
        }

        [Test]
        public void Crossing_AppliesStaminaExposureWetCost_AfterTravel()
        {
            var ctx = BuildHazardContext(floodBandMax: 1); // tier 1: stamina 5, exposure 5, wet 30
            ctx.World.Player.Condition.Stamina = 100f;

            var result = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(95f, ctx.World.Player.Condition.Stamina, 0.001f);
            Assert.AreEqual(5f, ConditionOps.GetExposure(ctx.World.Player.Condition, "black_water"), 0.001f);
            Assert.AreEqual(30f, ConditionOps.GetStatusSeverity(ctx.World.Player.Condition, ConditionOps.StatusWet), 0.001f);
        }

        [Test]
        public void LowStamina_ShortfallConvertsToFatigue_TravelStillSucceeds()
        {
            var ctx = BuildHazardContext(floodBandMax: 3); // tier 3: stamina cost 30
            ctx.World.Player.Condition.Stamina = 10f;

            var result = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0f, ctx.World.Player.Condition.Stamina); // clamped floor
            Assert.AreEqual(20f, ctx.World.Player.Condition.Fatigue, 0.001f); // 30 - 10 shortfall
        }

        [Test]
        public void Incapacitated_CannotTravel()
        {
            var ctx = BuildHazardContext(floodBandMax: 0);
            ctx.World.Player.Condition.Health = 3f;
            ConditionOps.RecomputeIncapacitation(ctx.World.Player.Condition, ctx.Definitions.Balance.Condition);

            var result = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.Incapacitated, result.Code);
        }

        [Test]
        public void Rope_MakesOtherwiseImpassableCurrent_Passable()
        {
            var world = new WorldState { RandomSeed = 1 };
            world.Player.CurrentLocationId = "location_a";
            var bus = new EventBus();

            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a_b"] = new RouteDefinition { Id = "route_a_b", FromLocationId = "location_a", ToLocationId = "location_b", TravelMinutes = 20 },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["storm"] = new DisasterPhaseDefinition { Id = "storm", StartMinute = 0, FloodBandMin = 0, FloodBandMax = 0, CurrentBandMin = 4, CurrentBandMax = 4 },
            };
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_rope"] = new ItemDefinition { Id = "item_rope", EquipSlot = "tool", Protection = new Dictionary<string, float> { ["current_reduction"] = 1f } },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), items,
                new Dictionary<string, LocationDefinition>(), routes, new Dictionary<string, SearchPointDefinition>(), phases);
            var ctx = new GameContext(world, registry, bus, new RngService(world), new TickScheduler(world, bus));

            var blocked = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));
            Assert.IsFalse(blocked.Success, "Sanity check: current at MaxLevel should block without rope.");

            var rope = InventoryOps.AddItem(ctx.World.Player.Inventory, ctx.Definitions, "item_rope", 1, () => "rope_1");
            new CommandProcessor(ctx).Submit(new EquipItemCommand("player", rope.InstanceId, "tool"));

            var result = new CommandProcessor(ctx).Submit(new BeginTravelCommand("player", "route_a_b"));
            Assert.IsTrue(result.Success);
        }
    }
}
