# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Hiện trạng

**Gate P1 PASS. Gate P2 PASS** (2026-07-28) — toàn bộ P2-A/B/C Done, user đã playtest hết
(Flood, Current Strength, Black Water Exposure, Electrified, Route Closure, Disaster Phase,
Equipment Protection, Return Window UI, Content P2, Scenario A–D). Rủi ro softlock route đóng
vĩnh viễn (Scenario D) — user chấp nhận có kiểm soát, xem `BACKLOG.md`.

**P3 — Shelter Loop: code xong toàn bộ (S10-S13/BL-P3-01..18), chờ user playtest** (2026-07-28→29).
221 EditMode test xanh, build Windows + smoke test headless pass. Ground/Upper Floor là Z-level
thật kiểu Project Zomboid (không phải logic suông — xem `docs/00-project-overview/isometric-game-placement-rules.md`
mục 5-6). Build System dùng Free Placement (world position tự do trong Zone, không phải Slot
cố định — xem `docs/plans/2026-07-28-free-placement.md`); Outdoor placement (Location ngoài
trời) chưa làm, chờ nội dung Module Outdoor. Elevated Storage vẫn là modifier không phải kho
riêng, Task System = Construction (không abstraction riêng), mọi tương tác qua 1 prop "Shelter
Console". Số liệu Event/Sleep (storage_flood_loss_chance, drain_backflow_trigger_chance,
pump_jam_chance, sleep_fatigue_recovery...) là tự đề xuất, chưa qua playtest — ghi `_note_p3`
trong `balance.json`. Chưa có: P4.

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
| `locations_p1.json`, `locations_p4.json` | location def | P1, P4 (`location_garage` trong file p4 dùng từ BL-P2-12; `location_school` vẫn để dành P4) |
| `routes_p1.json`, `routes_p4.json` | route def | P1, P4 (`route_shelter_garage` dùng từ BL-P2-12) |
| `searchpoints_p1.json`, `searchpoints_p4.json` | search point + loot table | P1, P4 (2 điểm garage dùng từ BL-P2-12; 2 điểm school để dành P4). **Lưu ý**: `DefinitionLoader` nạp mọi file khớp tiền tố bất kể hậu tố p1/p4 — nội dung "p4" đã sống trong registry từ trước, chỉ thiếu scene/travel point để chơi được |
| `modules_p3.json`, `shelterzones_p3.json` | build module (5) + shelter zone (8) | P3 — đọc từ BL-P3-01, trước đó nằm trong `DeferredPrefixes` |
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
| World State | `Core/State/WorldState.cs` | `WorldTimeMinutes`, `RngStreams`, `Player`, `Locations`, `Routes`, `Shelter`, `GetOrCreateLocation/Route` | ✅ | Thứ duy nhất được serialize |
| Shelter state | `Core/State/ShelterState.cs` | `ShelterState`, `BuiltModuleState`, `ConstructionState`, `PowerPriority` | ✅ | Chỉ một Main Shelter trong MVP (không key theo LocationId). Vật liệu xây dựng vẫn dùng `LocationState.StorageContainer` sẵn có (P1) |
| Route state | `Core/State/RouteState.cs` | `FloodState`, `CurrentStrength` enum, `IsElectrified` | ✅ | Route chưa từng đổi = mặc định Dry/None/false (không có entry) |
| Inventory state | `Core/State/InventoryState.cs`, `ItemInstanceState.cs`, `InventoryOps.cs` | `AddItem/RemoveItem/CountOf/TotalWeightKg/Move` | ✅ | Nhận `List<ItemInstanceState>` (dùng chung player/storage/searchpoint) + overload giữ API `InventoryState` cũ |
| Time | `Core/Time/SimulationClock.cs`, `TickScheduler.cs`, `GameTimeUtil.cs` | `AccumulateRealSeconds`, `Advance/FastForward`, `ShortTick/LongTick` | ✅ | `AdvanceOneMinute` là **nơi duy nhất** tăng `WorldTimeMinutes`; long tick mỗi 10 phút; anchor Day 0 17:00 |
| Events | `Core/Events/EventBus.cs`, `GameEvents.cs` | `Subscribe/Unsubscribe/Publish<T>` | 🟡 | struct event, handler copy-on-write. Có: `WorldTimeChanged`, `InventoryChanged`, `LocationChanged`, `SearchPointOpened`, `StorageOpened`, `TravelPointOpened`, `TravelStarted`, `WorldStateReloaded`, `ShelterConsoleOpened`/`BedOpened`/`ShelterEventTriggered`/`ConstructionStarted`/`ConstructionCompleted`/`PowerPriorityChanged`/`BeginPlacementMode` (P3) |
| Commands | `Core/Commands/IGameCommand.cs`, `CommandProcessor.cs`, `UseItemCommand.cs` | `Submit(command)` → `CommandResult`, `GameContext{World,Definitions,Events,Rng,Ticks}` | ✅ | Validate fail = không mutate. Command gameplay khác (Transfer/Search/Travel) ở `Systems/Commands` |
| Save | `Core/Save/WorldStateSerializer.cs`, `SaveFile.cs`, `SaveService.cs` | `Save/Load/SaveAutosave`, `PathForSlot` | ✅ | SHA256 checksum, atomic tmp→verify→.bak→rename, autosave 3 slot xoay vòng |
| Pointer over UI | `Core/UI/PointerOverUI.cs` | `MarkHover(bool)`, `ConsumeIsHovering()` | ⬜ | Cờ dùng chung giữa panel OnGUI (DebugTools/UI) và gameplay (Presentation) — không tạo phụ thuộc chéo giữa 2 nhánh đó, cả hai chỉ phụ thuộc Core. Đọc-rồi-xoá mỗi frame (OnGUI chạy sau LateUpdate cùng frame nên luôn trễ đúng 1 frame) |

