# 04-main-shelter-design.md

## 1. Mục tiêu

Tài liệu này xác định thiết kế chi tiết của Main Shelter trong MVP Siêu Bão Mưa Đen.

Main Shelter phải:

- Là trung tâm của Core Gameplay Loop.
- Có cấu trúc và Zone được thiết kế sẵn.
- Chứa các Fixed Core Component phục vụ Hazard và Event.
- Cho phép nhiều hướng chuẩn bị khác nhau.
- Có nguy cơ thất bại theo từng hệ thống.
- Có thể duy trì tới cuối Peak nếu được chuẩn bị hợp lý.
- Không phải vị trí an toàn tuyệt đối.
- Hỗ trợ Forced Evacuation sang Trường học.

---

## 2. Shelter Identity

```text
shelter_id: main_shelter_house
display_name: Nhà số 17
district: Basin Core
elevation: E2
shelter_type: Main Shelter
structure_type: Nhà dân hai tầng
initial_living_capacity: 2
maximum_living_capacity: 5
```

Main Shelter là một căn nhà hai tầng nằm trong khu dân cư có độ cao trung bình.

Vị trí đủ gần các nguồn tài nguyên đầu game nhưng vẫn chịu ảnh hưởng của:

- Nước dâng từ khu vực thấp.
- Drain Backflow.
- Mất điện khu vực.
- Black Water xâm nhập tầng dưới.

---

## 3. Vai trò gameplay

Main Shelter có sáu vai trò:

```text
Storage Hub
Rest and Recovery
Water Processing
Power Management
Construction Center
Peak Survival Space
```

Trong mỗi ngày, người chơi phải quyết định thời gian dành cho:

- Sắp xếp và bảo vệ tài nguyên.
- Xây dựng Module.
- Xử lý nước.
- Sửa thiết bị.
- Nghỉ ngơi.
- Phân công NPC.
- Chuẩn bị sơ tán.

---

## 4. Cấu trúc tổng thể

Main Shelter gồm ba tầng chức năng:

```text
Ground Floor
Upper Floor
Roof
```

### Ground Floor

- Dễ tiếp cận.
- Có phần lớn Core Component.
- Có nhiều Build Slot.
- Có nguy cơ ngập cao nhất.

### Upper Floor

- Là Safe Zone chính.
- Có Living Area.
- Phù hợp Elevated Storage.
- Sức chứa và lối vận chuyển hạn chế.

### Roof

- Có Observation Point.
- Có Antenna Mount.
- Chịu Wind Hazard.
- Không phù hợp lưu trữ dài hạn.

---

## 5. Fixed Core Component

Các Core Component không thể:

- Di chuyển.
- Tháo dỡ.
- Thay thế.
- Bị người chơi xây đè lên.

---

### 5.1. Main Staircase

```text
core_id: main_staircase
location: Central Hall
```

Vai trò:

- Kết nối Ground Floor và Upper Floor.
- Là tuyến vận chuyển tài nguyên duy nhất giữa hai tầng.
- Là lối sơ tán lên Safe Zone.

Rủi ro:

- Có thể bị vật phẩm hoặc nước cản trở.
- Large Object khó vận chuyển.
- Không được đặt Module làm hẹp lối đi.

Nếu Main Staircase không thể sử dụng:

- Upper Safe Area bị cô lập.
- Người chơi phải dùng External Access Route.
- Living Capacity và Storage Access giảm.

---

### 5.2. Structural Pillars

```text
core_id: structural_pillars
count: 3
```

Vai trò:

- Quyết định Structural Integrity của Shelter.
- Là điểm neo cho Structural Damage Event.
- Giới hạn vị trí xây Module.

Structural Pillar không thể được sửa bằng vật liệu thông thường nếu hư hỏng nghiêm trọng.

MVP chỉ cho phép:

- Gia cố tạm thời.
- Giảm tốc độ suy giảm.
- Sơ tán nếu kết cấu đạt trạng thái nguy hiểm.

---

### 5.3. Drain Core

```text
core_id: drain_core
location: Utility Area
connected_system: city_drainage_network
```

Vai trò:

