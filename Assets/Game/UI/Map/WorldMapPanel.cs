using System;
using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Systems.Intel;
using LastHope.Systems.Registry;
using LastHope.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Map
{
    /// <summary>
    /// Opened by WorldMapRequested (TravelPointView, S8) or toggled directly with M. Lists every
    /// route connected to the player's current location, but only shows real numbers for what the
    /// player actually knows (S15, IntelState) — routes without a record show "?" instead of live
    /// ETA/flood/current. A route connects directly to the player's current position, so it's
    /// re-observed Confirmed-fresh every time this panel opens (standing right at its mouth);
    /// routes intel was gathered on elsewhere decay by information age (IntelRules). A Travel
    /// button submits BeginTravelCommand regardless of what's shown — the command validates
    /// against real state, intel is display-only.
    /// </summary>
    public sealed class WorldMapPanel : MonoBehaviour
    {
        private const string PanelName = "WorldMap";

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

            // Resolved here, not Start(): OnEnable() runs before Start(), so resolving
            // _toggleAction in Start() meant OnEnable()'s Enable() call always hit a still-null
            // reference and never actually enabled the action.
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _toggleAction = map?.FindAction("ToggleMap", throwIfNotFound: false);
                _closeAction = map?.FindAction("Close", throwIfNotFound: false);
            }

            SetVisible(false);
        }

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);

            // Subscribing here relies on Start() actually running — it wouldn't have if Awake()
            // called gameObject.SetActive(false) directly (a GameObject deactivated from within
            // its own Awake() never gets its Start() called at all).
            if (_ctx != null)
            {
                _ctx.Events.Subscribe<WorldMapRequested>(_ => Open());
                _ctx.Events.Subscribe<ExclusivePanelOpened>(OnExclusivePanelOpened);
            }
        }

        // Enable only, never Disable(): these are shared InputAction instances (the same
        // "ToggleMap"/"Close" objects ContainerPanel also reads) — see ContainerPanel's OnEnable
        // comment for why Disable() on a shared action is unsafe (2026-07-24 Esc bugfix).
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

        private void Open()
        {
            SetVisible(true);
            Rebuild();
            _ctx.Events.Publish(new ExclusivePanelOpened(PanelName));
        }

        // Another focused panel (e.g. ContainerPanel) just opened — close so the two don't render
        // on top of each other unreadably (bugfix 2026-07-24).
        private void OnExclusivePanelOpened(ExclusivePanelOpened evt)
        {
            if (evt.PanelName != PanelName && _visible) SetVisible(false);
        }

        // CanvasGroup, not gameObject.SetActive(false): deactivating this GameObject would stop
        // its own Update() from running, so the M key could never be polled again to reopen it.
        private void SetVisible(bool visible)
        {
            _visible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private const float HeaderHeight = 48f;
        private const float RowHeight = 40f;

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            if (_ctx == null) return;
            if (!_ctx.Definitions.TryGetLocation(_ctx.World.Player.CurrentLocationId, out var location)) return;

            int index = 0;
            foreach (string routeId in location.ConnectedRouteIds)
            {
                if (!_ctx.Definitions.TryGetRoute(routeId, out var route)) continue;

                // Standing at the mouth of a connected route counts as direct observation —
                // always refresh it to Confirmed-now before rendering, so the map never shows a
                // stale "?" for the route the player is looking straight at.
                IntelSystem.RecordRouteObservation(_ctx, routeId, IntelConfidence.Confirmed);

                _rows.Add(BuildRouteRow(routeId, route, index));
                index++;
            }
        }

        private GameObject BuildRouteRow(string routeId, RouteDefinition route, int index)
        {
            string destinationId = _ctx.World.Player.CurrentLocationId == route.FromLocationId ? route.ToLocationId : route.FromLocationId;

            string label = _ctx.World.Intel.Records.TryGetValue(routeId, out var record)
                ? IntelRouteLabel(destinationId, record)
                : $"{destinationId}   ?";

            var row = new GameObject(routeId, typeof(RectTransform));
            row.transform.SetParent(_rowContainer, false);
            UiLayout.StretchTop(row.GetComponent<RectTransform>(), index * RowHeight, RowHeight);

            AddLabel(row.transform, label, 12f, 6f, 780f, 28f);
            AddButton(row.transform, "Travel", () =>
            {
                _processor.Submit(new BeginTravelCommand(_ctx.World.Player.ActorId, routeId));
                Rebuild();
            }, 800f, 4f);

            return row;
        }

        /// <summary>Renders from the IntelRecord only — never live RouteState/HazardRules. Age and
        /// confidence make stale memory visibly stale instead of silently wrong.</summary>
        private string IntelRouteLabel(string destinationId, IntelRecord record)
        {
            var confidence = IntelRules.EffectiveConfidence(record, _ctx.World.WorldTimeMinutes, _ctx.Definitions.Balance.Intel);
            long ageMinutes = _ctx.World.WorldTimeMinutes - record.ObservedAtMinute;

            string label = $"{destinationId}   Flood {record.FloodLevel} Current {record.CurrentLevel}   [{confidence}, {ageMinutes}' ago]";
            if (record.Closed == true) label += "   CLOSED (as last seen)";
            return label;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            UiLayout.StretchTop(header.GetComponent<RectTransform>(), 0f, HeaderHeight);
            AddLabel(header.transform, "World Map (M)", 12f, 10f, 300f, 32f);
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
            label.fontSize = 22;
            label.color = Color.white;
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, width, height);
            return label;
        }

        private static void AddButton(Transform parent, string text, Action onClick, float x, float y)
        {
            var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiLayout.TopLeft(go.GetComponent<RectTransform>(), x, y, 140f, 32f);
            go.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 20;
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
