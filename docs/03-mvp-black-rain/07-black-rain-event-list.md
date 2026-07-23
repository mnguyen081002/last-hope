# 07-black-rain-event-list.md

## 1. Mục tiêu

Tài liệu này định nghĩa danh sách Event của MVP Siêu Bão Mưa Đen.

Event phải:

- Hoạt động theo World Clock.
- Có điều kiện kích hoạt rõ ràng.
- Có phương thức phát hiện hợp lý.
- Có deadline và hậu quả.
- Tôn trọng Disaster Phase, Location State và Shelter State.
- Không tạo Resource bắt buộc hoàn toàn bằng RNG.
- Không làm gián đoạn gameplay bằng hội thoại hoặc menu không cần thiết.
- Tạo ra quyết định giữa thời gian, tài nguyên, con người và an toàn.

---

## 2. Cấu trúc Event

Mỗi Event được định nghĩa bằng:

```text
event_id
display_name
event_type
priority
phase
trigger_conditions
discovery_sources
soft_deadline
hard_deadline
available_responses
success_effects
failure_effects
expiration_effects
persistent_flags
```

---

## 3. Event Priority

```text
Critical
Major
Standard
Ambient
```

| Priority | Vai trò                                                         |
| -------- | --------------------------------------------------------------- |
| Critical | Có thể gây Shelter Failure, Player Death hoặc Forced Evacuation |
| Major    | Thay đổi đáng kể Resource, NPC, Route hoặc Outcome              |
| Standard | Tạo lựa chọn chiến lược cục bộ                                  |
| Ambient  | Phản ánh World State, không yêu cầu mục tiêu riêng              |

---

## 4. Event Budget

Số Event hoạt động đồng thời:

| Priority |   Số lượng mục tiêu |
| -------- | ------------------: |
| Critical |                 0–1 |
| Major    |                 1–2 |
| Standard |                 2–4 |
| Ambient  | Không giới hạn cứng |

Không kích hoạt thêm Critical Event nếu một Critical Event khác chưa được xử lý, trừ khi chúng thuộc cùng một Event Chain.

---

# 5. Main Event Timeline

| Event                           | Phase      | Priority |
| ------------------------------- | ---------- | -------- |
| Cảnh báo siêu bão               | Normal     | Major    |
| Nguồn điện chập chờn            | Warning    | Standard |
| Cửa hàng bắt đầu bị loot        | Warning    | Major    |
| Mưa lớn bắt đầu                 | First Rain | Major    |
| Mưa chuyển màu đen              | Black Rain | Critical |
| Tín hiệu cầu cứu tại trường học | Black Rain | Major    |
| Trạm bơm khu vực gặp sự cố      | Black Rain | Major    |
| Mất điện toàn khu vực           | Escalation | Critical |
| Drain Core chảy ngược           | Escalation | Critical |
| Tuyến thấp bị khóa              | Escalation | Major    |
| Máy bơm Shelter bị tắc          | Peak       | Critical |
| Storage có nguy cơ ngập         | Peak       | Critical |
| Tín hiệu bất thường đạt đỉnh    | Peak       | Major    |
| Nước bắt đầu rút                | Aftermath  | Major    |

---

# 6. Main Event Detail

## 6.1. Cảnh báo siêu bão

```text
event_id: storm_warning
event_type: Global
priority: Major
phase: Normal
```

### Trigger

```text
world_time >= Day 0, 19:00
```

### Discovery

- Radio.
- Television hoặc điện thoại trong tutorial.
- NPC Nguyễn Minh.
- Thông báo công cộng.

### Nội dung

Dự báo ban đầu cho biết:

- Bão sẽ đổ bộ trong vòng một ngày.
- Mưa lớn có thể gây ngập.
- Người dân nên dự trữ nước, thức ăn và pin.
- Một số khu thấp có thể phải sơ tán.

### Phản ứng

