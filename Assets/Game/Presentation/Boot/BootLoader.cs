using LastHope.Core.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// Nằm trong scene <c>00_Boot</c>. Load additive scene persistent rồi đặt nó làm active
    /// scene để mọi object sinh sau thuộc về nó.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        public const string PersistentSceneName = "10_GamePersistent";

        [SerializeField] string persistentScene = PersistentSceneName;
        [Tooltip("Scene gameplay load ngay sau persistent. S6 sẽ thay bằng SceneFlowController.")]
        [SerializeField] string initialGameplayScene = "90_TestSystems";

        void Start()
        {
            LoadAdditive(persistentScene, makeActive: true, onDone: () =>
            {
                if (!string.IsNullOrEmpty(initialGameplayScene))
                {
                    LoadAdditive(initialGameplayScene, makeActive: false, onDone: null);
                }
            });
        }

        void LoadAdditive(string sceneName, bool makeActive, System.Action onDone)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                GameLog.Info(LogCategory.Boot, $"{sceneName} đã load sẵn, bỏ qua.");
                onDone?.Invoke();
                return;
            }

            GameLog.Info(LogCategory.Boot, $"Load additive {sceneName}");
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            op.completed += _ =>
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (makeActive && scene.IsValid()) SceneManager.SetActiveScene(scene);
                GameLog.Info(LogCategory.Boot, $"{sceneName} sẵn sàng.");
                onDone?.Invoke();
            };
        }
    }
}
