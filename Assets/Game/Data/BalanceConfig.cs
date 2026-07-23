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
}
