using System;
using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Result of evaluating one route crossing (BL-P1 S8). Deterministic, no RNG: Impassable
    /// blocks entirely; everything else is a resource cost + warnings, never a hard stop — low
    /// stamina converts to Fatigue instead of blocking travel (BeginTravelCommand applies this).
    /// </summary>
    public readonly struct CrossingEvaluation
    {
        public readonly bool Passable;
        public readonly float StaminaCost;
        public readonly float ExposureGain;
        public readonly float WetGain;
        public readonly float TimeFactor;
        public readonly IReadOnlyList<string> Warnings;

        public CrossingEvaluation(bool passable, float staminaCost, float exposureGain, float wetGain, float timeFactor, IReadOnlyList<string> warnings)
        {
            Passable = passable;
            StaminaCost = staminaCost;
            ExposureGain = exposureGain;
            WetGain = wetGain;
            TimeFactor = timeFactor;
            Warnings = warnings;
        }
    }

    public static class TravelRules
    {
        public static CrossingEvaluation EvaluateCrossing(
            HazardRules.RouteHazardLevels hazard,
            PlayerConditionState condition,
            HazardBalance cfg,
            EquipmentProtection equipment)
        {
            var warnings = new List<string>();
            int effectiveCurrent = Math.Max(0, hazard.CurrentLevel - equipment.CurrentReduction);
            int tier = Math.Max(hazard.FloodLevel, effectiveCurrent);

            if (tier >= HazardRules.MaxLevel)
            {
                warnings.Add("Impassable: flood/current too high to cross safely.");
                return new CrossingEvaluation(false, 0f, 0f, 0f, 0f, warnings);
            }

            float staminaCost = cfg.CrossingStaminaCost[tier];
            float exposureGain = cfg.CrossingExposureGain[tier];
            float wetGain = cfg.CrossingWetGain[tier] * equipment.WetMultiplier;
            float timeFactor = cfg.CrossingTimeFactor[tier];

            if (hazard.FloodLevel <= equipment.BootsBlockLevel)
                exposureGain = 0f;
            else if (hazard.FloodLevel == equipment.BootsBlockLevel + 1)
                exposureGain *= equipment.BootsMediumMultiplier;

            if (condition.Stamina < staminaCost)
                warnings.Add("Not enough stamina — the shortfall will add Fatigue instead.");

            return new CrossingEvaluation(true, staminaCost, exposureGain, wetGain, timeFactor, warnings);
        }
    }
}
