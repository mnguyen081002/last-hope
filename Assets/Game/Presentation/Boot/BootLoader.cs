using LastHope.Core.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// Nằm trong scene <c>00_Boot</c>. Load additive scene persistent rồi đặt nó làm active
    /// scene để mọi object sinh sau thuộc về nó. Scene gameplay đầu tiên do
    /// <see cref="SceneFlowController"/> (trong persistent scene) load theo
    /// <c>LocationDefinition.SceneName</c>, không hard-code ở đây.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        public const string PersistentSceneName = "10_GamePersistent";

        [SerializeField] string persistentScene = PersistentSceneName;

        void Start()
        {
            if (SceneManager.GetSceneByName(persistentScene).isLoaded)
            {
                GameLog.Info(LogCategory.Boot, $"{persistentScene} đã load sẵn, bỏ qua.");
                return;
            }

            GameLog.Info(LogCategory.Boot, $"Load additive {persistentScene}");
            var op = SceneManager.LoadSceneAsync(persistentScene, LoadSceneMode.Additive);
            op.completed += _ =>
            {
                var scene = SceneManager.GetSceneByName(persistentScene);
                if (scene.IsValid()) SceneManager.SetActiveScene(scene);
                GameLog.Info(LogCategory.Boot, $"{persistentScene} sẵn sàng.");

                // Unload scene Boot — nếu không, BootCamera (tag MainCamera) tồn tại song song
                // với Main Camera thật trong Persistent, khiến Camera.main ở nơi khác có thể
                // trả về nhầm camera đứng yên ở gốc toạ độ.
                SceneManager.UnloadSceneAsync(gameObject.scene);
            };
        }
    }
}
