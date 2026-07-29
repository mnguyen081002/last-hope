using LastHope.Presentation.Player;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Cầu thang leo dần kiểu Project Zomboid — không đổi tầng tức thời ở một điểm, mà tiến
    /// độ nhích liên tục theo vị trí Y thật của player trong vùng cầu thang (giữa
    /// <see cref="bottomY"/> và <see cref="topY"/>), đi lùi lại thì tiến độ tụt lại. Chỉ đổi
    /// tầng thật khi tiến độ chạm 1 (<see cref="PlayerFloorState.SetClimbProgress"/>). Một
    /// GameObject duy nhất phục vụ cả hai chiều — không dùng phím tương tác (xem
    /// isometric-game-placement-rules.md mục 5).
    ///
    /// Kiểm tra "player có trong vùng không" bằng khoảng cách thuần trong <c>Update()</c> —
    /// **không** dùng <c>Collider2D</c>/<c>OnTrigger*2D</c>. Lý do: đây là logic thuần vị trí,
    /// không cần lợi ích gì từ physics engine (không va chạm, không lực), trong khi trigger
    /// event phụ thuộc lịch physics step và từng gây bất tin cậy khó chẩn đoán ở bản trước
    /// (không đi lên được — không rõ Enter/Stay có bắn đủ hay không). Update chạy mỗi frame
    /// chắc chắn, dễ suy luận và dễ debug hơn.
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

            if (inside && !wasInside)
            {
                if (player.CurrentFloor == lowerFloor) player.BeginClimb(lowerFloor, upperFloor);
                else if (player.CurrentFloor == upperFloor) player.BeginClimb(upperFloor, lowerFloor);
            }
            else if (!inside && wasInside && player.TransitioningToFloor != null)
            {
                player.CancelClimb();
            }

            if (inside && player.TransitioningToFloor != null)
            {
                float t = Mathf.InverseLerp(bottomY, topY, pos.y);
                bool climbingUp = player.TransitioningToFloor.Value == upperFloor;
                player.SetClimbProgress(climbingUp ? t : 1f - t);
            }

            wasInside = inside;
        }
    }
}
