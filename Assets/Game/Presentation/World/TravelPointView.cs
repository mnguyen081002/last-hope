using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Sprint 8: interacting opens the World Map (WorldMapRequested) so the player can compare
    /// every route connected to the current location — ETA, flood/current, return window — before
    /// picking one to travel. No longer bound to a single route (S6 had it submit BeginTravelCommand
    /// directly for one hardcoded route id) — WorldMapPanel enumerates LocationDefinition.
    /// ConnectedRouteIds for the player's current location itself.
    /// </summary>
    public sealed class TravelPointView : MonoBehaviour, IInteractable
    {
        public string PromptText => "Travel";

        public bool CanInteract(GameContext ctx) => true;

        public void Interact(GameContext ctx, CommandProcessor processor)
        {
            ctx.Events.Publish(new WorldMapRequested());
        }
    }
}
