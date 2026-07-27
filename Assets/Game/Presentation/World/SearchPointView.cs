using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Chỉ submit lệnh, không biết UI nào sẽ hiện kết quả — <c>OpenSearchPointCommand</c>
    /// publish <c>SearchPointOpened</c>, panel bên UI tự nghe (Presentation không phụ thuộc UI).
    /// </summary>
    public class SearchPointView : MonoBehaviour, IInteractable
    {
        [SerializeField] string searchPointId;

        public float HoldDurationSeconds =>
            GameBootstrapper.IsReady
            && GameBootstrapper.Services.Definitions.TryGetSearchPoint(searchPointId, out var definition)
                ? definition.OpenHoldSeconds
                : 0f;

        public string PromptText => HoldDurationSeconds > 0f ? "Giữ E để cạy" : "Nhấn E để mở";

        public void Interact() =>
            GameBootstrapper.Services.Commands.Submit(new OpenSearchPointCommand(searchPointId));
    }
}
