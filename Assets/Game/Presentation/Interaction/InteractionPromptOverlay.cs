using UnityEngine;

namespace LastHope.Presentation.Interaction
{
    /// <summary>Hiện prompt + thanh tiến trình giữ phím cho interactable gần nhất.</summary>
    public class InteractionPromptOverlay : MonoBehaviour
    {
        [SerializeField] InteractionDetector detector;

        GUIStyle labelStyle;

        void OnGUI()
        {
            if (detector == null || detector.CurrentTarget == null) return;

            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
            };

            const float width = 260f;
            const float height = 50f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 90f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.Label(new Rect(x, y + 4f, width, 20f), detector.CurrentTarget.PromptText, labelStyle);

            if (detector.CurrentTarget.HoldDurationSeconds > 0f)
            {
                const float barMargin = 20f;
                var barBg = new Rect(x + barMargin, y + 26f, width - barMargin * 2f, 12f);
                GUI.Box(barBg, GUIContent.none);

                var barFill = new Rect(barBg.x, barBg.y, barBg.width * detector.HoldProgress01, barBg.height);
                GUI.Box(barFill, GUIContent.none);
            }
        }
    }
}
