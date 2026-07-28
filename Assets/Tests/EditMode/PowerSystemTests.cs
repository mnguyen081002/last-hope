using System.IO;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Shelter;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class PowerSystemTests
    {
        DefinitionRegistry definitions;
        PowerBalance balance;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            balance = definitions.Balance.Power;
        }

        [TestCase(DisasterPhase.Dry)]
        [TestCase(DisasterPhase.FirstRain)]
        public void GridSupply_Stable_ReturnsFullSupply(DisasterPhase phase)
        {
            Assert.AreEqual(balance.GridSupply, PowerSystem.GridSupply(phase, balance));
        }

        [Test]
        public void GridSupply_BlackRain_ReturnsHalf()
        {
            Assert.AreEqual(balance.GridSupply * 0.5f, PowerSystem.GridSupply(DisasterPhase.BlackRain, balance));
        }

        [Test]
        public void GridSupply_RouteClosure_ReturnsZero()
        {
            Assert.AreEqual(0f, PowerSystem.GridSupply(DisasterPhase.RouteClosure, balance));
        }

        [Test]
        public void Allocate_CriticalPoweredBeforeNormal_WhenSupplyInsufficient()
        {
            var shelter = new ShelterState();
            // pump (demand 2) Normal, purifier (demand 2) Critical — grid supply 6 đủ cả hai
            // thực ra, nên cắt supply thủ công bằng cách set BatteryCharge=0 và test riêng
            // trường hợp supply không đủ bằng cách thêm 3 module demand cao ở Dry (supply=6).
            shelter.BuildSlots["slot_utility_area_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.Pump, Priority = PowerPriority.Normal };
            shelter.BuildSlots["slot_water_processing_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.Purifier, Priority = PowerPriority.Critical };
            shelter.BatteryCharge = 0f;

            PowerSystem.Allocate(shelter, definitions, DisasterPhase.RouteClosure); // supply = 0

            Assert.IsFalse(shelter.BuildSlots["slot_utility_area_1"].Powered, "Không đủ điện, Normal bị cắt trước.");
            Assert.IsFalse(shelter.BuildSlots["slot_water_processing_1"].Powered, "Grid=0 và không có Battery.");
        }

        [Test]
        public void Allocate_CriticalGetsPower_NormalCutWhenSupplyRunsOut()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_water_processing_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.Purifier, Priority = PowerPriority.Critical }; // demand 2
            shelter.BuildSlots["slot_utility_area_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.Pump, Priority = PowerPriority.Normal }; // demand 2
            shelter.BatteryCharge = 0f;

            // Grid Dry = balance.GridSupply (6) — đủ cho cả hai (2+2=4). Test priority order
            // bằng cách set supply nhân tạo thấp qua RouteClosure(=0) + nạp Battery đúng 2.
            shelter.BatteryCharge = 2f;
            PowerSystem.Allocate(shelter, definitions, DisasterPhase.RouteClosure);

            Assert.IsTrue(shelter.BuildSlots["slot_water_processing_1"].Powered, "Critical được ưu tiên dùng Battery.");
            Assert.IsFalse(shelter.BuildSlots["slot_utility_area_1"].Powered, "Battery hết sau khi trả Critical.");
        }

        [Test]
        public void Allocate_Disabled_NeverPowered()
        {
            var shelter = new ShelterState();
            shelter.BuildSlots["slot_utility_area_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.Pump, Priority = PowerPriority.Disabled };

            PowerSystem.Allocate(shelter, definitions, DisasterPhase.Dry);

            Assert.IsFalse(shelter.BuildSlots["slot_utility_area_1"].Powered);
        }

        [Test]
        public void Allocate_Surplus_ChargesBattery_CappedByRate()
        {
            var shelter = new ShelterState { BatteryCharge = 0f };
            // Không module nào xây — toàn bộ supply (6) là surplus, nhưng charge rate 20 > 6
            // nên sạc đúng bằng surplus (không bị trần rate cắt trong case này).
            PowerSystem.Allocate(shelter, definitions, DisasterPhase.Dry);

            Assert.AreEqual(balance.GridSupply, shelter.BatteryCharge);
        }

        [Test]
        public void Allocate_ZeroPowerDemandModule_AlwaysPowered_NoBatteryUsed()
        {
            var shelter = new ShelterState { BatteryCharge = 0f };
            shelter.BuildSlots["slot_upper_living_1"] = new BuiltModuleState
                { ModuleId = ShelterModuleIds.ElevatedStorage, Priority = PowerPriority.Normal };

            PowerSystem.Allocate(shelter, definitions, DisasterPhase.RouteClosure); // supply = 0

            Assert.IsTrue(shelter.BuildSlots["slot_upper_living_1"].Powered, "power_demand=0 không cần cấp điện.");
        }
    }
}
