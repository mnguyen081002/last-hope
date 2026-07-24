using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Loot table entry, rolled once on first open. Guaranteed entries always spawn (designer-controlled
    /// resource floor). Non-guaranteed entries spawn with probability Chance (0-100). Quantity is
    /// uniform in [Min, Max] either way.
    /// </summary>
    public sealed class LootEntry
    {
        public string ItemId { get; set; }
        public bool Guaranteed { get; set; }
        public int Chance { get; set; } = 100;
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
    }

    public sealed class SearchPointDefinition : DefinitionBase
    {
        public string LocationId { get; set; }

        /// <summary>
        /// Minutes consumed to open this search point. 0 = instant (P1 baseline decision 2026-07-24:
        /// search reveals everything immediately, decisions happen at carry capacity, not at reveal time).
        /// </summary>
        public int OpenTimeMinutes { get; set; }

        public List<LootEntry> LootTable { get; set; } = new List<LootEntry>();
    }
}
