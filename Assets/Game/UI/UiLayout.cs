using UnityEngine;

namespace LastHope.UI
{
    /// <summary>
    /// Manual, explicit RectTransform positioning for every code-built panel (BL-P1). Replaces
    /// reliance on Unity LayoutGroup components: a freshly created RectTransform's default size
    /// gave every dynamically built row zero height, so nested VerticalLayoutGroups stacked them
    /// all at the same position instead of one below another — every panel's text rendered on top
    /// of itself (2026-07-24 playtest screenshot: World Map title/Close/route row all overlapping).
    /// Every element now gets an explicit position and size; nothing is left to layout-group
    /// defaults.
    /// </summary>
    public static class UiLayout
    {
        /// <summary>Anchored to the top of the parent, stretched to full width, at a fixed height
        /// and a given vertical offset from the top (0 = flush with parent's top edge).</summary>
        public static void StretchTop(RectTransform rect, float yOffsetFromTop, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, -yOffsetFromTop);
            rect.sizeDelta = new Vector2(0f, height);
        }

        /// <summary>Fixed position and size relative to the parent's top-left corner.</summary>
        public static void TopLeft(RectTransform rect, float x, float yOffsetFromTop, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -yOffsetFromTop);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
