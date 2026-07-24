using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Events;
using LastHope.Systems.Shelter;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class EventCommandsTests
    {
        private static Dictionary<string, EventDefinition> Events() => new Dictionary<string, EventDefinition>
        {
            ["event_drain_backflow"] = new EventDefinition
            {
                Id = "event_drain_backflow",
                TriggerRequiresBlackWater = true,
                TriggerStateMinLevel = "Damp",
                AvailableResponses = new List<string> { "reinforce_seal" },
                Tags = new List<string> { "drain_backflow" },
            },
            ["event_storage_flood_risk"] = new EventDefinition
            {
                Id = "event_storage_flood_risk",
                TriggerStateMinLevel = "Shallow",
                AvailableResponses = new List<string> { "secure_storage" },
                Tags = new List<string> { "storage_flood_risk" },
            },
        };

        private static Dictionary<string, DisasterPhaseDefinition> Phases() => new Dictionary<string, DisasterPhaseDefinition>
        {
            ["phase_black_rain"] = new DisasterPhaseDefinition { Id = "phase_black_rain", StartMinute = 0, RainIntensity = 2, BlackWater = true },
        };

        private static GameContext BuildContext(Dictionary<string, EventDefinition> events)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            world.CurrentDisasterPhase = "phase_black_rain";
            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                disasterPhases: Phases(), events: events);
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx);
            _ = new EventSystem(ctx);
            return ctx;
        }

        [Test]
        public void DrainBackflow_TriggersOnce_WhenBlackWaterPhaseAndWaterAtDampOrAbove()
        {
            var ctx = BuildContext(Events());
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 12f; // Damp
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;

            string triggeredId = null;
            ctx.Events.Subscribe<EventTriggered>(e => triggeredId = e.EventId);

            ctx.Clock.FastForward(10);

            Assert.AreEqual("event_drain_backflow", triggeredId);
            Assert.AreEqual(1, ctx.World.ActiveEvents.Count);
            Assert.AreEqual(EventLifecycleState.Active, ctx.World.ActiveEvents[0].State);
            Assert.IsTrue(shelter.EventFlags.Contains(ShelterEventFlags.DrainBackflowActive));
        }

        [Test]
        public void DrainBackflow_DoesNotTrigger_WhenShelterStillDry()
        {
            var ctx = BuildContext(Events());
            int count = 0;
            ctx.Events.Subscribe<EventTriggered>(_ => count++);

            ctx.Clock.FastForward(10); // shelter starts Dry, no rain-driven rise this fast

            Assert.AreEqual(0, count);
        }

        [Test]
        public void DrainBackflow_DoesNotDoubleTrigger_WhileAlreadyActive()
        {
            // Isolated to just this event: with both events loaded, the ongoing backflow eventually
            // pushes WaterIntrusion into Shallow too, which would legitimately also trigger
            // event_storage_flood_risk — a different assertion than "does this one event double-fire".
            var events = new Dictionary<string, EventDefinition> { ["event_drain_backflow"] = Events()["event_drain_backflow"] };
            var ctx = BuildContext(events);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 15f; // Damp band — must match Level or WaterIntrusionSystem overwrites Level on the next tick
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;

            ctx.Clock.FastForward(10);
            ctx.Clock.FastForward(10);
            ctx.Clock.FastForward(10);

            Assert.AreEqual(1, ctx.World.ActiveEvents.Count);
        }

        [Test]
        public void ResolveEvent_ReinforceSeal_ClearsFlag_AdvancesClock_MarksResolved()
        {
            var ctx = BuildContext(Events());
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 15f;
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;
            ctx.Clock.FastForward(10);
            string instanceId = ctx.World.ActiveEvents[0].EventInstanceId;
            long before = ctx.World.WorldTimeMinutes;

            var result = new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", instanceId, "reinforce_seal"));

            Assert.IsTrue(result.Success);
            Assert.IsFalse(shelter.EventFlags.Contains(ShelterEventFlags.DrainBackflowActive));
            Assert.AreEqual(before + 15, ctx.World.WorldTimeMinutes);
            Assert.AreEqual(EventLifecycleState.Resolved, ctx.World.ActiveEvents[0].State);
            Assert.AreEqual("reinforce_seal", ctx.World.ActiveEvents[0].ChosenResponse);
        }

        [Test]
        public void ResolveEvent_UnknownInstance_FailsEventNotActive()
        {
            var ctx = BuildContext(Events());
            var result = new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", "nonexistent", "reinforce_seal"));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.EventNotActive, result.Code);
        }

        [Test]
        public void ResolveEvent_UnavailableResponse_FailsResponseUnavailable()
        {
            var ctx = BuildContext(Events());
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 15f;
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;
            ctx.Clock.FastForward(10);
            string instanceId = ctx.World.ActiveEvents[0].EventInstanceId;

            var result = new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", instanceId, "not_a_real_response"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.ResponseUnavailable, result.Code);
        }

        [Test]
        public void ResolveEvent_AlreadyResolved_CannotResolveAgain()
        {
            var ctx = BuildContext(Events());
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 15f;
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;
            ctx.Clock.FastForward(10);
            string instanceId = ctx.World.ActiveEvents[0].EventInstanceId;
            var processor = new CommandProcessor(ctx);
            processor.Submit(new ResolveEventCommand("player", instanceId, "reinforce_seal"));

            var result = processor.Submit(new ResolveEventCommand("player", instanceId, "reinforce_seal"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.EventNotActive, result.Code);
        }

        [Test]
        public void StorageFloodRisk_TriggersOnShallowWater_IndependentOfPhase()
        {
            var events = new Dictionary<string, EventDefinition> { ["event_storage_flood_risk"] = Events()["event_storage_flood_risk"] };
            var ctx = BuildContext(events);
            ctx.World.CurrentDisasterPhase = ""; // not black rain — this event has no phase requirement
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 35f; // Shallow band
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Shallow;

            ctx.Clock.FastForward(10);

            Assert.AreEqual(1, ctx.World.ActiveEvents.Count);
            Assert.AreEqual("event_storage_flood_risk", ctx.World.ActiveEvents[0].EventId);
        }

        [Test]
        public void EventCanRetrigger_AfterBeingResolved_IfConditionsHoldAgain()
        {
            var ctx = BuildContext(Events());
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 15f;
            shelter.WaterIntrusion.Level = WaterIntrusionLevel.Damp;
            ctx.Clock.FastForward(10);
            string firstInstanceId = ctx.World.ActiveEvents[0].EventInstanceId;
            new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", firstInstanceId, "reinforce_seal"));

            ctx.Clock.FastForward(10); // conditions (Damp + black rain) still hold

            Assert.AreEqual(2, ctx.World.ActiveEvents.Count);
        }
    }
}
