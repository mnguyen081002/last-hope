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
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StaircaseZone : MonoBehaviour
    {
        [SerializeField] int lowerFloor;
        [SerializeField] int upperFloor;
        [SerializeField] float bottomY;
        [SerializeField] float topY;

        void OnTriggerEnter2D(Collider2D other)
        {
            var floorState = other.GetComponent<PlayerFloorState>();
            if (floorState == null) return;

            if (floorState.CurrentFloor == lowerFloor) floorState.BeginClimb(lowerFloor, upperFloor);
            else if (floorState.CurrentFloor == upperFloor) floorState.BeginClimb(upperFloor, lowerFloor);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            var floorState = other.GetComponent<PlayerFloorState>();
            if (floorState == null || floorState.TransitioningToFloor == null) return;

            float t = Mathf.InverseLerp(bottomY, topY, other.transform.position.y);
            bool climbingUp = floorState.TransitioningToFloor.Value == upperFloor;
            floorState.SetClimbProgress(climbingUp ? t : 1f - t);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var floorState = other.GetComponent<PlayerFloorState>();
            if (floorState == null) return;

            // Rời vùng cầu thang mà chưa leo hết (progress < 1, CompleteClimb chưa chạy nên
            // TransitioningToFloor vẫn còn) — huỷ, giữ nguyên tầng cũ.
            if (floorState.TransitioningToFloor != null) floorState.CancelClimb();
        }
    }
}
