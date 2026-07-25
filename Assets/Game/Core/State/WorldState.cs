using System.Collections.Generic;
using LastHope.Core.Random;

namespace LastHope.Core.State
{
    // Minimal Sprint 2 stubs — each grows into its full shape when the owning system
    // (Hazard/Shelter/NPC/Event/Task) is implemented. Kept here because WorldState must
    // reference something serializable today; do not add fields speculatively beyond
    // what the current sprint's systems need.

    /// <summary>Cached hazard snapshot for display (World Map), recomputed by HazardSystem every
    /// long tick (S8). Commands (BeginTravelCommand) recompute fresh via HazardRules at the exact
    /// moment of travel rather than trusting this — it can be up to 9 minutes stale.
    /// Electrical/Modifiers are reserved for P3+ systems (Power System, Shelter Events) and are
    /// unused/inert this sprint.</summary>
    public sealed class RouteState
    {
        public string Id { get; set; }
        public int FloodLevel { get; set; }
        public int CurrentLevel { get; set; }
        public bool Contamination { get; set; }
        public bool Closed { get; set; }
        public bool Electrical { get; set; }
        public Dictionary<string, float> Modifiers { get; set; } = new Dictionary<string, float>();
    }

    /// <summary>SearchPointStates/DroppedItems added Sprint 6 (BL-P1-17) — search/drop containers
    /// for this location; both lazily created by InventoryOwnerResolver on first access.</summary>
    public sealed class LocationState
    {
        public string Id { get; set; }
        public string StatusName { get; set; } = "Normal";
        public Dictionary<string, SearchPointState> SearchPointStates { get; set; } = new Dictionary<string, SearchPointState>();
        public InventoryState DroppedItems { get; set; }
    }

    /// <summary>Water Intrusion Level (main-shelter-design.md §22), thresholds in
    /// ShelterBalance. Ordered low-to-high so callers can compare with &lt;/&gt;.</summary>
    public enum WaterIntrusionLevel { Dry, Damp, Shallow, Deep, Critical }

    public sealed class WaterIntrusionState
    {
        public WaterIntrusionLevel Level { get; set; } = WaterIntrusionLevel.Dry;
        public float Units { get; set; }
    }

    /// <summary>One Build Slot inside a ShelterZoneDefinition (S10 tracks slot existence/lock
    /// only; Module occupancy becomes real in S11's Build System).</summary>
    public sealed class BuildSlotState
    {
        public bool Locked { get; set; }
        public string ModuleInstanceId { get; set; }
    }

    public sealed class WaterStocksState
    {
        public float Clean { get; set; }
        public float Untreated { get; set; }
    }

    /// <summary>Ordered low-to-high so callers can compare with &lt;/&gt; — Critical wins ties for
    /// power (main-shelter-design.md §19-20, S12).</summary>
    public enum PowerPriority { Disabled, Normal, High, Critical }

    /// <summary>Named constants for ShelterState.EventFlags — centralized here (Core.State) since
    /// both a Systems-layer system (WaterIntrusionSystem) and a Core-layer command
    /// (ResolveEventCommand) need to read/write the same flag names, and Core cannot depend on
    /// Systems.</summary>
    public static class ShelterEventFlags
    {
        public const string LowerFloorPowerLocked = "lower_floor_power_locked";
        public const string GroundFloorLost = "ground_floor_lost";
        public const string DrainBackflowActive = "drain_backflow_active";
        public const string PumpJammed = "pump_jammed";
    }

    public sealed class ShelterPowerState
    {
        public float BatteryCharge { get; set; }

        /// <summary>Per-module priority, keyed by ModuleState.InstanceId. Missing entry defaults
        /// to Normal (set explicitly when a power-consuming module finishes building).</summary>
        public Dictionary<string, PowerPriority> Priorities { get; set; } = new Dictionary<string, PowerPriority>();
    }

    /// <summary>One built (or building) Module occupying a BuildSlot (S11). Active gates whether
    /// its effect (Pump drain, Purifier batch, etc.) currently applies — S11 defaults it true the
    /// moment construction completes (ungated); S12's PowerSystem starts flipping it false when
    /// the module can't be powered.</summary>
    public sealed class ModuleState
    {
        public string InstanceId { get; set; }
        public string ModuleId { get; set; }
        public string SlotId { get; set; }
        public float Durability { get; set; }
        public bool Active { get; set; }
    }

