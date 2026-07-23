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

`00_Boot` (BootLoader) → additive `10_GamePersistent` (services + **Player/Camera/HUD Canvas sống suốt phiên**, từ S5) → additive gameplay scene đầu (`90_TestSystems` — nay chỉ còn Ground/ScaleRef/Light, KHÔNG còn Player/Camera).

---

## LastHope.Core

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Core/Logging/GameLog.cs` | `GameLog` (static) + `LogCategory` enum | `Info/Warn/Error(LogCategory, string)` | ⬜ |
| `Assets/Game/Core/State/WorldState.cs` | `WorldState` + stub `RouteState/LocationState/ShelterState/NpcState/ActiveEventState/ActiveTaskState` (mỗi cái chỉ `Id`+`StatusName`, sẽ mở rộng khi hệ thống tương ứng được viết) | Root state graph: `WorldTimeMinutes`, `CurrentDisasterPhase`, `RandomSeed`, `RngStreams`, `Player`, các Dictionary state theo id | ⬜ (gián tiếp qua RngServiceTests) |
| `Assets/Game/Core/State/PlayerState.cs` | `PlayerState` | `ActorId`, `CurrentLocationId`, `Inventory` | ⬜ |
| `Assets/Game/Core/State/InventoryState.cs` | `InventoryState` + `OverloadState` enum | `Items` (instanceId→ItemInstanceState), `CurrentWeightKg/VolumeLiters`, `Overload` (nay được `Systems.Inventory.InventorySystem` set, xem dưới) | ✅ (InventoryRulesTests) |
| `Assets/Game/Core/Rules/InventoryRules.cs` | `InventoryRules` (static) | `ComputeOverload(inv,balance)` (max(weight%,vol%), >100%→Light >130%→Heavy), `CanAccept(dest,defs,balance,itemId,qty)` (chặn ở 150% hard cap, container không giới hạn luôn true), `SpeedModifierFor(overload,balance)`, `IsCapacityLimited(ownerId)` (chỉ "player") | ✅ (8 test) |
| `Assets/Game/Core/State/ItemInstanceState.cs` | `ItemInstanceState` + `ContaminationState`/`WetState` enum | `InstanceId`, `ItemId`, `Quantity`, `Condition`, `Durability`, `Contamination`, `Wet`, `ContainerId` | ⬜ |
| `Assets/Game/Core/State/InventoryOps.cs` | `InventoryOps` (static) | `RecalculateLoad(inv, defs)` (chỉ tính tổng weight/volume, KHÔNG set Overload); `AddItem(inv, defs, itemId, qty, idGen)` (merge stack theo MaxStackSize, không kiểm capacity) | ⬜ |
| `Assets/Game/Core/Random/RngStream.cs` | `RngStream` + `RngStreamState` | xorshift64* trên state `ulong` mutable, `NextInt(min,maxExcl)`, `NextDouble()` | ✅ |
| `Assets/Game/Core/Random/RngService.cs` | `RngService` | `GetStream(name)` — named stream derive từ `WorldState.RandomSeed ⊕ FNV1a64(name)`, state sống trong `WorldState.RngStreams` | ✅ |
| `Assets/Game/Core/Save/WorldStateSerializer.cs` | `WorldStateSerializer` (static) | `Serialize(WorldState)` (indented), `SerializeCanonical(WorldState)` (Formatting.None, dùng cho checksum/deep-compare), `Deserialize(json)`, `Settings` (snake_case, StringEnumConverter, ObjectCreationHandling.Replace) | ✅ |
| `Assets/Game/Core/Save/SaveFile.cs` | `SaveFile`, `SaveSlotInfo` | `SaveFile{SaveVersion,DefinitionVersion,SavedAtUtc,Checksum,SlotId,World(JRaw)}` — World embed verbatim, không re-serialize | ✅ |
| `Assets/Game/Core/Save/SaveService.cs` | `SaveService`, `SaveResult`, `LoadResult` | `Autosave(world)` (round-robin autosave_0/1/2), `SaveToSlot(world,slotId)` (atomic: tmp→verify→backup cũ→rename), `Load(slotId)`, `ListSlots()`. Checksum SHA256 trên world payload canonical | ✅ |
| `Assets/Game/Core/Events/EventBus.cs` | `EventBus` (+ private `EventChannel<T>`) | `Subscribe<T>/Unsubscribe<T>/Publish<T>` — struct event, copy-on-write handler array, zero-boxing | ✅ (gián tiếp qua CommandPipelineTests) |
| `Assets/Game/Core/Events/GameEvents.cs` | `IGameEvent` + 11 struct: `WorldTimeChanged`, `DisasterPhaseChanged`, `RouteStateChanged`, `ShelterWarningRaised`, `TaskCompleted`, `EventDiscovered`, `InventoryChanged`, `NpcStateChanged`, `OverloadStateChanged` (S5), `WorldStateReloaded` (S5, publish sau khi DebugPanel Load), `ItemTransferred` (S5) | | ⬜ |
| `Assets/Game/Core/Time/GameTimeUtil.cs` | `GameTimeUtil` (static) | `DayIndex(m)`, `TimeOfDayMinutes(m)`, `Format(m)` — anchor Day 0 17:00 = phút 0 | ⬜ (gián tiếp qua TickSchedulerTests) |
| `Assets/Game/Core/Time/SimulationClock.cs` | `SimulationClock` | `AccumulateRealSeconds(double)`, `TryConsumeMinute()`, `PendingGameSeconds`. **Bank dùng `decimal` nội bộ** (không phải double) — double cộng dồn ~17k lần bị lệch 1 phút/24h, xem comment trong file | ✅ |
| `Assets/Game/Core/Time/TickScheduler.cs` | `TickScheduler` | `SubscribeShort/Long(Action<long>)`, `RegisterThreshold(minute, cb)`, `Advance(clock, maxMinutes)` (bounded catch-up), `FastForward(minutes)` (Sleep/Travel). `AdvanceOneMinute()` private — NƠI DUY NHẤT tăng `WorldTimeMinutes` | ✅ |
| `Assets/Game/Core/Commands/IGameCommand.cs` | `IGameCommand`, `CommandResult`, `CommandErrorCode`, `GameContext` | `GameContext{World,Definitions,Events,Rng,Clock}` — bundle inject duy nhất (đã thêm `Clock` so với plan gốc, cần cho StartSleepCommand) | ✅ |
| `Assets/Game/Core/Commands/CommandProcessor.cs` | `CommandProcessor` | `Submit(IGameCommand) → CommandResult` — stamp WorldTime, Validate→Execute, log lỗi qua GameLog | ✅ |
| `Assets/Game/Core/Commands/InventoryOwnerResolver.cs` | `InventoryOwnerResolver` (internal static) | `TryResolve(ctx, ownerId, out inv)` — **chỉ biết "player"** hiện tại, NPC/Shelter thêm sau khi hệ thống đó tồn tại | ⬜ |
| `Assets/Game/Core/Commands/UseItemCommand.cs` | `UseItemCommand` | Giảm quantity item trong inventory actor, publish `InventoryChanged` | ✅ |
| `Assets/Game/Core/Commands/TransferItemCommand.cs` | `TransferItemCommand` | Chuyển item giữa 2 owner đã biết; move nguyên instance nếu chuyển hết quantity (giữ Condition/Contamination/Wet), chỉ split khi chuyển một phần. Validate nay kiểm `InventoryRules.CanAccept` (S5) → fail `InventoryFull`; Execute publish thêm `ItemTransferred` | ✅ |
| `Assets/Game/Core/Commands/StartSleepCommand.cs` | `StartSleepCommand` | `ctx.Clock.FastForward(Minutes)` — **chưa có interrupt-on-event** (chờ Event System M3+) | ⬜ |
| `Assets/Game/Core/Commands/TaskCommands.cs` | `StartTaskCommand`, `CancelTaskCommand`, `BeginTravelCommand` | Validate + set flag stub (`ActiveTaskState`/log) — **body đầy đủ chưa viết**, chờ Shelter Task (S10+)/Travel (S6) | ⬜ |

## LastHope.Data

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Data/Definitions/DefinitionBase.cs` | `DefinitionBase` (abstract) | `Id`, `DisplayNameKey`, `DataVersion` | ⬜ |
| `Assets/Game/Data/Definitions/ItemDefinition.cs` | `ItemDefinition` | `Category`, `BaseWeightKg`, `BaseVolumeLiters`, `MaxStackSize`, `MaxDurability`, `WaterResistance`, `Tags`, **`TwoHandCarry`** (S5, đánh dấu vật cồng kềnh như `item_water_container_20l`) | 🟡 (qua ContentValidationTests) |
| `Assets/Game/Data/Definitions/LocationDefinition.cs` | `LocationDefinition` | `SearchPointIds`, `ConnectedRouteIds`, **`SceneName`** (S5, chờ S6 SceneFlowController dùng) | 🟡 |
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

