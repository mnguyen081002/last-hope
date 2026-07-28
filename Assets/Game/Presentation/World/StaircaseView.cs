using LastHope.Presentation.Interaction;
using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Đổi tầng tức thời (BL-P3-01). IInteractable như mọi prop khác trong game — không dùng
    /// TriggerCollider2D đi-vào-là-kích-hoạt (xem isometric-game-placement-rules.md mục 5).
    /// Toggle GameObject root của 2 tầng bằng SetActive, dời player qua
    /// <see cref="PlayerAvatarSync.TeleportTo"/> (cùng API SceneFlowController dùng để đặt
    /// player tại spawn point).
    /// </summary>
    public class StaircaseView : MonoBehaviour, IInteractable
    {
        [SerializeField] GameObject ownFloorRoot;
        [SerializeField] GameObject otherFloorRoot;
        [SerializeField] Vector2 landingPosition;
        [SerializeField] string promptText = "Đổi tầng";

        public float HoldDurationSeconds => 0f;
        public string PromptText => promptText;

        public void Interact()
        {
            var player = FindFirstObjectByType<PlayerAvatarSync>();
            if (player == null) return;

            ownFloorRoot.SetActive(false);
            otherFloorRoot.SetActive(true);
            player.TeleportTo(landingPosition);
        }
    }
}