## LastHope.Data

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Definition types | `Data/Definitions/*.cs` | `ItemDefinition`, `LocationDefinition`, `RouteDefinition`, `SearchPointDefinition`, `ModuleDefinition`, `ShelterZoneDefinition`, `BalanceDefinition` | ✅ | Khớp schema snake_case sẵn có. `ModuleDefinition`/`ShelterZoneDefinition` (P3) khớp `modules_p3.json`/`shelterzones_p3.json`. Free Placement (2026-07-28) — `ShelterZoneDefinition` có `BoundsMinX/MinY/MaxX/MaxY` + `Contains(x,y)` thay `BuildSlotIds`; `ModuleDefinition` thêm `FootprintRadius` (bán kính va chạm, chưa có kích thước sprite thật) |
| JSON config | `Data/DefinitionJson.cs` | `Settings`, `Deserialize<T>` | ✅ | Một nơi duy nhất định nghĩa PascalCase ↔ snake_case |
| Registry | `Data/DefinitionRegistry.cs` | `GetItem/GetLocation/GetRoute/GetSearchPoint/GetModule/GetShelterZone`, `TryGet*`, `TryGetZoneForSlot`, `Balance` | ✅ | Chỉ đọc lúc chơi |
| Loader | `Data/DefinitionLoader.cs` | `LoadFromDirectory(path)` | ✅ | Nhận diện theo tiền tố file; **gom toàn bộ lỗi**; `events_/npcs_/phases_` còn bỏ qua có chủ đích (P4). `modules_/shelterzones_` đọc từ P3, validate zone/material tồn tại |

