using System.IO;
using LastHope.Data;
using LastHope.Data.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class DefinitionLoaderTests
    {
        static string RealDefinitionsPath =>
            Path.Combine(Application.streamingAssetsPath, "Definitions");

        string tempDir;

        [TearDown]
        public void TearDown()
        {
            if (tempDir != null && Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            tempDir = null;
        }

        void NewTempDir()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "lasthope_defs_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
        }

        void Write(string fileName, string content) =>
            File.WriteAllText(Path.Combine(tempDir, fileName), content);

        // ---------- Content thật ----------

        [Test]
        public void RealContent_LoadsWithoutErrors()
        {
            var registry = DefinitionLoader.LoadFromDirectory(RealDefinitionsPath);

            Assert.AreEqual("0.14.0", registry.DefinitionVersion);
            Assert.AreEqual(16, registry.Items.Count);
            Assert.AreEqual(4, registry.Locations.Count);
            Assert.AreEqual(3, registry.Routes.Count);
            Assert.AreEqual(10, registry.SearchPoints.Count);
        }

        [Test]
        public void RealContent_ParsesSnakeCaseFieldsAndEnums()
        {
            var registry = DefinitionLoader.LoadFromDirectory(RealDefinitionsPath);

            var water = registry.GetItem("item_water_bottle");
            Assert.AreEqual(ItemCategory.Water, water.Category);
            Assert.AreEqual(0.8f, water.BaseWeightKg, 0.0001f);
            Assert.AreEqual(4, water.MaxStackSize);
            Assert.AreEqual(-40f, water.UseEffects.Thirst, 0.0001f);

            var container = registry.GetItem("item_water_container_20l");
            Assert.IsTrue(container.TwoHandCarry);

            var boots = registry.GetItem("item_boots");
            Assert.AreEqual(EquipSlot.Feet, boots.EquipSlot);
            Assert.AreEqual(1, boots.Protection.ExposureBlockLevel);
        }

        [Test]
        public void RealContent_ParsesBalanceAndReferences()
        {
            var registry = DefinitionLoader.LoadFromDirectory(RealDefinitionsPath);

            Assert.AreEqual(15f, registry.Balance.Inventory.BackpackCapacityKg, 0.0001f);
            Assert.AreEqual(1.5f, registry.Balance.Travel.LoadFactorHeavy, 0.0001f);
            Assert.AreEqual("location_shelter", registry.Balance.NewGame.StartLocationId);

            var route = registry.GetRoute("route_shelter_store");
            Assert.AreEqual(25, route.TravelMinutes);
            Assert.AreEqual("location_convenience_store", route.OtherEnd("location_shelter"));

            var shelter = registry.GetLocation("location_shelter");
            Assert.IsTrue(shelter.IsShelter);
            Assert.AreEqual("20_MainShelter", shelter.SceneName);
        }

        // ---------- Validate gom lỗi ----------

        [Test]
        public void DuplicateId_IsReported()
        {
            NewTempDir();
            Write("items_test.json",
                @"[{""id"":""item_a"",""category"":""food""},{""id"":""item_a"",""category"":""food""}]");

            var ex = Assert.Throws<DefinitionLoadException>(
                () => DefinitionLoader.LoadFromDirectory(tempDir));

            StringAssert.Contains("ID trùng 'item_a'", string.Join("\n", ex.Errors));
        }

        [Test]
        public void DanglingReferences_AreAllReported_NotJustTheFirst()
        {
            NewTempDir();
            Write("items_test.json", @"[{""id"":""item_a"",""category"":""food""}]");
            Write("locations_test.json",
                @"[{""id"":""loc_a"",""connected_route_ids"":[""route_missing""],""search_point_ids"":[""sp_missing""]}]");
            Write("routes_test.json",
                @"[{""id"":""route_a"",""from_location_id"":""loc_ghost"",""to_location_id"":""loc_a""}]");
            Write("searchpoints_test.json",
                @"[{""id"":""sp_a"",""location_id"":""loc_a"",""loot_table"":[{""item_id"":""item_ghost""}]}]");

            var ex = Assert.Throws<DefinitionLoadException>(
                () => DefinitionLoader.LoadFromDirectory(tempDir));

            string all = string.Join("\n", ex.Errors);
            // Fail-first sẽ chỉ báo lỗi đầu tiên — đây là điểm mấu chốt cần giữ.
            Assert.GreaterOrEqual(ex.Errors.Count, 4, all);
            StringAssert.Contains("route_missing", all);
            StringAssert.Contains("sp_missing", all);
            StringAssert.Contains("loc_ghost", all);
            StringAssert.Contains("item_ghost", all);
        }

        [Test]
        public void MissingId_IsReported()
        {
            NewTempDir();
            Write("items_test.json", @"[{""category"":""food""}]");

            var ex = Assert.Throws<DefinitionLoadException>(
                () => DefinitionLoader.LoadFromDirectory(tempDir));

            StringAssert.Contains("thiếu 'id'", string.Join("\n", ex.Errors));
        }

        [Test]
        public void UnknownFilePrefix_IsReported()
        {
            NewTempDir();
            Write("mystery_test.json", "[]");

            var ex = Assert.Throws<DefinitionLoadException>(
                () => DefinitionLoader.LoadFromDirectory(tempDir));

            StringAssert.Contains("không nhận diện được", string.Join("\n", ex.Errors));
        }

        [Test]
        public void DeferredPrefixes_AreSkippedSilently()
        {
            NewTempDir();
            Write("items_test.json", @"[{""id"":""item_a"",""category"":""food""}]");
            Write("events_p9.json", @"[{""whatever"": true}]");

            var registry = DefinitionLoader.LoadFromDirectory(tempDir);

            Assert.AreEqual(1, registry.Items.Count);
        }

        [Test]
        public void MinGreaterThanMaxQuantity_IsReported()
        {
            NewTempDir();
            Write("items_test.json", @"[{""id"":""item_a"",""category"":""food""}]");
            Write("locations_test.json", @"[{""id"":""loc_a""}]");
            Write("searchpoints_test.json",
                @"[{""id"":""sp_a"",""location_id"":""loc_a"",""loot_table"":[{""item_id"":""item_a"",""min_quantity"":5,""max_quantity"":2}]}]");

            var ex = Assert.Throws<DefinitionLoadException>(
                () => DefinitionLoader.LoadFromDirectory(tempDir));

            StringAssert.Contains("min > max", string.Join("\n", ex.Errors));
        }
    }
}
