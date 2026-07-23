# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Assembly map (dependency một chiều)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Presentation, DebugTools, Unity.InputSystem, URP Runtime
```

## Scene flow

`00_Boot` (BootLoader) → additive `10_GamePersistent` (services sống suốt phiên) → additive gameplay scene đầu (`90_TestSystems` ở Sprint 1).

---

## LastHope.Core

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Core/Logging/GameLog.cs` | `GameLog` (static) + `LogCategory` enum | `Info/Warn/Error(LogCategory, string)` | ⬜ |
| `Assets/Game/Core/State/WorldState.cs` | `WorldState` + stub `RouteState/LocationState/ShelterState/NpcState/ActiveEventState/ActiveTaskState` (mỗi cái chỉ `Id`+`StatusName`, sẽ mở rộng khi hệ thống tương ứng được viết) | Root state graph: `WorldTimeMinutes`, `CurrentDisasterPhase`, `RandomSeed`, `RngStreams`, `Player`, các Dictionary state theo id | ⬜ (gián tiếp qua RngServiceTests) |
| `Assets/Game/Core/State/PlayerState.cs` | `PlayerState` | `ActorId`, `CurrentLocationId`, `Inventory` | ⬜ |
| `Assets/Game/Core/State/InventoryState.cs` | `InventoryState` + `OverloadState` enum | `Items` (instanceId→ItemInstanceState), `CurrentWeightKg/VolumeLiters`, `Overload`. **Overload chưa được set bởi ai** — capacity/overload rule thuộc `Systems.Inventory` (S5), chưa viết | ⬜ |
| `Assets/Game/Core/State/ItemInstanceState.cs` | `ItemInstanceState` + `ContaminationState`/`WetState` enum | `InstanceId`, `ItemId`, `Quantity`, `Condition`, `Durability`, `Contamination`, `Wet`, `ContainerId` | ⬜ |
| `Assets/Game/Core/State/InventoryOps.cs` | `InventoryOps` (static) | `RecalculateLoad(inv, defs)` (chỉ tính tổng weight/volume, KHÔNG set Overload); `AddItem(inv, defs, itemId, qty, idGen)` (merge stack theo MaxStackSize, không kiểm capacity) | ⬜ |
| `Assets/Game/Core/Random/RngStream.cs` | `RngStream` + `RngStreamState` | xorshift64* trên state `ulong` mutable, `NextInt(min,maxExcl)`, `NextDouble()` | ✅ |
| `Assets/Game/Core/Random/RngService.cs` | `RngService` | `GetStream(name)` — named stream derive từ `WorldState.RandomSeed ⊕ FNV1a64(name)`, state sống trong `WorldState.RngStreams` | ✅ |
| `Assets/Game/Core/Save/WorldStateSerializer.cs` | `WorldStateSerializer` (static) | `Serialize(WorldState)` (indented), `SerializeCanonical(WorldState)` (Formatting.None, dùng cho checksum/deep-compare), `Deserialize(json)`, `Settings` (snake_case, StringEnumConverter, ObjectCreationHandling.Replace) | ⬜ (chưa có SaveRoundTripTests — chờ S4 khi SaveService tồn tại) |
| `Assets/Game/Core/Events/EventBus.cs` | `EventBus` (+ private `EventChannel<T>`) | `Subscribe<T>/Unsubscribe<T>/Publish<T>` — struct event, copy-on-write handler array, zero-boxing | ✅ (gián tiếp qua CommandPipelineTests) |
| `Assets/Game/Core/Events/GameEvents.cs` | `IGameEvent` + 8 struct: `WorldTimeChanged`, `DisasterPhaseChanged`, `RouteStateChanged`, `ShelterWarningRaised`, `TaskCompleted`, `EventDiscovered`, `InventoryChanged`, `NpcStateChanged` | | ⬜ |
| `Assets/Game/Core/Time/GameTimeUtil.cs` | `GameTimeUtil` (static) | `DayIndex(m)`, `TimeOfDayMinutes(m)`, `Format(m)` — anchor Day 0 17:00 = phút 0 | ⬜ (gián tiếp qua TickSchedulerTests) |
| `Assets/Game/Core/Time/SimulationClock.cs` | `SimulationClock` | `AccumulateRealSeconds(double)`, `TryConsumeMinute()`, `PendingGameSeconds`. **Bank dùng `decimal` nội bộ** (không phải double) — double cộng dồn ~17k lần bị lệch 1 phút/24h, xem comment trong file | ✅ |
| `Assets/Game/Core/Time/TickScheduler.cs` | `TickScheduler` | `SubscribeShort/Long(Action<long>)`, `RegisterThreshold(minute, cb)`, `Advance(clock, maxMinutes)` (bounded catch-up), `FastForward(minutes)` (Sleep/Travel). `AdvanceOneMinute()` private — NƠI DUY NHẤT tăng `WorldTimeMinutes` | ✅ |
| `Assets/Game/Core/Commands/IGameCommand.cs` | `IGameCommand`, `CommandResult`, `CommandErrorCode`, `GameContext` | `GameContext{World,Definitions,Events,Rng,Clock}` — bundle inject duy nhất (đã thêm `Clock` so với plan gốc, cần cho StartSleepCommand) | ✅ |
| `Assets/Game/Core/Commands/CommandProcessor.cs` | `CommandProcessor` | `Submit(IGameCommand) → CommandResult` — stamp WorldTime, Validate→Execute, log lỗi qua GameLog | ✅ |
| `Assets/Game/Core/Commands/InventoryOwnerResolver.cs` | `InventoryOwnerResolver` (internal static) | `TryResolve(ctx, ownerId, out inv)` — **chỉ biết "player"** hiện tại, NPC/Shelter thêm sau khi hệ thống đó tồn tại | ⬜ |
| `Assets/Game/Core/Commands/UseItemCommand.cs` | `UseItemCommand` | Giảm quantity item trong inventory actor, publish `InventoryChanged` | ✅ |
| `Assets/Game/Core/Commands/TransferItemCommand.cs` | `TransferItemCommand` | Chuyển item giữa 2 owner đã biết; move nguyên instance nếu chuyển hết quantity (giữ Condition/Contamination/Wet), chỉ split khi chuyển một phần | ✅ |
| `Assets/Game/Core/Commands/StartSleepCommand.cs` | `StartSleepCommand` | `ctx.Clock.FastForward(Minutes)` — **chưa có interrupt-on-event** (chờ Event System M3+) | ⬜ |
| `Assets/Game/Core/Commands/TaskCommands.cs` | `StartTaskCommand`, `CancelTaskCommand`, `BeginTravelCommand` | Validate + set flag stub (`ActiveTaskState`/log) — **body đầy đủ chưa viết**, chờ Shelter Task (S10+)/Travel (S6) | ⬜ |

