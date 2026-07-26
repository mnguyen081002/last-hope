using System.IO;
using LastHope.Core.Text;
using LastHope.Presentation.Boot;
using LastHope.Presentation.CameraRig;
using LastHope.Presentation.Interaction;
using LastHope.Presentation.Player;
using LastHope.Presentation.World;
using LastHope.DebugTools.Overlay;
using LastHope.DebugTools.Panel;
using LastHope.Systems.Boot;
using LastHope.UI.Container;
using LastHope.UI.Events;
using LastHope.UI.Hud;
using LastHope.UI.Inventory;
using LastHope.UI.Map;
using LastHope.UI.Outcome;
using LastHope.UI.Shelter;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Builds the project's core scenes (00_Boot, 10_GamePersistent, 90_TestSystems) from scratch
    /// so they can be regenerated deterministically. Player/Camera/HUD live in 10_GamePersistent
    /// (persistent avatar — prerequisite for Sprint 6 scene switching); gameplay scenes contain
    /// only environment/interactables/spawn markers.
    ///
    /// 2026-07-25: rebuilt for 2D isometric (3D->2D migration). World positions carried over from
    /// the old 3D layout as (x, oldZ) — old Z (world depth) becomes the 2D Y axis; old Y (physical
    /// elevation, cosmetic-only in 3D) is dropped entirely, top-down 2D has no vertical axis.
    /// Ground/prop meshes are replaced by SpriteRenderer + Collider2D using solid-color sprite
    /// assets generated once into Assets/Game/Generated (same "blockout by color" spirit as the
    /// old primitives — not blocked on real art). Colliders are added only where the 3D collider
    /// actually blocked the player in practice (furniture-sized props, structural CoreComponents);
    /// Zone/BuildSlot markers and the stairs marker stay collider-less on purpose — in 3D their
    /// thin collider sat below the CharacterController capsule and never blocked movement, but a
    /// full-footprint Collider2D in top-down 2D would, so carrying that collider over verbatim
    /// would newly block walking across zones/slots that must stay walkable.
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string BootScenePath = ScenesFolder + "/00_Boot.unity";
        private const string PersistentScenePath = ScenesFolder + "/10_GamePersistent.unity";
        private const string TestSystemsScenePath = ScenesFolder + "/90_TestSystems.unity";
        private const string MainShelterScenePath = ScenesFolder + "/Shelters/20_MainShelter.unity";
        private const string ConvenienceStoreScenePath = ScenesFolder + "/Locations/41_Location_ConvenienceStore.unity";
        private const string UtilityGarageScenePath = ScenesFolder + "/Locations/42_Location_UtilityGarage.unity";
        private const string SchoolScenePath = ScenesFolder + "/Locations/43_Location_School.unity";
        private const string InputActionsPath = "Assets/Input/GameControls.inputactions";

        [MenuItem("Last Hope/Build Sprint 1 Scenes")]
        public static void BuildAll()
        {
            // Delete old scene files to avoid conflicts (3D→2D migration, etc.)
            DeleteIfExists(BootScenePath);
            DeleteIfExists(PersistentScenePath);
            DeleteIfExists(TestSystemsScenePath);
            DeleteIfExists(MainShelterScenePath);
            DeleteIfExists(ConvenienceStoreScenePath);
            DeleteIfExists(UtilityGarageScenePath);
            DeleteIfExists(SchoolScenePath);

            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError($"[SceneSetup] Could not load InputActionAsset at {InputActionsPath}");
                return;
            }

            BuildTestSystemsScene();
            BuildGamePersistentScene(inputActions);
            BuildMainShelterScene();
            BuildConvenienceStoreScene();
            BuildUtilityGarageScene();
            BuildSchoolScene();
            BuildBootScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(PersistentScenePath, true),
                new EditorBuildSettingsScene(TestSystemsScenePath, true),
                new EditorBuildSettingsScene(MainShelterScenePath, true),
                new EditorBuildSettingsScene(ConvenienceStoreScenePath, true),
                new EditorBuildSettingsScene(UtilityGarageScenePath, true),
                new EditorBuildSettingsScene(SchoolScenePath, true),
            };

            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Scenes built and registered in Build Settings.");
        }

        // ---------------------------------------------------------------
        // Generated placeholder art (solid-color sprites/tiles, persisted
        // as real asset files so Tilemap/SpriteRenderer references survive
        // scene save/reload — a runtime-only Sprite or Tile does not).
        // ---------------------------------------------------------------

        private const string GeneratedSpritesFolder = "Assets/Game/Generated/Sprites";
        private const string GeneratedTilesFolder = "Assets/Game/Generated/Tiles";

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static Sprite GetOrCreateSolidSprite(string name, Color color)
        {
            EnsureFolder(GeneratedSpritesFolder);
            string pngPath = $"{GeneratedSpritesFolder}/{name}.png";

            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (existing != null) return existing;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            Color32 c32 = color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(pngPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        }

        private static Tile GetOrCreateSolidTile(string name, Color color)
        {
            EnsureFolder(GeneratedTilesFolder);
            string assetPath = $"{GeneratedTilesFolder}/{name}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
            if (existing != null) return existing;

            var sprite = GetOrCreateSolidSprite("tile_" + name, color);
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white; // tint already baked into the sprite, keep tile neutral
            AssetDatabase.CreateAsset(tile, assetPath);
            return tile;
        }

        /// <summary>Marker/prop GameObject: SpriteRenderer sized via transform.localScale (same
        /// role localScale played on the old primitives) + optional BoxCollider2D.</summary>
        private static GameObject CreateSpriteMarker(string name, Vector2 position, Vector2 scale, Color color, bool solid)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetOrCreateSolidSprite("marker_" + ColorUtility.ToHtmlStringRGB(color), color);

            if (solid) go.AddComponent<BoxCollider2D>();

            return go;
        }

        private static void CreateGroundTilemap(string cellName, Color color, int minX, int maxX, int minY, int maxY)
        {
            var gridGo = new GameObject("Ground_Grid");
            var grid = gridGo.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f);

            var tilemapGo = new GameObject("Ground");
            tilemapGo.transform.SetParent(gridGo.transform, false);
            var tilemap = tilemapGo.AddComponent<Tilemap>();
            tilemapGo.AddComponent<TilemapRenderer>();

            var tile = GetOrCreateSolidTile("ground_" + cellName, color);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        // ---------------------------------------------------------------

        private static void BuildTestSystemsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGroundTilemap("test", new Color(0.5f, 0.5f, 0.5f), -10, 10, -10, 10);

            CreateScaleMarker("ScaleRef_1m", 1f, -3f);
            CreateScaleMarker("ScaleRef_2m", 2f, 0f);
            CreateScaleMarker("ScaleRef_3m", 3f, 4f);

            EditorSceneManager.SaveScene(scene, TestSystemsScenePath);
        }

        private static void CreateScaleMarker(string name, float size, float xPosition)
        {
            CreateSpriteMarker(name, new Vector2(xPosition, 4f), new Vector2(size, size),
                new Color(0.8f, 0.8f, 0.8f), solid: false);
        }

        private static void BuildGamePersistentScene(InputActionAsset inputActions)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("GamePersistent");
            root.AddComponent<GamePersistentMarker>();
            root.AddComponent<GameBootstrapper>();
            root.AddComponent<SimulationDriver>();
            root.AddComponent<SceneFlowController>();
            root.AddComponent<DebugOverlay>();
            root.AddComponent<DebugPanel>();

            // Player (persistent avatar; survives gameplay scene switches from Sprint 6 onward).
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, -6f, 0f);

            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            var footCollider = player.AddComponent<CircleCollider2D>();
            footCollider.radius = 0.3f;

            var playerController = player.AddComponent<PlayerController>();
            playerController.SetInputActions(inputActions);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            var visualRenderer = visual.AddComponent<SpriteRenderer>();
            var playerColor = new Color(0.2f, 0.5f, 0.9f);
            visualRenderer.sprite = GetOrCreateSolidSprite("marker_" + ColorUtility.ToHtmlStringRGB(playerColor), playerColor);
            visual.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            player.AddComponent<PlayerAvatarSync>();
            var detector = player.AddComponent<InteractionDetector>();
            detector.SetInputActions(inputActions);

            // Camera
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            cameraGo.AddComponent<AudioListener>();

            var rig = cameraGo.AddComponent<CameraRig>();
            rig.SetInputActions(inputActions);
            rig.SetTarget(player.transform);
            // Matches CameraRig's own follow offset so there's no visible snap on the first frame
            // — CameraRig recomputes and takes over regardless.
            cameraGo.transform.position = player.transform.position + new Vector3(0f, 0f, -10f);

            BuildHudCanvas(inputActions, detector);

            EditorSceneManager.SaveScene(scene, PersistentScenePath);
        }

        private static void BuildHudCanvas(InputActionAsset inputActions, InteractionDetector detector)
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // y=-100 (not -16): DebugOverlay (F1, OnGUI, always on by default) occupies
            // Rect(10,10,260,90) in screen pixels — a plain UGUI anchor at the same corner would
            // render underneath/through it. Placed below instead of relying on players disabling F1.
            var hudGo = new GameObject("ConditionHud", typeof(RectTransform));
            hudGo.transform.SetParent(canvasGo.transform, false);
            var hudRect = hudGo.GetComponent<RectTransform>();
            hudRect.anchorMin = hudRect.anchorMax = new Vector2(0f, 1f);
            hudRect.pivot = new Vector2(0f, 1f);
            hudRect.anchoredPosition = new Vector2(16f, -100f);
            hudRect.sizeDelta = new Vector2(260, 160);
            hudGo.AddComponent<ConditionHud>();

            var legendGo = new GameObject("ControlsLegend", typeof(RectTransform));
            legendGo.transform.SetParent(canvasGo.transform, false);
            var legendRect = legendGo.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 0f);
            legendRect.anchorMax = new Vector2(1f, 0f);
            legendRect.pivot = new Vector2(0.5f, 0f);
            legendRect.anchoredPosition = new Vector2(0f, 0f);
            legendRect.sizeDelta = new Vector2(0f, 32f);
            legendGo.AddComponent<ControlsLegend>();

            var promptGo = new GameObject("InteractionPrompt", typeof(RectTransform));
            promptGo.transform.SetParent(canvasGo.transform, false);
            var promptRect = promptGo.GetComponent<RectTransform>();
            promptRect.anchorMin = promptRect.anchorMax = new Vector2(0.5f, 0.3f);
            promptRect.sizeDelta = new Vector2(500, 40);
            var promptText = promptGo.AddComponent<TextMeshProUGUI>();
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.fontSize = 24;
            promptGo.AddComponent<InteractionPrompt>().SetDetector(detector);

            var panelGo = new GameObject("InventoryPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 0.5f);
            panelRect.sizeDelta = new Vector2(420, 0);
            panelRect.anchoredPosition = Vector2.zero;
            panelGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            panelGo.AddComponent<InventoryPanel>().SetInputActions(inputActions);

            var containerGo = new GameObject("ContainerPanel", typeof(RectTransform));
            containerGo.transform.SetParent(canvasGo.transform, false);
            var containerRect = containerGo.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1f, 0f);
            containerRect.anchorMax = new Vector2(1f, 1f);
            containerRect.pivot = new Vector2(1f, 0.5f);
            containerRect.sizeDelta = new Vector2(420, 0);
            containerRect.anchoredPosition = Vector2.zero;
            containerGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            containerGo.AddComponent<ContainerPanel>().SetInputActions(inputActions);

            var mapGo = new GameObject("WorldMapPanel", typeof(RectTransform));
            mapGo.transform.SetParent(canvasGo.transform, false);
            var mapRect = mapGo.GetComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0.2f, 0.2f);
            mapRect.anchorMax = new Vector2(0.8f, 0.8f);
            mapRect.offsetMin = Vector2.zero;
            mapRect.offsetMax = Vector2.zero;
            mapGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            mapGo.AddComponent<WorldMapPanel>().SetInputActions(inputActions);

            var buildGo = new GameObject("BuildPanel", typeof(RectTransform));
            buildGo.transform.SetParent(canvasGo.transform, false);
            var buildRect = buildGo.GetComponent<RectTransform>();
            buildRect.anchorMin = new Vector2(0.15f, 0.15f);
            buildRect.anchorMax = new Vector2(0.85f, 0.85f);
            buildRect.offsetMin = Vector2.zero;
            buildRect.offsetMax = Vector2.zero;
            buildGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            buildGo.AddComponent<BuildPanel>().SetInputActions(inputActions);

            var shelterGo = new GameObject("ShelterPanel", typeof(RectTransform));
            shelterGo.transform.SetParent(canvasGo.transform, false);
            var shelterRect = shelterGo.GetComponent<RectTransform>();
            shelterRect.anchorMin = new Vector2(0.15f, 0.15f);
            shelterRect.anchorMax = new Vector2(0.85f, 0.85f);
            shelterRect.offsetMin = Vector2.zero;
            shelterRect.offsetMax = Vector2.zero;
            shelterGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            shelterGo.AddComponent<ShelterPanel>().SetInputActions(inputActions);

            var eventsGo = new GameObject("EventPanel", typeof(RectTransform));
            eventsGo.transform.SetParent(canvasGo.transform, false);
            var eventsRect = eventsGo.GetComponent<RectTransform>();
            eventsRect.anchorMin = new Vector2(0.15f, 0.15f);
            eventsRect.anchorMax = new Vector2(0.85f, 0.85f);
            eventsRect.offsetMin = Vector2.zero;
            eventsRect.offsetMax = Vector2.zero;
            eventsGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            eventsGo.AddComponent<EventPanel>().SetInputActions(inputActions);

            var toastGo = new GameObject("EventToast", typeof(RectTransform));
            toastGo.transform.SetParent(canvasGo.transform, false);
            var toastRect = toastGo.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 1f);
            toastRect.anchorMax = new Vector2(0.5f, 1f);
            toastRect.pivot = new Vector2(0.5f, 1f);
            toastRect.sizeDelta = new Vector2(600f, 44f);
            toastRect.anchoredPosition = new Vector2(0f, -56f);
            toastGo.AddComponent<EventToast>();

            var outcomeGo = new GameObject("OutcomeReportPanel", typeof(RectTransform));
            outcomeGo.transform.SetParent(canvasGo.transform, false);
            var outcomeRect = outcomeGo.GetComponent<RectTransform>();
            outcomeRect.anchorMin = new Vector2(0.1f, 0.1f);
            outcomeRect.anchorMax = new Vector2(0.9f, 0.9f);
            outcomeRect.offsetMin = Vector2.zero;
            outcomeRect.offsetMax = Vector2.zero;
            outcomeGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
            outcomeGo.AddComponent<OutcomeReportPanel>().SetInputActions(inputActions);
        }

        private static void BuildMainShelterScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ground floor + upper floor land in visually separate regions of the same 2D map
            // (old Z=-10 for Upper vs Z range -6..6 for ground rooms carries straight over to a Y
            // split here) — no floor-hide system needed, exactly like a Project Zomboid building
            // blueprint shows both floors as distinct connected areas.
            CreateGroundTilemap("shelter", new Color(0.55f, 0.55f, 0.5f), -9, 9, -15, 15);

            var storage = CreateSpriteMarker("ShelterStorage", new Vector2(3f, 3f), Vector2.one,
                new Color(0.6f, 0.4f, 0.2f), solid: true);
            storage.AddComponent<ShelterStorageView>().SetShelterId("shelter_main");

            var travelPoint = CreateSpriteMarker("TravelPoint_Store", new Vector2(-3f, 3f), Vector2.one,
                new Color(0.9f, 0.7f, 0.1f), solid: true);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, 0f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            // S10 blockout: 6 ground-floor/upper zones + Fixed Core Component anchors
            // (main-shelter-design.md §6-14). Roof is co-located on the Upper area (single
            // zone/1 slot — a third physical elevation isn't worth a second connector for a blockout).
            CreateZoneMarker("shelter_entrance", new Vector2(-6f, -6f));
            CreateBuildSlots(new Vector2(-6f, -6f), "slot_shelter_entrance_1", "slot_shelter_entrance_2");

            CreateZoneMarker("central_hall", new Vector2(0f, -4f));
            CreateCoreComponent("main_staircase", new Vector2(1f, -4f));

            CreateZoneMarker("ground_storage", new Vector2(-6f, 3f));
            CreateBuildSlots(new Vector2(-6f, 3f), "slot_ground_storage_1", "slot_ground_storage_2");

            CreateZoneMarker("utility_area", new Vector2(6f, -4f));
            CreateBuildSlots(new Vector2(6f, -4f), "slot_utility_area_1", "slot_utility_area_2");
            CreateCoreComponent("structural_pillars", new Vector2(6f, -6f));
            CreateCoreComponent("electrical_backbone", new Vector2(7f, -4f));
            var drainCore = CreateSpriteMarker("DrainCore", new Vector2(5f, -3f), Vector2.one,
                new Color(0.2f, 0.6f, 0.8f), solid: true);
            drainCore.AddComponent<DrainCoreView>();

            CreateZoneMarker("water_processing", new Vector2(6f, 2f));
            CreateBuildSlots(new Vector2(6f, 2f), "slot_water_processing_1", "slot_water_processing_2");
            CreateCoreComponent("water_intake", new Vector2(7f, 2f));

            CreateZoneMarker("workshop", new Vector2(6f, 6f));
            CreateBuildSlots(new Vector2(6f, 6f), "slot_workshop_1");

            // Stairs signpost connecting Central Hall to the Upper area — 2026-07-25: replaces the
            // 3D ramp's continuous slope geometry. Purely a visual/wayfinding marker (walkable, no
            // collider) since 2D has no physical elevation to climb; both floors are simply drawn
            // as separate connected rooms on the same map.
            var stairs = CreateSpriteMarker("Stairs_UpperFloor", new Vector2(0f, -6.5f), new Vector2(2.5f, 1f),
                new Color(0.55f, 0.35f, 0.15f), solid: false);
            WorldLabel.Create(stairs.transform, "Stairs", heightOffset: 0.8f);

            CreateZoneMarker("upper_living", new Vector2(-2f, -10f));
            CreateBuildSlots(new Vector2(-2f, -10f), "slot_upper_living_1", "slot_upper_living_2", "slot_upper_living_3");

            CreateZoneMarker("roof", new Vector2(2.5f, -10f));
            CreateBuildSlots(new Vector2(2.5f, -10f), "slot_roof_1");
            CreateCoreComponent("antenna_mount", new Vector2(3.5f, -10f));

            CreateBoundaryWalls(-9f, 9f, -15f, 15f); // matches the ground tilemap bounds above

            EditorSceneManager.SaveScene(scene, MainShelterScenePath);
        }

        private static void CreateZoneMarker(string zoneId, Vector2 position)
        {
            // No collider by design: a Zone marker is a walkable area label, not an obstacle — see
            // class doc for why this differs from the old 3D primitive (which had a collider too
            // thin to ever actually block the CharacterController).
            var go = CreateSpriteMarker("Zone_" + zoneId, position, new Vector2(1f, 1f),
                new Color(0.3f, 0.3f, 0.3f), solid: false);
            WorldLabel.Create(go.transform, "Zone\n" + DisplayName.Prettify(zoneId), heightOffset: 0.6f);
        }

        private static void CreateCoreComponent(string coreId, Vector2 position)
        {
            var go = CreateSpriteMarker("Core_" + coreId, position, new Vector2(0.4f, 0.4f),
                new Color(0.4f, 0.4f, 0.45f), solid: true);
            go.AddComponent<CoreComponentView>().SetCoreId(coreId);
        }

        private static void CreateBuildSlots(Vector2 zoneCenter, params string[] slotIds)
        {
            for (int i = 0; i < slotIds.Length; i++)
            {
                // No collider by design — an empty build slot must stay walkable until a module is
                // actually built on it (see class doc).
                var go = CreateSpriteMarker("BuildSlot_" + slotIds[i],
                    zoneCenter + new Vector2(0.8f * (i + 1), 0.8f), new Vector2(0.5f, 0.5f),
                    new Color(0.7f, 0.7f, 0.2f), solid: false);
                go.AddComponent<BuildSlotView>().SetSlotId(slotIds[i]);
            }
        }

        /// <summary>Invisible perimeter (Collider2D only, no renderer) around a scene's walkable
        /// footprint — prevents walking off the edge of the designed layout.</summary>
        private static void CreateBoundaryWalls(float minX, float maxX, float minY, float maxY)
        {
            const float thickness = 1f;
            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;
            float sizeX = maxX - minX;
            float sizeY = maxY - minY;

            CreateWall("Wall_North", new Vector2(centerX, maxY + thickness / 2f), new Vector2(sizeX + thickness * 2f, thickness));
            CreateWall("Wall_South", new Vector2(centerX, minY - thickness / 2f), new Vector2(sizeX + thickness * 2f, thickness));
            CreateWall("Wall_East", new Vector2(maxX + thickness / 2f, centerY), new Vector2(thickness, sizeY));
            CreateWall("Wall_West", new Vector2(minX - thickness / 2f, centerY), new Vector2(thickness, sizeY));
        }

        private static void CreateWall(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void BuildConvenienceStoreScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGroundTilemap("store", new Color(0.55f, 0.5f, 0.45f), -8, 8, -8, 8);

            CreateSearchPoint("searchpoint_drink_shelf_1", new Vector2(-4f, 2f));
            CreateSearchPoint("searchpoint_drink_shelf_2", new Vector2(-4f, -2f));
            CreateSearchPoint("searchpoint_dry_shelf_1", new Vector2(-1f, 2f));
            CreateSearchPoint("searchpoint_dry_shelf_2", new Vector2(-1f, -2f));
            CreateSearchPoint("searchpoint_counter", new Vector2(2f, 0f));
            CreateSearchPoint("searchpoint_back_room", new Vector2(5f, 0f));

            var travelPoint = CreateSpriteMarker("TravelPoint_Shelter", new Vector2(0f, -6f), Vector2.one,
                new Color(0.9f, 0.7f, 0.1f), solid: true);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, -5f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            CreateBoundaryWalls(-7.5f, 7.5f, -7.5f, 7.5f); // matches the ground tilemap bounds above

            EditorSceneManager.SaveScene(scene, ConvenienceStoreScenePath);
        }

        private static void BuildUtilityGarageScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGroundTilemap("garage", new Color(0.55f, 0.5f, 0.45f), -8, 8, -8, 8);

            CreateSearchPoint("searchpoint_garage_workbench", new Vector2(-3f, 2f));
            CreateSearchPoint("searchpoint_garage_shelf", new Vector2(3f, 2f));

            var travelPoint = CreateSpriteMarker("TravelPoint_Shelter", new Vector2(0f, -6f), Vector2.one,
                new Color(0.9f, 0.7f, 0.1f), solid: true);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, -5f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            CreateBoundaryWalls(-7.5f, 7.5f, -7.5f, 7.5f); // matches the ground tilemap bounds above

            EditorSceneManager.SaveScene(scene, UtilityGarageScenePath);
        }

        private static void BuildSchoolScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGroundTilemap("school", new Color(0.55f, 0.5f, 0.45f), -8, 8, -8, 8);

            CreateSearchPoint("searchpoint_school_nurse", new Vector2(-3f, 2f));
            CreateSearchPoint("searchpoint_school_classroom", new Vector2(3f, 2f));

            var travelPoint = CreateSpriteMarker("TravelPoint_Shelter", new Vector2(0f, -6f), Vector2.one,
                new Color(0.9f, 0.7f, 0.1f), solid: true);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, -5f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            CreateBoundaryWalls(-7.5f, 7.5f, -7.5f, 7.5f); // matches the ground tilemap bounds above

            EditorSceneManager.SaveScene(scene, SchoolScenePath);
        }

        private static void CreateSearchPoint(string id, Vector2 position)
        {
            var go = CreateSpriteMarker(id, position, new Vector2(1.5f, 0.5f),
                new Color(0.45f, 0.6f, 0.35f), solid: true);
            go.AddComponent<SearchPointView>().SetSearchPointId(id);
        }

        private static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("Boot");
            root.AddComponent<BootLoader>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void DeleteIfExists(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath(scenePath, typeof(UnityEngine.SceneManagement.Scene)) != null ||
                File.Exists(scenePath))
            {
                AssetDatabase.DeleteAsset(scenePath);
                Debug.Log($"[SceneSetup] Deleted old scene: {scenePath}");
            }
        }
    }
}
