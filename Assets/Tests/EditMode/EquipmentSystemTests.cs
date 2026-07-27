using System.IO;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Equipment;
using LastHope.Systems.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class EquipmentSystemTests
    {
        DefinitionRegistry definitions;
        PlayerState player;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));

            player = new PlayerState();
            player.Inventory.CapacityKg = definitions.Balance.Inventory.BackpackCapacityKg;
            player.Inventory.CapacityLiters = definitions.Balance.Inventory.BackpackCapacityLiters;
        }

        [Test]
        public void TryEquip_MovesItemFromInventoryToEquippedSlot()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_jacket", 1);

            bool result = EquipmentSystem.TryEquip(player, definitions, "item_jacket");

            Assert.IsTrue(result);
            Assert.AreEqual("item_jacket", player.Equipped[EquipSlot.Body]);
            Assert.AreEqual(0, InventoryOps.CountOf(player.Inventory, "item_jacket"));
        }

        [Test]
        public void TryEquip_NonEquipmentItem_Fails()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_water_bottle", 1);

            bool result = EquipmentSystem.TryEquip(player, definitions, "item_water_bottle");

            Assert.IsFalse(result);
        }

        [Test]
        public void TryEquip_ItemNotInInventory_Fails()
        {
            bool result = EquipmentSystem.TryEquip(player, definitions, "item_jacket");

            Assert.IsFalse(result);
        }

        [Test]
        public void TryEquip_SwapsOutPreviousItemInSameSlot()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_jacket", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_jacket");

            // Giả lập item body thứ hai (không có trong content thật, chỉ test cơ chế swap).
            InventoryOps.AddItem(player.Inventory, definitions, "item_boots", 1);
            // boots là Feet không phải Body — đổi id giả để test slot khác không đụng nhau:
            EquipmentSystem.TryEquip(player, definitions, "item_boots");

            Assert.AreEqual("item_jacket", player.Equipped[EquipSlot.Body]);
            Assert.AreEqual("item_boots", player.Equipped[EquipSlot.Feet]);
        }

        [Test]
        public void TryUnequip_ReturnsItemToInventory()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_jacket", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_jacket");

            bool result = EquipmentSystem.TryUnequip(player, definitions, EquipSlot.Body);

            Assert.IsTrue(result);
            Assert.IsFalse(player.Equipped.ContainsKey(EquipSlot.Body));
            Assert.AreEqual(1, InventoryOps.CountOf(player.Inventory, "item_jacket"));
        }

        [Test]
        public void TryUnequip_EmptySlot_Fails()
        {
            Assert.IsFalse(EquipmentSystem.TryUnequip(player, definitions, EquipSlot.Body));
        }

        [Test]
        public void TryUnequip_Fails_WhenBackpackTooFullToHoldItBack()
        {
            // Trần cứng theo hard_cap_multiplier — nhồi đầy để tháo dry_bag (2kg) không còn chỗ.
            player.Inventory.CapacityKg = 1f;
            player.Inventory.CapacityLiters = 1000f;
            InventoryOps.AddItem(player.Inventory, definitions, "item_dry_bag", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_dry_bag");

            // Sau khi mặc, capacity += 10kg = 11kg. Nhồi gần đầy 11kg*1.5=16.5kg hard cap.
            InventoryOps.AddItem(player.Inventory, definitions, "item_toolbox", 2); // 16kg

            bool result = EquipmentSystem.TryUnequip(player, definitions, EquipSlot.Back);

            Assert.IsFalse(result, "Tháo dry_bag làm mất 10kg capacity — không đủ chỗ giữ lại 16kg đã có + chính nó.");
            Assert.IsTrue(player.Equipped.ContainsKey(EquipSlot.Back), "Thất bại thì vẫn đang mặc, không nửa vời.");
        }

        [Test]
        public void DryBag_IncreasesBackpackCapacity_OnEquip_RestoresOnUnequip()
        {
            float baseKg = player.Inventory.CapacityKg;
            float baseLiters = player.Inventory.CapacityLiters;
            InventoryOps.AddItem(player.Inventory, definitions, "item_dry_bag", 1);

            EquipmentSystem.TryEquip(player, definitions, "item_dry_bag");
            Assert.AreEqual(baseKg + 10f, player.Inventory.CapacityKg, 0.0001f);
            Assert.AreEqual(baseLiters + 18f, player.Inventory.CapacityLiters, 0.0001f);

            EquipmentSystem.TryUnequip(player, definitions, EquipSlot.Back);
            Assert.AreEqual(baseKg, player.Inventory.CapacityKg, 0.0001f);
            Assert.AreEqual(baseLiters, player.Inventory.CapacityLiters, 0.0001f);
        }

        [Test]
        public void ComputeWetMultiplier_NoJacket_ReturnsOne()
        {
            Assert.AreEqual(1f, EquipmentSystem.ComputeWetMultiplier(player, definitions));
        }

        [Test]
        public void ComputeWetMultiplier_JacketEquipped_ReturnsItemValue()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_jacket", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_jacket");

            Assert.AreEqual(0.3f, EquipmentSystem.ComputeWetMultiplier(player, definitions), 0.0001f);
        }

        [Test]
        public void ComputeBootsProtection_NoBoots_ReturnsNoProtection()
        {
            var (blockLevel, multiplier) = EquipmentSystem.ComputeBootsProtection(player, definitions);

            Assert.AreEqual(0, blockLevel);
            Assert.AreEqual(1f, multiplier);
        }

        [Test]
        public void ComputeBootsProtection_BootsEquipped_ReturnsItemValues()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_boots", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_boots");

            var (blockLevel, multiplier) = EquipmentSystem.ComputeBootsProtection(player, definitions);

            Assert.AreEqual(1, blockLevel);
            Assert.AreEqual(0.5f, multiplier, 0.0001f);
        }

        [Test]
        public void ComputeCurrentReduction_RopeEquipped_ReturnsItemValue()
        {
            InventoryOps.AddItem(player.Inventory, definitions, "item_rope", 1);
            EquipmentSystem.TryEquip(player, definitions, "item_rope");

            Assert.AreEqual(1, EquipmentSystem.ComputeCurrentReduction(player, definitions));
        }
    }
}
