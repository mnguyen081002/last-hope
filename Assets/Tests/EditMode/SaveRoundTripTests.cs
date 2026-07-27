using System.IO;
using LastHope.Core.Random;
using LastHope.Core.Save;
using LastHope.Core.State;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class SaveRoundTripTests
    {
        const string DefinitionVersion = "0.14.0";

        string directory;
        SaveService service;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "lasthope_saves_" + Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            service = new SaveService(directory, DefinitionVersion);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        static WorldState SampleWorld()
        {
            var world = new WorldState
            {
                WorldTimeMinutes = 1234,
                MasterSeed = 42UL,
                Player = new PlayerState
                {
                    CurrentLocationId = "location_shelter",
                    PositionX = 1.5f,
                    PositionY = -2.25f,
                    Thirst = 30f,
                },
            };
            world.RngStreams[RngService.Loot] = 987654321UL;

            InventoryOpsSeed(world);
            return world;
        }

        static void InventoryOpsSeed(WorldState world)
        {
            world.Player.Inventory.CapacityKg = 15f;
            world.Player.Inventory.CapacityLiters = 25f;
            world.Player.Inventory.Slots.Add(new ItemInstanceState
            {
                ItemId = "item_water_bottle", Quantity = 3,
            });

            var location = world.GetOrCreateLocation("location_convenience_store");
            location.SearchPoints["searchpoint_counter"] = new SearchPointState
            {
                Rolled = true,
                RemainingItems = { new ItemInstanceState { ItemId = "item_battery", Quantity = 2 } },
            };
        }

        [Test]
        public void RoundTrip_ProducesIdenticalCanonicalJson()
        {
            var world = SampleWorld();
            service.Save(world, "manual_0");

            var loaded = service.Load("manual_0");

            Assert.AreEqual(
                WorldStateSerializer.Serialize(world),
                WorldStateSerializer.Serialize(loaded));
        }

        [Test]
        public void RoundTrip_PreservesNestedState()
        {
            service.Save(SampleWorld(), "manual_0");
            var loaded = service.Load("manual_0");

            Assert.AreEqual(1234, loaded.WorldTimeMinutes);
            Assert.AreEqual(987654321UL, loaded.RngStreams[RngService.Loot]);
            Assert.AreEqual(3, loaded.Player.Inventory.Slots[0].Quantity);

            var searchPoint = loaded.Locations["location_convenience_store"]
                .SearchPoints["searchpoint_counter"];
            Assert.IsTrue(searchPoint.Rolled);
            Assert.AreEqual("item_battery", searchPoint.RemainingItems[0].ItemId);
        }

        [Test]
        public void SecondSave_MovesPreviousFileToBak()
        {
            service.Save(SampleWorld(), "manual_0");
            Assert.IsFalse(File.Exists(service.PathForSlot("manual_0") + ".bak"));

            service.Save(SampleWorld(), "manual_0");
            Assert.IsTrue(File.Exists(service.PathForSlot("manual_0") + ".bak"));
        }

        [Test]
        public void TamperedPayload_IsRejectedByChecksum()
        {
            service.Save(SampleWorld(), "manual_0");

            string path = service.PathForSlot("manual_0");
            File.WriteAllText(path, File.ReadAllText(path).Replace("1234", "9999"));

            var ex = Assert.Throws<SaveLoadException>(() => service.Load("manual_0"));
            Assert.AreEqual(SaveLoadError.ChecksumMismatch, ex.Error);
        }

        [Test]
        public void DefinitionVersionMismatch_IsRejected()
        {
            service.Save(SampleWorld(), "manual_0");

            var otherVersion = new SaveService(directory, "9.9.9");
            var ex = Assert.Throws<SaveLoadException>(() => otherVersion.Load("manual_0"));
            Assert.AreEqual(SaveLoadError.DefinitionVersionMismatch, ex.Error);
        }

        [Test]
        public void MissingSlot_IsReportedNotSilentlyReset()
        {
            var ex = Assert.Throws<SaveLoadException>(() => service.Load("manual_0"));
            Assert.AreEqual(SaveLoadError.FileNotFound, ex.Error);
        }

        [Test]
        public void CorruptJson_IsReported()
        {
            File.WriteAllText(service.PathForSlot("manual_0"), "{ khong phai json");

            var ex = Assert.Throws<SaveLoadException>(() => service.Load("manual_0"));
            Assert.AreEqual(SaveLoadError.Corrupt, ex.Error);
        }

        [Test]
        public void Autosave_FillsEmptySlotsThenRotatesToOldest()
        {
            var world = SampleWorld();

            Assert.AreEqual("autosave_0", service.SaveAutosave(world));
            Assert.AreEqual("autosave_1", service.SaveAutosave(world));
            Assert.AreEqual("autosave_2", service.SaveAutosave(world));

            // Hết slot trống → phải đè lên bản cũ nhất, không đè bừa.
            Assert.AreEqual("autosave_0", service.SaveAutosave(world));
        }

        [Test]
        public void RngState_ContinuesAfterSaveLoad_NoRepeat()
        {
            var world = SampleWorld();
            var services = new RngService(world.MasterSeed, world.RngStreams);

            for (int i = 0; i < 25; i++) services.Stream(RngService.Loot).NextULong();
            services.FlushState();
            service.Save(world, "manual_0");

            ulong expected = services.Stream(RngService.Loot).NextULong();

            var loaded = service.Load("manual_0");
            var restored = new RngService(loaded.MasterSeed, loaded.RngStreams);

            Assert.AreEqual(expected, restored.Stream(RngService.Loot).NextULong());
        }
    }
}
