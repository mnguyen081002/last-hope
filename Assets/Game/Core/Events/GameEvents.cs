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
}