- Thoát nước tầng dưới trong điều kiện bình thường.
- Là điểm kết nối Portable Pump.
- Là nguồn chính của Drain Backflow Event.

Trạng thái:

```text
Normal
Slow
Blocked
Backflow
Critical Backflow
```

Drain Core không thể bị loại bỏ.

Người chơi có thể:

- Làm sạch.
- Đóng van.
- Gắn Emergency Seal.
- Kết nối Pump.
- Chấp nhận bỏ Ground Floor.

---

### 5.4. Electrical Backbone

```text
core_id: electrical_backbone
location: Utility Area
```

Vai trò:

- Nhận điện từ City Grid.
- Phân phối điện tới Shelter Module.
- Kết nối Generator và Battery Bank.

Trạng thái:

```text
Stable
Unstable
Partial Failure
Offline
Flooded
```

Nếu Electrical Backbone bị ngập khi vẫn có điện:

- Tạo nguy cơ Electrified Water.
- Có thể làm hỏng Module.
- Buộc người chơi cắt điện tầng dưới.

---

### 5.5. Water Intake Point

```text
core_id: water_intake
location: Water Processing Area
```

Vai trò:

- Nhận nước máy trước khi hệ thống hỏng.
- Kết nối Water Storage và Water Purifier.
- Cho phép tích trữ Untreated Water trong Warning Phase.

Sau Black Rain Phase:

- Không còn được xem là nguồn nước an toàn.
- Có thể bị Contaminated nếu hệ thống thành phố chảy ngược.

---

### 5.6. Roof Antenna Mount

```text
core_id: antenna_mount
location: Roof
```

Vai trò:

- Lắp Communication Station.
- Cải thiện Signal Quality.
- Nhận Forecast và tín hiệu bất thường.

Rủi ro:

- Chịu Wind Hazard.
- Có thể cần sửa sau Event.
- Không thể sử dụng an toàn trong Peak nếu Wind đạt Critical.

---

## 6. Shelter Zone

Main Shelter có tám Zone.

```text
Entrance
Central Hall
Ground Storage
Utility Area
Water Processing Area
Workshop Area
Upper Living Area
Roof
```

---

## 7. Entrance

```text
zone_id: shelter_entrance
floor: Ground
build_slots: 2
water_risk: High
```

Vai trò:

- Điểm vào chính.
- Tiếp nhận Player, NPC và Resource.
- Phân loại đồ sạch, ướt và ô nhiễm.
- Lắp Flood Barrier.

Build Option:

- Flood Barrier.
- Wet Drop Zone.
- Temporary Light.
- Contaminated Container.

Event:

- Cửa bị rò.
- NPC cầu xin vào Shelter.
- Black Water tràn qua Entrance.
- Vật phẩm bị cuốn ra ngoài.

---

## 8. Central Hall

```text
zone_id: central_hall
floor: Ground
build_slots: 0
water_risk: Medium
```

Vai trò:

- Không gian kết nối.
- Chứa Main Staircase.
- Tuyến di chuyển bắt buộc.

Không được đặt Module cố định tại đây.

Có thể đặt tạm:

- Carried Object.
- Supply Crate.

Vật phẩm đặt tạm có thể:

- Cản lối.
- Làm chậm sơ tán.
- Bị ngập.

---

## 9. Ground Storage

```text
zone_id: ground_storage
floor: Ground
build_slots: 2
initial_capacity: 20 volume
water_risk: High
```

Vai trò:

- Kho chính đầu game.
- Chứa vật liệu và Large Object.
- Dễ vận chuyển từ Entrance.

Nhược điểm:

- Có nguy cơ ngập trong Escalation.
- Không thích hợp giữ Resource sống còn tới Peak.

Build Option:

- Storage Rack.
- Sealed Container.
- Elevated Storage nhỏ.
- Fuel Storage.

---

## 10. Utility Area

```text
zone_id: utility_area
floor: Ground
build_slots: 2
water_risk: Critical
```

Core Component:

- Drain Core.
- Electrical Backbone.

Build Option:

- Portable Pump.
- Generator Connection.
- Battery Bank.
- Emergency Drain Seal.

Đây là Zone có áp lực lớn nhất trong Peak.

