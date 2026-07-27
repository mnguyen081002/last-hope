using LastHope.Core.State;
using LastHope.Data.Definitions;
using UnityEngine;

namespace LastHope.Systems.Condition
{
    /// <summary>
    /// Tick thuần theo <c>balance.json.condition</c>. Xem
    /// docs/plans/2026-07-27-p2a-condition-core.md cho ngữ nghĩa từng field (doc gốc chỉ mô
    /// tả định tính, công thức cụ thể suy ra và ghi rõ ở đó).
    /// </summary>
    public static class ConditionSystem
    {
        public static void ApplyShortTick(
            PlayerState player, ConditionBalance balance, bool isAtShelter, bool isRaining = false)
        {
            player.Thirst = Clamp100(player.Thirst + balance.ThirstPerHour / 60f);
            player.Hunger = Clamp100(player.Hunger + balance.HungerPerHour / 60f);

            UpdateWet(player, balance, isAtShelter, isRaining);
            UpdateBodyTemperature(player, balance, isAtShelter);
            UpdateColdFlag(player, balance);
            UpdateStamina(player, balance);

            UpdateSickFlag(player, balance);
            if (player.IsSick)
            {
                // Cả 3 cùng xấu đi mỗi phút: Thirst/Hunger tăng thêm, Health giảm — không
                // floor (khác starvation), Sick nặng có thể dẫn tới tử vong (Health = 0).
                player.Thirst = Clamp100(player.Thirst + balance.SickDecayPerMinute);
                player.Hunger = Clamp100(player.Hunger + balance.SickDecayPerMinute);
                player.Health = Mathf.Max(0f, player.Health - balance.SickDecayPerMinute);
            }
        }

        public static void ApplyLongTick(PlayerState player, ConditionBalance balance)
        {
            player.Fatigue = Clamp100(player.Fatigue + balance.FatiguePerLongTick);

            if (player.Hunger >= 100f || player.Thirst >= 100f)
            {
                // Min-Max: floor không được HỒI máu nếu Health đã thấp hơn floor từ nguồn
                // khác (vd Sick không floor) — chỉ ngăn CHÍNH starvation kéo xuống dưới floor.
                player.Health = Mathf.Min(player.Health, Mathf.Max(
                    balance.StarvationHealthFloor, player.Health - balance.StarvationHealthDecayPerLongTick));
            }
        }

        public static bool IsCollapsed(PlayerState player, ConditionBalance balance) =>
            player.Health <= balance.CollapsedHealthThreshold;

        static void UpdateWet(PlayerState player, ConditionBalance balance, bool isAtShelter, bool isRaining)
        {
            if (isAtShelter)
            {
                player.Wet = Mathf.Max(0f, player.Wet - balance.WetDryPerMinuteAtShelter);
            }
            else if (isRaining)
            {
                player.Wet = Clamp100(player.Wet + balance.WetGainPerMinuteInRain);
            }
        }

        static void UpdateBodyTemperature(PlayerState player, ConditionBalance balance, bool isAtShelter)
        {
            if (player.Wet >= balance.WetThresholdForTempDrift)
            {
                player.BodyTemperature -= balance.BodyTempDriftDownPerMinute;
            }
            else if (isAtShelter)
            {
                player.BodyTemperature = Mathf.Min(37f,
                    player.BodyTemperature + balance.BodyTempRegenAtShelterPerMinute);
            }
        }

        static void UpdateColdFlag(PlayerState player, ConditionBalance balance)
        {
            if (player.BodyTemperature <= balance.ColdBodyTempThreshold) player.IsCold = true;
            else if (player.BodyTemperature >= balance.ColdClearBodyTempThreshold) player.IsCold = false;
            // Giữa hai ngưỡng: giữ nguyên trạng thái trước đó (hysteresis, tránh nhấp nháy ở biên).
        }

        static void UpdateStamina(PlayerState player, ConditionBalance balance)
        {
            bool halved = player.Fatigue >= 50f || player.Thirst >= 70f || player.IsCold;
            float regen = balance.StaminaRegenPerMinute * (halved ? balance.StaminaRegenHalvedMultiplier : 1f);
            player.Stamina = Clamp100(player.Stamina + regen);
        }

        static void UpdateSickFlag(PlayerState player, ConditionBalance balance)
        {
            if (player.BlackWaterExposure >= balance.SickExposureThreshold) player.IsSick = true;
        }

        static float Clamp100(float value) => Mathf.Clamp(value, 0f, 100f);
    }
}
