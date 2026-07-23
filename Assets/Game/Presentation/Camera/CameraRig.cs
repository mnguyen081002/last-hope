using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.CameraRig
{
    /// <summary>
    /// Fixed isometric orthographic camera per technical-specification.md mục 2:
    /// pitch 35.264°, yaw 45°, no rotation, zoom clamped via orthographicSize.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -12f);
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 12f;
        [SerializeField] private float zoomSpeed = 1.5f;
        [SerializeField] private float followSmoothing = 12f;

        private Camera _camera;
        private InputAction _zoomAction;

        public void SetTarget(Transform newTarget) => target = newTarget;
        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);

            if (inputActions != null)
            {
                var gameplayMap = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _zoomAction = gameplayMap?.FindAction("Zoom", throwIfNotFound: false);
            }
        }

        private void OnEnable() => _zoomAction?.Enable();
        private void OnDisable() => _zoomAction?.Disable();

        private void LateUpdate()
        {
            float zoomInput = _zoomAction != null ? _zoomAction.ReadValue<float>() : 0f;
            if (Mathf.Abs(zoomInput) > 0.0001f)
            {
                _camera.orthographicSize = Mathf.Clamp(
                    _camera.orthographicSize - zoomInput * zoomSpeed * Time.deltaTime,
                    minOrthoSize, maxOrthoSize);
            }

            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothing * Time.deltaTime));
        }
    }
}
