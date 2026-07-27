using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Time;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class TickSchedulerTests
    {
        WorldState world;
        EventBus events;
        TickScheduler scheduler;
        List<long> shortTicks;
        List<long> longTicks;

        [SetUp]
        public void SetUp()
        {
            world = new WorldState();
            events = new EventBus();
            scheduler = new TickScheduler(world, events);

            shortTicks = new List<long>();
            longTicks = new List<long>();
            scheduler.ShortTick += m => shortTicks.Add(m);
            scheduler.LongTick += m => longTicks.Add(m);
        }

        [Test]
        public void HundredMinutes_Gives100ShortAnd10LongTicks()
        {
            scheduler.Advance(100, maxCatchUpMinutes: 1000);

            Assert.AreEqual(100, shortTicks.Count);
            Assert.AreEqual(10, longTicks.Count);
            Assert.AreEqual(100, world.WorldTimeMinutes);
        }

        [Test]
        public void LongTick_FiresOnMultiplesOfTen_NoDoubleFire()
        {
            scheduler.Advance(30, maxCatchUpMinutes: 1000);

            CollectionAssert.AreEqual(new long[] { 10, 20, 30 }, longTicks);
        }

        [Test]
        public void CatchUp_IsBounded_AndReportsMinutesActuallyRun()
        {
            int ran = scheduler.Advance(500, maxCatchUpMinutes: 60);

            Assert.AreEqual(60, ran);
            Assert.AreEqual(60, world.WorldTimeMinutes);
            Assert.AreEqual(60, shortTicks.Count);
        }

        [Test]
        public void FastForward_RunsEveryMinute_NotAJump()
        {
            scheduler.FastForward(8 * 60);

            Assert.AreEqual(480, shortTicks.Count, "Sleep/Travel không được cộng thẳng thời gian.");
            Assert.AreEqual(48, longTicks.Count);
            Assert.AreEqual(480, world.WorldTimeMinutes);
        }

        [Test]
        public void PublishesWorldTimeChanged_WithLongTickFlag()
        {
            var received = new List<WorldTimeChanged>();
            events.Subscribe<WorldTimeChanged>(e => received.Add(e));

            scheduler.Advance(10, maxCatchUpMinutes: 1000);

            Assert.AreEqual(10, received.Count);
            Assert.IsFalse(received[0].IsLongTick);
            Assert.IsTrue(received[9].IsLongTick);
            Assert.AreEqual(10, received[9].WorldTimeMinutes);
        }

        [Test]
        public void ZeroOrNegativeMinutes_DoNothing()
        {
            Assert.AreEqual(0, scheduler.Advance(0));
            Assert.AreEqual(0, scheduler.Advance(-5));
            Assert.AreEqual(0, world.WorldTimeMinutes);
        }

        [Test]
        public void GameTimeUtil_AnchorsDayZeroAt17h()
        {
            Assert.AreEqual(0, GameTimeUtil.DayIndex(0));
            Assert.AreEqual(17, GameTimeUtil.HourOfDay(0));

            // 7 tiếng sau 17:00 là 00:00 hôm sau.
            long sevenHours = 7 * 60;
            Assert.AreEqual(1, GameTimeUtil.DayIndex(sevenHours));
            Assert.AreEqual(0, GameTimeUtil.HourOfDay(sevenHours));

            Assert.AreEqual("Day 0 17:30", GameTimeUtil.Format(30));
        }
    }
}
