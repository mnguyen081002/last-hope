using System;
using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    public readonly struct PowerDemandEntry
    {
        public readonly string ModuleInstanceId;
        public readonly float Demand;
        public readonly PowerPriority Priority;

        public PowerDemandEntry(string moduleInstanceId, float demand, PowerPriority priority)
        {
            ModuleInstanceId = moduleInstanceId;
            Demand = demand;
            Priority = priority;
        }
    }

    public readonly struct PowerAllocationResult
    {
        public readonly IReadOnlyDictionary<string, bool> Powered;
        public readonly float NewBatteryCharge;

        public PowerAllocationResult(IReadOnlyDictionary<string, bool> powered, float newBatteryCharge)
        {
            Powered = powered;
            NewBatteryCharge = newBatteryCharge;
        }
    }

    /// <summary>
    /// Pure power allocation (main-shelter-design.md §19-20, S12): City Grid first, then Battery;
    /// when supply is short, Disabled modules never get power and the rest are served
    /// highest-priority-first. One long-tick (10-minute) resolution — same convention
    /// ConditionSystem uses for hunger/thirst accrual.
    /// </summary>
    public static class PowerRules
    {
        private const int LongTickMinutes = 10;

        public static PowerAllocationResult Allocate(
            bool gridAvailable, float batteryCharge, IReadOnlyList<PowerDemandEntry> demands, PowerBalance cfg)
        {
            var sorted = new List<PowerDemandEntry>(demands);
            sorted.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Critical first

            var powered = new Dictionary<string, bool>();
            float gridCapacity = gridAvailable ? cfg.GridSupply : 0f;
            float batteryCapacityUnits = Math.Min(cfg.BatteryMaxDischargePerLongTick, batteryCharge) / LongTickMinutes;

            float gridUsed = 0f;
            float batteryUsed = 0f;

            foreach (var entry in sorted)
            {
                if (entry.Priority == PowerPriority.Disabled)
                {
                    powered[entry.ModuleInstanceId] = false;
                    continue;
                }

                float remainingGrid = gridCapacity - gridUsed;
                float remainingBattery = batteryCapacityUnits - batteryUsed;

                if (entry.Demand <= remainingGrid + remainingBattery)
                {
                    float fromGrid = Math.Min(entry.Demand, remainingGrid);
                    gridUsed += fromGrid;
                    batteryUsed += entry.Demand - fromGrid;
                    powered[entry.ModuleInstanceId] = true;
                }
                else
                {
                    powered[entry.ModuleInstanceId] = false;
                }
            }

            float newCharge = batteryCharge - batteryUsed * LongTickMinutes;
            if (gridAvailable && gridUsed < cfg.GridSupply)
                newCharge += cfg.BatteryChargeRatePerLongTick;

            newCharge = Math.Max(0f, Math.Min(cfg.BatteryMaxCharge, newCharge));

            return new PowerAllocationResult(powered, newCharge);
        }
    }
}
