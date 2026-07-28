using System;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Tầng hiện tại của player + tiến độ leo cầu thang (kiểu Project Zomboid — leo dần, không
    /// đổi tầng tức thời). Thuần Presentation — không lưu vào <c>WorldState</c>/save, giống
    /// quy ước "Save/Load không đổi scene" đã chấp nhận ở P1. Sống trên GameObject Player
    /// (DontDestroyOnLoad) nên giá trị tồn tại xuyên suốt phiên chơi — <see cref="ResetFloor"/>
    /// phải được gọi mỗi khi vào scene mới (xem <c>SceneFlowController.RepositionPlayer</c>).
    /// </summary>
    public class PlayerFloorState : MonoBehaviour
    {
        public int CurrentFloor { get; private set; }

        /// <summary>Tầng đang leo tới — null khi không đứng trong vùng cầu thang nào.</summary>
        public int? TransitioningToFloor { get; private set; }

        /// <summary>0 = còn ở hẳn <see cref="CurrentFloor"/>, 1 = đã leo hết tới <see cref="TransitioningToFloor"/>.</summary>
        public float ClimbProgress { get; private set; }

        /// <summary>Bắn mỗi khi có thay đổi cần vẽ lại (đổi tầng hẳn hoặc tiến độ leo nhích).</summary>
        public event Action Changed;

        /// <summary>Vào vùng cầu thang — chỉ nhận nếu đang đứng đúng <paramref name="fromFloor"/>.</summary>
        public void BeginClimb(int fromFloor, int toFloor)
        {
            if (CurrentFloor != fromFloor || TransitioningToFloor == toFloor) return;

            TransitioningToFloor = toFloor;
            ClimbProgress = 0f;
            Changed?.Invoke();
        }

        /// <summary>Gọi liên tục khi còn trong vùng cầu thang — tiến độ nhích theo vị trí thật, có thể lùi lại.</summary>
        public void SetClimbProgress(float progress)
        {
            if (TransitioningToFloor == null) return;

            ClimbProgress = Mathf.Clamp01(progress);
            if (ClimbProgress >= 1f) CompleteClimb();
            else Changed?.Invoke();
        }

        /// <summary>Rời vùng cầu thang giữa chừng (chưa leo hết) — huỷ, giữ nguyên tầng cũ.</summary>
        public void CancelClimb()
        {
            if (TransitioningToFloor == null) return;

            TransitioningToFloor = null;
            ClimbProgress = 0f;
            Changed?.Invoke();
        }

        void CompleteClimb()
        {
            CurrentFloor = TransitioningToFloor.Value;
            TransitioningToFloor = null;
            ClimbProgress = 0f;
            Changed?.Invoke();
        }

        /// <summary>Đổi tầng tức thời, không qua cầu thang — dùng cho Placement Mode (BL-P3-03),
        /// nơi người chơi chọn Zone ở tầng khác mà không cần tự đi lên trước.</summary>
        public void TeleportToFloor(int floor)
        {
            CurrentFloor = floor;
            TransitioningToFloor = null;
            ClimbProgress = 0f;
            Changed?.Invoke();
        }

        public void ResetFloor() => TeleportToFloor(0);
    }
}
