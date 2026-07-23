using TMPro;
using UnityEngine;

namespace LastHope.Presentation.Interaction
{
    /// <summary>
    /// Screen-space "E — <prompt>" HUD text. Lives in Presentation (not UI) because it needs
    /// IInteractable directly.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class InteractionPrompt : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        private InteractionDetector _detector;

        private void Awake()
        {
            _label = GetComponent<TextMeshProUGUI>();
            _label.text = string.Empty;
        }

        public void SetDetector(InteractionDetector detector)
        {
            if (_detector != null) _detector.TargetChanged -= OnTargetChanged;
            _detector = detector;
            if (_detector != null) _detector.TargetChanged += OnTargetChanged;
        }

        private void OnDestroy()
        {
            if (_detector != null) _detector.TargetChanged -= OnTargetChanged;
        }

        private void OnTargetChanged(IInteractable target)
        {
            _label.text = target != null ? $"E — {target.PromptText}" : string.Empty;
        }
    }
}
