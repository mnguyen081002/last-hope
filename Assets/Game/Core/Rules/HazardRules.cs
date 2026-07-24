using System;
using System.Collections.Generic;
using LastHope.Data.Definitions;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Deterministic flood/current level math shared by HazardSystem (live, current minute) and
    /// ReturnWindowCalculator (forecast, future minutes) — one formula, so live state and the
    /// forecast the player reads on the World Map can never drift apart (BL-P1 S8).
    /// </summary>
    public static class HazardRules
    {
        public const int MaxLevel = 4;

        public readonly struct RouteHazardLevels
        {
            public readonly int FloodLevel;
            public readonly int CurrentLevel;

            public RouteHazardLevels(int floodLevel, int currentLevel)
            {
                FloodLevel = floodLevel;
                CurrentLevel = currentLevel;
            }
        }

        /// <summary>Finds the phase active at atMinute (and the next one, if any) among
        /// phasesByStart (must be sorted ascending by StartMinute — DefinitionRegistry.
        /// DisasterPhasesSorted), then lerps that phase's flood/current band toward the next
        /// phase's band by how far through the current phase atMinute is.</summary>
        public static RouteHazardLevels EvaluateRoute(
            RouteDefinition route, IReadOnlyList<DisasterPhaseDefinition> phasesByStart, long atMinute)
        {
            DisasterPhaseDefinition current = null;
            DisasterPhaseDefinition next = null;
            for (int i = 0; i < phasesByStart.Count; i++)
            {
                if (phasesByStart[i].StartMinute > atMinute)
                {
                    next = phasesByStart[i];
                    break;
                }
                current = phasesByStart[i];
            }

            if (current == null) return new RouteHazardLevels(0, 0);

            float progress = next == null
                ? 1f
                : Clamp01((atMinute - current.StartMinute) / (float)(next.StartMinute - current.StartMinute));

            int flood = ComputeLevel(current.FloodBandMin, current.FloodBandMax, progress, route.BaseElevationLevel);
            int currentLevel = ComputeLevel(current.CurrentBandMin, current.CurrentBandMax, progress, route.BaseElevationLevel);
            return new RouteHazardLevels(flood, currentLevel);
        }

        public static int ComputeLevel(int bandMin, int bandMax, float phaseProgress, int baseElevationLevel)
        {
            float lerped = bandMin + (bandMax - bandMin) * Clamp01(phaseProgress);
            int rounded = (int)Math.Round(lerped, MidpointRounding.AwayFromZero);
            return Math.Clamp(rounded - baseElevationLevel, 0, MaxLevel);
        }

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
