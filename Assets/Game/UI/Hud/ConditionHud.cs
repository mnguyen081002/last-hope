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
    /// line, rebuilt on ConditionChanged.
    /// </summary>
    public sealed class ConditionHud : MonoBehaviour
    {
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
            var column = new GameObject("Column", typeof(RectTransform), typeof(VerticalLayoutGroup));
            column.transform.SetParent(root, false);

            _healthFill = AddBar(column.transform, "HP", Color.red);
            _staminaFill = AddBar(column.transform, "STA", Color.green);
            _hungerFill = AddBar(column.transform, "HUN", new Color(0.85f, 0.55f, 0.15f));
            _thirstFill = AddBar(column.transform, "THI", Color.cyan);

            var statusGo = new GameObject("StatusBadges", typeof(RectTransform));
            statusGo.transform.SetParent(column.transform, false);
            _statusLabel = statusGo.AddComponent<TextMeshProUGUI>();
            _statusLabel.fontSize = 14;
            statusGo.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 20);
        }

        private static Image AddBar(Transform parent, string label, Color color)
        {
            var row = new GameObject(label, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<HorizontalLayoutGroup>().spacing = 6;

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(row.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 14;
            textGo.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 18);

            var barGo = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(row.transform, false);
            var image = barGo.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.color = color;
            barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 14);
            return image;
        }
    }
}
