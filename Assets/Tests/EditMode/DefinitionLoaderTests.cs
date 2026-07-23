using System.IO;
using LastHope.Data;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class DefinitionLoaderTests
    {
        private static string FixturePath(string name) =>
            Path.Combine(Application.dataPath, "Tests/EditMode/Fixtures", name);

        [Test]
        public void ValidFixture_LoadsAllTypedCollections()
        {
            var result = DefinitionLoader.Load(FixturePath("valid_definitions"));

            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            Assert.AreEqual("0.1.0", result.Registry.DefinitionVersion);
            Assert.AreEqual(2, result.Registry.Items.Count);
            Assert.AreEqual(2, result.Registry.Locations.Count);
            Assert.AreEqual(1, result.Registry.Routes.Count);
            Assert.AreEqual(1, result.Registry.SearchPoints.Count);

            Assert.IsTrue(result.Registry.TryGetItem("item_water_bottle", out var water));
            Assert.AreEqual(0.8f, water.BaseWeightKg);
        }

        [Test]
        public void DuplicateIdsAndDanglingRef_AllReported()
        {
            var result = DefinitionLoader.Load(FixturePath("invalid_definitions"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(3, result.Errors.Count, string.Join("; ", result.Errors));
        }
    }
}