## LastHope.Data

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Data/Definitions/DefinitionBase.cs` | `DefinitionBase` (abstract) | `Id`, `DisplayNameKey`, `DataVersion` | ⬜ |
| `Assets/Game/Data/Definitions/ItemDefinition.cs` | `ItemDefinition` | `Category`, `BaseWeightKg`, `BaseVolumeLiters`, `MaxStackSize`, `MaxDurability`, `WaterResistance`, `Tags` | 🟡 (qua fixture) |
| `Assets/Game/Data/Definitions/LocationDefinition.cs` | `LocationDefinition` | `SearchPointIds`, `ConnectedRouteIds` | 🟡 |
| `Assets/Game/Data/Definitions/RouteDefinition.cs` | `RouteDefinition` | `FromLocationId`, `ToLocationId`, `TravelMinutes` | 🟡 |
| `Assets/Game/Data/Definitions/SearchPointDefinition.cs` | `SearchPointDefinition` + `LootEntry` | `LocationId`, `OpenTimeMinutes` (mặc định 0 — search mở tức thì), `LootTable` (List\<LootEntry\>: ItemId/Weight/MinQuantity/MaxQuantity) | 🟡 |
| `Assets/Game/Data/DefinitionRegistry.cs` | `DefinitionRegistry` | `DefinitionVersion`, `Items/Locations/Routes/SearchPoints` (IReadOnlyDictionary), `TryGetItem/Location/Route/SearchPoint` | ✅ (qua DefinitionLoaderTests) |
| `Assets/Game/Data/DefinitionLoader.cs` | `DefinitionLoader` (static) | `Load(directoryPath) → DefinitionLoadResult{Success,Registry,Errors}`. Routing theo prefix file: `manifest.json`, `items_*.json`, `locations_*.json`, `routes_*.json`, `searchpoints_*.json`. Gom TOÀN BỘ lỗi (duplicate id, dangling ref, missing id) — không fail-first | ✅ |

## LastHope.Systems

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Systems/Registry/GameServiceRegistry.cs` | `GameServiceRegistry` (static) | `Register<T>`, `Get<T>`, `TryGet<T>`, `Clear()` — service locator giới hạn, chỉ `GameBootstrapper` ghi | ⬜ |
| `Assets/Game/Systems/Boot/GameBootstrapper.cs` | `GameBootstrapper` (MonoBehaviour, sống trong `10_GamePersistent`) | Composition root: load Definitions từ `StreamingAssets/Definitions`, fail-fast nếu lỗi (dừng boot, `enabled=false`), tạo `WorldState` mới + seed, dựng toàn bộ Core service, đăng ký vào `GameServiceRegistry` | ⬜ (verify qua headless smoke test, chưa có PlayMode test) |
| `Assets/Game/Systems/Boot/SimulationDriver.cs` | `SimulationDriver` (MonoBehaviour) | Cầu nối Unity Time → Core: đọc service ở `Start()` (không phải `Awake()`, tránh phụ thuộc thứ tự component), `Update()` clamp delta 1s, gọi `SimulationClock.AccumulateRealSeconds` + `TickScheduler.Advance`. `DebugPaused`/`DebugTimeScale` cho tooling | ⬜ |