- Kiểm tra Shelter.
- Đi mua vật tư.
- Hỏi NPC.
- Bỏ qua và đi ngủ.

### Hậu quả

Event mở:

- World Map.
- Resource Objective.
- Shelter Inspection.
- Warning Phase Task.

### Persistent Flag

```text
storm_warning_received
```

---

## 6.2. Nguồn điện chập chờn

```text
event_id: grid_flicker
event_type: Shelter
priority: Standard
phase: Warning
```

### Trigger

```text
current_phase == Warning
AND
world_time >= Day 1, 10:00
```

### Dấu hiệu

- Đèn nhấp nháy.
- Radio mất tín hiệu ngắn.
- Electrical Backbone phát cảnh báo.
- Thiết bị đang chạy bị dừng trong vài giây.

### Phản ứng

- Kiểm tra Electrical Backbone.
- Sạc Battery.
- Chuẩn bị Emergency Lighting.
- Không xử lý.

### Kết quả

Kiểm tra hệ thống mở Power Management tutorial.

Không xử lý không gây Failure ngay nhưng:

- Battery có thể chưa được sạc.
- Người chơi nhận cảnh báo muộn hơn về Grid Failure.

---

## 6.3. Cửa hàng bắt đầu bị loot

```text
event_id: convenience_store_resource_rush
event_type: Location
priority: Major
phase: Warning
```

### Trigger

```text
current_phase == Warning
AND
world_time >= Day 1, 08:00
```

### Discovery

- NPC.
- Radio.
- Quan sát trực tiếp.
- Dòng người di chuyển trên Route A.

### Soft Deadline

```text
Day 1, 12:00
```

Sau Soft Deadline:

- Cửa hàng chuyển sang `Partially Looted`.
- Clean Water và Food giảm.

### Hard Deadline

```text
Day 1, 18:00
```

Sau Hard Deadline:

- Front Shop gần như Depleted.
- Delivery Bay vẫn có thể còn Resource.

### Phản ứng

- Tới ngay.
- Chờ đám đông giảm.
- Tìm nguồn Resource khác.
- Giúp kiểm soát xung đột.

### Persistent Flag

```text
store_looted_level
civilian_group_relation
```

---

## 6.4. Mưa lớn bắt đầu

```text
event_id: first_heavy_rain
event_type: Global
priority: Major
phase: First Rain
```

### Trigger

```text
world_time == Day 1, 18:00
```

### Tác động

- Rain State chuyển sang `Heavy Rain`.
- Wet System được kích hoạt.
- Route thấp chuyển từ `Dry` sang `Shallow`.
- Shelter bắt đầu Entrance Leakage.
- Travel Time tăng.

### Phản ứng

Không có lựa chọn ngăn Event.

Người chơi phải:

- Mặc Equipment phù hợp.
- Quyết định tiếp tục ở ngoài hay quay về.
- Bảo vệ item không chống nước.

### Persistent Flag

```text
first_rain_started
```

---

## 6.5. Mưa chuyển màu đen

```text
event_id: black_rain_transition
event_type: Global
priority: Critical
phase: Black Rain
```

### Trigger

```text
world_time == Day 2, 10:00
```

### Dấu hiệu báo trước

- Mưa có màu tối dần.
- Radio xuất hiện nhiễu.
- Nước trong vật chứa ngoài trời đổi màu.
- NPC phản ứng.
- Thiết bị đo cho kết quả bất thường.

### Tác động

- Rain State chuyển sang `Black Rain`.
- Nước ngoài trời mới thu được trở thành Black Water.
- Black Water Exposure được kích hoạt.
- Item ngoài trời có nguy cơ Contaminated.
- Intel Age của Route giảm giá trị nhanh hơn.
- Communication Interference tăng.

### Yêu cầu

Người chơi phải nhận tutorial ngắn về:

- Contamination.
- Protection.
- Clean và Contaminated Storage.

### Persistent Flag

```text
black_rain_started
```

---

