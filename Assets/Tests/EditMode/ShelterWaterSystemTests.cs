using System.IO;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class ShelterWaterSystemTests
    {
        DefinitionRegistry definitions;
        ShelterState shelter;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            shelter = new ShelterState();
        }

        [Test]
        public void ApplyLongTick_BlackRain_NoModules_InflowMinusPassiveDrain()
        {
            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.BlackRain);

            // inflow_by_rain_intensity[2]=4, passive_drain=2 -> gain=2.
            Assert.AreEqual(2f, shelter.WaterIntrusion, 0.001f);
        }

        [Test]
        public void ApplyLongTick_ClampsAtZero_WhenDrainExceedsInflow()
        {
            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry); // inflow[0]=0

            Assert.AreEqual(0f, shelter.WaterIntrusion);
        }

        [Test]
        public void ApplyLongTick_Barrier_ReducesInflow_AndDecaysDurability()
        {
            shelter.BuildSlots["slot_shelter_entrance_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Barrier, Durability = 100f };

            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.BlackRain);

            // inflow 4 * (1 - 0.7) = 1.2; gain = 1.2 - 2 = -0.8 -> clamp 0.
            Assert.AreEqual(0f, shelter.WaterIntrusion, 0.001f);
            Assert.AreEqual(98f, shelter.BuildSlots["slot_shelter_entrance_1"].Durability, 0.001f);
        }

        [Test]
        public void ApplyLongTick_Pump_PoweredAndNotJammed_ReducesWater()
        {
            shelter.WaterIntrusion = 20f;
            shelter.BuildSlots["slot_utility_area_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Powered = true };

            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry); // inflow=0

            // 20 + 0 - pump(6) - passive(2) = 12.
            Assert.AreEqual(12f, shelter.WaterIntrusion, 0.001f);
        }

        [Test]
        public void ApplyLongTick_Pump_NotPowered_NoEffect()
        {
            shelter.WaterIntrusion = 20f;
            shelter.BuildSlots["slot_utility_area_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Powered = false };

            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry);

            Assert.AreEqual(18f, shelter.WaterIntrusion, 0.001f); // chỉ passive drain -2.
        }

        [Test]
        public void ApplyLongTick_GroundFlooded_DisablesPumpOutput()
        {
            shelter.WaterIntrusion = 70f; // >= deep_threshold(60)
            shelter.BuildSlots["slot_utility_area_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Pump, Powered = true };

            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry);

            // Không có pump output vì Ground Floor bị khóa — chỉ passive drain.
            Assert.AreEqual(68f, shelter.WaterIntrusion, 0.001f);
        }

        [Test]
        public void ApplyLongTick_WaterIntake_AccumulatesUntreatedWater()
        {
            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry);

            // intake_untreated_per_hour=1 * 10/60 phút.
            Assert.AreEqual(1f * 10f / 60f, shelter.UntreatedWater, 0.001f);
        }

        [Test]
        public void Purifier_CompletesBatch_AfterEnoughLongTicks()
        {
            shelter.UntreatedWater = 10f;
            shelter.BuildSlots["slot_water_processing_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Purifier, Powered = true };

            for (int i = 0; i < 6; i++) // 6 × 10 phút = 60 phút = purify_batch_minutes
            {
                ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry);
            }

            Assert.AreEqual(3f, shelter.CleanWater, 0.001f);
            Assert.Less(shelter.PurifierFilterDurability, 100f);
        }

        [Test]
        public void Purifier_NotEnoughUntreatedWater_DoesNotProgress()
        {
            shelter.UntreatedWater = 1f; // < purify_batch_size (3)
            shelter.BuildSlots["slot_water_processing_1"] =
                new BuiltModuleState { ModuleId = ShelterModuleIds.Purifier, Powered = true };

            ShelterWaterSystem.ApplyLongTick(shelter, definitions, DisasterPhase.Dry);

            Assert.AreEqual(0f, shelter.PurifierBatchMinutes);
        }

        [TestCase(0f, "Dry")]
        [TestCase(15f, "Damp")]
        [TestCase(45f, "Shallow")]
        [TestCase(70f, "Deep")]
        [TestCase(90f, "Critical")]
        public void WaterIntrusionLevel_MatchesThresholds(float value, string expected)
        {
            Assert.AreEqual(expected, ShelterWaterSystem.WaterIntrusionLevel(value, definitions.Balance.Shelter));
        }
    }
}
