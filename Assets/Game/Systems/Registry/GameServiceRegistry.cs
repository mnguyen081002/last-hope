using System;
using System.Collections.Generic;

namespace LastHope.Systems.Registry
{
    /// <summary>
    /// Limited service locator for the handful of Core services Unity-side code needs
    /// (technical-specification.md mục 9/§14: Bootstrap Composition Root, no DI framework).
    /// Only GameBootstrapper registers into this; everything else only reads.
    /// </summary>
    public static class GameServiceRegistry
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service) => Services[typeof(T)] = service;

        public static T Get<T>()
        {
            if (Services.TryGetValue(typeof(T), out var service)) return (T)service;
            throw new InvalidOperationException($"Service '{typeof(T).Name}' is not registered.");
        }

        public static bool TryGet<T>(out T service)
        {
            if (Services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }

        public static void Clear() => Services.Clear();
    }
}
