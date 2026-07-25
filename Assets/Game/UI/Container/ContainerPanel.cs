using System.Collections.Generic;
using System.Linq;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Text;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
using LastHope.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Container
{
    /// <summary>
    /// One panel for both search-point and shelter-storage containers (BL-P1-17/18). Reads
    /// WorldState directly for display (InventoryOwnerResolver, read-only use here); every
    /// mutation goes through TransferItemCommand. Shelter storage additionally shows a "Store"
    /// section for moving items from the player into it — search points are take-only.
    /// </summary>
    public sealed class ContainerPanel : MonoBehaviour
    {
        private const string PanelName = "Container";
        private const string ShelterStoragePrefix = "shelter_storage:";

        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _closeAction;

        private const float HeaderHeight = 48f;
        private const float RowHeight = 36f;
        private const float SectionGap = 16f;
        private const float SectionHeaderHeight = 32f;

        private string _ownerId;
        private TextMeshProUGUI _titleLabel;
        private RectTransform _containerRows;
        private RectTransform _playerRows;
        private GameObject _playerSection;
        private RectTransform _playerSectionRect;

        private readonly List<GameObject> _containerRowObjects = new List<GameObject>();
        private readonly List<GameObject> _playerRowObjects = new List<GameObject>();

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            BuildLayout();

            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _closeAction = map?.FindAction("Close", throwIfNotFound: false);
            }
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            if (_ctx != null)
            {
                _ctx.Events.Subscribe<ContainerViewRequested>(OnContainerViewRequested);
                _ctx.Events.Subscribe<InventoryChanged>(OnInventoryChanged);
                _ctx.Events.Subscribe<ExclusivePanelOpened>(OnExclusivePanelOpened);
            }

            // Deactivating here (not in Awake) — SetActive(false) called from an object's own
            // Awake() stops Unity from ever calling that object's Start(), which would have
            // silently broken the ContainerViewRequested subscription above forever.
            gameObject.SetActive(false);
        }

        // Enable only, never Disable(): FindAction("Close") returns the SAME shared InputAction
        // instance that WorldMapPanel also reads. This panel toggling itself via SetActive cycles
        // OnEnable/OnDisable far more often than WorldMapPanel (which stays permanently active) —
        // an OnDisable() calling Disable() here would turn the Close action off globally the next
        // time this panel closes, silently breaking Esc for WorldMapPanel too (bugfix 2026-07-24).
        private void OnEnable() => _closeAction?.Enable();

        private void Update()
        {
            if (gameObject.activeSelf && _closeAction != null && _closeAction.WasPressedThisFrame())
                Close();
        }

        private void OnContainerViewRequested(ContainerViewRequested evt)
        {
            _ownerId = evt.OwnerId;
            _titleLabel.text = evt.TitleKey;
            _playerSection.SetActive(_ownerId.StartsWith(ShelterStoragePrefix));
            gameObject.SetActive(true);
            Rebuild();
            _ctx.Events.Publish(new ExclusivePanelOpened(PanelName));
        }

        private void OnInventoryChanged(InventoryChanged evt)
        {
            if (!gameObject.activeSelf || _ctx == null) return;
            if (evt.OwnerId == _ownerId || evt.OwnerId == _ctx.World.Player.ActorId) Rebuild();
        }

        // Another focused panel (e.g. WorldMapPanel) just opened — close so the two don't render
        // on top of each other unreadably (bugfix 2026-07-24).
        private void OnExclusivePanelOpened(ExclusivePanelOpened evt)
        {
            if (evt.PanelName != PanelName && gameObject.activeSelf) Close();
        }

        public void Close() => gameObject.SetActive(false);

        private void Rebuild()
        {
            foreach (var row in _containerRowObjects) Destroy(row);
            _containerRowObjects.Clear();
            foreach (var row in _playerRowObjects) Destroy(row);
            _playerRowObjects.Clear();

            if (_ctx == null || _ownerId == null) return;

            int containerCount = 0;
            if (InventoryOwnerResolver.TryResolve(_ctx, _ownerId, out var containerInv))
            {
                foreach (var item in containerInv.Items.Values.ToList())
                {
                    _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                    _containerRowObjects.Add(BuildRow(_containerRows, item, def, containerCount, "Take",
                        () => _processor.Submit(new TransferItemCommand(_ownerId, item.InstanceId, _ctx.World.Player.ActorId, item.Quantity))));
                    containerCount++;
                }
            }

            // PlayerSection sits below however many container rows there turned out to be —
            // repositioned every rebuild since that count changes as items are taken/added.
            UiLayout.StretchTop(_playerSectionRect, HeaderHeight + containerCount * RowHeight + SectionGap, 0f);

            if (_playerSection.activeSelf)
            {
                int playerCount = 0;
                foreach (var item in _ctx.World.Player.Inventory.Items.Values.ToList())
                {
                    _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                    _playerRowObjects.Add(BuildRow(_playerRows, item, def, playerCount, "Store",
                        () => _processor.Submit(new TransferItemCommand(_ctx.World.Player.ActorId, item.InstanceId, _ownerId, item.Quantity))));
                    playerCount++;
                }
            }
        }

        private GameObject BuildRow(Transform parent, ItemInstanceState item, ItemDefinition def, int index, string buttonText, System.Action onClick)
        {
            var row = new GameObject(item.InstanceId, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);

            string label = def != null && !string.IsNullOrEmpty(def.DisplayNameKey)
                ? def.DisplayNameKey
                : DisplayName.PrettifyWithoutPrefix(item.ItemId, "item_");
            AddLabel(row.transform, $"{label} x{item.Quantity}", 12f, 4f, 260f, 28f);
            AddButton(row.transform, buttonText, onClick, 280f, 2f);

            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            UiLayout.StretchTop(header.GetComponent<RectTransform>(), 0f, HeaderHeight);
            _titleLabel = AddLabel(header.transform, "Container", 12f, 10f, 160f, 30f);
            AddButton(header.transform, "Take All", TakeAll, 170f, 8f);
            AddButton(header.transform, "Close (Esc)", Close, 280f, 8f);

            var containerRowsGo = new GameObject("ContainerRows", typeof(RectTransform));
            containerRowsGo.transform.SetParent(root, false);
            UiLayout.StretchTop(containerRowsGo.GetComponent<RectTransform>(), HeaderHeight, 0f);
            _containerRows = containerRowsGo.GetComponent<RectTransform>();

            _playerSection = new GameObject("PlayerSection", typeof(RectTransform));
            _playerSection.transform.SetParent(root, false);
            _playerSectionRect = _playerSection.GetComponent<RectTransform>();
            UiLayout.StretchTop(_playerSectionRect, HeaderHeight + SectionGap, 0f);
            AddLabel(_playerSection.transform, "Your Inventory", 12f, 4f, 300f, 26f);

            var playerRowsGo = new GameObject("PlayerRows", typeof(RectTransform));
            playerRowsGo.transform.SetParent(_playerSection.transform, false);
            UiLayout.StretchTop(playerRowsGo.GetComponent<RectTransform>(), SectionHeaderHeight, 0f);
            _playerRows = playerRowsGo.GetComponent<RectTransform>();
        }

        private void TakeAll()
        {
            if (_ctx == null || _ownerId == null) return;
            if (!InventoryOwnerResolver.TryResolve(_ctx, _ownerId, out var inv)) return;

            foreach (string instanceId in inv.Items.Keys.ToList())
            {
                var item = inv.Items[instanceId];
                _processor.Submit(new TransferItemCommand(_ownerId, instanceId, _ctx.World.Player.ActorId, item.Quantity));
            }
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

        private static void AddButton(Transform parent, string text, System.Action onClick, float x, float y)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, 130f, 32f);
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
