using System.Collections.Generic;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class BuildRulesTests
    {
        private static Dictionary<string, ShelterZoneDefinition> Zones() => new Dictionary<string, ShelterZoneDefinition>
        {
            ["utility_area"] = new ShelterZoneDefinition { Id = "utility_area", BuildSlotIds = new List<string> { "slot_utility_1" } },
            ["shelter_entrance"] = new ShelterZoneDefinition { Id = "shelter_entrance", BuildSlotIds = new List<string> { "slot_entrance_1" } },
        };

        private static ModuleDefinition PumpModule() => new ModuleDefinition
        {
            Id = "module_pump",
            AllowedZoneIds = new List<string> { "utility_area" },
            Materials = new Dictionary<string, int> { ["item_pump_part"] = 1, ["item_scrap"] = 2 },
        };

        [Test]
        public void TryFindZoneForSlot_Found()
        {
            Assert.IsTrue(BuildRules.TryFindZoneForSlot(Zones(), "slot_utility_1", out var zone));
            Assert.AreEqual("utility_area", zone.Id);
        }

        [Test]
        public void TryFindZoneForSlot_NotFound()
        {
            Assert.IsFalse(BuildRules.TryFindZoneForSlot(Zones(), "slot_nonexistent", out _));
        }

        [Test]
        public void ValidatePlacement_UnknownSlot_InvalidSlot()
        {
            var shelter = new ShelterState();
            Assert.AreEqual(PlacementIssue.InvalidSlot, BuildRules.ValidatePlacement(Zones(), shelter, "slot_utility_1", PumpModule()));
        }

        [Test]
        public void ValidatePlacement_LockedSlot_SlotLocked()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_utility_1"] = new BuildSlotState { Locked = true };
            Assert.AreEqual(PlacementIssue.SlotLocked, BuildRules.ValidatePlacement(Zones(), shelter, "slot_utility_1", PumpModule()));
        }

        [Test]
        public void ValidatePlacement_OccupiedSlot_SlotOccupied()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_utility_1"] = new BuildSlotState { ModuleInstanceId = "existing_module" };
            Assert.AreEqual(PlacementIssue.SlotOccupied, BuildRules.ValidatePlacement(Zones(), shelter, "slot_utility_1", PumpModule()));
        }

        [Test]
        public void ValidatePlacement_WrongZone_WrongZone()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_entrance_1"] = new BuildSlotState();
            Assert.AreEqual(PlacementIssue.WrongZone, BuildRules.ValidatePlacement(Zones(), shelter, "slot_entrance_1", PumpModule()));
        }

        [Test]
        public void ValidatePlacement_ValidEmptySlotRightZone_None()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_utility_1"] = new BuildSlotState();
            Assert.AreEqual(PlacementIssue.None, BuildRules.ValidatePlacement(Zones(), shelter, "slot_utility_1", PumpModule()));
        }

        [Test]
        public void HasMaterials_SufficientAcrossOneStack_True()
        {
            var inv = new InventoryState();
            inv.Items["i1"] = new ItemInstanceState { InstanceId = "i1", ItemId = "item_scrap", Quantity = 5 };
            Assert.IsTrue(BuildRules.HasMaterials(inv, new Dictionary<string, int> { ["item_scrap"] = 5 }));
        }

        [Test]
        public void HasMaterials_SumsAcrossMultipleStacks_True()
        {
            var inv = new InventoryState();
            inv.Items["i1"] = new ItemInstanceState { InstanceId = "i1", ItemId = "item_scrap", Quantity = 2 };
            inv.Items["i2"] = new ItemInstanceState { InstanceId = "i2", ItemId = "item_scrap", Quantity = 3 };
            Assert.IsTrue(BuildRules.HasMaterials(inv, new Dictionary<string, int> { ["item_scrap"] = 5 }));
        }

        [Test]
        public void HasMaterials_Insufficient_False()
        {
            var inv = new InventoryState();
            inv.Items["i1"] = new ItemInstanceState { InstanceId = "i1", ItemId = "item_scrap", Quantity = 1 };
            Assert.IsFalse(BuildRules.HasMaterials(inv, new Dictionary<string, int> { ["item_scrap"] = 2 }));
        }

        [Test]
        public void DismantleRefund_HalvesRoundingDown()
        {
            var refund = BuildRules.DismantleRefund(new Dictionary<string, int> { ["item_wood"] = 5, ["item_scrap"] = 1 });
            Assert.AreEqual(2, refund["item_wood"]);
            Assert.IsFalse(refund.ContainsKey("item_scrap")); // 1/2 = 0, omitted
        }
    }
}
