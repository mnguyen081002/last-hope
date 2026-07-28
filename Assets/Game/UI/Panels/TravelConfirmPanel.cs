using LastHope.Core.Events;
using LastHope.Core.UI;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Hazard;
using LastHope.Systems.Travel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.UI.Panels
{
    /// <summary>
    /// Panel xác nhận trước khi thật sự đi (BL-P2-11 — Return Window UI, phạm vi rút gọn cho
    /// P2: không phải World Map đầy đủ, chỉ preview khi tương tác TravelPoint). Chưa submit
    /// gì tới khi người chơi bấm "Xác nhận" — Hủy không tốn thời gian trong game.
    /// </summary>
    public class TravelConfirmPanel : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;

        InputAction closeAction;
        bool visible;
        string routeId;

        void Awake()
        {
            if (controls != null)
            {
                closeAction = controls.FindActionMap("Gameplay", true).FindAction("Close", true);
            }
        }

        void OnEnable()
        {
            closeAction?.Enable();
            if (GameBootstrapper.IsReady) Subscribe();
            else GameBootstrapper.Ready += Subscribe;
        }

        void OnDisable()
        {
            closeAction?.Disable();
            GameBootstrapper.Ready -= Subscribe;
            if (GameBootstrapper.IsReady)
                GameBootstrapper.Services.Events.Unsubscribe<TravelPointOpened>(OnOpened);
        }

        void Update()
        {
            if (visible && closeAction != null && closeAction.WasPressedThisFrame()) Close();
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<TravelPointOpened>(OnOpened);
        }

        /// <summary>Tương tác lại đúng travel point đang mở = đóng (toggle), không mở lại.</summary>
        void OnOpened(TravelPointOpened e)
        {
            if (visible && routeId == e.RouteId)
            {
                Close();
                return;
            }

            routeId = e.RouteId;
            visible = true;
        }

        void Close() => visible = false;

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var world = services.World;
            var definitions = services.Definitions;
            var balance = definitions.Balance;

            if (!definitions.TryGetRoute(routeId, out var route))
            {
                Close();
                return;
            }

            var routeState = world.GetOrCreateRoute(routeId);
            var phaseNow = DisasterPhaseSystem.CurrentPhase(world.WorldTimeMinutes, balance.DisasterPhase);
            var flood = HazardSystem.EffectiveFlood(route, routeState, phaseNow);

            int oneWayMinutes = TravelSystem.ComputeTravelMinutes(world, definitions, routeId);
            int roundTripMinutes = oneWayMinutes * 2;
            var phaseAtReturn = DisasterPhaseSystem.CurrentPhase(
                world.WorldTimeMinutes + roundTripMinutes, balance.DisasterPhase);
            bool phaseRiskAtReturn = phaseAtReturn != phaseNow;

            const float width = 380f, height = 260f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Xác nhận di chuyển");
            GUILayout.Space(6f);

            GUILayout.Label($"Thời gian đi (một chiều): {oneWayMinutes} phút");
            GUILayout.Label($"Dự kiến quay lại: {roundTripMinutes} phút nữa (khứ hồi)");
            GUILayout.Space(4f);
            GUILayout.Label($"Flood hiện tại: {flood}");
            GUILayout.Label($"Current: {routeState.Current}   Điện: {(routeState.IsElectrified ? "Có" : "Không")}");

            if (phaseRiskAtReturn)
            {
                GUILayout.Space(6f);
                var prevColor = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label("⚠ Tuyến có thể không còn sử dụng được khi quay lại.");
                GUI.color = prevColor;
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Xác nhận đi"))
            {
                services.Commands.Submit(new BeginTravelCommand(routeId));
                Close();
            }
            if (GUILayout.Button("Hủy")) Close();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
