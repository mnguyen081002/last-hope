using TMPro;
using UnityEngine;

namespace LastHope.UI.Hud
{
    /// <summary>
    /// Always-visible key legend, bottom-left corner (2026-07-24 playtest feedback: "I don't know
    /// how I'm supposed to interact"). Static text, no logic — just a permanent on-screen reminder
    /// of what each key does since there's no tutorial/onboarding yet.
    /// </summary>
    public sealed class ControlsLegend : MonoBehaviour
    {
        private const string LegendText =
            "WASD di chuyển   E tương tác   I/Tab túi đồ   M bản đồ   Esc đóng   F1 debug   F2 debug panel";

        private void Awake()
        {
            var bg = gameObject.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = LegendText;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-16f, 0f);
            textRect.anchoredPosition = Vector2.zero;
        }
    }
}
