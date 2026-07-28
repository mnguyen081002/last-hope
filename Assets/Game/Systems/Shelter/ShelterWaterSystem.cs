using System.Linq;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Data.Definitions;
using UnityEngine;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Water Intrusion (BL-P3-02/05) + Water Intake/Purifier (BL-P3-09/12). Chạy mỗi Long
    /// Tick, sau <see cref="PowerSystem.Allocate"/> (Pump/Purifier cần biết mình có điện
    /// không trước khi tính output).
    /// </summary>
    public static class ShelterWaterSystem
    {
        public static void ApplyLongTick(ShelterState shelter, DefinitionRegistry definitions, DisasterPhase phase)
        {
            var balance = definitions.Balance.Shelter;
            var waterBalance = definitions.Balance.Water;

            int phaseIndex = Mathf.Min((int)phase, balance.InflowByRainIntensity.Length - 1);
            float entranceInflow = balance.InflowByRainIntensity[phaseIndex];

            var barrier = FindModule(shelter, ShelterModuleIds.Barrier);
            if (barrier != null && barrier.Durability > 0f)
            {
                entranceInflow *= 1f - balance.BarrierBlockFraction;
                barrier.Durability = Mathf.Max(0f, barrier.Durability - balance.BarrierDurabilityDecayPerLongTick);
            }

            float drainInflow = shelter.DrainBackflowActive ? balance.BackflowInflow : 0f;

            // Ground Floor bị khóa (mục 22 design doc) — Pump/Purifier đều ở Zone tầng Ground
            // trong content hiện có (utility_area/water_processing), nên dùng chung 1 cờ.
            bool groundFlooded = shelter.WaterIntrusion >= balance.DeepThreshold;

            float pumpOutput = 0f;
            var pump = FindModule(shelter, ShelterModuleIds.Pump);
            if (!groundFlooded && pump != null && pump.Powered && !pump.IsJammed)
            {
                pumpOutput = balance.PumpOutputPerLongTick;
            }

            float gain = entranceInflow + drainInflow - pumpOutput - balance.PassiveDrainPerLongTick;
            shelter.WaterIntrusion = Mathf.Clamp(shelter.WaterIntrusion + gain, 0f, 100f);

            // Water Intake — thụ động, quy đổi từ tốc độ theo giờ sang mỗi Long Tick (10 phút).
            shelter.UntreatedWater += waterBalance.IntakeUntreatedPerHour
                * TickScheduler.LongTickIntervalMinutes / 60f;

            ApplyPurifier(shelter, waterBalance, groundFlooded);
        }

        static void ApplyPurifier(ShelterState shelter, WaterBalance waterBalance, bool groundFlooded)
        {
            if (groundFlooded) return;
            if (shelter.PurifierFilterDurability <= 0f) return;

            var purifier = FindModule(shelter, ShelterModuleIds.Purifier);
            if (purifier == null || !purifier.Powered) return;
            if (shelter.UntreatedWater < waterBalance.PurifyBatchSize) return;

            shelter.PurifierBatchMinutes += TickScheduler.LongTickIntervalMinutes;
            if (shelter.PurifierBatchMinutes < waterBalance.PurifyBatchMinutes) return;

            shelter.PurifierBatchMinutes = 0f;
            shelter.UntreatedWater -= waterBalance.PurifyBatchSize;
            shelter.CleanWater += waterBalance.PurifyBatchSize;
            shelter.PurifierFilterDurability =
                Mathf.Max(0f, shelter.PurifierFilterDurability - waterBalance.FilterWearPerBatch);
        }

        public static BuiltModuleState FindModule(ShelterState shelter, string moduleId) =>
            shelter.BuildSlots.Values.FirstOrDefault(m => m.ModuleId == moduleId);

        public static string WaterIntrusionLevel(float value, ShelterBalance balance)
        {
            if (value >= balance.CriticalThreshold) return "Critical";
            if (value >= balance.DeepThreshold) return "Deep";
            if (value >= balance.ShallowThreshold) return "Shallow";
            if (value >= balance.DampThreshold) return "Damp";
            return "Dry";
        }
    }
}
