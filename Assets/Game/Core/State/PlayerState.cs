namespace LastHope.Core.State
{
    public sealed class PlayerState
    {
        public string ActorId { get; set; } = "player";
        public string CurrentLocationId { get; set; }
        public InventoryState Inventory { get; set; } = new InventoryState { OwnerId = "player" };
    }
}
