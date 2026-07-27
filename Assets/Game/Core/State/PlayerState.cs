namespace LastHope.Core.State
{
    public class PlayerState
    {
        public string CurrentLocationId;

        /// <summary>Vị trí trong scene hiện tại (world X/Y, 2D).</summary>
        public float PositionX;
        public float PositionY;

        public InventoryState Inventory = new();

        // Condition đầy đủ thuộc P2 — giữ sẵn field để save schema không đổi giữa chừng.
        public float Thirst;
        public float Hunger;
        public float Fatigue;
        public float Wet;
    }
}