## LastHope.DebugTools (bổ sung S4)

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Panel/DebugPanel.cs` | `DebugPanel` (MonoBehaviour, OnGUI, F2) | Xem World Time, Fast-forward clock, Pause/TimeScale (qua `SimulationDriver`), Add Item (bypass Command Layer — cheat có ghi rõ), Save/Autosave + **nút Load theo từng slot từ `SaveService.ListSlots()`** (S5, thay ô nhập tay là chính — ô nhập tay vẫn còn làm fallback "Load (typed id)"), state tree dump (`WorldStateSerializer.Serialize`). Sau Load, copy field vào `GameContext.World` hiện có + **publish `WorldStateReloaded`** (S5, để `PlayerAvatarSync` áp lại vị trí) | ⬜ (chưa test tự động, chỉ verify code compile + wiring scene) |

## LastHope.Presentation

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Presentation/Camera/CameraRig.cs` | `CameraRig` | Orthographic iso cố định (pitch 35.264°/yaw 45°), zoom qua Input System action "Zoom". `SetTarget(Transform)`, `SetInputActions(InputActionAsset)` | ⬜ (chỉ headless smoke test, chưa unit test) |
| `Assets/Game/Presentation/Player/PlayerController.cs` | `PlayerController` | CharacterController, di chuyển theo hướng camera (screen-relative), framerate-độc lập. `SpeedModifier` (S5: nay được `PlayerAvatarSync` set theo `OverloadStateChanged`), `SetCameraTransform`, `SetInputActions` | ⬜ |
| `Assets/Game/Presentation/Player/PlayerAvatarSync.cs` (S5) | `PlayerAvatarSync` | **Presentation-write exemption** có chủ đích: `LateUpdate` ghi transform→`PlayerState.PositionX/Y/Z/PositionLocationId` mỗi frame (fix bug "save không lưu vị trí"). Nghe `WorldStateReloaded` → teleport lại nếu `PositionLocationId==CurrentLocationId` (khác thì để yên, chờ S6 PlayerSpawnPoint). Nghe `OverloadStateChanged` → set `PlayerController.SpeedModifier` | ⬜ |
| `Assets/Game/Presentation/Interaction/IInteractable.cs` (S5) | `IInteractable` | `PromptText`, `CanInteract(ctx)`, `Interact(ctx,processor)` — tương tác tức thì (E), docs không spec hold-duration | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionDetector.cs` (S5) | `InteractionDetector` | `OverlapSphere` bán kính 1.6m mỗi 0.15s + cursor raycast tiebreak (ưu tiên object con trỏ trỏ vào). Đọc action "Interact" có sẵn, `Current`/`TargetChanged`. **Chưa có interactable thật trong scene** (chờ S6 SearchPointView...) | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionPrompt.cs` (S5) | `InteractionPrompt` | TextMeshProUGUI "E — {prompt}", nghe `InteractionDetector.TargetChanged` | ⬜ |
| `Assets/Game/Presentation/Boot/BootLoader.cs` | `BootLoader` (MonoBehaviour, sống trong `00_Boot`) | Load `10_GamePersistent` rồi `90_TestSystems` (additive, tuần tự). Scene gameplay đầu vẫn hard-code "90_TestSystems" — S6 sẽ đổi qua `SceneFlowController` | ⬜ |
| `Assets/Game/Presentation/Boot/GamePersistentMarker.cs` | `GamePersistentMarker` | `DontDestroyOnLoad` cho root scene `10_GamePersistent`, chặn instance thứ 2 | ⬜ |

