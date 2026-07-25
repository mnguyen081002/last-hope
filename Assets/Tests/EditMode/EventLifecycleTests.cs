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
    /// <summary>S14: full event lifecycle — discovery, soft/hard deadlines, expiration effects,
    /// chains, off-scene behavior, and event-priority sleep interrupts.</summary>
    public class EventLifecycleTests
    {
        private const string Shelter = "location_shelter";
        private const string Away = "location_away";
        private const string Store = "location_store";

        private static GameContext BuildContext(Dictionary<string, EventDefinition> events, string playerAt = Shelter)
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = playerAt;
            var bus = new EventBus();
            var locations = new Dictionary<string, LocationDefinition>
            {
                [Shelter] = new LocationDefinition { Id = Shelter, IsShelter = true },
                [Away] = new LocationDefinition { Id = Away },
                [Store] = new LocationDefinition { Id = Store },
            };
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(), new Dictionary<string, ItemDefinition>(),
                locations, new Dictionary<string, RouteDefinition>(), new Dictionary<string, SearchPointDefinition>(),
                events: events);
            var scheduler = new TickScheduler(world, bus);
            var ctx = new GameContext(world, registry, bus, new RngService(world), scheduler);
            _ = new WaterIntrusionSystem(ctx); // seeds the shelter state EventSystem requires
            _ = new EventSystem(ctx);
            return ctx;
        }

        /// <summary>chance 100 = triggers on the first long-tick, no other conditions.</summary>
        private static EventDefinition AutoEvent(string id) => new EventDefinition
        {
            Id = id,
            Priority = "Standard",
            TriggerChancePercentPerLongTick = 100,
            AvailableResponses = new List<string> { "ack" },
        };

        // --- Discovery ---

        [Test]
        public void RequiresDiscovery_TriggersUndiscovered_NoEventTriggeredPublished_DeadlinesUnarmed()
        {
            var def = AutoEvent("event_hidden");
            def.RequiresDiscovery = true;
            def.HardDeadlineMinutes = 60;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Away);
            int triggeredCount = 0;
            ctx.Events.Subscribe<EventTriggered>(_ => triggeredCount++);

            ctx.Clock.FastForward(10);

            var instance = ctx.World.ActiveEvents[0];
            Assert.AreEqual(EventLifecycleState.Undiscovered, instance.State);
            Assert.IsNull(instance.DeadlineMinute);
            Assert.AreEqual(0, triggeredCount);
        }

        [Test]
        public void Undiscovered_BecomesActive_WhenPlayerAtShelter_DeadlinesArmFromDiscovery()
        {
            var def = AutoEvent("event_hidden");
            def.RequiresDiscovery = true;
            def.HardDeadlineMinutes = 60;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Away);
            string discovered = null;
            ctx.Events.Subscribe<EventDiscovered>(e => discovered = e.EventId);

            ctx.Clock.FastForward(10); // triggers Undiscovered at minute 10
            ctx.World.Player.CurrentLocationId = Shelter;
            ctx.Clock.FastForward(10); // discovery at minute 20

            var instance = ctx.World.ActiveEvents[0];
            Assert.AreEqual(EventLifecycleState.Active, instance.State);
            Assert.AreEqual("event_hidden", discovered);
            Assert.AreEqual(20 + 60, instance.DeadlineMinute);
        }

        [Test]
        public void Undiscovered_StaysUndiscovered_WhilePlayerElsewhere()
        {
            var def = AutoEvent("event_hidden");
            def.RequiresDiscovery = true;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Away);

            ctx.Clock.FastForward(50);

            Assert.AreEqual(EventLifecycleState.Undiscovered, ctx.World.ActiveEvents[0].State);
        }

        [Test]
        public void Resolve_Undiscovered_FailsEventNotDiscovered()
        {
            var def = AutoEvent("event_hidden");
            def.RequiresDiscovery = true;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Away);
            ctx.Clock.FastForward(10);

            var result = new CommandProcessor(ctx).Submit(
                new ResolveEventCommand("player", ctx.World.ActiveEvents[0].EventInstanceId, "ack"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.EventNotDiscovered, result.Code);
        }

        // --- Deadlines ---

        [Test]
        public void HardDeadline_ExpiresUnresolved_PublishesEventExpired()
        {
            var def = AutoEvent("event_timed");
            def.HardDeadlineMinutes = 30;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });
            string expired = null;
            ctx.Events.Subscribe<EventExpired>(e => expired = e.EventId);

            ctx.Clock.FastForward(10); // triggers at 10, deadline 40
            ctx.Clock.FastForward(30); // minute 40 long-tick expires it

            Assert.AreEqual(EventLifecycleState.Expired, ctx.World.ActiveEvents[0].State);
            Assert.AreEqual("event_timed", expired);
        }

        [Test]
        public void SoftDeadline_PublishesDeadlineApproaching_ExactlyOnce()
        {
            var def = AutoEvent("event_timed");
            def.SoftDeadlineMinutes = 20;
            def.HardDeadlineMinutes = 60;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });
            var approaching = new List<long>();
            ctx.Events.Subscribe<EventDeadlineApproaching>(e => approaching.Add(e.HardDeadlineMinute));

            ctx.Clock.FastForward(60); // trigger at 10, soft at 30, still before hard (70)

            Assert.AreEqual(1, approaching.Count);
            Assert.AreEqual(70, approaching[0]); // carries the hard deadline for countdown UI
            Assert.IsTrue(ctx.World.ActiveEvents[0].SoftDeadlineNotified);
            Assert.AreEqual(EventLifecycleState.Active, ctx.World.ActiveEvents[0].State);
        }

        [Test]
        public void Expiration_AppliesFlags_EndsAsPersistentConsequence()
        {
            var def = AutoEvent("event_flagged");
            def.HardDeadlineMinutes = 30;
            def.ExpirationShelterFlags = new List<string> { "storage_soaked" };
            def.ExpirationPersistentFlags = new List<string> { "district_alert" };
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            ctx.Clock.FastForward(40);

            Assert.AreEqual(EventLifecycleState.PersistentConsequence, ctx.World.ActiveEvents[0].State);
            Assert.IsTrue(ctx.World.ShelterStates["shelter_main"].EventFlags.Contains("storage_soaked"));
            Assert.IsTrue(ctx.World.PersistentFlags["district_alert"]);
        }

        [Test]
        public void Expired_DoesNotRetrigger()
        {
            var def = AutoEvent("event_timed");
            def.HardDeadlineMinutes = 30;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            ctx.Clock.FastForward(40); // trigger + expire
            ctx.Clock.FastForward(50); // conditions (chance 100) would hold every tick

            Assert.AreEqual(1, ctx.World.ActiveEvents.Count);
        }

        // --- Chains ---

        private static Dictionary<string, EventDefinition> ChainPair(int hardDeadlineA)
        {
            var a = AutoEvent("event_a");
            a.HardDeadlineMinutes = hardDeadlineA;
            a.NextEventId = "event_b";
            var b = new EventDefinition
            {
                Id = "event_b",
                Priority = "Standard",
                TriggerPhaseId = "phase_that_never_exists", // unreachable naturally — chain-only
                AvailableResponses = new List<string> { "ack" },
            };
            return new Dictionary<string, EventDefinition> { [a.Id] = a, [b.Id] = b };
        }

        [Test]
        public void Chain_OnResolve_ForceTriggersNextEvent()
        {
            var ctx = BuildContext(ChainPair(hardDeadlineA: 0));
            ctx.Clock.FastForward(10);
            string instanceA = ctx.World.ActiveEvents[0].EventInstanceId;

            new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", instanceA, "ack"));

            var b = ctx.World.ActiveEvents.Find(e => e.EventId == "event_b");
            Assert.IsNotNull(b);
            Assert.AreEqual(EventLifecycleState.Active, b.State);
        }

        [Test]
        public void Chain_OnExpire_ForceTriggersNextEvent()
        {
            var ctx = BuildContext(ChainPair(hardDeadlineA: 30));

            ctx.Clock.FastForward(40); // A triggers at 10, expires at 40

            var b = ctx.World.ActiveEvents.Find(e => e.EventId == "event_b");
            Assert.IsNotNull(b);
            Assert.AreEqual(EventLifecycleState.Active, b.State);
        }

        // --- Off-scene ---

        [Test]
        public void OffScene_EventTriggersAndExpires_WhilePlayerElsewhere()
        {
            var def = AutoEvent("event_timed");
            def.HardDeadlineMinutes = 30;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Away);

            ctx.Clock.FastForward(50);

            Assert.AreEqual(EventLifecycleState.Expired, ctx.World.ActiveEvents[0].State);
        }

        // --- Sleep interrupts (event-system-design.md §14) ---

        private static CommandResult Sleep(GameContext ctx, int minutes, out bool interrupted, out bool ended)
        {
            bool wasInterrupted = false, wasEnded = false;
            ctx.Events.Subscribe<SleepInterrupted>(_ => wasInterrupted = true);
            ctx.Events.Subscribe<SleepEnded>(_ => wasEnded = true);
            var result = new CommandProcessor(ctx).Submit(new StartSleepCommand("player", minutes));
            interrupted = wasInterrupted;
            ended = wasEnded;
            return result;
        }

        [Test]
        public void Sleep_WokenByCriticalEvent()
        {
            var def = AutoEvent("event_critical");
            def.Priority = "Critical";
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            var result = Sleep(ctx, 480, out bool interrupted, out bool ended);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(interrupted);
            Assert.IsFalse(ended);
            Assert.AreEqual(10, ctx.World.WorldTimeMinutes); // event triggers on the first long-tick
        }

        [Test]
        public void Sleep_WokenByMajorEvent_AtShelter()
        {
            var def = AutoEvent("event_major");
            def.Priority = "Major";
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            Sleep(ctx, 480, out bool interrupted, out _);

            Assert.IsTrue(interrupted);
            Assert.AreEqual(10, ctx.World.WorldTimeMinutes);
        }

        [Test]
        public void Sleep_NotWokenByStandardEvent()
        {
            var def = AutoEvent("event_standard"); // Priority "Standard" from the fixture
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            Sleep(ctx, 480, out bool interrupted, out bool ended);

            Assert.IsFalse(interrupted);
            Assert.IsTrue(ended);
            Assert.AreEqual(480, ctx.World.WorldTimeMinutes);
        }

        [Test]
        public void Sleep_WokenByUndiscoveredCriticalEvent()
        {
            var def = AutoEvent("event_hidden_critical");
            def.Priority = "Critical";
            def.RequiresDiscovery = true;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def });

            Sleep(ctx, 480, out bool interrupted, out _);

            Assert.IsTrue(interrupted);
        }

        // --- S16: custom discovery location + per-response branching chain ---

        [Test]
        public void RequiresDiscovery_WithDiscoveryLocationId_IgnoresShelter_RequiresExactLocation()
        {
            var def = AutoEvent("event_at_store");
            def.RequiresDiscovery = true;
            def.DiscoveryLocationId = Store;
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def }, playerAt: Shelter);

            ctx.Clock.FastForward(10); // triggers Undiscovered; player at Shelter, not Store
            Assert.AreEqual(EventLifecycleState.Undiscovered, ctx.World.ActiveEvents[0].State);

            ctx.World.Player.CurrentLocationId = Store;
            ctx.Clock.FastForward(10);

            Assert.AreEqual(EventLifecycleState.Active, ctx.World.ActiveEvents[0].State);
        }

        private static Dictionary<string, EventDefinition> BranchingPair(string responseThatChains, string chainTarget)
        {
            var a = AutoEvent("event_a");
            a.HardDeadlineMinutes = 0;
            a.AvailableResponses = new List<string> { responseThatChains };
            a.NextEventId = "event_fallback"; // should be ignored when the response has a specific entry
            a.NextEventIdByResponse = new Dictionary<string, string> { [responseThatChains] = chainTarget };
            // TriggerPhaseId pointing at a phase that never exists = never triggers on its own
            // (no phase definitions are registered in this test context) — these only ever appear
            // via the chain, same pattern as EventCommandsTests' ChainPair.
            var b = new EventDefinition { Id = chainTarget, Priority = "Standard", TriggerPhaseId = "phase_never", AvailableResponses = new List<string> { "ack" } };
            var fallback = new EventDefinition { Id = "event_fallback", Priority = "Standard", TriggerPhaseId = "phase_never", AvailableResponses = new List<string> { "ack" } };
            return new Dictionary<string, EventDefinition> { [a.Id] = a, [b.Id] = b, [fallback.Id] = fallback };
        }

        [Test]
        public void Chain_ByResponse_OverridesFlatNextEventId()
        {
            var events = BranchingPair("help", "event_helped");
            var ctx = BuildContext(events);
            ctx.Clock.FastForward(10);
            string instanceA = ctx.World.ActiveEvents[0].EventInstanceId;

            new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", instanceA, "help"));

            Assert.IsNotNull(ctx.World.ActiveEvents.Find(e => e.EventId == "event_helped"));
            Assert.IsNull(ctx.World.ActiveEvents.Find(e => e.EventId == "event_fallback"));
        }

        [Test]
        public void Chain_ResponseWithoutSpecificEntry_FallsBackToNextEventId()
        {
            var def = AutoEvent("event_a");
            def.HardDeadlineMinutes = 0;
            def.AvailableResponses = new List<string> { "other" };
            def.NextEventId = "event_fallback";
            def.NextEventIdByResponse = new Dictionary<string, string> { ["help"] = "event_helped" };
            var fallback = new EventDefinition { Id = "event_fallback", Priority = "Standard", TriggerPhaseId = "phase_never", AvailableResponses = new List<string> { "ack" } };
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def, ["event_fallback"] = fallback });
            ctx.Clock.FastForward(10);
            string instanceA = ctx.World.ActiveEvents[0].EventInstanceId;

            new CommandProcessor(ctx).Submit(new ResolveEventCommand("player", instanceA, "other"));

            Assert.IsNotNull(ctx.World.ActiveEvents.Find(e => e.EventId == "event_fallback"));
        }

        [Test]
        public void Chain_OnExpire_AlwaysUsesFlatNextEventId_NeverByResponse()
        {
            var def = AutoEvent("event_a");
            def.HardDeadlineMinutes = 20;
            def.NextEventId = "event_fallback";
            def.NextEventIdByResponse = new Dictionary<string, string> { ["help"] = "event_helped" };
            var fallback = new EventDefinition { Id = "event_fallback", Priority = "Standard", TriggerPhaseId = "phase_never", AvailableResponses = new List<string> { "ack" } };
            var ctx = BuildContext(new Dictionary<string, EventDefinition> { [def.Id] = def, ["event_fallback"] = fallback });

            ctx.Clock.FastForward(40); // trigger at 10, expire at 30

            Assert.IsNotNull(ctx.World.ActiveEvents.Find(e => e.EventId == "event_fallback"));
        }
    }
}
