namespace LastHope.Core.UI
{
    /// <summary>
    /// Cờ dùng chung: panel OnGUI gọi <see cref="MarkHover"/> mỗi frame với true/false theo
    /// con trỏ có nằm trong rect của nó không. Gameplay (vd. <c>CameraRig</c> zoom) đọc qua
    /// <see cref="ConsumeIsHovering"/> để bỏ qua input khi chuột đang thao tác UI.
    ///
    /// IMGUI (OnGUI) và Input System là hai đường xử lý input tách biệt — <c>Event.current.Use()</c>
    /// chỉ chặn được sự kiện phía IMGUI, không chặn Input System đọc scroll wheel trực tiếp
    /// từ thiết bị, nên cần cờ riêng thay vì trông chờ IMGUI "nuốt" input hộ.
    ///
    /// OnGUI chạy sau LateUpdate trong cùng 1 frame (thứ tự cố định của Unity), nên
    /// <see cref="ConsumeIsHovering"/> luôn trả giá trị của frame trước (trễ đúng 1 frame) —
    /// đủ nhanh để chặn zoom khi đang cuộn panel. Đọc xong tự xoá về false để OnGUI frame
    /// hiện tại gom lại từ đầu, tránh cờ bị "kẹt" true khi panel đóng giữa chừng.
    /// </summary>
    public static class PointerOverUI
    {
        static bool accumulator;

        public static void MarkHover(bool isHovering) => accumulator |= isHovering;

        public static bool ConsumeIsHovering()
        {
            bool result = accumulator;
            accumulator = false;
            return result;
        }
    }
}
