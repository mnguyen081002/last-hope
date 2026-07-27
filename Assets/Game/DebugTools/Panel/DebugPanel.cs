using LastHope.Core.Commands;
using LastHope.Core.Diagnostics;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Core.UI;
using LastHope.Systems.Boot;
using LastHope.Systems.Condition;
using LastHope.Systems.Registry;
using LastHope.Systems.Hazard;
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

            // Chiều cao co theo Screen.height — cửa sổ Game view nhỏ (Play trong Editor) vẫn
            // cuộn được tới hết nội dung, không bị cắt cứng ở 760 và không kéo được xuống.
            float height = Mathf.Min(760f, Screen.height - 20f);
            var rect = new Rect(Screen.width - PanelWidth - 10f, 10f, PanelWidth, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label($"<b>Debug Panel</b> (F2)");
            GUILayout.Label(GameTimeUtil.Format(world.WorldTimeMinutes) +
                            $"  ·  phút {world.WorldTimeMinutes}");
            GUILayout.Label($"Location: {world.Player.CurrentLocationId}");

            DrawTimeControls(services);
            DrawConditionControls(services);
            DrawHazardControls(services);
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

        void DrawConditionControls(GameServices services)
        {
            GUILayout.Space(8f);
            GUILayout.Label("— Condition —");

            var player = services.World.Player;
            var balance = services.Definitions.Balance.Condition;

            GUILayout.Label($"HP {player.Health:F0}  Stamina {player.Stamina:F0}  Fatigue {player.Fatigue:F0}");
            GUILayout.Label($"Hunger {player.Hunger:F0}  Thirst {player.Thirst:F0}  BodyTemp {player.BodyTemperature:F1}°C");
            GUILayout.Label($"Wet {player.Wet:F0}  Exposure {player.BlackWaterExposure:F0}" +
                            $"  Cold:{player.IsCold}  Sick:{player.IsSick}" +
                            $"  Collapsed:{ConditionSystem.IsCollapsed(player, balance)}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+20 Thirst")) player.Thirst = Mathf.Min(100f, player.Thirst + 20f);
            if (GUILayout.Button("+20 Hunger")) player.Hunger = Mathf.Min(100f, player.Hunger + 20f);
            if (GUILayout.Button("+50 Wet")) player.Wet = Mathf.Min(100f, player.Wet + 50f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-20 HP")) player.Health = Mathf.Max(0f, player.Health - 20f);
            if (GUILayout.Button("+50 Exposure")) player.BlackWaterExposure = Mathf.Min(100f, player.BlackWaterExposure + 50f);
            if (GUILayout.Button("Reset")) ResetCondition(player);
            GUILayout.EndHorizontal();
        }

        static void ResetCondition(PlayerState player)
        {
            player.Health = 100f;
            player.Stamina = 100f;
            player.Fatigue = 0f;
            player.Hunger = 0f;
            player.Thirst = 0f;
            player.BodyTemperature = 37f;
            player.Wet = 0f;
            player.BlackWaterExposure = 0f;
            player.IsCold = false;
            player.IsSick = false;
        }

        const string TestRouteId = "route_shelter_store";

        void DrawHazardControls(GameServices services)
        {
            GUILayout.Space(8f);
            GUILayout.Label("— Hazard —");

            var phase = DisasterPhaseSystem.CurrentPhase(
                services.World.WorldTimeMinutes, services.Definitions.Balance.DisasterPhase);
            GUILayout.Label($"Disaster Phase: {phase} (raining: {DisasterPhaseSystem.IsRaining(phase)})");

            var routeState = services.World.GetOrCreateRoute(TestRouteId);
            GUILayout.Label($"{TestRouteId}: {routeState.Flood}" +
                            (HazardSystem.IsPassable(routeState.Flood) ? "" : " (chặn)") +
                            $"  Current:{routeState.Current}  Electrified:{routeState.IsElectrified}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dry")) routeState.Flood = FloodState.Dry;
            if (GUILayout.Button("Shallow")) routeState.Flood = FloodState.Shallow;
            if (GUILayout.Button("Medium")) routeState.Flood = FloodState.Medium;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Deep")) routeState.Flood = FloodState.Deep;
            if (GUILayout.Button("Impassable")) routeState.Flood = FloodState.Impassable;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Current None")) routeState.Current = CurrentStrength.None;
            if (GUILayout.Button("Current Strong")) routeState.Current = CurrentStrength.Strong;
            if (GUILayout.Button("Current Extreme")) routeState.Current = CurrentStrength.Extreme;
            GUILayout.EndHorizontal();

            if (GUILayout.Button(routeState.IsElectrified ? "Tắt Electrified" : "Bật Electrified"))
                routeState.IsElectrified = !routeState.IsElectrified;
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
