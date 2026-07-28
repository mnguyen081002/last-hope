using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Mở panel chọn số giờ ngủ (Sleep Simulation, BL-P3-13).</summary>
    public class BedView : MonoBehaviour, IInteractable
    {
        public float HoldDurationSeconds => 0f;
        public string PromptText => "Nhấn E để ngủ";

        public void Interact() =>
            GameBootstrapper.Services.Events.Publish(new BedOpened());
    }
}
