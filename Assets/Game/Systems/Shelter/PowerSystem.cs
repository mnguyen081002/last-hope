using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Phân bổ điện theo Power Priority (Critical trước). Tự đề xuất 2026-07-28, chưa qua
    /// playtest — xem docs/plans/2026-07-28-p3-shelter-loop.md.
    /// </summary>
    public static class PowerSystem
    {
        /// <summary>
        /// City Grid theo Disaster Phase (rút gọn từ bảng đầy đủ trong 04-main-shelter-design.md
        /// mục 19 — Stable/Stable/Unstable/PartialFailure/Failed ứng với 4 phase hiện có).
        /// </summary>
        public static float GridSupply(DisasterPhase phase, PowerBalance balance) => phase switch
        {
            DisasterPhase.BlackRain => balance.GridSupply * 0.5f,
            DisasterPhase.RouteClosure => 0f,
            _ => balance.GridSupply,
        };

        /// <summary>
        /// Cấp điện cho từng Module đã xây theo thứ tự Priority, xả/sạc Battery phần dư/thiếu.
        /// Ghi thẳng <see cref="BuiltModuleState.Powered"/> — không trả kết quả riêng.
        /// </summary>
        public static void Allocate(
            ShelterState shelter, DefinitionRegistry definitions, DisasterPhase phase)
        {
            var balance = definitions.Balance.Power;
            float supply = GridSupply(phase, balance);

            var ordered = new List<BuiltModuleState>(shelter.PlacedModules.Values);
            ordered.Sort((a, b) => a.Priority.CompareTo(b.Priority)); // Critical(0)..Disabled(3)

            float gridRemaining = supply;
            float batteryBudget = UnityEngine.Mathf.Min(shelter.BatteryCharge, balance.BatteryMaxDischargePerLongTick);
            float batteryUsed = 0f;

            foreach (var module in ordered)
            {
                module.Powered = false;
                if (module.Priority == PowerPriority.Disabled) continue;
                if (!definitions.TryGetModule(module.ModuleId, out var def)) continue;

                float demand = def.PowerDemand;
                if (demand <= 0f)
                {
                    module.Powered = true; // không tiêu thụ điện (vd Elevated Storage).
                    continue;
                }

                if (gridRemaining >= demand)
                {
                    gridRemaining -= demand;
                    module.Powered = true;
                }
                else if (gridRemaining + (batteryBudget - batteryUsed) >= demand)
                {
                    batteryUsed += demand - gridRemaining;
                    gridRemaining = 0f;
                    module.Powered = true;
                }
            }

            shelter.BatteryCharge -= batteryUsed;

            float surplus = UnityEngine.Mathf.Max(0f, gridRemaining);
            float charge = UnityEngine.Mathf.Min(surplus, balance.BatteryChargeRatePerLongTick);
            shelter.BatteryCharge = UnityEngine.Mathf.Min(balance.BatteryMaxCharge, shelter.BatteryCharge + charge);
        }
    }
}
