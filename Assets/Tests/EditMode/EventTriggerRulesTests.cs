using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class EventTriggerRulesTests
    {
        private static ShelterState Shelter(WaterIntrusionLevel level) =>
            new ShelterState { WaterIntrusion = new WaterIntrusionState { Level = level } };

        [Test]
        public void Evaluate_NoConditionsSet_AlwaysTrue()
        {
            var def = new EventDefinition();
            Assert.IsTrue(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), hasActivePump: false, roll: null));
        }

        [Test]
        public void Evaluate_PhaseIdMismatch_False()
        {
            var def = new EventDefinition { TriggerPhaseId = "phase_peak" };
            var phase = new DisasterPhaseDefinition { Id = "phase_dry" };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Dry), false, null));
        }

        [Test]
        public void Evaluate_PhaseIdMatch_True()
        {
            var def = new EventDefinition { TriggerPhaseId = "phase_peak" };
            var phase = new DisasterPhaseDefinition { Id = "phase_peak" };
            Assert.IsTrue(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Dry), false, null));
        }

        [Test]
        public void Evaluate_RequiresBlackWater_FalseWhenPhaseDoesNotHaveIt()
        {
            var def = new EventDefinition { TriggerRequiresBlackWater = true };
            var phase = new DisasterPhaseDefinition { Id = "phase_x", BlackWater = false };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Dry), false, null));
        }

        [Test]
        public void Evaluate_RequiresBlackWater_TrueWhenPhaseHasIt()
        {
            var def = new EventDefinition { TriggerRequiresBlackWater = true };
            var phase = new DisasterPhaseDefinition { Id = "phase_x", BlackWater = true };
            Assert.IsTrue(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Dry), false, null));
        }

        [Test]
        public void Evaluate_StateMinLevel_FalseWhenBelowThreshold()
        {
            var def = new EventDefinition { TriggerStateMinLevel = "Shallow" };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Damp), false, null));
        }

        [Test]
        public void Evaluate_StateMinLevel_TrueWhenAtOrAboveThreshold()
        {
            var def = new EventDefinition { TriggerStateMinLevel = "Shallow" };
            Assert.IsTrue(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Deep), false, null));
        }

        [Test]
        public void Evaluate_RequiresPumpModule_FalseWhenNoActivePump()
        {
            var def = new EventDefinition { TriggerRequiresPumpModule = true };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), hasActivePump: false, roll: null));
        }

        [Test]
        public void Evaluate_RequiresPumpModule_TrueWhenActivePump()
        {
            var def = new EventDefinition { TriggerRequiresPumpModule = true };
            Assert.IsTrue(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), hasActivePump: true, roll: null));
        }

        [Test]
        public void Evaluate_ChanceGate_NullRoll_False()
        {
            var def = new EventDefinition { TriggerChancePercentPerLongTick = 50 };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), false, roll: null));
        }

        [Test]
        public void Evaluate_ChanceGate_RollBelowThreshold_True()
        {
            var def = new EventDefinition { TriggerChancePercentPerLongTick = 50 };
            Assert.IsTrue(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), false, roll: 10));
        }

        [Test]
        public void Evaluate_ChanceGate_RollAtOrAboveThreshold_False()
        {
            var def = new EventDefinition { TriggerChancePercentPerLongTick = 50 };
            Assert.IsFalse(EventTriggerRules.Evaluate(def, null, Shelter(WaterIntrusionLevel.Dry), false, roll: 50));
        }

        [Test]
        public void Evaluate_AllConditionsCombined_MustAllPass()
        {
            var def = new EventDefinition
            {
                TriggerRequiresBlackWater = true,
                TriggerStateMinLevel = "Damp",
            };
            var phase = new DisasterPhaseDefinition { Id = "phase_x", BlackWater = true };

            Assert.IsTrue(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Damp), false, null));
            Assert.IsFalse(EventTriggerRules.Evaluate(def, phase, Shelter(WaterIntrusionLevel.Dry), false, null)); // state fails
        }
    }
}
