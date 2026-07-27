using LastHope.Core.Events;
using LastHope.Systems.Boot;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Cầu nối transform ↔ <c>PlayerState</c>. Ghi vị trí mỗi frame để autosave luôn đúng;
    /// áp lại từ state khi load save (<see cref="WorldStateReloaded"/>) — không tự đổi
    /// <c>CurrentLocationId</c>, đó là việc của Travel/SceneFlowController.
    /// </summary>
    public class PlayerAvatarSync : MonoBehaviour
    {
        void OnEnable()
        {
            if (GameBootstrapper.IsReady)
            {
                GameBootstrapper.Services.Events.Subscribe<WorldStateReloaded>(OnWorldStateReloaded);
            }
            else
            {
                GameBootstrapper.Ready += SubscribeOnceReady;
            }
        }

        void OnDisable()
        {
            GameBootstrapper.Ready -= SubscribeOnceReady;
            if (GameBootstrapper.IsReady)
            {
                GameBootstrapper.Services.Events.Unsubscribe<WorldStateReloaded>(OnWorldStateReloaded);
            }
        }

        void SubscribeOnceReady()
        {
            GameBootstrapper.Ready -= SubscribeOnceReady;
            GameBootstrapper.Services.Events.Subscribe<WorldStateReloaded>(OnWorldStateReloaded);
        }

        void Update()
        {
            if (!GameBootstrapper.IsReady) return;

            var playerState = GameBootstrapper.Services.World.Player;
            playerState.PositionX = transform.position.x;
            playerState.PositionY = transform.position.y;
        }

        void OnWorldStateReloaded(WorldStateReloaded _)
        {
            var playerState = GameBootstrapper.Services.World.Player;
            transform.position = new Vector3(playerState.PositionX, playerState.PositionY, transform.position.z);
        }

        /// <summary>Đặt vị trí tại spawn point khi vào scene mới (travel) — không đi qua state.</summary>
        public void TeleportTo(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}
