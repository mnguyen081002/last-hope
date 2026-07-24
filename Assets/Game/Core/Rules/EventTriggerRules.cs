using System;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure trigger evaluation (event-system-design.md §6, S13) — flat AND of whichever optional
    /// fields EventDefinition sets. `roll` is pre-rolled by the caller (EventSystem, via RNG
    /// stream "events") so this stays deterministic and testable without touching RNG state.
    /// </summary>
    public static class EventTriggerRules
    {
        public static bool Evaluate(
            EventDefinition def, DisasterPhaseDefinition currentPhase, ShelterState shelter, bool hasActivePump, int? roll)
        {
            if (!string.IsNullOrEmpty(def.TriggerPhaseId) && currentPhase?.Id != def.TriggerPhaseId)
                return false;

            if (def.TriggerRequiresBlackWater && !(currentPhase?.BlackWater ?? false))
                return false;

            if (!string.IsNullOrEmpty(def.TriggerStateMinLevel))
            {
                if (!Enum.TryParse<WaterIntrusionLevel>(def.TriggerStateMinLevel, out var minLevel))
                    return false;
                if (shelter.WaterIntrusion.Level < minLevel)
                    return false;
            }

            if (def.TriggerRequiresPumpModule && !hasActivePump)
                return false;

            if (def.TriggerChancePercentPerLongTick > 0)
            {
                if (roll == null || roll.Value >= def.TriggerChancePercentPerLongTick)
                    return false;
            }

            return true;
        }
    }
}
