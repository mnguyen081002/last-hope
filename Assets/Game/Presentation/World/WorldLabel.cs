using TMPro;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Floating world-space billboard text above an interactable — the blockout build uses
    /// undifferentiated primitives (cube/cylinder) for every interactable type, so a player can't
    /// tell a search shelf from a storage crate from a travel point without this (2026-07-24
    /// playtest feedback).
    /// </summary>
    public sealed class WorldLabel : MonoBehaviour
    {
        // CameraRig's isometric view never rotates (fixed pitch 35.264°/yaw 45°), so every label
        // needs exactly one "face the camera" orientation, set once — not a per-frame Camera.main
        // lookup + LookRotation. The old per-frame version computed `label.position - camera.position`
        // (label's forward = same direction the camera itself looks, i.e. Vector3.forward under the
        // camera's rotation) — using Vector3.back here instead pointed the readable face away from
        // the camera and rendered every label mirror-flipped (2026-07-24 screenshot report). Must be
        // Vector3.forward, matching the original's sign, not Vector3.back.
        private static readonly Quaternion FacingRotation =
            Quaternion.LookRotation(Quaternion.Euler(35.264f, 45f, 0f) * Vector3.forward);

        private TextMeshPro _text;

        public static WorldLabel Create(Transform parent, string text, float heightOffset = 1.4f)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, heightOffset, 0f);
            go.transform.rotation = FacingRotation; // world-space — cancels out a tilted parent (e.g. the ramp) too

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
    }
}
