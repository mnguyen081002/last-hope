using System.Collections.Generic;
using LastHope.Core.Rules;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class HazardRulesTests
    {
        [Test]
        public void ComputeLevel_ConstantBand_IgnoresProgress()
        {
            Assert.AreEqual(2, HazardRules.ComputeLevel(2, 2, 0f, 0));
            Assert.AreEqual(2, HazardRules.ComputeLevel(2, 2, 1f, 0));
        }

        [Test]
        public void ComputeLevel_LerpsAndRounds()
        {
            Assert.AreEqual(0, HazardRules.ComputeLevel(0, 4, 0f, 0));
            Assert.AreEqual(2, HazardRules.ComputeLevel(0, 4, 0.5f, 0));
            Assert.AreEqual(4, HazardRules.ComputeLevel(0, 4, 1f, 0));
        }

        [Test]
        public void ComputeLevel_BaseElevation_ReducesLevel_ClampsAtZero()
        {
            Assert.AreEqual(1, HazardRules.ComputeLevel(0, 4, 0.5f, 1)); // 2 - 1 = 1
            Assert.AreEqual(0, HazardRules.ComputeLevel(0, 4, 0.25f, 5)); // 1 - 5 clamps to 0
        }

        [Test]
        public void ComputeLevel_ClampsAtMaxLevel()
        {
            Assert.AreEqual(HazardRules.MaxLevel, HazardRules.ComputeLevel(0, 10, 1f, 0));
        }

        private static RouteDefinition Route(int baseElevation = 0) =>
            new RouteDefinition { Id = "r", FromLocationId = "a", ToLocationId = "b", TravelMinutes = 20, BaseElevationLevel = baseElevation };

        private static List<DisasterPhaseDefinition> Phases() => new List<DisasterPhaseDefinition>
        {
            new DisasterPhaseDefinition { Id = "dry", StartMinute = 0, FloodBandMin = 0, FloodBandMax = 0, CurrentBandMin = 0, CurrentBandMax = 0 },
            new DisasterPhaseDefinition { Id = "storm", StartMinute = 100, FloodBandMin = 0, FloodBandMax = 4, CurrentBandMin = 0, CurrentBandMax = 2 },
        };

        [Test]
        public void EvaluateRoute_EmptyPhases_ReturnsZero()
        {
            var levels = HazardRules.EvaluateRoute(Route(), new List<DisasterPhaseDefinition>(), 50);
            Assert.AreEqual(0, levels.FloodLevel);
            Assert.AreEqual(0, levels.CurrentLevel);
        }

        [Test]
        public void EvaluateRoute_BeforeNextPhase_UsesCurrentPhaseProgress()
        {
            var phases = Phases();
            // At minute 50, halfway between dry(0) and storm(100): progress 0.5 into "dry" band (0-0) -> still 0
            var levels = HazardRules.EvaluateRoute(Route(), phases, 50);
            Assert.AreEqual(0, levels.FloodLevel);
        }

        [Test]
        public void EvaluateRoute_LastPhase_ProgressIsFullyOne()
        {
            var phases = Phases();
            var levels = HazardRules.EvaluateRoute(Route(), phases, 100);
            Assert.AreEqual(HazardRules.MaxLevel, levels.FloodLevel); // storm band 0-4, progress=1 (no next phase) -> 4
        }
    }
}
