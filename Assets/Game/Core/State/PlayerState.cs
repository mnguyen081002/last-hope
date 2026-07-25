namespace LastHope.Core.State
{
    public sealed class PlayerState
    {
        public string ActorId { get; set; } = "player";
        public string CurrentLocationId { get; set; }
        public InventoryState Inventory { get; set; } = new InventoryState { OwnerId = "player" };
        public PlayerConditionState Condition { get; set; } = new PlayerConditionState();

        // Flat floats, not a Vector2 — Core stays UnityEngine-free. Written by
        // Presentation.PlayerAvatarSync every frame; PositionLocationId disambiguates which
        // scene the coordinates belong to (must match CurrentLocationId to be valid on load).
        // 2026-07-25: PositionZ dropped (3D->2D isometric migration) — 2D world position is X/Y only.
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string PositionLocationId { get; set; }
    }
}
