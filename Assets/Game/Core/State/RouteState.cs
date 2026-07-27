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

    public class RouteState
    {
        public FloodState Flood = FloodState.Dry;
    }
}