Nếu bị mất:

- Pump không thể vận hành bình thường.
- Electrical Backbone phải bị ngắt.
- Ground Floor gần như không thể duy trì.

---

## 11. Water Processing Area

```text
zone_id: water_processing
floor: Ground
build_slots: 2
water_risk: High
```

Core Component:

- Water Intake Point.

Build Option:

- Water Purifier.
- Clean Water Tank.
- Untreated Water Tank.
- Cleaning Station.

Quy tắc:

- Clean Water và Black Water không được dùng chung Container.
- Zone mất Cleanliness có thể làm toàn bộ Batch đang xử lý bị Contaminated.
- Nên hoàn thành xử lý nước trước Peak.

---

## 12. Workshop Area

```text
zone_id: workshop
floor: Ground
build_slots: 1
water_risk: Medium
```

Vai trò:

- Repair.
- Crafting.
- Salvage.
- Construction preparation.

Build Option:

- Basic Workbench.
- Tool Rack.

Nếu Workshop bị ngập:

- Active Crafting dừng.
- Tool không được bảo vệ có thể bị hỏng.
- Recipe yêu cầu Workshop không còn khả dụng.

---

## 13. Upper Living Area

```text
zone_id: upper_living
floor: Upper
build_slots: 3
water_risk: Low
initial_living_capacity: 2
```

Vai trò:

- Ngủ.
- Nghỉ.
- Điều trị.
- Safe Zone.
- Bảo vệ Resource sống còn.

Build Option:

- Elevated Storage.
- Medical Station.
- Additional Bed.
- Emergency Lighting.
- Drying Station.

Giới hạn:

- Build Slot thấp.
- Large Object khó đưa lên.
- Quá tải làm giảm Living Capacity.
- Không được biến toàn bộ Zone thành Storage.

---

## 14. Roof

```text
zone_id: roof
floor: Roof
build_slots: 1
water_risk: None
wind_risk: High
```

Core Component:

- Roof Antenna Mount.

Vai trò:

- Observation Point.
- Communication Station.
- Rain Collection trước Black Rain.
- Emergency Signal.

Build Option:

- Antenna.
- Rain Collector.
- Emergency Beacon.

Rain Collector chỉ an toàn trước khi Black Rain bắt đầu.

---

## 15. Trạng thái ban đầu

Tại Normal Phase:

```text
Structural Integrity: 85
Water Intrusion: Dry
Power: City Grid
Clean Water: 3 Unit
Untreated Water: 0
Food: 2 Unit
Living Capacity: 2
Cleanliness: 80
Security: Basic
```

Module ban đầu:

- Basic Ground Storage.
- One Bed.
- Basic Electrical Lighting.
- Basic Sink.
- Không có Pump.
- Không có Water Purifier.
- Không có Communication Station.

---

## 16. Tài nguyên ban đầu

| Resource         | Số lượng |
| ---------------- | -------: |
| Clean Water      |        3 |
| Food             |        2 |
| Bandage          |        1 |
| Battery Charge   |        2 |
| Wood             |        2 |
| Basic Tool       |        1 |
| Dry Clothing Set |        1 |

Tài nguyên ban đầu không đủ để hoàn thành Chapter nếu không khám phá.

---

## 17. Build Slot

| Zone             | Slot |
| ---------------- | ---: |
| Entrance         |    2 |
| Ground Storage   |    2 |
| Utility Area     |    2 |
| Water Processing |    2 |
| Workshop         |    1 |
| Upper Living     |    3 |
| Roof             |    1 |

Không phải mọi Slot đều sử dụng được ngay.

Một Slot có thể bị khóa bởi:

- Vật cản.
- Core Component.
- Hazard.
- Thiếu Connection.
- Structural Damage.

---

## 18. Module Priority

### Tier 1 — Survival Foundation

- Flood Barrier.
- Water Storage.
- Portable Pump.
- Elevated Storage.

### Tier 2 — Stability

- Water Purifier.
- Battery Bank.
- Drying Station.
- Medical Station.

### Tier 3 — Strategic Advantage

- Communication Station.
- Signal Stabilizer.
- Expanded Workshop.

