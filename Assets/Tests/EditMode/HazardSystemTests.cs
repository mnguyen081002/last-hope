using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using LastHope.Systems.Hazard;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class HazardSystemTests
    {
        HazardBalance balance;
        PlayerState player;

        [SetUp]
        public void SetUp()
        {
            balance = new HazardBalance();
            player = new PlayerState();
        }

        [TestCase(FloodState.Dry, true)]
        [TestCase(FloodState.Shallow, true)]
        [TestCase(FloodState.Medium, true)]
        [TestCase(FloodState.Deep, true)]
        [TestCase(FloodState.Impassable, false)]
        public void IsPassable_OnlyFalseForImpassable(FloodState state, bool expected)
        {
            Assert.AreEqual(expected, HazardSystem.IsPassable(state));
        }

        [TestCase(FloodState.Dry, 0)]
        [TestCase(FloodState.Shallow, 1)]
        [TestCase(FloodState.Medium, 2)]
        [TestCase(FloodState.Deep, 3)]
        public void FloodIndex_MatchesArrayPosition(FloodState state, int expectedIndex)
        {
            Assert.AreEqual(expectedIndex, HazardSystem.FloodIndex(state));
        }

        [Test]
        public void TimeFactor_ReadsFromBalanceArray()
        {
            Assert.AreEqual(1.0f, HazardSystem.TimeFactor(FloodState.Dry, balance));
            Assert.AreEqual(1.2f, HazardSystem.TimeFactor(FloodState.Shallow, balance));
            Assert.AreEqual(1.5f, HazardSystem.TimeFactor(FloodState.Medium, balance));
            Assert.AreEqual(2.0f, HazardSystem.TimeFactor(FloodState.Deep, balance));
        }

        [Test]
        public void ApplyCrossingCost_Dry_StillAddsSmallWetGain()
        {
            HazardSystem.ApplyCrossingCost(player, FloodState.Dry, balance);

            Assert.AreEqual(100f, player.Stamina, "Dry không tốn stamina.");
            Assert.AreEqual(10f, player.Wet, "Dry vẫn có wet_gain nhỏ (mưa/bùn).");
            Assert.AreEqual(0f, player.BlackWaterExposure);
        }

        [Test]
        public void ApplyCrossingCost_Deep_AppliesFullCost()
        {
            HazardSystem.ApplyCrossingCost(player, FloodState.Deep, balance);

            Assert.AreEqual(100f - 30f, player.Stamina, 0.0001f);
            Assert.AreEqual(30f, player.BlackWaterExposure, 0.0001f);
            Assert.AreEqual(90f, player.Wet, 0.0001f);
        }

        [Test]
        public void ApplyCrossingCost_ClampsAtBounds()
        {
            player.Stamina = 5f;
            player.BlackWaterExposure = 95f;
            player.Wet = 95f;

            HazardSystem.ApplyCrossingCost(player, FloodState.Deep, balance);

            Assert.AreEqual(0f, player.Stamina, "Stamina không âm.");
            Assert.AreEqual(100f, player.BlackWaterExposure, "Exposure không vượt 100.");
            Assert.AreEqual(100f, player.Wet, "Wet không vượt 100.");
        }

        [Test]
        public void ApplyCrossingCost_WetMultiplier_ScalesWetGain()
        {
            HazardSystem.ApplyCrossingCost(player, FloodState.Deep, balance, wetMultiplier: 0.3f);

            Assert.AreEqual(90f * 0.3f, player.Wet, 0.0001f); // jacket wet_multiplier = 0.3
        }

        [Test]
        public void ApplyCrossingCost_BootsBlockLevel_ZerosExposureAtOrBelowLevel()
        {
            // Boots block_level=1 -> Shallow (index 1) exposure = 0 dù crossing_exposure_gain[1]=5.
            HazardSystem.ApplyCrossingCost(player, FloodState.Shallow, balance, exposureBlockLevel: 1);

            Assert.AreEqual(0f, player.BlackWaterExposure);
        }

        [Test]
        public void ApplyCrossingCost_BootsMediumMultiplier_ScalesExposureAboveBlockLevel()
        {
            // Deep (index 3) > block_level 1 -> exposure = crossing_exposure_gain[3] * multiplier.
            HazardSystem.ApplyCrossingCost(
                player, FloodState.Deep, balance, exposureBlockLevel: 1, exposureMediumMultiplier: 0.5f);

            Assert.AreEqual(30f * 0.5f, player.BlackWaterExposure, 0.0001f);
        }

        // ---------- EffectiveFlood (Route Closure) ----------

        [Test]
        public void EffectiveFlood_NoClosePhase_ReturnsManualFlood()
        {
            var route = new RouteDefinition { ClosesAtPhase = null };
            var state = new RouteState { Flood = FloodState.Shallow };

            var result = HazardSystem.EffectiveFlood(route, state, DisasterPhase.RouteClosure);

            Assert.AreEqual(FloodState.Shallow, result);
        }

        [Test]
        public void EffectiveFlood_PhaseNotReachedYet_ReturnsManualFlood()
        {
            var route = new RouteDefinition { ClosesAtPhase = DisasterPhase.BlackRain };
            var state = new RouteState { Flood = FloodState.Dry };

            var result = HazardSystem.EffectiveFlood(route, state, DisasterPhase.FirstRain);

            Assert.AreEqual(FloodState.Dry, result);
        }

        [Test]
        public void EffectiveFlood_PhaseReached_OverridesToImpassable()
        {
            var route = new RouteDefinition { ClosesAtPhase = DisasterPhase.BlackRain };
            var state = new RouteState { Flood = FloodState.Dry }; // dù thủ công đang Dry

            var result = HazardSystem.EffectiveFlood(route, state, DisasterPhase.BlackRain);

            Assert.AreEqual(FloodState.Impassable, result);
        }

        // ---------- Current Strength ----------

        [Test]
        public void ApplyCurrentCrossing_None_NoStaminaCost_NeverSweeps()
        {
            var rng = new RngStream(1UL);

            for (int i = 0; i < 100; i++)
            {
                HazardSystem.ApplyCurrentCrossing(player, CurrentStrength.None, balance, rng);
            }

            Assert.AreEqual(100f, player.Stamina);
            Assert.AreEqual(100f, player.Health, "0% sweep chance không bao giờ trúng.");
        }

        [Test]
        public void ApplyCurrentCrossing_Extreme_CostsStamina()
        {
            var rng = new RngStream(1UL);

            HazardSystem.ApplyCurrentCrossing(player, CurrentStrength.Extreme, balance, rng);

            Assert.AreEqual(100f - balance.CurrentStrengthStaminaCost[4], player.Stamina, 0.0001f);
        }

        [Test]
        public void ApplyCurrentCrossing_RopeReduction_LowersEffectiveIndex()
        {
            var rng = new RngStream(1UL);

            // Extreme(4) - reduction(1) = Strong(3) — dùng đúng chi phí của index 3, không phải 4.
            HazardSystem.ApplyCurrentCrossing(player, CurrentStrength.Extreme, balance, rng, currentReduction: 1);

            Assert.AreEqual(100f - balance.CurrentStrengthStaminaCost[3], player.Stamina, 0.0001f);
        }

        [Test]
        public void ApplyCurrentCrossing_RopeReduction_NeverGoesBelowNone()
        {
            var rng = new RngStream(1UL);

            HazardSystem.ApplyCurrentCrossing(player, CurrentStrength.Weak, balance, rng, currentReduction: 99);

            Assert.AreEqual(100f - balance.CurrentStrengthStaminaCost[0], player.Stamina, 0.0001f);
        }

        [Test]
        public void ApplyCurrentCrossing_SweepDamage_DoesNotHealBelowFloorFromOtherSource()
        {
            // Health đã thấp hơn mọi thứ do nguồn khác (vd Sick không floor) — sweep không
            // được kéo nó lên, chỉ có thể làm giảm thêm hoặc giữ nguyên qua Mathf.Max(0,...).
            player.Health = 2f;
            var rng = new RngStream(1UL);

            // Thử nhiều seed để chắc chắn bắt được ít nhất 1 lần trúng sweep ở Extreme (50%).
            bool everWentBelowOrEqual = true;
            for (int i = 0; i < 50; i++)
            {
                player.Health = 2f;
                HazardSystem.ApplyCurrentCrossing(player, CurrentStrength.Extreme, balance, rng);
                if (player.Health > 2f) everWentBelowOrEqual = false;
            }

            Assert.IsTrue(everWentBelowOrEqual, "Sweep không được hồi máu, chỉ giảm hoặc giữ nguyên.");
        }

        // ---------- Electrified Water ----------

        [Test]
        public void ApplyElectrifiedCrossing_False_NoEffect()
        {
            HazardSystem.ApplyElectrifiedCrossing(player, false, balance, new ConditionBalance());

            Assert.AreEqual(100f, player.Stamina);
            Assert.AreEqual(100f, player.Health);
        }

        [Test]
        public void ApplyElectrifiedCrossing_True_DamagesButStopsAboveCollapseFloor()
        {
            var conditionBalance = new ConditionBalance();
            player.Health = conditionBalance.CollapsedHealthThreshold + 2f; // gần floor

            HazardSystem.ApplyElectrifiedCrossing(player, true, balance, conditionBalance);

            Assert.AreEqual(conditionBalance.CollapsedHealthThreshold + 1f, player.Health,
                "Không kill tức thời — dừng ngay trên ngưỡng Collapse.");
        }

        [Test]
        public void ApplyElectrifiedCrossing_DoesNotHeal_IfAlreadyBelowFloor()
        {
            var conditionBalance = new ConditionBalance();
            player.Health = 1f; // đã dưới floor (do Sick chẳng hạn) — Electrified không được hồi lên floor.

            HazardSystem.ApplyElectrifiedCrossing(player, true, balance, conditionBalance);

            Assert.AreEqual(1f, player.Health);
        }
    }
}
