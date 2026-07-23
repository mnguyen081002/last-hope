namespace LastHope.Core.State
{
    /// <summary>
    /// Runtime state of one search point container (BL-P1-17). Rolled once on first open
    /// (OpenSearchPointCommand) and never again — Inventory holds whatever hasn't been taken yet,
    /// persisting through Save/Load exactly as-is (the "leave it, come back later" design).
    /// Holding a full InventoryState (not a raw item list) means TransferItemCommand works on
    /// this container unchanged.
    /// </summary>
    public sealed class SearchPointState
    {
        public string SearchPointId { get; set; }
        public bool Rolled { get; set; }
        public InventoryState Inventory { get; set; }
    }
}
