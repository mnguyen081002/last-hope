using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Intel;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class IntelSystemTests
    {
        private static GameContext BuildContext()
        {
            var world = new WorldState();
            var bus = new EventBus();
            var routes = new Dictionary<string, RouteDefinition>
            {
                ["route_a"] = new RouteDefinition { Id = "route_a", FromLocationId = "location_shelter", ToLocationId = "location_store", TravelMinutes = 20 },
            };
            var searchPoints = new Dictionary<string, SearchPointDefinition>
            {
                ["sp_shelf"] = new SearchPointDefinition { Id = "sp_shelf", LocationId = "location_store" },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["phase_dry"] = new DisasterPhaseDefinition { Id = "phase_dry", StartMinute = 0, RainIntensity = 0 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(), routes, searchPoints, disasterPhases: phases);
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new IntelSystem(ctx);
            return ctx;
        }

        [Test]
        public void TravelCompleted_RecordsConfirmedIntel_ForRouteAndDestination()
        {
            var ctx = BuildContext();
            ctx.World.WorldTimeMinutes = 50;

            ctx.Events.Publish(new TravelCompleted("route_a", "location_shelter", "location_store", 20));

            Assert.IsTrue(ctx.World.Intel.Records.TryGetValue("route_a", out var routeRecord));
            Assert.AreEqual(IntelConfidence.Confirmed, routeRecord.Confidence);
            Assert.AreEqual(50, routeRecord.ObservedAtMinute);

            Assert.IsTrue(ctx.World.Intel.Records.TryGetValue("location_store", out var locationRecord));
            Assert.AreEqual(IntelConfidence.Confirmed, locationRecord.Confidence);
        }

        [Test]
        public void SearchPointOpened_RecordsConfirmedIntel_ForItsLocation()
        {
            var ctx = BuildContext();
            ctx.World.WorldTimeMinutes = 5;

            ctx.Events.Publish(new SearchPointOpened("sp_shelf", firstOpen: true));

            Assert.IsTrue(ctx.World.Intel.Records.TryGetValue("location_store", out var record));
            Assert.AreEqual(IntelConfidence.Confirmed, record.Confidence);
        }

        [Test]
        public void RecordRouteObservation_PublishesIntelUpdated()
        {
            var ctx = BuildContext();
            string updatedSubject = null;
            ctx.Events.Subscribe<IntelUpdated>(e => updatedSubject = e.SubjectId);

            IntelSystem.RecordRouteObservation(ctx, "route_a", IntelConfidence.Confirmed);

            Assert.AreEqual("route_a", updatedSubject);
        }

        [Test]
        public void RecordRouteObservation_OlderReObservation_DoesNotOverwriteNewer()
        {
            var ctx = BuildContext();
            ctx.World.WorldTimeMinutes = 100;
            IntelSystem.RecordRouteObservation(ctx, "route_a", IntelConfidence.Confirmed);

            ctx.World.WorldTimeMinutes = 40; // time went "backwards" relative to the stored record — shouldn't happen live, but the guard must hold
            IntelSystem.RecordRouteObservation(ctx, "route_a", IntelConfidence.Uncertain);

            Assert.AreEqual(100, ctx.World.Intel.Records["route_a"].ObservedAtMinute);
        }

        [Test]
        public void RecordRouteObservation_UnknownRoute_NoOp()
        {
            var ctx = BuildContext();
            IntelSystem.RecordRouteObservation(ctx, "route_does_not_exist", IntelConfidence.Confirmed);
            Assert.IsFalse(ctx.World.Intel.Records.ContainsKey("route_does_not_exist"));
        }
    }
}
