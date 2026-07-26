using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Top-down 2D movement: Rigidbody2D kinematic, moved in FixedUpdate via MovePosition. No
    /// gravity, no slope/step, no fall-recovery — those were 3D-only concepts (a top-down 2D
    /// plane has no vertical drop to fall off). Input maps straight to world X/Y: the camera
    /// never rotates, so "screen-relative" and "world-relative" are always the same direction.
    /// SpeedModifier is set by PlayerAvatarSync from Flood/Carry Load/Condition.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private float baseMoveSpeed = 4.5f;

        private Rigidbody2D _rigidbody;
        private InputAction _moveAction;
        private Vector2 _moveInput;

        public float SpeedModifier { get; set; } = 1f;

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            var gameplayMap = inputActions.FindActionMap("Gameplay", throwIfNotFound: true);
            _moveAction = gameplayMap.FindAction("Move", throwIfNotFound: true);
        }

        private void OnEnable() => _moveAction?.Enable();
        private void OnDisable() => _moveAction?.Disable();

        private void Update()
        {
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        private void FixedUpdate()
        {
            Vector2 velocity = _moveInput * (baseMoveSpeed * SpeedModifier);
            _rigidbody.MovePosition(_rigidbody.position + velocity * Time.fixedDeltaTime);
        }
    }
}
