using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class WaterIntrusionSystemTests
    {
        private static Dictionary<string, DisasterPhaseDefinition> BuildPhases() => new Dictionary<string, DisasterPhaseDefinition>
        {
            ["phase_dry"] = new DisasterPhaseDefinition { Id = "phase_dry", StartMinute = 0, RainIntensity = 0 },
            ["phase_black_rain"] = new DisasterPhaseDefinition { Id = "phase_black_rain", StartMinute = 80, RainIntensity = 2 },
        };

        private static Dictionary<string, ShelterZoneDefinition> BuildZones() => new Dictionary<string, ShelterZoneDefinition>
        {
            ["utility_area"] = new ShelterZoneDefinition
            {
                Id = "utility_area",
                Floor = "Ground",
                BuildSlotIds = new List<string> { "slot_utility_area_1", "slot_utility_area_2" },
            },
        };

        private static GameContext BuildContext()
        {
            var world = new WorldState();
            var bus = new EventBus();
            var registry = new DefinitionRegistry(
                "test", new BalanceConfig(),
                new Dictionary<string, ItemDefinition>(),
                new Dictionary<string, LocationDefinition>(),
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>(),
                BuildPhases(),
                BuildZones());
            var scheduler = new TickScheduler(world, bus);
            return new GameContext(world, registry, bus, new RngService(world), scheduler);
        }

        [Test]
        public void Construct_SeedsShelter_FromBalanceDefaults()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);

            var shelter = ctx.World.ShelterStates["shelter_main"];
            Assert.AreEqual(85f, shelter.StructuralIntegrity);
            Assert.AreEqual(2, shelter.LivingCapacity);
            Assert.AreEqual(3f, shelter.WaterStocks.Clean);
            Assert.AreEqual(WaterIntrusionLevel.Dry, shelter.WaterIntrusion.Level);
        }

        [Test]
        public void Construct_PopulatesBuildSlots_FromShelterZones()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);

            var shelter = ctx.World.ShelterStates["shelter_main"];
            Assert.IsTrue(shelter.BuildSlots.ContainsKey("slot_utility_area_1"));
            Assert.IsTrue(shelter.BuildSlots.ContainsKey("slot_utility_area_2"));
        }

        [Test]
        public void LongTick_NoRain_UnitsStayAtZero_NoEventPublished()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);
            int changeCount = 0;
            ctx.Events.Subscribe<ShelterWaterChanged>(_ => changeCount++);

            ctx.Clock.FastForward(10);

            Assert.AreEqual(0f, ctx.World.ShelterStates["shelter_main"].WaterIntrusion.Units);
            Assert.AreEqual(0, changeCount);
        }

        [Test]
        public void LongTick_Rain_AccumulatesUnits_CrossesDampThreshold_PublishesOnce()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);
            ctx.World.CurrentDisasterPhase = "phase_black_rain"; // RainIntensity 2 -> +4/long-tick, -2 drain = +2 net
            var seen = new List<WaterIntrusionLevel>();
            ctx.Events.Subscribe<ShelterWaterChanged>(e => seen.Add(e.Level));

            ctx.Clock.FastForward(10); // units = 2, still Dry
            Assert.AreEqual(WaterIntrusionLevel.Dry, ctx.World.ShelterStates["shelter_main"].WaterIntrusion.Level);

            ctx.Clock.FastForward(40); // +2/tick * 4 ticks = +8 more => units=10 -> Damp
            Assert.AreEqual(WaterIntrusionLevel.Damp, ctx.World.ShelterStates["shelter_main"].WaterIntrusion.Level);
            CollectionAssert.AreEqual(new[] { WaterIntrusionLevel.Damp }, seen);
        }

        [Test]
        public void LongTick_ReachesDeep_SetsFlag_ClearsWhenRecovered()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 58f; // just below Deep(60)
            ctx.World.CurrentDisasterPhase = "phase_black_rain"; // net +2/long-tick

            ctx.Clock.FastForward(10); // 58 -> 60 -> Deep
            Assert.AreEqual(WaterIntrusionLevel.Deep, shelter.WaterIntrusion.Level);
            Assert.IsTrue(shelter.EventFlags.Contains(ShelterEventFlags.LowerFloorPowerLocked));

            ctx.World.CurrentDisasterPhase = "phase_dry"; // net -2/long-tick (passive drain only)
            ctx.Clock.FastForward(10); // 60 -> 58 -> back to Shallow
            Assert.AreEqual(WaterIntrusionLevel.Shallow, shelter.WaterIntrusion.Level);
            Assert.IsFalse(shelter.EventFlags.Contains(ShelterEventFlags.LowerFloorPowerLocked));
        }

        [Test]
        public void WorldStateReloaded_DoesNotReseedAlreadySeededShelter()
        {
            var ctx = BuildContext();
            _ = new WaterIntrusionSystem(ctx);
            var shelter = ctx.World.ShelterStates["shelter_main"];
            shelter.WaterIntrusion.Units = 42f;

            ctx.Events.Publish(new WorldStateReloaded());

            Assert.AreEqual(42f, shelter.WaterIntrusion.Units);
            Assert.AreEqual(85f, shelter.StructuralIntegrity); // unchanged, not reset
        }
    }
}
