# Technology Decisions

## 1. Nền tảng triển khai

```text
Engine: Unity 6000.5.4f1
Primary Platform: Windows PC 64-bit
Programming Language: C#
Rendering Pipeline: Universal Render Pipeline
Project Type: 3D
Game Mode: Single-player
```

Linux và macOS không thuộc phạm vi build đầu tiên, nhưng không được sử dụng API chỉ hoạt động trên Windows nếu không cần thiết.

---

## 2. Camera và góc nhìn

```text
View: Isometric 3D
Projection: Orthographic
Camera Rotation: Fixed
Camera Pitch: 35.264°
Camera Yaw: 45°
```

Quy tắc:

- Không cho phép người chơi xoay camera trong MVP.
- Có thể zoom trong một khoảng giới hạn.
- Điều khiển di chuyển được tính theo hướng màn hình.
- Tường che nhân vật phải được ẩn hoặc làm mờ.
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

Sử dụng **Universal Render Pipeline**.

Thiết lập:

- Linear Color Space.
- Baked Lighting là nguồn sáng chính.
- Realtime Light giới hạn.
- Shadow Distance ngắn, phù hợp camera isometric.
- Stylized low/mid-poly material.
- Không dùng HDRP.
- Không dùng ray tracing.
- Không sử dụng fluid simulation toàn bản đồ.

Nước ngập được thể hiện bằng:

```text
Water Plane
+
Hazard Volume
+
Flood State
+
Local Shader Effect
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
CharacterController
```

Player movement phải:

- Độc lập framerate.
- Hỗ trợ slope và step.
- Có vận tốc gameplay riêng.
- Nhận Modifier từ Flood, Carry Load và Condition.
- Không phụ thuộc animation root motion.

### NPC

Sử dụng:

```text
Unity AI Navigation
NavMeshAgent
```

NPC ngoài Scene không dùng NavMesh.

NPC off-screen chỉ lưu:

- Location.
- Task.
- Condition.
- Event exposure.

### Interaction

Sử dụng kết hợp:

- Trigger hoặc Overlap để tìm đối tượng gần.
- Raycast từ camera hoặc con trỏ để chọn.
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
1 Unity Unit = 1 meter
```

### Pivot

- Prop: đáy giữa.
- Cửa: tại bản lề.
- Module: đáy giữa hoặc điểm snap được định nghĩa.
- Tường: góc dưới của modular grid.
- Nhân vật: giữa hai chân.

### Modular Grid

```text
Base Grid: 0.5 meter
Wall Width: bội số của 1 meter
Standard Floor Height: 3 meter
Standard Door Height: 2.2 meter
```

### Asset Pipeline

```text
AI Concept
↓
AI 3D Generation hoặc Blender Blockout
↓
Blender Cleanup
↓
Scale
↓
Pivot
↓
Topology
↓
UV và Material
↓
Collider
↓
Unity Import
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
Universal Render Pipeline
Input System
AI Navigation
TextMeshPro
Unity Test Framework
Newtonsoft JSON (com.unity.nuget.newtonsoft-json)
Physics module 3D (com.unity.modules.physics — cần cho CharacterController)
```

> Ghi chú hiện trạng 2026-07-23: manifest hiện tại chưa có URP, Input System, physics 3D và Newtonsoft — được thêm trong Sprint 1 (KAN-15). Module `physics2d` không dùng cho gameplay 3D.

Chưa thêm:

```text
Netcode for GameObjects
Addressables
Cinemachine
Behavior Tree Framework
Third-party Save Framework
Third-party Dependency Injection Framework
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
Universal Render Pipeline

Camera:
Fixed orthographic isometric

Input:
Unity Input System

Runtime UI:
uGUI và TextMeshPro

Player Movement:
CharacterController

NPC Navigation:
Unity AI Navigation

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
