using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
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

        /// <summary>Mở panel xác nhận (BL-P2-11) — panel mới thật sự submit <c>BeginTravelCommand</c>.</summary>
        public void Interact() =>
            GameBootstrapper.Services.Events.Publish(new TravelPointOpened(routeId));
    }
}
