# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Assembly map (dependency một chiều)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI,
                                      Unity.InputSystem, Unity.TextMeshPro, UnityEngine.UI, URP Runtime
```

Presentation và UI (từ S5) đều thêm reference `Unity.InputSystem`, `Unity.TextMeshPro`, `UnityEngine.UI` (cần cho Interaction/InventoryPanel).

## Scene flow

`00_Boot` (BootLoader) → additive `10_GamePersistent` (services + Player/Camera/HUD Canvas sống suốt phiên) → **`SceneFlowController` (S6) load scene gameplay đầu tiên** theo `LocationDefinition.SceneName` của `Player.CurrentLocationId` (mặc định `location_shelter` → `20_MainShelter`), và load/unload lại mỗi khi `TravelCompleted`/`WorldStateReloaded` bắn ra. `90_TestSystems` (Ground/ScaleRef/Light) không còn nằm trong luồng boot tự động — chỉ để mở tay trong Editor kiểm tra scale.

---

## LastHope.Core

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Core/Logging/GameLog.cs` | `GameLog` (static) + `LogCategory` enum | `Info/Warn/Error(LogCategory, string)` | ⬜ |
| `Assets/Game/Core/State/WorldState.cs` | `WorldState` + `RouteState`/`NpcState`/`ActiveEventState`/`ActiveTaskState` (vẫn stub `Id`+`StatusName`) + **`LocationState`** (S6: +`SearchPointStates:Dict<string,SearchPointState>`, +`DroppedItems:InventoryState`) + **`ShelterState`** (S6: +`Storage:InventoryState`) | Root state graph: `WorldTimeMinutes`, `CurrentDisasterPhase`, `RandomSeed`, `RngStreams`, `Player`, **`PlaythroughId`** (S6, cho telemetry), các Dictionary state theo id | 🟡 (gián tiếp qua SearchPointTests/OwnerResolverTests) |
| `Assets/Game/Core/State/SearchPointState.cs` (S6) | `SearchPointState` | `{SearchPointId, Rolled, Inventory:InventoryState}` — roll 1 lần, Inventory là InventoryState đầy đủ nên TransferItemCommand dùng lại nguyên vẹn | ✅ (SearchPointTests) |
| `Assets/Game/Core/State/PlayerState.cs` | `PlayerState` | `ActorId`, `CurrentLocationId`, `Inventory`, `Condition` chưa có (chờ S7) | ⬜ |
| `Assets/Game/Core/State/InventoryState.cs` | `InventoryState` + `OverloadState` enum | `Items` (instanceId→ItemInstanceState), `CurrentWeightKg/VolumeLiters`, `Overload` (nay được `Systems.Inventory.InventorySystem` set, xem dưới) | ✅ (InventoryRulesTests) |
| `Assets/Game/Core/Rules/InventoryRules.cs` | `InventoryRules` (static) | `ComputeOverload(inv,balance)` (max(weight%,vol%), >100%→Light >130%→Heavy), `CanAccept(dest,defs,balance,itemId,qty)` (chặn ở 150% hard cap, container không giới hạn luôn true), `SpeedModifierFor(overload,balance)`, **`LoadFactorFor(overload,balance)`** (S6, dùng cho travel time), `IsCapacityLimited(ownerId)` (chỉ "player") | ✅ (8 test) |
| `Assets/Game/Core/State/ItemInstanceState.cs` | `ItemInstanceState` + `ContaminationState`/`WetState` enum | `InstanceId`, `ItemId`, `Quantity`, `Condition`, `Durability`, `Contamination`, `Wet`, `ContainerId` | ⬜ |
| `Assets/Game/Core/State/InventoryOps.cs` | `InventoryOps` (static) | `RecalculateLoad(inv, defs)` (chỉ tính tổng weight/volume, KHÔNG set Overload); `AddItem(inv, defs, itemId, qty, idGen)` (merge stack theo MaxStackSize, không kiểm capacity) | ⬜ |
| `Assets/Game/Core/Random/RngStream.cs` | `RngStream` + `RngStreamState` | xorshift64* trên state `ulong` mutable, `NextInt(min,maxExcl)`, `NextDouble()` | ✅ |
| `Assets/Game/Core/Random/RngService.cs` | `RngService` | `GetStream(name)` — named stream derive từ `WorldState.RandomSeed ⊕ FNV1a64(name)`, state sống trong `WorldState.RngStreams` | ✅ |
| `Assets/Game/Core/Save/WorldStateSerializer.cs` | `WorldStateSerializer` (static) | `Serialize(WorldState)` (indented), `SerializeCanonical(WorldState)` (Formatting.None, dùng cho checksum/deep-compare), `Deserialize(json)`, `Settings` (snake_case, StringEnumConverter, ObjectCreationHandling.Replace) | ✅ |
| `Assets/Game/Core/Save/SaveFile.cs` | `SaveFile`, `SaveSlotInfo` | `SaveFile{SaveVersion,DefinitionVersion,SavedAtUtc,Checksum,SlotId,World(JRaw)}` — World embed verbatim, không re-serialize | ✅ |
| `Assets/Game/Core/Save/SaveService.cs` | `SaveService`, `SaveResult`, `LoadResult` | `Autosave(world)` (round-robin autosave_0/1/2), `SaveToSlot(world,slotId)` (atomic: tmp→verify→backup cũ→rename), `Load(slotId)`, `ListSlots()`. Checksum SHA256 trên world payload canonical | ✅ |
| `Assets/Game/Core/Events/EventBus.cs` | `EventBus` (+ private `EventChannel<T>`) | `Subscribe<T>/Unsubscribe<T>/Publish<T>` — struct event, copy-on-write handler array, zero-boxing | ✅ (gián tiếp qua CommandPipelineTests) |
| `Assets/Game/Core/Events/GameEvents.cs` | `IGameEvent` + 15 struct: `WorldTimeChanged`, `DisasterPhaseChanged`, `RouteStateChanged`, `ShelterWarningRaised`, `TaskCompleted`, `EventDiscovered`, `InventoryChanged`, `NpcStateChanged`, `OverloadStateChanged` (S5), `WorldStateReloaded` (S5, publish sau khi DebugPanel Load), `ItemTransferred` (S5), **`SearchPointOpened`, `ContainerViewRequested`** (S6, UI-routing — không phải sim state), **`TravelStarted`, `TravelCompleted`** (S6) | | ⬜ |
| `Assets/Game/Core/Time/GameTimeUtil.cs` | `GameTimeUtil` (static) | `DayIndex(m)`, `TimeOfDayMinutes(m)`, `Format(m)` — anchor Day 0 17:00 = phút 0 | ⬜ (gián tiếp qua TickSchedulerTests) |
| `Assets/Game/Core/Time/SimulationClock.cs` | `SimulationClock` | `AccumulateRealSeconds(double)`, `TryConsumeMinute()`, `PendingGameSeconds`. **Bank dùng `decimal` nội bộ** (không phải double) — double cộng dồn ~17k lần bị lệch 1 phút/24h, xem comment trong file | ✅ |
| `Assets/Game/Core/Time/TickScheduler.cs` | `TickScheduler` | `SubscribeShort/Long(Action<long>)`, `RegisterThreshold(minute, cb)`, `Advance(clock, maxMinutes)` (bounded catch-up), `FastForward(minutes)` (Sleep/Travel). `AdvanceOneMinute()` private — NƠI DUY NHẤT tăng `WorldTimeMinutes` | ✅ |
| `Assets/Game/Core/Commands/IGameCommand.cs` | `IGameCommand`, `CommandResult`, `CommandErrorCode` (+**`NotAtLocation`** S6), `GameContext` | `GameContext{World,Definitions,Events,Rng,Clock}` — bundle inject duy nhất (đã thêm `Clock` so với plan gốc, cần cho StartSleepCommand) | ✅ |
| `Assets/Game/Core/Commands/CommandProcessor.cs` | `CommandProcessor` | `Submit(IGameCommand) → CommandResult` — stamp WorldTime, Validate→Execute, log lỗi qua GameLog | ✅ |
| `Assets/Game/Core/Commands/InventoryOwnerResolver.cs` | `InventoryOwnerResolver` (**public** static, đổi từ internal ở S6 để UI đọc được) | `TryResolve(ctx, ownerId, out inv)` — scheme: `"player"`, `"searchpoint:<id>"` (KHÔNG tự tạo state, fail nếu chưa `Rolled`), `"shelter_storage:<id>"` (lazy-create), `"location_dropped:<id>"` (lazy-create). Để dành `"npc:<id>"` | ✅ (OwnerResolverTests, 6 test) |
| `Assets/Game/Core/Commands/UseItemCommand.cs` | `UseItemCommand` | Giảm quantity item trong inventory actor, publish `InventoryChanged` | ✅ |
| `Assets/Game/Core/Commands/TransferItemCommand.cs` | `TransferItemCommand` | Chuyển item giữa 2 owner đã biết (S6: nay thật sự dùng được cross-owner nhờ resolver mở rộng); move nguyên instance nếu chuyển hết quantity (giữ Condition/Contamination/Wet), chỉ split khi chuyển một phần. Validate kiểm `InventoryRules.CanAccept` → fail `InventoryFull`; Execute publish thêm `ItemTransferred` | ✅ |
| `Assets/Game/Core/Commands/StartSleepCommand.cs` | `StartSleepCommand` | `ctx.Clock.FastForward(Minutes)` — **chưa có interrupt-on-event** (chờ Event System M3+) | ⬜ |
| `Assets/Game/Core/Commands/OpenSearchPointCommand.cs` (S6) | `OpenSearchPointCommand` | Validate: đúng location (`NotAtLocation`). Execute: roll MỘT LẦN qua stream "loot" (bỏ qua nếu đã `Rolled`), `OpenTimeMinutes>0` → FastForward, publish `SearchPointOpened`+`ContainerViewRequested` | ✅ (SearchPointTests) |
| `Assets/Game/Core/Commands/TaskCommands.cs` | `StartTaskCommand`, `CancelTaskCommand` (vẫn chỉ validate+flag, chờ Shelter Task S10+), **`BeginTravelCommand`** (S6: **đầy đủ** — validate adjacency 2 đầu route → `NotAtLocation` nếu sai; Execute: loadFactor theo Overload qua `InventoryRules.LoadFactorFor`, FastForward(ceil(minutes×factor)), đổi `CurrentLocationId`, publish `TravelStarted`/`TravelCompleted`) | ✅ (TravelTests, 5 test cho BeginTravel) |

