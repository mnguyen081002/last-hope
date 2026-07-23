namespace LastHope.Core.Events
{
    /// <summary>Marker for event-bus payloads (technical-specification.md mục 9/§34).</summary>
    public interface IGameEvent { }

    public readonly struct WorldTimeChanged : IGameEvent
    {
        public readonly long WorldTimeMinutes;
        public readonly int DayIndex;
        public readonly int TimeOfDayMinutes;

        public WorldTimeChanged(long worldTimeMinutes, int dayIndex, int timeOfDayMinutes)
        {
            WorldTimeMinutes = worldTimeMinutes;
            DayIndex = dayIndex;
            TimeOfDayMinutes = timeOfDayMinutes;
        }
    }

    public readonly struct DisasterPhaseChanged : IGameEvent
    {
        public readonly string From;
        public readonly string To;
        public DisasterPhaseChanged(string from, string to) { From = from; To = to; }
    }

    public readonly struct RouteStateChanged : IGameEvent
    {
        public readonly string RouteId;
        public RouteStateChanged(string routeId) { RouteId = routeId; }
    }

    public readonly struct ShelterWarningRaised : IGameEvent
    {
        public readonly string ShelterId;
        public readonly string WarningKey;
        public ShelterWarningRaised(string shelterId, string warningKey) { ShelterId = shelterId; WarningKey = warningKey; }
    }

    public readonly struct TaskCompleted : IGameEvent
    {
        public readonly string TaskId;
        public TaskCompleted(string taskId) { TaskId = taskId; }
    }

    public readonly struct EventDiscovered : IGameEvent
    {
        public readonly string EventId;
        public EventDiscovered(string eventId) { EventId = eventId; }
    }

    public readonly struct InventoryChanged : IGameEvent
    {
        public readonly string OwnerId;
        public InventoryChanged(string ownerId) { OwnerId = ownerId; }
    }

    public readonly struct NpcStateChanged : IGameEvent
    {
        public readonly string NpcId;
        public NpcStateChanged(string npcId) { NpcId = npcId; }
    }

    public readonly struct OverloadStateChanged : IGameEvent
    {
        public readonly string OwnerId;
        public readonly Core.State.OverloadState Overload;
        public OverloadStateChanged(string ownerId, Core.State.OverloadState overload)
        {
            OwnerId = ownerId;
            Overload = overload;
        }
    }

    /// <summary>Published by SaveService-driven loads after WorldState fields are copied in
    /// place, so systems that cache derived values (e.g. avatar position) can resync.</summary>
    public readonly struct WorldStateReloaded : IGameEvent { }

    public readonly struct ItemTransferred : IGameEvent
    {
        public readonly string SourceOwnerId;
        public readonly string DestinationOwnerId;
        public readonly string ItemId;
        public readonly int Quantity;

        public ItemTransferred(string sourceOwnerId, string destinationOwnerId, string itemId, int quantity)
        {
            SourceOwnerId = sourceOwnerId;
            DestinationOwnerId = destinationOwnerId;
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    public readonly struct SearchPointOpened : IGameEvent
    {
        public readonly string SearchPointId;
        public readonly bool FirstOpen;
        public SearchPointOpened(string searchPointId, bool firstOpen) { SearchPointId = searchPointId; FirstOpen = firstOpen; }
    }

    /// <summary>UI-routing event, not sim state: tells the UI layer to open a container view
    /// for the given owner id (search point or shelter storage).</summary>
    public readonly struct ContainerViewRequested : IGameEvent
    {
        public readonly string OwnerId;
        public readonly string TitleKey;
        public ContainerViewRequested(string ownerId, string titleKey) { OwnerId = ownerId; TitleKey = titleKey; }
    }

    public readonly struct TravelStarted : IGameEvent
    {
        public readonly string RouteId;
        public readonly string FromLocationId;
        public readonly string ToLocationId;
        public readonly int PlannedMinutes;

        public TravelStarted(string routeId, string fromLocationId, string toLocationId, int plannedMinutes)
        {
            RouteId = routeId;
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
            PlannedMinutes = plannedMinutes;
        }
    }

    public readonly struct TravelCompleted : IGameEvent
    {
        public readonly string RouteId;
        public readonly string FromLocationId;
        public readonly string ToLocationId;
        public readonly int MinutesSpent;

        public TravelCompleted(string routeId, string fromLocationId, string toLocationId, int minutesSpent)
        {
            RouteId = routeId;
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
            MinutesSpent = minutesSpent;
        }
    }
}
