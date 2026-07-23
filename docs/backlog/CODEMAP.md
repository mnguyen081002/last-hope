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

Chưa có class nào (asmdef trống, chờ S2/S3).

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

## Data định nghĩa game (chưa có nội dung)

`Assets/StreamingAssets/Definitions/` — chỉ có `README.md` placeholder. JSON thật (`manifest.json`, `items_p1.json`, ...) sẽ thêm ở S2/S5.

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- **Chưa có** Save/Command/EventBus/Tick/GameBootstrapper — đó là S3–S4. WorldState/Registry/RNG/Serializer đã có (S2) nhưng **chưa được ai khởi tạo lúc chạy game** (không có Boot code gọi tới) — tồn tại như thư viện độc lập, test trực tiếp bằng NUnit constructor.
- `DebugOverlay` (F1) là overlay tối thiểu Sprint 1, KHÔNG phải Debug Panel v1 (F2, sẽ thêm state tree + save/load ở S4) — hai file khác nhau, đừng nhầm.
- `PlayerController.SpeedModifier` tồn tại nhưng chưa có hệ thống nào set nó (Carry Load/Flood ở M2/P2).
- Input action "Interact" (E) đã khai báo trong `.inputactions` nhưng chưa có script nào subscribe.
- `InventoryOps.RecalculateLoad` chỉ tính tổng, **không set `Overload`** — capacity 15kg/25L trong bảng baseline chưa được code ở đâu cả, sẽ vào `Systems/Inventory/InventorySystem.cs` (S5).
- Chưa có Content JSON thật trong `Assets/StreamingAssets/Definitions/` (vẫn chỉ có README) — 5 file fixture test nằm ở `Assets/Tests/EditMode/Fixtures/{valid,invalid}_definitions/`, KHÔNG phải data thật của game.
- `WorldStateSerializer` chưa có test roundtrip qua file thật (chỉ dùng gián tiếp nếu có) — `SaveRoundTripTests` ở S4 sẽ là bài test đầu tiên chạy nó qua `SaveService`.

## Ghi chú kỹ thuật quan trọng (tránh dò lại code để hiểu "tại sao")

- RNG dùng xorshift64* tự viết (không dùng `System.Random`) vì cần expose state để serialize và tiếp tục sequence bit-exact sau load — xem `RngStream.cs`.
- `DefinitionLoader` không ném exception cho lỗi data (chỉ throw nếu JSON không đọc được, và exception đó cũng bị bắt + gom vào `Errors`). Gọi `Load()` luôn trả về `DefinitionLoadResult`, không bao giờ throw ra ngoài với input hợp lệ về mặt cấu trúc file.
- Naming JSON trên đĩa là **snake_case**, nhưng C# property là PascalCase — đừng thêm `[JsonProperty]` thủ công, `SnakeCaseNamingStrategy` tự convert.
