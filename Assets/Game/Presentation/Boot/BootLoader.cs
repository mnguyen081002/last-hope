using LastHope.Core.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// 00_Boot per technical-specification.md mục 7: loads the persistent scene, then the
    /// active gameplay scene. Definition Data / World State loading arrives in Sprint 2 (BL-P1-06/07);
    /// for Sprint 1 this only wires scene loading + logging.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string persistentSceneName = "10_GamePersistent";
        [SerializeField] private string firstSceneName = "90_TestSystems";

        private void Start()
        {
            GameLog.Info(LogCategory.Boot, "Boot started.");
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(persistentSceneName, LoadSceneMode.Additive);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != persistentSceneName) return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameLog.Info(LogCategory.Boot, $"Persistent scene loaded, loading '{firstSceneName}'.");
            SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);
        }
    }
}
