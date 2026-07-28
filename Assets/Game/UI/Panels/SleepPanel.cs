using LastHope.Core.Events;
using LastHope.Core.UI;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.UI.Panels
{
    /// <summary>Chọn số giờ ngủ (Sleep Simulation, BL-P3-13). Tự mở khi nghe <see cref="BedOpened"/>.</summary>
    public class SleepPanel : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;

        InputAction closeAction;
        bool visible;
        float hours = 6f;

        void Awake()
        {
            if (controls != null) closeAction = controls.FindActionMap("Gameplay", true).FindAction("Close", true);
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
                GameBootstrapper.Services.Events.Unsubscribe<BedOpened>(OnOpened);
        }

        void Update()
        {
            if (visible && closeAction != null && closeAction.WasPressedThisFrame()) visible = false;
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<BedOpened>(OnOpened);
        }

        void OnOpened(BedOpened e) => visible = !visible;

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var balance = services.Definitions.Balance.Shelter;

            const float width = 320f, height = 160f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Ngủ");
            GUILayout.Label($"Số giờ: {hours:F0}");
            hours = GUILayout.HorizontalSlider(hours, balance.SleepMinHours, balance.SleepMaxHours);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ngủ"))
            {
                services.Commands.Submit(new SleepCommand(Mathf.Round(hours)));
                visible = false;
            }
            if (GUILayout.Button("Hủy")) visible = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
