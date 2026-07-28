using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Đổi tầng bằng cách đi bộ qua, kiểu Project Zomboid — không cần bấm phím (khác mọi
    /// prop tương tác khác trong game, cố ý: đổi tầng là di chuyển thuần tuý, không có hệ quả
    /// cần xác nhận, xem isometric-game-placement-rules.md mục 5). Đặt <c>Collider2D.isTrigger
    /// = true</c> trên cùng GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FloorTransitionTrigger : MonoBehaviour
    {
        [SerializeField] int targetFloor;

        void OnTriggerEnter2D(Collider2D other)
        {
            var floorState = other.GetComponent<PlayerFloorState>();
            if (floorState == null) return;

            floorState.SetFloor(targetFloor);
        }
    }
}
