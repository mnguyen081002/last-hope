using LastHope.Core.Commands;
using LastHope.Presentation.Interaction;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Sprint 6 minimal: interacting submits BeginTravelCommand directly for the single connected
    /// route. Sprint 8 replaces this with publishing WorldMapRequested for route choice among
    /// several connected routes.
    /// </summary>
    public sealed class TravelPointView : MonoBehaviour, IInteractable
    {
        [SerializeField] private string routeId;

        public void SetRouteId(string id) => routeId = id;

        public string PromptText => "Travel";

        public bool CanInteract(GameContext ctx) => true;

        public void Interact(GameContext ctx, CommandProcessor processor)
        {
            processor.Submit(new BeginTravelCommand(ctx.World.Player.ActorId, routeId));
        }
    }
}
