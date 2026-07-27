# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Hiện trạng

Xong **P1-A** (skeleton 2D) + **P1-B** (technical foundation → Gate M1 tự động PASS, 51 EditMode test).
Chưa có: Interaction/Item/Inventory/Search/Travel (P1-C), toàn bộ P2/P3/P4.

Verify pipeline hiện tại: batchmode compile → 51 EditMode test → sinh scene → build Windows → smoke test headless (boot → persistent → test room, load definitions v0.14.0).

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

## Assembly map (thiết kế mục tiêu, chưa dựng)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI
```

Dependency một chiều: `Data → Core → Systems → Presentation/UI/DebugTools`. Test assembly tham chiếu tất cả. `EditorTools` chỉ Editor-only.

## Scene flow (thiết kế mục tiêu, chưa dựng)

`00_Boot` → additive `10_GamePersistent` (services + Player/Camera/HUD sống suốt phiên) →
`SceneFlowController` load scene gameplay theo `LocationDefinition.SceneName`.

---

## LastHope.Core

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Logging | `Core/Diagnostics/GameLog.cs` | `Info/Warn/Error(LogCategory, msg)`, `Enabled` | ✅ | Error luôn ghi, không tắt được |
| RNG | `Core/Random/RngStream.cs`, `RngService.cs` | `Stream(name)`, `FlushState()`, `NextInt/NextChance` | ✅ | xorshift64*, stream đặt tên độc lập; **phải `FlushState()` trước khi save** |
| World State | `Core/State/WorldState.cs` | `WorldTimeMinutes`, `RngStreams`, `Player`, `Locations`, `GetOrCreateLocation` | ✅ | Thứ duy nhất được serialize |
| Inventory state | `Core/State/InventoryState.cs`, `ItemInstanceState.cs`, `InventoryOps.cs` | `AddItem/RemoveItem/CountOf/TotalWeightKg` | ✅ | Ops thuần tính toán; luật overload/2-tay chưa làm (S5) |
| Time | `Core/Time/SimulationClock.cs`, `TickScheduler.cs`, `GameTimeUtil.cs` | `AccumulateRealSeconds`, `Advance/FastForward`, `ShortTick/LongTick` | ✅ | `AdvanceOneMinute` là **nơi duy nhất** tăng `WorldTimeMinutes`; long tick mỗi 10 phút; anchor Day 0 17:00 |
| Events | `Core/Events/EventBus.cs`, `GameEvents.cs` | `Subscribe/Unsubscribe/Publish<T>` | 🟡 | struct event, handler copy-on-write |
| Commands | `Core/Commands/IGameCommand.cs`, `CommandProcessor.cs`, `UseItemCommand.cs` | `Submit(command)` → `CommandResult` | ✅ | Validate fail = không mutate. Mới có `UseItemCommand`; Transfer/Search/Travel ở S5-S6 |
| Save | `Core/Save/WorldStateSerializer.cs`, `SaveFile.cs`, `SaveService.cs` | `Save/Load/SaveAutosave`, `PathForSlot` | ✅ | SHA256 checksum, atomic tmp→verify→.bak→rename, autosave 3 slot xoay vòng |

## LastHope.Data

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Definition types | `Data/Definitions/*.cs` | `ItemDefinition`, `LocationDefinition`, `RouteDefinition`, `SearchPointDefinition`, `BalanceDefinition` | ✅ | Khớp schema snake_case sẵn có |
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

## LastHope.Presentation

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Camera | `Presentation/Camera/CameraRig.cs` | `SetTarget(t)`, `Target` | ⬜ | Orthographic 2D, `transparencySortMode = CustomAxis` trục (0,1,0) |
| Player | `Presentation/Player/PlayerController.cs` | `SpeedModifier`, `Facing`, `IsMoving` | ⬜ | `Rigidbody2D` kinematic, Move → world X/Y |
| Boot | `Presentation/Boot/BootLoader.cs` | — | ⬜ | `00_Boot` → additive persistent → gameplay scene |

Chưa có: `PlayerAvatarSync` (ghi vị trí vào `PlayerState`), animation 8 hướng, interaction.

## LastHope.UI

Chưa có gì — HUD/Inventory panel ở S5.

## LastHope.DebugTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Overlay | `DebugTools/Overlay/DebugOverlay.cs` | `SetTracked(t)` | ⬜ | F1: FPS + toạ độ X/Y |
| Debug Panel | `DebugTools/Panel/DebugPanel.cs` | — | ⬜ | F2: tua giờ, time scale, thêm/dùng item, save/load. **Hệ thống mới phải thêm mục vào đây** |

## LastHope.EditorTools

| Hệ thống | File | API chính | Test | Ghi chú |
| --- | --- | --- | --- | --- |
| Sinh scene | `EditorTools/SceneSetup.cs` | menu `Last Hope/Build Sprint 1 Scenes` | ⬜ | Scene **không sửa tay** — đổi cấu trúc thì sửa file này rồi chạy lại |
| Build | `EditorTools/BuildScript.cs` | `BuildWindowsDevelopment` | ⬜ | Chạy được qua `-executeMethod` |

## Scene

| Scene | Nội dung |
| --- | --- |
| `00_Boot` | `BootCamera`, `BootLoader` |
| `10_GamePersistent` | `GameServices` (Bootstrapper + Driver + DebugPanel), `Player`, `Main Camera` + `CameraRig`, `DebugOverlay` |
| `90_TestSystems` | Ground tiled 32×20, 4 tường biên, 4 prop test Y-sort |

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
