using LastHope.Core.State;
using LastHope.Core.UI;
using LastHope.Data.Definitions;
using LastHope.Presentation.CameraControl;
using LastHope.Presentation.Player;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Rê chuột dừng trên sprite Module đã xây (<see cref="PlacedModuleRenderer"/>) hiện menu
    /// nhỏ cạnh con trỏ với nút "Tháo" — thay cho nút "Tháo" cũ trong <c>ShelterPanel</c> (user
    /// yêu cầu 2026-07-29, đổi sang pattern quen thuộc kiểu Project Zomboid/RimWorld). Paradigm
    /// tương tác thứ ba trong game — khác <c>IInteractable</c> (bám player, phím E) và khác
    /// click-đặt của <see cref="PlacementModeController"/>.
    /// </summary>
    public class PlacedModuleHoverMenu : MonoBehaviour
    {
        const float ProximityRadius = 1.6f; // giống InteractionDetector.detectionRadius mặc định.

        Camera worldCamera;
        PlayerFloorState playerFloorState;
        PlacementModeController placementMode;

        string hoveredPlacementId;
        string hoveredModuleId;

        // Rect vẽ menu lần OnGUI gần nhất (GUI-space) — dùng để giữ menu mở khi chuột đã rời
        // vùng world-hover (bán kính nhỏ quanh Module) nhưng còn nằm trong chính cái menu, vd
        // đang di chuột từ Module sang nút "Tháo". Không neo menu theo vị trí chuột — neo theo
        // vị trí world CỦA MODULE (xem OnGUI) nên đứng yên, không "chạy theo" chuột.
        Rect? menuScreenRect;

        void OnEnable()
        {
            worldCamera = FindFirstObjectByType<CameraRig>()?.GetComponent<Camera>();
            playerFloorState = FindFirstObjectByType<PlayerFloorState>();
            placementMode = FindFirstObjectByType<PlacementModeController>();
        }

        void Update()
        {
            bool placementActive = placementMode != null && placementMode.Active;
            var mouse = Mouse.current;

            if (placementActive || worldCamera == null || playerFloorState == null
                || mouse == null || !GameBootstrapper.IsReady)
            {
                hoveredPlacementId = null;
                menuScreenRect = null;
                return;
            }

            Vector2 rawScreenPos = mouse.position.ReadValue();
            var guiMouse = new Vector2(rawScreenPos.x, Screen.height - rawScreenPos.y);

            Vector3 screenPos = rawScreenPos;
            screenPos.z = -worldCamera.transform.position.z;
            Vector3 world = worldCamera.ScreenToWorldPoint(screenPos);

            var services = GameBootstrapper.Services;
            var definitions = services.Definitions;
            Vector3 playerPos = playerFloorState.transform.position;

            string worldHoverId = null;
            string worldHoverModuleId = null;

            foreach (var pair in services.World.Shelter.PlacedModules)
            {
                var built = pair.Value;
                if (!definitions.TryGetShelterZone(built.ZoneId, out var zone)) continue;
                int floor = zone.Floor == ShelterFloor.Upper ? 1 : 0;
                if (floor != playerFloorState.CurrentFloor) continue;

                if (!definitions.TryGetModule(built.ModuleId, out var module)) continue;

                float dx = built.PositionX - world.x, dy = built.PositionY - world.y;
                if (dx * dx + dy * dy > module.FootprintRadius * module.FootprintRadius) continue;

                float pdx = built.PositionX - playerPos.x, pdy = built.PositionY - playerPos.y;
                if (pdx * pdx + pdy * pdy > ProximityRadius * ProximityRadius) continue;

                worldHoverId = pair.Key;
                worldHoverModuleId = built.ModuleId;
                break;
            }

            if (worldHoverId != null)
            {
                hoveredPlacementId = worldHoverId;
                hoveredModuleId = worldHoverModuleId;
            }
            else if (hoveredPlacementId != null && menuScreenRect.HasValue && menuScreenRect.Value.Contains(guiMouse))
            {
                // Chuột đã rời Module nhưng còn trong menu đang hiện — giữ nguyên, không đóng.
            }
            else
            {
                hoveredPlacementId = null;
                menuScreenRect = null;
            }
        }

        void OnGUI()
        {
            if (hoveredPlacementId == null) return;

            if (!GameBootstrapper.Services.World.Shelter.PlacedModules.TryGetValue(hoveredPlacementId, out var built))
            {
                // Vừa bị tháo/đặt lại ở nơi khác trong cùng frame — đóng menu, tránh tra cứu hỏng.
                hoveredPlacementId = null;
                menuScreenRect = null;
                return;
            }

            Vector3 screenPoint = worldCamera.WorldToScreenPoint(new Vector3(built.PositionX, built.PositionY, 0f));
            var anchor = new Vector2(screenPoint.x, Screen.height - screenPoint.y);

            const float width = 140f, height = 54f;
            var rect = new Rect(anchor.x + 12f, anchor.y - height - 12f, width, height);
            menuScreenRect = rect;
            PointerOverUI.MarkHover(true);

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(hoveredModuleId);
            if (GUILayout.Button("Tháo"))
            {
                GameBootstrapper.Services.Commands.Submit(new DismantleModuleCommand(hoveredPlacementId));
                hoveredPlacementId = null;
                menuScreenRect = null;
            }
            GUILayout.EndArea();
        }
    }
}
