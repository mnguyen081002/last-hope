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
        public static void ApplyShortTick(PlayerState player, ConditionBalance balance, bool isAtShelter)
        {
            player.Thirst = Clamp100(player.Thirst + balance.ThirstPerHour / 60f);
            player.Hunger = Clamp100(player.Hunger + balance.HungerPerHour / 60f);

            UpdateWet(player, balance, isAtShelter);
            UpdateBodyTemperature(player, balance, isAtShelter);
            UpdateColdFlag(player, balance);
            UpdateStamina(player, balance);
        }

        public static void ApplyLongTick(PlayerState player, ConditionBalance balance)
        {
            player.Fatigue = Clamp100(player.Fatigue + balance.FatiguePerLongTick);

            if (player.Hunger >= 100f || player.Thirst >= 100f)
            {
                player.Health = Mathf.Max(balance.StarvationHealthFloor,
                    player.Health - balance.StarvationHealthDecayPerLongTick);
            }

            UpdateSickFlag(player, balance);
            if (player.IsSick)
            {
                // Không có floor — khác starvation, Sick nặng có thể dẫn tới tử vong (Health = 0).
                player.Health = Mathf.Max(0f, player.Health - balance.SickHealthDecayPerLongTick);
            }
        }

        public static bool IsCollapsed(PlayerState player, ConditionBalance balance) =>
            player.Health <= balance.CollapsedHealthThreshold;

        static void UpdateWet(PlayerState player, ConditionBalance balance, bool isAtShelter)
        {
            if (isAtShelter)
            {
                player.Wet = Mathf.Max(0f, player.Wet - balance.WetDryPerMinuteAtShelter);
            }
            // Wet gain do mưa ambient cần Disaster Phase (P2-B) để biết trời có mưa —
            // chưa nối. Wet hiện chỉ tăng qua Hazard crossing khi hệ thống đó xây xong.
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
