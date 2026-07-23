using LastHope.Core.Random;
using LastHope.Core.State;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class RngServiceTests
    {
        [Test]
        public void SameSeedSameStream_SameSequence()
        {
            var worldA = new WorldState { RandomSeed = 12345 };
            var worldB = new WorldState { RandomSeed = 12345 };
            var streamA = new RngService(worldA).GetStream("loot");
            var streamB = new RngService(worldB).GetStream("loot");

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(streamA.NextInt(0, 1000), streamB.NextInt(0, 1000));
        }

        [Test]
        public void ExtraCallsOnOtherStream_DoNotShiftLootStream()
        {
            var worldA = new WorldState { RandomSeed = 999 };
            var worldB = new WorldState { RandomSeed = 999 };
            var serviceA = new RngService(worldA);
            var serviceB = new RngService(worldB);

            var lootA = serviceA.GetStream("loot");
            int[] expected = new int[20];
            for (int i = 0; i < expected.Length; i++) expected[i] = lootA.NextInt(0, 10000);

            var lootB = serviceB.GetStream("loot");
            var npcB = serviceB.GetStream("npc");
            int[] actual = new int[20];
            for (int i = 0; i < actual.Length; i++)
            {
                npcB.NextInt(0, 10000); // interleave calls on an unrelated stream
                actual[i] = lootB.NextInt(0, 10000);
            }

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void StreamState_SurvivesRoundtrip_ContinuesSequence()
        {
            var world = new WorldState { RandomSeed = 42 };
            var service = new RngService(world);
            var loot = service.GetStream("loot");

            for (int i = 0; i < 5; i++) loot.NextInt(0, 10000);

            // Simulate save/load: state is captured by reference inside world.RngStreams,
            // a fresh RngService over the SAME world object must continue, not restart.
            var resumedService = new RngService(world);
            var resumedLoot = resumedService.GetStream("loot");

            var freshWorld = new WorldState { RandomSeed = 42 };
            var freshService = new RngService(freshWorld);
            var freshLoot = freshService.GetStream("loot");
            for (int i = 0; i < 5; i++) freshLoot.NextInt(0, 10000); // catch up to the same point

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(freshLoot.NextInt(0, 10000), resumedLoot.NextInt(0, 10000));
        }
    }
}
