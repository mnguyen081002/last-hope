# 09-mvp-technical-specification.md

## 1. Mục tiêu

Tài liệu này xác định kiến trúc kỹ thuật tối thiểu để triển khai MVP Siêu Bão Mưa Đen.

Technical Specification phải:

- Hỗ trợ toàn bộ Core Gameplay Loop.
- Sử dụng một World Clock duy nhất.
- Lưu được Persistent World State.
- Hỗ trợ Location và Shelter thay đổi theo thời gian.
- Cho phép Event diễn ra khi người chơi không có mặt.
- Không phụ thuộc vào Time Skip ngoài Sleep.
- Có cấu trúc dữ liệu tương thích với Multiplayer tương lai.
- Tránh xây các hệ thống không cần thiết cho MVP.

---

## 2. Phạm vi kỹ thuật

### Bắt buộc

- World Clock.
- Disaster Timeline.
- Player Condition.
- Inventory.
- Item Instance.
- Shelter State.
- Location State.
- Route State.
- Hazard.
- Timed Task.
- Background Task.
- Event System.
- Information System.
- NPC State.
- Save và Load.
- Outcome Evaluation.

### Chưa triển khai

- Network Transport.
- Server Authority hoàn chỉnh.
- Matchmaking.
- Dedicated Server.
- Procedural World.
- Fluid Simulation toàn bản đồ.
- Faction AI.
- Combat System chuyên sâu.

---

## 3. Kiến trúc tổng thể

Hệ thống được chia thành bốn lớp:

```text
Definition Data
Runtime State
Simulation Systems
Presentation Layer
```

---

## 4. Definition Data

Definition Data là dữ liệu thiết kế không thay đổi trong một lượt chơi.

Ví dụ:

```text
Item Definition
Location Definition
Route Definition
Event Definition
Recipe Definition
Shelter Module Definition
Hazard Definition
NPC Definition
Disaster Phase Definition
```

Definition Data phải:

- Có ID ổn định.
- Không chứa Runtime State.
- Có thể chỉnh sửa mà không thay code hệ thống.
- Được validate khi load.

---

## 5. Runtime State

Runtime State là dữ liệu thay đổi trong quá trình chơi.

Ví dụ:

```text
World State
Player State
Inventory State
Location State
Shelter State
NPC State
Event Instance
Task Instance
Hazard Instance
Intel Record
```

Runtime State phải có thể:

- Save.
- Load.
- Đồng bộ trong Multiplayer tương lai.
- Cập nhật độc lập với Presentation.

---

## 6. World State

```text
world_state
├── world_time
├── day_index
├── time_of_day
├── current_disaster_phase
├── disaster_state
├── weather_state
├── infrastructure_states
├── route_states
├── location_states
├── shelter_states
├── npc_states
├── active_events
├── active_tasks
├── shared_intel
├── persistent_flags
└── random_seed
```

World State là nguồn dữ liệu chính của toàn bộ Simulation.

---

## 7. World Clock

World Clock sử dụng một giá trị thời gian tích lũy.

Khuyến nghị:

```text
world_time_minutes
```

đại diện cho tổng số phút trong game từ đầu Chapter.

Ví dụ:

```text
Day 0, 17:00
=
world_time_minutes: 0
```

Tỷ lệ:

```text
1 phút thực
=
5 phút trong game
```

World Clock không phụ thuộc framerate.

---

## 8. Simulation Tick

Không cập nhật toàn bộ hệ thống mỗi frame.

Sử dụng ba cấp cập nhật:

### Frame Update

Dùng cho:

- Player movement.
- Local interaction.
- Animation.
- Local Hazard Volume.
- Search progress.

### Short Simulation Tick

Baseline:

```text
1 phút trong game
```

Dùng cho:

- Stamina và Exposure cục bộ.
- Timed Task.
- Passive Module.
- Resource consumption nhỏ.

### Long Simulation Tick

Baseline:

```text
10 phút trong game
```

Dùng cho:

- NPC off-screen.
- Shelter consumption.
- Location state.
- Regional Hazard.
- Event condition.
- Infrastructure.

Phase Transition được kiểm tra khi World Clock vượt mốc.

---

## 9. Sleep Simulation

Sleep là trường hợp duy nhất World Clock được chuyển nhanh.

Quy trình:

```text
Validate Sleep
↓
Set Sleep Target
↓
Simulate theo từng Long Tick
↓
Check Interrupt Event
↓
Wake hoặc tiếp tục
↓
Set World Clock tại thời điểm kết thúc
```

Không cộng thẳng toàn bộ thời gian mà bỏ qua Simulation.

Sleep Simulation phải xử lý:

- Resource Consumption.
- Passive Task.
- Hazard.
- NPC.
- Event.
- Shelter State.
- Disaster Phase.

