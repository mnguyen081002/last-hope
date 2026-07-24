using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Visual anchor for a Fixed Core Component (main-shelter-design.md §5) that has no
    /// interaction yet in S10 — Main Staircase, Structural Pillars, Electrical Backbone, Water
    /// Intake Point, Roof Antenna Mount. Drain Core is the one exception with real interaction
    /// (see DrainCoreView) since checking Water Intrusion is meaningful the moment it exists.</summary>
    public sealed class CoreComponentView : MonoBehaviour
    {
        [SerializeField] private string coreId;

        public void SetCoreId(string id) => coreId = id;

        private void Awake() => WorldLabel.Create(transform, WorldLabel.Prettify(coreId));
    }
}
