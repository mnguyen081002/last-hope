using System.Collections.Generic;

namespace LastHope.Data.Definitions
{
    public sealed class LocationDefinition : DefinitionBase
    {
        public List<string> SearchPointIds { get; set; } = new List<string>();
        public List<string> ConnectedRouteIds { get; set; } = new List<string>();

        /// <summary>Unity scene to load when the player is at this location (S6 SceneFlowController).</summary>
        public string SceneName { get; set; }

        /// <summary>True if this location is a shelter (S7 ConditionSystem: body temp regen,
        /// wet drying). Data-driven rather than a hardcoded location-id check.</summary>
        public bool IsShelter { get; set; }
    }
}
