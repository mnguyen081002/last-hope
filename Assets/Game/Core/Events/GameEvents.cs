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

    /// <summary>Publish trực tiếp từ Presentation (StorageView) — kho không đi qua Command vì luôn mở sẵn.</summary>
    public readonly struct StorageOpened
    {
        public readonly string LocationId;

        public StorageOpened(string locationId) => LocationId = locationId;
    }

    /// <summary>Publish trực tiếp từ Presentation (TravelPointView) — mở panel xác nhận trước khi thật sự đi.</summary>
    public readonly struct TravelPointOpened
    {
        public readonly string RouteId;

        public TravelPointOpened(string routeId) => RouteId = routeId;
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

    /// <summary>Shelter Event kích hoạt (P3): "drain_backflow" | "storage_flood_risk" | "pump_jam".</summary>
    public readonly struct ShelterEventTriggered
    {
        public readonly string EventId;

        public ShelterEventTriggered(string eventId) => EventId = eventId;
    }

    /// <summary>Bắn khi Construction hoàn thành (BuildSystem.ApplyShortTick).</summary>
    public readonly struct ConstructionCompleted
    {
        public readonly string SlotId;
        public readonly string ModuleId;

        public ConstructionCompleted(string slotId, string moduleId)
        {
            SlotId = slotId;
            ModuleId = moduleId;
        }
    }

    /// <summary>Publish trực tiếp từ Presentation (ShelterConsoleView) — chỉ một Shelter trong MVP, không cần payload.</summary>
    public readonly struct ShelterConsoleOpened
    {
    }

    /// <summary>Publish trực tiếp từ Presentation (BedView).</summary>
    public readonly struct BedOpened
    {
    }
}