Người chơi không đủ thời gian và tài nguyên để hoàn thành toàn bộ.

---

## 19. Power Network

Power Supply được phân bổ qua Electrical Backbone.

### City Grid

| Phase      | Trạng thái      |
| ---------- | --------------- |
| Normal     | Stable          |
| Warning    | Stable          |
| First Rain | Unstable        |
| Black Rain | Partial Failure |
| Escalation | Failed          |
| Peak       | Failed          |
| Aftermath  | Failed          |

### Power Priority

Người chơi có thể đặt ưu tiên:

```text
Critical
High
Normal
Disabled
```

Module Critical được cấp điện trước.

---

## 20. Power Strategy

Ba cấu hình phổ biến:

### Flood Defense

```text
Portable Pump: Critical
Emergency Lighting: High
Communication: Disabled
Water Purifier: Disabled
```

### Resource Stability

```text
Water Purifier: Critical
Battery Charging: High
Portable Pump: Normal
Communication: Disabled
```

### Information and Evacuation

```text
Communication: Critical
Emergency Lighting: High
Portable Pump: Normal
Water Purifier: Disabled
```

Không có cấu hình tối ưu cho mọi tình huống.

---

## 21. Water Intrusion Model

Water Intrusion tăng từ ba nguồn:

```text
Entrance Leakage
Drain Backflow
Structural Seepage
```

Công thức logic:

```text
water_gain
=
entrance_inflow
+
drain_inflow
+
structural_inflow
-
pump_output
-
passive_drain_output
```

---

## 22. Water Intrusion Level

```text
Dry
Damp
Shallow Flood
Deep Flood
Critical Flood
```

| Mức      | Hậu quả                                   |
| -------- | ----------------------------------------- |
| Dry      | Hoạt động bình thường                     |
| Damp     | Item không bảo vệ có nguy cơ Wet          |
| Shallow  | Di chuyển chậm, Storage thấp bị ảnh hưởng |
| Deep     | Module điện tầng dưới bị khóa             |
| Critical | Ground Floor không thể duy trì            |

---

## 23. Shelter Transition theo Phase

### Normal

- Shelter hoạt động bình thường.
- Tutorial Storage và Rest.

### Warning

- Cho phép tích trữ nước.
- Mở Building System.
- Có thể kiểm tra Drain Core.

### First Rain

- Entrance Leakage bắt đầu.
- Electrical Backbone có cảnh báo.
- Wet Item xuất hiện.

### Black Rain

- Water Intrusion chuyển sang Contaminated.
- Entrance cần phân loại vật phẩm.
- Ground Storage bắt đầu có nguy cơ.

### Escalation

- City Grid thất bại.
- Drain Backflow bắt đầu.
- Người chơi phải chuyển Resource lên cao.
- Temporary Shelter có thể được kích hoạt.

### Peak

- Pump Jam Event.
- Storage Flood Event.
- Power Allocation Event.
- Safe Zone Capacity được kiểm tra.

### Aftermath

- Đánh giá Structural Damage.
- Kiểm kê Resource.
- Xác định Main Shelter còn sử dụng được hay không.

---

## 24. Main Shelter Event Anchor

| Event                | Nguồn                      |
| -------------------- | -------------------------- |
| Entrance Leakage     | Flood State                |
| Electrical Flicker   | Grid instability           |
| Drain Backflow       | Drainage failure           |
| Pump Jam             | Debris và Module Condition |
| Storage Flood        | Water Intrusion            |
| Contamination Spread | Storage Rule violation     |
| Generator Overheat   | Overload hoặc Maintenance  |
| Roof Signal          | Communication Station      |
| Structural Crack     | Water Pressure             |
| Forced Evacuation    | Shelter Failure            |

---

## 25. Shelter Failure Path

### Lower Floor Lost

Điều kiện:

```text
water_intrusion >= Deep
AND
pump_output < inflow
```

Hậu quả:

- Ground Storage bị mất.
- Workshop bị khóa.
- Utility Module ngừng hoạt động.
- Upper Floor vẫn có thể duy trì.

---

### Power Collapse

Điều kiện:

