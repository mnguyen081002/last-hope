namespace LastHope.Core.Time
{
    /// <summary>
    /// Đổi giây thực thành phút game. Giữ accumulator giây game nội bộ và chỉ "bank" ra
    /// phút nguyên — phần lẻ ở lại accumulator nên chạy 24h với delta thay đổi liên tục
    /// vẫn không trôi thời gian.
    /// </summary>
    public class SimulationClock
    {
        public const float DefaultTimeScale = 5f;

        double accumulatedGameSeconds;

        /// <summary>Số giây game trôi qua trên mỗi giây thực.</summary>
        public float TimeScale { get; set; } = DefaultTimeScale;

        public bool Paused { get; set; }

        /// <summary>Phần giây game chưa đủ 1 phút. Chỉ dùng để chẩn đoán/test.</summary>
        public double PendingSeconds => accumulatedGameSeconds;

        /// <summary>
        /// Nạp thời gian thực. Trả về số **phút game nguyên** vừa tích đủ; phần dư giữ lại.
        /// </summary>
        public int AccumulateRealSeconds(float realSeconds)
        {
            if (Paused || realSeconds <= 0f) return 0;

            accumulatedGameSeconds += realSeconds * TimeScale;

            int minutes = (int)(accumulatedGameSeconds / 60.0);
            if (minutes > 0) accumulatedGameSeconds -= minutes * 60.0;
            return minutes;
        }

        public void Reset() => accumulatedGameSeconds = 0.0;
    }
}