## 6.6. Tín hiệu cầu cứu tại trường học

```text
event_id: school_rescue_signal
event_type: Location
priority: Major
phase: Black Rain
```

### Trigger

```text
current_phase == Black Rain
AND
school_rescue_signal_triggered == false
```

### Discovery

- Radio.
- Observation Point.
- Nguyễn Minh.
- Phạm An.
- Ánh sáng từ mái trường.

### Soft Deadline

```text
6 giờ sau khi phát hiện
```

Sau Soft Deadline:

- Tầng trệt ngập sâu hơn.
- Main Gate không còn an toàn.
- NPC có thể bị Injury.

### Hard Deadline

```text
Khi Escalation đạt Day 3, 10:00
```

### Phản ứng

- Tới cứu trực tiếp.
- Gửi NPC hỗ trợ.
- Cung cấp Route Intel.
- Chuẩn bị Trường học làm Shelter trước.
- Bỏ qua.

### Kết quả

Thành công có thể:

- Cứu NPC.
- Mở Temporary Shelter.
- Tăng Living Capacity.
- Mở Roof Observation Point.

### Persistent Flag

```text
school_survivor_count
temporary_shelter_available
```

---

## 6.7. Trạm bơm khu vực gặp sự cố

```text
event_id: regional_pump_failure
event_type: Location
priority: Major
phase: Black Rain
```

### Trigger

```text
current_phase == Black Rain
AND
regional_pump_state == Unstable
```

### Discovery

- Kỹ thuật viên Lê Hùng.
- Communication Station.
- Tiếng còi báo động khu Utility.
- Drainage Intel.

### Deadline

```text
Trước Day 3, 12:00
```

Sau deadline:

- Trạm không thể được khôi phục hoàn toàn.
- Chỉ còn Salvage Option.
- Utility Flood Modifier tăng.

### Lựa chọn

#### Restore

- Dùng Pump Part và Electronic Component.
- Tốn Active Work.
- Giảm Flood State khu vực.

#### Salvage

- Lấy linh kiện về Shelter.
- Mất lợi ích hạ tầng.

#### Bỏ qua

- Trạm ngừng hoạt động.
- Không nhận Resource.
- Route thấp bị khóa sớm hơn.

### Persistent Flag

```text
regional_pump_restored
regional_pump_salvaged
```

---

## 6.8. Mất điện toàn khu vực

```text
event_id: regional_power_failure
event_type: Global
priority: Critical
phase: Escalation
```

### Trigger

```text
world_time == Day 3, 00:00
OR
infrastructure_state causes earlier failure
```

Không được kích hoạt sớm hơn `Day 2, 20:00`.

### Dấu hiệu

- Grid Flicker tăng.
- Bản tin cảnh báo.
- Transformer phát tiếng động.
- Một số Location mất đèn trước.

### Tác động

- City Grid chuyển sang `Failed`.
- Shelter Module không có nguồn dự phòng dừng.
- Electrified Water tại một số Zone có thể biến mất sau khi điện bị cắt.
- Route và Location tối hơn.
- Battery và Fuel trở thành Resource trọng yếu.

### Phản ứng

- Khởi động Generator.
- Phân bổ Battery.
- Tắt Module không cần thiết.
- Hoàn thành Water Processing trước khi mất điện.
- Chấp nhận sống không có Power.

### Persistent Flag

```text
regional_grid_failed
```

---

## 6.9. Drain Core chảy ngược

```text
event_id: shelter_drain_backflow
event_type: Shelter
priority: Critical
phase: Escalation
```

### Trigger

```text
regional_water_pressure >= 3
AND
drain_core_state != Sealed
```

### Dấu hiệu

- Âm thanh từ đường ống.
- Nước đen xuất hiện quanh Drain Core.
- Kỹ thuật viên cảnh báo.
- Water Intrusion tăng bất thường.

### Soft Deadline

```text
30 phút trong game
```

Sau Soft Deadline:

