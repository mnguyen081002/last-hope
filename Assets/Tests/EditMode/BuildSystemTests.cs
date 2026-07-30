using System.IO;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Systems.Inventory;
using LastHope.Systems.Shelter;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class BuildSystemTests
    {
        const string UtilityZone = "utility_area";
        const string UpperLivingZone = "upper_living";
        const float X = 5f, Y = 5f; // trong bounds utility_area (0,0)-(10,8).

        DefinitionRegistry definitions;
        WorldState world;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            world = new WorldState { MasterSeed = 1UL };
        }

        void GiveMaterials(string moduleId)
        {
            var module = definitions.GetModule(moduleId);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            foreach (var pair in module.Materials)
                InventoryOps.AddItem(storage, definitions, pair.Key, pair.Value);
        }

        /// <summary>Packed item là TwoHandCarry (item_packed_*) — phải qua InventorySystem.Add
        /// để đi đúng đường CarriedObjectItemId, không phải InventoryOps.AddItem (chỉ biết Slots).</summary>
        void GivePackedItem(string moduleId, int quantity = 1)
        {
            var module = definitions.GetModule(moduleId);
            InventorySystem.Add(world.Player.Inventory, definitions, module.PackedItemId, quantity);
        }

        // ---------- Production (StartConstruction/CanStartProduction) ----------

        [Test]
        public void CanStartProduction_NotEnoughMaterials_IsRejected()
        {
            var reason = BuildSystem.CanStartProduction(world, definitions, ShelterModuleIds.Pump, out _);
            Assert.AreEqual(BuildRejectReason.NotEnoughMaterials, reason);
        }

        [Test]
        public void CanStartProduction_Valid_ReturnsNone()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            var reason = BuildSystem.CanStartProduction(world, definitions, ShelterModuleIds.Pump, out var module);
            Assert.AreEqual(BuildRejectReason.None, reason);
            Assert.AreEqual(ShelterModuleIds.Pump, module.Id);
        }

        [Test]
        public void CanStartProduction_AnotherConstructionInProgress_IsRejected()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, ShelterModuleIds.Pump);

            GiveMaterials(ShelterModuleIds.Purifier);
            var reason = BuildSystem.CanStartProduction(world, definitions, ShelterModuleIds.Purifier, out _);
            Assert.AreEqual(BuildRejectReason.ConstructionInProgress, reason);
        }

        [Test]
        public void HasEnoughMaterials_ChecksIndependentlyOfConstructionState()
        {
            var module = definitions.GetModule(ShelterModuleIds.Pump);
            Assert.IsFalse(BuildSystem.HasEnoughMaterials(world, module));

            GiveMaterials(ShelterModuleIds.Pump);
            Assert.IsTrue(BuildSystem.HasEnoughMaterials(world, module));
        }

        [Test]
        public void StartConstruction_DeductsMaterials_SetsConstructionState_NoPosition()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, ShelterModuleIds.Pump);

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"));
            Assert.AreEqual(ShelterModuleIds.Pump, world.Shelter.Construction.ModuleId);
            Assert.AreEqual(definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes,
                world.Shelter.Construction.MinutesRemaining, 0.001f);
        }

        [Test]
        public void ApplyShortTick_DecrementsMinutes_CompletesAtZero_AddsReadyToClaim()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, ShelterModuleIds.Pump);
            int minutes = definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes;

            string completedModuleId = null;
            for (int i = 0; i < minutes - 1; i++)
            {
                completedModuleId = BuildSystem.ApplyShortTick(world);
                Assert.IsNull(completedModuleId, $"Chưa xong ở phút {i + 1}.");
            }

            completedModuleId = BuildSystem.ApplyShortTick(world);
            Assert.AreEqual(ShelterModuleIds.Pump, completedModuleId);
            Assert.IsNull(world.Shelter.Construction);
            Assert.AreEqual(1, world.Shelter.ReadyToClaim[ShelterModuleIds.Pump]);
            Assert.AreEqual(0, world.Shelter.PlacedModules.Count, "Production xong không tự đặt vào thế giới.");
        }

        [Test]
        public void ApplyShortTick_Paused_DoesNotProgress()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, ShelterModuleIds.Pump);
            BuildSystem.SetPaused(world, true);
            float before = world.Shelter.Construction.MinutesRemaining;

            BuildSystem.ApplyShortTick(world);

            Assert.AreEqual(before, world.Shelter.Construction.MinutesRemaining);
        }

        [Test]
        public void CancelConstruction_ClearsWithoutRefund()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, ShelterModuleIds.Pump);

            bool cancelled = BuildSystem.CancelConstruction(world);

            Assert.IsTrue(cancelled);
            Assert.IsNull(world.Shelter.Construction);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"), "Không hoàn vật liệu.");
        }

        // ---------- Claim ----------

        [Test]
        public void CanClaim_NothingReady_IsRejected()
        {
            var reason = BuildSystem.CanClaim(world, definitions, ShelterModuleIds.Pump, out _);
            Assert.AreEqual(BuildRejectReason.NothingToClaim, reason);
        }

        [Test]
        public void CanClaim_InventoryFull_IsRejected()
        {
            world.Shelter.ReadyToClaim[ShelterModuleIds.Pump] = 1;
            world.Player.Inventory.CarriedObjectItemId = "item_wood"; // packed item TwoHandCarry — tay đang bận.

            var reason = BuildSystem.CanClaim(world, definitions, ShelterModuleIds.Pump, out _);
            Assert.AreEqual(BuildRejectReason.InventoryFull, reason);
        }

        [Test]
        public void ClaimProduction_DecrementsReadyToClaim_AddsPackedItemToPlayer()
        {
            world.Shelter.ReadyToClaim[ShelterModuleIds.Pump] = 2;

            BuildSystem.ClaimProduction(world, definitions, ShelterModuleIds.Pump);

            Assert.AreEqual(1, world.Shelter.ReadyToClaim[ShelterModuleIds.Pump]);
            Assert.AreEqual("item_packed_pump", world.Player.Inventory.CarriedObjectItemId);
        }

        [Test]
        public void ClaimProduction_LastOne_RemovesKey()
        {
            world.Shelter.ReadyToClaim[ShelterModuleIds.Pump] = 1;

            BuildSystem.ClaimProduction(world, definitions, ShelterModuleIds.Pump);

            Assert.IsFalse(world.Shelter.ReadyToClaim.ContainsKey(ShelterModuleIds.Pump));
        }

        // ---------- Placement hình học (CanRedeployAt) ----------

        [Test]
        public void CanRedeployAt_WrongZone_IsRejected()
        {
            GivePackedItem(ShelterModuleIds.Pump);
            var reason = BuildSystem.CanRedeployAt(world, definitions, UpperLivingZone, 0f, 0f, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.WrongZone, reason);
        }

        [Test]
        public void CanRedeployAt_OutOfBounds_IsRejected()
        {
            GivePackedItem(ShelterModuleIds.Pump);
            var reason = BuildSystem.CanRedeployAt(world, definitions, UtilityZone, -5f, -5f, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.OutOfBounds, reason);
        }

        [Test]
        public void CanRedeployAt_NoPackedModule_IsRejected()
        {
            var reason = BuildSystem.CanRedeployAt(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.NotEnoughPackedModules, reason);
        }

        [Test]
        public void CanRedeployAt_Overlapping_IsRejected()
        {
            world.Shelter.PlacedModules["placed_1"] = new BuiltModuleState
            {
                ModuleId = ShelterModuleIds.ElevatedStorage, ZoneId = UtilityZone, PositionX = X, PositionY = Y,
            };
            GivePackedItem(ShelterModuleIds.Pump);

            var reason = BuildSystem.CanRedeployAt(world, definitions, UtilityZone, X + 0.1f, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.Overlapping, reason);
        }

        [Test]
        public void CanRedeployAt_RotationSwapsRectangularFootprint()
        {
            GivePackedItem(ShelterModuleIds.Pump);

            // Pump r000 là 2×1: tâm x=0.6 khiến cạnh trái vượt BoundsMinX=0.
            Assert.AreEqual(BuildRejectReason.OutOfBounds,
                BuildSystem.CanRedeployAt(world, definitions, UtilityZone, 0.6f, 1.1f,
                    ShelterModuleIds.Pump, rotationQuarterTurns: 0));

            // r090 là 1×2 nên cùng tâm này nằm trọn trong Zone.
            Assert.AreEqual(BuildRejectReason.None,
                BuildSystem.CanRedeployAt(world, definitions, UtilityZone, 0.6f, 1.1f,
                    ShelterModuleIds.Pump, rotationQuarterTurns: 1));
        }

        [Test]
        public void CanRedeployAt_BatteryBankRejectsRotation()
        {
            GivePackedItem(ShelterModuleIds.BatteryBank);

            var reason = BuildSystem.CanRedeployAt(
                world, definitions, UtilityZone, X, Y, ShelterModuleIds.BatteryBank,
                rotationQuarterTurns: 1);

            Assert.AreEqual(BuildRejectReason.RotationNotAllowed, reason);
        }

        [Test]
        public void RedeployModule_Valid_CreatesInstantly_RemovesFromPlayerInventory()
        {
            GivePackedItem(ShelterModuleIds.Pump);

            var reason = BuildSystem.CanRedeployAt(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.None, reason);

            string placementId = BuildSystem.RedeployModule(
                world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump,
                rotationQuarterTurns: 3);

            Assert.IsNull(world.Shelter.Construction, "Redeploy không qua Construction/BuildMinutes.");
            Assert.IsNull(world.Player.Inventory.CarriedObjectItemId, "Packed item (TwoHandCarry) phải được thả tay ra sau khi đặt.");
            var placed = world.Shelter.PlacedModules[placementId];
            Assert.AreEqual(ShelterModuleIds.Pump, placed.ModuleId);
            Assert.AreEqual(UtilityZone, placed.ZoneId);
            Assert.AreEqual(X, placed.PositionX, 0.001f);
            Assert.AreEqual(Y, placed.PositionY, 0.001f);
            Assert.AreEqual(3, placed.RotationQuarterTurns);
        }

        // ---------- Dismantle ----------

        [Test]
        public void CanDismantle_InventoryFull_IsRejected()
        {
            world.Shelter.PlacedModules["placed_1"] = new BuiltModuleState
            {
                ModuleId = ShelterModuleIds.Pump, ZoneId = UtilityZone, PositionX = X, PositionY = Y,
            };
            world.Player.Inventory.CarriedObjectItemId = "item_wood"; // packed item TwoHandCarry — tay đang bận.

            var reason = BuildSystem.CanDismantle(world, definitions, "placed_1");
            Assert.AreEqual(BuildRejectReason.InventoryFull, reason);
        }

        [Test]
        public void DismantleModule_RemovesFromPlacedModules_AddsPackedItemToPlayer()
        {
            world.Shelter.PlacedModules["placed_1"] = new BuiltModuleState
            {
                ModuleId = ShelterModuleIds.Pump, ZoneId = UtilityZone, PositionX = X, PositionY = Y,
            };

            bool dismantled = BuildSystem.DismantleModule(world, definitions, "placed_1");

            Assert.IsTrue(dismantled);
            Assert.IsFalse(world.Shelter.PlacedModules.ContainsKey("placed_1"));
            Assert.AreEqual("item_packed_pump", world.Player.Inventory.CarriedObjectItemId);
        }

        // ---------- TryFindModuleByPackedItem ----------

        [Test]
        public void TryFindModuleByPackedItem_KnownPackedItem_ReturnsModule()
        {
            bool found = BuildSystem.TryFindModuleByPackedItem(definitions, "item_packed_pump", out var module);
            Assert.IsTrue(found);
            Assert.AreEqual(ShelterModuleIds.Pump, module.Id);
        }

        [Test]
        public void TryFindModuleByPackedItem_UnknownItem_ReturnsFalse()
        {
            bool found = BuildSystem.TryFindModuleByPackedItem(definitions, "item_wood", out var module);
            Assert.IsFalse(found);
            Assert.IsNull(module);
        }
    }
}
