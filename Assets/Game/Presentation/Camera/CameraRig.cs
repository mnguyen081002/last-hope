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
        [SerializeField] private float followDistance = 16.97f;
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 12f;
        [SerializeField] private float zoomSpeed = 0.02f;
        [SerializeField] private float followSmoothing = 12f;

        private Camera _camera;
        private InputAction _zoomAction;
        private Vector3 _offset;

        public void SetTarget(Transform newTarget) => target = newTarget;
        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);

            // Must be derived from the fixed rotation, not hand-picked (the old (0,12,-12) offset
            // didn't actually point back along this rotation's forward axis, so the player rendered
            // off-center — 2026-07-24 playtest). transform.forward for this exact pitch/yaw is
            // (~0.577, -0.577, 0.577); the camera must sit at target - forward*distance to look
            // straight at it.
            _offset = transform.rotation * new Vector3(0f, 0f, -followDistance);

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
            // Mouse scroll is a per-frame impulse (a whole notch arrives in one frame), not a held
            // analog axis — scaling it by Time.deltaTime (as before) shrank it to near-zero, which
            // is why zoom felt almost unresponsive (2026-07-24 playtest).
            float zoomInput = _zoomAction != null ? _zoomAction.ReadValue<float>() : 0f;
            if (Mathf.Abs(zoomInput) > 0.0001f)
            {
                _camera.orthographicSize = Mathf.Clamp(
                    _camera.orthographicSize - zoomInput * zoomSpeed,
                    minOrthoSize, maxOrthoSize);
            }

            if (target == null) return;

            Vector3 desiredPosition = target.position + _offset;
            transform.position = Vector3.Lerp(
                transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothing * Time.deltaTime));
        }
    }
}