    /// <summary>Storage added Sprint 6 (BL-P1-18) — unlimited-capacity shelter container,
    /// lazily created by InventoryOwnerResolver on first access. S10 adds the real shelter
    /// simulation fields (WaterIntrusionSystem owns seeding + upkeep of this shelter's state —
    /// see WaterIntrusionSystem.Resync). S11 adds Modules. PowerState arrives with S12 — not
    /// declared here yet, its owning system doesn't exist.</summary>
    public sealed class ShelterState
    {
        public string Id { get; set; }
        public string StatusName { get; set; } = "Normal";
        public InventoryState Storage { get; set; }

        public float StructuralIntegrity { get; set; }
        public WaterIntrusionState WaterIntrusion { get; set; } = new WaterIntrusionState();
        public int LivingCapacity { get; set; }
        public int Occupants { get; set; }
        public Dictionary<string, BuildSlotState> BuildSlots { get; set; } = new Dictionary<string, BuildSlotState>();
        public Dictionary<string, ModuleState> Modules { get; set; } = new Dictionary<string, ModuleState>();
        public WaterStocksState WaterStocks { get; set; } = new WaterStocksState();
        public ShelterPowerState Power { get; set; } = new ShelterPowerState();

        /// <summary>Named boolean flags for shelter-scoped conditions no dedicated field exists
        /// for yet (e.g. "ground_floor_lost", "lower_floor_power_locked") — same role as
        /// WorldState.PersistentFlags but scoped to one shelter.</summary>
        public HashSet<string> EventFlags { get; set; } = new HashSet<string>();
    }

    /// <summary>Ordered healthy-to-dead so callers can compare with &lt;/&gt; (npc-framework §3).</summary>
    public enum NpcHealthState { Healthy, Injured, Critical, Dead }

    /// <summary>Replaces the S2-era Id/StatusName stub (S15) — reduced npc-framework §3 shape.
    /// Simulation (consumption, trust drift, task work) arrives with S16's NpcSystem; S15 only
    /// establishes the state + owner "npc:&lt;id&gt;" so save/load and commands have something real
    /// to hold.</summary>
    public sealed class NpcState
    {
        public string Id { get; set; }
        public string LocationId { get; set; }
        public NpcHealthState Health { get; set; } = NpcHealthState.Healthy;
        public float Hunger { get; set; }
        public float Thirst { get; set; }

        /// <summary>0-100 (npc-framework trust model); starting value comes from NpcDefinition.</summary>
        public int Trust { get; set; }

        public bool Recruited { get; set; }
        public string CurrentTaskId { get; set; }
        public HashSet<string> Flags { get; set; } = new HashSet<string>();

        /// <summary>NpcSystem pressure counters (S16) — consecutive long-ticks of unmet Hunger/
        /// Thirst, and of standing in a Deep+ flooded shelter, before Health drops a step. Reset
        /// to 0 whenever the condition clears.</summary>
        public int StarvingLongTicks { get; set; }
        public int FloodExposureLongTicks { get; set; }

        /// <summary>Owner id "npc:&lt;id&gt;" — lazily created by InventoryOwnerResolver.</summary>
        public InventoryState Inventory { get; set; }
    }

    /// <summary>Information confidence, ordered low-to-high so callers can compare (S15,
    /// world-map intel). Stored on the record as observed; IntelRules.EffectiveConfidence decays
    /// it by information age at read time.</summary>
    public enum IntelConfidence { Unverified, Uncertain, Reliable, Confirmed }

    /// <summary>One remembered observation about a subject (route/location). Flat payload —
    /// route hazard fields are null for non-route subjects; a free-form payload dict isn't
    /// warranted until a subject kind actually needs one.</summary>
    public sealed class IntelRecord
    {
        public string SubjectId { get; set; }
        public string Kind { get; set; } // "route" | "location"
        public IntelConfidence Confidence { get; set; }
        public long ObservedAtMinute { get; set; }
        public int? FloodLevel { get; set; }
        public int? CurrentLevel { get; set; }
        public bool? Closed { get; set; }
    }

    /// <summary>What the player actually knows about the world (S15) — the World Map renders
    /// from this, never from live RouteStates. Keyed by subject id.</summary>
    public sealed class IntelState
    {
        public Dictionary<string, IntelRecord> Records { get; set; } = new Dictionary<string, IntelRecord>();
    }

    /// <summary>Full 8-state lifecycle from event-system-design.md §5. S14 walks
    /// Undiscovered→Active (discovery), Active→Resolved (ResolveEventCommand), Active→Expired/
    /// PersistentConsequence (hard deadline). Dormant/Triggered/Discovered remain transient
    /// concepts that never persist as an instance state.</summary>
    public enum EventLifecycleState { Dormant, Triggered, Undiscovered, Discovered, Active, Resolved, Expired, PersistentConsequence }

