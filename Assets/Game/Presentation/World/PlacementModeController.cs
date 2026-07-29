using LastHope.Core.Events;
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
    /// phím + OnGUI). Bật khi nghe <see cref="BeginPlacementMode"/> từ ShelterPanel: ghost
    /// theo con trỏ (xanh/đỏ theo <see cref="BuildSystem.CanPlaceAt"/>), khung mờ biên Zone,
    /// click trái xác nhận, ESC (action <c>Close</c>) huỷ.
    /// </summary>
    public class PlacementModeController : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;
        [SerializeField] PlayerFloorState playerFloorState;
        [SerializeField] Camera worldCamera;

        InputAction closeAction;
        bool active;
        string zoneId;
        string moduleId;

        GameObject ghost;
        SpriteRenderer ghostRenderer;
        GameObject zoneBoundsBox;
        BuildRejectReason lastReason;
        GUIStyle hintStyle;

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
                GameBootstrapper.Services.Events.Unsubscribe<BeginPlacementMode>(OnBeginPlacement);
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<BeginPlacementMode>(OnBeginPlacement);
        }

        void OnBeginPlacement(BeginPlacementMode e)
        {
            zoneId = e.ZoneId;
            moduleId = e.ModuleId;
            active = true;

            var definitions = GameBootstrapper.Services.Definitions;
            if (definitions.TryGetShelterZone(zoneId, out var zone))
            {
                // Đứng đúng tầng của Zone đang đặt — Placement Mode coi như bước "lên kế hoạch",
                // không bắt phải tự đi bộ lên trước.
                playerFloorState?.TeleportToFloor(zone.Floor == ShelterFloor.Upper ? 1 : 0);
                CreateZoneBoundsBox(zone);
            }

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

            var mouse = Mouse.current;
            if (worldCamera == null || mouse == null) return;

            Vector3 screenPos = mouse.position.ReadValue();
            screenPos.z = -worldCamera.transform.position.z;
            Vector3 world = worldCamera.ScreenToWorldPoint(screenPos);
            ghost.transform.position = new Vector3(world.x, world.y, 0f);

            var services = GameBootstrapper.Services;
            var reason = BuildSystem.CanPlaceAt(
                services.World, services.Definitions, zoneId, world.x, world.y, moduleId);
            lastReason = reason;
            ghostRenderer.color = reason == BuildRejectReason.None
                ? new Color(0.3f, 0.9f, 0.3f, 0.6f)
                : new Color(0.9f, 0.3f, 0.3f, 0.6f);

            if (reason == BuildRejectReason.None && mouse.leftButton.wasPressedThisFrame)
            {
                services.Commands.Submit(new StartConstructionCommand(zoneId, world.x, world.y, moduleId));
                EndPlacement();
            }
        }

        void EndPlacement()
        {
            active = false;
            if (ghost != null) Destroy(ghost);
            if (zoneBoundsBox != null) Destroy(zoneBoundsBox);
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
            GUI.Label(new Rect(x, y + 24f, width, 20f), $"{status} · ESC: Huỷ", hintStyle);
        }

        static string RejectReasonText(BuildRejectReason reason) => reason switch
        {
            BuildRejectReason.OutOfBounds => "Ngoài vùng Zone cho phép",
            BuildRejectReason.Overlapping => "Chồng lên Module khác",
            BuildRejectReason.NotEnoughMaterials => "Không đủ vật liệu",
            BuildRejectReason.ConstructionInProgress => "Đang có công trình khác thi công",
            BuildRejectReason.WrongZone => "Module không đặt được ở Zone này",
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
            ghostRenderer.sprite = WhiteSquareSprite();
            ghost.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            ghostRenderer.sortingOrder = 100;
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
