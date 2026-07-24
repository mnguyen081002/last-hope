namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Timeline threshold for WorldState.CurrentDisasterPhase (BL-P1 S7). Loaded from
    /// phases_*.json; DisasterPhaseSystem orders these by StartMinute and picks "the latest
    /// phase whose StartMinute has passed" as current. FloodBand/CurrentBand are consumed by
    /// the Hazard system (S8) — S7 only needs the phase transitions themselves.
    /// </summary>
    public sealed class DisasterPhaseDefinition : DefinitionBase
    {
        public long StartMinute { get; set; }
        public int FloodBandMin { get; set; }
        public int FloodBandMax { get; set; }
        public int CurrentBandMin { get; set; }
        public int CurrentBandMax { get; set; }
        public bool BlackWater { get; set; }
        public int RainIntensity { get; set; }
    }
}
