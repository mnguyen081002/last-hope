using LastHope.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHope.Editor
{
    public static class FreshProjectBootstrap
    {
        private const string ScenePath = "Assets/Scenes/DayOne.unity";
        private const string CharacterFrames = "Assets/Art/Production/CharacterM/Frames";

        [MenuItem("Last Hope/Rebuild Fresh Day One")]
        public static void Create()
        {
            EnsureFolder("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.075f);

            CreateBlock("Shelter Floor", Vector2.zero, new Vector2(10f, 8f), new Color(0.22f, 0.23f, 0.2f), false);
            CreateWall("Wall North", new Vector2(0f, 4f), new Vector2(10f, 0.4f));
            CreateWall("Wall South Left", new Vector2(-3.25f, -4f), new Vector2(3.5f, 0.4f));
            CreateWall("Wall South Right", new Vector2(3.25f, -4f), new Vector2(3.5f, 0.4f));
            CreateWall("Wall West", new Vector2(-5f, 0f), new Vector2(0.4f, 8f));
            CreateWall("Wall East", new Vector2(5f, 0f), new Vector2(0.4f, 8f));
            CreateSpriteVisual("Production Map", "Assets/Art/Production/Map/production-map-base.png", new Vector2(0f, -11f), Vector2.one, -20);
            CreateSpriteVisual("Shelter Entrance Art", "Assets/Art/Generated/shelter.png", new Vector2(0f, -4.7f), new Vector2(0.28f, 0.28f), -2);

            GameObject player = CreateBlock("Player", new Vector2(0f, 1f), new Vector2(0.75f, 0.75f), new Color(0.85f, 0.78f, 0.42f), true);
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            player.AddComponent<PlayerMotor>();
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            playerRenderer.color = Color.white;
            player.transform.localScale = Vector3.one * 0.48f;
            DirectionalSpriteAnimator animator = player.AddComponent<DirectionalSpriteAnimator>();
            animator.Configure(
                LoadFrames("idle-down", 6), LoadFrames("idle-up", 6), LoadFrames("idle-left", 6), LoadFrames("idle-right", 6),
                LoadFrames("walk-down", 12), LoadFrames("walk-up", 12), LoadFrames("walk-left", 12), LoadFrames("walk-right", 12));
            camera.gameObject.AddComponent<CameraFollow2D>().Configure(player.transform);

            CreateInteraction("Radio", "radio", "nghe radio", new Vector2(-3.6f, 2.8f), new Color(0.35f, 0.72f, 0.76f));
            CreateInteraction("Storage", "storage", "kiểm tra kho", new Vector2(0f, 2.8f), new Color(0.72f, 0.58f, 0.3f));
            CreateInteraction("Filter Unit", "filter_unit", "kiểm tra máy lọc", new Vector2(3.6f, 2.8f), new Color(0.45f, 0.7f, 0.45f));
            CreateInteraction("Shelter Door", "door", "mở cửa / trở về", new Vector2(0f, -4f), new Color(0.65f, 0.65f, 0.62f));
            GameObject nearLoot = CreateInteraction("Near Loot", "near_loot", "lục điểm gần", new Vector2(-3f, -8f), Color.white);
            ApplySprite(nearLoot, "Assets/Art/Production/Loot/Chest/Frames/chest-open-0.png", 0.5f);
            GameObject farLoot = CreateInteraction("Far Loot", "far_loot", "lục điểm xa", new Vector2(5f, -14f), Color.white);
            ApplySprite(farLoot, "Assets/Art/Generated/loot-crate.png", 0.32f);
            CreateInteraction("Workbench", "workbench", "lắp bộ lọc", new Vector2(-3.6f, -2.6f), new Color(0.55f, 0.62f, 0.7f));

            DayOneDirector director = new GameObject("Day One Director").AddComponent<DayOneDirector>();
            director.Configure(player.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Fresh Day One scene created: " + ScenePath);
        }

        private static GameObject CreateInteraction(string name, string id, string label, Vector2 position, Color color)
        {
            GameObject result = CreateBlock(name, position, Vector2.one, color, false);
            result.AddComponent<DayOneInteractable>().Configure(id, label);
            return result;
        }

        private static GameObject CreateWall(string name, Vector2 position, Vector2 scale)
        {
            return CreateBlock(name, position, scale, new Color(0.12f, 0.13f, 0.12f), true);
        }

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{CharacterFrames}/{prefix}-{i}.png");
            }
            return frames;
        }

        private static GameObject CreateSpriteVisual(string name, string path, Vector2 position, Vector2 scale, int sortingOrder)
        {
            GameObject result = new GameObject(name);
            result.transform.position = position;
            result.transform.localScale = scale;
            SpriteRenderer renderer = result.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            renderer.sortingOrder = sortingOrder;
            return result;
        }

        private static void ApplySprite(GameObject target, string path, float scale)
        {
            target.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            target.GetComponent<SpriteRenderer>().color = Color.white;
            target.transform.localScale = Vector3.one * scale;
        }

        private static GameObject CreateBlock(string name, Vector2 position, Vector2 scale, Color color, bool collider)
        {
            GameObject result = new GameObject(name);
            result.transform.position = position;
            result.transform.localScale = scale;
            SpriteRenderer renderer = result.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = color;
            if (collider)
            {
                result.AddComponent<BoxCollider2D>();
            }
            return result;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }
    }
}
