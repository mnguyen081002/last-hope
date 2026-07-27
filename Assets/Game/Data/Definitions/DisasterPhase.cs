namespace LastHope.Data.Definitions
{
    /// <summary>
    /// Timeline thảm họa, suy ra thuần từ <c>WorldTimeMinutes</c> — không lưu state riêng
    /// (đơn điệu tăng theo giờ chơi, không cần hysteresis như Cold/Sick). Nằm ở Data (không
    /// phải Core) vì <see cref="RouteDefinition.ClosesAtPhase"/> cần tham chiếu — Data không
    /// được phụ thuộc Core (ngược hướng dependency Data → Core → Systems).
    /// </summary>
    public enum DisasterPhase
    {
        Dry,
        FirstRain,
        BlackRain,
        RouteClosure,
    }
}
