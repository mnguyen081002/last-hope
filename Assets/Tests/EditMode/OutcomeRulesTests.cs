using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class OutcomeRulesTests
    {
        private static SliceBalance Cfg() => new SliceBalance
        {
            EvacuationLocationId = "location_school",
            MinCleanWaterForStableSurvival = 1f,
        };

        private static WorldState World() => new WorldState();

        private static ShelterState Shelter(bool groundFloorLost, float cleanWater) => new ShelterState
        {
            Id = "shelter_main",
            WaterStocks = new WaterStocksState { Clean = cleanWater },
            EventFlags = groundFloorLost ? new System.Collections.Generic.HashSet<string> { ShelterEventFlags.GroundFloorLost } : new System.Collections.Generic.HashSet<string>(),
        };

        [Test]
        public void Incapacitated_AlwaysCollapse_RegardlessOfShelterState()
        {
            var world = World();
            world.Player.Condition.Incapacitation = IncapacitationState.Collapsed;
            var shelter = Shelter(groundFloorLost: false, cleanWater: 100f);

            Assert.AreEqual(Outcome.Collapse, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterIntact_EnoughWater_StableSurvival()
        {
            var world = World();
            var shelter = Shelter(groundFloorLost: false, cleanWater: 5f);

            Assert.AreEqual(Outcome.StableSurvival, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterIntact_ExactlyAtMinimum_StableSurvival()
        {
            var world = World();
            var shelter = Shelter(groundFloorLost: false, cleanWater: 1f);

            Assert.AreEqual(Outcome.StableSurvival, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterIntact_BelowMinimumWater_Collapse()
        {
            var world = World();
            var shelter = Shelter(groundFloorLost: false, cleanWater: 0.5f);

            Assert.AreEqual(Outcome.Collapse, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterLost_EvacuatedAndAtSchool_ForcedEvacuation()
        {
            var world = World();
            world.PersistentFlags["evacuated"] = true;
            world.Player.CurrentLocationId = "location_school";
            var shelter = Shelter(groundFloorLost: true, cleanWater: 0f);

            Assert.AreEqual(Outcome.ForcedEvacuation, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterLost_EvacuatedButNotYetAtSchool_Collapse()
        {
            var world = World();
            world.PersistentFlags["evacuated"] = true;
            world.Player.CurrentLocationId = "location_shelter"; // still traveling
            var shelter = Shelter(groundFloorLost: true, cleanWater: 0f);

            Assert.AreEqual(Outcome.Collapse, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterLost_NeverEvacuated_Collapse()
        {
            var world = World();
            world.Player.CurrentLocationId = "location_school"; // being there isn't enough without the flag
            var shelter = Shelter(groundFloorLost: true, cleanWater: 0f);

            Assert.AreEqual(Outcome.Collapse, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }

        [Test]
        public void ShelterLost_TakesPriorityOverWaterStock()
        {
            var world = World();
            world.PersistentFlags["evacuated"] = true;
            world.Player.CurrentLocationId = "location_school";
            var shelter = Shelter(groundFloorLost: true, cleanWater: 999f); // plenty of water doesn't matter once lost

            Assert.AreEqual(Outcome.ForcedEvacuation, OutcomeRules.Evaluate(world, shelter, Cfg()));
        }
    }
}
