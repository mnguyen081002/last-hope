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
}
