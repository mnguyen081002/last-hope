using TMPro;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Floating world-space billboard text above an interactable — the blockout build uses
    /// undifferentiated primitives (cube/cylinder) for every interactable type, so a player can't
    /// tell a search shelf from a storage crate from a travel point without this (2026-07-24
    /// playtest feedback). Faces the camera every frame.
    /// </summary>
    public sealed class WorldLabel : MonoBehaviour
    {
        private TextMeshPro _text;

        public static WorldLabel Create(Transform parent, string text, float heightOffset = 1.4f)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, heightOffset, 0f);

            var label = go.AddComponent<WorldLabel>();
            label._text = go.AddComponent<TextMeshPro>();
            label._text.text = text;
            // 4 was nearly invisible, 14 was oversized AND the outline below caused visible mesh
            // corruption on the default (non-outline-capable) material — reverted, plain color only.
            label._text.fontSize = 6f;
            label._text.alignment = TextAlignmentOptions.Center;
            label._text.color = Color.white;
            return label;
        }

        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
