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
        public HazardBalance Hazard = new();
        public DisasterPhaseBalance DisasterPhase = new();
        public ShelterBalance Shelter = new();
        public PowerBalance Power = new();
        public WaterBalance Water = new();
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

    /// <summary>
    /// Khớp 1:1 `balance.json.hazard`. Mảng 4 phần tử = index theo <c>FloodState</c>
    /// Dry(0)/Shallow(1)/Medium(2)/Deep(3) — chốt với user 2026-07-27. Impassable không có
    /// index (route bị chặn hoàn toàn, không tính crossing cost).
    /// </summary>
    public class HazardBalance
    {
        public float[] CrossingStaminaCost = { 0f, 5f, 15f, 30f };
        public float[] CrossingExposureGain = { 0f, 5f, 15f, 30f };
        public float[] CrossingWetGain = { 10f, 30f, 60f, 90f };
        public float[] CrossingTimeFactor = { 1.0f, 1.2f, 1.5f, 2.0f };
        public float ContaminatedHandlingExposureGain = 10f;

        // Tự đề xuất 2026-07-27, chưa qua playtest — xem docs/plans/2026-07-27-p2b-phase-2.md.
        // Mảng 5 phần tử = index theo CurrentStrength None(0)..Extreme(4), không có mức nào chặn
        // hoàn toàn (khác Flood) — Route đã có Flood/Route Closure lo việc chặn.
        public float[] CurrentStrengthStaminaCost = { 0f, 8f, 20f, 35f, 55f };
        public float[] CurrentStrengthSweepChancePercent = { 0f, 5f, 15f, 30f, 50f };
        public float CurrentSweepHealthDamage = 10f;
        public float ElectrifiedWaterDamage = 15f;
        public float ElectrifiedWaterStaminaCost = 10f;
    }

    /// <summary>Tự đề xuất 2026-07-27, chưa qua playtest — mốc tính bằng phút game (Day 0 17:00 = phút 0).</summary>
    public class DisasterPhaseBalance
    {
        public float FirstRainAtMinute = 240f;
        public float BlackRainAtMinute = 600f;
        public float RouteClosureAtMinute = 900f;
    }

    /// <summary>
    /// Khớp `balance.json.shelter`. Mảng <see cref="InflowByRainIntensity"/> 5 phần tử nhưng
    /// hệ thống chỉ dùng index 0-3 (theo <c>DisasterPhase</c> Dry/FirstRain/BlackRain/
    /// RouteClosure) — timeline rút gọn không có phase thứ 5. Field có tiền tố "P3" là tự đề
    /// xuất 2026-07-28, chưa qua playtest — xem docs/plans/2026-07-28-p3-shelter-loop.md.
    /// </summary>
    public class ShelterBalance
    {
        public float DampThreshold = 10f;
        public float ShallowThreshold = 30f;
        public float DeepThreshold = 60f;
        public float CriticalThreshold = 85f;
        public float[] InflowByRainIntensity = { 0f, 2f, 4f, 6f, 9f };
        public float BackflowInflow = 6f;
        public float PassiveDrainPerLongTick = 2f;
        public float PumpOutputPerLongTick = 6f;
        public float BarrierBlockFraction = 0.7f;
        public float BarrierDurabilityDecayPerLongTick = 2f;
        public float InitialStructuralIntegrity = 85f;
        public int InitialLivingCapacity = 2;
        public float InitialCleanWater = 3f;
        public float InitialUntreatedWater = 0f;

        public float StorageFloodLossChancePercent = 15f;
        public float DrainBackflowTriggerChancePercent = 10f;
        public float DrainBackflowResolveMinutes = 20f;
        public float PumpJamChancePercent = 8f;
        public float PumpJamResolveMinutes = 15f;
        public float SleepFatigueRecoveryPerHour = 12f;
        public float SleepMinHours = 1f;
        public float SleepMaxHours = 10f;
    }

    /// <summary>Khớp `balance.json.power`. Grid Supply theo Disaster Phase — xem <c>PowerSystem.GridSupply</c>.</summary>
    public class PowerBalance
    {
        public float GridSupply = 6f;
        public float BatteryMaxCharge = 360f;
        public float BatteryMaxDischargePerLongTick = 30f;
        public float BatteryChargeRatePerLongTick = 20f;
    }

    /// <summary>Khớp `balance.json.water` (Water Purifier).</summary>
    public class WaterBalance
    {
        public float PurifyBatchSize = 3f;
        public float PurifyBatchMinutes = 60f;
        public float FilterWearPerBatch = 33.34f;
        public float IntakeUntreatedPerHour = 1f;
    }
}
