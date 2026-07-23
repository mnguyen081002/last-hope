using UnityEngine;

namespace LastHope.Core.Logging
{
    public enum LogCategory
    {
        Boot,
        World,
        Input,
        Save,
        Debug
    }

    public static class GameLog
    {
        public static void Info(LogCategory category, string message)
        {
            Debug.Log($"[{category}] {message}");
        }

        public static void Warn(LogCategory category, string message)
        {
            Debug.LogWarning($"[{category}] {message}");
        }

        public static void Error(LogCategory category, string message)
        {
            Debug.LogError($"[{category}] {message}");
        }
    }
}
