using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Inventory;
using LastHope.Systems.Registry;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.UI.Panels
{
    /// <summary>
    /// Panel túi đồ người chơi tự bấm mở (khác <see cref="DebugTools.Panel.DebugPanel"/> —
    /// panel đó là cheat tool cho dev, panel này là gameplay thật). Toggle qua action
    /// <c>ToggleInventory</c> (nhấn lại = đóng) hoặc ESC (action <c>Close</c>).
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;

        InputAction toggleAction;
        InputAction closeAction;
        bool visible;
        Vector2 scroll;
        float openedAtRealTime;

        void Awake()
        {
            if (controls != null)
            {
                var map = controls.FindActionMap("Gameplay", true);
                toggleAction = map.FindAction("ToggleInventory", true);
                closeAction = map.FindAction("Close", true);
            }
        }

        void OnEnable()
        {
            toggleAction?.Enable();
            closeAction?.Enable();
        }

        void OnDisable()
        {
            toggleAction?.Disable();
            closeAction?.Disable();
        }

        void Update()
        {
            if (toggleAction != null && toggleAction.WasPressedThisFrame())
            {
                SetVisible(!visible);
            }
            else if (visible && closeAction != null && closeAction.WasPressedThisFrame())
            {
                SetVisible(false);
            }
        }

        void SetVisible(bool value)
        {
            visible = value;
            if (visible)
            {
                openedAtRealTime = Time.realtimeSinceStartup;
            }
            else if (GameBootstrapper.IsReady)
            {
                GameBootstrapper.Services.Telemetry.LogInventoryOpenDuration(
                    Time.realtimeSinceStartup - openedAtRealTime);
            }
        }

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var inventory = services.World.Player.Inventory;
            string currentLocationId = services.World.Player.CurrentLocationId;

            const float width = 320f, height = 380f;
            var rect = new Rect(10f, Screen.height - height - 10f, width, height);

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Túi đồ");

            float weight = InventoryOps.TotalWeightKg(inventory, services.Definitions);
            float volume = InventoryOps.TotalVolumeLiters(inventory, services.Definitions);
            var tier = InventorySystem.ComputeLoadTier(inventory, services.Definitions, services.Definitions.Balance.Inventory);
            GUILayout.Label($"{weight:F1}/{inventory.CapacityKg:F0} kg   {volume:F1}/{inventory.CapacityLiters:F0} L   ({tier})");

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height - 150f));

            if (!string.IsNullOrEmpty(inventory.CarriedObjectItemId))
            {
                DrawRow(services, $"[ôm] {inventory.CarriedObjectItemId}", inventory.CarriedObjectItemId,
                    InventoryOwner.Player, InventoryOwner.DroppedItems(currentLocationId));
            }

            foreach (var slot in new List<ItemInstanceState>(inventory.Slots))
            {
                DrawRow(services, $"{slot.ItemId} ×{slot.Quantity}", slot.ItemId,
                    InventoryOwner.Player, InventoryOwner.DroppedItems(currentLocationId));
            }
            GUILayout.EndScrollView();

            var dropped = services.World.GetOrCreateLocation(currentLocationId).DroppedItems;
            if (dropped.Count > 0)
            {
                GUILayout.Label("Đồ dưới đất tại đây");
                foreach (var slot in new List<ItemInstanceState>(dropped))
                {
                    DrawRow(services, $"{slot.ItemId} ×{slot.Quantity}", slot.ItemId,
                        InventoryOwner.DroppedItems(currentLocationId), InventoryOwner.Player, buttonLabel: "Nhặt");
                }
            }

            GUILayout.EndArea();
        }

        static void DrawRow(
            GameServices services, string label, string itemId,
            InventoryOwner from, InventoryOwner to, string buttonLabel = "Bỏ")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(200f));
            if (GUILayout.Button(buttonLabel, GUILayout.Width(60f)))
            {
                services.Commands.Submit(new TransferItemCommand(from, to, itemId, 1));
            }
            GUILayout.EndHorizontal();
        }
    }
}
