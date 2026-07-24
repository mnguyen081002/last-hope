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
    public class ConditionSystemTests
    {
        private static GameContext BuildContext(bool raining = false, bool atShelter = false)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "loc";
            world.CurrentDisasterPhase = raining ? "phase_rain" : "phase_dry";

            var bus = new EventBus();
            var phases = new Dictionary<string, DisasterPhaseDefinition>
            {
                ["phase_dry"] = new DisasterPhaseDefinition { Id = "phase_dry", StartMinute = 0, RainIntensity = 0 },
                ["phase_rain"] = new DisasterPhaseDefinition { Id = "phase_rain", StartMinute = 0, RainIntensity = 2 },
            };
            var locations = new Dictionary<string, LocationDefinition>
            {
                ["loc"] = new LocationDefinition { Id = "loc", IsShelter = atShelter },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(),
                new Dictionary<string, ItemDefinition>(),
                locations,
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>(),
                phases);
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        [Test]
        public void LongTick_AccruesHungerAndThirst()
        {
            var ctx = BuildContext();
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(10); // one long tick

            var c = ctx.World.Player.Condition;
            Assert.AreEqual(3.33f * (10f / 60f), c.Thirst, 0.001f);
            Assert.AreEqual(3.1f * (10f / 60f), c.Hunger, 0.001f);
        }

        [Test]
        public void LongTick_AccruesFatigue()
        {
            var ctx = BuildContext();
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0.2f, ctx.World.Player.Condition.Fatigue, 0.001f);
        }

        [Test]
        public void TravelCompleted_AddsFlatFatigue()
        {
            var ctx = BuildContext();
            _ = new ConditionSystem(ctx);

            ctx.Events.Publish(new TravelCompleted("route_a", "loc_a", "loc_b", 25));

            Assert.AreEqual(8f, ctx.World.Player.Condition.Fatigue, 0.001f);
        }

        [Test]
        public void ShortTick_RainingOutdoors_GainsWet_ShelterDries()
        {
            var rainingCtx = BuildContext(raining: true, atShelter: false);
            _ = new ConditionSystem(rainingCtx);
            rainingCtx.Clock.FastForward(5);
            float wetAfterRain = ConditionOps.GetStatusSeverity(rainingCtx.World.Player.Condition, ConditionOps.StatusWet);
            Assert.Greater(wetAfterRain, 0f);

            var shelterCtx = BuildContext(raining: true, atShelter: true);
            var c = shelterCtx.World.Player.Condition;
            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusWet, 20f, 0);
            _ = new ConditionSystem(shelterCtx);
            shelterCtx.Clock.FastForward(5);
            float wetAtShelter = ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet);
            Assert.Less(wetAtShelter, 20f);
        }

        [Test]
        public void ShortTick_WetAndRaining_DrivesBodyTempDown()
        {
            var ctx = BuildContext(raining: true, atShelter: false);
            var c = ctx.World.Player.Condition;
            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusWet, 90f, 0); // already above the 50 threshold
            _ = new ConditionSystem(ctx);

            float before = c.BodyTemperatureC;
            ctx.Clock.FastForward(1);

            Assert.Less(c.BodyTemperatureC, before);
        }

        [Test]
        public void ShortTick_ColdStatus_Hysteresis()
        {
            var ctx = BuildContext();
            var c = ctx.World.Player.Condition;
            c.BodyTemperatureC = 34.9f;
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(1);
            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold), 0f);

            c.BodyTemperatureC = 36.1f;
            ctx.Clock.FastForward(1);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusCold));
        }

        [Test]
        public void ShortTick_StaminaRegen_HalvedWhenExposed()
        {
            var normalCtx = BuildContext();
            _ = new ConditionSystem(normalCtx);
            normalCtx.World.Player.Condition.Stamina = 50f;
            normalCtx.Clock.FastForward(1);
            Assert.AreEqual(51f, normalCtx.World.Player.Condition.Stamina, 0.001f);

            var exposedCtx = BuildContext();
            var c = exposedCtx.World.Player.Condition;
            c.Stamina = 50f;
            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure, 100f, 0);
            _ = new ConditionSystem(exposedCtx);
            exposedCtx.Clock.FastForward(1);
            Assert.AreEqual(50.5f, c.Stamina, 0.001f);
        }

        [Test]
        public void LongTick_StarvationDecay_FloorsAtOne_NeverKills()
        {
            var ctx = BuildContext();
            var c = ctx.World.Player.Condition;
            c.Hunger = 100f;
            c.Health = 1.4f;
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(10); // one long tick: -0.5, would go to 0.9 without the floor

            Assert.AreEqual(1f, c.Health, 0.001f);
        }

        [Test]
        public void LongTick_HealthAtOrBelowFive_BecomesCollapsed()
        {
            var ctx = BuildContext();
            var c = ctx.World.Player.Condition;
            c.Health = 5f;
            _ = new ConditionSystem(ctx);

            ctx.Clock.FastForward(10);

            Assert.IsTrue(ConditionOps.IsIncapacitated(c));
        }
    }
}
