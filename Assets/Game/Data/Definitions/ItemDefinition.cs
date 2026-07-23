using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public sealed class ItemDefinition : DefinitionBase
    {
        public string Category { get; set; }
        public float BaseWeightKg { get; set; }
        public float BaseVolumeLiters { get; set; }
        public int MaxStackSize { get; set; } = 1;
        public float MaxDurability { get; set; } = 100f;
        public float WaterResistance { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>True for large objects (e.g. a 20L water container) that occupy the
        /// CarriedObject slot instead of the backpack — mechanical, hence a typed field, not a tag.</summary>
        public bool TwoHandCarry { get; set; }
    }
}
