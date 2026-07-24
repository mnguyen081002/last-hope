using System.Collections.Generic;
using LastHope.Core.Rules;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class ReturnWindowCalculatorTests
    {
        private static RouteDefinition Route() =>
            new RouteDefinition { Id = "r", FromLocationId = "a", ToLocationId = "b", TravelMinutes = 20 };

        [Test]
        public void RisingFloodBand_ReportsMinutesUntilWorseAndImpassable()
        {
            var phases = new List<DisasterPhaseDefinition>
            {
                new DisasterPhaseDefinition { Id = "dry", StartMinute = 0, FloodBandMin = 0, FloodBandMax = 0 },
                new DisasterPhaseDefinition { Id = "storm", StartMinute = 100, FloodBandMin = 0, FloodBandMax = 4 },
            };

            var window = ReturnWindowCalculator.Evaluate(Route(), phases, fromMinute: 90);

            // baseline at minute 90 is still tier 0 (dry phase, band 0-0); storm starts at 100 (10 minutes later)
            // and immediately begins climbing toward level 4 as progress advances from 0.
            Assert.IsTrue(window.MinutesUntilWorse.HasValue);
            Assert.AreEqual(10, window.MinutesUntilWorse.Value);
            Assert.IsTrue(window.MinutesUntilImpassable.HasValue);
        }

        [Test]
        public void FlatBand_NeverWorsens_BothNull()
        {
            var phases = new List<DisasterPhaseDefinition>
            {
                new DisasterPhaseDefinition { Id = "dry", StartMinute = 0, FloodBandMin = 0, FloodBandMax = 0 },
            };

            var window = ReturnWindowCalculator.Evaluate(Route(), phases, fromMinute: 0);

            Assert.IsFalse(window.MinutesUntilWorse.HasValue);
            Assert.IsFalse(window.MinutesUntilImpassable.HasValue);
        }

        [Test]
        public void AlreadyImpassableAtBaseline_WorseNeverTriggers_ImpassableStillReportedAtFirstSample()
        {
            var phases = new List<DisasterPhaseDefinition>
            {
                new DisasterPhaseDefinition { Id = "storm", StartMinute = 0, FloodBandMin = 4, FloodBandMax = 4 },
            };

            var window = ReturnWindowCalculator.Evaluate(Route(), phases, fromMinute: 0);

            // Baseline tier is already MaxLevel, so it can never register as "worse" (nothing is
            // higher than MaxLevel). MinutesUntilImpassable has no such guard — it just reports
            // the first future sample that is still at/above MaxLevel, which is immediately true
            // here. The UI's own "IMPASSABLE" label (not this calculator) covers "already blocked".
            Assert.IsFalse(window.MinutesUntilWorse.HasValue);
            Assert.IsTrue(window.MinutesUntilImpassable.HasValue);
            Assert.AreEqual(10, window.MinutesUntilImpassable.Value);
        }
    }
}
