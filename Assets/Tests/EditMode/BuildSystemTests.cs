using System.IO;
using LastHope.Core.State;
using LastHope.Data;
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

        [Test]
        public void CanPlaceAt_WrongZone_IsRejected()
        {
            // Pump chỉ cho phép ở utility_area, không phải upper_living.
            var reason = BuildSystem.CanPlaceAt(world, definitions, UpperLivingZone, 0f, 0f, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.WrongZone, reason);
        }

        [Test]
        public void CanPlaceAt_OutOfBounds_IsRejected()
        {
            // utility_area bounds (0,0)-(10,8) — (-5,-5) nằm ngoài.
            var reason = BuildSystem.CanPlaceAt(world, definitions, UtilityZone, -5f, -5f, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.OutOfBounds, reason);
        }

        [Test]
        public void CanPlaceAt_NotEnoughMaterials_IsRejected()
        {
            var reason = BuildSystem.CanPlaceAt(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.NotEnoughMaterials, reason);
        }

        [Test]
        public void CanPlaceAt_Valid_ReturnsNone()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            var reason = BuildSystem.CanPlaceAt(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.None, reason);
        }

        [Test]
        public void StartConstruction_DeductsMaterials_SetsConstructionState()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"));
            Assert.AreEqual(UtilityZone, world.Shelter.Construction.ZoneId);
            Assert.AreEqual(X, world.Shelter.Construction.PositionX, 0.001f);
            Assert.AreEqual(Y, world.Shelter.Construction.PositionY, 0.001f);
            Assert.AreEqual(definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes,
                world.Shelter.Construction.MinutesRemaining, 0.001f);
        }

        [Test]
        public void CanPlaceAt_Overlapping_IsRejected()
        {
            world.Shelter.PlacedModules["placed_1"] = new BuiltModuleState
            {
                ModuleId = ShelterModuleIds.ElevatedStorage, ZoneId = UtilityZone, PositionX = X, PositionY = Y,
            };

            var reason = BuildSystem.CanPlaceAt(world, definitions, UtilityZone, X + 0.1f, Y, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.Overlapping, reason);
        }

        [Test]
        public void CanPlaceAt_AnotherConstructionInProgress_IsRejected()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);

            var reason = BuildSystem.CanPlaceAt(
                world, definitions, "water_processing", 5f, -5f, ShelterModuleIds.Purifier);
            Assert.AreEqual(BuildRejectReason.ConstructionInProgress, reason);
        }

        [Test]
        public void ApplyShortTick_DecrementsMinutes_CompletesAtZero()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            int minutes = definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes;

            (string PlacementId, string ModuleId)? completed = null;
            for (int i = 0; i < minutes - 1; i++)
            {
                completed = BuildSystem.ApplyShortTick(world);
                Assert.IsFalse(completed.HasValue, $"Chưa xong ở phút {i + 1}.");
            }

            completed = BuildSystem.ApplyShortTick(world);
            Assert.IsTrue(completed.HasValue);
            Assert.AreEqual(ShelterModuleIds.Pump, completed.Value.ModuleId);
            Assert.IsNull(world.Shelter.Construction);

            var placed = world.Shelter.PlacedModules[completed.Value.PlacementId];
            Assert.AreEqual(ShelterModuleIds.Pump, placed.ModuleId);
            Assert.AreEqual(UtilityZone, placed.ZoneId);
            Assert.AreEqual(X, placed.PositionX, 0.001f);
            Assert.AreEqual(Y, placed.PositionY, 0.001f);
        }

        [Test]
        public void ApplyShortTick_Paused_DoesNotProgress()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);
            BuildSystem.SetPaused(world, true);
            float before = world.Shelter.Construction.MinutesRemaining;

            BuildSystem.ApplyShortTick(world);

            Assert.AreEqual(before, world.Shelter.Construction.MinutesRemaining);
        }

        [Test]
        public void CancelConstruction_ClearsWithoutRefund()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, UtilityZone, X, Y, ShelterModuleIds.Pump);

            bool cancelled = BuildSystem.CancelConstruction(world);

            Assert.IsTrue(cancelled);
            Assert.IsNull(world.Shelter.Construction);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"), "Không hoàn vật liệu.");
        }

        [Test]
        public void DismantleModule_RemovesFromPlacedModules()
        {
            world.Shelter.PlacedModules["placed_1"] = new BuiltModuleState
            {
                ModuleId = ShelterModuleIds.Pump, ZoneId = UtilityZone, PositionX = X, PositionY = Y,
            };

            bool dismantled = BuildSystem.DismantleModule(world, "placed_1");

            Assert.IsTrue(dismantled);
            Assert.IsFalse(world.Shelter.PlacedModules.ContainsKey("placed_1"));
        }
    }
}
