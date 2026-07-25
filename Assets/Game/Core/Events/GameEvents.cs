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

    public readonly struct ConditionChanged : IGameEvent
    {
        public readonly string ActorId;
        public ConditionChanged(string actorId) { ActorId = actorId; }
    }

    public readonly struct StatusEffectChanged : IGameEvent
    {
        public readonly string ActorId;
        public readonly string StatusId;
        public readonly float Severity;

        public StatusEffectChanged(string actorId, string statusId, float severity)
        {
            ActorId = actorId;
            StatusId = statusId;
            Severity = severity;
        }
    }

    public readonly struct EquipmentChanged : IGameEvent
    {
        public readonly string ActorId;
        public readonly string Slot;
        public readonly string ItemInstanceId; // null when unequipped

        public EquipmentChanged(string actorId, string slot, string itemInstanceId)
        {
            ActorId = actorId;
            Slot = slot;
            ItemInstanceId = itemInstanceId;
        }
    }

    /// <summary>UI-routing event, not sim state: TravelPointView publishes this instead of
    /// submitting BeginTravelCommand directly (S8) — WorldMapPanel opens and shows every route
    /// connected to the player's current location.</summary>
    public readonly struct WorldMapRequested : IGameEvent { }

    /// <summary>Published by a "focused" full-attention panel (Container, WorldMap) whenever it
    /// opens, so any other such panel that's currently visible closes itself instead of rendering
    /// on top of it unreadably (bugfix 2026-07-24: E at a travel point opening the map while a
    /// search container was still open from an earlier interaction).</summary>
    public readonly struct ExclusivePanelOpened : IGameEvent
    {
        public readonly string PanelName;
        public ExclusivePanelOpened(string panelName) { PanelName = panelName; }
    }

    /// <summary>Published by WaterIntrusionSystem (S10) only when WaterIntrusionState.Level
    /// actually changes (not every long-tick) — mirrors RouteStateChanged's change-only rule.</summary>
    public readonly struct ShelterWaterChanged : IGameEvent
    {
        public readonly string ShelterId;
        public readonly Core.State.WaterIntrusionLevel Level;
        public ShelterWaterChanged(string shelterId, Core.State.WaterIntrusionLevel level)
        {
            ShelterId = shelterId;
            Level = level;
        }
    }

    /// <summary>Published by TaskSystem (S11) every long-tick a task's Progress changes, and by
    /// Pause/Resume/Cancel commands — BuildPanel rebuilds its list on this.</summary>
    public readonly struct TaskStateChanged : IGameEvent
    {
        public readonly string TaskId;
        public TaskStateChanged(string taskId) { TaskId = taskId; }
    }

    /// <summary>Finer-grained than TaskStateChanged — just the Progress number, for a build
    /// progress bar that doesn't need to know the whole task shape.</summary>
    public readonly struct BuildProgressChanged : IGameEvent
    {
        public readonly string SlotId;
        public readonly float Progress;
        public BuildProgressChanged(string slotId, float progress) { SlotId = slotId; Progress = progress; }
    }

    /// <summary>Published by TaskSystem when a Build task reaches 100% and its ModuleState is
    /// created.</summary>
    public readonly struct ModuleCompleted : IGameEvent
    {
        public readonly string SlotId;
        public readonly string ModuleInstanceId;
        public ModuleCompleted(string slotId, string moduleInstanceId) { SlotId = slotId; ModuleInstanceId = moduleInstanceId; }
    }

    /// <summary>Published by PowerSystem (S12) whenever any module's Active (powered) state
    /// actually changes, or a priority is set.</summary>
    public readonly struct PowerStateChanged : IGameEvent
    {
        public readonly string ShelterId;
        public PowerStateChanged(string shelterId) { ShelterId = shelterId; }
    }

    /// <summary>Published by WaterSystem (passive intake) and StartPurifyBatchCommand/
    /// CollectWaterCommand (S12) whenever WaterStocksState changes.</summary>
    public readonly struct WaterStocksChanged : IGameEvent
    {
        public readonly string ShelterId;
        public WaterStocksChanged(string shelterId) { ShelterId = shelterId; }
    }

    public readonly struct SleepStarted : IGameEvent { }

    /// <summary>Woken early — S12 wake condition is WaterIntrusion reaching Deep/Critical;
    /// S14 will add Event-priority wake conditions on top without changing this struct.</summary>
    public readonly struct SleepInterrupted : IGameEvent
    {
        public readonly int MinutesSlept;
        public SleepInterrupted(int minutesSlept) { MinutesSlept = minutesSlept; }
    }

    public readonly struct SleepEnded : IGameEvent { }

    /// <summary>Published by EventSystem (S13) when a trigger condition is met — S13 skips
    /// Undiscovered/Discovered, so the instance is already Active by the time this fires.</summary>
    public readonly struct EventTriggered : IGameEvent
    {
        public readonly string EventInstanceId;
        public readonly string EventId;
        public EventTriggered(string eventInstanceId, string eventId) { EventInstanceId = eventInstanceId; EventId = eventId; }
    }

    /// <summary>Published once per instance the long-tick its soft deadline passes (S14) — the
    /// "act soon" warning. Carries the hard deadline so UI can show the real countdown.</summary>
    public readonly struct EventDeadlineApproaching : IGameEvent
    {
        public readonly string EventInstanceId;
        public readonly string EventId;
        public readonly long HardDeadlineMinute;
        public EventDeadlineApproaching(string eventInstanceId, string eventId, long hardDeadlineMinute)
        {
            EventInstanceId = eventInstanceId;
            EventId = eventId;
            HardDeadlineMinute = hardDeadlineMinute;
        }
    }

    /// <summary>Published when an Active event passes its hard deadline unresolved (S14). The
    /// instance ends as Expired, or PersistentConsequence when expiration flags were applied.</summary>
    public readonly struct EventExpired : IGameEvent
    {
        public readonly string EventInstanceId;
        public readonly string EventId;
        public EventExpired(string eventInstanceId, string eventId) { EventInstanceId = eventInstanceId; EventId = eventId; }
    }

    /// <summary>Published by IntelSystem (S15) whenever a subject's IntelRecord is written or
    /// replaced — WorldMapPanel rebuilds on this while open.</summary>
    public readonly struct IntelUpdated : IGameEvent
    {
        public readonly string SubjectId;
        public IntelUpdated(string subjectId) { SubjectId = subjectId; }
    }

    /// <summary>Published by ResolveEventCommand (S13) when a player response resolves an Active event.</summary>
    public readonly struct EventResolved : IGameEvent
    {
        public readonly string EventInstanceId;
        public readonly string EventId;
        public readonly string ResponseId;
        public EventResolved(string eventInstanceId, string eventId, string responseId)
        {
            EventInstanceId = eventInstanceId;
            EventId = eventId;
            ResponseId = responseId;
        }
    }
}
