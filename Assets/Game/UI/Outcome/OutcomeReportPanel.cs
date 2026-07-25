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

namespace LastHope.UI.Outcome
{
    /// <summary>
    /// Causal Outcome Report v1 (S18). Opens itself when OutcomeReached fires — no toggle key,
    /// same "opens on an event, not a key" pattern WorldMapPanel uses for WorldMapRequested. Shows
    /// the ending, the DecisionLog (Major Decisions), remaining resources, NPC outcomes, and the
    /// shelter's final flood state. Same lifecycle-safe CanvasGroup pattern as every other panel.
    /// </summary>
    public sealed class OutcomeReportPanel : MonoBehaviour
    {
        private const string PanelName = "OutcomeReport";
        private const float HeaderHeight = 48f;
        private const float RowHeight = 32f;

        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private InputAction _closeAction;
        private CanvasGroup _canvasGroup;
        private bool _visible;
        private RectTransform _rowContainer;
        private TextMeshProUGUI _headerLabel;
        private readonly List<GameObject> _rows = new List<GameObject>();

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            BuildLayout();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _closeAction = map?.FindAction("Close", throwIfNotFound: false);
            }

            SetVisible(false);
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            if (_ctx != null)
            {
                _ctx.Events.Subscribe<OutcomeReached>(OnOutcomeReached);
                _ctx.Events.Subscribe<ExclusivePanelOpened>(OnExclusivePanelOpened);
            }
        }

        private void OnEnable() => _closeAction?.Enable();

        private void Update()
        {
            if (_visible && _closeAction != null && _closeAction.WasPressedThisFrame())
                SetVisible(false);
        }

        private void OnOutcomeReached(OutcomeReached evt)
        {
            _headerLabel.text = $"Outcome: {evt.Outcome}";
            SetVisible(true);
            Rebuild();
            _ctx.Events.Publish(new ExclusivePanelOpened(PanelName));
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
        }

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
            if (_ctx == null) return;

            int index = 0;

            index = AddSectionHeader("Major Decisions:", index);
            if (_ctx.World.DecisionLog.Count == 0)
                _rows.Add(TextRow("(none)", index++));
            else
                foreach (var entry in _ctx.World.DecisionLog)
                    _rows.Add(TextRow($"{entry.Minute}'  {DisplayName.Prettify(entry.DecisionId)} — {DisplayName.Prettify(entry.Payload ?? "")}", index++));

            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            _ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter);

            index = AddSectionHeader("Resources Preserved:", index);
            if (shelter != null)
                _rows.Add(TextRow($"Clean Water {shelter.WaterStocks.Clean:0}   Untreated {shelter.WaterStocks.Untreated:0}   Modules {shelter.Modules.Count}", index++));

            index = AddSectionHeader("Shelter Outcome:", index);
            if (shelter != null)
                _rows.Add(TextRow($"{shelter.WaterIntrusion.Level} ({shelter.WaterIntrusion.Units:0}/100)", index++));

            index = AddSectionHeader("NPC Outcome:", index);
            bool anyNpc = false;
            foreach (var npc in _ctx.World.NpcStates.Values)
            {
                if (!npc.Recruited) continue;
                anyNpc = true;
                string name = _ctx.Definitions.TryGetNpc(npc.Id, out var def) ? def.DisplayName : DisplayName.Prettify(npc.Id);
                _rows.Add(TextRow($"{name} — {npc.Health}, Trust {npc.Trust}", index++));
            }
            if (!anyNpc) _rows.Add(TextRow("(none recruited)", index++));
        }

        private int AddSectionHeader(string text, int index)
        {
            var row = NewRow(index);
            var label = AddLabel(row.transform, text, 12f, 6f, 500f, 28f);
            label.fontStyle = FontStyles.Bold;
            _rows.Add(row);
            return index + 1;
        }

        private GameObject TextRow(string text, int index)
        {
            var row = NewRow(index);
            AddLabel(row.transform, text, 24f, 6f, 850f, 28f);
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
            _headerLabel = AddLabel(header.transform, "Outcome", 12f, 10f, 500f, 32f);
            AddButton(header.transform, "Close (Esc)", () => SetVisible(false), 520f, 8f);

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
