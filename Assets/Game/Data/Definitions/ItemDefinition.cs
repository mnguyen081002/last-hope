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

        /// <summary>Condition stat deltas applied on UseItemCommand (S7). Known keys: "thirst",
        /// "hunger", "health", "stamina", "fatigue" — unknown keys are ignored.</summary>
        public Dictionary<string, float> UseEffects { get; set; } = new Dictionary<string, float>();

        /// <summary>Equipment slot this item occupies when worn (S8 EquipItemCommand); null/empty
        /// for non-equippable items. Added now so P1 items don't need another schema migration.</summary>
        public string EquipSlot { get; set; }

        /// <summary>Protection multipliers this item grants while equipped (S8 Hazard/TravelRules),
        /// e.g. "wet_resistance" -> 0.3. Empty and unused until S8 ships equippable content.</summary>
        public Dictionary<string, float> Protection { get; set; } = new Dictionary<string, float>();
    }
}
