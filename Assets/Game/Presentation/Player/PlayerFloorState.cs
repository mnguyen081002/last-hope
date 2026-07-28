using System;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Tầng hiện tại của player trong scene đang đứng (Z-level kiểu Project Zomboid, thuần
    /// Presentation — không lưu vào <c>WorldState</c>/save, giống quy ước "Save/Load không đổi
    /// scene" đã chấp nhận ở P1). Sống trên GameObject Player (DontDestroyOnLoad) nên giá trị
    /// tồn tại xuyên suốt phiên chơi — <see cref="ResetFloor"/> phải được gọi mỗi khi vào scene
    /// mới (xem <c>SceneFlowController.RepositionPlayer</c>) để không mang nhầm tầng cũ sang.
    /// </summary>
    public class PlayerFloorState : MonoBehaviour
    {
        public int CurrentFloor { get; private set; }

        public event Action<int> FloorChanged;

        public void SetFloor(int floor)
        {
            if (floor == CurrentFloor) return;
            CurrentFloor = floor;
            FloorChanged?.Invoke(floor);
        }

        public void ResetFloor() => SetFloor(0);
    }
}
