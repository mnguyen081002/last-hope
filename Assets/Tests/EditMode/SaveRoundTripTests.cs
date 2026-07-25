using System;
using System.IO;
using LastHope.Core.Random;
using LastHope.Core.Save;
using LastHope.Core.State;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class SaveRoundTripTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "LastHopeSaveTests_" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        }

        private static WorldState BuildPopulatedWorld()
        {
            var world = new WorldState { WorldTimeMinutes = 4321, RandomSeed = 777 };
            world.PersistentFlags["intro_seen"] = true;
            new RngService(world).GetStream("loot").NextInt(0, 100); // ensure a stream state exists
            world.Player.Inventory.Items["inst_1"] = new ItemInstanceState
            {
                InstanceId = "inst_1",
                ItemId = "item_test",
                Quantity = 3,
            };
            world.ShelterStates["shelter_main"] = new ShelterState
            {
                Id = "shelter_main",
                StructuralIntegrity = 85f,
                LivingCapacity = 2,
                Occupants = 1,
                WaterIntrusion = new WaterIntrusionState { Level = WaterIntrusionLevel.Damp, Units = 12f },
                WaterStocks = new WaterStocksState { Clean = 3f, Untreated = 1f },
                BuildSlots = { ["slot_utility_area_1"] = new BuildSlotState { Locked = false, ModuleInstanceId = null } },
                EventFlags = { "lower_floor_power_locked" },
            };
            world.NpcStates["npc_minh"] = new NpcState
            {
                Id = "npc_minh",
                LocationId = "location_shelter",
                Health = NpcHealthState.Injured,
                Trust = 35,
                Flags = { "recruited" },
            };
            world.Intel.Records["route_shelter_store"] = new IntelRecord
            {
                SubjectId = "route_shelter_store",
                Kind = "route",
                Confidence = IntelConfidence.Confirmed,
                ObservedAtMinute = 120,
                FloodLevel = 1,
                CurrentLevel = 0,
                Closed = false,
            };
            return world;
        }

        [Test]
        public void SaveLoad_CanonicalJsonEqual()
        {
            var service = new SaveService(_tempDir, "0.1.0");
            var world = BuildPopulatedWorld();

            var saveResult = service.SaveToSlot(world, "manual_0");
            Assert.IsTrue(saveResult.Success, saveResult.Error);

            var loadResult = service.Load("manual_0");
            Assert.IsTrue(loadResult.Success, loadResult.Error);

            string beforeJson = WorldStateSerializer.SerializeCanonical(world);
            string afterJson = WorldStateSerializer.SerializeCanonical(loadResult.World);
            Assert.AreEqual(beforeJson, afterJson);
        }

        [Test]
        public void Save_CreatesBackupOfPreviousSlot()
        {
            var service = new SaveService(_tempDir, "0.1.0");
            var world = BuildPopulatedWorld();

            service.SaveToSlot(world, "manual_0");
            string firstContent = File.ReadAllText(Path.Combine(_tempDir, "manual_0.json"));

            world.WorldTimeMinutes = 9999;
            service.SaveToSlot(world, "manual_0");

            string backupPath = Path.Combine(_tempDir, "manual_0.json.bak");
            Assert.IsTrue(File.Exists(backupPath));
            Assert.AreEqual(firstContent, File.ReadAllText(backupPath));
        }

        [Test]
        public void CorruptedChecksum_LoadRejected()
        {
            var service = new SaveService(_tempDir, "0.1.0");
            service.SaveToSlot(BuildPopulatedWorld(), "manual_0");

            string path = Path.Combine(_tempDir, "manual_0.json");
            File.WriteAllText(path, File.ReadAllText(path).Replace("4321", "4322"));

            var loadResult = service.Load("manual_0");
            Assert.IsFalse(loadResult.Success);
            Assert.IsTrue(loadResult.Error.ToLowerInvariant().Contains("checksum"));
        }

        [Test]
        public void DefinitionVersionMismatch_LoadRejected()
        {
            var service = new SaveService(_tempDir, "0.1.0");
            service.SaveToSlot(BuildPopulatedWorld(), "manual_0");

            var otherVersionService = new SaveService(_tempDir, "9.9.9");
            var loadResult = otherVersionService.Load("manual_0");

            Assert.IsFalse(loadResult.Success);
            Assert.IsTrue(loadResult.Error.Contains("0.1.0"));
            Assert.IsTrue(loadResult.Error.Contains("9.9.9"));
        }

        [Test]
        public void AutosaveRotation_OverwritesOldestOfThree()
        {
            var service = new SaveService(_tempDir, "0.1.0");
            var world = BuildPopulatedWorld();

            var r1 = service.Autosave(world);
            var r2 = service.Autosave(world);
            var r3 = service.Autosave(world);
            var r4 = service.Autosave(world);

            Assert.AreEqual("autosave_0", r1.SlotId);
            Assert.AreEqual("autosave_1", r2.SlotId);
            Assert.AreEqual("autosave_2", r3.SlotId);
            Assert.AreEqual("autosave_0", r4.SlotId);
        }
    }
}
