using LastHope.Presentation.Boot;
using LastHope.Presentation.CameraRig;
using LastHope.Presentation.Player;
using LastHope.DebugTools.Overlay;
using LastHope.Systems.Boot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Builds the Sprint 1 skeleton scenes (00_Boot, 10_GamePersistent, 90_TestSystems) from
    /// scratch so they can be regenerated deterministically. See mvp-execution-plan.md Sprint 1.
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string BootScenePath = ScenesFolder + "/00_Boot.unity";
        private const string PersistentScenePath = ScenesFolder + "/10_GamePersistent.unity";
        private const string TestSystemsScenePath = ScenesFolder + "/90_TestSystems.unity";
        private const string InputActionsPath = "Assets/Input/GameControls.inputactions";

        [MenuItem("Last Hope/Build Sprint 1 Scenes")]
        public static void BuildAll()
        {
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError($"[SceneSetup] Could not load InputActionAsset at {InputActionsPath}");
                return;
            }

            BuildTestSystemsScene(inputActions);
            BuildGamePersistentScene();
            BuildBootScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(PersistentScenePath, true),
                new EditorBuildSettingsScene(TestSystemsScenePath, true),
            };

            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Sprint 1 scenes built and registered in Build Settings.");
        }

        private static void BuildTestSystemsScene(InputActionAsset inputActions)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ground: default Plane primitive is 10x10 units, scale (2,1,2) -> 20x20m.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);

            // Scale reference cubes: 1m, 2m, 3m cubes in a row.
            CreateScaleCube("ScaleRef_1m", 1f, -3f);
            CreateScaleCube("ScaleRef_2m", 2f, 0f);
            CreateScaleCube("ScaleRef_3m", 3f, 4f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Player
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.1f, -6f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.7f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.85f, 0f);

            var playerController = player.AddComponent<PlayerController>();
            playerController.SetInputActions(inputActions);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            visual.transform.localScale = new Vector3(0.6f, 0.85f, 0.6f);
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

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
            playerController.SetCameraTransform(cameraGo.transform);
            cameraGo.transform.position = player.transform.position + new Vector3(0f, 12f, -12f);
            cameraGo.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);

            EditorSceneManager.SaveScene(scene, TestSystemsScenePath);
        }

        private static void CreateScaleCube(string name, float size, float xPosition)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.localScale = new Vector3(size, size, size);
            cube.transform.position = new Vector3(xPosition, size / 2f, 4f);
        }

        private static void BuildGamePersistentScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("GamePersistent");
            root.AddComponent<GamePersistentMarker>();
            root.AddComponent<GameBootstrapper>();
            root.AddComponent<SimulationDriver>();
            root.AddComponent<DebugOverlay>();

            EditorSceneManager.SaveScene(scene, PersistentScenePath);
        }

        private static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("Boot");
            root.AddComponent<BootLoader>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
        }
    }
}
