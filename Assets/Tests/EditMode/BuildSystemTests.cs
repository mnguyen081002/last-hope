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
        const string PumpSlot = "slot_utility_area_1";
        const string StorageSlot = "slot_upper_living_1";

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
        public void CanStartConstruction_WrongZone_IsRejected()
        {
            var reason = BuildSystem.CanStartConstruction(world, definitions, StorageSlot, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.WrongZone, reason);
        }

        [Test]
        public void CanStartConstruction_NotEnoughMaterials_IsRejected()
        {
            var reason = BuildSystem.CanStartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.NotEnoughMaterials, reason);
        }

        [Test]
        public void CanStartConstruction_Valid_ReturnsNone()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            var reason = BuildSystem.CanStartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.None, reason);
        }

        [Test]
        public void StartConstruction_DeductsMaterials_SetsConstructionState()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);

            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"));
            Assert.AreEqual(PumpSlot, world.Shelter.Construction.SlotId);
            Assert.AreEqual(definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes,
                world.Shelter.Construction.MinutesRemaining, 0.001f);
        }

        [Test]
        public void CanStartConstruction_SlotOccupied_IsRejected()
        {
            world.Shelter.BuildSlots[PumpSlot] = new BuiltModuleState { ModuleId = ShelterModuleIds.Pump };
            var reason = BuildSystem.CanStartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);
            Assert.AreEqual(BuildRejectReason.SlotOccupied, reason);
        }

        [Test]
        public void CanStartConstruction_AnotherConstructionInProgress_IsRejected()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);

            var reason = BuildSystem.CanStartConstruction(
                world, definitions, "slot_water_processing_1", ShelterModuleIds.Purifier);
            Assert.AreEqual(BuildRejectReason.ConstructionInProgress, reason);
        }

        [Test]
        public void ApplyShortTick_DecrementsMinutes_CompletesAtZero()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);
            int minutes = definitions.GetModule(ShelterModuleIds.Pump).BuildMinutes;

            (string SlotId, string ModuleId)? completed = null;
            for (int i = 0; i < minutes - 1; i++)
            {
                completed = BuildSystem.ApplyShortTick(world);
                Assert.IsFalse(completed.HasValue, $"Chưa xong ở phút {i + 1}.");
            }

            completed = BuildSystem.ApplyShortTick(world);
            Assert.IsTrue(completed.HasValue);
            Assert.AreEqual(PumpSlot, completed.Value.SlotId);
            Assert.AreEqual(ShelterModuleIds.Pump, completed.Value.ModuleId);
            Assert.IsNull(world.Shelter.Construction);
            Assert.AreEqual(ShelterModuleIds.Pump, world.Shelter.BuildSlots[PumpSlot].ModuleId);
        }

        [Test]
        public void ApplyShortTick_Paused_DoesNotProgress()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);
            BuildSystem.SetPaused(world, PumpSlot, true);
            float before = world.Shelter.Construction.MinutesRemaining;

            BuildSystem.ApplyShortTick(world);

            Assert.AreEqual(before, world.Shelter.Construction.MinutesRemaining);
        }

        [Test]
        public void CancelConstruction_ClearsWithoutRefund()
        {
            GiveMaterials(ShelterModuleIds.Pump);
            BuildSystem.StartConstruction(world, definitions, PumpSlot, ShelterModuleIds.Pump);

            bool cancelled = BuildSystem.CancelConstruction(world, PumpSlot);

            Assert.IsTrue(cancelled);
            Assert.IsNull(world.Shelter.Construction);
            var storage = world.GetOrCreateLocation(ShelterModuleIds.LocationId).StorageContainer;
            Assert.AreEqual(0, InventoryOps.CountOf(storage, "item_pump_part"), "Không hoàn vật liệu.");
        }

        [Test]
        public void DismantleModule_RemovesFromBuildSlots()
        {
            world.Shelter.BuildSlots[PumpSlot] = new BuiltModuleState { ModuleId = ShelterModuleIds.Pump };

            bool dismantled = BuildSystem.DismantleModule(world, PumpSlot);

            Assert.IsTrue(dismantled);
            Assert.IsFalse(world.Shelter.BuildSlots.ContainsKey(PumpSlot));
        }
    }
}
