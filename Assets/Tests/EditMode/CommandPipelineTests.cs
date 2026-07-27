using System.IO;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class CommandPipelineTests
    {
        GameContext context;
        CommandProcessor processor;
        DefinitionRegistry definitions;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));

            var world = new WorldState { MasterSeed = 1UL };
            world.Player.CurrentLocationId = "location_shelter";
            world.Player.Thirst = 80f;

            context = new GameContext
            {
                World = world,
                Definitions = definitions,
                Events = new EventBus(),
                Rng = new RngService(1UL, world.RngStreams),
            };
            processor = new CommandProcessor(context);
        }

        [Test]
        public void UseItem_AppliesEffect_AndConsumesOne()
        {
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_water_bottle", 2);

            var result = processor.Submit(new UseItemCommand("item_water_bottle"));

            Assert.IsTrue(result.Success);
            // use_effects.thirst = -40 → 80 - 40 = 40.
            Assert.AreEqual(40f, context.World.Player.Thirst, 0.001f);
            Assert.AreEqual(1, InventoryOps.CountOf(context.World.Player.Inventory, "item_water_bottle"));
        }

        [Test]
        public void UseItem_Medkit_HealsHealth()
        {
            context.World.Player.Health = 30f;
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_medkit", 1);

            var result = processor.Submit(new UseItemCommand("item_medkit"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(80f, context.World.Player.Health, 0.001f); // use_effects.health = 50
        }

        [Test]
        public void UseItem_Medkit_ClampsHealthAt100()
        {
            context.World.Player.Health = 90f;
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_medkit", 1);

            processor.Submit(new UseItemCommand("item_medkit"));

            Assert.AreEqual(100f, context.World.Player.Health);
        }

        [Test]
        public void UseItem_PublishesInventoryChanged()
        {
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_water_bottle", 1);

            bool published = false;
            context.Events.Subscribe<InventoryChanged>(_ => published = true);

            processor.Submit(new UseItemCommand("item_water_bottle"));

            Assert.IsTrue(published);
        }

        [Test]
        public void ValidateFail_DoesNotMutateState()
        {
            float thirstBefore = context.World.Player.Thirst;

            var result = processor.Submit(new UseItemCommand("item_water_bottle"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandErrorCode.ItemNotFound, result.Error);
            Assert.AreEqual(thirstBefore, context.World.Player.Thirst, 0.001f);
            Assert.IsEmpty(context.World.Player.Inventory.Slots);
        }

        [Test]
        public void UnknownItem_IsRejected()
        {
            var result = processor.Submit(new UseItemCommand("item_khong_ton_tai"));

            Assert.AreEqual(CommandErrorCode.UnknownDefinition, result.Error);
        }

        [Test]
        public void ItemWithoutUseEffects_IsRejected()
        {
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_toolbox", 1);

            var result = processor.Submit(new UseItemCommand("item_toolbox"));

            Assert.AreEqual(CommandErrorCode.InvalidTarget, result.Error);
        }

        [Test]
        public void Submit_StampsWorldTime()
        {
            context.World.WorldTimeMinutes = 777;
            InventoryOps.AddItem(context.World.Player.Inventory, definitions, "item_water_bottle", 1);

            var command = new UseItemCommand("item_water_bottle");
            processor.Submit(command);

            Assert.AreEqual(777, command.WorldTime);
        }

        // ---------- InventoryOps ----------

        [Test]
        public void AddItem_FillsExistingStackBeforeOpeningNewSlot()
        {
            var inventory = context.World.Player.Inventory;

            // item_water_bottle max_stack_size = 4.
            InventoryOps.AddItem(inventory, definitions, "item_water_bottle", 3);
            Assert.AreEqual(1, inventory.Slots.Count);

            InventoryOps.AddItem(inventory, definitions, "item_water_bottle", 3);
            Assert.AreEqual(2, inventory.Slots.Count);
            Assert.AreEqual(4, inventory.Slots[0].Quantity);
            Assert.AreEqual(2, inventory.Slots[1].Quantity);
        }

        [Test]
        public void RemoveItem_ReportsHowManyItActuallyRemoved()
        {
            var inventory = context.World.Player.Inventory;
            InventoryOps.AddItem(inventory, definitions, "item_battery", 3);

            Assert.AreEqual(3, InventoryOps.RemoveItem(inventory, "item_battery", 10));
            Assert.IsEmpty(inventory.Slots);
        }

        [Test]
        public void TotalWeight_UsesDefinitionValues()
        {
            var inventory = context.World.Player.Inventory;
            InventoryOps.AddItem(inventory, definitions, "item_water_bottle", 2); // 0.8 kg mỗi chai

            Assert.AreEqual(1.6f, InventoryOps.TotalWeightKg(inventory, definitions), 0.001f);
        }

        [Test]
        public void ItemInstance_DoesNotStackAcrossDifferentCondition()
        {
            var fresh = new ItemInstanceState { ItemId = "item_water_bottle", Condition = 100f };
            var worn = new ItemInstanceState { ItemId = "item_water_bottle", Condition = 40f };

            Assert.IsFalse(fresh.CanStackWith(worn));
            Assert.IsTrue(fresh.CanStackWith(fresh.Clone()));
        }
    }
}