`BalanceDefinition` khai báo đủ nhóm đang dùng: `inventory`/`travel`/`new_game`/`condition`/`hazard`/`disaster_phase`/`shelter`/`power`/`water`. Còn lại (`intel`/`npc`/`slice`) chưa đọc, để dành P4.

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
| Commands | `Systems/Commands/{TransferItemCommand,OpenSearchPointCommand,TakeAllFromSearchPointCommand,BeginTravelCommand,EquipItemCommand,UnequipItemCommand,StartConstructionCommand,CancelConstructionCommand,DismantleModuleCommand,SetPowerPriorityCommand,ResolveDrainBackflowCommand,RepairPumpJamCommand,SleepCommand}.cs` | implement `IGameCommand` | ✅ | `TransferItemCommand` dùng chung cho Take/Store/Withdraw/Drop/PickUp qua `InventoryOwner`. 7 command P3 mới (build/power/event/sleep) |
| Telemetry | `Systems/Telemetry/TelemetryLogger.cs` | `LogSearchClosed`, `LogInventoryOpenDuration` (+ tự subscribe Travel/Location/Search/Construction/PowerPriority/ShelterEvent) | ⬜ | JSONL `persistentDataPath/Telemetry/session_*.jsonl`. Sự kiện có EventBus sẵn thì tự nghe; sự kiện chỉ UI biết (đóng panel, thời gian mở) UI gọi thẳng. P3 (BL-P3-18, 2026-07-29) — nối `ConstructionStarted`/`ConstructionCompleted` (Build Choice + thời gian chờ Task = chênh `world_time_minutes` 2 dòng log), `PowerPriorityChanged` (Power Allocation choice), `ShelterEventTriggered` (3 Shelter Event). `ConstructionStarted`/`PowerPriorityChanged` là event mới, publish từ `StartConstructionCommand`/`SetPowerPriorityCommand` |
| Condition | `Systems/Condition/{ConditionSystem,ConditionDriver}.cs` | `ApplyShortTick/ApplyLongTick/IsCollapsed` | ✅ | `ConditionDriver` subscribe `TickScheduler` trong `GameServices.BindWorld`, dựng lại mỗi lần (kể cả sau Load). Wet gain do mưa ambient nhân thêm `EquipmentSystem.ComputeWetMultiplier` (jacket); Black Water Exposure gain qua Hazard crossing. `PlayerState.MinutesAtShelterContinuous` (P3) — Exposure tự giảm sau `ShelterTreatExposureMinutes` ở Shelter, `IsSick` giờ tự tắt khi Exposure tụt dưới ngưỡng (trước chỉ tự bật, cần P3 mới có cách giảm Exposure) |
| Power | `Systems/Shelter/PowerSystem.cs` | `GridSupply`, `Allocate` | ✅ | Phân bổ điện theo Power Priority (Critical trước), xả/sạc Battery. Grid Supply theo Disaster Phase rút gọn (Stable/Stable/Nửa/0). Số tự đề xuất 2026-07-28, chưa qua playtest |
| Shelter Water | `Systems/Shelter/ShelterWaterSystem.cs` | `ApplyLongTick`, `FindModule`, `WaterIntrusionLevel` | ✅ | Water Intrusion (inflow theo phase - barrier - pump - passive drain), Water Intake + Water Purifier batch. Ground Floor (Pump/Purifier ở Zone Ground) khóa khi `WaterIntrusion >= DeepThreshold` |
| Build | `Systems/Shelter/BuildSystem.cs` | `CanPlaceAt`, `StartConstruction/CancelConstruction/SetPaused/DismantleModule`, `ApplyShortTick` | ✅ | Free Placement (2026-07-28, thay Slot cố định) — `CanPlaceAt(zoneId, x, y, moduleId)` validate world position trong bounds Zone + không chồng lấn `PlacedModules` khác (tổng `FootprintRadius`) + đủ vật liệu. Chỉ 1 construction cùng lúc (MVP). Tick mỗi phút qua `ShortTick` — tự chạy dù rời Shelter/Sleep (Passive Task = hệ quả của kiến trúc tick sẵn có, không cần abstraction riêng). `ShelterState.PlacedModules` key theo placementId tự sinh (`NextPlacementId` counter), không phải slot id |
| Shelter Event | `Systems/Shelter/ShelterEventSystem.cs` | `ApplyLongTick` (Drain Backflow/Storage Flood Risk/Pump Jam) | ✅ | Chance tự đề xuất 2026-07-28, chưa qua playtest. Storage Flood Risk miễn nhiễm nếu có `module_elevated_storage` (đơn giản hoá — không phải kho vật lý riêng) |
| Shelter Driver | `Systems/Shelter/ShelterDriver.cs` | — | ⬜ | Nối Power/ShelterWater/ShelterEvent vào `LongTick`, Build vào `ShortTick`. Dựng trong `GameServices.BindWorld` giống `ConditionDriver` |
| Hazard | `Systems/Hazard/HazardSystem.cs` | `IsPassable`, `EffectiveFlood`, `TimeFactor`, `ApplyCrossingCost`, `ApplyCurrentCrossing`, `ApplyElectrifiedCrossing` | ✅ | Flood: `balance.json.hazard.crossing_*` (số thật). Current/Electrified: số tự đề xuất, **user đã verify 2026-07-28**. `ApplyCrossingCost`/`ApplyCurrentCrossing` nhận tham số protection (default = không đổi hành vi cũ) từ boots/jacket/rope. Structural Collapse **chưa làm** |
| Disaster Phase | `Systems/Hazard/DisasterPhaseSystem.cs` | `CurrentPhase`, `IsRaining` | ✅ | Suy thuần từ `WorldTimeMinutes`, không lưu state. Số tự đề xuất, **user đã verify 2026-07-28**. `IsRaining` nối vào `ConditionSystem.UpdateWet` (field `WetGainPerMinuteInRain` bỏ trống từ P2-A) |
| Equipment | `Systems/Equipment/EquipmentSystem.cs` | `TryEquip/TryUnequip/CanUnequip`, `ComputeWetMultiplier`, `ComputeBootsProtection`, `ComputeCurrentReduction` | ✅ | Đồ mặc không nằm trong `InventoryState.Slots` (không tính Carry Load); dry_bag cộng/trừ thẳng `CapacityKg/Liters` lúc equip/unequip, tháo bị từ chối nếu tràn túi. `CanUnequip` (2026-07-27) kiểm tra thuần không mutate — `UnequipItemCommand.Validate` gọi hàm này để từ chối đúng lúc thay vì để `Execute` âm thầm no-op. Gloves (`handles_contaminated`) **còn treo** — chưa có action "xử lý đồ nhiễm bẩn" |

