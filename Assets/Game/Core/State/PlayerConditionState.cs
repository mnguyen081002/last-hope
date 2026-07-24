using System.Collections.Generic;

namespace LastHope.Core.State
{
    public enum IncapacitationState { None, Collapsed }

    public sealed class StatusEffectState
    {
        public float Severity { get; set; }
        public long AppliedAtMinute { get; set; }
    }

    /// <summary>
    /// Player survival stats (BL-P1 S7). Hunger/Thirst are need-meters — 0 means no need,
    /// 100 means fully starved/dehydrated — the opposite convention from Health/Stamina where
    /// 100 is best. Driven by Systems.Condition.ConditionSystem every tick; mutated only through
    /// Core.Rules.ConditionOps so every change stays clamp-safe and deterministic.
    /// </summary>
    public sealed class PlayerConditionState
    {
        public float Health { get; set; } = 100f;
        public float Stamina { get; set; } = 100f;
        public float Fatigue { get; set; }
        public float Hunger { get; set; }
        public float Thirst { get; set; }
        public float BodyTemperatureC { get; set; } = 36.5f;

        public Dictionary<string, StatusEffectState> StatusEffects { get; set; } = new Dictionary<string, StatusEffectState>();

        /// <summary>Cumulative hazard exposure, e.g. "black_water" — not clamped to 0-100 like a
        /// meter, just accumulates until treated (S9 RestAtShelterCommand) or thresholds trigger
        /// a status effect above.</summary>
        public Dictionary<string, float> Exposures { get; set; } = new Dictionary<string, float>();

        public IncapacitationState Incapacitation { get; set; }

        /// <summary>True only during a RestAtShelterCommand.TreatExposure session's FastForward
        /// (S9) — ConditionSystem's LongTick applies bonus "black_water" exposure decay while set,
        /// then RestAtShelterCommand clears it when the session ends.</summary>
        public bool TreatingExposure { get; set; }
    }
}
