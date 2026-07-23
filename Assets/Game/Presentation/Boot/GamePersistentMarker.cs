using UnityEngine;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// Marks 10_GamePersistent's root as surviving scene loads. Hosts World Clock,
    /// Definition Registry, Command/Event Bus, Save Service and Debug Service from
    /// Sprint 2 onward (technical-specification.md mục 7); Sprint 1 only sets up persistence.
    /// </summary>
    public class GamePersistentMarker : MonoBehaviour
    {
        private static bool _instanceExists;

        private void Awake()
        {
            if (_instanceExists)
            {
                Destroy(gameObject);
                return;
            }

            _instanceExists = true;
            DontDestroyOnLoad(gameObject);
        }
    }
}
