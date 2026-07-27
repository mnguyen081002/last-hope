using System.Collections.Generic;
using LastHope.Core.Random;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class RngServiceTests
    {
        const ulong Seed = 123456789UL;

        static List<int> Take(RngStream stream, int count)
        {
            var values = new List<int>(count);
            for (int i = 0; i < count; i++) values.Add(stream.NextInt(0, 1000));
            return values;
        }

        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var a = new RngService(Seed, new Dictionary<string, ulong>());
            var b = new RngService(Seed, new Dictionary<string, ulong>());

            CollectionAssert.AreEqual(
                Take(a.Stream(RngService.Loot), 50),
                Take(b.Stream(RngService.Loot), 50));
        }

        [Test]
        public void DifferentSeed_ProducesDifferentSequence()
        {
            var a = new RngService(Seed, new Dictionary<string, ulong>());
            var b = new RngService(Seed + 1, new Dictionary<string, ulong>());

            CollectionAssert.AreNotEqual(
                Take(a.Stream(RngService.Loot), 50),
                Take(b.Stream(RngService.Loot), 50));
        }

        [Test]
        public void DrawingFromOneStream_DoesNotShiftAnother()
        {
            var reference = new RngService(Seed, new Dictionary<string, ulong>());
            var expectedEvents = Take(reference.Stream(RngService.Events), 20);

            var service = new RngService(Seed, new Dictionary<string, ulong>());
            // Rút nhiều ở "loot" trước — không được ảnh hưởng chuỗi của "events".
            Take(service.Stream(RngService.Loot), 500);

            CollectionAssert.AreEqual(expectedEvents, Take(service.Stream(RngService.Events), 20));
        }

        [Test]
        public void StreamState_SurvivesRoundTrip_SequenceContinues()
        {
            var backing = new Dictionary<string, ulong>();
            var service = new RngService(Seed, backing);

            Take(service.Stream(RngService.Loot), 37);
            service.FlushState();

            var expected = Take(service.Stream(RngService.Loot), 20);

            // Dựng lại service từ đúng state đã flush — phải chạy tiếp, không quay về đầu.
            var restoredBacking = new Dictionary<string, ulong>(backing);
            var restored = new RngService(Seed, restoredBacking);

            CollectionAssert.AreEqual(expected, Take(restored.Stream(RngService.Loot), 20));
        }

        [Test]
        public void FlushState_WritesEveryTouchedStream()
        {
            var backing = new Dictionary<string, ulong>();
            var service = new RngService(Seed, backing);

            service.Stream(RngService.Loot).NextULong();
            service.Stream(RngService.Npc).NextULong();
            service.FlushState();

            Assert.IsTrue(backing.ContainsKey(RngService.Loot));
            Assert.IsTrue(backing.ContainsKey(RngService.Npc));
            Assert.IsFalse(backing.ContainsKey(RngService.Events), "Stream chưa dùng thì không ghi.");
        }

        [Test]
        public void NextInt_StaysInRange()
        {
            var stream = new RngStream(Seed);

            for (int i = 0; i < 1000; i++)
            {
                int value = stream.NextInt(5, 10);
                Assert.GreaterOrEqual(value, 5);
                Assert.Less(value, 10);
            }
        }

        [Test]
        public void NextChance_HonoursBoundaries()
        {
            var stream = new RngStream(Seed);

            for (int i = 0; i < 100; i++)
            {
                Assert.IsFalse(stream.NextChance(0f));
                Assert.IsTrue(stream.NextChance(100f));
            }
        }

        [Test]
        public void ZeroSeed_DoesNotFreezeGenerator()
        {
            var stream = new RngStream(0UL);

            ulong first = stream.NextULong();
            ulong second = stream.NextULong();

            Assert.AreNotEqual(0UL, first);
            Assert.AreNotEqual(first, second);
        }
    }
}
