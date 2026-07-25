# Technology Decisions

## 1. Nền tảng triển khai

```text
Engine: Unity 6000.5.4f1
Primary Platform: Windows PC 64-bit
Programming Language: C#
Rendering Pipeline: Universal Render Pipeline (2D Renderer)
Project Type: 2D
Game Mode: Single-player
```

> **ADR 2026-07-25:** Đổi từ 3D sang 2D isometric (kiểu Project Zomboid — sprite phẳng trên lưới iso). Lý do: không có khả năng dựng/render asset 3D (không có Blender/3D pipeline khả dụng cho 1 dev); art 2D dễ tạo hơn nhiều. `LastHope.Core`/`Data`/`Systems` không đổi (C# thuần, không phụ thuộc UnityEngine 3D API) — chỉ `LastHope.Presentation`/`EditorTools` đổi.

Linux và macOS không thuộc phạm vi build đầu tiên, nhưng không được sử dụng API chỉ hoạt động trên Windows nếu không cần thiết.

---

## 2. Camera và góc nhìn

```text
View: Isometric 2D (art vẽ theo góc chiếu iso, camera không xoay)
Projection: Orthographic
Camera Rotation: Không có — camera nhìn thẳng trục Z, không pitch/yaw
```

Quy tắc:

- Camera không xoay, không nghiêng — góc "isometric" nằm ở cách vẽ sprite/tile, không phải góc camera.
- Có thể zoom trong một khoảng giới hạn (qua `orthographicSize`).
- Điều khiển di chuyển ánh xạ thẳng theo trục world X/Y — không còn khái niệm "theo hướng màn hình" vì camera không xoay nên world và màn hình luôn cùng hướng.
- Vật thể che nhân vật xử lý bằng **Y-sort** (`Camera.transparencySortMode = CustomAxis`), không phải ẩn/làm mờ theo raycast 3D.
- Camera không trực tiếp chứa gameplay state.

Prototype sử dụng Camera Rig tự triển khai, chưa cần Cinemachine.

---

## 3. Điều khiển

Sử dụng **Unity Input System**.

Điều khiển PC mặc định:

```text
WASD: Di chuyển
Mouse: Chọn và tương tác
Left Click: Tương tác hoặc xác nhận
Right Click: Hủy hoặc mở hành động phụ
E: Tương tác nhanh
I: Inventory
M: World Map
Tab: Shelter hoặc Character Overview
Escape: Menu
Mouse Wheel: Zoom camera
```

Controller support chưa thuộc phạm vi prototype đầu tiên nhưng Action Map không được phụ thuộc trực tiếp vào phím cụ thể.

---

## 4. Render Pipeline

Sử dụng **Universal Render Pipeline — 2D Renderer** (`Renderer2DData`).

Thiết lập:

- Linear Color Space.
- Light2D (Global/Point/Freeform) là nguồn sáng chính — thay Baked Lighting 3D, dùng cho hiệu ứng mưa/đêm/Black Rain.
- Shadow Caster 2D giới hạn, chỉ dùng khi cần tín hiệu gameplay rõ (không phải thẩm mỹ).
- Sprite pixel-art hoặc flat-shaded, vẽ theo góc chiếu isometric (footprint diamond).
- Không dùng HDRP.
- Không dùng ray tracing.
- Không sử dụng fluid simulation toàn bản đồ.

Nước ngập được thể hiện bằng:

```text
Sprite/Tile nước
+
Trigger Collider2D (Hazard Volume)
+
Flood State
+
Sprite animation hoặc shader 2D
```

Gameplay State của nước không được đọc trực tiếp từ shader.

---

## 5. UI

Sử dụng:

```text
Unity UI — uGUI
TextMeshPro
```

Lý do:

- Phù hợp Inventory và drag-and-drop.
- Dễ triển khai HUD, World Map và Shelter Overview.
- Ổn định cho runtime UI.
- Không cần duy trì hai hệ UI trong MVP.

UI Toolkit chỉ được dùng cho Editor Tool nếu cần.

---

## 6. Physics và di chuyển

### Player

Sử dụng:

```text
Rigidbody2D (Kinematic) + Collider2D
```

Player movement phải:

- Độc lập framerate.
- Không có khái niệm slope/step/gravity — top-down 2D di chuyển tự do trên mặt phẳng X/Y.
- Có vận tốc gameplay riêng.
- Nhận Modifier từ Flood, Carry Load và Condition.
- Không phụ thuộc animation root motion.

### NPC

Chưa có Presentation/visual cho NPC (chỉ có `NpcState` mô phỏng, không có GameObject trong scene). Khi cần dựng visual:

```text
Di chuyển đơn giản tới target (steering trực tiếp)
```

Hoãn pathfinding phức tạp (A*/grid-based) tới khi content thật sự cần NPC né vật cản/đi qua nhiều phòng. Không phụ thuộc NavMeshAgent hay plugin pathfinding 2D ngoài trong giai đoạn này.

NPC off-screen chỉ lưu:

- Location.
- Task.
- Condition.
- Event exposure.

### Interaction

Sử dụng kết hợp:

- `Physics2D.OverlapCircleNonAlloc` để tìm đối tượng gần.
- `Camera.ScreenToWorldPoint` + `Physics2D.OverlapPoint` từ con trỏ để chọn.
- Interface chung cho Interactable.

---

## 7. Cấu trúc Scene

MVP sử dụng Scene tách rời và load additive.

```text
00_Boot
01_MainMenu
10_GamePersistent
20_MainShelter
30_Route_Commercial
31_Route_Residential
32_Route_Utility
40_Location_Residential
41_Location_ConvenienceStore
42_Location_Pharmacy
43_Location_UtilityGarage
44_Location_School
45_Location_PumpStation
46_Location_WeatherStation
90_TestSystems
91_TestGameplay
```

### `00_Boot`

Chịu trách nhiệm:

- Khởi tạo service.
- Load Definition Data.
- Load hoặc tạo World State.
- Chuyển sang Main Menu hoặc Game.

### `10_GamePersistent`

Chứa:

- World Clock.
- Simulation Scheduler.
- Command Bus.
- Event Bus.
- Save Service.
- Definition Registry.
- Audio Manager.
- Scene Transition Service.
- Debug Service.

Scene này tồn tại trong toàn bộ phiên chơi.

### Gameplay Scene

Mỗi thời điểm chỉ có một Shelter, Route hoặc Location Scene chính được active.

Scene chỉ trình bày Runtime State.

Khi unload:

- Trạng thái được ghi về World State.
- Không giữ dữ liệu quan trọng riêng trong GameObject.

---

## 8. Scene Loading

Giai đoạn đầu sử dụng:

```text
Unity SceneManager
+
Additive Scene Loading
```

Chưa sử dụng Addressables trong P1–P4.

Addressables chỉ được bổ sung khi:

- Content Production bắt đầu.
- Thời gian load hoặc memory yêu cầu.
- Số lượng asset tăng rõ rệt.

Không đưa Addressables thành dependency của Core Gameplay Systems.

---

## 9. Definition Data

Definition Data sử dụng **JSON thuần** (Newtonsoft JSON), đặt tại `Assets/StreamingAssets/Definitions/`, load qua Definition Registry tại `00_Boot`.

> **ADR 2026-07-23:** Quyết định trước đây là ScriptableObject. Đổi sang JSON thuần vì: team 1 developer nên không cần authoring qua Inspector; JSON diff được trong git; validate hàng loạt được bằng tool tự động; AI/tool sinh content trực tiếp được; đường load giống nhau giữa Editor và build. ScriptableObject không dùng làm Definition Data trong MVP.

Các loại chính:

```text
ItemDefinition
EquipmentDefinition
LocationDefinition
RouteDefinition
SearchPointDefinition
ShelterDefinition
ShelterModuleDefinition
HazardDefinition
EventDefinition
NPCDefinition
RecipeDefinition
DisasterDefinition
DisasterPhaseDefinition
```

Mỗi Definition phải có:

```text
string id
string displayNameKey
int dataVersion
```

ID sử dụng:

- Chữ thường.
- Snake case.
- Không thay đổi sau khi content được đưa vào Save.

Ví dụ:

```text
item_clean_water
location_convenience_store
event_black_rain_transition
module_portable_pump
```

Không dùng tên GameObject làm ID dữ liệu.

---

## 10. Runtime State

Runtime State sử dụng plain C# class.

Không dùng `MonoBehaviour` hoặc `ScriptableObject` làm Save State.

Ví dụ:

```text
WorldState
PlayerState
InventoryState
ItemInstanceState
LocationState
RouteState
ShelterState
ModuleState
NPCState
EventState
TaskState
IntelState
```

Runtime object tham chiếu Definition bằng stable ID.

```text
itemDefinitionId
locationDefinitionId
eventDefinitionId
```

Không serialize reference tới GameObject, Component hoặc Scene Object.

---

## 11. Save System

Save format:

```text
Versioned JSON
+
Atomic File Write
+
Checksum
```

Save location:

```text
Application.persistentDataPath
```

Quy trình ghi:

```text
Serialize State
↓
Ghi temporary file
↓
Kiểm tra dữ liệu
↓
Đổi temporary file thành save chính
↓
Giữ một backup gần nhất
```

Save slot MVP:

```text
3 Autosave Slot luân phiên
1 Manual Save Slot
```

Save phải chứa:

- Save version.
- Definition version.
- World State.
- Random seed.
- Event state.
- Task progress.
- Loot depletion.
- Intel.
- Persistent flags.

Không save trực tiếp texture, mesh hoặc Scene object.

---

## 12. World Clock

World Clock lưu bằng:

```text
long worldTimeSeconds
```

Quy đổi mặc định:

```text
1 real second
=
5 game seconds
```

Tương đương:

```text
1 real minute
=
5 game minutes
```

Các cấp cập nhật:

```text
Frame Update:
Movement, animation, local interaction

Short Tick:
1 game minute

Long Tick:
10 game minutes
```

World Clock:

- Không phụ thuộc framerate.
- Không dừng khi mở Inventory.
- Không có gameplay fast-forward.
- Chỉ được mô phỏng nhanh trong Sleep.

---

## 13. Kiến trúc code

Sử dụng kiến trúc data-driven với các lớp:

```text
Definition
Runtime State
Simulation System
Command
Presentation
```

### Command

Mọi hành động thay đổi Runtime State đi qua Command.

Ví dụ:

```text
TransferItemCommand
StartSearchCommand
StartTaskCommand
CancelTaskCommand
BeginTravelCommand
SetPowerPriorityCommand
AssignNpcTaskCommand
StartSleepCommand
SelectEventResponseCommand
```

### Event Bus

Simulation phát thông báo:

```text
WorldTimeChanged
DisasterPhaseChanged
InventoryChanged
RouteStateChanged
TaskCompleted
EventDiscovered
ShelterWarningRaised
NpcStateChanged
```

Presentation lắng nghe Event nhưng không sở hữu State.

---

## 14. Dependency Management

Không sử dụng Dependency Injection Framework bên ngoài trong MVP.

Sử dụng:

```text
Bootstrap Composition Root
+
Constructor Injection cho pure C# service
+
Serialized Reference cho presentation component
```

Không dùng global singleton tùy tiện.

Các service toàn cục được khởi tạo tại `00_Boot` và đăng ký trong một Game Service Registry giới hạn.

---

## 15. Assembly Definition

Tạo các assembly:

```text
LastHope.Core
LastHope.Data
LastHope.Simulation
LastHope.Gameplay
LastHope.Presentation
LastHope.UI
LastHope.Debug
LastHope.Tests
```

Quy tắc dependency:

```text
Core
↑
Data
↑
Simulation
↑
Gameplay
↑
Presentation và UI
```

`Simulation` không được tham chiếu tới UI.

---

## 16. Source Control

Sử dụng:

```text
Git
Git LFS
```

Git LFS quản lý:

- `.fbx`
- `.blend`
- Texture lớn.
- Audio.
- Video.
- File binary từ AI asset pipeline.

Branch strategy:

```text
main
+
short-lived feature branches
```

Không tạo nhiều branch dài hạn cho một developer.

Mỗi commit phải:

- Chỉ chứa một thay đổi logic chính.
- Không commit generated cache.
- Không commit Unity Library folder.
- Không commit file build.

---

## 17. Asset Convention

### Scale

```text
Pixel-per-unit: xác định khi có art thật (khởi điểm gợi ý 64–128 PPU)
Tile iso: kích thước cụ thể điền khi có art thật (vd 64×32 px cho tile diamond)
```

### Pivot

- Sprite prop: đáy giữa (đúng ô tile đứng trên).
- Cửa: tại bản lề, sprite đủ 1-2 ô tile.
- Module: đáy giữa, snap theo lưới tile.
- Tường: góc dưới của ô tile.
- Nhân vật: chân, giữa 2 ô ngang.

### Modular Grid

```text
Grid.CellLayout: Isometric (Tilemap)
Wall Width: bội số 1 ô tile
Floor/tầng: chuyển bằng tile cầu thang (đổi sorting layer/floor index), không phải cao độ vật lý
```

### Asset Pipeline

```text
AI Concept
↓
AI 2D Generation hoặc pixel-art thủ công
↓
Cleanup (crop, palette, outline)
↓
Pivot + slice (Sprite Editor)
↓
Import vào Unity (Sprite 2D/Tile)
↓
Gán Collider2D / TilemapCollider2D
↓
Isometric Camera Review
```

Không đưa asset AI trực tiếp vào production scene trước bước cleanup.

---

## 18. Art Performance Baseline

### Texture

```text
Small Prop: 512–1024
Large Prop: 1024
Environment Module: 1024–2048
Character: 2048
```

### Geometry

Không đặt polygon budget cứng cho mọi asset.

Ưu tiên:

- Silhouette.
- Tần suất xuất hiện.
- Khoảng cách camera.
- GPU instancing.
- Shared material.
- Static batching.

Gameplay-critical collider phải là collider đơn giản, không dùng Mesh Collider phức tạp nếu không cần.

---

## 19. Audio

Sử dụng Unity Audio Mixer.

Mixer Group:

```text
Master
Music
Ambience
Weather
SFX
UI
Radio
```

Rain, Drain Backflow và Electrical Hazard phải có audio state riêng vì chúng là tín hiệu gameplay.

---

## 20. Telemetry

Prototype ghi telemetry cục bộ dưới dạng:

```text
JSON Lines hoặc CSV
```

Không sử dụng dịch vụ analytics trực tuyến trong P1–P4.

Telemetry lưu:

- Playthrough ID.
- Session ID.
- World Time.
- Event.
- Relevant State.
- Result.

---

## 21. Automated Test

Sử dụng Unity Test Framework.

### Edit Mode Test

- Definition validation.
- Inventory rules.
- Resource calculation.
- Event conditions.
- Outcome evaluation.
- Save serialization.

### Play Mode Test

- World Clock.
- Sleep simulation.
- Search progress.
- Scene transition.
- Passive task.
- Route closure.
- Shelter water intrusion.

---

## 22. Build Configuration

### Development

```text
Platform: Windows x86_64
Scripting Backend: Mono
Development Build: Enabled
Script Debugging: Khi cần
```

### Release Candidate

```text
Platform: Windows x86_64
Scripting Backend: IL2CPP
Development Build: Disabled
```

Target baseline:

```text
Windows 10 và Windows 11
1920 × 1080
Resizable Window
Fullscreen Window
```

---

## 23. Package Baseline

Các package cần dùng trong giai đoạn đầu:

```text
Universal Render Pipeline (2D Renderer)
Input System
Tilemap (com.unity.modules.tilemap)
2D Sprite (com.unity.2d.sprite)
Physics2D (com.unity.modules.physics2d)
TextMeshPro
Unity Test Framework
Newtonsoft JSON (com.unity.nuget.newtonsoft-json)
```

> Ghi chú hiện trạng 2026-07-25: đổi sang 2D isometric — bỏ `Physics module 3D` (`com.unity.modules.physics`, dùng cho `CharacterController`, không còn dùng) và `AI Navigation` (chưa từng được implement, không có NPC visual nào tồn tại) khỏi baseline. `physics2d` từ giờ dùng thật (trước chỉ khai báo sẵn, chưa dùng).

Chưa thêm:

```text
Netcode for GameObjects
Addressables
Cinemachine
Behavior Tree Framework
Third-party Save Framework
Third-party Dependency Injection Framework
Pathfinding plugin 2D (A*/NavMeshAgent2D ngoài) — hoãn tới khi content cần
```

Chỉ thêm package mới khi có use case đã được kiểm chứng.

---

## 24. Quyết định kỹ thuật đã khóa

```text
Engine:
Unity 6000.5.4f1

Platform:
Windows PC 64-bit

Language:
C#

Rendering:
Universal Render Pipeline (2D Renderer)

Camera:
Fixed orthographic 2D, không xoay — isometric nằm ở art + Y-sort

Input:
Unity Input System

Runtime UI:
uGUI và TextMeshPro

Player Movement:
Rigidbody2D (Kinematic) + Collider2D

NPC Navigation:
Chưa implement — khi cần, steering đơn giản trước, hoãn pathfinding phức tạp

Definition Data:
JSON thuần (Newtonsoft) — xem ADR mục 9

Runtime State:
Plain C# class

Save:
Versioned JSON

Scene Structure:
Persistent scene + additive gameplay scenes

Source Control:
Git + Git LFS

Asset Tool:
Blender + AI generation

Networking:
Không triển khai trong MVP
```