## LastHope.Presentation

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Presentation/Camera/CameraRig.cs` | `CameraRig` | Orthographic iso cố định (pitch 35.264°/yaw 45°), zoom qua Input System action "Zoom". `SetTarget(Transform)`, `SetInputActions(InputActionAsset)` | ⬜ (chỉ headless smoke test, chưa unit test) |
| `Assets/Game/Presentation/Player/PlayerController.cs` | `PlayerController` | CharacterController, di chuyển theo hướng camera (screen-relative), framerate-độc lập. `SpeedModifier` (hook cho Carry Load/Flood sau này), `SetCameraTransform`, `SetInputActions` | ⬜ |
| `Assets/Game/Presentation/Boot/BootLoader.cs` | `BootLoader` (MonoBehaviour, sống trong `00_Boot`) | Load `10_GamePersistent` rồi `90_TestSystems` (additive, tuần tự). Chưa load Definition Data / World State (sẽ nối vào `GameBootstrapper` ở S3) | ⬜ |
| `Assets/Game/Presentation/Boot/GamePersistentMarker.cs` | `GamePersistentMarker` | `DontDestroyOnLoad` cho root scene `10_GamePersistent`, chặn instance thứ 2 | ⬜ |

## LastHope.UI

Chưa có class nào (asmdef trống).

## LastHope.DebugTools

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `DebugOverlay` | OnGUI overlay, toggle **F1**: FPS, world position, build version. Tự tìm GameObject tag "Player" nếu chưa gán. **Chưa hiển thị World Clock/State** (đó là `DebugPanel` ở S4, phím F2) | ⬜ |

## LastHope.EditorTools (Editor-only, không build vào Player)

| File | Class | API chính | Ghi chú |
| --- | --- | --- | --- |
| `Assets/Game/EditorTools/SceneSetup.cs` | `SceneSetup` | `[MenuItem] BuildAll()` — dựng lại `00_Boot`/`10_GamePersistent`/`90_TestSystems` từ code, đăng ký Build Settings | Chạy lại bất cứ khi nào cần tái tạo scene từ đầu (deterministic) |
| `Assets/Game/EditorTools/RenderPipelineSetup.cs` | `RenderPipelineSetup` | `[MenuItem] Setup()` — tạo `Assets/Settings/LastHope_URP.asset` + Renderer, gán Graphics+Quality, Linear color space | Đã chạy 1 lần, asset đã tồn tại — chạy lại thì tái sử dụng asset cũ (idempotent) |
| `Assets/Game/EditorTools/BuildScript.cs` | `BuildScript` | `[MenuItem] BuildWindowsDevelopment()` → `Builds/Windows/LastHope.exe`, Mono, Development build | Dùng làm smoke test nhanh sau mỗi sprint |

## Input

| File | Nội dung |
| --- | --- |
| `Assets/Input/GameControls.inputactions` | Action map "Gameplay": `Move` (Vector2, WASD composite), `Zoom` (Axis, scroll), `Interact` (Button, E — **chưa có code nào đọc action này**, chờ S5 Interaction System) |

## Data định nghĩa game (chưa có content thật)

`Assets/StreamingAssets/Definitions/` — có `manifest.json` (`definition_version: 0.1.0`, cần để `GameBootstrapper` load được registry rỗng hợp lệ lúc boot) + `README.md`. **Chưa có** `items_*.json`/`locations_*.json`/`routes_*.json`/`searchpoints_*.json` thật — Registry hiện tại rỗng (0 item/location/route/searchpoint). Content P1 thật sẽ thêm ở S5.

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- **Chưa có** Save (SaveFile/SaveService) và Debug Panel v1 — đó là S4. Command/EventBus/Tick/GameBootstrapper đã xong và **đang chạy thật** trong `10_GamePersistent` (verify qua headless smoke test: boot → load definitions → tạo WorldState → log seed, không exception).
- `DebugOverlay` (F1) là overlay tối thiểu Sprint 1, KHÔNG phải Debug Panel v1 (F2, sẽ thêm state tree + save/load ở S4) — hai file khác nhau, đừng nhầm.
- `InventoryOwnerResolver` chỉ nhận biết owner id = `"player"`. Gọi `TransferItemCommand`/`UseItemCommand` với owner id khác sẽ luôn fail validation `InvalidActor` — không phải bug, chỉ là NPC/Shelter storage chưa tồn tại.
- `StartTaskCommand`/`CancelTaskCommand`/`BeginTravelCommand` chỉ validate + ghi flag/log, KHÔNG có effect thật (task không tốn resource, travel không đổi scene/tiêu thời gian) — đó là việc của S6 (Travel) và S10+ (Shelter Task).
- `StartSleepCommand` fast-forward clock nhưng KHÔNG kiểm tra event/interrupt (Event System chưa tồn tại).
- `PlayerController.SpeedModifier` tồn tại nhưng chưa có hệ thống nào set nó (Carry Load/Flood ở M2/P2).
- Input action "Interact" (E) đã khai báo trong `.inputactions` nhưng chưa có script nào subscribe.
- `InventoryOps.RecalculateLoad` chỉ tính tổng, **không set `Overload`** — capacity 15kg/25L trong bảng baseline chưa được code ở đâu cả, sẽ vào `Systems/Inventory/InventorySystem.cs` (S5).
- Chưa có Content JSON thật trong `Assets/StreamingAssets/Definitions/` (vẫn chỉ có README) — 5 file fixture test nằm ở `Assets/Tests/EditMode/Fixtures/{valid,invalid}_definitions/`, KHÔNG phải data thật của game.
- `WorldStateSerializer` chưa có test roundtrip qua file thật (chỉ dùng gián tiếp nếu có) — `SaveRoundTripTests` ở S4 sẽ là bài test đầu tiên chạy nó qua `SaveService`.

## Ghi chú kỹ thuật quan trọng (tránh dò lại code để hiểu "tại sao")

- RNG dùng xorshift64* tự viết (không dùng `System.Random`) vì cần expose state để serialize và tiếp tục sequence bit-exact sau load — xem `RngStream.cs`.
- `DefinitionLoader` không ném exception cho lỗi data (chỉ throw nếu JSON không đọc được, và exception đó cũng bị bắt + gom vào `Errors`). Gọi `Load()` luôn trả về `DefinitionLoadResult`, không bao giờ throw ra ngoài với input hợp lệ về mặt cấu trúc file.
- Naming JSON trên đĩa là **snake_case**, nhưng C# property là PascalCase — đừng thêm `[JsonProperty]` thủ công, `SnakeCaseNamingStrategy` tự convert.
