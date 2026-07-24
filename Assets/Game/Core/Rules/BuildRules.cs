using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Data.Definitions;

namespace LastHope.Core.Rules
{
    public enum PlacementIssue { None, InvalidSlot, SlotLocked, SlotOccupied, WrongZone }

    /// <summary>Pure placement/material checks for the Build System (S11) — no GameContext
    /// dependency, matching TravelRules/HazardRules: callers (BuildCommands) translate the result
    /// into a CommandErrorCode themselves.</summary>
    public static class BuildRules
    {
        public static bool TryFindZoneForSlot(IReadOnlyDictionary<string, ShelterZoneDefinition> zones, string slotId, out ShelterZoneDefinition zone)
        {
            foreach (var candidate in zones.Values)
            {
                if (candidate.BuildSlotIds.Contains(slotId))
                {
                    zone = candidate;
                    return true;
                }
            }
            zone = null;
            return false;
        }

        public static PlacementIssue ValidatePlacement(
            IReadOnlyDictionary<string, ShelterZoneDefinition> zones, ShelterState shelter, string slotId, ModuleDefinition module)
        {
            if (!shelter.BuildSlots.TryGetValue(slotId, out var slot))
                return PlacementIssue.InvalidSlot;
            if (slot.Locked)
                return PlacementIssue.SlotLocked;
            if (!string.IsNullOrEmpty(slot.ModuleInstanceId))
                return PlacementIssue.SlotOccupied;
            if (!TryFindZoneForSlot(zones, slotId, out var zone) || !module.AllowedZoneIds.Contains(zone.Id))
                return PlacementIssue.WrongZone;

            return PlacementIssue.None;
        }

        public static bool HasMaterials(InventoryState inv, IReadOnlyDictionary<string, int> materials)
        {
            foreach (var required in materials)
            {
                int have = 0;
                foreach (var item in inv.Items.Values)
                    if (item.ItemId == required.Key) have += item.Quantity;
                if (have < required.Value) return false;
            }
            return true;
        }

        /// <summary>50% of each material, rounded down.</summary>
        public static Dictionary<string, int> DismantleRefund(IReadOnlyDictionary<string, int> materials)
        {
            var refund = new Dictionary<string, int>();
            foreach (var kvp in materials)
            {
                int qty = kvp.Value / 2;
                if (qty > 0) refund[kvp.Key] = qty;
            }
            return refund;
        }
    }
}
