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
    }
}
