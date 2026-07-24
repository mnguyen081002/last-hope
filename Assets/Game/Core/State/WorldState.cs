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

    /// <summary>Storage added Sprint 6 (BL-P1-18) — unlimited-capacity shelter container,
    /// lazily created by InventoryOwnerResolver on first access. S10 adds the real shelter
    /// simulation fields (WaterIntrusionSystem owns seeding + upkeep of this shelter's state —
    /// see WaterIntrusionSystem.Resync). PowerState/Modules arrive with S12/S11 respectively —
    /// not declared here yet, their owning system doesn't exist.</summary>
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
        public WaterStocksState WaterStocks { get; set; } = new WaterStocksState();

        /// <summary>Named boolean flags for shelter-scoped conditions no dedicated field exists
        /// for yet (e.g. "ground_floor_lost", "lower_floor_power_locked") — same role as
        /// WorldState.PersistentFlags but scoped to one shelter.</summary>
        public HashSet<string> EventFlags { get; set; } = new HashSet<string>();
    }

    public sealed class NpcState { public string Id { get; set; } public string StatusName { get; set; } = "Unknown"; }
    public sealed class ActiveEventState { public string Id { get; set; } public string StatusName { get; set; } = "Active"; }
    public sealed class ActiveTaskState { public string Id { get; set; } public string StatusName { get; set; } = "Queued"; }

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

        public Dictionary<string, bool> PersistentFlags { get; set; } = new Dictionary<string, bool>();

        public ulong RandomSeed { get; set; }
        public Dictionary<string, RngStreamState> RngStreams { get; set; } = new Dictionary<string, RngStreamState>();

        public PlayerState Player { get; set; } = new PlayerState();

        /// <summary>Stable id for this playthrough's telemetry (BL-P1-21), generated once at new game.</summary>
        public string PlaythroughId { get; set; }
    }
}
