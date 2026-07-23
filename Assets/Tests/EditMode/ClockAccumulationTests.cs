using LastHope.Core.Time;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class ClockAccumulationTests
    {
        [Test]
        public void Simulate24GameHours_VaryingDeltas_NoDrift()
        {
            var clock = new SimulationClock();
            // decimal bookkeeping for the harness itself: summing ~17,000 double deltas via
            // `remaining -= delta` in double arithmetic accumulates its own rounding error
            // (observed landing 1 minute short) independent of the clock under test.
            decimal totalRealSeconds = 24m * 60m * 60m / (decimal)SimulationClock.GameSecondsPerRealSecond; // 17280
            var rnd = new System.Random(12345);
            decimal remaining = totalRealSeconds;
            int minutesConsumed = 0;

            while (remaining > 0)
            {
                decimal delta = System.Math.Min(remaining, (decimal)(rnd.NextDouble() * 2.0));
                if (delta <= 0) delta = remaining;
                remaining -= delta;

                clock.AccumulateRealSeconds((double)delta);
                while (clock.TryConsumeMinute()) minutesConsumed++;
            }

            Assert.AreEqual(1440, minutesConsumed);
            Assert.Less(clock.PendingGameSeconds, 60);
        }

        [Test]
        public void SingleSubMinuteDelta_ProducesNoMinuteYet()
        {
            var clock = new SimulationClock();
            clock.AccumulateRealSeconds(0.9); // 0.9 * 5 = 4.5 game-seconds, far less than 60
            Assert.IsFalse(clock.TryConsumeMinute());
        }
    }
}
