using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Cầu thang leo dần kiểu Project Zomboid — không đổi tầng tức thời ở một điểm, mà tiến
    /// độ nhích liên tục theo vị trí Y thật của player trong vùng cầu thang (giữa
    /// <see cref="bottomY"/> và <see cref="topY"/>), đi lùi lại thì tiến độ tụt lại. Một
    /// GameObject duy nhất phục vụ cả hai chiều — không dùng phím tương tác (xem
    /// isometric-game-placement-rules.md mục 5).
    ///
    /// Theo dõi bằng <c>Update()</c> mỗi frame (không phải <c>OnTriggerStay2D</c>/Collider2D —
    /// xem ghi chú trong lịch sử sửa lỗi trước). Tiến độ (<c>PlayerFloorState.BlendT</c>) tính
    /// THUẦN theo vị trí hình học mỗi frame (<c>InverseLerp(bottomY, topY, playerY)</c>) —
    /// KHÔNG suy hoặc cache "hướng đang leo" từ trạng thái cũ. Bản trước suy hướng một lần lúc
    /// bắt đầu (dựa theo CurrentFloor tại thời điểm đó) rồi cache, gây lệch khi đứng nán ở biên
    /// vùng hoặc vào từ hai đầu khác nhau (không đối xứng) — xem
    /// docs/plans/2026-07-29-staircase-blend-fix.md.
    /// </summary>
    public class StaircaseZone : MonoBehaviour
    {
        [SerializeField] int lowerFloor;
        [SerializeField] int upperFloor;
        [SerializeField] float bottomY;
        [SerializeField] float topY;
        [SerializeField] float halfWidth = 0.75f;

        PlayerFloorState player;
        bool wasInside;

        void Start()
        {
            player = FindFirstObjectByType<PlayerFloorState>();
        }

        void Update()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerFloorState>();
                if (player == null) return;
            }

            Vector3 pos = player.transform.position;
            bool inside = Mathf.Abs(pos.x - transform.position.x) <= halfWidth && pos.y >= bottomY && pos.y <= topY;

            if (inside)
            {
                float t = Mathf.InverseLerp(bottomY, topY, pos.y);
                player.UpdateBlend(lowerFloor, upperFloor, t);
            }
            else if (wasInside)
            {
                player.EndBlend();
            }

            wasInside = inside;
        }
    }
}
