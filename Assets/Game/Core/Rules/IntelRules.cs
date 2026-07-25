using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure information-age math (S15, world map intel). One decay formula for every reader —
    /// WorldMapPanel today, radio forecast UI later — same sharing convention as HazardRules.
    /// </summary>
    public static class IntelRules
    {
        /// <summary>Confidence decays with information age: one step down after
        /// ConfirmedToReliableMinutes, two after ReliableToUncertainMinutes (baseline 60'/180'),
        /// floored at Unverified. Decay steps apply from whatever confidence the record was
        /// observed at — Reliable radio intel ages the same way Confirmed observation does.</summary>
        public static IntelConfidence EffectiveConfidence(IntelRecord record, long nowMinute, IntelBalance cfg)
        {
            long age = nowMinute - record.ObservedAtMinute;
            int steps = age >= cfg.ReliableToUncertainMinutes ? 2 : age >= cfg.ConfirmedToReliableMinutes ? 1 : 0;
            int decayed = (int)record.Confidence - steps;
            return decayed < (int)IntelConfidence.Unverified ? IntelConfidence.Unverified : (IntelConfidence)decayed;
        }

        /// <summary>Newer observation wins; an equal-time observation also wins (re-observing
        /// refreshes the payload). Never keep stale data over fresh data regardless of
        /// confidence — a fresh Uncertain rumor still describes the world later than an old
        /// Confirmed sighting.</summary>
        public static bool ShouldReplace(IntelRecord existing, IntelRecord incoming)
            => existing == null || incoming.ObservedAtMinute >= existing.ObservedAtMinute;
    }
}
