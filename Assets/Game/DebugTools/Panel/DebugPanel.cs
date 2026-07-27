using LastHope.Core.Commands;
using LastHope.Core.Diagnostics;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Systems.Boot;
using LastHope.Systems.Registry;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.DebugTools.Panel
{
    /// <summary>
    /// Bảng cheat F2: tua giờ, đổi tốc độ, thêm đồ, save/load. Đây là công cụ verify tay
    /// cho Gate M1 — mọi hệ thống mới phải thêm mục vào đây (DoD backlog mục 13).
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        const float PanelWidth = 320f;

        bool visible;
        Vector2 scroll;
        string statusMessage = "";
        string addItemId = "item_water_bottle";
        string addItemQuantity = "1";

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f2Key.wasPressedThisFrame) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var world = services.World;

            GUILayout.BeginArea(new Rect(Screen.width - PanelWidth - 10f, 10f, PanelWidth, 460f),
                GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label($"<b>Debug Panel</b> (F2)");
            GUILayout.Label(GameTimeUtil.Format(world.WorldTimeMinutes) +
                            $"  ·  phút {world.WorldTimeMinutes}");
            GUILayout.Label($"Location: {world.Player.CurrentLocationId}");

            DrawTimeControls(services);
            DrawInventoryControls(services);
            DrawSaveControls(services);

            if (!string.IsNullOrEmpty(statusMessage))
            {
                GUILayout.Space(6f);
                GUILayout.Label(statusMessage);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawTimeControls(GameServices services)
        {
            GUILayout.Space(8f);
            GUILayout.Label("— Thời gian —");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+10p")) services.Ticks.FastForward(10);
            if (GUILayout.Button("+1h")) services.Ticks.FastForward(60);
            if (GUILayout.Button("+8h")) services.Ticks.FastForward(8 * 60);
            GUILayout.EndHorizontal();

            var clock = services.Clock;
            clock.Paused = GUILayout.Toggle(clock.Paused, "Tạm dừng");
            GUILayout.Label($"Tốc độ: ×{clock.TimeScale:F1}");
            clock.TimeScale = GUILayout.HorizontalSlider(clock.TimeScale, 0f, 60f);
        }

        void DrawInventoryControls(GameServices services)
        {
            GUILayout.Space(8f);
            GUILayout.Label("— Túi đồ —");

            var inventory = services.World.Player.Inventory;
            float weight = InventoryOps.TotalWeightKg(inventory, services.Definitions);
            float volume = InventoryOps.TotalVolumeLiters(inventory, services.Definitions);
            GUILayout.Label($"{weight:F1}/{inventory.CapacityKg:F0} kg   " +
                            $"{volume:F1}/{inventory.CapacityLiters:F0} L");

            foreach (var slot in inventory.Slots)
            {
                GUILayout.Label($"  {slot.ItemId} ×{slot.Quantity}");
            }

            GUILayout.BeginHorizontal();
            addItemId = GUILayout.TextField(addItemId);
            addItemQuantity = GUILayout.TextField(addItemQuantity, GUILayout.Width(40f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Thêm"))
            {
                if (!services.Definitions.Items.ContainsKey(addItemId))
                {
                    statusMessage = $"Không có item '{addItemId}'.";
                }
                else
                {
                    int.TryParse(addItemQuantity, out int quantity);
                    InventoryOps.AddItem(inventory, services.Definitions, addItemId,
                        Mathf.Max(1, quantity));
                    statusMessage = $"Đã thêm {addItemId}.";
                }
            }

            if (GUILayout.Button("Dùng"))
            {
                var result = services.Commands.Submit(new UseItemCommand(addItemId));
                statusMessage = result.Success ? $"Đã dùng {addItemId}." : $"Lỗi: {result.Error}";
            }
            GUILayout.EndHorizontal();
        }

        void DrawSaveControls(GameServices services)
        {
            GUILayout.Space(8f);
            GUILayout.Label("— Save —");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save manual"))
            {
                Run(() =>
                {
                    services.SaveTo(SaveService.ManualSlotId);
                    statusMessage = "Đã ghi manual_0.";
                });
            }

            if (GUILayout.Button("Load manual"))
            {
                Run(() =>
                {
                    services.LoadFrom(SaveService.ManualSlotId);
                    statusMessage = "Đã load manual_0.";
                });
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Autosave"))
            {
                Run(() => statusMessage = $"Đã ghi {services.SaveAutosave()}.");
            }
        }

        void Run(System.Action action)
        {
            try
            {
                action();
            }
            catch (SaveLoadException e)
            {
                statusMessage = $"{e.Error}: {e.Message}";
                GameLog.Warn(LogCategory.Save, e.Message);
            }
        }
    }
}
