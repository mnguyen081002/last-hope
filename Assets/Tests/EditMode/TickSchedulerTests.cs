using System.Collections.Generic;
using System.Linq;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Time;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class TickSchedulerTests
    {
        [Test]
        public void FastForward100Minutes_Fires100Short10Long()
        {
            var world = new WorldState();
            var scheduler = new TickScheduler(world, new EventBus());
            int shortCount = 0, longCount = 0;
            scheduler.SubscribeShort(_ => shortCount++);
            scheduler.SubscribeLong(_ => longCount++);

            scheduler.FastForward(100);

            Assert.AreEqual(100, shortCount);
            Assert.AreEqual(10, longCount);
            Assert.AreEqual(100, world.WorldTimeMinutes);
        }

        [Test]
        public void Advance_AcrossManySmallChunks_NeverDoubleFires()
        {
            var world = new WorldState();
            var scheduler = new TickScheduler(world, new EventBus());
            var seenMinutes = new List<long>();
            scheduler.SubscribeShort(m => seenMinutes.Add(m));

            var clock = new SimulationClock();
            for (int i = 0; i < 1000; i++)
            {
                clock.AccumulateRealSeconds(0.1); // 0.1 * 5 = 0.5 game-seconds per chunk
                scheduler.Advance(clock, 1000);
            }

            // 1000 * 0.1 realSec * 5 = 500 game-seconds = 8 minutes + 20s remainder
            CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4, 5, 6, 7, 8 }, seenMinutes);
        }

        [Test]
        public void CatchUp_BoundedPerAdvance_RemainderPreserved()
        {
            var world = new WorldState();
            var scheduler = new TickScheduler(world, new EventBus());
            var clock = new SimulationClock();

            clock.AccumulateRealSeconds(500 * 60 / SimulationClock.GameSecondsPerRealSecond); // bank 500 minutes

            int firstBatch = scheduler.Advance(clock, 60);
            Assert.AreEqual(60, firstBatch);
            Assert.AreEqual(60, world.WorldTimeMinutes);

            int totalRemaining = 0;
            int batch;
            while ((batch = scheduler.Advance(clock, 60)) > 0) totalRemaining += batch;

            Assert.AreEqual(440, totalRemaining);
            Assert.AreEqual(500, world.WorldTimeMinutes);
        }

        [Test]
        public void FastForward_RecordsStrictlyConsecutiveMinutes_LongEveryTenth()
        {
            var world = new WorldState();
            var scheduler = new TickScheduler(world, new EventBus());
            var shortMinutes = new List<long>();
            var longMinutes = new List<long>();
            scheduler.SubscribeShort(m => shortMinutes.Add(m));
            scheduler.SubscribeLong(m => longMinutes.Add(m));

            scheduler.FastForward(35);

            CollectionAssert.AreEqual(Enumerable.Range(1, 35).Select(i => (long)i), shortMinutes);
            CollectionAssert.AreEqual(new long[] { 10, 20, 30 }, longMinutes);
        }

        [Test]
        public void Threshold_FiresExactlyOnceWhenCrossed()
        {
            var world = new WorldState();
            var scheduler = new TickScheduler(world, new EventBus());
            int fireCount = 0;
            scheduler.RegisterThreshold(50, _ => fireCount++);

            scheduler.FastForward(45);
            Assert.AreEqual(0, fireCount);

            scheduler.FastForward(15); // crosses 50 (now at 60)
            Assert.AreEqual(1, fireCount);

            scheduler.FastForward(50); // well past, must not refire
            Assert.AreEqual(1, fireCount);
        }
    }
}
