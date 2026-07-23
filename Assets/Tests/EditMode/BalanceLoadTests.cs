using System;
using System.IO;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class BalanceLoadTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "LastHopeBalanceTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            File.WriteAllText(Path.Combine(_tempDir, "manifest.json"), "{\"definition_version\":\"0.0.1\"}");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public void MissingBalanceFile_FallsBackToDefaults()
        {
            var result = DefinitionLoader.Load(_tempDir);

            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            Assert.AreEqual(15f, result.Registry.Balance.Inventory.BackpackCapacityKg);
            Assert.AreEqual(25f, result.Registry.Balance.Inventory.BackpackCapacityLiters);
        }

        [Test]
        public void ValidBalanceFile_OverridesDefaults()
        {
            File.WriteAllText(Path.Combine(_tempDir, "balance.json"), @"{
                ""inventory"": { ""backpack_capacity_kg"": 20, ""backpack_capacity_liters"": 30 },
                ""travel"": { ""load_factor_heavy"": 2.0 },
                ""new_game"": { ""start_location_id"": ""location_test"" }
            }");

            var result = DefinitionLoader.Load(_tempDir);

            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            Assert.AreEqual(20f, result.Registry.Balance.Inventory.BackpackCapacityKg);
            Assert.AreEqual(30f, result.Registry.Balance.Inventory.BackpackCapacityLiters);
            Assert.AreEqual(2.0f, result.Registry.Balance.Travel.LoadFactorHeavy);
            Assert.AreEqual("location_test", result.Registry.Balance.NewGame.StartLocationId);
        }

        [Test]
        public void UnparsableBalanceFile_FallsBackToDefaults_NotHardError()
        {
            File.WriteAllText(Path.Combine(_tempDir, "balance.json"), "{ not valid json");

            var result = DefinitionLoader.Load(_tempDir);

            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            Assert.AreEqual(15f, result.Registry.Balance.Inventory.BackpackCapacityKg);
        }
    }
}
