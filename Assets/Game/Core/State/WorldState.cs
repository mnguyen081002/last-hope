using System.Collections.Generic;
using LastHope.Core.Random;

namespace LastHope.Core.State
{
    // Minimal Sprint 2 stubs — each grows into its full shape when the owning system
    // (Hazard/Shelter/NPC/Event/Task) is implemented. Kept here because WorldState must
    // reference something serializable today; do not add fields speculatively beyond
    // what the current sprint's systems need.
    public sealed class RouteState { public string Id { get; set; } public string StatusName { get; set; } = "Dry"; }
    public sealed class LocationState { public string Id { get; set; } public string StatusName { get; set; } = "Normal"; }
    public sealed class ShelterState { public string Id { get; set; } public string StatusName { get; set; } = "Normal"; }
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
    }
}
