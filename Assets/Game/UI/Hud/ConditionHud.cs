using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Systems.Registry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastHope.UI.Hud
{
    /// <summary>
    /// Always-visible player survival HUD (BL-P1 S9) — the real player-facing counterpart to
    /// DebugPanel's Condition section (F2, dev-only), which existed since S7 specifically so this
    /// could be verified before building the player-facing version. 4 stat bars + a status badge
    /// line, rebuilt on ConditionChanged. Every element explicitly positioned (no LayoutGroup) —
    /// see UiLayout.cs for why.
    /// </summary>
    public sealed class ConditionHud : MonoBehaviour
    {
        private const float RowHeight = 32f;

        private GameContext _ctx;
        private Image _healthFill;
        private Image _staminaFill;
        private Image _hungerFill;
        private Image _thirstFill;
        private TextMeshProUGUI _statusLabel;

        private void Awake() => BuildLayout();

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            if (_ctx == null) return;

            _ctx.Events.Subscribe<ConditionChanged>(_ => Rebuild());
            Rebuild();
        }

        private void Rebuild()
        {
            PlayerConditionState c = _ctx.World.Player.Condition;
            _healthFill.fillAmount = Mathf.Clamp01(c.Health / 100f);
            _staminaFill.fillAmount = Mathf.Clamp01(c.Stamina / 100f);
            _hungerFill.fillAmount = Mathf.Clamp01(c.Hunger / 100f);
            _thirstFill.fillAmount = Mathf.Clamp01(c.Thirst / 100f);

            var badges = new List<string>();
            foreach (var kvp in c.StatusEffects) badges.Add(kvp.Key);
            if (c.Incapacitation == IncapacitationState.Collapsed) badges.Add("collapsed");
            _statusLabel.text = string.Join(" | ", badges);
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            _healthFill = AddBar(root, "HP", Color.red, 0);
            _staminaFill = AddBar(root, "STA", Color.green, 1);
            _hungerFill = AddBar(root, "HUN", new Color(0.85f, 0.55f, 0.15f), 2);
            _thirstFill = AddBar(root, "THI", Color.cyan, 3);

            var statusGo = new GameObject("StatusBadges", typeof(RectTransform));
            statusGo.transform.SetParent(root, false);
            _statusLabel = statusGo.AddComponent<TextMeshProUGUI>();
            _statusLabel.fontSize = 18;
            _statusLabel.color = new Color(1f, 0.4f, 0.4f);
            UiLayout.TopLeft(statusGo.GetComponent<RectTransform>(), 0f, 4 * RowHeight, 260f, 24f);
        }

        private static Image AddBar(Transform parent, string label, Color color, int index)
        {
            var row = new GameObject(label, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(row.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 18;
            text.color = Color.white;
            UiLayout.TopLeft(textGo.GetComponent<RectTransform>(), 0f, 2f, 44f, 24f);

            var barGo = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(row.transform, false);
            var image = barGo.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.color = color;
            UiLayout.TopLeft(barGo.GetComponent<RectTransform>(), 48f, 6f, 180f, 18f);
            return image;
        }
    }
}
