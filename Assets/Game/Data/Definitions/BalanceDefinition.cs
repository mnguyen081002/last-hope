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
        public ConditionBalance Condition = new();
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

    /// <summary>Khớp 1:1 `balance.json.condition`. Field chưa có hệ thống tiêu thụ (wet mưa
    /// ambient, black water exposure gain, shelter treat) vẫn khai báo đủ — sẵn sàng khi
    /// Hazard (P2-B) / Sleep (P3) nối vào, không phải sửa Data layer lần nữa.</summary>
    public class ConditionBalance
    {
        public float ThirstPerHour = 3.33f;
        public float HungerPerHour = 3.1f;
        public float FatiguePerLongTick = 0.2f;
        public float FatiguePerTravel = 8f;
        public float StaminaRegenPerMinute = 1f;
        public float StaminaRegenHalvedMultiplier = 0.5f;
        public float BodyTempDriftDownPerMinute = 0.05f;
        public float BodyTempRegenAtShelterPerMinute = 0.1f;
        public float WetThresholdForTempDrift = 50f;
        public float WetGainPerMinuteInRain = 1f;
        public float WetDryPerMinuteAtShelter = 2f;
        public float ColdBodyTempThreshold = 35f;
        public float ColdClearBodyTempThreshold = 36f;
        public float BlackWaterExposureThreshold = 40f;
        public float SickExposureThreshold = 70f;
        public float StarvationHealthDecayPerLongTick = 0.5f;
        public float StarvationHealthFloor = 1f;

        /// <summary>
        /// Tốc độ Thirst/Hunger/Health cùng xấu đi mỗi phút game khi Sick. Quy đổi từ "mỗi
        /// 30 giây thực" theo DefaultTimeScale ×5: 30s thực × 5 = 150s game = 2.5 phút
        /// game → 1/2.5 = 0.4/phút.
        /// </summary>
        public float SickDecayPerMinute = 0.4f;

        public float CollapsedHealthThreshold = 5f;
        public float ShelterRestMinutes = 60f;
        public float ShelterTreatExposureMinutes = 60f;
        public float ShelterTreatExposureDecayPerLongTick = 5f;
    }
}
