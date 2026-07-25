using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Text;
using LastHope.Systems.Registry;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Events
{
    /// <summary>
    /// Toggled with V (S14). Lists Active events with priority + deadline countdown and a button
    /// per available response (ResolveEventCommand) — events are now resolvable in the player UI,
    /// not just the F2 debug panel. Undiscovered instances are deliberately hidden. A short
    /// history of finished events (Resolved/Expired/PersistentConsequence) sits below. Same
    /// lifecycle-safe pattern as ShelterPanel/BuildPanel (CanvasGroup, ExclusivePanelOpened).
    /// </summary>
    public sealed class EventPanel : MonoBehaviour
    {
        private const string PanelName = "Events";
        private const float HeaderHeight = 48f;
        private const float RowHeight = 36f;
        private const int HistoryRows = 6;

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
                _toggleAction = map?.FindAction("ToggleEvents", throwIfNotFound: false);
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
                _ctx.Events.Subscribe<EventTriggered>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<EventDiscovered>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<EventResolved>(_ => { if (_visible) Rebuild(); });
                _ctx.Events.Subscribe<EventExpired>(_ => { if (_visible) Rebuild(); });
                // Rebuild once per game minute while open so deadline countdowns stay current.
                _ctx.Events.Subscribe<WorldTimeChanged>(_ => { if (_visible) Rebuild(); });
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

            int index = 0;
            int activeCount = 0;
            foreach (var instance in _ctx.World.ActiveEvents)
            {
                if (instance.State != EventLifecycleState.Active) continue;
                if (!_ctx.Definitions.TryGetEvent(instance.EventId, out var def)) continue;
                _rows.Add(ActiveEventRow(instance, def, index++));
                activeCount++;
            }

            if (activeCount == 0)
            {
                var row = NewRow(index++);
                AddLabel(row.transform, "No active events.", 12f, 6f, 400f, 28f);
                _rows.Add(row);
            }

            index = AddHistoryRows(index);
        }

        private GameObject ActiveEventRow(ActiveEventState instance, Data.Definitions.EventDefinition def, int index)
        {
            var row = NewRow(index);

            string deadline = instance.DeadlineMinute.HasValue
                ? $"  deadline {System.Math.Max(0, instance.DeadlineMinute.Value - _ctx.World.WorldTimeMinutes)}'"
                : "";
            AddLabel(row.transform, $"{Name(instance.EventId)} [{def.Priority}]{deadline}", 12f, 6f, 460f, 28f);

            float x = 480f;
            foreach (string responseId in def.AvailableResponses)
            {
                string captured = responseId;
                AddButton(row.transform, DisplayName.Prettify(responseId), () =>
                {
                    var result = _processor.Submit(new ResolveEventCommand(_ctx.World.Player.ActorId, instance.EventInstanceId, captured));
                    if (!result.Success) Debug.Log($"[Events] Resolve failed: {result.Code}");
                    Rebuild();
                }, x, 4f);
                x += 230f;
            }
            return row;
        }

        private int AddHistoryRows(int index)
        {
            var finished = new List<ActiveEventState>();
            foreach (var instance in _ctx.World.ActiveEvents)
            {
                if (instance.State == EventLifecycleState.Resolved
                    || instance.State == EventLifecycleState.Expired
                    || instance.State == EventLifecycleState.PersistentConsequence)
                    finished.Add(instance);
            }
            if (finished.Count == 0) return index;

            var header = NewRow(index++);
            AddLabel(header.transform, "History:", 12f, 6f, 200f, 28f);
            _rows.Add(header);

            int start = System.Math.Max(0, finished.Count - HistoryRows);
            for (int i = finished.Count - 1; i >= start; i--)
            {
                var instance = finished[i];
                string outcome = instance.State == EventLifecycleState.Resolved
                    ? $"resolved ({DisplayName.Prettify(instance.ChosenResponse ?? "")})"
                    : "expired";
                var row = NewRow(index++);
                AddLabel(row.transform, $"{Name(instance.EventId)} — {outcome}", 24f, 6f, 700f, 28f);
                _rows.Add(row);
            }
            return index;
        }

        private static string Name(string eventId) => DisplayName.PrettifyWithoutPrefix(eventId, "event_");

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
            AddLabel(header.transform, "Events (V)", 12f, 10f, 300f, 32f);
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
