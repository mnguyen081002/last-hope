using System.IO;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Systems.Search;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class SearchSystemTests
    {
        const string DrinkShelf1 = "searchpoint_drink_shelf_1"; // guaranteed: 3 water bottle
        const string Counter = "searchpoint_counter"; // battery chance 60% x2, canned food guaranteed x1

        DefinitionRegistry definitions;
        WorldState world;
        RngService rng;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            world = new WorldState { MasterSeed = 999UL };
            rng = new RngService(world.MasterSeed, world.RngStreams);
        }

        [Test]
        public void Open_RollsGuaranteedItems()
        {
            var state = SearchSystem.Open(world, definitions, rng, DrinkShelf1);

            Assert.IsTrue(state.Rolled);
            Assert.AreEqual(3, InventoryOps.CountOf(state.RemainingItems, "item_water_bottle"));
        }

        [Test]
        public void Open_SecondTime_DoesNotReroll()
        {
            var first = SearchSystem.Open(world, definitions, rng, DrinkShelf1);
            int firstCount = InventoryOps.CountOf(first.RemainingItems, "item_water_bottle");

            // Giả lập đã lấy bớt trước khi mở lại.
            InventoryOps.RemoveItem(first.RemainingItems, "item_water_bottle", 1);

            var second = SearchSystem.Open(world, definitions, rng, DrinkShelf1);

            Assert.AreSame(first, second);
            Assert.AreEqual(firstCount - 1, InventoryOps.CountOf(second.RemainingItems, "item_water_bottle"));
        }

        [Test]
        public void Open_SameSeed_ProducesSameRoll()
        {
            var worldA = new WorldState { MasterSeed = 42UL };
            var stateA = SearchSystem.Open(worldA, definitions, new RngService(42UL, worldA.RngStreams), Counter);

            var worldB = new WorldState { MasterSeed = 42UL };
            var stateB = SearchSystem.Open(worldB, definitions, new RngService(42UL, worldB.RngStreams), Counter);

            Assert.AreEqual(
                InventoryOps.CountOf(stateA.RemainingItems, "item_battery"),
                InventoryOps.CountOf(stateB.RemainingItems, "item_battery"));
        }

        [Test]
        public void TakeAll_WithinCapacity_TakesEverything()
        {
            SearchSystem.Open(world, definitions, rng, DrinkShelf1);

            bool tookEverything = SearchSystem.TakeAll(world, definitions, DrinkShelf1);

            Assert.IsTrue(tookEverything);
            Assert.AreEqual(3, InventoryOps.CountOf(world.Player.Inventory, "item_water_bottle"));
        }

        [Test]
        public void TakeAll_OverCapacity_LeavesSomeBehind_ReportsFalse()
        {
            // Sức chứa cực nhỏ để chắc chắn tràn.
            world.Player.Inventory.CapacityKg = 0.5f;
            world.Player.Inventory.CapacityLiters = 100f;

            SearchSystem.Open(world, definitions, rng, DrinkShelf1); // 3 chai nước = 2.4kg

            bool tookEverything = SearchSystem.TakeAll(world, definitions, DrinkShelf1);

            Assert.IsFalse(tookEverything);
            int remaining = InventoryOps.CountOf(
                world.GetOrCreateLocation("location_convenience_store")
                    .SearchPoints[DrinkShelf1].RemainingItems,
                "item_water_bottle");
            Assert.Greater(remaining, 0, "Phải còn đồ trong container — đây là exit criteria triage.");
        }

        [Test]
        public void TakeAll_SurvivesSaveLoad_DoesNotReroll()
        {
            SearchSystem.Open(world, definitions, rng, DrinkShelf1);
            SearchSystem.TakeAll(world, definitions, DrinkShelf1); // lấy hết 3 chai

            // Mở lại search point khác chưa từng đụng tới — vẫn phải roll đúng 1 lần khi save/load.
            var json = LastHope.Core.Save.WorldStateSerializer.Serialize(world);
            var reloaded = LastHope.Core.Save.WorldStateSerializer.Deserialize(json);
            var rngReloaded = new RngService(reloaded.MasterSeed, reloaded.RngStreams);

            var state = SearchSystem.Open(reloaded, definitions, rngReloaded, DrinkShelf1);

            Assert.IsTrue(state.Rolled);
            Assert.AreEqual(0, InventoryOps.CountOf(state.RemainingItems, "item_water_bottle"),
                "Đã lấy hết trước khi save — load xong không được có lại (không re-roll).");
        }
    }
}
