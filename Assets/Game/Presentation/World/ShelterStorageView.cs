using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Binds a scene object to a shelter's storage container (BL-P1-18). No sim mutation
    /// is needed to open it — just tells the UI which owner id to display.</summary>
    public sealed class ShelterStorageView : MonoBehaviour, IInteractable
    {
        [SerializeField] private string shelterId = "shelter_main";

        public void SetShelterId(string id) => shelterId = id;

        public string PromptText => "Storage";

        private void Awake() => WorldLabel.Create(transform, "Storage");

        public bool CanInteract(GameContext ctx) => true;

        public void Interact(GameContext ctx, CommandProcessor processor)
        {
            ctx.Events.Publish(new ContainerViewRequested("shelter_storage:" + shelterId, shelterId));
        }
    }
}
