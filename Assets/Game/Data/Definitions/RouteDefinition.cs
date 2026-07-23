namespace LastHope.Data.Definitions
{
    public sealed class RouteDefinition : DefinitionBase
    {
        public string FromLocationId { get; set; }
        public string ToLocationId { get; set; }
        public int TravelMinutes { get; set; }
    }
}
