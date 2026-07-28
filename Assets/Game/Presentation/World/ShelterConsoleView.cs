using LastHope.Core.Events;
using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Mở toàn bộ giao diện quản lý Shelter (Zone/Slot/Power/Water) — chỉ một Shelter trong MVP.</summary>
    public class ShelterConsoleView : MonoBehaviour, IInteractable
    {
        public float HoldDurationSeconds => 0f;
        public string PromptText => "Nhấn E để quản lý Shelter";

        public void Interact() =>
            GameBootstrapper.Services.Events.Publish(new ShelterConsoleOpened());
    }
}
