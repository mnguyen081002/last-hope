using LastHope.Core.State;
using LastHope.Data.Definitions;
using UnityEngine;

namespace LastHope.Systems.Hazard
{
    /// <summary>Chi phí băng qua route theo mực nước — chỉ phần có số thật trong balance.json.</summary>
    public static class HazardSystem
    {
        public static bool IsPassable(FloodState state) => state != FloodState.Impassable;

        /// <summary>Index vào mảng crossing_* — chỉ hợp lệ cho Dry..Deep, không gọi cho Impassable.</summary>
        public static int FloodIndex(FloodState state) => (int)state;

        public static float TimeFactor(FloodState state, HazardBalance balance) =>
            balance.CrossingTimeFactor[FloodIndex(state)];

        /// <summary>Áp chi phí băng qua một lần (gọi mỗi chuyến Travel, giống FatiguePerTravel).</summary>
        public static void ApplyCrossingCost(PlayerState player, FloodState state, HazardBalance balance)
        {
            int index = FloodIndex(state);

            player.Stamina = Mathf.Clamp(player.Stamina - balance.CrossingStaminaCost[index], 0f, 100f);
            player.BlackWaterExposure = Mathf.Clamp(
                player.BlackWaterExposure + balance.CrossingExposureGain[index], 0f, 100f);
            player.Wet = Mathf.Clamp(player.Wet + balance.CrossingWetGain[index], 0f, 100f);
        }
    }
}
