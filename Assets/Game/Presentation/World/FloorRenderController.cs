using System.Collections.Generic;
using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Hiện/ẩn từng tầng theo vị trí player, kiểu Project Zomboid: tầng hiện tại vẽ đầy đủ,
    /// tầng ngay dưới vẽ mờ (thấy bố cục nhưng không va chạm được), tầng khác ẩn hẳn. Trong
    /// lúc đứng trong vùng cầu thang (<see cref="PlayerFloorState.IsBlending"/>), hai tầng
    /// liên quan mờ/rõ dần theo <see cref="PlayerFloorState.BlendT"/> thay vì đổi nhị phân —
    /// đúng cảm giác đang leo, không phải dịch chuyển tức thời. Không dùng raycast occlusion —
    /// chỉ đổi alpha + sortingOrder + bật/tắt Collider2D (xem isometric-game-placement-rules.md
    /// mục 6).
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
            Refresh(); // scene luôn bắt đầu ở tầng 0 — PlayerFloorState.ResetFloor() sẽ khớp lại ngay sau.
        }

        void OnEnable()
        {
            playerFloorState = FindFirstObjectByType<PlayerFloorState>();
            if (playerFloorState != null) playerFloorState.Changed += Refresh;
        }

        void OnDisable()
        {
            if (playerFloorState != null) playerFloorState.Changed -= Refresh;
        }

        void Refresh()
        {
            int settledFloor = playerFloorState != null ? playerFloorState.CurrentFloor : 0;
            bool blending = playerFloorState != null && playerFloorState.IsBlending;
            int blendLower = blending ? playerFloorState.BlendLowerFloor.Value : 0;
            int blendUpper = blending ? playerFloorState.BlendUpperFloor.Value : 0;
            float t = blending ? playerFloorState.BlendT : 0f;

            // FindObjectsByType(sortMode) — overload 1 tham số — mặc định LOẠI TRỪ GameObject
            // inactive. Tầng chưa từng active (vd Upper lúc mới vào scene) sẽ không bao giờ
            // được tìm thấy lại nếu dùng overload đó, nên Refresh chỉ toàn thấy tầng đang
            // active — tầng kia không bao giờ được bật lên được. Phải truyền rõ Include.
            foreach (var level in FindObjectsByType<FloorLevel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (blending && level.Floor == blendUpper)
                {
                    SetFloorVisual(level.gameObject, Mathf.Lerp(dimmedAlpha, 1f, t), collidable: t >= 0.5f);
                    continue;
                }

                if (blending && level.Floor == blendLower)
                {
                    SetFloorVisual(level.gameObject, Mathf.Lerp(1f, dimmedAlpha, t), collidable: t < 0.5f);
                    continue;
                }

                int diff = level.Floor - settledFloor;
                if (diff > 0 || diff < -1)
                {
                    level.gameObject.SetActive(false);
                    continue;
                }

                bool dimmed = diff == -1;
                SetFloorVisual(level.gameObject, dimmed ? dimmedAlpha : 1f, collidable: !dimmed);
            }
        }

        void SetFloorVisual(GameObject root, float alpha, bool collidable)
        {
            root.SetActive(true);
            bool dimmed = alpha < 1f;

            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!baseOrder.TryGetValue(sr, out int order))
                {
                    order = sr.sortingOrder;
                    baseOrder[sr] = order;
                }
                sr.sortingOrder = dimmed ? order + DimmedOrderOffset : order;

                var color = sr.color;
                color.a = alpha;
                sr.color = color;
            }

            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = collidable;
            }
        }
    }
}
