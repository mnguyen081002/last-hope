using System;
using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Data.Definitions;
using LastHope.Systems.Registry;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastHope.UI.Map
{
    /// <summary>
    /// Opened by WorldMapRequested (TravelPointView, S8) or toggled directly with M. Lists every
    /// route connected to the player's current location with ETA/flood/current/return-window, and
    /// a Travel button that submits BeginTravelCommand — the player picks among routes here instead
    /// of a TravelPointView being bound to one hardcoded route (S6 behavior).
    /// </summary>
    public sealed class WorldMapPanel : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _toggleAction;
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
            if (_ctx != null) _ctx.Events.Subscribe<WorldMapRequested>(_ => Open());
        }

        private void OnEnable() => _toggleAction?.Enable();
        private void OnDisable() => _toggleAction?.Disable();

        private void Update()
        {
            if (_toggleAction != null && _toggleAction.WasPressedThisFrame())
                SetVisible(!_visible);
        }

        private void Open()
        {
            SetVisible(true);
            Rebuild();
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

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            if (_ctx == null) return;
            if (!_ctx.Definitions.TryGetLocation(_ctx.World.Player.CurrentLocationId, out var location)) return;

            foreach (string routeId in location.ConnectedRouteIds)
            {
                if (!_ctx.Definitions.TryGetRoute(routeId, out var route)) continue;
                _rows.Add(BuildRouteRow(routeId, route));
            }
        }

        private GameObject BuildRouteRow(string routeId, RouteDefinition route)
        {
            var hazard = HazardRules.EvaluateRoute(route, _ctx.Definitions.DisasterPhasesSorted, _ctx.World.WorldTimeMinutes);
            var equipment = EquipmentRules.ResolveTravelProtection(_ctx.World.Player.Inventory, _ctx.Definitions);
            var crossing = TravelRules.EvaluateCrossing(hazard, _ctx.World.Player.Condition, _ctx.Definitions.Balance.Hazard, equipment);
            var window = ReturnWindowCalculator.Evaluate(route, _ctx.Definitions.DisasterPhasesSorted, _ctx.World.WorldTimeMinutes);

            string destinationId = _ctx.World.Player.CurrentLocationId == route.FromLocationId ? route.ToLocationId : route.FromLocationId;
            float loadFactor = InventoryRules.LoadFactorFor(_ctx.World.Player.Inventory.Overload, _ctx.Definitions.Balance);
            int eta = (int)Math.Ceiling(route.TravelMinutes * loadFactor * crossing.TimeFactor);

            string label = $"{destinationId}  ETA {eta}m  Flood {hazard.FloodLevel} Current {hazard.CurrentLevel}";
            if (!crossing.Passable) label += "  IMPASSABLE";
            if (window.MinutesUntilImpassable.HasValue) label += $"  closes in {window.MinutesUntilImpassable}m";
            else if (window.MinutesUntilWorse.HasValue) label += $"  worsens in {window.MinutesUntilWorse}m";

            var row = new GameObject(routeId, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_rowContainer, false);
            row.GetComponent<HorizontalLayoutGroup>().spacing = 8;

            AddLabel(row.transform, label, 460);
            AddButton(row.transform, "Travel", () =>
            {
                _processor.Submit(new BeginTravelCommand(_ctx.World.Player.ActorId, routeId));
                Rebuild();
            });

            return row;
        }

        private void BuildLayout()
        {
            RectTransform root = GetComponent<RectTransform>();

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(root, false);
            AddLabel(header.transform, "World Map (M)", 200);
            AddButton(header.transform, "Close", () => SetVisible(false));

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

        private static void AddButton(Transform parent, string text, Action onClick)
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