---

## 10. Disaster System

```text
disaster_runtime
├── disaster_id
├── current_phase
├── phase_start_time
├── phase_end_time
├── rain_state
├── rain_intensity
├── regional_water_pressure
├── contamination_level
├── interference_level
├── wind_intensity
├── infrastructure_modifiers
└── event_anchor_states
```

Disaster System chịu trách nhiệm:

- Chuyển Phase.
- Cập nhật Global Hazard.
- Gửi Phase Change Event.
- Kích hoạt Event Anchor.
- Cập nhật Route và Location Modifier.

---

## 11. Route System

Mỗi Route Segment có:

```text
route_state
├── route_id
├── flood_state
├── current_strength
├── contamination_state
├── electrical_hazard
├── structural_risk
├── wind_risk
├── closure_state
├── active_modifiers
└── last_update_time
```

Route System tính:

```text
actual_travel_time
accessibility
equipment_warning
return_window
```

Route State thật và Route Intel của người chơi phải tách riêng.

---

## 12. Location System

```text
location_runtime
├── location_id
├── exploration_state
├── disaster_state
├── access_state
├── zone_states
├── search_point_states
├── resource_remaining
├── hazard_instances
├── npc_presence
├── event_instances
└── persistent_flags
```

Location không tải trong scene vẫn được cập nhật theo Long Tick hoặc Event.

---

## 13. Search Point

```text
search_point_state
├── search_point_id
├── remaining_resource_pool
├── search_progress
├── depletion_state
├── contamination_state
├── destruction_state
└── last_searched_time
```

Search diễn ra theo thời gian thực.

Loot được reveal theo progress threshold.

Search Point không tự refill.

---

## 14. Item System

Tách:

```text
Item Definition
Item Instance
```

### Item Definition

```text
item_id
category
base_weight
base_volume
stack_rule
maximum_durability
water_resistance
tags
```

### Item Instance

```text
instance_id
item_id
quantity
condition
durability
maximum_durability
contamination_state
wet_state
container_id
custom_flags
```

Item có Condition hoặc Contamination khác nhau không được stack.

---

## 15. Inventory System

```text
inventory_state
├── owner_id
├── equipment_slots
├── quick_slots
├── backpack_container
├── carried_object
├── items
├── current_weight
├── current_volume
└── overload_state
```

Inventory System chịu trách nhiệm:

- Validate Weight và Volume.
- Equip.
- Transfer.
- Stack.
- Split.
- Drop.
- Contamination transfer.
- Carried Object.

World Clock không dừng khi Inventory mở.

Single-player có thể giữ điều khiển camera hoặc giảm input, nhưng Simulation vẫn chạy.

---

## 16. Player Condition System

```text
player_condition
├── health
├── stamina
├── fatigue
├── hunger
├── thirst
├── body_temperature
├── injuries
├── status_effects
├── carry_load
└── incapacitation_state
```

Condition System nhận Modifier từ:

- Item.
- Equipment.
- Hazard.
- Task.
- Sleep.
- Food và Water.
- Injury.

---

## 17. Hazard System

```text
hazard_instance
├── hazard_instance_id
├── hazard_definition_id
├── area_reference
├── intensity
├── exposure_rate
├── start_time
├── duration
├── affected_targets
└── active_modifiers
```

Exposure lưu riêng theo Target:

```text
exposure_state
├── target_id
├── hazard_id
├── current_exposure
├── protection_modifier
├── threshold_state
└── status_effects
```

---

## 18. Shelter System

```text
shelter_runtime
├── shelter_id
├── structural_integrity
├── water_intrusion
├── cleanliness
├── living_capacity
├── occupants
├── zone_states
├── core_components
├── module_states
├── storage_states
├── power_state
├── water_state
├── active_tasks
├── passive_tasks
└── event_flags
```

---

## 19. Shelter Module

```text
module_instance
├── module_instance_id
├── module_definition_id
├── zone_id
├── build_state
├── construction_progress
├── condition
├── durability
├── power_priority
├── operating_state
├── input_storage
├── output_storage
└── maintenance_state
```

Module Definition xác định:

- Build Cost.
- Build Time.
- Allowed Zone.
- Power Demand.
- Input.
- Output.
- Failure Rule.

---

## 20. Power System

Power System tính:

```text
available_power
requested_power
allocated_power
stored_charge
fuel_consumption
```

Phân bổ theo:

1. Critical.
2. High.
3. Normal.
4. Disabled.

Nếu hai Module cùng Priority nhưng không đủ Power:

- Dùng thứ tự người chơi thiết lập.
- Không chọn ngẫu nhiên.

---

## 21. Water System

```text
water_state
├── clean_water
├── untreated_water
├── black_water
├── active_batches
├── filter_charge
└── contamination_flags
```

