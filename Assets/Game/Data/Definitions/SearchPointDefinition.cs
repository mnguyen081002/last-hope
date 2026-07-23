using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Loot table entry: on first open, rolled once against Weight, quantity uniform in [Min, Max].
    /// </summary>
    public sealed class LootEntry
    {
        public string ItemId { get; set; }
        public int Weight { get; set; } = 1;
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
