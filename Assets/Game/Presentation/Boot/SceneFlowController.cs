using System.Collections;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Logging;
using LastHope.Presentation.World;
using LastHope.Systems.Registry;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Presentation.Boot
{
    /// <summary>
    /// Sole owner of gameplay-scene lifecycle from Sprint 6 onward (BL-P1-19/20): loads/unloads
    /// the additive scene matching Player.CurrentLocationId's LocationDefinition.SceneName, and
    /// places the player at a PlayerSpawnPoint when no saved position matches the new scene.
    /// </summary>
    public sealed class SceneFlowController : MonoBehaviour
    {
        private GameContext _ctx;
        private string _currentSceneName;

        private void Start()
        {
            if (!GameServiceRegistry.TryGet(out _ctx)) return;

            _ctx.Events.Subscribe<TravelCompleted>(OnTravelCompleted);
            _ctx.Events.Subscribe<WorldStateReloaded>(OnWorldStateReloaded);

            LoadSceneForCurrentLocation();
        }

        private void OnTravelCompleted(TravelCompleted evt) => LoadSceneForCurrentLocation();
        private void OnWorldStateReloaded(WorldStateReloaded evt) => LoadSceneForCurrentLocation();

        private void LoadSceneForCurrentLocation()
        {
            string locationId = _ctx.World.Player.CurrentLocationId;
            if (string.IsNullOrEmpty(locationId))
            {
                GameLog.Warn(LogCategory.World, "SceneFlowController: Player.CurrentLocationId is empty, nothing to load.");
                return;
            }

            if (!_ctx.Definitions.TryGetLocation(locationId, out var def) || string.IsNullOrEmpty(def.SceneName))
            {
                GameLog.Warn(LogCategory.World, $"SceneFlowController: location '{locationId}' has no SceneName, nothing to load.");
                return;
            }

            if (def.SceneName == _currentSceneName) return;

            GameLog.Info(LogCategory.World, $"SceneFlowController: switching to '{def.SceneName}' for location '{locationId}'.");
            StartCoroutine(SwitchScene(def.SceneName));
        }

        private IEnumerator SwitchScene(string sceneName)
        {
            string previous = _currentSceneName;

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (!string.IsNullOrEmpty(previous))
                yield return SceneManager.UnloadSceneAsync(previous);

            _currentSceneName = sceneName;
            PlaceAtSpawnPointIfNeeded();
            GameLog.Info(LogCategory.World, $"SceneFlowController: '{sceneName}' loaded and active.");
        }

        private void PlaceAtSpawnPointIfNeeded()
        {
            var player = _ctx.World.Player;
            if (player.PositionLocationId == player.CurrentLocationId) return; // PlayerAvatarSync already placed it

            var spawn = FindFirstObjectByType<PlayerSpawnPoint>();
            var playerGo = GameObject.FindWithTag("Player");
            if (spawn == null || playerGo == null) return;

            var controller = playerGo.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            playerGo.transform.position = spawn.transform.position;
            if (controller != null) controller.enabled = true;
        }
    }
}