Water Processing Task cần:

- Input.
- Container.
- Power.
- Filter.
- Duration.

Input được reserve khi Task bắt đầu.

---

## 22. Task System

Mọi hoạt động dài dùng `Task Instance`.

```text
task_instance
├── task_id
├── task_definition_id
├── owner_ids
├── target_id
├── start_time
├── required_duration
├── progress
├── state
├── interruptible
├── reserved_resources
└── completion_effect
```

Task State:

```text
Queued
Active
Paused
Completed
Cancelled
Failed
```

---

## 23. Active Task

Active Task cần người thực hiện.

Ví dụ:

- Build.
- Repair.
- Treatment.
- Craft.
- Move Resource.
- Clear Obstacle.

Progress tăng khi:

- Owner còn hợp lệ.
- Tool còn hoạt động.
- Hazard không khóa Task.
- Required Resource đã reserve.

---

## 24. Passive Task

Passive Task do Module vận hành.

Ví dụ:

- Water Purification.
- Battery Charging.
- Pumping.
- Drying.

Passive Task tiếp tục khi Player rời Shelter.

Dừng khi:

- Mất Power.
- Input hết.
- Output đầy.
- Module hỏng.
- Event gián đoạn.

---

## 25. Event System

Tách:

```text
Event Definition
Event Instance
```

### Event Instance

```text
event_instance
├── event_instance_id
├── event_definition_id
├── state
├── priority
├── trigger_time
├── discovery_state
├── soft_deadline
├── hard_deadline
├── selected_response
├── progress
├── participants
├── persistent_flags
└── random_seed
```

Event Trigger không được phụ thuộc Presentation Scene đang tải.

---

## 26. Event Evaluation

Event Condition được kiểm tra:

- Khi Long Tick chạy.
- Khi Phase đổi.
- Khi relevant state thay đổi.
- Khi người chơi vào Location.
- Khi Sleep Simulation chạy.

Không kiểm tra toàn bộ Event Definition mỗi frame.

Sử dụng nhóm đăng ký theo:

```text
time_trigger
phase_trigger
state_trigger
location_trigger
resource_trigger
```

---

## 27. NPC System

```text
npc_runtime
├── npc_id
├── current_location
├── survivor_state
├── recruitment_state
├── condition
├── fatigue
├── hunger
├── thirst
├── trust
├── current_task
├── shelter_assignment
├── event_chain_state
└── persistent_flags
```

NPC off-screen cập nhật theo Long Tick.

Không cần pathfinding ngoài scene.

---

## 28. Information System

```text
intel_record
├── intel_id
├── intel_type
├── subject_id
├── source_id
├── observed_time
├── received_time
├── confidence
├── expiration_time
├── verified
└── shared_state
```

World Map UI đọc `Intel Record`, không đọc trực tiếp Route State thật.

---

## 29. Save System

Save phải chứa:

```text
save_version
definition_version
world_state
player_states
inventory_states
shelter_states
location_states
route_states
npc_states
event_instances
task_instances
intel_records
random_seed
```

---

## 30. Save Trigger

Autosave:

- Khi ngủ.
- Khi chuyển Disaster Phase.
- Khi vào hoặc rời Shelter.
- Khi hoàn thành Main Event.
- Theo chu kỳ an toàn.

Không autosave giữa một thao tác ghi dữ liệu chưa hoàn tất.

---

## 31. Save Versioning

Save có:

```text
save_version
definition_version
```

Khi Definition Data thay đổi:

- Migration nếu thay đổi nhỏ.
- Từ chối load với thông báo rõ nếu không tương thích.
- Không silently reset World State.

---

## 32. Determinism

Các kết quả có random phải sử dụng seed lưu trong Save.

Random được phép dùng cho:

- Optional Event selection.
- Resource phụ.
- NPC Location Variant.
- Minor consequence.

Không random lại khi Load.

---

## 33. Scene và Streaming

MVP có thể sử dụng:

- Một Shelter Scene.
- Một Scene cho mỗi Location.
- Route Scene hoặc Travel Segment.
- World Map UI.

Khi unload Scene:

- Runtime State được ghi về World State.
- Local Object tạm không được coi là nguồn dữ liệu chính.
- Khi load lại, Scene dựng từ Runtime State.

---

## 34. Presentation Event Bus

Simulation gửi thông báo tới Presentation qua Event hoặc Message.

Ví dụ:

```text
WorldTimeChanged
DisasterPhaseChanged
RouteStateChanged
ShelterWarningRaised
TaskCompleted
EventDiscovered
InventoryChanged
NPCStateChanged
```

UI không trực tiếp sửa Runtime State ngoài Command hợp lệ.

---

## 35. Command Layer

Player Action được gửi dưới dạng Command:

