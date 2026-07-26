using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.DebugTools.Overlay
{
    /// <summary>
    /// Minimal always-available debug overlay per mvp-implementation-plan.md mục 6 (M0 deliverable).
    /// Extended with World Clock / State panels in Sprint 4 (Debug Panel v1, BL-P1-12).
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private Transform trackedTarget;

        private bool _visible = true;
        private float _smoothedDeltaTime;

        public void SetTrackedTarget(Transform t) => trackedTarget = t;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                _visible = !_visible;

            if (trackedTarget == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) trackedTarget = player.transform;
            }

            _smoothedDeltaTime += (Time.unscaledDeltaTime - _smoothedDeltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            float fps = 1f / Mathf.Max(_smoothedDeltaTime, 0.0001f);
            string posText = trackedTarget != null
                ? $"Pos: ({trackedTarget.position.x:F1}, {trackedTarget.position.y:F1})"
                : "Pos: n/a";

            string text = $"Last Hope — Debug Overlay (F1)\nFPS: {fps:F0}\n{posText}\nBuild: {Application.version}";
            GUI.Box(new Rect(10, 10, 260, 90), text);
        }
    }
}
