using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using UnityEngine;

namespace LastHope.Presentation.World
{
    public class TravelPointView : MonoBehaviour, IInteractable
    {
        [SerializeField] string routeId;
        [SerializeField] string destinationLabel = "";

        public float HoldDurationSeconds => 0f;
        public string PromptText => string.IsNullOrEmpty(destinationLabel)
            ? "Nhấn E để di chuyển"
            : $"Nhấn E để đi tới {destinationLabel}";

        public void Interact() =>
            GameBootstrapper.Services.Commands.Submit(new BeginTravelCommand(routeId));
    }
}
