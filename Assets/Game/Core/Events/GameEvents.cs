namespace LastHope.Core.Events
{
    // Event là struct để publish không sinh rác — tick chạy mỗi phút game suốt phiên chơi.

    public readonly struct WorldTimeChanged
    {
        public readonly long WorldTimeMinutes;
        public readonly bool IsLongTick;

        public WorldTimeChanged(long worldTimeMinutes, bool isLongTick)
        {
            WorldTimeMinutes = worldTimeMinutes;
            IsLongTick = isLongTick;
        }
    }

    public readonly struct InventoryChanged
    {
        public readonly string OwnerId;

        public InventoryChanged(string ownerId) => OwnerId = ownerId;
    }

    public readonly struct LocationChanged
    {
        public readonly string FromLocationId;
        public readonly string ToLocationId;

        public LocationChanged(string fromLocationId, string toLocationId)
        {
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
        }
    }

    public readonly struct SearchPointOpened
    {
        public readonly string SearchPointId;

        public SearchPointOpened(string searchPointId) => SearchPointId = searchPointId;
    }

    /// <summary>Bắn lúc bắt đầu di chuyển; <see cref="LocationChanged"/> bắn khi tới nơi.</summary>
    public readonly struct TravelStarted
    {
        public readonly string RouteId;
        public readonly int TravelMinutes;

        public TravelStarted(string routeId, int travelMinutes)
        {
            RouteId = routeId;
            TravelMinutes = travelMinutes;
        }
    }

    /// <summary>Bắn sau khi load save — view phải đọc lại toàn bộ state, không tin cache cũ.</summary>
    public readonly struct WorldStateReloaded
    {
    }
}
