using System.Collections.Generic;
using System.Linq;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
using TMPro;
using UnityEngine;
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
        private const string ShelterStoragePrefix = "shelter_storage:";

        private GameContext _ctx;
        private CommandProcessor _processor;

        private string _ownerId;
        private TextMeshProUGUI _titleLabel;
        private RectTransform _containerRows;
        private RectTransform _playerRows;
        private GameObject _playerSection;

        private readonly List<GameObject> _containerRowObjects = new List<GameObject>();
        private readonly List<GameObject> _playerRowObjects = new List<GameObject>();

        private void Awake()
        {
            BuildLayout();
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            if (_ctx != null)
            {
                _ctx.Events.Subscribe<ContainerViewRequested>(OnContainerViewRequested);
                _ctx.Events.Subscribe<InventoryChanged>(OnInventoryChanged);
            }

            // Deactivating here (not in Awake) — SetActive(false) called from an object's own
            // Awake() stops Unity from ever calling that object's Start(), which would have
            // silently broken the ContainerViewRequested subscription above forever.
            gameObject.SetActive(false);
        }

        private void OnContainerViewRequested(ContainerViewRequested evt)
        {
            _ownerId = evt.OwnerId;
            _titleLabel.text = evt.TitleKey;
            _playerSection.SetActive(_ownerId.StartsWith(ShelterStoragePrefix));
            gameObject.SetActive(true);
            Rebuild();
        }

        private void OnInventoryChanged(InventoryChanged evt)
        {
            if (!gameObject.activeSelf || _ctx == null) return;
            if (evt.OwnerId == _ownerId || evt.OwnerId == _ctx.World.Player.ActorId) Rebuild();
        }

        public void Close() => gameObject.SetActive(false);

        private void Rebuild()
        {
            foreach (var row in _containerRowObjects) Destroy(row);
            _containerRowObjects.Clear();
            foreach (var row in _playerRowObjects) Destroy(row);
            _playerRowObjects.Clear();

            if (_ctx == null || _ownerId == null) return;

            if (InventoryOwnerResolver.TryResolve(_ctx, _ownerId, out var containerInv))
            {
                foreach (var item in containerInv.Items.Values.ToList())
                {
                    _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                    _containerRowObjects.Add(BuildRow(_containerRows, item, def, "Take",
                        () => _processor.Submit(new TransferItemCommand(_ownerId, item.InstanceId, _ctx.World.Player.ActorId, item.Quantity))));
                }
            }

            if (_playerSection.activeSelf)
            {
                foreach (var item in _ctx.World.Player.Inventory.Items.Values.ToList())
                {
                    _ctx.Definitions.TryGetItem(item.ItemId, out ItemDefinition def);
                    _playerRowObjects.Add(BuildRow(_playerRows, item, def, "Store",
                        () => _processor.Submit(new TransferItemCommand(_ctx.World.Player.ActorId, item.InstanceId, _ownerId, item.Quantity))));
                }
            }
        }

        private GameObject BuildRow(Transform parent, ItemInstanceState item, ItemDefinition def, string buttonText, System.Action onClick)
        {
            var row = new GameObject(item.InstanceId, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<HorizontalLayoutGroup>().spacing = 8;

            string label = def?.DisplayNameKey ?? item.ItemId;
            AddLabel(row.transform, $"{label} x{item.Quantity}", 220);
            AddButton(row.transform, buttonText, onClick);

            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(root, false);
            _titleLabel = AddLabel(header.transform, "Container", 200);
            AddButton(header.transform, "Take All", TakeAll);
            AddButton(header.transform, "Close", Close);

            var containerRowsGo = new GameObject("ContainerRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            containerRowsGo.transform.SetParent(root, false);
            _containerRows = containerRowsGo.GetComponent<RectTransform>();

            _playerSection = new GameObject("PlayerSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _playerSection.transform.SetParent(root, false);
            AddLabel(_playerSection.transform, "Your Inventory", 300);
            var playerRowsGo = new GameObject("PlayerRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            playerRowsGo.transform.SetParent(_playerSection.transform, false);
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

        private static void AddButton(Transform parent, string text, System.Action onClick)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 24);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
            AddLabel(go.transform, text, 80);
        }
    }
}
