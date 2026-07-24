using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// One of Main Shelter's 8 fixed zones (main-shelter-design.md §6-14, S10). Single-shelter
    /// assumption for now (id implicitly belongs to the one shelter, "shelter_main") — S17 adds a
    /// second shelter and migrates ShelterState to a real per-shelter map.
    /// </summary>
    public sealed class ShelterZoneDefinition : DefinitionBase
    {
        /// <summary>"Ground" / "Upper" / "Roof".</summary>
        public string Floor { get; set; }

        public List<string> BuildSlotIds { get; set; } = new List<string>();

        /// <summary>"None" / "Low" / "Medium" / "High" / "Critical" — display-only in S10, no rule
        /// reads this yet (WaterIntrusionRules operates on the shelter as a whole).</summary>
        public string WaterRisk { get; set; }
    }
}
