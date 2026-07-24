using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure Water Intrusion math (main-shelter-design.md §21-22, S10) — one formula for
    /// WaterIntrusionSystem's live tick today; reused as-is by any future forecast UI, same
    /// reason HazardRules.EvaluateRoute is shared between HazardSystem and ReturnWindowCalculator.
    /// </summary>
    public static class WaterIntrusionRules
    {
        /// <summary>units_delta = inflow(phase, blocked 70% by an active Barrier, + backflow) -
        /// passive_drain - pump_output.</summary>
        public static float ComputeDelta(int rainIntensity, bool backflowActive, int activePumpCount, bool hasActiveBarrier, ShelterBalance cfg)
        {
            float[] table = cfg.InflowByRainIntensity;
            int index = rainIntensity < 0 ? 0 : rainIntensity >= table.Length ? table.Length - 1 : rainIntensity;
            float inflow = table.Length == 0 ? 0f : table[index];
            if (hasActiveBarrier) inflow *= 1f - cfg.BarrierBlockFraction;
            if (backflowActive) inflow += cfg.BackflowInflow;

            float passiveDrain = backflowActive ? 0f : cfg.PassiveDrainPerLongTick;
            float pumpOutput = activePumpCount * cfg.PumpOutputPerLongTick;

            return inflow - passiveDrain - pumpOutput;
        }

        public static WaterIntrusionLevel LevelFor(float units, ShelterBalance cfg)
        {
            if (units >= cfg.CriticalThreshold) return WaterIntrusionLevel.Critical;
            if (units >= cfg.DeepThreshold) return WaterIntrusionLevel.Deep;
            if (units >= cfg.ShallowThreshold) return WaterIntrusionLevel.Shallow;
            if (units >= cfg.DampThreshold) return WaterIntrusionLevel.Damp;
            return WaterIntrusionLevel.Dry;
        }

        public static float Clamp01To100(float units) => units < 0f ? 0f : units > 100f ? 100f : units;
    }
}
