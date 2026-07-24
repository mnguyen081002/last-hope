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

    public sealed class NpcState { public string Id { get; set; } public string StatusName { get; set; } = "Unknown"; }
    public sealed class ActiveEventState { public string Id { get; set; } public string StatusName { get; set; } = "Active"; }

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

        public ulong RandomSeed { get; set; }
        public Dictionary<string, RngStreamState> RngStreams { get; set; } = new Dictionary<string, RngStreamState>();

        public PlayerState Player { get; set; } = new PlayerState();

        /// <summary>Stable id for this playthrough's telemetry (BL-P1-21), generated once at new game.</summary>
        public string PlaythroughId { get; set; }
    }
}
