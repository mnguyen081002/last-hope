using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Rules
{
    /// <summary>Resolved equipment effects relevant to a route crossing (BL-P1 S8): rope reduces
    /// effective current, jacket reduces wet gain, boots block/reduce exposure gain at low flood
    /// levels. None = no equipment effect (defaults that leave TravelRules unaffected).</summary>
    public readonly struct EquipmentProtection
    {
        public readonly int CurrentReduction;
        public readonly float WetMultiplier;
        public readonly int BootsBlockLevel;
        public readonly float BootsMediumMultiplier;

        public EquipmentProtection(int currentReduction, float wetMultiplier, int bootsBlockLevel, float bootsMediumMultiplier)
        {
            CurrentReduction = currentReduction;
            WetMultiplier = wetMultiplier;
            BootsBlockLevel = bootsBlockLevel;
            BootsMediumMultiplier = bootsMediumMultiplier;
        }

        public static readonly EquipmentProtection None = new EquipmentProtection(0, 1f, 0, 1f);
    }

    /// <summary>
    /// Reads ItemDefinition.Protection off currently equipped items (InventoryState.EquipmentSlots,
    /// S8) — shared by BeginTravelCommand/WorldMapPanel (rope/jacket/boots) and TransferItemCommand
    /// (gloves let you handle a contaminated item without an exposure cost).
    /// </summary>
    public static class EquipmentRules
    {
        public static float SumProtection(InventoryState inventory, DefinitionRegistry definitions, string protectionKey)
        {
            float total = 0f;
            foreach (string instanceId in inventory.EquipmentSlots.Values)
            {
                if (!inventory.Items.TryGetValue(instanceId, out var item)) continue;
                if (definitions.TryGetItem(item.ItemId, out var def) && def.Protection.TryGetValue(protectionKey, out float value))
                    total += value;
            }
            return total;
        }

        public static bool HasProtection(InventoryState inventory, DefinitionRegistry definitions, string protectionKey) =>
            SumProtection(inventory, definitions, protectionKey) > 0f;

        public static EquipmentProtection ResolveTravelProtection(InventoryState inventory, DefinitionRegistry definitions)
        {
            int currentReduction = (int)SumProtection(inventory, definitions, "current_reduction");
            float wetMultiplier = 1f;
            float bootsMultiplier = 1f;
            int bootsBlockLevel = 0;

            foreach (string instanceId in inventory.EquipmentSlots.Values)
            {
                if (!inventory.Items.TryGetValue(instanceId, out var item)) continue;
                if (!definitions.TryGetItem(item.ItemId, out var def)) continue;

                if (def.Protection.TryGetValue("wet_multiplier", out float wm)) wetMultiplier *= wm;
                if (def.Protection.TryGetValue("exposure_block_level", out float blockLevel))
                {
                    bootsBlockLevel = (int)blockLevel;
                    bootsMultiplier = def.Protection.TryGetValue("exposure_medium_multiplier", out float mm) ? mm : 1f;
                }
            }

            return new EquipmentProtection(currentReduction, wetMultiplier, bootsBlockLevel, bootsMultiplier);
        }
    }
}
