using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Placeholder movement per technical-specification.md mục 6:
    /// CharacterController, framerate-independent, direction relative to fixed screen/camera.
    /// SpeedModifier is the hook for future Flood/Carry Load/Condition modifiers.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float baseMoveSpeed = 4.5f;
        [SerializeField] private float gravity = -20f;

        private CharacterController _controller;
        private InputAction _moveAction;
        private float _verticalVelocity;

        public float SpeedModifier { get; set; } = 1f;

        public void SetCameraTransform(Transform cam) => cameraTransform = cam;
        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            var gameplayMap = inputActions.FindActionMap("Gameplay", throwIfNotFound: true);
            _moveAction = gameplayMap.FindAction("Move", throwIfNotFound: true);
        }

        private void OnEnable() => _moveAction?.Enable();
        private void OnDisable() => _moveAction?.Disable();

        private void Update()
        {
            Vector2 input = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

            Vector3 screenForward = cameraTransform != null ? Flatten(cameraTransform.forward) : Vector3.forward;
            Vector3 screenRight = cameraTransform != null ? Flatten(cameraTransform.right) : Vector3.right;

            Vector3 moveDirection = screenForward * input.y + screenRight * input.x;
            if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

            Vector3 horizontal = moveDirection * (baseMoveSpeed * SpeedModifier);

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }
    }
}
