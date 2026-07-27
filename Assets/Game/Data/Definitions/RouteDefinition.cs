namespace LastHope.Data.Definitions
{
    /// <summary>Tuyến đi hai chiều giữa hai location.</summary>
    public class RouteDefinition : DefinitionBase
    {
        public string FromLocationId;
        public string ToLocationId;
        public int TravelMinutes;

        /// <summary>
        /// Null = route không bao giờ tự đóng theo Disaster Phase (mặc định — content hiện
        /// có không set field này). Set khi có route thay thế (BL-P2-12), tránh softlock.
        /// </summary>
        public DisasterPhase? ClosesAtPhase;

        /// <summary>Đầu còn lại của tuyến tính từ <paramref name="locationId"/>.</summary>
        public string OtherEnd(string locationId) =>
            locationId == FromLocationId ? ToLocationId : FromLocationId;

        public bool Connects(string locationId) =>
            locationId == FromLocationId || locationId == ToLocationId;
    }
}
