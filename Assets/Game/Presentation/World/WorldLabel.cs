using TMPro;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Floating label above an interactable — the blockout build uses undifferentiated
    /// primitives/sprites for every interactable type, so a player can't tell a search shelf
    /// from a storage crate from a travel point without this (2026-07-24 playtest feedback).
    /// 2026-07-25: camera never rotates in 2D (unlike the old fixed-but-tilted 3D isometric rig),
    /// so no facing rotation is needed at all — identity always reads correctly.
    /// </summary>
    public sealed class WorldLabel : MonoBehaviour
    {
        private TextMeshPro _text;

        public static WorldLabel Create(Transform parent, string text, float heightOffset = 1.4f)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, heightOffset, 0f);
            go.transform.rotation = Quaternion.identity;

            var label = go.AddComponent<WorldLabel>();
            label._text = go.AddComponent<TextMeshPro>();
            label._text.text = text;
            label._text.fontSize = 6f;
            label._text.alignment = TextAlignmentOptions.Center;
            label._text.color = Color.white;
            return label;
        }
    }
}
