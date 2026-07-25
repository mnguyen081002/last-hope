using System.IO;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Boot;
using LastHope.Systems.Condition;
using LastHope.Systems.Disaster;
using LastHope.Systems.Events;
using LastHope.Systems.Hazard;
using LastHope.Systems.Intel;
using LastHope.Systems.Npc;
using LastHope.Systems.Shelter;
using LastHope.Systems.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    /// <summary>S17: loads the actual shipped content (not fixtures) and fast-forwards the full
    /// P4 slice timeline end-to-end — the same sanity check every prior sprint's headless smoke
    /// test does at boot, but carried all the way to Aftermath instead of just a few seconds.
    /// Mirrors GameBootstrapper's system construction order (minus TelemetryLogger, which writes
    /// files, and InventorySystem/GameBootstrapper's MonoBehaviour-only wiring, neither of which
    /// this test needs).</summary>
    public class SliceTimelineTests
    {
        private static GameContext BuildRealContext()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Definitions");
            var loadResult = DefinitionLoader.Load(path);
            Assert.IsTrue(loadResult.Success, string.Join("; ", loadResult.Errors));

            var world = new WorldState();
            world.Player.CurrentLocationId = loadResult.Registry.Balance.NewGame.StartLocationId;

            var bus = new EventBus();
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, loadResult.Registry, bus, new RngService(world), scheduler);

            _ = new DisasterPhaseSystem(ctx);
            _ = new ConditionSystem(ctx);
            _ = new HazardSystem(ctx);
            _ = new TaskSystem(ctx);
            _ = new PowerSystem(ctx);
            _ = new WaterIntrusionSystem(ctx);
            _ = new WaterSystem(ctx);
            _ = new EventSystem(ctx);
            _ = new IntelSystem(ctx);
            _ = new NpcSystem(ctx);

            return ctx;
        }

        [Test]
        public void FullSliceTimeline_FastForwards960Minutes_NoException_ReachesFinalPhase()
        {
            var ctx = BuildRealContext();

            Assert.DoesNotThrow(() => ctx.Clock.FastForward(970));

            Assert.AreEqual("phase_p4_end", ctx.World.CurrentDisasterPhase);
        }

        [Test]
        public void FullSliceTimeline_VisitsEveryPhaseInOrder()
        {
            var ctx = BuildRealContext();
            var seen = new System.Collections.Generic.List<string> { ctx.World.CurrentDisasterPhase };
            ctx.Events.Subscribe<DisasterPhaseChanged>(e => seen.Add(e.To));

            ctx.Clock.FastForward(970);

            CollectionAssert.AreEqual(
                new[] { "phase_p4_normal", "phase_p4_warning", "phase_p4_first_rain", "phase_p4_black_rain",
                         "phase_p4_escalation", "phase_p4_peak", "phase_p4_aftermath", "phase_p4_end" },
                seen);
        }

        [Test]
        public void FullSliceTimeline_GridFailureEvent_SetsGridDownFlag()
        {
            var ctx = BuildRealContext();

            ctx.Clock.FastForward(970);

            Assert.IsTrue(ctx.World.PersistentFlags.TryGetValue("grid_down", out bool down) && down);
        }
    }
}
