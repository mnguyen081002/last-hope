using System.Collections.Generic;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class PowerRulesTests
    {
        private static PowerBalance Cfg() => new PowerBalance
        {
            GridSupply = 6f,
            BatteryMaxCharge = 360f,
            BatteryMaxDischargePerLongTick = 30f, // 3 units/min * 10 min
            BatteryChargeRatePerLongTick = 20f,
        };

        [Test]
        public void Allocate_GridSufficient_AllPowered()
        {
            var demands = new List<PowerDemandEntry>
            {
                new PowerDemandEntry("pump", 2f, PowerPriority.Normal),
                new PowerDemandEntry("purifier", 2f, PowerPriority.Normal),
            };
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 0f, demands, Cfg());

            Assert.IsTrue(result.Powered["pump"]);
            Assert.IsTrue(result.Powered["purifier"]);
        }

        [Test]
        public void Allocate_GridInsufficient_LowerPriorityDropped()
        {
            var demands = new List<PowerDemandEntry>
            {
                new PowerDemandEntry("critical_module", 5f, PowerPriority.Critical),
                new PowerDemandEntry("normal_module", 5f, PowerPriority.Normal),
            };
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 0f, demands, Cfg());

            Assert.IsTrue(result.Powered["critical_module"]); // 5 of 6 grid
            Assert.IsFalse(result.Powered["normal_module"]); // only 1 grid left, needs 5
        }

        [Test]
        public void Allocate_DisabledPriority_NeverPoweredEvenWithSpareCapacity()
        {
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 2f, PowerPriority.Disabled) };
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 0f, demands, Cfg());

            Assert.IsFalse(result.Powered["m1"]);
        }

        [Test]
        public void Allocate_BatteryCoversShortfall_WhenGridInsufficient()
        {
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 8f, PowerPriority.Critical) }; // grid only has 6
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 100f, demands, Cfg());

            Assert.IsTrue(result.Powered["m1"]); // 6 grid + 2 battery = 8
        }

        [Test]
        public void Allocate_NoGridNoBattery_NothingPowered()
        {
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 2f, PowerPriority.Critical) };
            var result = PowerRules.Allocate(gridAvailable: false, batteryCharge: 0f, demands, Cfg());

            Assert.IsFalse(result.Powered["m1"]);
        }

        [Test]
        public void Allocate_GridSurplus_ChargesBattery()
        {
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 2f, PowerPriority.Normal) }; // 4 spare grid capacity
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 0f, demands, Cfg());

            Assert.AreEqual(20f, result.NewBatteryCharge, 0.01f); // BatteryChargeRatePerLongTick
        }

        [Test]
        public void Allocate_BatteryDischarge_CapsAtMaxDischargeRate_AndDrainsCharge()
        {
            // Demand exceeds grid+max-discharge-rate (6 + 3 = 9 max), so cannot be fully served even
            // with plenty of battery charge stored.
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 10f, PowerPriority.Critical) };
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 300f, demands, Cfg());

            Assert.IsFalse(result.Powered["m1"]);
        }

        [Test]
        public void Allocate_BatteryDrainedByUsage()
        {
            var demands = new List<PowerDemandEntry> { new PowerDemandEntry("m1", 8f, PowerPriority.Critical) }; // 6 grid + 2 battery
            var result = PowerRules.Allocate(gridAvailable: true, batteryCharge: 100f, demands, Cfg());

            Assert.AreEqual(80f, result.NewBatteryCharge, 0.01f); // 100 - 2 units * 10 min
        }
    }
}