```text
StartTask
CancelTask
TransferItem
SetPowerPriority
StartSleep
SelectEventResponse
AssignNPCTask
BeginTravel
UseItem
```

Command được validate trước khi thay đổi State.

Cấu trúc này hỗ trợ Multiplayer Authority trong tương lai.

---

## 36. Multiplayer Compatibility

Dù MVP là Single-player, Runtime State phải tránh:

- Tham chiếu trực tiếp tới một Player duy nhất.
- Time riêng cho Player.
- Pause khi mở UI.
- Task chỉ lưu trong Animation.
- Event Result lưu riêng trong Scene.
- Inventory Shared ngầm định.

Mỗi hành động quan trọng cần:

```text
actor_id
target_id
world_time
validated_result
```

---

## 37. Error Handling

Hệ thống phải xử lý:

- Item bị xóa trong khi Task đang dùng.
- Module mất Power giữa Task.
- NPC chết khi đang assigned.
- Route khóa trong khi chuẩn bị Travel.
- Event hết hạn giữa Interaction.
- Container bị phá khi đang chứa item.
- Save trong trạng thái Task paused.

Task phải chuyển sang State hợp lệ, không mất dữ liệu im lặng.

---

## 38. Debug Tool Requirement

Prototype cần Debug Panel cho phép:

- Chỉnh World Clock.
- Chuyển Disaster Phase.
- Thay đổi Flood State.
- Thêm hoặc xóa Resource.
- Chỉnh Player Condition.
- Kích hoạt Event.
- Thay đổi Shelter Water Intrusion.
- Thay đổi Power.
- Teleport giữa Location.
- Xem Persistent Flag.

Debug Tool không thuộc bản phát hành.

---

## 39. Telemetry Interface

Các hệ thống gửi Event telemetry:

```text
time_spent
travel_started
travel_completed
search_completed
item_collected
resource_consumed
task_started
task_completed
event_resolved
npc_state_changed
shelter_failure
player_death
chapter_outcome
```

Telemetry phải gắn với:

```text
world_time
session_id
playthrough_id
```

---

## 40. Performance Target

MVP không cần mô phỏng toàn bộ thế giới mỗi frame.

Mục tiêu:

- Không tạo hàng trăm NPC.
- Không chạy pathfinding ngoài Scene.
- Không chạy Fluid Simulation toàn bản đồ.
- Event Condition được nhóm và kiểm tra theo Tick.
- Location ngoài Scene dùng trạng thái trừu tượng.

---

## 41. Data Validation

Khi load Definition Data, kiểm tra:

- ID không trùng.
- Reference tồn tại.
- Event Deadline hợp lệ.
- Location có Route tiếp cận.
- Resource bắt buộc có nhiều nguồn.
- Module có Allowed Zone.
- Recipe không tham chiếu Item thiếu.
- Phase Timeline không chồng chéo.
- Event Chain không tạo vòng lặp vô hạn.

---

## 42. MVP System Dependency

```text
World Clock
↓
Disaster System
↓
Route, Location, Hazard, Event
↓
Player, NPC, Shelter, Task
↓
Information và UI
↓
Outcome
```

World Clock và Runtime State phải được hoàn thiện trước hệ thống nội dung chi tiết.

---

## 43. Acceptance Criteria

Technical Foundation đạt yêu cầu khi:

1. World Clock chạy ổn định và không phụ thuộc framerate.
2. Sleep mô phỏng đúng mọi Tick và có thể bị gián đoạn.
3. Location ngoài Scene vẫn thay đổi theo Phase.
4. Event có thể hết hạn khi Player không có mặt.
5. Loot depletion vẫn tồn tại sau Save và Load.
6. Passive Task tiếp tục khi Player rời Shelter.
7. Route State thật tách khỏi Map Intel.
8. Shelter Module có thể mất Power và tiếp tục đúng khi được cấp lại.
9. Save giữ nguyên Random Result.
10. Toàn bộ Chapter có thể hoàn thành mà không cần sửa State thủ công.
11. Runtime State không phụ thuộc một Player duy nhất.
12. Outcome Report đọc được nguyên nhân chính từ Persistent State.

---

## 44. Quyết định chốt

- Definition Data và Runtime State phải tách riêng.
- Một World State là nguồn dữ liệu chính.
- Simulation dùng Frame, Short và Long Tick.
- Sleep chạy Simulation theo Tick, không bỏ qua thời gian.
- Scene chỉ trình bày Runtime State.
- Event và Task phải tồn tại ngoài Scene.
- Command Layer được dùng cho hành động thay đổi State.
- Save lưu Event, Task, Intel và Random Seed.
- MVP chưa triển khai Networking nhưng dữ liệu phải tương thích Multiplayer.
- Debug Tool và Telemetry là yêu cầu bắt buộc của prototype.
