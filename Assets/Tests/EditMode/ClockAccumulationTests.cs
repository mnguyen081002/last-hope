using LastHope.Core.Time;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class ClockAccumulationTests
    {
        [Test]
        public void ExactMinute_BanksOneMinute()
        {
            var clock = new SimulationClock { TimeScale = 60f };

            Assert.AreEqual(1, clock.AccumulateRealSeconds(1f));
            Assert.AreEqual(0.0, clock.PendingSeconds, 0.0001);
        }

        [Test]
        public void PartialSeconds_AreKept_NotDiscarded()
        {
            var clock = new SimulationClock { TimeScale = 60f };

            // 0.5s × 60 = 30 giây game — chưa đủ 1 phút.
            Assert.AreEqual(0, clock.AccumulateRealSeconds(0.5f));
            Assert.AreEqual(30.0, clock.PendingSeconds, 0.0001);

            // Nửa còn lại phải bank ra đúng 1 phút.
            Assert.AreEqual(1, clock.AccumulateRealSeconds(0.5f));
        }

        [Test]
        public void Paused_AccumulatesNothing()
        {
            var clock = new SimulationClock { TimeScale = 60f, Paused = true };

            Assert.AreEqual(0, clock.AccumulateRealSeconds(10f));
            Assert.AreEqual(0.0, clock.PendingSeconds, 0.0001);
        }

        [Test]
        public void TwentyFourHours_WithVaryingDelta_DoesNotDrift()
        {
            var clock = new SimulationClock { TimeScale = SimulationClock.DefaultTimeScale };

            // Delta thay đổi liên tục như frame thật — đây là chỗ accumulator hay trôi.
            float[] deltas = { 0.0163f, 0.0331f, 0.0089f, 0.0207f, 0.0412f, 0.0074f };

            const int targetMinutes = 24 * 60;
            double realSecondsNeeded = targetMinutes * 60.0 / SimulationClock.DefaultTimeScale;

            int totalMinutes = 0;
            double realElapsed = 0.0;
            int i = 0;

            while (realElapsed < realSecondsNeeded)
            {
                float delta = deltas[i++ % deltas.Length];
                realElapsed += delta;
                totalMinutes += clock.AccumulateRealSeconds(delta);
            }

            // Sai số cho phép: 1 phút (do vòng lặp dừng ở ranh giới frame).
            Assert.AreEqual(targetMinutes, totalMinutes, 1.0,
                $"Trôi {totalMinutes - targetMinutes} phút sau 24h game.");
        }

        [Test]
        public void LargeDelta_BanksAllMinutesAtOnce()
        {
            var clock = new SimulationClock { TimeScale = 60f };

            Assert.AreEqual(10, clock.AccumulateRealSeconds(10f));
        }
    }
}
