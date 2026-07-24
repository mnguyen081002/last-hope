using System.Collections.Generic;
using LastHope.Data.Definitions;

namespace LastHope.Core.Rules
{
    /// <summary>Forecast result for the World Map (BL-P1 S8): null means "not observed within the
    /// sampled horizon", not "never".</summary>
    public readonly struct ReturnWindow
    {
        public readonly int? MinutesUntilWorse;
        public readonly int? MinutesUntilImpassable;

        public ReturnWindow(int? minutesUntilWorse, int? minutesUntilImpassable)
        {
            MinutesUntilWorse = minutesUntilWorse;
            MinutesUntilImpassable = minutesUntilImpassable;
        }
    }

    /// <summary>
    /// Samples HazardRules.EvaluateRoute forward in 10-minute steps — deterministic, no RNG, same
    /// formula HazardSystem uses live, so the forecast a player reads never lies about what will
    /// actually happen.
    /// </summary>
    public static class ReturnWindowCalculator
    {
        private const int StepMinutes = 10;
        private const int HorizonMinutes = 24 * 60; // one game-day forecast ceiling

        public static ReturnWindow Evaluate(
            RouteDefinition route, IReadOnlyList<DisasterPhaseDefinition> phasesByStart, long fromMinute)
        {
            var baseline = HazardRules.EvaluateRoute(route, phasesByStart, fromMinute);
            int baselineTier = System.Math.Max(baseline.FloodLevel, baseline.CurrentLevel);

            int? minutesUntilWorse = null;
            int? minutesUntilImpassable = null;

            for (int step = StepMinutes; step <= HorizonMinutes; step += StepMinutes)
            {
                var sample = HazardRules.EvaluateRoute(route, phasesByStart, fromMinute + step);
                int tier = System.Math.Max(sample.FloodLevel, sample.CurrentLevel);

                if (minutesUntilWorse == null && tier > baselineTier)
                    minutesUntilWorse = step;

                if (minutesUntilImpassable == null && tier >= HazardRules.MaxLevel)
                {
                    minutesUntilImpassable = step;
                    break;
                }
            }

            return new ReturnWindow(minutesUntilWorse, minutesUntilImpassable);
        }
    }
}
