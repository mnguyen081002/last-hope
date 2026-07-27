namespace LastHope.Data.Definitions
{
    /// <summary>Tuyến đi hai chiều giữa hai location.</summary>
    public class RouteDefinition : DefinitionBase
    {
        public string FromLocationId;
        public string ToLocationId;
        public int TravelMinutes;

        /// <summary>Đầu còn lại của tuyến tính từ <paramref name="locationId"/>.</summary>
        public string OtherEnd(string locationId) =>
            locationId == FromLocationId ? ToLocationId : FromLocationId;

        public bool Connects(string locationId) =>
            locationId == FromLocationId || locationId == ToLocationId;
    }
}
