using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.DebugTools.Overlay
{
    /// <summary>Overlay chẩn đoán tối thiểu: F1 bật/tắt, hiện FPS và vị trí player.</summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] bool visibleOnStart = true;
        [SerializeField] Transform trackedTransform;

        const float FpsSampleInterval = 0.25f;

        bool visible;
        float fps;
        float sampleElapsed;
        int sampleFrames;
        GUIStyle style;

        public void SetTracked(Transform target) => trackedTransform = target;

        void Awake() => visible = visibleOnStart;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame) visible = !visible;

            sampleElapsed += Time.unscaledDeltaTime;
            sampleFrames++;
            if (sampleElapsed >= FpsSampleInterval)
            {
                fps = sampleFrames / sampleElapsed;
                sampleElapsed = 0f;
                sampleFrames = 0;
            }
        }

        void OnGUI()
        {
            if (!visible) return;

            style ??= new GUIStyle(GUI.skin.label) { fontSize = 14, richText = false };

            string position = trackedTransform != null
                ? $"X {trackedTransform.position.x:F2}  Y {trackedTransform.position.y:F2}"
                : "no target";

            GUI.Box(new Rect(10f, 10f, 220f, 62f), GUIContent.none);
            GUI.Label(new Rect(18f, 16f, 210f, 20f), $"FPS {fps:F0}", style);
            GUI.Label(new Rect(18f, 34f, 210f, 20f), position, style);
            GUI.Label(new Rect(18f, 52f, 210f, 20f), "F1 = toggle", style);
        }
    }
}
