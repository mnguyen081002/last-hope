using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class ConditionOpsTests
    {
        [Test]
        public void ApplyHealth_ClampsToDefaultFloorAndCeiling()
        {
            var c = new PlayerConditionState { Health = 2f };
            ConditionOps.ApplyHealth(c, -50f);
            Assert.AreEqual(0f, c.Health);

            c.Health = 98f;
            ConditionOps.ApplyHealth(c, 50f);
            Assert.AreEqual(100f, c.Health);
        }

        [Test]
        public void ApplyHealth_RespectsCustomFloor()
        {
            var c = new PlayerConditionState { Health = 2f };
            ConditionOps.ApplyHealth(c, -50f, floor: 1f);
            Assert.AreEqual(1f, c.Health);
        }

        [Test]
        public void SetStatusSeverity_ZeroOrBelow_RemovesStatus()
        {
            var c = new PlayerConditionState();
            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusCold, 50f, 10);
            Assert.IsTrue(c.StatusEffects.ContainsKey(ConditionOps.StatusCold));

            ConditionOps.SetStatusSeverity(c, ConditionOps.StatusCold, 0f, 20);
            Assert.IsFalse(c.StatusEffects.ContainsKey(ConditionOps.StatusCold));
        }

        [Test]
        public void GetStatusSeverity_MissingStatus_ReturnsZero()
        {
            var c = new PlayerConditionState();
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusWet));
        }

        [Test]
        public void AddExposure_Accumulates_AndNeverGoesNegative()
        {
            var c = new PlayerConditionState();
            ConditionOps.AddExposure(c, "black_water", 10f);
            ConditionOps.AddExposure(c, "black_water", 15f);
            Assert.AreEqual(25f, ConditionOps.GetExposure(c, "black_water"));

            ConditionOps.AddExposure(c, "black_water", -100f);
            Assert.AreEqual(0f, ConditionOps.GetExposure(c, "black_water"));
        }

        [Test]
        public void ExposureStatusChain_BelowThreshold_NoStatus()
        {
            var c = new PlayerConditionState();
            var cfg = new ConditionBalance();
            ConditionOps.AddExposure(c, "black_water", 39f);

            ConditionOps.ApplyExposureStatusChain(c, "black_water", 5, cfg);

            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure));
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick));
        }

        [Test]
        public void ExposureStatusChain_AtBlackWaterThreshold_SetsExposureStatusOnly()
        {
            var c = new PlayerConditionState();
            var cfg = new ConditionBalance();
            ConditionOps.AddExposure(c, "black_water", 40f);

            ConditionOps.ApplyExposureStatusChain(c, "black_water", 5, cfg);

            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure), 0f);
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick));
        }

        [Test]
        public void ExposureStatusChain_AtSickThreshold_SetsBothStatuses()
        {
            var c = new PlayerConditionState();
            var cfg = new ConditionBalance();
            ConditionOps.AddExposure(c, "black_water", 70f);

            ConditionOps.ApplyExposureStatusChain(c, "black_water", 5, cfg);

            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure), 0f);
            Assert.Greater(ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick), 0f);
        }

        [Test]
        public void ExposureStatusChain_RecoveringBelowThreshold_ClearsStatuses()
        {
            var c = new PlayerConditionState();
            var cfg = new ConditionBalance();
            ConditionOps.AddExposure(c, "black_water", 70f);
            ConditionOps.ApplyExposureStatusChain(c, "black_water", 5, cfg);

            ConditionOps.AddExposure(c, "black_water", -60f); // now 10, below both thresholds
            ConditionOps.ApplyExposureStatusChain(c, "black_water", 6, cfg);

            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusBlackWaterExposure));
            Assert.AreEqual(0f, ConditionOps.GetStatusSeverity(c, ConditionOps.StatusSick));
        }

        [Test]
        public void RecomputeIncapacitation_AtOrBelowThreshold_IsCollapsed()
        {
            var c = new PlayerConditionState { Health = 5f };
            var cfg = new ConditionBalance();

            ConditionOps.RecomputeIncapacitation(c, cfg);

            Assert.IsTrue(ConditionOps.IsIncapacitated(c));
            Assert.AreEqual(IncapacitationState.Collapsed, c.Incapacitation);
        }

        [Test]
        public void RecomputeIncapacitation_AboveThreshold_IsNotCollapsed()
        {
            var c = new PlayerConditionState { Health = 6f };
            var cfg = new ConditionBalance();

            ConditionOps.RecomputeIncapacitation(c, cfg);

            Assert.IsFalse(ConditionOps.IsIncapacitated(c));
        }
    }
}
