using System;
using UnityEngine;

namespace LastHope.Core.Diagnostics
{
    /// <summary>Nhóm log theo hệ thống, bật/tắt độc lập qua <see cref="GameLog.Enabled"/>.</summary>
    [Flags]
    public enum LogCategory
    {
        None = 0,
        Boot = 1 << 0,
        Time = 1 << 1,
        State = 1 << 2,
        Save = 1 << 3,
        Input = 1 << 4,
        Camera = 1 << 5,
        Inventory = 1 << 6,
        Search = 1 << 7,
        Travel = 1 << 8,
        Shelter = 1 << 9,
        Event = 1 << 10,
        Npc = 1 << 11,
        All = ~0
    }

    /// <summary>
    /// Log tập trung của game. Mọi hệ thống log qua đây thay vì gọi thẳng <see cref="Debug"/>
    /// để có prefix category thống nhất và tắt được theo nhóm khi debug.
    /// </summary>
    public static class GameLog
    {
        public static LogCategory Enabled = LogCategory.All;

        public static void Info(LogCategory category, string message)
        {
            if (IsEnabled(category)) Debug.Log(Format(category, message));
        }

        public static void Warn(LogCategory category, string message)
        {
            if (IsEnabled(category)) Debug.LogWarning(Format(category, message));
        }

        /// <summary>Error luôn ghi, không chịu ảnh hưởng của <see cref="Enabled"/>.</summary>
        public static void Error(LogCategory category, string message)
        {
            Debug.LogError(Format(category, message));
        }

        public static bool IsEnabled(LogCategory category) => (Enabled & category) != 0;

        static string Format(LogCategory category, string message) => $"[{category}] {message}";
    }
}
