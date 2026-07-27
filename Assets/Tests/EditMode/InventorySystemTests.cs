using System.IO;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Systems.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class InventorySystemTests
    {
        DefinitionRegistry definitions;
        InventoryState inventory;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));

            inventory = new InventoryState
            {
                CapacityKg = definitions.Balance.Inventory.BackpackCapacityKg,
                CapacityLiters = definitions.Balance.Inventory.BackpackCapacityLiters,
            };
        }

        [Test]
        public void LoadTier_Normal_UnderLightThreshold()
        {
            // 15kg cap, water bottle 0.8kg -> 5 chai ~2.7kg, còn rất xa 100%.
            InventoryOps.AddItem(inventory, definitions, "item_water_bottle", 5);

            var tier = InventorySystem.ComputeLoadTier(inventory, definitions, definitions.Balance.Inventory);

            Assert.AreEqual(LoadTier.Normal, tier);
        }

        [Test]
        public void LoadTier_CrossesLightThreshold()
        {
            // 8kg toolbox x2 = 16kg > 15kg cap (100%) nhưng < 130% heavy.
            InventoryOps.AddItem(inventory, definitions, "item_toolbox", 2);

            var tier = InventorySystem.ComputeLoadTier(inventory, definitions, definitions.Balance.Inventory);

            Assert.AreEqual(LoadTier.Light, tier);
        }

        [Test]
        public void LoadTier_CrossesHeavyThreshold()
        {
            // CapacityKg 10: toolbox 8kg + 7 water bottle (0.8kg mỗi chai) = 13.6kg = 136%,
            // nằm trong khoảng Heavy [130%, 150%).
            var heavy = new InventoryState { CapacityKg = 10f, CapacityLiters = 1000f };
            heavy.Slots.Add(new ItemInstanceState { ItemId = "item_toolbox", Quantity = 1 });
            heavy.Slots.Add(new ItemInstanceState { ItemId = "item_water_bottle", Quantity = 7 });

            Assert.AreEqual(LoadTier.Heavy,
                InventorySystem.ComputeLoadTier(heavy, definitions, definitions.Balance.Inventory));
        }

        [Test]
        public void LoadTier_HardCap_IsBlocked()
        {
            // CapacityKg 10: toolbox 8kg + 9 water bottle (0.8kg) = 15.2kg = 152% > 150% hard cap.
            var full = new InventoryState { CapacityKg = 10f, CapacityLiters = 1000f };
            full.Slots.Add(new ItemInstanceState { ItemId = "item_toolbox", Quantity = 1 });
            full.Slots.Add(new ItemInstanceState { ItemId = "item_water_bottle", Quantity = 9 });

            Assert.AreEqual(LoadTier.Blocked,
                InventorySystem.ComputeLoadTier(full, definitions, definitions.Balance.Inventory));
        }

        [Test]
        public void SpeedModifier_MatchesBalanceValues()
        {
            var balance = definitions.Balance.Inventory;

            Assert.AreEqual(1f, InventorySystem.SpeedModifierFor(LoadTier.Normal, balance));
            Assert.AreEqual(balance.SpeedModifierLight, InventorySystem.SpeedModifierFor(LoadTier.Light, balance));
            Assert.AreEqual(balance.SpeedModifierHeavy, InventorySystem.SpeedModifierFor(LoadTier.Heavy, balance));
        }

        [Test]
        public void CanAdd_RejectsPastHardCap()
        {
            var full = new InventoryState { CapacityKg = 1f, CapacityLiters = 1000f };

            bool canAdd = InventorySystem.CanAdd(
                full, definitions, definitions.Balance.Inventory, "item_toolbox", 1); // 8kg vào cap 1kg

            Assert.IsFalse(canAdd);
        }

        [Test]
        public void TwoHandCarry_RoutesIntoCarriedSlot_NotBackpack()
        {
            Assert.IsTrue(InventorySystem.CanAdd(
                inventory, definitions, definitions.Balance.Inventory, "item_water_container_20l", 1));

            InventorySystem.Add(inventory, definitions, "item_water_container_20l", 1);

            Assert.AreEqual("item_water_container_20l", inventory.CarriedObjectItemId);
            Assert.IsEmpty(inventory.Slots, "Vật hai tay không được vào Slots.");
        }

        [Test]
        public void TwoHandCarry_CannotCarryTwoAtOnce()
        {
            InventorySystem.Add(inventory, definitions, "item_water_container_20l", 1);

            bool canAddSecond = InventorySystem.CanAdd(
                inventory, definitions, definitions.Balance.Inventory, "item_water_container_20l", 1);

            Assert.IsFalse(canAddSecond);
        }
    }
}