## LastHope.UI

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/UI/Inventory/InventoryPanel.cs` (S5) | `InventoryPanel` | Panel code-built hoàn toàn (không prefab): list phẳng item + 2 thanh weight/volume màu theo Overload (xanh/cam/đỏ), nút **Use** (qua `UseItemCommand`). **Drop chưa có** (chờ S6 `location_dropped:` owner). Toggle phím I/Tab | ⬜ |

## LastHope.DebugTools

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `DebugOverlay` | OnGUI overlay, toggle **F1**: FPS, world position, build version. Tự tìm GameObject tag "Player" nếu chưa gán. **Chưa hiển thị World Clock/State** (đó là `DebugPanel` ở S4, phím F2) | ⬜ |

## LastHope.EditorTools (Editor-only, không build vào Player)

| File | Class | API chính | Ghi chú |
| --- | --- | --- | --- |
| `Assets/Game/EditorTools/SceneSetup.cs` | `SceneSetup` | `[MenuItem] BuildAll()` — dựng lại `00_Boot`/`10_GamePersistent`/`90_TestSystems` từ code, đăng ký Build Settings. **S5: Player+Camera+HUD Canvas (EventSystem, InputSystemUIInputModule, InteractionPrompt, InventoryPanel) nay dựng trong `10_GamePersistent`**, không còn trong `90_TestSystems` | Chạy lại bất cứ khi nào cần tái tạo scene từ đầu (deterministic) |
| `Assets/Game/EditorTools/RenderPipelineSetup.cs` | `RenderPipelineSetup` | `[MenuItem] Setup()` — tạo `Assets/Settings/LastHope_URP.asset` + Renderer, gán Graphics+Quality, Linear color space | Đã chạy 1 lần, asset đã tồn tại — chạy lại thì tái sử dụng asset cũ (idempotent) |
| `Assets/Game/EditorTools/BuildScript.cs` | `BuildScript` | `[MenuItem] BuildWindowsDevelopment()` → `Builds/Windows/LastHope.exe`, Mono, Development build | Dùng làm smoke test nhanh sau mỗi sprint |
| `Assets/Game/EditorTools/TmpSetup.cs` (S5) | `TmpSetup` | `[MenuItem] ImportEssentials()` — import "TMP Essential Resources.unitypackage" từ PackageCache (TMP Settings + LiberationSans SDF font). **Chạy KHÔNG kèm `-quit`** — `AssetDatabase.ImportPackage` là async, dùng callback `importPackageCompleted` để tự `EditorApplication.Exit` | Đã chạy 1 lần, asset đã có ở `Assets/TextMesh Pro/` — không cần chạy lại trừ khi asset đó bị xoá |

## Input

| File | Nội dung |
| --- | --- |
| `Assets/Input/GameControls.inputactions` | Action map "Gameplay": `Move` (Vector2, WASD composite), `Zoom` (Axis, scroll), `Interact` (Button, E — đọc bởi `InteractionDetector`, S5), **`ToggleInventory`** (Button, phím I/Tab — đọc bởi `InventoryPanel`, S5) |

## Data định nghĩa game — CONTENT THẬT ĐÃ CÓ (S5)

`Assets/StreamingAssets/Definitions/` — `manifest.json` (`definition_version: 0.2.0`) + `README.md` + content P1 thật:
- `items_p1.json` — 5 item: `item_water_bottle` (0.8kg/1.0L st4), `item_canned_food` (0.4/0.5 st8), `item_battery` (0.15/0.2 st10), `item_toolbox` (8/12 st1), `item_water_container_20l` (18/30 st1, `two_hand_carry:true`).
- `locations_p1.json` — `location_shelter` (scene `20_MainShelter`, chưa dựng — S6), `location_convenience_store` (scene `41_Location_ConvenienceStore`, chưa dựng — S6), nối bởi 1 route.
- `routes_p1.json` — `route_shelter_store` 25 phút game.
- `searchpoints_p1.json` — 6 điểm gắn với `location_convenience_store` (2 kệ nước, 2 kệ khô, quầy, kho — kho có `open_time_minutes:2`). **Chưa có GameObject nào trong scene tham chiếu các id này** — chờ S6 `SearchPointView`.
- `balance.json` — khớp default `BalanceConfig` (xem BalanceConfig.cs ở trên).

⚠ Registry hiện tại LOAD ĐƯỢC content này (ContentValidationTests xác nhận 0 lỗi, đúng count) nhưng **không có scene/GameObject nào dùng tới** — search point/location vẫn chỉ là dữ liệu, chưa có gameplay thật cho tới S6.

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- **M1 (S2-S4) PASS**, **S5 PASS (2026-07-24)**: 32/32 EditMode test, build Windows 0 lỗi, headless smoke 10s không exception.
- `DebugOverlay` (F1, Sprint 1) và `DebugPanel` (F2, Sprint 4) là 2 file khác nhau — F1 luôn hiện (FPS/vị trí), F2 toggle riêng (World Time/Save/Add Item/state dump).
- `InventoryOwnerResolver` chỉ nhận biết owner id = `"player"`. Gọi `TransferItemCommand`/`UseItemCommand` với owner id khác sẽ luôn fail validation `InvalidActor` — chờ S6 mở owner `searchpoint:`/`shelter_storage:`/`location_dropped:`.
- `StartTaskCommand`/`CancelTaskCommand`/`BeginTravelCommand` chỉ validate + ghi flag/log, KHÔNG có effect thật (task không tốn resource, travel không đổi scene/tiêu thời gian) — đó là việc của S6 (Travel) và S10+ (Shelter Task).
- `StartSleepCommand` fast-forward clock nhưng KHÔNG kiểm tra event/interrupt (Event System chưa tồn tại).
- `InteractionDetector` đã chạy nhưng **chưa có interactable thật nào trong scene** — `Current` sẽ luôn null cho tới khi S6 thêm `SearchPointView`/`ShelterStorageView`/`TravelPointView`.
- Content JSON P1 đã load được (0 lỗi) nhưng **chưa có scene/blockout dùng tới** — `20_MainShelter` và `41_Location_ConvenienceStore` (khai trong `SceneName`) chưa tồn tại, chờ S6.
- `WorldStateSerializer` chưa có test roundtrip qua file thật ngoài `SaveRoundTripTests` (S4) — S5 thêm `PlayerPositionSaveTests` xác nhận field vị trí sống qua serialize.
- `BeginTravelCommand` (TaskCommands.cs) chưa dùng `TravelBalance.LoadFactor*` — sẽ nối ở S6.

## Ghi chú kỹ thuật quan trọng (tránh dò lại code để hiểu "tại sao")

- RNG dùng xorshift64* tự viết (không dùng `System.Random`) vì cần expose state để serialize và tiếp tục sequence bit-exact sau load — xem `RngStream.cs`.
- `DefinitionLoader` không ném exception cho lỗi data (chỉ throw nếu JSON không đọc được, và exception đó cũng bị bắt + gom vào `Errors`). Gọi `Load()` luôn trả về `DefinitionLoadResult`, không bao giờ throw ra ngoài với input hợp lệ về mặt cấu trúc file. `balance.json` là NGOẠI LỆ — thiếu/lỗi parse fallback default, không tính vào `Errors`.
- Naming JSON trên đĩa là **snake_case**, nhưng C# property là PascalCase — đừng thêm `[JsonProperty]` thủ công, `SnakeCaseNamingStrategy` tự convert.
- **TextMeshPro cần "TMP Essential Resources" import trước khi dùng** — nếu thiếu, mọi `TextMeshProUGUI.Awake()` throw NullReferenceException lúc runtime (không phải lúc compile!). Đã import (`Assets/TextMesh Pro/`), đừng xoá folder đó.
- `PlayerAvatarSync` là **ngoại lệ có chủ đích** của nguyên tắc "mọi thay đổi state qua Command" — vị trí liên tục (continuous data) không phải rule, ghi thẳng vào `PlayerState.Position*` mỗi frame. Đừng dùng pattern này cho state rời rạc khác.
- SceneSetup giờ tạo UI 100% bằng code (không prefab) — `InventoryPanel`/`InteractionPrompt` tự dựng hierarchy trong `Awake()`/`BuildLayout()`, không phụ thuộc asset `.prefab` nào.
