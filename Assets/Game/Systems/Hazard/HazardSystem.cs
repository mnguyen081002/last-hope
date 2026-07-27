using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using UnityEngine;

namespace LastHope.Systems.Hazard
{
    /// <summary>Chi phí băng qua route — chỉ phần có số thật/tự đề xuất trong balance.json.</summary>
    public static class HazardSystem
    {
        public static bool IsPassable(FloodState state) => state != FloodState.Impassable;

        /// <summary>Index vào mảng crossing_* — chỉ hợp lệ cho Dry..Deep, không gọi cho Impassable.</summary>
        public static int FloodIndex(FloodState state) => (int)state;

        public static float TimeFactor(FloodState state, HazardBalance balance) =>
            balance.CrossingTimeFactor[FloodIndex(state)];

        /// <summary>
        /// Flood thực tế áp dụng: Route Closure (theo Disaster Phase) đè lên flood thủ công
        /// nếu route đã tới ngưỡng tự đóng, kể cả khi cheat/state đang set khác.
        /// </summary>
        public static FloodState EffectiveFlood(RouteDefinition route, RouteState state, DisasterPhase currentPhase)
        {
            if (route.ClosesAtPhase.HasValue && currentPhase >= route.ClosesAtPhase.Value)
                return FloodState.Impassable;

            return state.Flood;
        }

        /// <summary>Áp chi phí băng qua Flood một lần (gọi mỗi chuyến Travel, giống FatiguePerTravel).</summary>
        public static void ApplyCrossingCost(PlayerState player, FloodState state, HazardBalance balance)
        {
            int index = FloodIndex(state);

            player.Stamina = Mathf.Clamp(player.Stamina - balance.CrossingStaminaCost[index], 0f, 100f);
            player.BlackWaterExposure = Mathf.Clamp(
                player.BlackWaterExposure + balance.CrossingExposureGain[index], 0f, 100f);
            player.Wet = Mathf.Clamp(player.Wet + balance.CrossingWetGain[index], 0f, 100f);
        }

        /// <summary>
        /// Tốn stamina theo Current Strength, roll rủi ro "cuốn" — trúng thì Health giảm một
        /// khoản cố định. Không có Rope-giảm-rủi-ro trong slice này (cần Equipment System
        /// thật sự cho mặc đồ, để dành P2-C).
        /// </summary>
        public static void ApplyCurrentCrossing(
            PlayerState player, CurrentStrength current, HazardBalance balance, RngStream rng)
        {
            int index = (int)current;
            player.Stamina = Mathf.Clamp(player.Stamina - balance.CurrentStrengthStaminaCost[index], 0f, 100f);

            if (rng.NextChance(balance.CurrentStrengthSweepChancePercent[index]))
            {
                player.Health = Mathf.Max(0f, player.Health - balance.CurrentSweepHealthDamage);
            }
        }

        /// <summary>
        /// Instant Hazard: không kill tức thời — Health dừng ở ngay trên ngưỡng Collapse,
        /// không tự đủ để đưa Health về 0 dù chỉ mỗi Electrified Water.
        /// </summary>
        public static void ApplyElectrifiedCrossing(
            PlayerState player, bool isElectrified, HazardBalance hazardBalance, ConditionBalance conditionBalance)
        {
            if (!isElectrified) return;

            player.Stamina = Mathf.Clamp(player.Stamina - hazardBalance.ElectrifiedWaterStaminaCost, 0f, 100f);

            // Floor không được HỒI máu nếu Health đã thấp hơn floor từ nguồn khác (vd Sick) —
            // Min với giá trị hiện tại đảm bảo chỉ giảm, không bao giờ tăng.
            float floor = conditionBalance.CollapsedHealthThreshold + 1f;
            player.Health = Mathf.Min(player.Health,
                Mathf.Max(floor, player.Health - hazardBalance.ElectrifiedWaterDamage));
        }
    }
}