    /// <summary>Replaces the S2-era Id/StatusName stub (S13).</summary>
    public sealed class ActiveEventState
    {
        public string EventInstanceId { get; set; }
        public string EventId { get; set; }
        public EventLifecycleState State { get; set; }
        public long TriggeredAtMinute { get; set; }

        /// <summary>Hard deadline in world minutes; null if the definition has none. Armed when
        /// the instance becomes Active (at trigger, or at discovery for RequiresDiscovery events);
        /// enforced by EventSystem every long-tick (S14).</summary>
        public long? DeadlineMinute { get; set; }

        /// <summary>Soft "act soon" deadline in world minutes (S14); null if none. Same arming
        /// rule as DeadlineMinute.</summary>
        public long? SoftDeadlineMinute { get; set; }

        /// <summary>True once EventDeadlineApproaching has been published for this instance —
        /// serialized so save/load can't re-announce the same soft deadline.</summary>
        public bool SoftDeadlineNotified { get; set; }

        public string ChosenResponse { get; set; }
    }

    /// <summary>Passive tasks (Build) advance every LongTick regardless of player location/sleep —
    /// they're work the shelter itself is doing. Active tasks (S12+: Purify batch, Repair) also
    /// require RequiredWorker to be present at the shelter to advance. S11 only ever creates
    /// Passive tasks (StartBuildCommand); the Active branch exists now so TaskSystem doesn't need
    /// re-architecting when S12 adds the first Active task type.</summary>
    public enum TaskKind { Active, Passive }
    public enum TaskStatus { Running, Paused }

    public sealed class ActiveTaskState
    {
        public string TaskId { get; set; }
        public TaskKind Kind { get; set; }

        /// <summary>What the task acts on — a Build task's TargetId is the BuildSlot id.</summary>
        public string TargetId { get; set; }

        /// <summary>ModuleDefinition id being built — only set for Build tasks (null for other
        /// Passive/Active task kinds S12+ adds).</summary>
        public string ModuleId { get; set; }

        public float Progress { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Running;

        /// <summary>Actor id that must be present at the shelter for an Active task to advance;
        /// null/empty for Passive tasks.</summary>
        public string RequiredWorker { get; set; }
    }

    /// <summary>
    /// Single root Runtime World State (technical-specification.md mục 9/§6).
    /// Plain C# class — no MonoBehaviour/ScriptableObject. Definitions referenced by stable id only.
    /// </summary>
    public sealed class WorldState
    {
        public long WorldTimeMinutes { get; set; }
        public string CurrentDisasterPhase { get; set; } = "normal";

        public Dictionary<string, RouteState> RouteStates { get; set; } = new Dictionary<string, RouteState>();
        public Dictionary<string, LocationState> LocationStates { get; set; } = new Dictionary<string, LocationState>();
        public Dictionary<string, ShelterState> ShelterStates { get; set; } = new Dictionary<string, ShelterState>();
        public Dictionary<string, NpcState> NpcStates { get; set; } = new Dictionary<string, NpcState>();
        public List<ActiveEventState> ActiveEvents { get; set; } = new List<ActiveEventState>();
        public List<ActiveTaskState> ActiveTasks { get; set; } = new List<ActiveTaskState>();

        /// <summary>Reserved-materials pile per task, owner id "task:&lt;TaskId&gt;" (S11) — the
        /// single source of truth for what a Build task has reserved; lazily created by
        /// InventoryOwnerResolver, consumed/returned by TaskSystem/CancelTaskCommand.</summary>
        public Dictionary<string, InventoryState> TaskInventories { get; set; } = new Dictionary<string, InventoryState>();

        public Dictionary<string, bool> PersistentFlags { get; set; } = new Dictionary<string, bool>();

        /// <summary>Player knowledge layer (S15) — written by IntelSystem, read by World Map.</summary>
        public IntelState Intel { get; set; } = new IntelState();

        public ulong RandomSeed { get; set; }
        public Dictionary<string, RngStreamState> RngStreams { get; set; } = new Dictionary<string, RngStreamState>();

        public PlayerState Player { get; set; } = new PlayerState();

        /// <summary>Stable id for this playthrough's telemetry (BL-P1-21), generated once at new game.</summary>
        public string PlaythroughId { get; set; }
    }
}
