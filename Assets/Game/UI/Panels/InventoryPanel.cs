using System.Collections.Generic;
using LastHope.Core.State;
using LastHope.Core.UI;
using LastHope.Data.Definitions;
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
        string statusMessage = "";

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

            const float width = 340f, height = 460f;
            var rect = new Rect(10f, Screen.height - height - 10f, width, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Túi đồ");

            float weight = InventoryOps.TotalWeightKg(inventory, services.Definitions);
            float volume = InventoryOps.TotalVolumeLiters(inventory, services.Definitions);
            var balance = services.Definitions.Balance.Inventory;
            var tier = InventorySystem.ComputeLoadTier(inventory, services.Definitions, balance);
            float ratio = InventorySystem.LoadRatio(inventory, services.Definitions);
            GUILayout.Label($"{weight:F1}/{inventory.CapacityKg:F0} kg   {volume:F1}/{inventory.CapacityLiters:F0} L   ({tier})");
            DrawCapacityBar(ratio, tier, balance.HardCapMultiplier);

            DrawEquippedRow(services);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height - 210f));

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

            if (!string.IsNullOrEmpty(statusMessage))
            {
                GUILayout.Label(statusMessage);
            }

            GUILayout.EndArea();
        }

        /// <summary>Thanh đầy tới hard cap (không phải capacity thường) — thấy rõ còn bao nhiêu
        /// khoảng overload trước khi bị Blocked, không chỉ bao nhiêu so với sức chứa gốc.</summary>
        static void DrawCapacityBar(float ratio, LoadTier tier, float hardCapMultiplier)
        {
            var barRect = GUILayoutUtility.GetRect(10f, 16f, GUILayout.ExpandWidth(true));
            GUI.Box(barRect, GUIContent.none);

            float fill = hardCapMultiplier > 0f ? Mathf.Clamp01(ratio / hardCapMultiplier) : 0f;
            var fillRect = new Rect(barRect.x + 1f, barRect.y + 1f, (barRect.width - 2f) * fill, barRect.height - 2f);

            var prevColor = GUI.color;
            GUI.color = ColorForTier(tier);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = prevColor;

            GUI.Label(barRect, $"{ratio * 100f:F0}%");
        }

        static Color ColorForTier(LoadTier tier) => tier switch
        {
            LoadTier.Blocked => new Color(0.8f, 0.2f, 0.2f),
            LoadTier.Heavy => new Color(0.85f, 0.55f, 0.15f),
            LoadTier.Light => new Color(0.85f, 0.8f, 0.2f),
            _ => new Color(0.25f, 0.65f, 0.3f),
        };

        void DrawEquippedRow(GameServices services)
        {
            var equipped = services.World.Player.Equipped;
            if (equipped.Count == 0) return;

            GUILayout.Label("Đang mặc");
            foreach (var pair in new Dictionary<EquipSlot, string>(equipped))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"[{pair.Key}] {pair.Value}", GUILayout.Width(220f));
                if (GUILayout.Button("Tháo", GUILayout.Width(60f)))
                {
                    var result = services.Commands.Submit(new UnequipItemCommand(pair.Key));
                    statusMessage = result.Success ? "" : $"Không tháo được ({result.Error}).";
                }
                GUILayout.EndHorizontal();
            }
        }

        static void DrawRow(
            GameServices services, string label, string itemId,
            InventoryOwner from, InventoryOwner to, string buttonLabel = "Bỏ")
        {
            bool isEquippable = from.Kind == InventoryOwnerKind.Player
                && services.Definitions.TryGetItem(itemId, out var item) && item.IsEquipment;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(isEquippable ? 140f : 200f));

            if (isEquippable && GUILayout.Button("Mặc", GUILayout.Width(50f)))
            {
                services.Commands.Submit(new EquipItemCommand(itemId));
            }

            if (GUILayout.Button(buttonLabel, GUILayout.Width(60f)))
            {
                services.Commands.Submit(new TransferItemCommand(from, to, itemId, 1));
            }
            GUILayout.EndHorizontal();
        }
    }
}