## LastHope.Presentation

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Camera | `Presentation/Camera/CameraRig.cs` | `SetTarget(t)`, `Target` | ⬜ | Orthographic 2D, `transparencySortMode = CustomAxis` trục (0,1,0). Zoom bỏ qua khi con trỏ đang ở trên panel OnGUI (`PointerOverUI.ConsumeIsHovering()`, 2026-07-27 — trước đó cuộn chuột trên panel vừa cuộn panel vừa zoom camera) |
| Player | `Presentation/Player/PlayerController.cs` | `SpeedModifier`, `Facing`, `IsMoving` | ⬜ | `Rigidbody2D` kinematic; va chạm tự viết qua `Rigidbody2D.Cast` (kinematic không tự chặn) |
| Player avatar sync | `Presentation/Player/PlayerAvatarSync.cs` | `TeleportTo(pos)` | ⬜ | Ghi transform → `PlayerState` mỗi frame; áp lại từ state khi `WorldStateReloaded` |
| Movement modifier | `Presentation/Player/PlayerMovementModifierSync.cs` | — | ⬜ | Overload (hệ số) × Collapsed (chặn nhị phân) → `PlayerController.SpeedModifier`. Đổi tên từ `PlayerOverloadSync` 2026-07-27 khi thêm Collapsed |
| Boot | `Presentation/Boot/BootLoader.cs` | — | ⬜ | `00_Boot` → additive persistent (không hard-code scene gameplay). Tự unload scene `00_Boot` sau khi persistent sẵn sàng (2026-07-29) — trước đó không unload, `BootCamera` (tag `MainCamera`) tồn tại song song Main Camera thật, `Camera.main` ở nơi khác có thể trả nhầm |
| Scene flow | `Presentation/Boot/SceneFlowController.cs` | — | ⬜ | Load scene theo `LocationDefinition.SceneName` lúc boot + mỗi lần `LocationChanged`, đặt player tại `PlayerSpawnPoint`. `RepositionPlayer` tìm spawn theo tên scene cụ thể (2026-07-27 — trước dùng `FindFirstObjectByType` toàn cục, trúng nhầm scene cũ chưa unload xong khi Travel). Nghe thêm `TravelStarted` (đã có `RouteId`) để chọn đúng `PlayerSpawnPoint` khi scene có nhiều cổng ra vào (BL-P2-12, 2026-07-27) |
| Interaction | `Presentation/Interaction/{IInteractable,InteractionDetector,InteractionPromptOverlay}.cs` | `CurrentTarget`, `HoldProgress01` | ⬜ | Nhấn tức thì (`HoldDurationSeconds` ≤0) hoặc giữ phím thật (progress bar, thả sớm = hủy). `SearchPointView` chỉ đòi hold ở lần mở **đầu tiên** — đã `Rolled` thì mở lại tức thì |
| World views | `Presentation/World/{SearchPointView,StorageView,TravelPointView,PlayerSpawnPoint,ShelterConsoleView,BedView}.cs` | implement `IInteractable` | ⬜ | Chỉ submit Command/publish event, không biết UI nào phản hồi (tránh phụ thuộc `UI`). `PlayerSpawnPoint.RouteId` (2026-07-27) — scene nhiều `TravelPoint` thì nhiều spawn, mỗi cái gắn đúng route dẫn tới nó. `TravelPointView.Interact()` (2026-07-28, BL-P2-11) đổi từ submit thẳng `BeginTravelCommand` sang publish `TravelPointOpened` — `TravelConfirmPanel` (UI) mới thật sự submit sau khi user bấm "Xác nhận". `ShelterConsoleView`/`BedView` (P3) publish `ShelterConsoleOpened`/`BedOpened` — không có payload, chỉ một Shelter |
| Z-level tầng | `Presentation/World/{FloorLevel,FloorRenderController,StaircaseZone}.cs`, `Presentation/Player/PlayerFloorState.cs` | `FloorLevel.Floor`, `PlayerFloorState.{CurrentFloor,IsBlending,BlendLowerFloor,BlendUpperFloor,BlendT,UpdateBlend,EndBlend,TeleportToFloor,ResetFloor}`, `FloorRenderController` (tự tìm `FloorLevel`/`PlayerFloorState` qua `FindObjectsByType`) | ⬜ | P3, dựng lại 5 lần sau nhiều vòng user review (2026-07-28→29) — lịch sử đầy đủ: `docs/plans/2026-07-29-staircase-blend-fix.md`. Bản hiện tại: `StaircaseZone.Update()` mỗi frame so `Mathf.Abs(playerX-zoneX)<=halfWidth && playerY trong [bottomY,topY]` (không Collider2D — lý do ở bản trước, xem plan), nếu trong vùng tính `t = InverseLerp(bottomY,topY,playerY)` **thuần theo vị trí, không suy/cache "hướng leo"** rồi gọi `PlayerFloorState.UpdateBlend(lowerFloor,upperFloor,t)`; rời vùng gọi `EndBlend()` chốt `CurrentFloor` theo `t` lúc rời (>=0.5 → tầng trên). Bản trước suy hướng 1 lần lúc bắt đầu từ `CurrentFloor` cũ rồi cache — gây 2 bug: tầng dưới rõ hẳn (không mờ) sau khi leo xong, và 2 đầu vùng xử lý không đối xứng. Root riêng ở gốc scene (không phụ thuộc GroundFloor/UpperFloor active). `FloorRenderController` khi `IsBlending` nội suy alpha liên tục giữa `BlendLowerFloor`/`BlendUpperFloor` theo `BlendT` (không nhị phân Full/Dimmed), đổi Collider2D của 2 tầng đúng mốc 0.5; khi không blend thì như cũ (Full/Dimmed alpha 0.35 + sortingOrder -1000/Hidden theo hiệu số tầng). `PlayerFloorState` sống trên Player (DontDestroyOnLoad) nên `SceneFlowController.RepositionPlayer` phải gọi `ResetFloor()` mỗi lần đổi scene; `PlacementModeController` dùng `TeleportToFloor` (đổi tức thời, không leo) khi tự chuyển tầng theo Zone đang chọn — thuần Presentation, không qua Command/WorldState/save (giống scope cut "Save/Load không đổi scene" ở P1). Fix đã áp dụng: `FindObjectsByType<FloorLevel>(FindObjectsInactive.Include, sortMode)` — overload không truyền `FindObjectsInactive` mặc định loại trừ GameObject inactive |
| Free Placement | `Presentation/World/PlacementModeController.cs` | bật khi nghe `BeginPlacementMode` | ⬜ | P3 (2026-07-28) — tương tác chuột đầu tiên trong game (mọi thứ khác dùng phím + OnGUI). Ghost `SpriteRenderer` (dựng runtime bằng `Sprite.Create(Texture2D.whiteTexture,...)` — an toàn ở build, không như `AssetDatabase`/`Resources.Load`) theo con trỏ, xanh/đỏ theo `BuildSystem.CanPlaceAt`, khung mờ biên Zone. Click trái xác nhận submit `StartConstructionCommand`, ESC (action `Close`) huỷ. Tự chuyển `PlayerFloorState` sang đúng tầng của Zone đang chọn (không bắt tự đi cầu thang trước). Sửa 2026-07-29 (user báo ghost đỏ lệch chuột): field `worldCamera` tường minh (wire qua `SceneSetup.cs`) thay `Camera.main` (xem thêm dòng Boot ở trên). Thêm khung OnGUI hiện tên Module + lý do bị từ chối dịch tiếng Việt (`RejectReasonText`) + "ESC: Huỷ" khi đang Placement Mode — trước đó không có chữ hướng dẫn nào |

