using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>
    /// Pure slice-ending classification (S18). Reads WorldState/ShelterState only — no GameContext
    /// dependency, same convention as TravelRules/HazardRules/WaterIntrusionRules.
    /// </summary>
    public static class OutcomeRules
    {
        /// <summary>
        /// Collapse: player Incapacitated (no separate player-death state exists).
        /// Otherwise, if the shelter's ground floor is lost (WaterIntrusionSystem's
        /// GroundFloorLost flag): Forced Evacuation if the player committed to evacuating
        /// (EvacuateCommand) AND reached SliceBalance.EvacuationLocationId — Collapse otherwise
        /// (lost the shelter with nowhere safe reached).
        /// Otherwise (shelter still viable): Stable Survival only if clean water meets the
        /// minimum floor — surviving-but-starved doesn't count as a good ending, matching the
        /// plan's "≥ tài nguyên tối thiểu" being a required Stable Survival condition, not optional.
        /// </summary>
        public static Outcome Evaluate(WorldState world, ShelterState mainShelter, SliceBalance cfg)
        {
            if (ConditionOps.IsIncapacitated(world.Player.Condition))
                return Outcome.Collapse;

            bool shelterLost = mainShelter.EventFlags.Contains(ShelterEventFlags.GroundFloorLost);
            if (shelterLost)
            {
                bool evacuated = world.PersistentFlags.TryGetValue("evacuated", out bool ev) && ev;
                bool reachedSafety = evacuated && world.Player.CurrentLocationId == cfg.EvacuationLocationId;
                return reachedSafety ? Outcome.ForcedEvacuation : Outcome.Collapse;
            }

            return mainShelter.WaterStocks.Clean >= cfg.MinCleanWaterForStableSurvival
                ? Outcome.StableSurvival
                : Outcome.Collapse;
        }
    }
}
