using LastHope.Core.Commands;
using LastHope.Presentation.Interaction;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Binds a scene object to a SearchPointDefinition id (BL-P1-17).</summary>
    public sealed class SearchPointView : MonoBehaviour, IInteractable
    {
        [SerializeField] private string searchPointId;

        public void SetSearchPointId(string id) => searchPointId = id;

        public string PromptText => "Search";

        private void Awake() => WorldLabel.Create(transform, $"Search\n{searchPointId}");

        public bool CanInteract(GameContext ctx) => true;

        public void Interact(GameContext ctx, CommandProcessor processor)
        {
            processor.Submit(new OpenSearchPointCommand(ctx.World.Player.ActorId, searchPointId));
        }
    }
}
