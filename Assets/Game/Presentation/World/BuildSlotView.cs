using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Visual placeholder for one Build Slot (S10 blockout) — binds a scene position to
    /// a slot id from ShelterZoneDefinition.BuildSlotIds. Not interactable yet: placement/building
    /// is S11's BuildPanel, which reads slot ids from ShelterState.BuildSlots directly rather than
    /// through world interaction.</summary>
    public sealed class BuildSlotView : MonoBehaviour
    {
        [SerializeField] private string slotId;

        public void SetSlotId(string id) => slotId = id;

        private void Awake()
        {
            string displayId = slotId.StartsWith("slot_") ? slotId.Substring("slot_".Length) : slotId;
            WorldLabel.Create(transform, $"Slot\n{WorldLabel.Prettify(displayId)}");
        }
    }
}
