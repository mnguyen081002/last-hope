using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Disaster;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class DisasterPhaseSystemTests
    {
        private static Dictionary<string, DisasterPhaseDefinition> BuildPhases() => new Dictionary<string, DisasterPhaseDefinition>
        {
            ["phase_dry"] = new DisasterPhaseDefinition { Id = "phase_dry", StartMinute = 0, RainIntensity = 0 },
            ["phase_first_rain"] = new DisasterPhaseDefinition { Id = "phase_first_rain", StartMinute = 30, RainIntensity = 1 },
            ["phase_black_rain"] = new DisasterPhaseDefinition { Id = "phase_black_rain", StartMinute = 80, RainIntensity = 2, BlackWater = true },
        };

        private static GameContext BuildContext(long startWorldTime = 0)
        {
            var world = new WorldState { WorldTimeMinutes = startWorldTime };
            var bus = new EventBus();
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(),
                new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(),
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>(),
                BuildPhases());
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        [Test]
        public void Construct_AtMinuteZero_SetsFirstPhase()
        {
            var ctx = BuildContext();
            _ = new DisasterPhaseSystem(ctx);

            Assert.AreEqual("phase_dry", ctx.World.CurrentDisasterPhase);
        }

        [Test]
        public void FastForward_CrossesThresholds_TransitionsInOrder()
        {
            var ctx = BuildContext();
            _ = new DisasterPhaseSystem(ctx);
            var seen = new List<string>();
            ctx.Events.Subscribe<DisasterPhaseChanged>(e => seen.Add(e.To));

            ctx.Clock.FastForward(30);
            Assert.AreEqual("phase_first_rain", ctx.World.CurrentDisasterPhase);

            ctx.Clock.FastForward(50); // now at 80
            Assert.AreEqual("phase_black_rain", ctx.World.CurrentDisasterPhase);

            CollectionAssert.AreEqual(new[] { "phase_first_rain", "phase_black_rain" }, seen);
        }

        [Test]
        public void ThresholdAlreadyPassed_DoesNotRefire()
        {
            var ctx = BuildContext();
            _ = new DisasterPhaseSystem(ctx);
            ctx.Clock.FastForward(30);

            int fireCount = 0;
            ctx.Events.Subscribe<DisasterPhaseChanged>(_ => fireCount++);
            ctx.Clock.FastForward(5); // still before black_rain(80), no transition expected

            Assert.AreEqual(0, fireCount);
        }

        [Test]
        public void Construct_MidTimeline_RecomputesCorrectPhase_AndFutureThresholdStillFires()
        {
            var ctx = BuildContext(startWorldTime: 50); // between first_rain(30) and black_rain(80)
            _ = new DisasterPhaseSystem(ctx);

            Assert.AreEqual("phase_first_rain", ctx.World.CurrentDisasterPhase);

            ctx.Clock.FastForward(30); // crosses 80
            Assert.AreEqual("phase_black_rain", ctx.World.CurrentDisasterPhase);
        }

        [Test]
        public void WorldStateReloaded_ResyncsPhase()
        {
            var ctx = BuildContext();
            _ = new DisasterPhaseSystem(ctx);
            ctx.Clock.FastForward(30);
            Assert.AreEqual("phase_first_rain", ctx.World.CurrentDisasterPhase);

            // Simulate a load that jumped world time backward without reconstructing the system.
            ctx.World.WorldTimeMinutes = 5;
            ctx.World.CurrentDisasterPhase = "phase_first_rain"; // stale, as if copied from a bad save
            ctx.Events.Publish(new WorldStateReloaded());

            Assert.AreEqual("phase_dry", ctx.World.CurrentDisasterPhase);
        }
    }
}
