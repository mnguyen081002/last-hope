namespace LastHope.Data
{
    /// <summary>
    /// Every gameplay tuning constant, single object loaded from balance.json (not a keyed
    /// Definition list). Values are the baseline chosen 2026-07-24 (docs/plans/2026-07-24-p1-p2-
    /// completion-plan.md) — design docs specify no numbers ("cân bằng trong prototype"), so these
    /// are ours to tune, always via this file, never hard-coded in gameplay logic.
    /// </summary>
    public sealed class BalanceConfig
    {
        public InventoryBalance Inventory { get; set; } = new InventoryBalance();
        public TravelBalance Travel { get; set; } = new TravelBalance();
        public NewGameBalance NewGame { get; set; } = new NewGameBalance();
        public ConditionBalance Condition { get; set; } = new ConditionBalance();
        public HazardBalance Hazard { get; set; } = new HazardBalance();
        public ShelterBalance Shelter { get; set; } = new ShelterBalance();
        public PowerBalance Power { get; set; } = new PowerBalance();
        public WaterBalance Water { get; set; } = new WaterBalance();
        public IntelBalance Intel { get; set; } = new IntelBalance();
    }

    public sealed class InventoryBalance
    {
        public float BackpackCapacityKg { get; set; } = 15f;
        public float BackpackCapacityLiters { get; set; } = 25f;
        public float OverloadLightThreshold { get; set; } = 1.0f;
        public float OverloadHeavyThreshold { get; set; } = 1.3f;
        public float HardCapMultiplier { get; set; } = 1.5f;
        public float SpeedModifierLight { get; set; } = 0.6f;
        public float SpeedModifierHeavy { get; set; } = 0.35f;
    }

    public sealed class TravelBalance
    {
        public float LoadFactorNormal { get; set; } = 1.0f;
        public float LoadFactorLight { get; set; } = 1.25f;
        public float LoadFactorHeavy { get; set; } = 1.5f;
    }

    public sealed class NewGameBalance
    {
        public string StartLocationId { get; set; } = "location_shelter";

        /// <summary>Single-shelter assumption (S10) — the one ShelterState WaterIntrusionSystem
        /// seeds/ticks. S17 introduces a second shelter (shelter_school) alongside this.</summary>
        public string MainShelterId { get; set; } = "shelter_main";
    }

    public sealed class ConditionBalance
    {
        public float ThirstPerHour { get; set; } = 3.33f;
        public float HungerPerHour { get; set; } = 3.1f;
        public float FatiguePerLongTick { get; set; } = 0.2f;
        public float FatiguePerTravel { get; set; } = 8f;
        public float StaminaRegenPerMinute { get; set; } = 1f;
        public float StaminaRegenHalvedMultiplier { get; set; } = 0.5f;
        public float BodyTempDriftDownPerMinute { get; set; } = 0.05f;
        public float BodyTempRegenAtShelterPerMinute { get; set; } = 0.1f;
        public float WetThresholdForTempDrift { get; set; } = 50f;
        public float WetGainPerMinuteInRain { get; set; } = 1f;
        public float WetDryPerMinuteAtShelter { get; set; } = 2f;
        public float ColdBodyTempThreshold { get; set; } = 35.0f;
        public float ColdClearBodyTempThreshold { get; set; } = 36.0f;
        public float BlackWaterExposureThreshold { get; set; } = 40f;
        public float SickExposureThreshold { get; set; } = 70f;
        public float StarvationHealthDecayPerLongTick { get; set; } = 0.5f;
        public float StarvationHealthFloor { get; set; } = 1f;
        public float SickHealthDecayPerLongTick { get; set; } = 0.5f;
        public float CollapsedHealthThreshold { get; set; } = 5f;

        public int ShelterRestMinutes { get; set; } = 60;
        public int ShelterTreatExposureMinutes { get; set; } = 60;
        public float ShelterTreatExposureDecayPerLongTick { get; set; } = 5f;
    }

    /// <summary>Crossing-cost table indexed by tier 0-3 (tier 4 = HazardRules.MaxLevel = Impassable,
    /// blocked entirely — no cost to index). Tier = max(FloodLevel, effective CurrentLevel).</summary>
    public sealed class HazardBalance
    {
        public float[] CrossingStaminaCost { get; set; } = { 0f, 5f, 15f, 30f };
        public float[] CrossingExposureGain { get; set; } = { 0f, 5f, 15f, 30f };
        public float[] CrossingWetGain { get; set; } = { 10f, 30f, 60f, 90f };
        public float[] CrossingTimeFactor { get; set; } = { 1.0f, 1.2f, 1.5f, 2.0f };
        public float ContaminatedHandlingExposureGain { get; set; } = 10f;
    }

    /// <summary>Water Intrusion model (main-shelter-design.md §21-22, S10 baseline —
    /// docs/plans/2026-07-24-p3-p4-completion-plan.md "Bảng baseline"). InflowByRainIntensity is
    /// indexed by DisasterPhaseDefinition.RainIntensity (0-3 in the current phases_p2.json content;
    /// additive to extend when P4's longer timeline ships in S17).</summary>
    public sealed class ShelterBalance
    {
        public float DampThreshold { get; set; } = 10f;
        public float ShallowThreshold { get; set; } = 30f;
        public float DeepThreshold { get; set; } = 60f;
        public float CriticalThreshold { get; set; } = 85f;

        public float[] InflowByRainIntensity { get; set; } = { 0f, 2f, 4f, 6f };
        public float BackflowInflow { get; set; } = 6f;
        public float PassiveDrainPerLongTick { get; set; } = 2f;

        /// <summary>Applied once per active Portable Pump module (S11+); ungated by power until
        /// S12's PowerSystem starts flipping Module.Active based on allocation.</summary>
        public float PumpOutputPerLongTick { get; set; } = 6f;

        /// <summary>Fraction of table inflow an active, undamaged Barrier module blocks (S11).</summary>
        public float BarrierBlockFraction { get; set; } = 0.7f;
        public float BarrierDurabilityDecayPerLongTick { get; set; } = 2f;

        public float InitialStructuralIntegrity { get; set; } = 85f;
        public int InitialLivingCapacity { get; set; } = 2;
        public float InitialCleanWater { get; set; } = 3f;
        public float InitialUntreatedWater { get; set; } = 0f;
    }

    /// <summary>Power allocation model (main-shelter-design.md §19-20, S12) — one long-tick
    /// (10-minute) resolution, same convention ConditionSystem uses for hourly accrual.</summary>
    public sealed class PowerBalance
    {
        public float GridSupply { get; set; } = 6f;
        public float BatteryMaxCharge { get; set; } = 360f; // unit-minutes
        public float BatteryMaxDischargePerLongTick { get; set; } = 30f; // 3 units * 10 min
        public float BatteryChargeRatePerLongTick { get; set; } = 20f; // 2 units * 10 min
    }

    /// <summary>Water Processing model (main-shelter-design.md §11, S12).</summary>
    public sealed class WaterBalance
    {
        public float PurifyBatchSize { get; set; } = 3f;
        public int PurifyBatchMinutes { get; set; } = 60;

        /// <summary>A filter lasts 3 batches — ModuleState.Durability (MaxDurability 100) reused
        /// as the Purifier's filter-life meter; ~100/3 per batch.</summary>
        public float FilterWearPerBatch { get; set; } = 33.34f;

        public float IntakeUntreatedPerHour { get; set; } = 1f;
    }

    /// <summary>Information Age decay (S15, baseline plan: Confirmed→Reliable 60',
    /// →Uncertain 180') — read by IntelRules.EffectiveConfidence.</summary>
    public sealed class IntelBalance
    {
        public int ConfirmedToReliableMinutes { get; set; } = 60;
        public int ReliableToUncertainMinutes { get; set; } = 180;
    }
}
