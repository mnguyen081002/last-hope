using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.CameraRig
{
    /// <summary>
    /// Fixed 2D isometric orthographic camera: no rotation — the camera looks straight down
    /// -Z, the isometric look comes entirely from the sprite art and from CustomAxis
    /// transparency sorting, not from a tilted camera. Follows target in X/Y, zoom clamped
    /// via orthographicSize.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform target;
        [SerializeField] private float followDistance = 10f;
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 12f;
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float followSmoothing = 12f;

        // Matches the diamond-tile projection ratio (2:1 width:height) typical of 2D isometric
        // art — tune once real tile art exists. Drives sprite draw order: two sprites at the
        // same screen position but different world Y must still sort "further back" vs
        // "closer to camera" correctly.
        [SerializeField] private Vector3 transparencySortAxis = new Vector3(0f, 1f, 0.26f);

        private Camera _camera;
        private InputAction _zoomAction;
        private Vector3 _offset;

        public void SetTarget(Transform newTarget) => target = newTarget;
        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.transparencySortMode = TransparencySortMode.CustomAxis;
            _camera.transparencySortAxis = transparencySortAxis;

            _offset = new Vector3(0f, 0f, -followDistance);

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
            // analog axis — scaling it by Time.deltaTime shrinks it to near-zero.
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
