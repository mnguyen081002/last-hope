using System.IO;
using LastHope.DebugTools.Overlay;
using LastHope.DebugTools.Panel;
using LastHope.Presentation.Boot;
using LastHope.Presentation.CameraControl;
using LastHope.Presentation.Interaction;
using LastHope.Presentation.Player;
using LastHope.Presentation.World;
using LastHope.Systems.Boot;
using LastHope.UI.Panels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Sinh toàn bộ scene bằng code. Scene là asset binary — sửa tay sẽ trôi khỏi tầm kiểm
    /// soát của review, nên mọi thay đổi cấu trúc scene phải sửa ở đây rồi chạy lại menu.
    /// </summary>
    public static class SceneSetup
    {
        const string ScenesRoot = "Assets/Scenes";
        const string PlaceholderRoot = "Assets/Art/Placeholder";
        const string ControlsPath = "Assets/Input/GameControls.inputactions";
        const string PlayerSpritePath =
            "Assets/Art/Production/Character8Direction/Frames/walk-down-right-0.png";

        const float PixelsPerUnit = 100f;

        // Sorting layer mặc định; sort thực tế do CustomAxis theo Y quyết định.
        const int GroundOrder = -100;

        [MenuItem("Last Hope/Build All Scenes")]
        public static void BuildAllScenes()
        {
            Directory.CreateDirectory(ScenesRoot);
            EnsurePlaceholderSprites();

            BuildBootScene();
            BuildPersistentScene();
            BuildTestRoomScene();
            BuildMainShelterScene();
            BuildConvenienceStoreScene();
            BuildUtilityGarageScene();

            RegisterScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetup] Đã sinh xong 6 scene và cập nhật Build Settings.");
        }

        // ---------- Scene builders ----------

        static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = NewCamera("BootCamera", orthographicSize: 5f);
            camera.tag = "MainCamera";

            var loader = new GameObject("BootLoader");
            loader.AddComponent<BootLoader>();

            SaveScene(scene, $"{ScenesRoot}/00_Boot.unity");
        }

        static void BuildPersistentScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (controls == null) Debug.LogWarning($"[SceneSetup] Không thấy {ControlsPath}");

            // Composition root phải Awake trước mọi view đọc GameBootstrapper.Services.
            var servicesGo = new GameObject("GameServices");
            servicesGo.AddComponent<GameBootstrapper>();
            servicesGo.AddComponent<SimulationDriver>();
            servicesGo.AddComponent<DebugPanel>();

            var player = BuildPlayer(controls);

            var cameraGo = NewCamera("Main Camera", orthographicSize: 6f);
            cameraGo.tag = "MainCamera";
            var rig = cameraGo.AddComponent<CameraRig>();
            SetSerialized(rig, so =>
            {
                so.FindProperty("target").objectReferenceValue = player.transform;
                so.FindProperty("controls").objectReferenceValue = controls;
            });
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var overlayGo = new GameObject("DebugOverlay");
            var overlay = overlayGo.AddComponent<DebugOverlay>();
            SetSerialized(overlay, so =>
                so.FindProperty("trackedTransform").objectReferenceValue = player.transform);

            var promptGo = new GameObject("InteractionPrompt");
            var prompt = promptGo.AddComponent<InteractionPromptOverlay>();
            SetSerialized(prompt, so =>
                so.FindProperty("detector").objectReferenceValue = player.GetComponent<InteractionDetector>());

            var sceneFlowGo = new GameObject("SceneFlowController");
            var sceneFlow = sceneFlowGo.AddComponent<SceneFlowController>();
            SetSerialized(sceneFlow, so =>
            {
                so.FindProperty("playerAvatar").objectReferenceValue = player.GetComponent<PlayerAvatarSync>();
                so.FindProperty("cameraRig").objectReferenceValue = rig;
            });

            var inventoryPanelGo = new GameObject("InventoryPanel");
            var inventoryPanel = inventoryPanelGo.AddComponent<InventoryPanel>();
            SetSerialized(inventoryPanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            var searchPanel = new GameObject("SearchPanel").AddComponent<SearchPanel>();
            SetSerialized(searchPanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            var storagePanel = new GameObject("StoragePanel").AddComponent<StoragePanel>();
            SetSerialized(storagePanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            var travelConfirmPanel = new GameObject("TravelConfirmPanel").AddComponent<TravelConfirmPanel>();
            SetSerialized(travelConfirmPanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            var shelterPanel = new GameObject("ShelterPanel").AddComponent<ShelterPanel>();
            SetSerialized(shelterPanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            var sleepPanel = new GameObject("SleepPanel").AddComponent<SleepPanel>();
            SetSerialized(sleepPanel, so => so.FindProperty("controls").objectReferenceValue = controls);

            SaveScene(scene, $"{ScenesRoot}/10_GamePersistent.unity");
        }

        static void BuildTestRoomScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            const float halfWidth = 16f;
            const float halfHeight = 10f;

            BuildGround(halfWidth, halfHeight);
            BuildBoundary(halfWidth, halfHeight);
            BuildSortTestProps();

            SaveScene(scene, $"{ScenesRoot}/90_TestSystems.unity");
        }

        static void BuildMainShelterScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            const float halfWidth = 10f, halfHeight = 8f;

            // Ground/Upper là 2 GameObject root cùng chiếm 1 footprint world (đè lên nhau) —
            // chỉ 1 cái active tại một thời điểm (BL-P3-01, xem isometric-game-placement-rules.md
            // mục 5-6: floor visibility toggle bằng SetActive, không dựng Tilemap/cầu thang vật lý).
            var groundFloor = new GameObject("GroundFloor");
            BuildGround(halfWidth, halfHeight, groundFloor);
            BuildBoundary(halfWidth, halfHeight, groundFloor);

            var upperFloor = new GameObject("UpperFloor");
            BuildGround(halfWidth, halfHeight, upperFloor);
            BuildBoundary(halfWidth, halfHeight, upperFloor);
            upperFloor.SetActive(false); // mặc định player ở Ground khi vào Shelter.

            var groundInteractables = new GameObject("Interactables");
            groundInteractables.transform.SetParent(groundFloor.transform, false);
            BuildStorage(groundInteractables, "location_shelter", new Vector2(-4f, 3f));
            BuildShelterConsole(groundInteractables, new Vector2(0f, 3f));
            BuildTravelPoint(groundInteractables, "route_shelter_store", "cửa hàng tiện lợi", new Vector2(4f, -3f));
            BuildTravelPoint(groundInteractables, "route_shelter_garage", "gara sửa xe", new Vector2(-8f, -6f));
            // Mỗi TravelPoint có spawn riêng sát cạnh, gắn đúng routeId — 2 cổng ra vào thì
            // phải có 2 spawn (bug user báo trước đó: spawn (0,0) cách xa cổng, giữa phòng).
            BuildPlayerSpawnPoint(groundInteractables, new Vector2(3f, -2f), "route_shelter_store");
            BuildPlayerSpawnPoint(groundInteractables, new Vector2(-7f, -5f), "route_shelter_garage");

            var upperInteractables = new GameObject("Interactables");
            upperInteractables.transform.SetParent(upperFloor.transform, false);
            BuildBed(upperInteractables, new Vector2(0f, 2f));

            // Cầu thang: 1 điểm mỗi tầng, cùng vị trí world (4,3) — điểm đến lệch (4,2) để
            // không đứng đè lên chính prop vừa tương tác.
            BuildStaircase(groundInteractables, new Vector2(4f, 3f), "Lên gác",
                ownFloorRoot: groundFloor, otherFloorRoot: upperFloor, landingPosition: new Vector2(4f, 2f));
            BuildStaircase(upperInteractables, new Vector2(4f, 3f), "Xuống dưới",
                ownFloorRoot: upperFloor, otherFloorRoot: groundFloor, landingPosition: new Vector2(4f, 2f));

            SaveScene(scene, $"{ScenesRoot}/Shelters/20_MainShelter.unity");
        }

        static void BuildConvenienceStoreScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            const float halfWidth = 10f, halfHeight = 8f;
            BuildGround(halfWidth, halfHeight);
            BuildBoundary(halfWidth, halfHeight);

            var root = new GameObject("Interactables");
            BuildSearchPoint(root, "searchpoint_drink_shelf_1", new Vector2(-6f, 4f));
            BuildSearchPoint(root, "searchpoint_drink_shelf_2", new Vector2(-3f, 4f));
            BuildSearchPoint(root, "searchpoint_dry_shelf_1", new Vector2(0f, 4f));
            BuildSearchPoint(root, "searchpoint_dry_shelf_2", new Vector2(3f, 4f));
            BuildSearchPoint(root, "searchpoint_counter", new Vector2(6f, 0f));
            BuildSearchPoint(root, "searchpoint_back_room", new Vector2(6f, -4f));
            BuildTravelPoint(root, "route_shelter_store", "shelter", new Vector2(-6f, -5f));
            BuildPlayerSpawnPoint(root, new Vector2(-5f, -4f), "route_shelter_store");

            SaveScene(scene, $"{ScenesRoot}/Locations/41_Location_ConvenienceStore.unity");
        }

        static void BuildUtilityGarageScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            const float halfWidth = 10f, halfHeight = 8f;
            BuildGround(halfWidth, halfHeight);
            BuildBoundary(halfWidth, halfHeight);

            var root = new GameObject("Interactables");
            BuildSearchPoint(root, "searchpoint_garage_workbench", new Vector2(-4f, 4f));
            BuildSearchPoint(root, "searchpoint_garage_shelf", new Vector2(4f, 4f));
            BuildTravelPoint(root, "route_shelter_garage", "shelter", new Vector2(-6f, -5f));
            BuildPlayerSpawnPoint(root, new Vector2(-5f, -4f), "route_shelter_garage");

            SaveScene(scene, $"{ScenesRoot}/Locations/42_Location_UtilityGarage.unity");
        }

        // ---------- Pieces ----------

        static GameObject BuildPlayer(InputActionAsset controls)
        {
            var root = new GameObject("Player");
            root.transform.position = Vector3.zero;

            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Collider bám chân, không bao cả sprite — footprint mới là thứ va chạm.
            var collider = root.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.5f, 0.4f);
            collider.direction = CapsuleDirection2D.Horizontal;
            collider.offset = new Vector2(0f, 0.2f);

            var controller = root.AddComponent<PlayerController>();
            SetSerialized(controller, so =>
                so.FindProperty("controls").objectReferenceValue = controls);

            root.AddComponent<PlayerAvatarSync>();
            root.AddComponent<PlayerMovementModifierSync>();

            var detector = root.AddComponent<InteractionDetector>();
            SetSerialized(detector, so =>
                so.FindProperty("controls").objectReferenceValue = controls);

            // Sprite là child, đẩy lên trên để root transform nằm ở chân → Y-sort đúng.
            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(root.transform, false);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerSpritePath);
            var renderer = spriteGo.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            if (sprite != null)
            {
                spriteGo.transform.localPosition =
                    new Vector3(0f, sprite.bounds.extents.y, 0f);
            }

            return root;
        }

        static void BuildGround(float halfWidth, float halfHeight, GameObject parent = null)
        {
            var go = new GameObject("Ground");
            if (parent != null) go.transform.SetParent(parent.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPlaceholder("placeholder-ground.png");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(halfWidth * 2f, halfHeight * 2f);
            renderer.sortingOrder = GroundOrder;
        }

        static void BuildBoundary(float halfWidth, float halfHeight, GameObject parent = null)
        {
            var root = new GameObject("Boundary");
            if (parent != null) root.transform.SetParent(parent.transform, false);
            const float thickness = 1f;

            AddWall(root, "Wall_Top",
                new Vector2(0f, halfHeight + thickness * 0.5f),
                new Vector2(halfWidth * 2f + thickness * 2f, thickness));
            AddWall(root, "Wall_Bottom",
                new Vector2(0f, -halfHeight - thickness * 0.5f),
                new Vector2(halfWidth * 2f + thickness * 2f, thickness));
            AddWall(root, "Wall_Left",
                new Vector2(-halfWidth - thickness * 0.5f, 0f),
                new Vector2(thickness, halfHeight * 2f));
            AddWall(root, "Wall_Right",
                new Vector2(halfWidth + thickness * 0.5f, 0f),
                new Vector2(thickness, halfHeight * 2f));
        }

        static void AddWall(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        /// <summary>Vài prop rải theo trục Y để mắt kiểm tra Y-sort khi player đi trước/sau.</summary>
        static void BuildSortTestProps()
        {
            var root = new GameObject("SortTestProps");
            var positions = new[]
            {
                new Vector2(-4f, 3f), new Vector2(0f, 1.5f),
                new Vector2(4f, -1f), new Vector2(-2f, -3.5f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                BuildWorldProp(root, $"Prop_{i}", positions[i], Color.white);
            }
        }

        /// <summary>Prop có sprite + collider chặn — dùng làm nền cho mọi interactable trong world.</summary>
        static GameObject BuildWorldProp(GameObject parent, string name, Vector2 position, Color tint)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = position;

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            var renderer = spriteGo.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPlaceholder("placeholder-prop.png");
            renderer.color = tint;
            if (renderer.sprite != null)
            {
                spriteGo.transform.localPosition = new Vector3(0f, renderer.sprite.bounds.extents.y, 0f);
            }

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 0.4f);
            collider.offset = new Vector2(0f, 0.2f);

            return go;
        }

        static void BuildSearchPoint(GameObject parent, string searchPointId, Vector2 position)
        {
            var go = BuildWorldProp(parent, searchPointId, position, new Color(0.75f, 0.62f, 0.4f));
            var view = go.AddComponent<SearchPointView>();
            SetSerialized(view, so => so.FindProperty("searchPointId").stringValue = searchPointId);
        }

        static void BuildStorage(GameObject parent, string locationId, Vector2 position)
        {
            var go = BuildWorldProp(parent, "Storage", position, new Color(0.4f, 0.6f, 0.75f));
            var view = go.AddComponent<StorageView>();
            SetSerialized(view, so => so.FindProperty("locationId").stringValue = locationId);
        }

        static void BuildShelterConsole(GameObject parent, Vector2 position)
        {
            var go = BuildWorldProp(parent, "ShelterConsole", position, new Color(0.55f, 0.5f, 0.65f));
            go.AddComponent<ShelterConsoleView>();
        }

        static void BuildBed(GameObject parent, Vector2 position)
        {
            var go = BuildWorldProp(parent, "Bed", position, new Color(0.6f, 0.45f, 0.5f));
            go.AddComponent<BedView>();
        }

        static void BuildStaircase(
            GameObject parent, Vector2 position, string promptText,
            GameObject ownFloorRoot, GameObject otherFloorRoot, Vector2 landingPosition)
        {
            var go = BuildWorldProp(parent, "Staircase", position, new Color(0.5f, 0.4f, 0.3f));
            var view = go.AddComponent<StaircaseView>();
            SetSerialized(view, so =>
            {
                so.FindProperty("ownFloorRoot").objectReferenceValue = ownFloorRoot;
                so.FindProperty("otherFloorRoot").objectReferenceValue = otherFloorRoot;
                so.FindProperty("landingPosition").vector2Value = landingPosition;
                so.FindProperty("promptText").stringValue = promptText;
            });
        }

        static void BuildTravelPoint(GameObject parent, string routeId, string destinationLabel, Vector2 position)
        {
            var go = BuildWorldProp(parent, "TravelPoint", position, new Color(0.4f, 0.7f, 0.45f));
            var view = go.AddComponent<TravelPointView>();
            SetSerialized(view, so =>
            {
                so.FindProperty("routeId").stringValue = routeId;
                so.FindProperty("destinationLabel").stringValue = destinationLabel;
            });
        }

        static void BuildPlayerSpawnPoint(GameObject parent, Vector2 position, string routeId = "")
        {
            var go = new GameObject("PlayerSpawnPoint");
            go.transform.SetParent(parent.transform, false);
            go.transform.position = position;
            var spawn = go.AddComponent<PlayerSpawnPoint>();
            SetSerialized(spawn, so => so.FindProperty("routeId").stringValue = routeId);
        }

        static GameObject NewCamera(string name, float orthographicSize)
        {
            var go = new GameObject(name);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.12f);
            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = new Vector3(0f, 1f, 0f);
            go.transform.position = new Vector3(0f, 0f, -10f);
            return go;
        }

        // ---------- Placeholder sprites ----------

        /// <summary>
        /// Sinh sprite placeholder dạng PNG thật (không tạo asset runtime) để scene tham chiếu
        /// được bằng GUID ổn định. Chỉ ghi khi file chưa tồn tại — không đè art thật.
        /// </summary>
        static void EnsurePlaceholderSprites()
        {
            Directory.CreateDirectory(PlaceholderRoot);

            WritePlaceholderIfMissing("placeholder-ground.png", 100, 100,
                new Color32(46, 50, 56, 255), new Color32(38, 42, 47, 255));
            WritePlaceholderIfMissing("placeholder-prop.png", 100, 140,
                new Color32(128, 92, 60, 255), new Color32(150, 110, 72, 255));
        }

        static void WritePlaceholderIfMissing(
            string fileName, int width, int height, Color32 fill, Color32 edge)
        {
            string path = $"{PlaceholderRoot}/{fileName}";
            if (File.Exists(path)) return;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    pixels[y * width + x] = border ? edge : fill;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(path);
        }

        static void ConfigureSpriteImporter(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            // Tiled draw mode yêu cầu mesh FullRect.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        static Sprite LoadPlaceholder(string fileName) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderRoot}/{fileName}");

        // ---------- Utils ----------

        static void SetSerialized(Object target, System.Action<SerializedObject> apply)
        {
            var so = new SerializedObject(target);
            apply(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SaveScene(Scene scene, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[SceneSetup] Đã ghi {path}");
        }

        static void RegisterScenesInBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesRoot}/00_Boot.unity", true),
                new EditorBuildSettingsScene($"{ScenesRoot}/10_GamePersistent.unity", true),
                new EditorBuildSettingsScene($"{ScenesRoot}/90_TestSystems.unity", true),
                new EditorBuildSettingsScene($"{ScenesRoot}/Shelters/20_MainShelter.unity", true),
                new EditorBuildSettingsScene($"{ScenesRoot}/Locations/41_Location_ConvenienceStore.unity", true),
                new EditorBuildSettingsScene($"{ScenesRoot}/Locations/42_Location_UtilityGarage.unity", true),
            };
        }
    }
}