- Utility Area chuyển sang `Shallow Flood`.

### Hard Deadline

```text
120 phút trong game
```

Sau Hard Deadline:

- Ground Floor có nguy cơ Deep Flood.
- Electrical Backbone phải bị ngắt.

### Phản ứng

- Đóng van.
- Lắp Emergency Seal.
- Kết nối Pump.
- Cắt điện tầng dưới.
- Bỏ Ground Floor.

### Persistent Flag

```text
drain_backflow_resolved
lower_floor_flood_state
```

---

## 6.10. Tuyến thấp bị khóa

```text
event_id: low_route_closure
event_type: Regional
priority: Major
phase: Escalation
```

### Trigger

```text
regional_water_pressure >= 4
OR
world_time >= Day 3, 08:00
```

Thời điểm có thể chậm hơn nếu Regional Pump được khôi phục.

### Tác động

- Route A chuyển sang `Impassable`.
- Cửa hàng và tầng thấp Hiệu thuốc không còn tiếp cận bình thường.
- Trạm bơm chỉ còn Alternative Access nếu đã mở.
- Expedition đang ở khu vực phải dùng Route Return đặc biệt.

### Cảnh báo

- Forecast.
- Route Intel.
- Nước dâng trực quan.
- NPC.
- Communication Station.

### Persistent Flag

```text
commercial_low_route_closed
```

---

## 6.11. Máy bơm Shelter bị tắc

```text
event_id: shelter_pump_jam
event_type: Shelter
priority: Critical
phase: Peak
```

### Trigger

```text
current_phase == Peak
AND
portable_pump_active == true
AND
debris_risk >= threshold
```

Event không xảy ra nếu Pump chưa được xây.

Trong trường hợp không có Pump, Event tương đương là `Water Intrusion Surge`.

### Soft Deadline

```text
20 phút trong game
```

### Hard Deadline

```text
60 phút trong game
```

### Phản ứng

- Làm sạch Pump.
- Chuyển sang Manual Operation.
- Dùng Pump Part thay thế.
- Tắt Pump và sơ tán tầng dưới.
- Giao NPC kỹ thuật xử lý.

### Kết quả thất bại

- Pump dừng.
- Water Intrusion tăng nhanh.
- Utility Area có thể bị mất.

---

## 6.12. Storage có nguy cơ ngập

```text
event_id: shelter_storage_flood_risk
event_type: Shelter
priority: Critical
phase: Peak
```

### Trigger

```text
ground_storage_water_level >= Shallow
AND
protected_storage_capacity < critical_resource_volume
```

### Soft Deadline

```text
30 phút trong game
```

### Hard Deadline

```text
90 phút trong game
```

### Phản ứng

- Chuyển Resource lên Upper Floor.
- Dùng Sealed Container.
- Ưu tiên Survival Resource.
- Bỏ lại Material.
- Giao NPC vận chuyển.

### Kết quả thất bại

Resource thấp có thể:

- Wet.
- Contaminated.
- Destroyed.
- Không còn dùng được trong Peak.

Event phải lưu chính xác Resource nào bị mất.

---

## 6.13. Tín hiệu bất thường đạt đỉnh

```text
event_id: peak_unknown_signal
event_type: Global
priority: Major
phase: Peak
```

### Trigger

```text
world_time >= Day 3, 23:00
AND
world_time <= Day 4, 02:00
```

### Discovery

- Communication Station.
- Trạm thời tiết.
- Phạm An.
- Radio cầm tay với Signal Quality đủ cao.

### Điều kiện ghi lại đầy đủ

```text
communication_powered
AND
signal_source_active
AND
operator_or_skill_available
```

### Phản ứng

- Duy trì Power cho Communication Station.
- Ngắt Pump để cấp điện cho Radio.
- Ghi lại tín hiệu một phần.
- Bỏ qua để ưu tiên Survival Module.

### Kết quả

Tín hiệu cung cấp:

