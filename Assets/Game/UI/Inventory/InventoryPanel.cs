using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
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
        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _toggleAction;

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
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _toggleAction = map?.FindAction("ToggleInventory", throwIfNotFound: false);
            }

            if (_ctx != null)
            {
                _ctx.Events.Subscribe<InventoryChanged>(OnInventoryChanged);
                _ctx.Events.Subscribe<OverloadStateChanged>(OnOverloadChanged);
                Rebuild();
            }

            gameObject.SetActive(false);
        }

        private void OnEnable() => _toggleAction?.Enable();
        private void OnDisable() => _toggleAction?.Disable();

        private void Update()
        {
            if (_toggleAction != null && _toggleAction.WasPressedThisFrame())
                gameObject.SetActive(!gameObject.activeSelf);
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
            foreach (ItemInstanceState item in inv.Items.Values)
            {
                _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                _rows.Add(BuildRow(item, def));
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

        private GameObject BuildRow(ItemInstanceState item, ItemDefinition def)
        {
            var row = new GameObject(item.InstanceId, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_rowContainer, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = false;
            layout.spacing = 8;

            string label = def?.DisplayNameKey ?? item.ItemId;
            AddLabel(row.transform, $"{label} x{item.Quantity}", 260);

            AddButton(row.transform, "Use", () =>
                _processor.Submit(new UseItemCommand(_ctx.World.Player.ActorId, item.InstanceId, 1)));

            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform), typeof(VerticalLayoutGroup));
            header.transform.SetParent(root, false);
            _weightLabel = AddLabel(header.transform, "Weight", 300);
            _weightFill = AddBar(header.transform);
            _volumeLabel = AddLabel(header.transform, "Volume", 300);
            _volumeFill = AddBar(header.transform);

            var rowsGo = new GameObject("Rows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rowsGo.transform.SetParent(root, false);
            _rowContainer = rowsGo.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI AddLabel(Transform parent, string text, float width)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 24);
            return label;
        }

        private static Image AddBar(Transform parent)
        {
            var go = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.color = Color.green;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 16);
            return image;
        }

        private static void AddButton(Transform parent, string text, System.Action onClick)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 24);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
            AddLabel(go.transform, text, 60);
        }
    }
}
