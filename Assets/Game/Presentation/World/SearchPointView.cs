using LastHope.Presentation.Interaction;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Registry;
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

        /// <summary>
        /// Đã cạy/mở lần đầu rồi thì các lần sau mở lại tức thì — không phải giữ phím nữa
        /// (thao tác khó chỉ xảy ra một lần, không phải mỗi lần quay lại).
        /// </summary>
        public float HoldDurationSeconds
        {
            get
            {
                if (!GameBootstrapper.IsReady) return 0f;

                var services = GameBootstrapper.Services;
                if (!services.Definitions.TryGetSearchPoint(searchPointId, out var definition)) return 0f;
                if (AlreadyOpened(services, definition.LocationId)) return 0f;

                return definition.OpenHoldSeconds;
            }
        }

        public string PromptText => HoldDurationSeconds > 0f ? "Giữ E để cạy" : "Nhấn E để mở";

        public void Interact() =>
            GameBootstrapper.Services.Commands.Submit(new OpenSearchPointCommand(searchPointId));

        bool AlreadyOpened(GameServices services, string locationId)
        {
            var location = services.World.GetOrCreateLocation(locationId);
            return location.SearchPoints.TryGetValue(searchPointId, out var state) && state.Rolled;
        }
    }
}