- Narrative Clue.
- Forecast về thời điểm mưa giảm.
- Campaign Knowledge.

Không thu tín hiệu không gây thua.

---

## 6.14. Nước bắt đầu rút

```text
event_id: rain_weakening
event_type: Global
priority: Major
phase: Aftermath
```

### Trigger

```text
world_time == Day 4, 06:00
```

### Tác động

- Rain Intensity giảm.
- Regional Water Pressure ngừng tăng.
- Một số Route chuyển từ Impassable sang Deep.
- Peak Event không còn phát sinh.
- Outcome Evaluation bắt đầu.

### Hoạt động cuối

- Kiểm tra NPC.
- Kiểm kê Resource.
- Xử lý Injury.
- Thu thập Narrative Clue.
- Quyết định ở lại hoặc rời Shelter.

---

# 7. Optional Event Pool

Mỗi lượt chơi chọn từ bốn đến sáu Event tùy chọn.

Không chọn Event gây trùng chức năng quá nhiều.

---

## 7.1. Xe cứu hộ gặp nạn

```text
event_id: rescue_vehicle_crash
priority: Major
phase: First Rain hoặc Black Rain
```

Có thể cung cấp:

- Medicine.
- Rope.
- Battery.
- Một NPC bị thương.

Rủi ro:

- Current Strength.
- Fuel Leak.
- Deadline ngắn.

---

## 7.2. Vật tư trôi dạt

```text
event_id: floating_supply_cache
priority: Standard
phase: Black Rain
```

Resource:

- Food.
- Container.
- Waterproof Material.

Yêu cầu:

- Rope.
- Current Strength phù hợp.
- Không quá tải.

Resource bị cuốn đi khi hết deadline.

---

## 7.3. NPC yêu cầu nước sạch

```text
event_id: civilian_water_request
priority: Standard
phase: Black Rain hoặc Escalation
```

Lựa chọn:

- Cho Water.
- Đổi Resource.
- Từ chối.
- Chỉ đường tới Trường học.

Ảnh hưởng:

- NPC relation.
- Resource Pressure.
- Optional Intel.

---

## 7.4. Cầu tạm bị hỏng

```text
event_id: service_link_damage
priority: Major
phase: Escalation
```

Chỉ xảy ra nếu Elevated Service Link đã được xây.

Phản ứng:

- Repair.
- Giới hạn tải.
- Bỏ Shortcut.
- Dùng Route khác.

Không phá Shortcut mà không có cảnh báo Wind hoặc Structural Damage.

---

## 7.5. Khu vực rút nước tạm thời

```text
event_id: temporary_water_recession
priority: Standard
phase: Escalation
```

Tạo cửa sổ từ `60–120 phút` để:

- Vào Location tầng thấp.
- Thu hồi Resource.
- Mở Alternative Access.

Nước quay lại sau deadline.

---

## 7.6. Hiệu thuốc bị chiếm

```text
event_id: pharmacy_occupied
priority: Major
phase: Warning hoặc First Rain
```

Lựa chọn:

- Thương lượng.
- Trao đổi Resource.
- Đi lối phụ.
- Chờ nhóm rời đi.

MVP không cần Combat chuyên sâu.

---

## 7.7. NPC tại Shelter bị bệnh

```text
event_id: shelter_sickness
priority: Major
phase: Escalation hoặc Peak
```

Điều kiện:

- Black Water Exposure.
- Cleanliness thấp.
- Contaminated Storage.

Phản ứng:

- Điều trị.
- Cách ly.
- Giao Mai hỗ trợ.
- Không xử lý.

---

## 7.8. Generator quá nhiệt

```text
event_id: generator_overheat
priority: Critical
phase: Peak
```

Điều kiện:

- Generator tải cao.
- Maintenance thấp.
- Thông gió không đủ.

Phản ứng:

- Tắt Generator.
- Giảm tải.
- Repair.
- Chuyển sang Battery.

