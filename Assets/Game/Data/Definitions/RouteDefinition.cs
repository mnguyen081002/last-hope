namespace LastHope.Data.Definitions
{
    public sealed class RouteDefinition : DefinitionBase
    {
        public string FromLocationId { get; set; }
        public string ToLocationId { get; set; }
        public int TravelMinutes { get; set; }

        /// <summary>Subtracted from the phase's flood/current band before clamping (S8
        /// HazardRules) — a route on higher ground sees less flood/current than the raw phase
        /// band would suggest. 0 = no elevation advantage.</summary>
        public int BaseElevationLevel { get; set; }
    }
}
