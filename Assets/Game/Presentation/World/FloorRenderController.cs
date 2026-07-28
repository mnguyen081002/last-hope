using System.Collections.Generic;
using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Hiện/ẩn từng tầng theo vị trí player, kiểu Project Zomboid: tầng hiện tại vẽ đầy đủ,
    /// tầng ngay dưới vẽ mờ (thấy bố cục nhưng không va chạm được), tầng khác ẩn hẳn. Không
    /// dùng raycast occlusion — chỉ đổi alpha + sortingOrder + bật/tắt Collider2D (xem
    /// isometric-game-placement-rules.md mục 6).
    /// </summary>
    public class FloorRenderController : MonoBehaviour
    {
        /// <summary>Đẩy hẳn xuống dưới mọi thứ của tầng hiện tại, bất kể vị trí Y (CustomAxis
        /// sort theo Y chỉ phá vỡ tie trong cùng order — order thấp hơn luôn vẽ trước).</summary>
        const int DimmedOrderOffset = -1000;

        [SerializeField] float dimmedAlpha = 0.35f;

        readonly Dictionary<SpriteRenderer, int> baseOrder = new();
        PlayerFloorState playerFloorState;

        void Awake()
        {
            Refresh(0); // scene luôn bắt đầu ở tầng 0 — PlayerFloorState.ResetFloor() sẽ khớp lại ngay sau.
        }

        void OnEnable()
        {
            playerFloorState = FindFirstObjectByType<PlayerFloorState>();
            if (playerFloorState != null) playerFloorState.FloorChanged += Refresh;
        }

        void OnDisable()
        {
            if (playerFloorState != null) playerFloorState.FloorChanged -= Refresh;
        }

        void Refresh(int currentFloor)
        {
            foreach (var level in FindObjectsByType<FloorLevel>(FindObjectsSortMode.None))
            {
                int diff = level.Floor - currentFloor;

                if (diff > 0 || diff < -1)
                {
                    level.gameObject.SetActive(false);
                    continue;
                }

                level.gameObject.SetActive(true);
                bool dimmed = diff == -1;
                ApplyRenderers(level.gameObject, dimmed);
                ApplyColliders(level.gameObject, enabled: !dimmed);
            }
        }

        void ApplyRenderers(GameObject root, bool dimmed)
        {
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!baseOrder.TryGetValue(sr, out int order))
                {
                    order = sr.sortingOrder;
                    baseOrder[sr] = order;
                }
                sr.sortingOrder = dimmed ? order + DimmedOrderOffset : order;

                var color = sr.color;
                color.a = dimmed ? dimmedAlpha : 1f;
                sr.color = color;
            }
        }

        static void ApplyColliders(GameObject root, bool enabled)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = enabled;
            }
        }
    }
}
