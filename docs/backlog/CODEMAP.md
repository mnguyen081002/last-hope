# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Hiện trạng

**Gate P1 PASS**. **P2-A Player Condition Core xong** (user đã verify). **P2-B phần Flood
State xong** (124 EditMode test) — Current Strength/Electrified Water/Route Closure/
Disaster Phase **chưa làm** (balance.json không có số). P2-C chưa bắt đầu. Chưa có: P3/P4.

Verify pipeline: batchmode compile → EditMode test → sinh 5 scene (`SceneSetup.BuildAllScenes`)
→ build Windows → smoke test headless (boot → persistent → GameBootstrapper → SceneFlowController
load `20_MainShelter` theo `location_shelter` → tìm `PlayerSpawnPoint` → camera snap, 0 lỗi).

---

## Content data có sẵn

`Assets/StreamingAssets/Definitions/` — 18 file JSON, `definition_version 0.14.0`, dùng lại trực tiếp. Schema trên đĩa là **snake_case** — code Data layer phải đọc theo convention này (`SnakeCaseNamingStrategy`, xem `docs/plans/2026-07-24-mvp-coding-plan.md` mục "Quyết định kiến trúc khóa").

| File | Nội dung | Dùng ở sprint |
| --- | --- | --- |
| `manifest.json` | `definition_version` | Definition Registry (BL-P1-06) |
| `balance.json` | inventory cap/overload, travel load factor, condition rates, new_game | baseline số liệu, xuyên suốt |
| `items_p1.json`, `items_p2.json`, `items_p3_materials.json` | item def: weight/volume/stack/use_effects | P1, P2, P3 |
| `locations_p1.json`, `locations_p4.json` | location def | P1, P4 |
| `routes_p1.json`, `routes_p4.json` | route def | P1, P4 |
| `searchpoints_p1.json`, `searchpoints_p4.json` | search point + loot table | P1, P4 |
| `modules_p3.json`, `shelterzones_p3.json` | build module + shelter zone | P3 |
| `events_p3.json`, `events_p4.json`, `events_p4_minh.json` | event def | P3, P4 |
| `npcs_p4.json`, `phases_p4.json` | NPC + disaster phase timeline | P4 |

## Art có sẵn

`Assets/Art/` — **743 PNG sprite**. Đáng chú ý: `Production/Character8Direction/Frames/` (walk 8 hướng × 4 frame), `Production/Terrain*`, `Production/World*`, `Production/Loot*`. Prompt sinh asset: `docs/asset-prompts/`.

---

