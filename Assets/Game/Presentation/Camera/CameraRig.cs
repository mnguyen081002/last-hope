using LastHope.Core.Diagnostics;
using LastHope.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.CameraControl
{
    /// <summary>
    /// Camera orthographic 2D cố định: không xoay, bám target theo X/Y, zoom qua
    /// orthographicSize. Góc isometric nằm ở art, không ở camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] InputActionAsset controls;

        [Header("Follow")]
        [SerializeField] Vector2 followOffset = Vector2.zero;
        [Tooltip("Thời gian smooth damp khi bám target. 0 = bám cứng.")]
        [SerializeField] float followSmoothTime = 0.12f;

        [Header("Zoom")]
        [SerializeField] float defaultOrthographicSize = 6f;
        [SerializeField] float minOrthographicSize = 3f;
        [SerializeField] float maxOrthographicSize = 12f;
        [Tooltip("Số unit orthographicSize thay đổi mỗi nấc cuộn chuột.")]
        [SerializeField] float zoomStep = 0.75f;
        [SerializeField] float zoomSmoothTime = 0.1f;

        Camera cam;
        InputAction zoomAction;
        Vector2 followVelocity;
        float targetSize;
        float sizeVelocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = new Vector3(0f, 1f, 0f);

            targetSize = Mathf.Clamp(defaultOrthographicSize, minOrthographicSize, maxOrthographicSize);
            cam.orthographicSize = targetSize;

            if (controls != null)
            {
                zoomAction = controls.FindActionMap("Gameplay", true).FindAction("Zoom", true);
            }
        }

        void OnEnable() => zoomAction?.Enable();

        void OnDisable() => zoomAction?.Disable();

        void LateUpdate()
        {
            ApplyZoom();
            FollowTarget();
        }

        void ApplyZoom()
        {
            // Cuộn chuột trên panel OnGUI (Debug/Inventory/Search/Storage) không được zoom
            // camera cùng lúc — IMGUI và Input System đọc scroll độc lập nhau.
            if (zoomAction != null && !PointerOverUI.ConsumeIsHovering())
            {
                float scroll = zoomAction.ReadValue<float>();
                if (!Mathf.Approximately(scroll, 0f))
                {
                    // Scroll lên (dương) = zoom vào = orthographicSize nhỏ đi.
                    targetSize = Mathf.Clamp(
                        targetSize - Mathf.Sign(scroll) * zoomStep,
                        minOrthographicSize,
                        maxOrthographicSize);
                }
            }

            cam.orthographicSize = Mathf.SmoothDamp(
                cam.orthographicSize, targetSize, ref sizeVelocity, zoomSmoothTime);
        }

        void FollowTarget()
        {
            if (target == null) return;

            Vector2 desired = (Vector2)target.position + followOffset;
            Vector2 current = transform.position;

            Vector2 next = followSmoothTime > 0f
                ? Vector2.SmoothDamp(current, desired, ref followVelocity, followSmoothTime)
                : desired;

            // Giữ nguyên Z: camera 2D phải đứng trước mặt phẳng game.
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }

        /// <summary>Gán target lúc runtime (Player spawn sau camera).</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (newTarget == null) return;

            Vector2 snap = (Vector2)newTarget.position + followOffset;
            transform.position = new Vector3(snap.x, snap.y, transform.position.z);
            followVelocity = Vector2.zero;
            GameLog.Info(LogCategory.Camera, $"Target = {newTarget.name}");
        }
    }
}
