using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// A buildable Shelter module (main-shelter-design.md §18, S11). Content: modules_p3.json —
    /// Barrier/Portable Pump/Elevated Storage/Water Purifier/Battery Bank (Tier 1-2 baseline set;
    /// Tier 3 strategic modules are P4+ content).
    /// </summary>
    public sealed class ModuleDefinition : DefinitionBase
    {
        /// <summary>ShelterZoneDefinition ids this module may be built into.</summary>
        public List<string> AllowedZoneIds { get; set; } = new List<string>();

        public Dictionary<string, int> Materials { get; set; } = new Dictionary<string, int>();
        public int BuildMinutes { get; set; }

        /// <summary>Power units demanded while Active — read by PowerSystem, S12. 0 for modules
        /// that don't consume power (Barrier, Elevated Storage, Battery Bank itself).</summary>
        public float PowerDemand { get; set; }

        public float MaxDurability { get; set; } = 100f;

        /// <summary>Role tags read by rule code instead of string-matching Id — "pump", "barrier",
        /// "purifier", "battery_bank", "elevated_storage".</summary>
        public List<string> Tags { get; set; } = new List<string>();
    }
}
