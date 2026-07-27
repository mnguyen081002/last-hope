namespace LastHope.Presentation.Interaction
{
    /// <summary>Vật thể trong world người chơi tương tác được bằng phím Interact.</summary>
    public interface IInteractable
    {
        /// <summary>Số giây thực phải giữ phím. 0 = kích hoạt ngay khi nhấn.</summary>
        float HoldDurationSeconds { get; }

        /// <summary>Text hiện trên prompt, vd "Mở kệ nước" / "Giữ để cạy kho".</summary>
        string PromptText { get; }

        /// <summary>Gọi khi giữ đủ thời gian (hoặc ngay khi nhấn nếu HoldDurationSeconds ≤ 0).</summary>
        void Interact();
    }
}
