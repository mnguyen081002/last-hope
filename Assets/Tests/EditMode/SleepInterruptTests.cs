using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class SleepInterruptTests
    {
        private static GameContext BuildContext(bool atShelter = true)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = atShelter ? "location_shelter" : "elsewhere";
            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>());
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx); // seeds shelter_main
            return ctx;
        }

        [Test]
        public void NotAtShelter_FailsNotAtLocation()
        {
            var ctx = BuildContext(atShelter: false);
            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", 60));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.NotAtLocation, result.Code);
        }

        [Test]
        public void Incapacitated_FailsIncapacitated()
        {
            var ctx = BuildContext();
            ConditionOps.ApplyHealth(ctx.World.Player.Condition, -100f); // Health -> 0, below CollapsedHealthThreshold
            ConditionOps.RecomputeIncapacitation(ctx.World.Player.Condition, ctx.Definitions.Balance.Condition);

            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", 60));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.Incapacitated, result.Code);
        }

        [Test]
        public void ShelterDeepFlooded_FailsUnsafeToSleep()
        {
            var ctx = BuildContext();
            ctx.World.ShelterStates["shelter_main"].WaterIntrusion.Level = WaterIntrusionLevel.Deep;

            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", 60));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.UnsafeToSleep, result.Code);
        }

        [Test]
        public void NormalSleep_CompletesFullDuration_PublishesSleepEnded()
        {
            var ctx = BuildContext();
            bool ended = false;
            ctx.Events.Subscribe<SleepEnded>(_ => ended = true);
            long before = ctx.World.WorldTimeMinutes;

            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", 60));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(ended);
            Assert.AreEqual(before + 60, ctx.World.WorldTimeMinutes);
        }

        [Test]
        public void ShelterStartsSafe_FloodRisesDuringSleep_WakesEarly_PublishesSleepInterrupted()
        {
            // Real rain phase this time (RainIntensity 3 -> +6 inflow -2 passive drain = +4/long-tick)
            // so WaterIntrusionSystem genuinely pushes Units past Deep(60) partway through the sleep.
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            world.CurrentDisasterPhase = "phase_storm"; // no DisasterPhaseSystem wired in this test — set directly
            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["location_shelter"] = new LocationDefinition { Id = "location_shelter", IsShelter = true },
            };
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["phase_storm"] = new DisasterPhaseDefinition { Id = "phase_storm", StartMinute = 0, RainIntensity = 3 },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                disasterPhases: phases);
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx);

            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 50f; // safe (Shallow) at sleep start; +4/tick will cross Deep(60) at minute 30
            int? minutesSlept = null;
            ctx.Events.Subscribe<SleepInterrupted>(e => minutesSlept = e.MinutesSlept);

            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", 120));

            Assert.IsTrue(result.Success); // sleep itself succeeds (wakes early, doesn't fail)
            Assert.IsNotNull(minutesSlept);
            Assert.AreEqual(30, minutesSlept.Value);
            Assert.AreEqual(WaterIntrusionLevel.Deep, shelter.WaterIntrusion.Level);
            Assert.Less(ctx.World.WorldTimeMinutes, 120); // woke up before the full 120 minutes
        }

        [Test]
        public void FastForwardWithInterrupt_StopsEarly_ReturnsElapsedMinutes()
        {
            var ctx = BuildContext();
            int elapsed = ctx.Clock.FastForward(100, m => m >= 30);
            Assert.AreEqual(30, elapsed);
            Assert.AreEqual(30, ctx.World.WorldTimeMinutes);
        }

        [Test]
        public void FastForwardWithInterrupt_NeverTriggers_RunsFullDuration()
        {
            var ctx = BuildContext();
            int elapsed = ctx.Clock.FastForward(20, m => false);
            Assert.AreEqual(20, elapsed);
        }
    }
}
