namespace LastHope.Core.State
{
    public enum ContaminationState { Clean, Contaminated }
    public enum WetState { Dry, Damp, Soaked }

    /// <summary>
    /// One item instance (technical-specification.md mục 9/§14 "Item Instance").
    /// Instances with differing Condition/Contamination/Wet do not stack.
    /// </summary>
    public sealed class ItemInstanceState
    {
        public string InstanceId { get; set; }
        public string ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public float Condition { get; set; } = 1f;
        public float Durability { get; set; }
        public ContaminationState Contamination { get; set; } = ContaminationState.Clean;
        public WetState Wet { get; set; } = WetState.Dry;
        public string ContainerId { get; set; }
    }
}
