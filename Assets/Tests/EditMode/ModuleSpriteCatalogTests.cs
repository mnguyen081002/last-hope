using LastHope.Presentation.World;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class ModuleSpriteCatalogTests
    {
        [TestCase("module_barrier")]
        [TestCase("module_pump")]
        [TestCase("module_elevated_storage")]
        [TestCase("module_purifier")]
        public void RotatableModule_HasFourImportedDirectionalSprites(string moduleId)
        {
            Assert.IsTrue(ModuleSpriteCatalog.HasAllDirections(moduleId));

            for (int quarterTurns = 0; quarterTurns < 4; quarterTurns++)
            {
                Sprite sprite = ModuleSpriteCatalog.Load(moduleId, quarterTurns);
                Assert.IsNotNull(sprite, ModuleSpriteCatalog.ResourcePath(moduleId, quarterTurns));
                Assert.AreEqual(512, sprite.texture.width);
                Assert.AreEqual(512, sprite.texture.height);
                Assert.AreEqual(256f, sprite.pixelsPerUnit, 0.001f);
                Assert.AreEqual(0f, sprite.pivot.y, 0.001f, "Pivot phải ở đáy giữa footprint.");
            }
        }

        [Test]
        public void BatteryBank_UsesSingleRotationNeutralSprite()
        {
            Assert.IsNotNull(ModuleSpriteCatalog.Load("module_battery_bank", 0));
            Assert.IsNull(ModuleSpriteCatalog.Load("module_battery_bank", 1));
            Assert.IsFalse(ModuleSpriteCatalog.HasAllDirections("module_battery_bank"));
        }

        [Test]
        public void QuarterTurns_AreNormalizedForResourceNames()
        {
            Assert.AreEqual(
                "Art/ShelterModulesP3/module_pump_r270",
                ModuleSpriteCatalog.ResourcePath("module_pump", -1));
            Assert.AreEqual(
                "Art/ShelterModulesP3/module_pump_r090",
                ModuleSpriteCatalog.ResourcePath("module_pump", 5));
        }
    }
}
