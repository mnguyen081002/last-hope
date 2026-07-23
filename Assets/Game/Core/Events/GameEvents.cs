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
}
