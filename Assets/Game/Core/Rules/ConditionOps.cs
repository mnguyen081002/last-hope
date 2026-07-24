using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure, deterministic mutators for PlayerConditionState — no Unity, no I/O. Systems.Condition
    /// calls these every tick; UseItemCommand calls these when an item's UseEffects apply.
    /// </summary>
    public static class ConditionOps
    {
        public const string StatusWet = "wet";
        public const string StatusCold = "cold";
        public const string StatusBleeding = "bleeding";
        public const string StatusSick = "sick";
        public const string StatusBlackWaterExposure = "black_water_exposure";
        public const string StatusDisoriented = "disoriented";

        public static float Clamp(float value, float min = 0f, float max = 100f) =>
            value < min ? min : value > max ? max : value;

        public static void ApplyHealth(PlayerConditionState c, float delta, float floor = 0f) =>
            c.Health = Clamp(c.Health + delta, floor, 100f);

        public static void ApplyStamina(PlayerConditionState c, float delta) => c.Stamina = Clamp(c.Stamina + delta);
        public static void ApplyFatigue(PlayerConditionState c, float delta) => c.Fatigue = Clamp(c.Fatigue + delta);
        public static void ApplyHunger(PlayerConditionState c, float delta) => c.Hunger = Clamp(c.Hunger + delta);
        public static void ApplyThirst(PlayerConditionState c, float delta) => c.Thirst = Clamp(c.Thirst + delta);

        public static void SetStatusSeverity(PlayerConditionState c, string statusId, float severity, long atMinute)
        {
            severity = Clamp(severity);
            if (severity <= 0f)
            {
                c.StatusEffects.Remove(statusId);
                return;
            }

            if (!c.StatusEffects.TryGetValue(statusId, out var status))
            {
                status = new StatusEffectState();
                c.StatusEffects[statusId] = status;
            }
            status.Severity = severity;
            status.AppliedAtMinute = atMinute;
        }

        public static float GetStatusSeverity(PlayerConditionState c, string statusId) =>
            c.StatusEffects.TryGetValue(statusId, out var s) ? s.Severity : 0f;

        public static void AddExposure(PlayerConditionState c, string hazardId, float delta)
        {
            c.Exposures.TryGetValue(hazardId, out float current);
            c.Exposures[hazardId] = System.Math.Max(0f, current + delta);
        }

        public static float GetExposure(PlayerConditionState c, string hazardId) =>
            c.Exposures.TryGetValue(hazardId, out float v) ? v : 0f;

        /// <summary>Threshold chain for the "black_water" exposure meter: >=Sick threshold sets
        /// both Sick and BlackWaterExposure statuses, >=BlackWater threshold sets just the latter,
        /// below both clears them. Re-evaluated every long tick so recovering exposure clears
        /// statuses automatically once it drops back under threshold.</summary>
        public static void ApplyExposureStatusChain(PlayerConditionState c, string hazardId, long atMinute, ConditionBalance cfg)
        {
            float exposure = GetExposure(c, hazardId);

            if (exposure >= cfg.SickExposureThreshold)
            {
                SetStatusSeverity(c, StatusSick, 100f, atMinute);
                SetStatusSeverity(c, StatusBlackWaterExposure, 100f, atMinute);
            }
            else if (exposure >= cfg.BlackWaterExposureThreshold)
            {
                SetStatusSeverity(c, StatusBlackWaterExposure, 100f, atMinute);
                c.StatusEffects.Remove(StatusSick);
            }
            else
            {
                c.StatusEffects.Remove(StatusBlackWaterExposure);
                c.StatusEffects.Remove(StatusSick);
            }
        }

        public static void RecomputeIncapacitation(PlayerConditionState c, ConditionBalance cfg) =>
            c.Incapacitation = c.Health <= cfg.CollapsedHealthThreshold ? IncapacitationState.Collapsed : IncapacitationState.None;

        public static bool IsIncapacitated(PlayerConditionState c) => c.Incapacitation == IncapacitationState.Collapsed;
    }
}
