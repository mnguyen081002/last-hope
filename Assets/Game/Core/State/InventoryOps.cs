using System;
using LastHope.Data;

namespace LastHope.Core.State
{
    /// <summary>
    /// Weight/volume recompute + add primitives shared by commands and debug tools.
    /// Does not enforce capacity/overload rules — that belongs to Systems.Inventory (Sprint 5).
    /// </summary>
    public static class InventoryOps
    {
        public static void RecalculateLoad(InventoryState inv, DefinitionRegistry defs)
        {
            float weight = 0f;
            float volume = 0f;

            foreach (var item in inv.Items.Values)
            {
                if (!defs.TryGetItem(item.ItemId, out var def)) continue;
                weight += def.BaseWeightKg * item.Quantity;
                volume += def.BaseVolumeLiters * item.Quantity;
            }

            inv.CurrentWeightKg = weight;
            inv.CurrentVolumeLiters = volume;
        }

        /// <summary>
        /// Adds quantity to an existing compatible stack (same item, Condition/Contamination/Wet,
        /// under MaxStackSize) or creates a new instance. Recalculates load afterward.
        /// </summary>
        public static ItemInstanceState AddItem(
            InventoryState inv, DefinitionRegistry defs, string itemId, int quantity, Func<string> idGenerator)
        {
            if (!defs.TryGetItem(itemId, out var def))
                throw new ArgumentException($"Unknown item id '{itemId}'.", nameof(itemId));

            foreach (var existing in inv.Items.Values)
            {
                if (existing.ItemId != itemId) continue;
                if (existing.Contamination != ContaminationState.Clean || existing.Wet != WetState.Dry) continue;
                if (existing.Quantity >= def.MaxStackSize) continue;

                int space = def.MaxStackSize - existing.Quantity;
                int toAdd = Math.Min(space, quantity);
                existing.Quantity += toAdd;
                quantity -= toAdd;

                if (quantity <= 0)
                {
                    RecalculateLoad(inv, defs);
                    return existing;
                }
            }

            var instance = new ItemInstanceState
            {
                InstanceId = idGenerator(),
                ItemId = itemId,
                Quantity = quantity,
                ContainerId = inv.OwnerId,
            };
            inv.Items[instance.InstanceId] = instance;
            RecalculateLoad(inv, defs);
            return instance;
        }
    }
}