Chưa có: animation theo hướng (8-direction sprite swap) — cắt phạm vi P1-C, xem plan doc.

## LastHope.UI

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Inventory | `UI/Panels/InventoryPanel.cs` | toggle qua action `ToggleInventory` | ⬜ | OnGUI (không phải uGUI — quyết định P1-C, xem plan doc). Hiện túi + Carried Object + đồ dưới đất tại location + khu "Đang mặc" (nút Tháo, hiện thông báo nếu bị từ chối) và nút "Mặc" cạnh item equipment trong túi + thanh progress tải trọng (đầy tới hard cap, 2026-07-27). Đóng: nhấn lại `ToggleInventory` hoặc ESC (`Close`) |
| Search | `UI/Panels/SearchPanel.cs` | `Open(searchPointId)` | ⬜ | Tự mở khi nghe `SearchPointOpened`. Take lẻ / Take All, báo triage nếu còn sót. Đóng: tương tác lại đúng search point (toggle) hoặc ESC |
| Storage | `UI/Panels/StoragePanel.cs` | — | ⬜ | Tự mở khi nghe `StorageOpened`. Chuyển 2 chiều player ↔ kho. Đóng: tương tác lại đúng kho (toggle) hoặc ESC |
| Travel confirm | `UI/Panels/TravelConfirmPanel.cs` | — | ⬜ | BL-P2-11 (2026-07-28), phạm vi rút gọn thay cho World Map đầy đủ (P4). Tự mở khi nghe `TravelPointOpened`. Hiện Travel Time một chiều (`TravelSystem.ComputeTravelMinutes`), Estimated Return Time = khứ hồi ×2, Known Hazard (Flood/Current/Electrified route hiện tại), cảnh báo nếu `DisasterPhaseSystem.CurrentPhase` tại thời điểm dự kiến quay lại khác hiện tại (không chặn, chỉ cảnh báo). Nút "Xác nhận đi" mới submit `BeginTravelCommand`; "Hủy" không tốn thời gian. Đóng: tương tác lại đúng travel point (toggle) hoặc ESC |
| Shelter | `UI/Panels/ShelterPanel.cs` | — | ⬜ | P3, tự mở khi nghe `ShelterConsoleOpened` (toggle). Toàn bộ Zone trong một panel (phạm vi rút gọn — không đi bộ tới từng Zone vật lý). Overview (Structural/Water Intrusion/Clean-Untreated Water/Battery/Filter), banner 3 Event + nút xử lý, Construction hiện thị 1 chỗ (chỉ 1 cái chạy cùng lúc). Free Placement (2026-07-28) — mỗi Zone liệt kê Module xây được kèm nút "Chọn vị trí" (publish `BeginPlacementMode`, đóng panel, xem `PlacementModeController`) thay vì "Xây" trực tiếp; danh sách `PlacedModules` hiện theo `(x,y)` thật, nút đổi Power Priority xoay vòng/Tháo. Sửa 2026-07-29 — nút "Chọn vị trí" giờ disable + hiện "— thiếu vật liệu" ngay tại danh sách Zone nếu không đủ nguyên liệu (`BuildSystem.HasEnoughMaterials`, tách từ `CanPlaceAt`), thay vì để user vào Placement Mode rồi mới biết (vật liệu không phụ thuộc vị trí đặt) |
| Sleep | `UI/Panels/SleepPanel.cs` | — | ⬜ | P3, tự mở khi nghe `BedOpened` (toggle). Slider chọn giờ ngủ (`SleepMinHours`-`SleepMaxHours`), submit `SleepCommand` |

