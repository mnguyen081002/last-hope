using System.Collections.Generic;
using System.Text;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Core.Text;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
using LastHope.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Shelter
{
    /// <summary>
    /// Toggled with B (S11). Lists every Build Slot in the Main Shelter: empty+buildable slots
    /// show one "Build" button per module allowed in that zone, in-progress slots show
    /// Progress/Pause/Resume/Cancel, completed slots show Dismantle. Same lifecycle-safe pattern
    /// as WorldMapPanel/ContainerPanel (CanvasGroup, action resolve in Awake, ExclusivePanelOpened)
    /// — copied deliberately after the 2026-07-24 lifecycle bugfix rather than reinvented.
    /// </summary>
    public sealed class BuildPanel : MonoBehaviour
    {
        private const string PanelName = "Build";
        private const float HeaderHeight = 48f;
        private const float RowHeight = 40f;

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
                _toggleAction = map?.FindAction("ToggleBuild", throwIfNotFound: false);
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
                _ctx.Events.Subscribe<TaskStateChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<BuildProgressChanged>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<ModuleCompleted>(_ => { if (_visible) Rebuild(); });
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
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelterState)) return;

            int index = 0;
            foreach (var kvp in shelterState.BuildSlots)
                _rows.Add(BuildSlotRow(shelterState, kvp.Key, kvp.Value, index++));
        }

        private GameObject BuildSlotRow(ShelterState shelter, string slotId, BuildSlotState slot, int index)
        {
            var row = new GameObject(slotId, typeof(RectTransform));
            row.transform.SetParent(_rowContainer, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);

            string slotName = DisplayName.PrettifyWithoutPrefix(slotId, "slot_");
            var activeTask = _ctx.World.ActiveTasks.Find(t => t.TargetId == slotId);

            if (slot.Locked)
            {
                AddLabel(row.transform, $"{slotName}: Locked", 12f, 6f, 780f, 28f);
            }
            else if (!string.IsNullOrEmpty(slot.ModuleInstanceId))
            {
                var module = shelter.Modules[slot.ModuleInstanceId];
                string moduleName = DisplayName.Prettify(module.ModuleId);
                AddLabel(row.transform, $"{slotName}: {moduleName} (durability {module.Durability:0}, {(module.Active ? "active" : "inactive")})", 12f, 6f, 620f, 28f);
                AddButton(row.transform, "Dismantle", () =>
                {
                    _processor.Submit(new DismantleModuleCommand(_ctx.World.Player.ActorId, slotId));
                    Rebuild();
                }, 640f, 4f);
            }
            else if (activeTask != null)
            {
                string moduleName = DisplayName.Prettify(activeTask.ModuleId);
                AddLabel(row.transform, $"{slotName}: building {moduleName} ({activeTask.Progress:0}%) [{activeTask.Status}]", 12f, 6f, 460f, 28f);
                if (activeTask.Status == TaskStatus.Running)
                    AddButton(row.transform, "Pause", () => { _processor.Submit(new PauseTaskCommand(_ctx.World.Player.ActorId, activeTask.TaskId)); Rebuild(); }, 480f, 4f);
                else
                    AddButton(row.transform, "Resume", () => { _processor.Submit(new ResumeTaskCommand(_ctx.World.Player.ActorId, activeTask.TaskId)); Rebuild(); }, 480f, 4f);
                AddButton(row.transform, "Cancel", () => { _processor.Submit(new CancelTaskCommand(_ctx.World.Player.ActorId, activeTask.TaskId)); Rebuild(); }, 620f, 4f);
            }
            else
            {
                AddLabel(row.transform, $"{slotName}:", 12f, 6f, 100f, 28f);
                float x = 110f;
                if (BuildRules.TryFindZoneForSlot(_ctx.Definitions.ShelterZones, slotId, out var zone))
                {
                    foreach (var module in _ctx.Definitions.Modules.Values)
                    {
                        if (!module.AllowedZoneIds.Contains(zone.Id)) continue;
                        string moduleId = module.Id;
                        string moduleName = DisplayName.Prettify(moduleId);
                        AddButton(row.transform, $"Build {moduleName} ({FormatMaterials(module.Materials)})", () =>
                        {
                            var result = _processor.Submit(new StartBuildCommand(_ctx.World.Player.ActorId, slotId, moduleId));
                            if (!result.Success) Debug.Log($"[Build] Start '{moduleId}' at '{slotId}' failed: {result.Code}");
                            Rebuild();
                        }, x, 4f);
                        x += 260f;
                    }
                }
            }

            return row;
        }

        private static string FormatMaterials(IReadOnlyDictionary<string, int> materials)
        {
            var sb = new StringBuilder();
            foreach (var kvp in materials)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(kvp.Value).Append('x').Append(DisplayName.PrettifyWithoutPrefix(kvp.Key, "item_"));
            }
            return sb.ToString();
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            UiLayout.StretchTop(header.GetComponent<RectTransform>(), 0f, HeaderHeight);
            AddLabel(header.transform, "Build (B)", 12f, 10f, 300f, 32f);
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
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, 250f, 32f);
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
