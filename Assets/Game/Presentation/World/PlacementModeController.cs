using LastHope.Core.Events;
using LastHope.Data;
using LastHope.Data.Definitions;
using LastHope.Presentation.Player;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Shelter;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Free Placement (BL-P3-03) — tương tác chuột đầu tiên trong game (mọi thứ khác dùng
    /// phím + OnGUI). Bật khi nghe <see cref="BeginPlacementMode"/> từ InventoryPanel (bấm "Đặt"
    /// cạnh 1 packed item): ghost theo con trỏ (xanh/đỏ theo <see cref="BuildSystem.CanRedeployAt"/>),
    /// khung mờ biên Zone, click trái xác nhận (<c>RedeployModuleCommand</c> — tức thì, không
    /// chờ), ESC (action <c>Close</c>) huỷ. Đổi 2026-07-30: không còn nhận ZoneId cố định —
    /// Zone tự resolve mỗi frame từ <see cref="ModuleDefinition.AllowedZoneIds"/> lọc theo tầng
    /// đang đứng (<see cref="PlayerFloorState.CurrentFloor"/>) + vị trí chuột, vì Production
    /// không còn chọn Zone trước nữa (xem docs/plans/2026-07-30-module-production-placement-loop.md).
    /// Không còn auto-teleport tầng lúc mở — Module có nhiều Zone khả dĩ ở nhiều tầng
    /// (vd module_elevated_storage) thì người chơi tự đi cầu thang để đổi tầng trong lúc ghost
    /// đang mở.
    /// </summary>
    public class PlacementModeController : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;
        [SerializeField] PlayerFloorState playerFloorState;
        [SerializeField] Camera worldCamera;

        InputAction closeAction;
        InputAction rotateAction;
        bool active;
        public bool Active => active;
        string moduleId;
        ModuleDefinition module;
        bool canRotate;
        int rotationQuarterTurns;

        GameObject ghost;
        SpriteRenderer ghostRenderer;
        GameObject footprintOutline;
        SpriteRenderer footprintRenderer;
        GameObject zoneBoundsBox;
        string boundsZoneId;
        BuildRejectReason lastReason;
        GUIStyle hintStyle;

        void Awake()
        {
            if (controls == null) return;
            var gameplay = controls.FindActionMap("Gameplay", true);
            closeAction = gameplay.FindAction("Close", true);
            rotateAction = gameplay.FindAction("RotateModule", false);
        }

        void OnEnable()
        {
            closeAction?.Enable();
            rotateAction?.Enable();
            if (GameBootstrapper.IsReady) Subscribe();
            else GameBootstrapper.Ready += Subscribe;
        }

        void OnDisable()
        {
            closeAction?.Disable();
            rotateAction?.Disable();
            GameBootstrapper.Ready -= Subscribe;
            if (GameBootstrapper.IsReady)
                GameBootstrapper.Services.Events.Unsubscribe<BeginPlacementMode>(OnBeginPlacement);
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<BeginPlacementMode>(OnBeginPlacement);
        }

        void OnBeginPlacement(BeginPlacementMode e)
        {
            moduleId = e.ModuleId;
            rotationQuarterTurns = 0;
            active = true;

            var definitions = GameBootstrapper.Services.Definitions;
            definitions.TryGetModule(moduleId, out module);
            canRotate = module != null && module.IsRotatable && ModuleSpriteCatalog.HasAllDirections(moduleId);

            CreateGhost();
        }

        void Update()
        {
            if (!active) return;

            if (closeAction != null && closeAction.WasPressedThisFrame())
            {
                EndPlacement();
                return;
            }

            if (canRotate && rotateAction != null && rotateAction.WasPressedThisFrame())
            {
                rotationQuarterTurns = (rotationQuarterTurns + 1) % 4;
                UpdateGhostSprite();
            }

            var mouse = Mouse.current;
            if (worldCamera == null || mouse == null || module == null) return;

            Vector3 screenPos = mouse.position.ReadValue();
            screenPos.z = -worldCamera.transform.position.z;
            Vector3 world = worldCamera.ScreenToWorldPoint(screenPos);
            ghost.transform.position = new Vector3(world.x, world.y, 0f);

            BuildSystem.GetFootprint(module, rotationQuarterTurns, out float footprintWidth, out float footprintHeight);
            footprintOutline.transform.position = new Vector3(world.x, world.y, 0f);
            footprintRenderer.size = new Vector2(footprintWidth, footprintHeight);

            var services = GameBootstrapper.Services;
            var definitions = services.Definitions;
            int currentFloor = playerFloorState != null ? playerFloorState.CurrentFloor : 0;
            string resolvedZoneId = ResolveZoneId(definitions, module, currentFloor, world.x, world.y);
            UpdateZoneBoundsBox(definitions, resolvedZoneId);

            var reason = resolvedZoneId == null
                ? BuildRejectReason.OutOfBounds
                : BuildSystem.CanRedeployAt(
                    services.World, definitions, resolvedZoneId, world.x, world.y, moduleId,
                    rotationQuarterTurns);
            lastReason = reason;
            var tint = reason == BuildRejectReason.None
                ? new Color(0.3f, 0.9f, 0.3f, 0.6f)
                : new Color(0.9f, 0.3f, 0.3f, 0.6f);
            ghostRenderer.color = tint;
            footprintRenderer.color = new Color(tint.r, tint.g, tint.b, 0.35f);

            if (reason == BuildRejectReason.None && mouse.leftButton.wasPressedThisFrame)
            {
                services.Commands.Submit(new RedeployModuleCommand(
                    resolvedZoneId, world.x, world.y, moduleId, rotationQuarterTurns));
                EndPlacement();
            }
        }

        /// <summary>Zone không còn chọn trước — lọc <see cref="ModuleDefinition.AllowedZoneIds"/>
        /// theo tầng đang đứng rồi theo vị trí chuột. Đa số Module chỉ có 1 Zone khả dĩ (transparent);
        /// module_elevated_storage có 2 (Ground/Upper) — người chơi tự đi đúng tầng để resolve.</summary>
        static string ResolveZoneId(
            DefinitionRegistry definitions, ModuleDefinition module, int currentFloor, float x, float y)
        {
            foreach (var zoneId in module.AllowedZoneIds)
            {
                if (definitions.TryGetShelterZone(zoneId, out var zone)
                    && (int)zone.Floor == currentFloor && zone.Contains(x, y))
                    return zoneId;
            }
            return null;
        }

        void EndPlacement()
        {
            active = false;
            if (ghost != null) Destroy(ghost);
            if (footprintOutline != null) Destroy(footprintOutline);
            if (zoneBoundsBox != null) Destroy(zoneBoundsBox);
            zoneBoundsBox = null;
            boundsZoneId = null;
        }

        void OnGUI()
        {
            if (!active) return;

            hintStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };

            string status = lastReason == BuildRejectReason.None
                ? "Click trái để đặt"
                : RejectReasonText(lastReason);

            const float width = 360f;
            const float height = 50f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 90f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.Label(new Rect(x, y + 4f, width, 20f), $"Đặt {moduleId}", hintStyle);
            string rotateHint = canRotate ? $" · R: Xoay ({rotationQuarterTurns * 90}°)" : "";
            GUI.Label(new Rect(x, y + 24f, width, 20f), $"{status}{rotateHint} · ESC: Huỷ", hintStyle);
        }

        static string RejectReasonText(BuildRejectReason reason) => reason switch
        {
            BuildRejectReason.OutOfBounds => "Ngoài vùng Zone cho phép",
            BuildRejectReason.Overlapping => "Chồng lên Module khác",
            BuildRejectReason.NotEnoughPackedModules => "Không có Module đã gói sẵn trong túi",
            BuildRejectReason.WrongZone => "Module không đặt được ở Zone này",
            BuildRejectReason.RotationNotAllowed => "Module này không hỗ trợ xoay",
            _ => "Không thể đặt ở đây",
        };

        /// <summary>Sprite trắng 1x1 dựng bằng Texture2D.whiteTexture — an toàn ở runtime build
        /// (khác AssetDatabase/Resources.Load, không tồn tại ngoài Editor hoặc cần đặt sẵn
        /// trong thư mục Resources).</summary>
        static Sprite WhiteSquareSprite() =>
            Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);

        void CreateGhost()
        {
            ghost = new GameObject("PlacementGhost");
            ghostRenderer = ghost.AddComponent<SpriteRenderer>();
            ghostRenderer.sortingOrder = 100;
            UpdateGhostSprite();

            // Khung diện tích chiếm — tách khỏi sprite Module (art không phải lúc nào cũng phủ
            // đúng hết footprint hình chữ nhật dùng để validate overlap), đổi kích thước theo
            // đúng BuildSystem.GetFootprint mỗi khi rotate.
            footprintOutline = new GameObject("PlacementFootprint");
            footprintRenderer = footprintOutline.AddComponent<SpriteRenderer>();
            footprintRenderer.drawMode = SpriteDrawMode.Sliced;
            footprintRenderer.sprite = WhiteSquareSprite();
            footprintRenderer.sortingOrder = 90;
        }

        void UpdateGhostSprite()
        {
            var sprite = ModuleSpriteCatalog.Load(moduleId, rotationQuarterTurns);
            bool hasProductionArt = sprite != null;
            ghostRenderer.sprite = hasProductionArt ? sprite : WhiteSquareSprite();
            ghost.transform.localScale = hasProductionArt
                ? Vector3.one
                : new Vector3(0.6f, 0.6f, 1f);
        }

        void UpdateZoneBoundsBox(DefinitionRegistry definitions, string resolvedZoneId)
        {
            if (resolvedZoneId == boundsZoneId) return;

            if (zoneBoundsBox != null)
            {
                Destroy(zoneBoundsBox);
                zoneBoundsBox = null;
            }
            boundsZoneId = resolvedZoneId;

            if (resolvedZoneId != null && definitions.TryGetShelterZone(resolvedZoneId, out var zone))
                CreateZoneBoundsBox(zone);
        }

        void CreateZoneBoundsBox(ShelterZoneDefinition zone)
        {
            zoneBoundsBox = new GameObject("ZoneBounds");
            var renderer = zoneBoundsBox.AddComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 0.12f);
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.sprite = WhiteSquareSprite();
            renderer.size = new Vector2(zone.BoundsMaxX - zone.BoundsMinX, zone.BoundsMaxY - zone.BoundsMinY);
            renderer.sortingOrder = -50;
            zoneBoundsBox.transform.position = new Vector3(
                (zone.BoundsMinX + zone.BoundsMaxX) / 2f, (zone.BoundsMinY + zone.BoundsMaxY) / 2f, 0f);
        }
    }
}
