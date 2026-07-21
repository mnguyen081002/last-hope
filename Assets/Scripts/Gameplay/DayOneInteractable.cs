using UnityEngine;

namespace LastHope.Gameplay
{
    public sealed class DayOneInteractable : MonoBehaviour
    {
        [SerializeField] private string interactionId = string.Empty;
        [SerializeField] private string displayName = string.Empty;

        public string InteractionId => interactionId;
        public string DisplayName => displayName;

        public void Configure(string id, string label)
        {
            interactionId = id;
            displayName = label;
        }
    }
}
