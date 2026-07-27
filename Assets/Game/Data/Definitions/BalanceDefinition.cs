namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Số liệu cân bằng toàn cục (`balance.json`). Chỉ khai báo các nhóm đã có hệ thống dùng
    /// tới — nhóm của milestone sau (power, water, npc, slice...) thêm khi làm tới, JSON dư
    /// field thì bỏ qua chứ không lỗi.
    /// </summary>
    public class BalanceDefinition
    {
        public InventoryBalance Inventory = new();
        public TravelBalance Travel = new();
        public NewGameBalance NewGame = new();
    }

    public class InventoryBalance
    {
        public float BackpackCapacityKg = 15f;
        public float BackpackCapacityLiters = 25f;

        /// <summary>Tỉ lệ tải bắt đầu overload nhẹ / nặng (1.0 = 100% sức chứa).</summary>
        public float OverloadLightThreshold = 1f;
        public float OverloadHeavyThreshold = 1.3f;

        /// <summary>Trần cứng: quá mức này không nhặt thêm được.</summary>
        public float HardCapMultiplier = 1.5f;

        public float SpeedModifierLight = 0.6f;
        public float SpeedModifierHeavy = 0.35f;
    }

    public class TravelBalance
    {
        public float LoadFactorNormal = 1f;
        public float LoadFactorLight = 1.25f;
        public float LoadFactorHeavy = 1.5f;
    }

    public class NewGameBalance
    {
        public string StartLocationId;
        public string MainShelterId;
    }
}