---

## 7.9. Gara sập một phần

```text
event_id: utility_garage_partial_collapse
priority: Major
phase: Escalation
```

Tác động:

- Một Zone bị khóa.
- Kho Parts có thể được mở hoặc phá hủy.
- Large Object có thể bị mắc kẹt.

---

## 7.10. Tín hiệu giả hoặc không đầy đủ

```text
event_id: distorted_distress_signal
priority: Standard
phase: Black Rain
```

Không phải tín hiệu cố ý giả.

Nhiễu làm người chơi chỉ nhận được:

- Vị trí không đầy đủ.
- Deadline không rõ.
- Nội dung bị mất đoạn.

Phạm An hoặc Communication Station có thể xác minh.

---

## 7.11. Người sống sót xin trú ẩn

```text
event_id: shelter_entry_request
priority: Major
phase: Escalation
```

Lựa chọn:

- Tiếp nhận.
- Chuyển tới Trường học.
- Cung cấp Resource.
- Từ chối.

Ảnh hưởng:

- Living Capacity.
- Water và Food.
- Security.
- Outcome.

---

## 7.12. Mái Shelter bị hư

```text
event_id: shelter_roof_damage
priority: Major
phase: Escalation hoặc Peak
```

Tác động:

- Upper Living Area bị Wet.
- Communication Station có nguy cơ hỏng.
- Structural Integrity giảm.

Phản ứng:

- Repair ngoài trời.
- Che tạm.
- Di chuyển Module.
- Chấp nhận mất Roof Function.

---

# 8. Event Chain Summary

| Chain         | Event                                                       |
| ------------- | ----------------------------------------------------------- |
| Disaster      | Storm Warning → First Rain → Black Rain → Peak → Aftermath  |
| Shelter Flood | Entrance Leakage → Drain Backflow → Pump Jam → Storage Risk |
| School Rescue | Rescue Signal → Roof Rescue → Temporary Shelter             |
| Regional Pump | Pump Failure → Restore hoặc Salvage → Route Modifier        |
| Signal        | Broken Transmission → Operator Rescue → Peak Signal         |
| Medical       | Medical Distress → Rescue Choice → Shelter Treatment        |
| Neighbor      | Introduction → Missing Relative → Trust Outcome             |

---

# 9. Event Randomization Rule

Cho phép thay đổi:

- Vị trí NPC giữa các Location hợp lệ.
- Thời điểm Event trong khoảng giới hạn.
- Resource phụ.
- Route bị ảnh hưởng đầu tiên.
- Optional Event được chọn.

Không thay đổi ngẫu nhiên:

- Black Rain Transition.
- Peak Start.
- Regional Grid Failure.
- Điều kiện sống sót tối thiểu.
- Resource bắt buộc duy nhất.
- Kết quả lớn sau khi người chơi đã đáp ứng đầy đủ yêu cầu.

---

# 10. Event Save Requirement

Save phải lưu:

```text
event_instance_id
event_state
trigger_time
discovery_state
soft_deadline
hard_deadline
selected_response
progress
participants
persistent_flags
random_seed
```

Load Game không được tạo lại Event Result bằng seed mới.

---

# 11. Telemetry

Cần ghi nhận:

```text
event_discovered
event_started
event_response_selected
event_resolved
event_expired
time_remaining_at_resolution
resources_spent
npc_outcome
event_failure_reason
```

---

# 12. Quyết định chốt

- MVP có 14 Main Event.
- Mỗi lượt dùng từ 4–6 Optional Event.
- Event không chờ người chơi.
- Critical Event không được chồng chéo không kiểm soát.
- Các thay đổi Global Phase không phụ thuộc RNG.
- Event Resource không thay thế nguồn cung bắt buộc.
- NPC quan trọng không chết do một lần kiểm tra xác suất.
- Event khi ngủ được mô phỏng và có thể đánh thức người chơi.
- Event Outcome được lưu trong World State.