## LastHope.DebugTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Overlay | `DebugTools/Overlay/DebugOverlay.cs` | `SetTracked(t)` | ⬜ | F1: FPS + toạ độ X/Y |
| Debug Panel | `DebugTools/Panel/DebugPanel.cs` | — | ⬜ | F2: tua giờ, time scale, thêm/dùng item, save/load. **Hệ thống mới phải thêm mục vào đây**. Chiều cao co theo `Screen.height` (2026-07-27 — trước cố định 760px, Game view nhỏ trong Editor bị cắt không cuộn tới được mục "Túi đồ"). Có ô tìm + danh sách toàn bộ item để bấm "Thêm" trực tiếp (2026-07-27), thay vì chỉ gõ tay id. Mục Hazard chọn được **route bất kỳ** để chỉnh Flood/Current/Electrified (2026-07-28, BL-P2-13). Mục Shelter (P3, 2026-07-28) — cheat Water Intrusion/Clean-Untreated Water/Battery, bật/tắt Drain Backflow và Pump Jam thủ công để test không phải chờ nhiều giờ game |

## LastHope.EditorTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Sinh scene | `EditorTools/SceneSetup.cs` | menu `Last Hope/Build All Scenes` | ⬜ | Scene **không sửa tay** — đổi cấu trúc thì sửa file này rồi chạy lại. Đổi tên từ `Build Sprint 1 Scenes` 2026-07-27 |
| Build | `EditorTools/BuildScript.cs` | `BuildWindowsDevelopment` | ⬜ | Chạy được qua `-executeMethod` |

