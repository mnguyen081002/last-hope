using LastHope.Data.Definitions;
using LastHope.Systems.Hazard;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class DisasterPhaseSystemTests
    {
        DisasterPhaseBalance balance;

        [SetUp]
        public void SetUp() => balance = new DisasterPhaseBalance();

        [Test]
        public void CurrentPhase_StartsDry()
        {
            Assert.AreEqual(DisasterPhase.Dry, DisasterPhaseSystem.CurrentPhase(0, balance));
            Assert.AreEqual(DisasterPhase.Dry, DisasterPhaseSystem.CurrentPhase(
                (long)balance.FirstRainAtMinute - 1, balance));
        }

        [Test]
        public void CurrentPhase_TransitionsAtExactMinute()
        {
            Assert.AreEqual(DisasterPhase.FirstRain,
                DisasterPhaseSystem.CurrentPhase((long)balance.FirstRainAtMinute, balance));
            Assert.AreEqual(DisasterPhase.BlackRain,
                DisasterPhaseSystem.CurrentPhase((long)balance.BlackRainAtMinute, balance));
            Assert.AreEqual(DisasterPhase.RouteClosure,
                DisasterPhaseSystem.CurrentPhase((long)balance.RouteClosureAtMinute, balance));
        }

        [Test]
        public void CurrentPhase_StaysAtRouteClosure_WellPastThreshold()
        {
            Assert.AreEqual(DisasterPhase.RouteClosure,
                DisasterPhaseSystem.CurrentPhase((long)balance.RouteClosureAtMinute + 100000, balance));
        }

        [Test]
        public void IsRaining_FalseBeforeFirstRain_TrueFromFirstRainOnward()
        {
            Assert.IsFalse(DisasterPhaseSystem.IsRaining(DisasterPhase.Dry));
            Assert.IsTrue(DisasterPhaseSystem.IsRaining(DisasterPhase.FirstRain));
            Assert.IsTrue(DisasterPhaseSystem.IsRaining(DisasterPhase.BlackRain));
            Assert.IsTrue(DisasterPhaseSystem.IsRaining(DisasterPhase.RouteClosure));
        }
    }
}
