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
        [SerializeField] PlayerFloorState playerFloorState;

        string loadedGameplayScene;

        /// <summary>RouteId của chuyến đi vừa bắt đầu — dùng để chọn đúng spawn point (gần
        /// cổng nào) khi <see cref="LocationChanged"/> bắn ngay sau đó. Rỗng ở lúc boot lần
        /// đầu (không có route nào vừa đi qua) → spawn mặc định.</summary>
        string pendingRouteId = "";

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
                GameBootstrapper.Services.Events.Unsubscribe<TravelStarted>(OnTravelStarted);
                GameBootstrapper.Services.Events.Unsubscribe<LocationChanged>(OnLocationChanged);
            }
        }

        void OnReady()
        {
            GameBootstrapper.Ready -= OnReady;
            GameBootstrapper.Services.Events.Subscribe<TravelStarted>(OnTravelStarted);
            GameBootstrapper.Services.Events.Subscribe<LocationChanged>(OnLocationChanged);

            string locationId = GameBootstrapper.Services.World.Player.CurrentLocationId;
            LoadScene(SceneNameFor(locationId));
        }

        void OnTravelStarted(TravelStarted e) => pendingRouteId = e.RouteId;

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
                RepositionPlayer(sceneName);
                return;
            }

            string previous = loadedGameplayScene;
            loadedGameplayScene = sceneName;

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadOp.completed += _ =>
            {
                RepositionPlayer(sceneName);
                if (!string.IsNullOrEmpty(previous)) SceneManager.UnloadSceneAsync(previous);
            };
        }

        // Scene cũ chưa kịp unload (bất đồng bộ) khi callback này chạy — cả 2 scene có thể
        // cùng tồn tại PlayerSpawnPoint. Phải tìm đúng trong scene vừa load bằng tên, không
        // dùng FindFirstObjectByType toàn cục (thứ tự không đảm bảo, hay trúng scene cũ).
        void RepositionPlayer(string sceneName)
        {
            if (playerAvatar == null) return;

            string routeId = pendingRouteId;
            pendingRouteId = ""; // dùng một lần — LocationChanged kế tiếp không phải do Travel này nữa

            var spawn = FindSpawnPointInScene(SceneManager.GetSceneByName(sceneName), routeId);
            if (spawn == null)
            {
                GameLog.Warn(LogCategory.Boot, $"Không thấy PlayerSpawnPoint trong '{sceneName}'.");
                return;
            }

            playerAvatar.TeleportTo(spawn.transform.position);
            if (cameraRig != null) cameraRig.SetTarget(playerAvatar.transform);
            // Tầng (Z-level) là state cục bộ theo scene — sang scene mới luôn về tầng 0, tránh
            // mang nhầm tầng cũ (vd đang đứng Upper Floor lúc Travel rời Shelter).
            playerFloorState?.ResetFloor();
        }

        /// <summary>Scene nhiều cổng ra vào có nhiều spawn point — ưu tiên cái khớp
        /// <paramref name="routeId"/> vừa đi qua, không khớp (hoặc rỗng, vd. boot lần đầu)
        /// thì lấy cái đầu tiên tìm thấy.</summary>
        static PlayerSpawnPoint FindSpawnPointInScene(Scene scene, string routeId)
        {
            PlayerSpawnPoint fallback = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var spawn in root.GetComponentsInChildren<PlayerSpawnPoint>(true))
                {
                    if (!string.IsNullOrEmpty(routeId) && spawn.RouteId == routeId) return spawn;
                    fallback ??= spawn;
                }
            }
            return fallback;
        }
    }
}
