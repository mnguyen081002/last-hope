namespace LastHope.Core.State
{
    public sealed class PlayerState
    {
        public string ActorId { get; set; } = "player";
        public string CurrentLocationId { get; set; }
        public InventoryState Inventory { get; set; } = new InventoryState { OwnerId = "player" };

        // Flat floats, not a Vector3 — Core stays UnityEngine-free. Written by
        // Presentation.PlayerAvatarSync every frame; PositionLocationId disambiguates which
        // scene the coordinates belong to (must match CurrentLocationId to be valid on load).
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public string PositionLocationId { get; set; }
    }
}
