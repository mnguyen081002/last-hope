using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class TravelRulesTests
    {
        private static HazardRules.RouteHazardLevels Levels(int flood, int current) => new HazardRules.RouteHazardLevels(flood, current);

        [Test]
        public void Tier0_Passable_UsesTier0Costs()
        {
            var cfg = new HazardBalance();
            var result = TravelRules.EvaluateCrossing(Levels(0, 0), new PlayerConditionState(), cfg, EquipmentProtection.None);

            Assert.IsTrue(result.Passable);
            Assert.AreEqual(cfg.CrossingStaminaCost[0], result.StaminaCost);
            Assert.AreEqual(cfg.CrossingExposureGain[0], result.ExposureGain);
            Assert.AreEqual(cfg.CrossingWetGain[0], result.WetGain);
            Assert.AreEqual(cfg.CrossingTimeFactor[0], result.TimeFactor);
        }

        [Test]
        public void FloodAtMaxLevel_IsImpassable()
        {
            var cfg = new HazardBalance();
            var result = TravelRules.EvaluateCrossing(Levels(HazardRules.MaxLevel, 0), new PlayerConditionState(), cfg, EquipmentProtection.None);

            Assert.IsFalse(result.Passable);
            Assert.AreEqual(0f, result.StaminaCost);
            Assert.IsNotEmpty(result.Warnings);
        }

        [Test]
        public void CurrentAtMaxLevel_IsImpassable()
        {
            var cfg = new HazardBalance();
            var result = TravelRules.EvaluateCrossing(Levels(0, HazardRules.MaxLevel), new PlayerConditionState(), cfg, EquipmentProtection.None);

            Assert.IsFalse(result.Passable);
        }

        [Test]
        public void Rope_ReducesEffectiveCurrent_MakesImpassableRoutePassable()
        {
            var cfg = new HazardBalance();
            var rope = new EquipmentProtection(currentReduction: 1, wetMultiplier: 1f, bootsBlockLevel: 0, bootsMediumMultiplier: 1f);

            var result = TravelRules.EvaluateCrossing(Levels(0, HazardRules.MaxLevel), new PlayerConditionState(), cfg, rope);

            Assert.IsTrue(result.Passable); // current reduced from 4 to 3, tier 3 is below MaxLevel
        }

        [Test]
        public void Jacket_ReducesWetGain()
        {
            var cfg = new HazardBalance();
            var jacket = new EquipmentProtection(0, 0.3f, 0, 1f);

            var result = TravelRules.EvaluateCrossing(Levels(1, 0), new PlayerConditionState(), cfg, jacket);

            Assert.AreEqual(cfg.CrossingWetGain[1] * 0.3f, result.WetGain, 0.001f);
        }

        [Test]
        public void Boots_BlockExposure_AtOrBelowBlockLevel()
        {
            var cfg = new HazardBalance();
            var boots = new EquipmentProtection(0, 1f, bootsBlockLevel: 1, bootsMediumMultiplier: 0.5f);

            var result = TravelRules.EvaluateCrossing(Levels(1, 0), new PlayerConditionState(), cfg, boots);

            Assert.AreEqual(0f, result.ExposureGain);
        }

        [Test]
        public void Boots_HalveExposure_OneTierAboveBlockLevel()
        {
            var cfg = new HazardBalance();
            var boots = new EquipmentProtection(0, 1f, bootsBlockLevel: 1, bootsMediumMultiplier: 0.5f);

            var result = TravelRules.EvaluateCrossing(Levels(2, 0), new PlayerConditionState(), cfg, boots);

            Assert.AreEqual(cfg.CrossingExposureGain[2] * 0.5f, result.ExposureGain, 0.001f);
        }

        [Test]
        public void Boots_NoEffect_TwoTiersAboveBlockLevel()
        {
            var cfg = new HazardBalance();
            var boots = new EquipmentProtection(0, 1f, bootsBlockLevel: 1, bootsMediumMultiplier: 0.5f);

            var result = TravelRules.EvaluateCrossing(Levels(3, 0), new PlayerConditionState(), cfg, boots);

            Assert.AreEqual(cfg.CrossingExposureGain[3], result.ExposureGain, 0.001f);
        }

        [Test]
        public void LowStamina_StillPassable_ButWarns()
        {
            var cfg = new HazardBalance();
            var condition = new PlayerConditionState { Stamina = 2f };

            var result = TravelRules.EvaluateCrossing(Levels(2, 0), condition, cfg, EquipmentProtection.None);

            Assert.IsTrue(result.Passable);
            Assert.IsNotEmpty(result.Warnings);
        }
    }
}
