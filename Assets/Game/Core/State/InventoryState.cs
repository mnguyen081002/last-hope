using System.Collections.Generic;

namespace LastHope.Core.State
{
    public enum OverloadState { Normal, Light, Heavy }

    /// <summary>
    /// One actor's inventory (technical-specification.md mục 9/§15). Capacity/overload
    /// thresholds are owned by LastHope.Systems.Inventory (Sprint 5), not by this state class.
    /// </summary>
    public sealed class InventoryState
    {
        public string OwnerId { get; set; }
        public Dictionary<string, string> EquipmentSlots { get; set; } = new Dictionary<string, string>();
        public List<string> QuickSlots { get; set; } = new List<string>();
        public string BackpackContainerId { get; set; }
        public string CarriedObjectInstanceId { get; set; }
        public Dictionary<string, ItemInstanceState> Items { get; set; } = new Dictionary<string, ItemInstanceState>();
        public float CurrentWeightKg { get; set; }
        public float CurrentVolumeLiters { get; set; }
        public OverloadState Overload { get; set; } = OverloadState.Normal;
    }
}
