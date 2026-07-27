using LastHope.Data.Definitions;

namespace LastHope.Systems.Hazard
{
    /// <summary>Suy phase thuần từ world time — không lưu state riêng (đơn điệu tăng).</summary>
    public static class DisasterPhaseSystem
    {
        public static DisasterPhase CurrentPhase(long worldTimeMinutes, DisasterPhaseBalance balance)
        {
            if (worldTimeMinutes >= balance.RouteClosureAtMinute) return DisasterPhase.RouteClosure;
            if (worldTimeMinutes >= balance.BlackRainAtMinute) return DisasterPhase.BlackRain;
            if (worldTimeMinutes >= balance.FirstRainAtMinute) return DisasterPhase.FirstRain;
            return DisasterPhase.Dry;
        }

        public static bool IsRaining(DisasterPhase phase) => phase >= DisasterPhase.FirstRain;
    }
}
