using System.Collections.Generic;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class EquipmentRulesTests
    {
        private static DefinitionRegistry BuildRegistry(Dictionary<string, ItemDefinition> items) =>
            new DefinitionRegistry(
                "test", new BalanceConfig(), items,
                new Dictionary<string, LocationDefinition>(),
                new Dictionary<string, RouteDefinition>(),
                new Dictionary<string, SearchPointDefinition>());

        private static InventoryState InventoryWithEquipped(string slot, string itemId)
        {
            var inv = new InventoryState { OwnerId = "player" };
            inv.Items["inst_1"] = new ItemInstanceState { InstanceId = "inst_1", ItemId = itemId, Quantity = 1 };
            inv.EquipmentSlots[slot] = "inst_1";
            return inv;
        }

        [Test]
        public void SumProtection_NoEquipment_ReturnsZero()
        {
            var registry = BuildRegistry(new Dictionary<string, ItemDefinition>());
            var inv = new InventoryState { OwnerId = "player" };

            Assert.AreEqual(0f, EquipmentRules.SumProtection(inv, registry, "current_reduction"));
            Assert.IsFalse(EquipmentRules.HasProtection(inv, registry, "current_reduction"));
        }

        [Test]
        public void SumProtection_ReadsEquippedItemProtection()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_rope"] = new ItemDefinition { Id = "item_rope", Protection = new Dictionary<string, float> { ["current_reduction"] = 1f } },
            };
            var registry = BuildRegistry(items);
            var inv = InventoryWithEquipped("tool", "item_rope");

            Assert.AreEqual(1f, EquipmentRules.SumProtection(inv, registry, "current_reduction"));
            Assert.IsTrue(EquipmentRules.HasProtection(inv, registry, "current_reduction"));
        }

        [Test]
        public void ResolveTravelProtection_NothingEquipped_ReturnsNone()
        {
            var registry = BuildRegistry(new Dictionary<string, ItemDefinition>());
            var inv = new InventoryState { OwnerId = "player" };

            var result = EquipmentRules.ResolveTravelProtection(inv, registry);

            Assert.AreEqual(0, result.CurrentReduction);
            Assert.AreEqual(1f, result.WetMultiplier);
            Assert.AreEqual(0, result.BootsBlockLevel);
        }

        [Test]
        public void ResolveTravelProtection_CombinesRopeJacketBoots()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                ["item_rope"] = new ItemDefinition { Id = "item_rope", Protection = new Dictionary<string, float> { ["current_reduction"] = 1f } },
                ["item_jacket"] = new ItemDefinition { Id = "item_jacket", Protection = new Dictionary<string, float> { ["wet_multiplier"] = 0.3f } },
                ["item_boots"] = new ItemDefinition { Id = "item_boots", Protection = new Dictionary<string, float> { ["exposure_block_level"] = 1f, ["exposure_medium_multiplier"] = 0.5f } },
            };
            var registry = BuildRegistry(items);
            var inv = new InventoryState { OwnerId = "player" };
            inv.Items["rope_1"] = new ItemInstanceState { InstanceId = "rope_1", ItemId = "item_rope", Quantity = 1 };
            inv.Items["jacket_1"] = new ItemInstanceState { InstanceId = "jacket_1", ItemId = "item_jacket", Quantity = 1 };
            inv.Items["boots_1"] = new ItemInstanceState { InstanceId = "boots_1", ItemId = "item_boots", Quantity = 1 };
            inv.EquipmentSlots["tool"] = "rope_1";
            inv.EquipmentSlots["body"] = "jacket_1";
            inv.EquipmentSlots["feet"] = "boots_1";

            var result = EquipmentRules.ResolveTravelProtection(inv, registry);

            Assert.AreEqual(1, result.CurrentReduction);
            Assert.AreEqual(0.3f, result.WetMultiplier, 0.001f);
            Assert.AreEqual(1, result.BootsBlockLevel);
            Assert.AreEqual(0.5f, result.BootsMediumMultiplier, 0.001f);
        }
    }
}
