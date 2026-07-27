using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Kho shelter luôn mở sẵn (không roll, không hold) — chỉ publish để UI hiện panel.</summary>
    public class StorageView : MonoBehaviour, IInteractable
    {
        [SerializeField] string locationId;

        public float HoldDurationSeconds => 0f;
        public string PromptText => "Nhấn E để mở kho";

        public void Interact() =>
            GameBootstrapper.Services.Events.Publish(new StorageOpened(locationId));
    }
}
