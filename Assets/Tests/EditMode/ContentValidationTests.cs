using System.IO;
using LastHope.Data;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    /// <summary>Loads the actual shipped P1 content (not a test fixture) to catch authoring
    /// mistakes before they reach a build.</summary>
    public class ContentValidationTests
    {
        [Test]
        public void ShippedP1Content_LoadsWithZeroErrors()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Definitions");
            var result = DefinitionLoader.Load(path);

            Assert.IsTrue(result.Success, string.Join("; ", result.Errors));
            Assert.AreEqual("0.5.0", result.Registry.DefinitionVersion);
            Assert.AreEqual(11, result.Registry.Items.Count);
            Assert.AreEqual(2, result.Registry.Locations.Count);
            Assert.AreEqual(1, result.Registry.Routes.Count);
            Assert.AreEqual(6, result.Registry.SearchPoints.Count);
            Assert.AreEqual(4, result.Registry.DisasterPhases.Count);
        }
    }
}
