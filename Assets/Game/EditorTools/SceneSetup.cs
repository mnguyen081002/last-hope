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
using LastHope.UI.Hud;
using LastHope.UI.Inventory;
using LastHope.UI.Map;
using LastHope.UI.Shelter;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Builds the project's core scenes (00_Boot, 10_GamePersistent, 90_TestSystems) from scratch
    /// so they can be regenerated deterministically. Player/Camera/HUD live in 10_GamePersistent
    /// (persistent avatar — prerequisite for Sprint 6 scene switching); gameplay scenes contain
    /// only environment/interactables/spawn markers.
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string BootScenePath = ScenesFolder + "/00_Boot.unity";
        private const string PersistentScenePath = ScenesFolder + "/10_GamePersistent.unity";
        private const string TestSystemsScenePath = ScenesFolder + "/90_TestSystems.unity";
        private const string MainShelterScenePath = ScenesFolder + "/Shelters/20_MainShelter.unity";
        private const string ConvenienceStoreScenePath = ScenesFolder + "/Locations/41_Location_ConvenienceStore.unity";
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

            BuildTestSystemsScene();
            BuildGamePersistentScene(inputActions);
            BuildMainShelterScene();
            BuildConvenienceStoreScene();
            BuildBootScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(PersistentScenePath, true),
                new EditorBuildSettingsScene(TestSystemsScenePath, true),
                new EditorBuildSettingsScene(MainShelterScenePath, true),
                new EditorBuildSettingsScene(ConvenienceStoreScenePath, true),
            };

            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Scenes built and registered in Build Settings.");
        }

        private static void BuildTestSystemsScene()
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

            EditorSceneManager.SaveScene(scene, TestSystemsScenePath);
        }

        private static void CreateScaleCube(string name, float size, float xPosition)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.localScale = new Vector3(size, size, size);
            cube.transform.position = new Vector3(xPosition, size / 2f, 4f);
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
            playerController.SetCameraTransform(cameraGo.transform);
            // Matches CameraRig.Awake()'s own offset formula (rotation * back * distance) so there's
            // no visible snap on the first frame — CameraRig recomputes and takes over regardless.
            cameraGo.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            cameraGo.transform.position = player.transform.position + cameraGo.transform.rotation * new Vector3(0f, 0f, -16.97f);

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
        }

        private static void BuildMainShelterScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            // (1.5,1.5) = 15x15m (±7.5) used to leave the ramp/Upper Floor platform (z down to
            // -13) entirely off the edge — walking past z=-7.5 meant no floor at all, an
            // unrecoverable fall (2026-07-24 playtest). (1.8,3) = 18x30m (x:±9, z:±15) comfortably
            // covers the whole layout with margin.
            ground.transform.localScale = new Vector3(1.8f, 1f, 3f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var storage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            storage.name = "ShelterStorage";
            storage.transform.position = new Vector3(3f, 0.5f, 3f);
            storage.AddComponent<ShelterStorageView>().SetShelterId("shelter_main");

            var travelPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            travelPoint.name = "TravelPoint_Store";
            travelPoint.transform.position = new Vector3(-3f, 0.5f, 3f);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, 0.1f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            // S10 blockout: 6 ground-floor/upper zones + Fixed Core Component anchors
            // (main-shelter-design.md §6-14). Roof is co-located on the Upper platform (single
            // zone/1 slot — a third physical elevation isn't worth the extra ramp for a blockout).
            CreateZoneMarker("shelter_entrance", new Vector3(-6f, 0.5f, -6f));
            CreateBuildSlots(new Vector3(-6f, 0.5f, -6f), "slot_shelter_entrance_1", "slot_shelter_entrance_2");

            CreateZoneMarker("central_hall", new Vector3(0f, 0.5f, -4f));
            CreateCoreComponent("main_staircase", new Vector3(1f, 0.5f, -4f));

            CreateZoneMarker("ground_storage", new Vector3(-6f, 0.5f, 3f));
            CreateBuildSlots(new Vector3(-6f, 0.5f, 3f), "slot_ground_storage_1", "slot_ground_storage_2");

            CreateZoneMarker("utility_area", new Vector3(6f, 0.5f, -4f));
            CreateBuildSlots(new Vector3(6f, 0.5f, -4f), "slot_utility_area_1", "slot_utility_area_2");
            CreateCoreComponent("structural_pillars", new Vector3(6f, 0.5f, -6f));
            CreateCoreComponent("electrical_backbone", new Vector3(7f, 0.5f, -4f));
            var drainCore = new GameObject("DrainCore");
            drainCore.transform.position = new Vector3(5f, 0.5f, -3f);
            drainCore.AddComponent<DrainCoreView>();

            CreateZoneMarker("water_processing", new Vector3(6f, 0.5f, 2f));
            CreateBuildSlots(new Vector3(6f, 0.5f, 2f), "slot_water_processing_1", "slot_water_processing_2");
            CreateCoreComponent("water_intake", new Vector3(7f, 0.5f, 2f));

            CreateZoneMarker("workshop", new Vector3(6f, 0.5f, 6f));
            CreateBuildSlots(new Vector3(6f, 0.5f, 6f), "slot_workshop_1");

            // Upper Floor: raised platform reached by a ramp from Central Hall (Main Staircase).
            var upperPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperPlatform.name = "UpperFloor";
            upperPlatform.transform.position = new Vector3(0f, 2.4f, -10f);
            upperPlatform.transform.localScale = new Vector3(8f, 0.2f, 6f);
            CreateRamp(new Vector3(0f, 0.15f, -5f), new Vector3(0f, 2.5f, -8f), width: 2.5f);

            CreateZoneMarker("upper_living", new Vector3(-2f, 2.5f, -10f));
            CreateBuildSlots(new Vector3(-2f, 2.5f, -10f), "slot_upper_living_1", "slot_upper_living_2", "slot_upper_living_3");

            CreateZoneMarker("roof", new Vector3(2.5f, 2.5f, -10f));
            CreateBuildSlots(new Vector3(2.5f, 2.5f, -10f), "slot_roof_1");
            CreateCoreComponent("antenna_mount", new Vector3(3.5f, 2.5f, -10f));

            CreateBoundaryWalls(-9f, 9f, -15f, 15f); // matches the Ground plane's (1.8,1,3) scale

            EditorSceneManager.SaveScene(scene, MainShelterScenePath);
        }

        private static void CreateZoneMarker(string zoneId, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Zone_" + zoneId;
            go.transform.position = position;
            go.transform.localScale = new Vector3(1f, 0.1f, 1f);
            WorldLabel.Create(go.transform, "Zone\n" + DisplayName.Prettify(zoneId), heightOffset: 0.6f);
        }

        private static void CreateCoreComponent(string coreId, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Core_" + coreId;
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
            go.AddComponent<CoreComponentView>().SetCoreId(coreId);
        }

        private static void CreateBuildSlots(Vector3 zoneCenter, params string[] slotIds)
        {
            for (int i = 0; i < slotIds.Length; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "BuildSlot_" + slotIds[i];
                go.transform.position = zoneCenter + new Vector3(0.8f * (i + 1), 0.1f, 0.8f);
                go.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
                go.AddComponent<BuildSlotView>().SetSlotId(slotIds[i]);
            }
        }

        /// <summary>Boxy walkable ramp connecting a ground point to a raised platform point —
        /// rotation computed from the two points so callers never hand-tune angles.</summary>
        private static void CreateRamp(Vector3 groundPoint, Vector3 platformPoint, float width)
        {
            Vector3 diff = platformPoint - groundPoint;
            float horizontalRun = new Vector2(diff.x, diff.z).magnitude;
            float length = diff.magnitude;
            float pitch = Mathf.Atan2(diff.y, horizontalRun) * Mathf.Rad2Deg;
            float yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp_UpperFloor";
            ramp.transform.position = (groundPoint + platformPoint) / 2f;
            ramp.transform.rotation = Quaternion.Euler(-pitch, yaw, 0f);
            ramp.transform.localScale = new Vector3(width, 0.3f, length);
            // Default primitive grey blended into the equally-grey Ground/UpperFloor, and unlike
            // every other marker in the scene it never got a WorldLabel — together that's why the
            // 2026-07-24 playtest never spotted it at all.
            ramp.GetComponent<Renderer>().material.color = new Color(0.55f, 0.35f, 0.15f);
            WorldLabel.Create(ramp.transform, "Ramp", heightOffset: 1f);
        }

        /// <summary>Invisible perimeter (BoxCollider only, no renderer) around a scene's walkable
        /// footprint — prevents walking off the edge in the first place, rather than only catching
        /// the fall after it happens (2026-07-24 playtest: fell through the map with no floor
        /// below and no way back). PlayerController's grounded-position fallback stays as a second
        /// line of defense for any scene that doesn't call this, or any gap these walls miss.</summary>
        private static void CreateBoundaryWalls(float minX, float maxX, float minZ, float maxZ)
        {
            const float height = 10f;
            const float thickness = 1f;
            float centerX = (minX + maxX) / 2f;
            float centerZ = (minZ + maxZ) / 2f;
            float sizeX = maxX - minX;
            float sizeZ = maxZ - minZ;

            CreateWall("Wall_North", new Vector3(centerX, height / 2f, maxZ + thickness / 2f), new Vector3(sizeX + thickness * 2f, height, thickness));
            CreateWall("Wall_South", new Vector3(centerX, height / 2f, minZ - thickness / 2f), new Vector3(sizeX + thickness * 2f, height, thickness));
            CreateWall("Wall_East", new Vector3(maxX + thickness / 2f, height / 2f, centerZ), new Vector3(thickness, height, sizeZ));
            CreateWall("Wall_West", new Vector3(minX - thickness / 2f, height / 2f, centerZ), new Vector3(thickness, height, sizeZ));
        }

        private static void CreateWall(string name, Vector3 position, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = size;
            Object.DestroyImmediate(go.GetComponent<MeshRenderer>()); // collider only — invisible
        }

        private static void BuildConvenienceStoreScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreateSearchPoint("searchpoint_drink_shelf_1", new Vector3(-4f, 0.5f, 2f));
            CreateSearchPoint("searchpoint_drink_shelf_2", new Vector3(-4f, 0.5f, -2f));
            CreateSearchPoint("searchpoint_dry_shelf_1", new Vector3(-1f, 0.5f, 2f));
            CreateSearchPoint("searchpoint_dry_shelf_2", new Vector3(-1f, 0.5f, -2f));
            CreateSearchPoint("searchpoint_counter", new Vector3(2f, 0.5f, 0f));
            CreateSearchPoint("searchpoint_back_room", new Vector3(5f, 0.5f, 0f));

            var travelPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            travelPoint.name = "TravelPoint_Shelter";
            travelPoint.transform.position = new Vector3(0f, 0.5f, -6f);
            travelPoint.AddComponent<TravelPointView>();

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, 0.1f, -5f);
            spawn.AddComponent<PlayerSpawnPoint>();

            CreateBoundaryWalls(-7.5f, 7.5f, -7.5f, 7.5f); // matches the Ground plane's (1.5,1,1.5) scale

            EditorSceneManager.SaveScene(scene, ConvenienceStoreScenePath);
        }

        private static void CreateSearchPoint(string id, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.5f, 1f, 0.5f);
            go.AddComponent<SearchPointView>().SetSearchPointId(id);
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
