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

    /// <summary>Bắn khi Production hoàn thành (BuildSystem.ApplyShortTick) — chuyển "Ready to
    /// Claim", không tạo Module trong thế giới (khác hành vi cũ trước 2026-07-30).</summary>
    public readonly struct ConstructionCompleted
    {
        public readonly string ModuleId;

        public ConstructionCompleted(string moduleId) => ModuleId = moduleId;
    }

    /// <summary>Bắn lúc bắt đầu sản xuất (StartConstructionCommand); <see cref="ConstructionCompleted"/>
    /// bắn khi xong — chênh world_time_minutes giữa hai sự kiện = thời gian chờ Task (BL-P3-18).</summary>
    public readonly struct ConstructionStarted
    {
        public readonly string ModuleId;
        public readonly int MinutesRequired;

        public ConstructionStarted(string moduleId, int minutesRequired)
        {
            ModuleId = moduleId;
            MinutesRequired = minutesRequired;
        }
    }

    /// <summary>Bắn khi Nhận sản phẩm Ready to Claim (ClaimProductionCommand) — cộng packed item
    /// vào túi Player.</summary>
    public readonly struct ProductionClaimed
    {
        public readonly string ModuleId;

        public ProductionClaimed(string moduleId) => ModuleId = moduleId;
    }

    /// <summary>Bắn khi đổi Power Priority của Module đã xây (SetPowerPriorityCommand) — đo Power Allocation choice (BL-P3-18).</summary>
    public readonly struct PowerPriorityChanged
    {
        public readonly string PlacementId;
        public readonly string ModuleId;
        public readonly string Priority;

        public PowerPriorityChanged(string placementId, string moduleId, string priority)
        {
            PlacementId = placementId;
            ModuleId = moduleId;
            Priority = priority;
        }
    }

    /// <summary>Bắn khi Tháo Module (DismantleModuleCommand) — Presentation (PlacedModuleRenderer)
    /// nghe để xoá sprite trong thế giới tương ứng.</summary>
    public readonly struct ModuleDismantled
    {
        public readonly string PlacementId;

        public ModuleDismantled(string placementId) => PlacementId = placementId;
    }

    /// <summary>Bắn khi đặt lại Module đã gói (RedeployModuleCommand) — tức thì, không qua
    /// Construction. Presentation (PlacedModuleRenderer) nghe để vẽ sprite ngay.</summary>
    public readonly struct ModuleRedeployed
    {
        public readonly string PlacementId;
        public readonly string ModuleId;

        public ModuleRedeployed(string placementId, string moduleId)
        {
            PlacementId = placementId;
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

    /// <summary>
    /// Publish từ UI (InventoryPanel) khi bấm "Đặt" cạnh 1 packed item trong túi Player —
    /// Presentation (PlacementModeController) nghe để bật chế độ đặt tự do trong thế giới (Free
    /// Placement, BL-P3-03). Không mang ZoneId — Placement tự resolve Zone theo tầng đang đứng +
    /// vị trí chuột (đổi 2026-07-30, xem docs/plans/2026-07-30-module-production-placement-loop.md).
    /// Luôn là đặt Module đã gói (<see cref="RedeployModuleCommand"/>) — Production không còn đi
    /// qua Placement Mode nữa.
    /// </summary>
    public readonly struct BeginPlacementMode
    {
        public readonly string ModuleId;

        public BeginPlacementMode(string moduleId) => ModuleId = moduleId;
    }
}
