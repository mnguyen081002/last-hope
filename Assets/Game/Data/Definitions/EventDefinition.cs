using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Schema follows event-system-design.md §4/§25 in full for future sprints, but S13 only
    /// consumes the Trigger→Active→Resolved path — Priority/EventType/Scope are display-only this
    /// sprint (no Event Budget enforcement, no Discovery, no Expiration yet, those are S14).
    /// Trigger model is a flat AND of whichever optional fields are set (not the full Compound
    /// trigger tree from the design doc) — a real trigger-tree evaluator is only worth building
    /// once more than 3 events exist to justify it.
    /// </summary>
    public sealed class EventDefinition : DefinitionBase
    {
        public string EventType { get; set; }
        public string Priority { get; set; }
        public string Scope { get; set; }

        /// <summary>Phase trigger: current phase id must equal this (optional).</summary>
        public string TriggerPhaseId { get; set; }

        /// <summary>Phase trigger: current phase's BlackWater flag must be true (optional).</summary>
        public bool TriggerRequiresBlackWater { get; set; }

        /// <summary>State trigger: shelter WaterIntrusion.Level must be at least this
        /// (WaterIntrusionLevel name, e.g. "Shallow"; optional).</summary>
        public string TriggerStateMinLevel { get; set; }

        /// <summary>State trigger: shelter must have an active module tagged "pump" (optional).</summary>
        public bool TriggerRequiresPumpModule { get; set; }

        /// <summary>Chance trigger: rolled via RNG stream "events" once per long-tick when every
        /// other condition already holds; 0 = no chance gate (trigger as soon as conditions hold).</summary>
        public int TriggerChancePercentPerLongTick { get; set; }

        /// <summary>Offset in minutes from the moment the instance becomes Active (trigger, or
        /// discovery for RequiresDiscovery events); 0 = none. Soft publishes
        /// EventDeadlineApproaching once; hard expires the event (S14).</summary>
        public int SoftDeadlineMinutes { get; set; }
        public int HardDeadlineMinutes { get; set; }

        /// <summary>When true the instance starts Undiscovered (S14): invisible to player UI and
        /// unresolvable until discovered. Discovery source today is being at a shelter
        /// (shelter-scope events); radio/NPC sources arrive with S15 Intel. Deadlines arm at
        /// discovery, not trigger.</summary>
        public bool RequiresDiscovery { get; set; }

        /// <summary>Event Chain (S14): this event id is force-triggered (bypassing its trigger
        /// conditions) when the current event is Resolved or Expired; null/empty = no chain.</summary>
        public string NextEventId { get; set; }

        /// <summary>Persistent Consequence on expiration (S14): flag names added to
        /// ShelterState.EventFlags / set true in WorldState.PersistentFlags when the hard deadline
        /// passes unresolved. Any entry makes the instance end as PersistentConsequence instead of
        /// Expired.</summary>
        public List<string> ExpirationShelterFlags { get; set; } = new List<string>();
        public List<string> ExpirationPersistentFlags { get; set; } = new List<string>();

        public List<string> AvailableResponses { get; set; } = new List<string>();

        /// <summary>Role tag read by EventSystem/ResolveEventCommand instead of switching on Id —
        /// same convention as ModuleDefinition.Tags.</summary>
        public List<string> Tags { get; set; } = new List<string>();
    }
}
