using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Systems.Registry;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Binds simulation state to the player's Transform. This is the presentation-write
    /// exemption: continuous position data is not a gameplay rule, so it bypasses the Command
    /// Layer by design — unlike discrete mutations, which always still go through commands.
    /// 2D world position is X/Y only.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerAvatarSync : MonoBehaviour
    {
        private GameContext _ctx;
        private Rigidbody2D _rigidbody;
        private PlayerController _playerController;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (!GameServiceRegistry.TryGet(out _ctx)) return;

            _ctx.Events.Subscribe<WorldStateReloaded>(OnWorldStateReloaded);
            _ctx.Events.Subscribe<OverloadStateChanged>(OnOverloadStateChanged);

            ApplyFromState();
        }

        private void OnDestroy()
        {
            if (_ctx == null) return;
            _ctx.Events.Unsubscribe<WorldStateReloaded>(OnWorldStateReloaded);
            _ctx.Events.Unsubscribe<OverloadStateChanged>(OnOverloadStateChanged);
        }

        private void LateUpdate()
        {
            if (_ctx == null) return;

            // Only the raw coordinates are continuous data. PositionLocationId is NOT touched
            // here — it must only change when something has actually confirmed the position is
            // valid for CurrentLocationId (SceneFlowController's spawn placement, or a loaded
            // save whose location already matches). Stamping it unconditionally every frame
            // races SceneFlowController's placement check and leaves new-game players stranded
            // at their pre-scene-load coordinates (BL-P1-19 bug — do not reintroduce).
            var player = _ctx.World.Player;
            Vector2 pos = transform.position;
            player.PositionX = pos.x;
            player.PositionY = pos.y;
        }

        private void OnWorldStateReloaded(WorldStateReloaded evt) => ApplyFromState();

        private void ApplyFromState()
        {
            var player = _ctx.World.Player;

            // No matching saved position for the scene we're in — SceneFlowController /
            // PlayerSpawnPoint handles cross-scene placement; nothing to do here yet.
            if (player.PositionLocationId != player.CurrentLocationId) return;

            var target = new Vector2(player.PositionX, player.PositionY);
            _rigidbody.position = target;
            transform.position = target;
        }

        private void OnOverloadStateChanged(OverloadStateChanged evt)
        {
            if (evt.OwnerId != _ctx.World.Player.ActorId || _playerController == null) return;
            _playerController.SpeedModifier = InventoryRules.SpeedModifierFor(evt.Overload, _ctx.Definitions.Balance);
        }
    }
}
