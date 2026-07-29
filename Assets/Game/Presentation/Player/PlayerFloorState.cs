using System;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Tầng hiện tại của player + trạng thái blend khi đứng trong vùng cầu thang (kiểu Project
    /// Zomboid — leo dần, không đổi tầng tức thời). Thuần Presentation — không lưu vào
    /// <c>WorldState</c>/save, giống quy ước "Save/Load không đổi scene" đã chấp nhận ở P1.
    /// Sống trên GameObject Player (DontDestroyOnLoad) nên giá trị tồn tại xuyên suốt phiên
    /// chơi — <see cref="ResetFloor"/> phải được gọi mỗi khi vào scene mới (xem
    /// <c>SceneFlowController.RepositionPlayer</c>).
    ///
    /// Cố ý KHÔNG lưu "hướng đang leo" — <see cref="BlendT"/> luôn được tính lại thuần theo vị
    /// trí hình học mỗi frame bởi <see cref="StaircaseZone"/>, không suy ra một lần rồi cache.
    /// Bản trước cache hướng lúc bắt đầu leo (dựa theo CurrentFloor cũ) gây lệch khi đứng nán ở
    /// biên vùng hoặc vào từ hai đầu khác nhau — xem docs/plans/2026-07-29-staircase-blend-fix.md.
    /// </summary>
    public class PlayerFloorState : MonoBehaviour
    {
        public int CurrentFloor { get; private set; }

        public int? BlendLowerFloor { get; private set; }
        public int? BlendUpperFloor { get; private set; }

        /// <summary>0 = hoàn toàn BlendLowerFloor, 1 = hoàn toàn BlendUpperFloor.</summary>
        public float BlendT { get; private set; }

        public bool IsBlending => BlendLowerFloor.HasValue;

        /// <summary>Bắn mỗi khi có thay đổi cần vẽ lại.</summary>
        public event Action Changed;

        /// <summary>Gọi mỗi frame còn trong vùng cầu thang — <paramref name="t"/> tính thuần từ vị trí, không phụ thuộc lịch sử.</summary>
        public void UpdateBlend(int lowerFloor, int upperFloor, float t)
        {
            BlendLowerFloor = lowerFloor;
            BlendUpperFloor = upperFloor;
            BlendT = Mathf.Clamp01(t);
            Changed?.Invoke();
        }

        /// <summary>Rời vùng cầu thang — chốt CurrentFloor theo BlendT lúc rời (gần tầng nào hơn).</summary>
        public void EndBlend()
        {
            if (!BlendLowerFloor.HasValue) return;

            CurrentFloor = BlendT >= 0.5f ? BlendUpperFloor.Value : BlendLowerFloor.Value;
            BlendLowerFloor = null;
            BlendUpperFloor = null;
            BlendT = 0f;
            Changed?.Invoke();
        }

        /// <summary>Đổi tầng tức thời, không qua cầu thang — dùng cho Placement Mode (BL-P3-03),
        /// nơi người chơi chọn Zone ở tầng khác mà không cần tự đi lên trước.</summary>
        public void TeleportToFloor(int floor)
        {
            CurrentFloor = floor;
            BlendLowerFloor = null;
            BlendUpperFloor = null;
            BlendT = 0f;
            Changed?.Invoke();
        }

        public void ResetFloor() => TeleportToFloor(0);
    }
}
