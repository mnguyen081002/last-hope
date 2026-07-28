using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.UI;
using LastHope.Data.Definitions;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Inventory;
using LastHope.Systems.Registry;
using LastHope.Systems.Shelter;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.UI.Panels
{
    /// <summary>
    /// Toàn bộ giao diện quản lý Shelter (BL-P3-02/03/04/11) — mọi Zone/Slot trong một panel
    /// (không đi bộ tới từng Zone vật lý, xem docs/plans/2026-07-28-p3-shelter-loop.md).
    /// Tự mở khi nghe <see cref="ShelterConsoleOpened"/>.
    /// </summary>
    public class ShelterPanel : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;

        InputAction closeAction;
        bool visible;
        Vector2 scroll;
        string statusMessage = "";

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
                GameBootstrapper.Services.Events.Unsubscribe<ShelterConsoleOpened>(OnOpened);
        }

        void Update()
        {
            if (visible && closeAction != null && closeAction.WasPressedThisFrame()) visible = false;
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<ShelterConsoleOpened>(OnOpened);
        }

        void OnOpened(ShelterConsoleOpened e) => visible = !visible; // toggle — tương tác lại console = đóng.

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var shelter = services.World.Shelter;
            var definitions = services.Definitions;
            var balance = definitions.Balance.Shelter;

            const float width = 560f, height = 620f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Shelter — Nhà số 17");

            DrawOverview(shelter, definitions, balance);
            DrawEvents(services, shelter);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height - 230f));
            foreach (var zone in definitions.ShelterZones.Values)
            {
                DrawZone(services, zone);
            }
            GUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(statusMessage)) GUILayout.Label(statusMessage);
            if (GUILayout.Button("Đóng")) visible = false;
            GUILayout.EndArea();
        }

        void DrawOverview(ShelterState shelter, Data.DefinitionRegistry definitions, ShelterBalance balance)
        {
            string waterLevel = ShelterWaterSystem.WaterIntrusionLevel(shelter.WaterIntrusion, balance);
            GUILayout.Label($"Structural Integrity: {shelter.StructuralIntegrity:F0}   "
                + $"Water Intrusion: {shelter.WaterIntrusion:F0} ({waterLevel})");
            GUILayout.Label($"Clean Water: {shelter.CleanWater:F1}   Untreated Water: {shelter.UntreatedWater:F1}   "
                + $"Battery: {shelter.BatteryCharge:F0}/{definitions.Balance.Power.BatteryMaxCharge:F0}");
            GUILayout.Label($"Filter Durability: {shelter.PurifierFilterDurability:F0}%");
        }

        void DrawEvents(GameServices services, ShelterState shelter)
        {
            if (shelter.DrainBackflowActive)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("⚠ Drain Backflow — nước đang chảy ngược vào Utility Area.");
                if (GUILayout.Button("Xử lý", GUILayout.Width(70f)))
                {
                    var result = services.Commands.Submit(new ResolveDrainBackflowCommand());
                    statusMessage = result.Success ? "Đã xử lý Drain Backflow." : $"Không xử lý được ({result.Error}).";
                }
                GUILayout.EndHorizontal();
            }

            if (shelter.StorageFloodRiskActive)
            {
                GUILayout.Label("⚠ Storage Flood Risk — đồ trong kho có thể bị cuốn trôi (xây Elevated Storage để bảo vệ).");
            }

            var pump = ShelterWaterSystem.FindModule(shelter, ShelterModuleIds.Pump);
            if (pump != null && pump.IsJammed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("⚠ Pump Jam — Pump đang kẹt, không bơm được nước ra.");
                if (GUILayout.Button("Sửa", GUILayout.Width(70f)))
                {
                    var result = services.Commands.Submit(new RepairPumpJamCommand());
                    statusMessage = result.Success ? "Đã sửa xong Pump." : $"Không sửa được ({result.Error}).";
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawZone(GameServices services, Data.Definitions.ShelterZoneDefinition zone)
        {
            GUILayout.Label($"— {zone.Id} ({zone.Floor}, nguy cơ ngập: {zone.WaterRisk}) —");
            foreach (string slotId in zone.BuildSlotIds)
            {
                DrawSlot(services, slotId, zone);
            }
        }

        void DrawSlot(GameServices services, string slotId, Data.Definitions.ShelterZoneDefinition zone)
        {
            var shelter = services.World.Shelter;
            var definitions = services.Definitions;

            if (shelter.BuildSlots.TryGetValue(slotId, out var built))
            {
                DrawBuiltModule(services, slotId, built);
                return;
            }

            if (shelter.Construction != null && shelter.Construction.SlotId == slotId)
            {
                var c = shelter.Construction;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"[{slotId}] Đang xây {c.ModuleId} — còn {c.MinutesRemaining:F0} phút"
                    + (c.Paused ? " (tạm dừng)" : ""), GUILayout.Width(340f));
                if (GUILayout.Button(c.Paused ? "Tiếp tục" : "Tạm dừng", GUILayout.Width(90f)))
                {
                    BuildSystem.SetPaused(services.World, slotId, !c.Paused);
                }
                if (GUILayout.Button("Huỷ", GUILayout.Width(60f)))
                {
                    services.Commands.Submit(new CancelConstructionCommand(slotId));
                }
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.Label($"[{slotId}] Trống");
            foreach (var pair in definitions.Modules)
            {
                var module = pair.Value;
                if (!module.AllowedZoneIds.Contains(zone.Id)) continue;

                string cost = string.Join(", ", ListMaterials(module));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  {module.Id} ({cost}, {module.BuildMinutes} phút)", GUILayout.Width(400f));
                if (GUILayout.Button("Xây", GUILayout.Width(60f)))
                {
                    var result = services.Commands.Submit(new StartConstructionCommand(slotId, module.Id));
                    statusMessage = result.Success ? "" : $"Không xây được ({result.Error}).";
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawBuiltModule(GameServices services, string slotId, BuiltModuleState built)
        {
            GUILayout.BeginHorizontal();
            string jam = built.IsJammed ? " [KẸT]" : "";
            GUILayout.Label(
                $"[{slotId}] {built.ModuleId} — bền {built.Durability:F0}%, "
                + $"{(built.Powered ? "có điện" : "mất điện")}{jam}",
                GUILayout.Width(300f));

            if (GUILayout.Button(built.Priority.ToString(), GUILayout.Width(80f)))
            {
                var next = (PowerPriority)(((int)built.Priority + 1) % 4);
                services.Commands.Submit(new SetPowerPriorityCommand(slotId, next));
            }
            if (GUILayout.Button("Tháo", GUILayout.Width(60f)))
            {
                services.Commands.Submit(new DismantleModuleCommand(slotId));
            }
            GUILayout.EndHorizontal();
        }

        static IEnumerable<string> ListMaterials(Data.Definitions.ModuleDefinition module)
        {
            foreach (var pair in module.Materials) yield return $"{pair.Key}×{pair.Value}";
        }
    }
}
