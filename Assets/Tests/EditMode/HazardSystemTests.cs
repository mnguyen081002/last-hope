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
    }
}
