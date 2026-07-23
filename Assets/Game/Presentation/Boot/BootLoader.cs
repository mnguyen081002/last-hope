using LastHope.Core.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// 00_Boot per technical-specification.md mục 7: loads the persistent scene. From Sprint 6,
    /// SceneFlowController (living in 10_GamePersistent) takes over loading the first gameplay
    /// scene — it knows the player's starting LocationDefinition.SceneName; this class no longer
    /// hard-codes one.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string persistentSceneName = "10_GamePersistent";

        private void Start()
        {
            GameLog.Info(LogCategory.Boot, "Boot started.");
            SceneManager.LoadScene(persistentSceneName, LoadSceneMode.Additive);
        }
    }
}
