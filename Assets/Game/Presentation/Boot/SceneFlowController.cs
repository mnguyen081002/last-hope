using LastHope.Core.Diagnostics;
using LastHope.Core.Events;
using LastHope.Presentation.CameraControl;
using LastHope.Presentation.Player;
using LastHope.Presentation.World;
using LastHope.Systems.Boot;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// Chủ sở hữu duy nhất vòng đời scene gameplay: load scene theo
    /// <c>LocationDefinition.SceneName</c> lúc boot và mỗi khi Travel đổi location, đặt player
    /// tại <see cref="PlayerSpawnPoint"/> của scene mới.
    ///
    /// Không xử lý load save đang đứng ở location khác scene hiện tại (scope cut P1 — xem
    /// `docs/plans/2026-07-27-p1c-exploration-gameplay.md`); <see cref="WorldStateReloaded"/>
    /// chỉ áp lại toạ độ, không đổi scene.
    /// </summary>
    public class SceneFlowController : MonoBehaviour
    {
        [SerializeField] PlayerAvatarSync playerAvatar;
        [SerializeField] CameraRig cameraRig;

        string loadedGameplayScene;

        void OnEnable()
        {
            if (GameBootstrapper.IsReady) OnReady();
            else GameBootstrapper.Ready += OnReady;
        }

        void OnDisable()
        {
            GameBootstrapper.Ready -= OnReady;
            if (GameBootstrapper.IsReady)
            {
                GameBootstrapper.Services.Events.Unsubscribe<LocationChanged>(OnLocationChanged);
            }
        }

        void OnReady()
        {
            GameBootstrapper.Ready -= OnReady;
            GameBootstrapper.Services.Events.Subscribe<LocationChanged>(OnLocationChanged);

            string locationId = GameBootstrapper.Services.World.Player.CurrentLocationId;
            LoadScene(SceneNameFor(locationId));
        }

        void OnLocationChanged(LocationChanged e) => LoadScene(SceneNameFor(e.ToLocationId));

        string SceneNameFor(string locationId)
        {
            var definitions = GameBootstrapper.Services.Definitions;
            return definitions.TryGetLocation(locationId, out var location) ? location.SceneName : null;
        }

        void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                GameLog.Error(LogCategory.Boot, "SceneFlowController: location không có SceneName.");
                return;
            }

            if (sceneName == loadedGameplayScene)
            {
                RepositionPlayer();
                return;
            }

            string previous = loadedGameplayScene;
            loadedGameplayScene = sceneName;

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadOp.completed += _ =>
            {
                RepositionPlayer();
                if (!string.IsNullOrEmpty(previous)) SceneManager.UnloadSceneAsync(previous);
            };
        }

        void RepositionPlayer()
        {
            if (playerAvatar == null) return;

            var spawn = FindFirstObjectByType<PlayerSpawnPoint>();
            if (spawn == null)
            {
                GameLog.Warn(LogCategory.Boot, $"Không thấy PlayerSpawnPoint trong '{loadedGameplayScene}'.");
                return;
            }

            playerAvatar.TeleportTo(spawn.transform.position);
            if (cameraRig != null) cameraRig.SetTarget(playerAvatar.transform);
        }
    }
}
