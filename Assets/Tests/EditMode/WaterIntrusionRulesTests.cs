using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class WaterIntrusionRulesTests
    {
        private static ShelterBalance Cfg() => new ShelterBalance
        {
            DampThreshold = 10f,
            ShallowThreshold = 30f,
            DeepThreshold = 60f,
            CriticalThreshold = 85f,
            InflowByRainIntensity = new[] { 0f, 2f, 4f, 6f },
            BackflowInflow = 6f,
            PassiveDrainPerLongTick = 2f,
            PumpOutputPerLongTick = 6f,
        };

        [Test]
        public void ComputeDelta_NoRain_OnlyPassiveDrain()
        {
            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity: 0, backflowActive: false, activePumpCount: 0, Cfg());
            Assert.AreEqual(-2f, delta); // 0 inflow - 2 passive drain
        }

        [Test]
        public void ComputeDelta_RainIntensity_ReadsTableByIndex()
        {
            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity: 2, backflowActive: false, activePumpCount: 0, Cfg());
            Assert.AreEqual(2f, delta); // 4 inflow - 2 passive drain
        }

        [Test]
        public void ComputeDelta_RainIntensityBeyondTable_ClampsToLastEntry()
        {
            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity: 99, backflowActive: false, activePumpCount: 0, Cfg());
            Assert.AreEqual(4f, delta); // last entry (6) - 2 passive drain
        }

        [Test]
        public void ComputeDelta_Backflow_AddsInflow_AndZeroesPassiveDrain()
        {
            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity: 0, backflowActive: true, activePumpCount: 0, Cfg());
            Assert.AreEqual(6f, delta); // 0 + 6 backflow - 0 passive drain (suppressed during backflow)
        }

        [Test]
        public void ComputeDelta_ActivePump_SubtractsPumpOutput()
        {
            float delta = WaterIntrusionRules.ComputeDelta(rainIntensity: 3, backflowActive: false, activePumpCount: 1, Cfg());
            Assert.AreEqual(-2f, delta); // 6 inflow - 2 passive drain - 6 pump
        }

        [TestCase(0f, WaterIntrusionLevel.Dry)]
        [TestCase(9.9f, WaterIntrusionLevel.Dry)]
        [TestCase(10f, WaterIntrusionLevel.Damp)]
        [TestCase(29.9f, WaterIntrusionLevel.Damp)]
        [TestCase(30f, WaterIntrusionLevel.Shallow)]
        [TestCase(59.9f, WaterIntrusionLevel.Shallow)]
        [TestCase(60f, WaterIntrusionLevel.Deep)]
        [TestCase(84.9f, WaterIntrusionLevel.Deep)]
        [TestCase(85f, WaterIntrusionLevel.Critical)]
        [TestCase(100f, WaterIntrusionLevel.Critical)]
        public void LevelFor_MatchesThresholdBands(float units, WaterIntrusionLevel expected)
        {
            Assert.AreEqual(expected, WaterIntrusionRules.LevelFor(units, Cfg()));
        }

        [Test]
        public void Clamp01To100_ClampsBothEnds()
        {
            Assert.AreEqual(0f, WaterIntrusionRules.Clamp01To100(-5f));
            Assert.AreEqual(100f, WaterIntrusionRules.Clamp01To100(150f));
            Assert.AreEqual(42f, WaterIntrusionRules.Clamp01To100(42f));
        }
    }
}
