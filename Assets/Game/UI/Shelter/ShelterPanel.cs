using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Text;
using LastHope.Systems.Registry;
using LastHope.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Shelter
{
    /// <summary>
    /// Toggled with N (S12). Power allocation (cycle each power-consuming module's priority),
    /// water stocks (Purify Batch per Purifier module, Collect Water into carryable bottles), and
    /// a short active-task summary — the detailed Build/Pause/Cancel controls live in BuildPanel
    /// (B), this panel is about ongoing shelter operation, not construction. Same lifecycle-safe
    /// pattern as BuildPanel/WorldMapPanel/ContainerPanel.
    /// </summary>
    public sealed class ShelterPanel : MonoBehaviour
    {
        private const string PanelName = "Shelter";
        private const float HeaderHeight = 48f;
        private const float RowHeight = 36f;

        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _toggleAction;
        private InputAction _closeAction;
        private CanvasGroup _canvasGroup;
        private bool _visible;
        private RectTransform _rowContainer;
        private readonly List<GameObject> _rows = new List<GameObject>();

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            BuildLayout();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _toggleAction = map?.FindAction("ToggleShelter", throwIfNotFound: false);
                _closeAction = map?.FindAction("Close", throwIfNotFound: false);
            }

            SetVisible(false);
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            if (_ctx != null)
            {
                _ctx.Events.Subscribe<ExclusivePanelOpened>(OnExclusivePanelOpened);
                _ctx.Events.Subscribe<PowerStateChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<WaterStocksChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<ShelterWaterChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<NpcStateChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<TaskStateChanged>(_ => { if (_visible) Rebuild(); });
            }
        }

        private void OnEnable()
        {
            _toggleAction?.Enable();
            _closeAction?.Enable();
        }

        private void Update()
        {
            if (_toggleAction != null && _toggleAction.WasPressedThisFrame())
                SetVisible(!_visible);
            else if (_visible && _closeAction != null && _closeAction.WasPressedThisFrame())
                SetVisible(false);
        }

        private void OnExclusivePanelOpened(ExclusivePanelOpened evt)
        {
            if (evt.PanelName != PanelName && _visible) SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            _visible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
            if (visible)
            {
                Rebuild();
                _ctx?.Events.Publish(new ExclusivePanelOpened(PanelName));
            }
        }

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
            if (_ctx == null) return;

            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter)) return;

            int index = 0;
            _rows.Add(SummaryRow(shelter, index++));

            if (shelter.EventFlags.Contains(ShelterEventFlags.GroundFloorLost))
                _rows.Add(EvacuateRow(index++));

            foreach (var module in shelter.Modules.Values)
            {
                if (!_ctx.Definitions.TryGetModule(module.ModuleId, out var def)) continue;
                if (def.PowerDemand > 0f) _rows.Add(PowerRow(shelter, module, index++));
                if (def.Tags.Contains("purifier")) _rows.Add(PurifierRow(module, index++));
            }

            foreach (var npc in _ctx.World.NpcStates.Values)
            {
                if (npc.Recruited) _rows.Add(NpcRow(npc, index++));
            }

            _rows.Add(CollectWaterRow(index++));
            _rows.Add(TaskSummaryRow(index++));
        }

        private GameObject SummaryRow(ShelterState shelter, int index)
        {
            var row = NewRow(index);
            AddLabel(row.transform, $"Battery {shelter.Power.BatteryCharge:0}/{_ctx.Definitions.Balance.Power.BatteryMaxCharge:0}   Clean Water {shelter.WaterStocks.Clean:0}   Untreated {shelter.WaterStocks.Untreated:0}   Flood {shelter.WaterIntrusion.Level} ({shelter.WaterIntrusion.Units:0}/100)   Occupants {shelter.Occupants}/{shelter.LivingCapacity}", 12f, 6f, 1000f, 28f);
            return row;
        }

        private GameObject EvacuateRow(int index)
        {
            var row = NewRow(index);
            AddLabel(row.transform, "Ground floor lost — shelter unsafe.", 12f, 6f, 400f, 28f);
            AddButton(row.transform, "Evacuate (leave storage behind)", () =>
            {
                var result = _processor.Submit(new EvacuateCommand(_ctx.World.Player.ActorId));
                if (!result.Success) Debug.Log($"[Shelter] Evacuate failed: {result.Code}");
                Rebuild();
            }, 420f, 4f);
            return row;
        }

        private GameObject NpcRow(NpcState npc, int index)
        {
            var row = NewRow(index);
            string name = _ctx.Definitions.TryGetNpc(npc.Id, out var def) ? def.DisplayName : DisplayName.Prettify(npc.Id);
            AddLabel(row.transform, $"{name} [{npc.Health}]  Trust {npc.Trust}  Hunger {npc.Hunger:0}  Thirst {npc.Thirst:0}", 12f, 6f, 700f, 28f);
            return row;
        }

        private GameObject PowerRow(ShelterState shelter, ModuleState module, int index)
        {
            var row = NewRow(index);
            var priority = shelter.Power.Priorities.TryGetValue(module.InstanceId, out var p) ? p : PowerPriority.Normal;
            string moduleName = DisplayName.Prettify(module.ModuleId);
            string slotName = DisplayName.PrettifyWithoutPrefix(module.SlotId, "slot_");
            AddLabel(row.transform, $"{moduleName} [{slotName}]  {(module.Active ? "powered" : "unpowered")}", 12f, 6f, 420f, 28f);
            AddButton(row.transform, $"Priority: {priority}", () =>
            {
                var next = (PowerPriority)(((int)priority + 1) % 4);
                _processor.Submit(new SetPowerPriorityCommand(_ctx.World.Player.ActorId, module.InstanceId, next));
                Rebuild();
            }, 440f, 4f);
            return row;
        }

        private GameObject PurifierRow(ModuleState module, int index)
        {
            var row = NewRow(index);
            AddLabel(row.transform, $"Purifier filter life {module.Durability:0}", 12f, 6f, 300f, 28f);
            AddButton(row.transform, "Purify Batch", () =>
            {
                var result = _processor.Submit(new StartPurifyBatchCommand(_ctx.World.Player.ActorId, module.InstanceId));
                if (!result.Success) Debug.Log($"[Shelter] Purify batch failed: {result.Code}");
                Rebuild();
            }, 320f, 4f);
            return row;
        }

        private GameObject CollectWaterRow(int index)
        {
            var row = NewRow(index);
            AddLabel(row.transform, "Bottle up Clean Water:", 12f, 6f, 250f, 28f);
            AddButton(row.transform, "Collect 1", () =>
            {
                var result = _processor.Submit(new CollectWaterCommand(_ctx.World.Player.ActorId, 1));
                if (!result.Success) Debug.Log($"[Shelter] Collect water failed: {result.Code}");
                Rebuild();
            }, 260f, 4f);
            return row;
        }

        private GameObject TaskSummaryRow(int index)
        {
            var row = NewRow(index);
            int activeCount = _ctx.World.ActiveTasks.Count;
            AddLabel(row.transform, $"Active shelter tasks: {activeCount} (see Build panel — B)", 12f, 6f, 500f, 28f);
            return row;
        }

        private GameObject NewRow(int index)
        {
            var row = new GameObject("Row" + index, typeof(RectTransform));
            row.transform.SetParent(_rowContainer, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);
            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            UiLayout.StretchTop(header.GetComponent<RectTransform>(), 0f, HeaderHeight);
            AddLabel(header.transform, "Shelter (N)", 12f, 10f, 300f, 32f);
            AddButton(header.transform, "Close (Esc)", () => SetVisible(false), 320f, 8f);

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
            label.fontSize = 18;
            label.color = Color.white;
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, width, height);
            return label;
        }

        private static void AddButton(Transform parent, string text, System.Action onClick, float x, float y)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, 220f, 30f);
            go.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 14;
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
