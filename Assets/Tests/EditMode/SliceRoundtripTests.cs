using System;
using System.Collections.Generic;
using System.IO;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Save;
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
using LastHope.Systems.Outcome;
using LastHope.Systems.Shelter;
using LastHope.Systems.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    /// <summary>S18 Gate P4 check: save mid-slice on real content, load into a fresh context, keep
    /// simulating, and confirm the run still reaches an Outcome with no exception — the save/load
    /// boundary can't silently break a run in progress.</summary>
    public class SliceRoundtripTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp() => _tempDir = Path.Combine(Path.GetTempPath(), "LastHopeSliceRoundtrip_" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        }

        private static DefinitionRegistry LoadRealRegistry()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Definitions");
            var result = DefinitionLoader.Load(path);
            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            return result.Registry;
        }

        private static GameContext BuildContext(WorldState world, DefinitionRegistry registry)
        {
            var bus = new EventBus();
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);

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
            _ = new OutcomeSystem(ctx);

            return ctx;
        }

        [Test]
        public void SaveLoad_MidTimeline_ContinuesToOutcome_NoException()
        {
            var registry = LoadRealRegistry();
            var world = new WorldState();
            world.Player.CurrentLocationId = registry.Balance.NewGame.StartLocationId;
            var ctx = BuildContext(world, registry);

            ctx.Clock.FastForward(500); // partway through — past FirstRain, into BlackRain

            var saveService = new SaveService(_tempDir, registry.DefinitionVersion);
            var saveResult = saveService.SaveToSlot(world, "manual_0");
            Assert.IsTrue(saveResult.Success, saveResult.Error);

            var loadResult = saveService.Load("manual_0");
            Assert.IsTrue(loadResult.Success, loadResult.Error);

            var reloadedCtx = BuildContext(loadResult.World, registry);
            reloadedCtx.Events.Publish(new WorldStateReloaded());

            Outcome? reached = null;
            reloadedCtx.Events.Subscribe<OutcomeReached>(e => reached = e.Outcome);

            Assert.DoesNotThrow(() => reloadedCtx.Clock.FastForward(470)); // finish the 970' timeline

            Assert.IsTrue(reached.HasValue);
        }

        [Test]
        public void SaveLoad_MidEvent_PreservesActiveEventState()
        {
            var registry = LoadRealRegistry();
            var world = new WorldState();
            world.Player.CurrentLocationId = registry.Balance.NewGame.StartLocationId;
            var ctx = BuildContext(world, registry);

            // Storm Warning triggers at phase_p4_warning (minute 120) with no discovery gate.
            ctx.Clock.FastForward(130);
            var before = world.ActiveEvents.Find(e => e.EventId == "event_storm_warning");
            Assert.IsNotNull(before, "expected event_storm_warning to have triggered by minute 130");

            var saveService = new SaveService(_tempDir, registry.DefinitionVersion);
            saveService.SaveToSlot(world, "manual_0");
            var loadResult = saveService.Load("manual_0");
            Assert.IsTrue(loadResult.Success, loadResult.Error);

            var after = loadResult.World.ActiveEvents.Find(e => e.EventId == "event_storm_warning");
            Assert.IsNotNull(after);
            Assert.AreEqual(before.State, after.State);
            Assert.AreEqual(before.EventInstanceId, after.EventInstanceId);
            Assert.AreEqual(before.TriggeredAtMinute, after.TriggeredAtMinute);
        }

        /// <summary>Gate P4 check (plan §"Gate P4"): confirm the 3 named outcomes are each
        /// reachable and distinct — no strategy softlocks. Jumps straight to the final phase
        /// (rather than fast-forwarding the full natural timeline) and wires only OutcomeSystem —
        /// this test is about OutcomeRules' branching, not about whether a pump keeps the shelter
        /// dry for 970 minutes (that's WaterIntrusionRulesTests' job). Real end-to-end natural
        /// timelines are already covered by the two tests above.</summary>
        [Test]
        public void ThreeDifferentStrategies_ReachDifferentOutcomes_NoSoftlock()
        {
            var doNothing = RunScenarioAtFinalPhase(ctx => SeedShelter(ctx, groundFloorLost: false, cleanWater: 5f));
            Assert.AreEqual(Outcome.StableSurvival, doNothing);

            var neverEvacuates = RunScenarioAtFinalPhase(ctx => SeedShelter(ctx, groundFloorLost: true, cleanWater: 0f));
            Assert.AreEqual(Outcome.Collapse, neverEvacuates);

            var evacuates = RunScenarioAtFinalPhase(ctx =>
            {
                SeedShelter(ctx, groundFloorLost: true, cleanWater: 0f);
                ctx.World.PersistentFlags["evacuated"] = true;
                ctx.World.Player.CurrentLocationId = ctx.Definitions.Balance.Slice.EvacuationLocationId;
            });
            Assert.AreEqual(Outcome.ForcedEvacuation, evacuates);

            var outcomes = new HashSet<Outcome> { doNothing, neverEvacuates, evacuates };
            Assert.AreEqual(3, outcomes.Count, "the 3 strategies should not collapse onto the same ending");
        }

        private static void SeedShelter(GameContext ctx, bool groundFloorLost, float cleanWater)
        {
            string shelterId = ctx.Definitions.Balance.NewGame.MainShelterId;
            var shelter = new ShelterState
            {
                Id = shelterId,
                WaterStocks = new WaterStocksState { Clean = cleanWater },
            };
            if (groundFloorLost) shelter.EventFlags.Add(ShelterEventFlags.GroundFloorLost);
            ctx.World.ShelterStates[shelterId] = shelter;
        }

        private static Outcome RunScenarioAtFinalPhase(Action<GameContext> seed)
        {
            var registry = LoadRealRegistry();
            var world = new WorldState();
            world.Player.CurrentLocationId = registry.Balance.NewGame.StartLocationId;

            var lastPhase = registry.DisasterPhasesSorted[registry.DisasterPhasesSorted.Count - 1];
            world.WorldTimeMinutes = lastPhase.StartMinute;
            world.CurrentDisasterPhase = lastPhase.Id;

            var bus = new EventBus();
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new OutcomeSystem(ctx);

            seed(ctx);

            Outcome? reached = null;
            ctx.Events.Subscribe<OutcomeReached>(e => reached = e.Outcome);

            Assert.DoesNotThrow(() => ctx.Clock.FastForward(10));

            Assert.IsTrue(reached.HasValue, "expected an outcome to be reached once the final phase is active");
            return reached.Value;
        }
    }
}
