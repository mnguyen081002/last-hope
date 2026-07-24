using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
using LastHope.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Inventory
{
    /// <summary>
    /// Minimal P1 inventory screen: flat item list + weight/volume bars. No grid — per
    /// mvp-product-backlog.md P1 exit criteria, decisions must not require one. Builds its own
    /// child UI hierarchy in code (no prefabs), consistent with the rest of the project.
    /// </summary>
    public sealed class InventoryPanel : MonoBehaviour
    {
        private const float HeaderHeight = 130f;
        private const float RowHeight = 34f;

        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _toggleAction;
        private CanvasGroup _canvasGroup;
        private bool _visible;

        private RectTransform _rowContainer;
        private Image _weightFill;
        private Image _volumeFill;
        private TextMeshProUGUI _weightLabel;
        private TextMeshProUGUI _volumeLabel;

        private readonly List<GameObject> _rows = new List<GameObject>();

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            BuildLayout();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Resolved here, not Start(): OnEnable() runs before Start() in Unity's lifecycle, so
            // resolving _toggleAction in Start() meant OnEnable()'s _toggleAction?.Enable() call
            // always hit a still-null reference and never actually enabled the action.
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _toggleAction = map?.FindAction("ToggleInventory", throwIfNotFound: false);
            }

            SetVisible(false);
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            if (_ctx != null)
            {
                _ctx.Events.Subscribe<InventoryChanged>(OnInventoryChanged);
                _ctx.Events.Subscribe<OverloadStateChanged>(OnOverloadChanged);
                Rebuild();
            }
        }

        // Enable only, never Disable(): see ContainerPanel's OnEnable comment — "ToggleInventory"
        // is a shared InputAction instance, and Disable() on it would affect every other consumer.
        private void OnEnable() => _toggleAction?.Enable();

        private void Update()
        {
            if (_toggleAction != null && _toggleAction.WasPressedThisFrame())
                SetVisible(!_visible);
        }

        // CanvasGroup, not gameObject.SetActive(false): deactivating this GameObject would stop
        // its own Update() from running, so the toggle key could never be polled again to show it
        // back — the panel would hide once and never reopen.
        private void SetVisible(bool visible)
        {
            _visible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void OnInventoryChanged(InventoryChanged evt)
        {
            if (_ctx != null && evt.OwnerId == _ctx.World.Player.ActorId) Rebuild();
        }

        private void OnOverloadChanged(OverloadStateChanged evt)
        {
            if (_ctx != null && evt.OwnerId == _ctx.World.Player.ActorId) Rebuild();
        }

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            InventoryState inv = _ctx.World.Player.Inventory;
            int index = 0;
            foreach (ItemInstanceState item in inv.Items.Values)
            {
                _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                _rows.Add(BuildRow(item, def, index));
                index++;
            }

            InventoryBalance cap = _ctx.Definitions.Balance.Inventory;
            float weightRatio = cap.BackpackCapacityKg > 0 ? inv.CurrentWeightKg / cap.BackpackCapacityKg : 0f;
            float volumeRatio = cap.BackpackCapacityLiters > 0 ? inv.CurrentVolumeLiters / cap.BackpackCapacityLiters : 0f;

            Color barColor = inv.Overload == OverloadState.Heavy ? Color.red
                : inv.Overload == OverloadState.Light ? new Color(1f, 0.6f, 0f)
                : Color.green;

            _weightFill.fillAmount = Mathf.Clamp01(weightRatio);
            _weightFill.color = barColor;
            _weightLabel.text = $"Weight {inv.CurrentWeightKg:0.0} / {cap.BackpackCapacityKg:0} kg";

            _volumeFill.fillAmount = Mathf.Clamp01(volumeRatio);
            _volumeFill.color = barColor;
            _volumeLabel.text = $"Volume {inv.CurrentVolumeLiters:0.0} / {cap.BackpackCapacityLiters:0} L";
        }

        private GameObject BuildRow(ItemInstanceState item, ItemDefinition def, int index)
        {
            var row = new GameObject(item.InstanceId, typeof(RectTransform));
            row.transform.SetParent(_rowContainer, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);

            string label = def?.DisplayNameKey ?? item.ItemId;
            AddLabel(row.transform, $"{label} x{item.Quantity}", 12f, 4f, 280f, 26f);

            AddButton(row.transform, "Use", () =>
                _processor.Submit(new UseItemCommand(_ctx.World.Player.ActorId, item.InstanceId, 1)), 300f, 2f);

            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            UiLayout.StretchTop(header.GetComponent<RectTransform>(), 0f, HeaderHeight);

            _weightLabel = AddLabel(header.transform, "Weight", 12f, 10f, 380f, 26f);
            _weightFill = AddBar(header.transform, 12f, 40f, 380f, 20f);
            _volumeLabel = AddLabel(header.transform, "Volume", 12f, 68f, 380f, 26f);
            _volumeFill = AddBar(header.transform, 12f, 98f, 380f, 20f);

            var rowsGo = new GameObject("Rows", typeof(RectTransform));
            rowsGo.transform.SetParent(root, false);
            UiLayout.StretchTop(rowsGo.GetComponent<RectTransform>(), HeaderHeight, 0f);
            _rowContainer = rowsGo.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI AddLabel(Transform parent, string text, float x, float y, float width, float height)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 22;
            label.color = Color.white;
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, width, height);
            return label;
        }

        private static Image AddBar(Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.color = Color.green;
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, width, height);
            return image;
        }

        private static void AddButton(Transform parent, string text, System.Action onClick, float x, float y)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, 90f, 30f);
            go.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
        }
    }
}