## LastHope.Data

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Data/Definitions/DefinitionBase.cs` | `DefinitionBase` (abstract) | `Id`, `DisplayNameKey`, `DataVersion` | ⬜ |
| `Assets/Game/Data/Definitions/ItemDefinition.cs` | `ItemDefinition` | `Category`, `BaseWeightKg`, `BaseVolumeLiters`, `MaxStackSize`, `MaxDurability`, `WaterResistance`, `Tags`, **`TwoHandCarry`** (S5, đánh dấu vật cồng kềnh như `item_water_container_20l`) | 🟡 (qua ContentValidationTests) |
| `Assets/Game/Data/Definitions/LocationDefinition.cs` | `LocationDefinition` | `SearchPointIds`, `ConnectedRouteIds`, `SceneName` (S6: nay `SceneFlowController` dùng thật để load/unload scene) | 🟡 |
| `Assets/Game/Data/Definitions/RouteDefinition.cs` | `RouteDefinition` | `FromLocationId`, `ToLocationId`, `TravelMinutes` | 🟡 |
| `Assets/Game/Data/Definitions/SearchPointDefinition.cs` | `SearchPointDefinition` + `LootEntry` | `LocationId`, `OpenTimeMinutes` (mặc định 0 — search mở tức thì), `LootTable` (List\<LootEntry\>: ItemId/Weight/MinQuantity/MaxQuantity) | 🟡 |
| `Assets/Game/Data/BalanceConfig.cs` (S5) | `BalanceConfig`, `InventoryBalance`, `TravelBalance`, `NewGameBalance` | Object config duy nhất (không phải Definition list) — mọi hằng số tuning: capacity 15kg/25L, overload threshold 100%/130%, hard cap 150%, speed modifier, travel load factor, start location | ✅ (BalanceLoadTests) |
| `Assets/Game/Data/DefinitionRegistry.cs` | `DefinitionRegistry` | `DefinitionVersion`, **`Balance`** (S5), `Items/Locations/Routes/SearchPoints` (IReadOnlyDictionary), `TryGetItem/Location/Route/SearchPoint`. ⚠ Ctor đổi signature S5: thêm tham số `BalanceConfig` (vị trí thứ 2) | ✅ (qua DefinitionLoaderTests) |
| `Assets/Game/Data/DefinitionLoader.cs` | `DefinitionLoader` (static) | `Load(directoryPath) → DefinitionLoadResult{Success,Registry,Errors}`. Routing theo prefix file: `manifest.json`, `items_*.json`, `locations_*.json`, `routes_*.json`, `searchpoints_*.json`, **`balance.json`** (S5, object đơn — thiếu/lỗi parse thì fallback default, KHÔNG tính là lỗi). Gom TOÀN BỘ lỗi (duplicate id, dangling ref, missing id) — không fail-first | ✅ |

## LastHope.Systems

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Systems/Registry/GameServiceRegistry.cs` | `GameServiceRegistry` (static) | `Register<T>`, `Get<T>`, `TryGet<T>`, `Clear()` — service locator giới hạn, chỉ `GameBootstrapper` ghi | ⬜ |
| `Assets/Game/Systems/Boot/GameBootstrapper.cs` | `GameBootstrapper` (MonoBehaviour, sống trong `10_GamePersistent`) | Composition root: load Definitions từ `StreamingAssets/Definitions`, fail-fast nếu lỗi (dừng boot, `enabled=false`), tạo `WorldState` mới + seed, set `Player.CurrentLocationId = Balance.NewGame.StartLocationId` (S5), dựng toàn bộ Core service + **`InventorySystem`** (S5), đăng ký vào `GameServiceRegistry` | ⬜ (verify qua headless smoke test, chưa có PlayMode test) |
| `Assets/Game/Systems/Boot/SimulationDriver.cs` | `SimulationDriver` (MonoBehaviour) | Cầu nối Unity Time → Core: đọc service ở `Start()` (không phải `Awake()`, tránh phụ thuộc thứ tự component), `Update()` clamp delta 1s, gọi `SimulationClock.AccumulateRealSeconds` + `TickScheduler.Advance`. `DebugPaused`/`DebugTimeScale` cho tooling | ⬜ (verify qua headless smoke test 10s không exception) |
| `Assets/Game/Systems/Inventory/InventorySystem.cs` (S5) | `InventorySystem` (plain C#) | Nghe `InventoryChanged` (chỉ owner "player") → `InventoryRules.ComputeOverload` → nếu đổi thì set `Inventory.Overload` + publish `OverloadStateChanged`. `RecomputeAll()` gọi 1 lần lúc boot | ✅ (qua InventoryRulesTests) |
| `Assets/Game/Systems/Telemetry/TelemetryLogger.cs` (S6) | `TelemetryLogger` | JSONL `persistentDataPath/Telemetry/session_*.jsonl` (`File.AppendAllText`, không giữ file handle). Nghe `TravelStarted/Completed` (kèm carry load lúc về), `SearchPointOpened`, `ItemTransferred` (chỉ log khi đích = player → `item_collected`). `Log()` public cho UI gọi thêm (chưa dùng — `item_left_behind`/`inventory_open_time` để dành cho `ContainerPanel`/`InventoryPanel` sau) | ✅ (TelemetryTests) |

## LastHope.DebugTools (bổ sung S4)

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Panel/DebugPanel.cs` | `DebugPanel` (MonoBehaviour, OnGUI, F2) | Xem World Time, Fast-forward clock, Pause/TimeScale (qua `SimulationDriver`), Add Item (bypass Command Layer — cheat có ghi rõ), Save/Autosave + **nút Load theo từng slot từ `SaveService.ListSlots()`** (S5, thay ô nhập tay là chính — ô nhập tay vẫn còn làm fallback "Load (typed id)"), state tree dump (`WorldStateSerializer.Serialize`). Sau Load, copy field vào `GameContext.World` hiện có + **publish `WorldStateReloaded`** (S5, để `PlayerAvatarSync` áp lại vị trí) | ⬜ (chưa test tự động, chỉ verify code compile + wiring scene) |

## LastHope.Presentation

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Presentation/Camera/CameraRig.cs` | `CameraRig` | Orthographic iso cố định (pitch 35.264°/yaw 45°), zoom qua Input System action "Zoom". `SetTarget(Transform)`, `SetInputActions(InputActionAsset)` | ⬜ (chỉ headless smoke test, chưa unit test) |
| `Assets/Game/Presentation/Player/PlayerController.cs` | `PlayerController` | CharacterController, di chuyển theo hướng camera (screen-relative), framerate-độc lập. `SpeedModifier` (S5: nay được `PlayerAvatarSync` set theo `OverloadStateChanged`), `SetCameraTransform`, `SetInputActions` | ⬜ |
| `Assets/Game/Presentation/Player/PlayerAvatarSync.cs` (S5) | `PlayerAvatarSync` | **Presentation-write exemption** có chủ đích: `LateUpdate` ghi transform→`PlayerState.PositionX/Y/Z` mỗi frame. **KHÔNG tự ghi `PositionLocationId`** (fix bug rơi-khỏi-map 2026-07-24 — field này chỉ được `SceneFlowController` stamp sau khi đặt spawn xong, tránh race). Nghe `WorldStateReloaded` → teleport lại nếu `PositionLocationId==CurrentLocationId` (khác thì để yên, `SceneFlowController` lo). Nghe `OverloadStateChanged` → set `PlayerController.SpeedModifier` | ⬜ |
| `Assets/Game/Presentation/Interaction/IInteractable.cs` (S5) | `IInteractable` | `PromptText`, `CanInteract(ctx)`, `Interact(ctx,processor)` — tương tác tức thì (E), docs không spec hold-duration | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionDetector.cs` (S5) | `InteractionDetector` | `OverlapSphere` bán kính 1.6m mỗi 0.15s + cursor raycast tiebreak (ưu tiên object con trỏ trỏ vào). Đọc action "Interact" có sẵn, `Current`/`TargetChanged`. S6: nay có interactable thật (SearchPointView×6, ShelterStorageView, TravelPointView×2) | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionPrompt.cs` (S5) | `InteractionPrompt` | TextMeshProUGUI "E — {prompt}", nghe `InteractionDetector.TargetChanged` | ⬜ |
| `Assets/Game/Presentation/World/SearchPointView.cs` (S6) | `SearchPointView` (`IInteractable`) | `[SerializeField] searchPointId` bind với `SearchPointDefinition`. Interact → `OpenSearchPointCommand` | ⬜ |
| `Assets/Game/Presentation/World/ShelterStorageView.cs` (S6) | `ShelterStorageView` (`IInteractable`) | `[SerializeField] shelterId`. Interact → publish `ContainerViewRequested` trực tiếp (không cần command để "nhìn") | ⬜ |
| `Assets/Game/Presentation/World/TravelPointView.cs` (S6) | `TravelPointView` (`IInteractable`) | `[SerializeField] routeId`. Interact → submit `BeginTravelCommand` thẳng (S8 sẽ đổi thành mở WorldMapPanel để chọn route) | ⬜ |
| `Assets/Game/Presentation/World/PlayerSpawnPoint.cs` (S6) | `PlayerSpawnPoint` | Marker rỗng — `SceneFlowController` dùng khi không có vị trí save khớp scene mới | ⬜ |
| `Assets/Game/Presentation/Boot/BootLoader.cs` | `BootLoader` (MonoBehaviour, sống trong `00_Boot`) | S6: chỉ load `10_GamePersistent` additive — KHÔNG còn hard-code scene gameplay thứ 2 (đó là việc của `SceneFlowController`) | ⬜ |
| `Assets/Game/Presentation/Boot/GamePersistentMarker.cs` | `GamePersistentMarker` | `DontDestroyOnLoad` cho root scene `10_GamePersistent`, chặn instance thứ 2 | ⬜ |
| `Assets/Game/Presentation/Boot/SceneFlowController.cs` (S6) | `SceneFlowController` | Chủ sở hữu DUY NHẤT vòng đời scene gameplay: nghe `TravelCompleted`/`WorldStateReloaded` → load additive scene theo `LocationDefinition.SceneName` của `Player.CurrentLocationId`, unload scene cũ, đặt player ở `PlayerSpawnPoint` nếu `PositionLocationId != CurrentLocationId`. **Là nơi DUY NHẤT được phép stamp `player.PositionLocationId = CurrentLocationId`** sau khi đặt xong (fix bug rơi-khỏi-map — xem PlayerAvatarSync). Log ở mỗi bước, verify qua headless smoke: `"placed player at spawn (0.00, 0.10, 0.00) for 'location_shelter'"` | ⬜ (verify qua smoke test, chưa PlayMode test) |

## LastHope.UI

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/UI/Inventory/InventoryPanel.cs` (S5) | `InventoryPanel` | Panel code-built hoàn toàn (không prefab): list phẳng item + 2 thanh weight/volume màu theo Overload (xanh/cam/đỏ), nút **Use** (qua `UseItemCommand`). **Drop vẫn chưa làm** (owner `location_dropped:` đã tồn tại từ S6 nhưng UI Drop button chưa nối). Toggle phím I/Tab | ⬜ |
| `Assets/Game/UI/Container/ContainerPanel.cs` (S6) | `ContainerPanel` | 1 panel dùng chung search point + shelter storage. Nghe `ContainerViewRequested` → hiện list container (đọc qua `InventoryOwnerResolver`, chỉ đọc — mọi mutate qua `TransferItemCommand`): nút Take/Take All. Nếu owner là `shelter_storage:` → hiện thêm khối "Your Inventory" với nút Store (2 chiều) | ⬜ |

## LastHope.DebugTools

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `DebugOverlay` | OnGUI overlay, toggle **F1**: FPS, world position, build version. Tự tìm GameObject tag "Player" nếu chưa gán. **Chưa hiển thị World Clock/State** (đó là `DebugPanel` ở S4, phím F2) | ⬜ |

## LastHope.EditorTools (Editor-only, không build vào Player)

| File | Class | API chính | Ghi chú |
| --- | --- | --- | --- |
| `Assets/Game/EditorTools/SceneSetup.cs` | `SceneSetup` | `[MenuItem] BuildAll()` — dựng lại 5 scene từ code, đăng ký Build Settings: `00_Boot`, `10_GamePersistent` (Player+Camera+HUD Canvas: EventSystem, InputSystemUIInputModule, InteractionPrompt, InventoryPanel, **ContainerPanel** S6, **SceneFlowController** S6), `90_TestSystems` (chỉ Ground/Light, không tự động load), **`Shelters/20_MainShelter`** (S6: ShelterStorageView "shelter_main", TravelPointView, PlayerSpawnPoint), **`Locations/41_Location_ConvenienceStore`** (S6: 6 SearchPointView đúng id JSON, TravelPointView, PlayerSpawnPoint) | Chạy lại bất cứ khi nào cần tái tạo scene từ đầu (deterministic) |
| `Assets/Game/EditorTools/RenderPipelineSetup.cs` | `RenderPipelineSetup` | `[MenuItem] Setup()` — tạo `Assets/Settings/LastHope_URP.asset` + Renderer, gán Graphics+Quality, Linear color space | Đã chạy 1 lần, asset đã tồn tại — chạy lại thì tái sử dụng asset cũ (idempotent) |
| `Assets/Game/EditorTools/BuildScript.cs` | `BuildScript` | `[MenuItem] BuildWindowsDevelopment()` → `Builds/Windows/LastHope.exe`, Mono, Development build | Dùng làm smoke test nhanh sau mỗi sprint |
| `Assets/Game/EditorTools/TmpSetup.cs` (S5) | `TmpSetup` | `[MenuItem] ImportEssentials()` — import "TMP Essential Resources.unitypackage" từ PackageCache (TMP Settings + LiberationSans SDF font). **Chạy KHÔNG kèm `-quit`** — `AssetDatabase.ImportPackage` là async, dùng callback `importPackageCompleted` để tự `EditorApplication.Exit` | Đã chạy 1 lần, asset đã có ở `Assets/TextMesh Pro/` — không cần chạy lại trừ khi asset đó bị xoá |

## Input

| File | Nội dung |
| --- | --- |
| `Assets/Input/GameControls.inputactions` | Action map "Gameplay": `Move` (Vector2, WASD composite), `Zoom` (Axis, scroll), `Interact` (Button, E — đọc bởi `InteractionDetector`, S5), **`ToggleInventory`** (Button, phím I/Tab — đọc bởi `InventoryPanel`, S5) |

## Data định nghĩa game — CONTENT THẬT + SCENE ĐÃ NỐI (S5+S6)

`Assets/StreamingAssets/Definitions/` — `manifest.json` (`definition_version: 0.2.0`) + `README.md` + content P1 thật:
- `items_p1.json` — 5 item: `item_water_bottle` (0.8kg/1.0L st4), `item_canned_food` (0.4/0.5 st8), `item_battery` (0.15/0.2 st10), `item_toolbox` (8/12 st1), `item_water_container_20l` (18/30 st1, `two_hand_carry:true`).
- `locations_p1.json` — `location_shelter` (scene `20_MainShelter`, **đã dựng S6**), `location_convenience_store` (scene `41_Location_ConvenienceStore`, **đã dựng S6**), nối bởi 1 route.
- `routes_p1.json` — `route_shelter_store` 25 phút game.
- `searchpoints_p1.json` — 6 điểm gắn với `location_convenience_store` (2 kệ nước, 2 kệ khô, quầy, kho — kho có `open_time_minutes:2`). **Đã có `SearchPointView` tương ứng trong `41_Location_ConvenienceStore`** (S6).
- `balance.json` — khớp default `BalanceConfig`.

✅ Từ S6: content này KHÔNG CÒN chỉ là dữ liệu — mỗi search point/route/location đều có GameObject/scene thật dùng nó (xác nhận qua headless smoke: boot → SceneFlowController load đúng `20_MainShelter`).

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems` → `Shelters/20_MainShelter` → `Locations/41_Location_ConvenienceStore`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- **M1 (S2-S4) PASS**, **S5 PASS**, **S6 PASS → Gate P1 PASS về mặt kỹ thuật (2026-07-24)**: 48/48 EditMode test, build Windows 0 lỗi, headless smoke 12s xác nhận SceneFlowController chuyển scene thành công.
- `DebugOverlay` (F1, Sprint 1) và `DebugPanel` (F2, Sprint 4) là 2 file khác nhau — F1 luôn hiện (FPS/vị trí), F2 toggle riêng (World Time/Save/Add Item/Travel cheat/state dump).
- `StartTaskCommand`/`CancelTaskCommand` vẫn chỉ validate + ghi flag/log, KHÔNG có effect thật (task không tốn resource) — đó là việc của Shelter Task (S10+). `BeginTravelCommand` thì ĐÃ đầy đủ từ S6.
- `StartSleepCommand` fast-forward clock nhưng KHÔNG kiểm tra event/interrupt (Event System chưa tồn tại).
- **Chưa playtest thật bằng tay/mắt** — mọi xác nhận ở trên là test tự động + headless smoke (không render hình). User nên tự mở Editor hoặc chạy `Builds/Windows/LastHope.exe` để chơi thử: Chuẩn bị (shelter) → Đi (route_shelter_store, phím E ở TravelPoint hoặc DebugPanel cheat) → Search (E ở 6 kệ trong cửa hàng) → chọn đồ (túi 15kg/25L không chứa hết được ~20kg/35L tổng) → Về → Cất (ShelterStorage) → Save/Load.
- `InventoryPanel` chưa có nút Drop dù owner `location_dropped:` đã hoạt động được (test qua OwnerResolverTests) — chỉ thiếu nút UI.
- `ContainerPanel`/`InventoryPanel` chưa gọi `TelemetryLogger.Log()` cho `item_left_behind`/`inventory_open_time` — TelemetryLogger có API sẵn (`Log(string,IDictionary)`), chỉ chưa được UI gọi tới.
- Toàn bộ hệ thống P2 (Condition/Status Effect/Hazard/Equipment/Return Window) **CHƯA VIẾT GÌ** — đó là S7-S9, xem `docs/plans/2026-07-24-p1-p2-completion-plan.md`.

## Ghi chú kỹ thuật quan trọng (tránh dò lại code để hiểu "tại sao")

- RNG dùng xorshift64* tự viết (không dùng `System.Random`) vì cần expose state để serialize và tiếp tục sequence bit-exact sau load — xem `RngStream.cs`.
- `DefinitionLoader` không ném exception cho lỗi data (chỉ throw nếu JSON không đọc được, và exception đó cũng bị bắt + gom vào `Errors`). Gọi `Load()` luôn trả về `DefinitionLoadResult`, không bao giờ throw ra ngoài với input hợp lệ về mặt cấu trúc file. `balance.json` là NGOẠI LỆ — thiếu/lỗi parse fallback default, không tính vào `Errors`.
- Naming JSON trên đĩa là **snake_case**, nhưng C# property là PascalCase — đừng thêm `[JsonProperty]` thủ công, `SnakeCaseNamingStrategy` tự convert.
- **TextMeshPro cần "TMP Essential Resources" import trước khi dùng** — nếu thiếu, mọi `TextMeshProUGUI.Awake()` throw NullReferenceException lúc runtime (không phải lúc compile!). Đã import (`Assets/TextMesh Pro/`), đừng xoá folder đó.
- `PlayerAvatarSync` là **ngoại lệ có chủ đích** của nguyên tắc "mọi thay đổi state qua Command" — vị trí liên tục (continuous data) không phải rule, ghi thẳng vào `PlayerState.Position*` mỗi frame. Đừng dùng pattern này cho state rời rạc khác.
- SceneSetup giờ tạo UI 100% bằng code (không prefab) — `InventoryPanel`/`InteractionPrompt` tự dựng hierarchy trong `Awake()`/`BuildLayout()`, không phụ thuộc asset `.prefab` nào.
