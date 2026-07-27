using System.IO;
using LastHope.DebugTools.Overlay;
using LastHope.DebugTools.Panel;
using LastHope.Presentation.Boot;
using LastHope.Presentation.CameraControl;
using LastHope.Presentation.Player;
using LastHope.Systems.Boot;
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

        [MenuItem("Last Hope/Build Sprint 1 Scenes")]
        public static void BuildSprint1Scenes()
        {
            Directory.CreateDirectory(ScenesRoot);
            EnsurePlaceholderSprites();

            BuildBootScene();
            BuildPersistentScene();
            BuildTestRoomScene();

            RegisterScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetup] Đã sinh xong 3 scene và cập nhật Build Settings.");
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

        static void BuildGround(float halfWidth, float halfHeight)
        {
            var go = new GameObject("Ground");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPlaceholder("placeholder-ground.png");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(halfWidth * 2f, halfHeight * 2f);
            renderer.sortingOrder = GroundOrder;
        }

        static void BuildBoundary(float halfWidth, float halfHeight)
        {
            var root = new GameObject("Boundary");
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

            var sprite = LoadPlaceholder("placeholder-prop.png");
            for (int i = 0; i < positions.Length; i++)
            {
                var go = new GameObject($"Prop_{i}");
                go.transform.SetParent(root.transform, false);
                go.transform.position = positions[i];

                var spriteGo = new GameObject("Sprite");
                spriteGo.transform.SetParent(go.transform, false);
                var renderer = spriteGo.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                if (sprite != null)
                {
                    spriteGo.transform.localPosition = new Vector3(0f, sprite.bounds.extents.y, 0f);
                }

                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.8f, 0.4f);
                collider.offset = new Vector2(0f, 0.2f);
            }
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
            };
        }
    }
}