## Scene

| Scene | Nội dung |
| --- | --- |
| `00_Boot` | `BootCamera`, `BootLoader` |
| `10_GamePersistent` | `GameServices` (Bootstrapper+Driver+DebugPanel), `Player` (Controller+AvatarSync+OverloadSync+InteractionDetector+FloorState), `Main Camera`+`CameraRig`, `DebugOverlay`, `InteractionPrompt`, `SceneFlowController`, `InventoryPanel`, `SearchPanel`, `StoragePanel`, `TravelConfirmPanel`, `ShelterPanel`, `SleepPanel`, `PlacementModeController` (P3, 2026-07-28) |
| `90_TestSystems` | Ground tiled 32×20, 4 tường biên, 4 prop test Y-sort — không còn nằm trong luồng boot chính, chỉ để test thủ công |
| `Shelters/20_MainShelter` | `GroundFloor` (`FloorLevel` floor=0, active mặc định: `StorageView`, `ShelterConsoleView`, 2 `TravelPointView` → store/gara, 2 `PlayerSpawnPoint`, vùng trigger đổi tầng lên) + `UpperFloor` (`FloorLevel` floor=1, inactive mặc định: `BedView`, vùng trigger đổi tầng xuống) — 2 root cùng footprint world, `FloorRenderController` (P3, 2026-07-28) |
| `Locations/41_Location_ConvenienceStore` | `location_convenience_store` — 6 `SearchPointView` khớp `searchpoints_p1.json`, `TravelPointView` (→ shelter), `PlayerSpawnPoint` đặt sát `TravelPoint` (2026-07-27) |
| `Locations/42_Location_UtilityGarage` | `location_garage` — 2 `SearchPointView` khớp `searchpoints_p4.json` (workbench/shelf), `TravelPointView` (→ shelter), `PlayerSpawnPoint` sát `TravelPoint` (BL-P2-12, 2026-07-27) |

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