```text
city_grid == Failed
AND
generator unavailable
AND
battery_charge == 0
```

Hậu quả:

- Passive Module dừng.
- Emergency Lighting mất.
- Communication Station ngừng hoạt động.

Power Collapse không tự động gây Game Over.

---

### Safe Zone Failure

Điều kiện:

- Upper Living Area không thể sử dụng.
- Main Staircase bị mất.
- Structural Integrity nguy hiểm.
- Living Capacity thấp hơn số người không thể sơ tán.

Đây là điều kiện buộc phải Forced Evacuation.

---

## 26. Forced Evacuation

Điểm đến chính:

```text
Temporary Shelter: Trường học
```

Điều kiện:

- Trường học đã được khảo sát.
- Upper Floor đã được mở.
- Route còn khả dụng.
- Có ít nhất một Safe Entry.

Người chơi phải ưu tiên mang:

1. Người bị thương.
2. Clean Water.
3. Medicine.
4. Food.
5. Essential Tool.
6. Portable Module.
7. Material.

Core Component và Fixed Module bị bỏ lại.

---

## 27. NPC Task tại Shelter

NPC có thể được phân công:

- Theo dõi Water Intrusion.
- Vận hành Pump.
- Xử lý nước.
- Repair Module.
- Điều trị.
- Canh Entrance.
- Theo dõi Radio.
- Chuyển Resource lên tầng cao.

NPC không thể thực hiện Task khi:

- Fatigue quá cao.
- Bị Injury nặng.
- Trust quá thấp với Task nguy hiểm.
- Không có Tool hoặc Skill phù hợp.

---

## 28. Multiplayer Rule

- Shelter State là dữ liệu chung.
- Power Priority được đồng bộ.
- Storage là kho chung.
- Build Slot chỉ có thể chứa một Module hợp lệ.
- Người chơi có thể hoạt động tại các Zone khác nhau.
- Critical Event không dừng World Clock.
- Forced Evacuation yêu cầu vận chuyển vật lý.

---

## 29. UI Requirement

Shelter Overview phải hiển thị:

```text
Structural Integrity
Water Intrusion
Power Supply
Power Demand
Clean Water
Occupants
Living Capacity
Storage Risk
Critical Module
Active Event
```

Mỗi cảnh báo phải nêu:

- Vấn đề.
- Nguyên nhân.
- Hậu quả.
- Thời gian phản ứng dự kiến.
- Hành động khả dụng.

---

## 30. Dữ liệu hệ thống

```text
main_shelter
├── structural_integrity
├── water_intrusion
├── cleanliness
├── security
├── living_capacity
├── occupants
├── core_components
├── zones
├── build_slots
├── modules
├── power_state
├── water_state
├── storage_state
├── active_tasks
├── passive_tasks
└── event_flags
```

---

## 31. Phạm vi MVP

Triển khai:

- Ba tầng chức năng.
- Tám Zone.
- Sáu Fixed Core Component.
- Build Slot.
- Power Priority.
- Water Intrusion.
- Drain Backflow.
- Clean, Wet và Contaminated Storage.
- Upper Safe Area.
- Shelter Event.
- Forced Evacuation.

Chưa triển khai:

- Thay đổi kiến trúc Shelter.
- Xây thêm tầng.
- Di chuyển Core Component.
- Mạng điện và ống nước vật lý chi tiết.
- Hệ thống trang trí.
- Phòng thủ chiến đấu chuyên sâu.

---

## 32. Quyết định chốt

- Main Shelter là nhà dân hai tầng tại Elevation `E2`.
- Shelter có cấu trúc và Zone cố định.
- Core Component không thể di chuyển hoặc tháo dỡ.
- Ground Floor cung cấp phần lớn chức năng nhưng chịu rủi ro ngập.
- Upper Living Area là Safe Zone cuối cùng.
- Drain Core là Hazard Anchor chính.
- Electrical Backbone là điểm phân phối Power chung.
- Không thể bảo vệ toàn bộ Shelter trong một lượt chơi.
- Lower Floor có thể bị mất mà Chapter vẫn tiếp tục.
- Trường học là điểm Forced Evacuation chính.