## Assembly map (đã dựng)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI
```

Dependency một chiều: `Data → Core → Systems → Presentation/UI/DebugTools`. **`Presentation`
và `UI` là hai nhánh song song, không phụ thuộc lẫn nhau** — liên lạc qua `EventBus`
(`SearchPointOpened`, `StorageOpened`), không gọi thẳng. Test assembly tham chiếu tất cả.
`EditorTools` thấy toàn bộ (Editor-only, không bị ràng buộc layering).

## Scene flow (đã dựng)

`00_Boot` → additive `10_GamePersistent` (GameServices/Player/Camera/UI panel sống suốt
phiên) → `SceneFlowController` (trong persistent scene) đọc `WorldState.Player.CurrentLocationId`
→ `LocationDefinition.SceneName`, load additive, đặt player tại `PlayerSpawnPoint` của scene
đó. Travel (`LocationChanged`) lặp lại quy trình, unload scene cũ. **Save/Load không đổi
scene** (scope cut P1 — xem `docs/plans/2026-07-27-p1c-exploration-gameplay.md`).

---

## LastHope.Core

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Logging | `Core/Diagnostics/GameLog.cs` | `Info/Warn/Error(LogCategory, msg)`, `Enabled` | ✅ | Error luôn ghi, không tắt được |
| RNG | `Core/Random/RngStream.cs`, `RngService.cs` | `Stream(name)`, `FlushState()`, `NextInt/NextChance` | ✅ | xorshift64*, stream đặt tên độc lập; **phải `FlushState()` trước khi save** |
| World State | `Core/State/WorldState.cs` | `WorldTimeMinutes`, `RngStreams`, `Player`, `Locations`, `Routes`, `GetOrCreateLocation/Route` | ✅ | Thứ duy nhất được serialize |
| Route state | `Core/State/RouteState.cs` | `FloodState` enum (Dry/Shallow/Medium/Deep/Impassable) | ✅ | Route chưa từng đổi = mặc định Dry (không có entry) |
| Inventory state | `Core/State/InventoryState.cs`, `ItemInstanceState.cs`, `InventoryOps.cs` | `AddItem/RemoveItem/CountOf/TotalWeightKg/Move` | ✅ | Nhận `List<ItemInstanceState>` (dùng chung player/storage/searchpoint) + overload giữ API `InventoryState` cũ |
| Time | `Core/Time/SimulationClock.cs`, `TickScheduler.cs`, `GameTimeUtil.cs` | `AccumulateRealSeconds`, `Advance/FastForward`, `ShortTick/LongTick` | ✅ | `AdvanceOneMinute` là **nơi duy nhất** tăng `WorldTimeMinutes`; long tick mỗi 10 phút; anchor Day 0 17:00 |
| Events | `Core/Events/EventBus.cs`, `GameEvents.cs` | `Subscribe/Unsubscribe/Publish<T>` | 🟡 | struct event, handler copy-on-write. Có: `WorldTimeChanged`, `InventoryChanged`, `LocationChanged`, `SearchPointOpened`, `StorageOpened`, `TravelStarted`, `WorldStateReloaded` |
| Commands | `Core/Commands/IGameCommand.cs`, `CommandProcessor.cs`, `UseItemCommand.cs` | `Submit(command)` → `CommandResult`, `GameContext{World,Definitions,Events,Rng,Ticks}` | ✅ | Validate fail = không mutate. Command gameplay khác (Transfer/Search/Travel) ở `Systems/Commands` |
| Save | `Core/Save/WorldStateSerializer.cs`, `SaveFile.cs`, `SaveService.cs` | `Save/Load/SaveAutosave`, `PathForSlot` | ✅ | SHA256 checksum, atomic tmp→verify→.bak→rename, autosave 3 slot xoay vòng |

## LastHope.Data

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Definition types | `Data/Definitions/*.cs` | `ItemDefinition`, `LocationDefinition`, `RouteDefinition`, `SearchPointDefinition`, `BalanceDefinition` | ✅ | Khớp schema snake_case sẵn có. `SearchPointDefinition.OpenHoldSeconds` (đổi tên từ `OpenTimeMinutes` 2026-07-27, đơn vị giây thực giữ phím) |
| JSON config | `Data/DefinitionJson.cs` | `Settings`, `Deserialize<T>` | ✅ | Một nơi duy nhất định nghĩa PascalCase ↔ snake_case |
| Registry | `Data/DefinitionRegistry.cs` | `GetItem/GetLocation/GetRoute/GetSearchPoint`, `TryGet*`, `Balance` | ✅ | Chỉ đọc lúc chơi |
| Loader | `Data/DefinitionLoader.cs` | `LoadFromDirectory(path)` | ✅ | Nhận diện theo tiền tố file; **gom toàn bộ lỗi**; `events_/npcs_/phases_/modules_/shelterzones_` đang bỏ qua có chủ đích |

`BalanceDefinition` mới khai báo `inventory`/`travel`/`new_game` — các nhóm còn lại trong `balance.json` (condition, power, water, intel, npc, slice, shelter) **chưa đọc**, thêm khi làm tới P2/P3/P4.

## LastHope.Systems

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Service registry | `Systems/Registry/GameServices.cs` | `BindWorld/ReloadWorld/SaveTo/LoadFrom/SaveAutosave` | 🟡 | Tập service cố định, không phải DI container |
| Composition root | `Systems/Boot/GameBootstrapper.cs` | `Services` (static), `IsReady`, `Ready` event | ⬜ | Trong `10_GamePersistent`, `DontDestroyOnLoad`; definition lỗi = fail-fast |
| New game | `Systems/Boot/NewGameFactory.cs` | `Create(definitions, seed)` | 🟡 | Lấy số từ `balance.json`, không hard-code |
| Tick driver | `Systems/Boot/SimulationDriver.cs` | — | ⬜ | MonoBehaviour **duy nhất** đọc `Time.deltaTime`; clamp delta 1s; autosave 300s |
| Inventory rules | `Systems/Inventory/InventorySystem.cs` | `ComputeLoadTier`, `SpeedModifierFor`, `CanAdd/Add` | ✅ | `LoadTier` Normal/Light/Heavy/Blocked theo `balance.json`; vật hai tay (`TwoHandCarry`) route riêng khỏi `Slots` |
| Owner scheme | `Systems/Inventory/InventoryOwner.cs` | `InventoryOwner{Player,ShelterStorage,SearchPoint,DroppedItems}`, `InventoryOwnerOps` | ✅ | Struct tham số lệnh (không nằm trong save) quy đổi ra `List<ItemInstanceState>` thật |
| Search | `Systems/Search/SearchSystem.cs` | `Open`, `TakeAll` | ✅ | Roll 1 lần qua stream `"loot"`; `TakeAll` binary-search phần lớn nhất còn nhặt được, trả `false` nếu sót (triage) |
| Travel | `Systems/Travel/TravelSystem.cs` | `ComputeTravelMinutes`, `Travel` | ✅ | loadFactor × floodTimeFactor (nhân dồn, cố ý); `FastForward` từng phút qua `TickScheduler`; áp crossing cost một lần mỗi chuyến |
| Commands | `Systems/Commands/{TransferItemCommand,OpenSearchPointCommand,TakeAllFromSearchPointCommand,BeginTravelCommand}.cs` | implement `IGameCommand` | ✅ | `TransferItemCommand` dùng chung cho Take/Store/Withdraw/Drop/PickUp qua `InventoryOwner` |
| Telemetry | `Systems/Telemetry/TelemetryLogger.cs` | `LogSearchClosed`, `LogInventoryOpenDuration` (+ tự subscribe Travel/Location/Search event) | ⬜ | JSONL `persistentDataPath/Telemetry/session_*.jsonl`. Sự kiện có EventBus sẵn thì tự nghe; sự kiện chỉ UI biết (đóng panel, thời gian mở) UI gọi thẳng |
| Condition | `Systems/Condition/{ConditionSystem,ConditionDriver}.cs` | `ApplyShortTick/ApplyLongTick/IsCollapsed` | ✅ | `ConditionDriver` subscribe `TickScheduler` trong `GameServices.BindWorld`, dựng lại mỗi lần (kể cả sau Load). Wet gain do mưa ambient **chưa nối nguồn** (chờ Disaster Phase); Black Water Exposure gain **đã nối** qua Hazard crossing |
| Hazard | `Systems/Hazard/HazardSystem.cs` | `IsPassable`, `FloodIndex`, `TimeFactor`, `ApplyCrossingCost` | ✅ | Chỉ Flood State (`balance.json.hazard.crossing_*`, mảng 4 phần tử = Dry/Shallow/Medium/Deep). Current Strength/Electrified Water/Structural Collapse **chưa làm** — không có số |

## LastHope.Presentation

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Camera | `Presentation/Camera/CameraRig.cs` | `SetTarget(t)`, `Target` | ⬜ | Orthographic 2D, `transparencySortMode = CustomAxis` trục (0,1,0) |
| Player | `Presentation/Player/PlayerController.cs` | `SpeedModifier`, `Facing`, `IsMoving` | ⬜ | `Rigidbody2D` kinematic; va chạm tự viết qua `Rigidbody2D.Cast` (kinematic không tự chặn) |
| Player avatar sync | `Presentation/Player/PlayerAvatarSync.cs` | `TeleportTo(pos)` | ⬜ | Ghi transform → `PlayerState` mỗi frame; áp lại từ state khi `WorldStateReloaded` |
| Movement modifier | `Presentation/Player/PlayerMovementModifierSync.cs` | — | ⬜ | Overload (hệ số) × Collapsed (chặn nhị phân) → `PlayerController.SpeedModifier`. Đổi tên từ `PlayerOverloadSync` 2026-07-27 khi thêm Collapsed |
| Boot | `Presentation/Boot/BootLoader.cs` | — | ⬜ | `00_Boot` → additive persistent (không hard-code scene gameplay) |
| Scene flow | `Presentation/Boot/SceneFlowController.cs` | — | ⬜ | Load scene theo `LocationDefinition.SceneName` lúc boot + mỗi lần `LocationChanged`, đặt player tại `PlayerSpawnPoint` |
| Interaction | `Presentation/Interaction/{IInteractable,InteractionDetector,InteractionPromptOverlay}.cs` | `CurrentTarget`, `HoldProgress01` | ⬜ | Nhấn tức thì (`HoldDurationSeconds` ≤0) hoặc giữ phím thật (progress bar, thả sớm = hủy). `SearchPointView` chỉ đòi hold ở lần mở **đầu tiên** — đã `Rolled` thì mở lại tức thì |
| World views | `Presentation/World/{SearchPointView,StorageView,TravelPointView,PlayerSpawnPoint}.cs` | implement `IInteractable` | ⬜ | Chỉ submit Command/publish event, không biết UI nào phản hồi (tránh phụ thuộc `UI`) |

Chưa có: animation theo hướng (8-direction sprite swap) — cắt phạm vi P1-C, xem plan doc.

## LastHope.UI

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Inventory | `UI/Panels/InventoryPanel.cs` | toggle qua action `ToggleInventory` | ⬜ | OnGUI (không phải uGUI — quyết định P1-C, xem plan doc). Hiện túi + Carried Object + đồ dưới đất tại location. Đóng: nhấn lại `ToggleInventory` hoặc ESC (`Close`) |
| Search | `UI/Panels/SearchPanel.cs` | `Open(searchPointId)` | ⬜ | Tự mở khi nghe `SearchPointOpened`. Take lẻ / Take All, báo triage nếu còn sót. Đóng: tương tác lại đúng search point (toggle) hoặc ESC |
| Storage | `UI/Panels/StoragePanel.cs` | — | ⬜ | Tự mở khi nghe `StorageOpened`. Chuyển 2 chiều player ↔ kho. Đóng: tương tác lại đúng kho (toggle) hoặc ESC |

## LastHope.DebugTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Overlay | `DebugTools/Overlay/DebugOverlay.cs` | `SetTracked(t)` | ⬜ | F1: FPS + toạ độ X/Y |
| Debug Panel | `DebugTools/Panel/DebugPanel.cs` | — | ⬜ | F2: tua giờ, time scale, thêm/dùng item, save/load. **Hệ thống mới phải thêm mục vào đây** |

## LastHope.EditorTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Sinh scene | `EditorTools/SceneSetup.cs` | menu `Last Hope/Build All Scenes` | ⬜ | Scene **không sửa tay** — đổi cấu trúc thì sửa file này rồi chạy lại. Đổi tên từ `Build Sprint 1 Scenes` 2026-07-27 |
| Build | `EditorTools/BuildScript.cs` | `BuildWindowsDevelopment` | ⬜ | Chạy được qua `-executeMethod` |

## Scene

| Scene | Nội dung |
| --- | --- |
| `00_Boot` | `BootCamera`, `BootLoader` |
| `10_GamePersistent` | `GameServices` (Bootstrapper+Driver+DebugPanel), `Player` (Controller+AvatarSync+OverloadSync+InteractionDetector), `Main Camera`+`CameraRig`, `DebugOverlay`, `InteractionPrompt`, `SceneFlowController`, `InventoryPanel`, `SearchPanel`, `StoragePanel` |
| `90_TestSystems` | Ground tiled 32×20, 4 tường biên, 4 prop test Y-sort — không còn nằm trong luồng boot chính, chỉ để test thủ công |
| `Shelters/20_MainShelter` | `location_shelter` — `StorageView`, `TravelPointView` (→ store), `PlayerSpawnPoint` |
| `Locations/41_Location_ConvenienceStore` | `location_convenience_store` — 6 `SearchPointView` khớp `searchpoints_p1.json`, `TravelPointView` (→ shelter), `PlayerSpawnPoint` |

---

## Ghi chú thiết kế bắt buộc

- **2D isometric là ràng buộc khóa cứng**: Tilemap Isometric, `SpriteRenderer` +
  `Collider2D`, `Rigidbody2D` kinematic, camera orthographic không xoay +
  `transparencySortMode = CustomAxis`. Không `Rigidbody`/`CharacterController`/mesh/raycast
  occlusion. Chi tiết: `technical-specification.md`.
- Placement (grid/anchor/socket, sort order, floor toggle...) phải theo đúng
  `docs/00-project-overview/isometric-game-placement-rules.md`.
- Definition JSON đã có sẵn (bảng trên) — code Data layer phải khớp schema snake_case hiện
  hành, không tự đặt schema mới rồi phải viết lại content.
- `docs/mvp-product-backlog.md` mô tả chi tiết từng item — đọc cùng `BACKLOG.md` trước khi
  bắt đầu implement.
- **`Presentation` không phụ thuộc `UI`** (và ngược lại) — liên lạc qua `EventBus`. View
  (Presentation) submit Command hoặc publish event; Panel (UI) tự subscribe, không ai gọi
  thẳng ai.
- **UI dùng OnGUI cho P1** (không phải uGUI/Canvas đầy đủ) — quyết định P1-C, xem
  `docs/plans/2026-07-27-p1c-exploration-gameplay.md`. uGUI thật để lại cho polish sau P4.
