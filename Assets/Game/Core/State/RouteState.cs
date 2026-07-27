namespace LastHope.Core.State
{
    /// <summary>
    /// Mực nước trên route. Dry–Deep ứng index 0–3 trong `balance.json.hazard.crossing_*`.
    /// Impassable là trạng thái riêng, không có index — route bị chặn hoàn toàn, không đi
    /// qua được (tránh softlock bằng route thay thế, không phải bằng cách vẫn cho đi qua).
    /// </summary>
    public enum FloodState
    {
        Dry,
        Shallow,
        Medium,
        Deep,
        Impassable,
    }

    /// <summary>
    /// Lực dòng nước. 5 mức, index 0–4 thẳng vào mảng `current_strength_*` — khác Flood,
    /// **không có mức nào chặn hoàn toàn**: Route đã có Flood/Route Closure lo việc chặn,
    /// Current chỉ tăng rủi ro (sweep) khi băng qua.
    /// </summary>
    public enum CurrentStrength
    {
        None,
        Weak,
        Moderate,
        Strong,
        Extreme,
    }

    public class RouteState
    {
        public FloodState Flood = FloodState.Dry;
        public CurrentStrength Current = CurrentStrength.None;

        /// <summary>
        /// Instant Hazard, set thủ công qua Debug Panel (chưa có nguồn hạ tầng tự động —
        /// Power/Grid thuộc P3). Mỗi lần Travel qua route này gây damage tức thời.
        /// </summary>
        public bool IsElectrified;
    }
}
