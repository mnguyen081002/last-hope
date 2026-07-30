using System.Collections.Generic;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Nguồn sprite runtime duy nhất cho Shelter Module. Art nằm trong Resources để cả ghost
    /// ở Persistent scene và renderer ở Main Shelter cùng resolve theo một quy ước tên file.
    /// </summary>
    public static class ModuleSpriteCatalog
    {
        const string ResourceRoot = "Art/ShelterModulesP3";
        static readonly Dictionary<string, Sprite> Cache = new();

        public static int NormalizeQuarterTurns(int quarterTurns) =>
            ((quarterTurns % 4) + 4) % 4;

        public static string ResourcePath(string moduleId, int quarterTurns) =>
            $"{ResourceRoot}/{moduleId}_r{NormalizeQuarterTurns(quarterTurns) * 90:000}";

        public static Sprite Load(string moduleId, int quarterTurns = 0)
        {
            if (string.IsNullOrEmpty(moduleId)) return null;

            string path = ResourcePath(moduleId, quarterTurns);
            if (!Cache.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>(path);
                Cache[path] = sprite;
            }
            return sprite;
        }

        public static bool HasAllDirections(string moduleId)
        {
            for (int quarterTurns = 0; quarterTurns < 4; quarterTurns++)
            {
                if (Load(moduleId, quarterTurns) == null) return false;
            }
            return true;
        }
    }
}
