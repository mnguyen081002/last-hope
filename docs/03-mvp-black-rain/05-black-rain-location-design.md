# 05-black-rain-location-design.md

## 1. Mục tiêu

Tài liệu này xác định thiết kế chi tiết cho các Location trong MVP Siêu Bão Mưa Đen.

Mỗi Location phải:

- Có vai trò chiến lược riêng.
- Có thời điểm giá trị cao nhất.
- Thay đổi theo Disaster Phase.
- Có Ordinary Loot bị depletion vĩnh viễn.
- Có ít nhất một Return Hook hợp lý.
- Có Hazard và Access Rule riêng.
- Có Event hoặc Information liên quan.
- Không trở thành vô dụng hoàn toàn sau khi đã loot.

---

## 2. Cấu trúc Location

Mỗi Location sử dụng cấu trúc:

```text
Location
├── Zone
├── Entrance
├── Search Point
├── Hazard State
├── Resource Profile
├── Event Anchor
├── Return Hook
└── Persistent State
```

---

## 3. Exploration State

```text
Unknown
Discovered
Entered
Partially Explored
Main Objective Completed
Fully Explored
Depleted
Changed
Inaccessible
Destroyed
```

Một Location có thể đồng thời:

- Depleted về Ordinary Loot.
- Changed do Disaster.
- Vẫn có Event hoặc Route Function.

---

## 4. Loot Rule

Ordinary Loot không respawn.

Mỗi Search Point định nghĩa:

```text
resource_pool
search_duration
access_requirement
contamination_risk
depletion_state
```

Loot quan trọng dùng Controlled Placement thay vì RNG hoàn toàn.

---

## 5. Location Content Budget

| Location          | Zone | Search Point | Thời lượng mục tiêu |
| ----------------- | ---: | -----------: | ------------------: |
| Khu nhà dân       |    4 |         8–10 |          12–18 phút |
| Cửa hàng tiện lợi |    4 |         8–12 |          12–20 phút |
| Hiệu thuốc        |    5 |        10–12 |          18–25 phút |
| Gara điện nước    |    6 |        12–15 |          22–30 phút |
| Trường học        |    7 |        14–18 |          25–35 phút |
| Trạm bơm          |    6 |        10–14 |          25–35 phút |
| Trạm thời tiết    |    5 |         9–12 |          20–30 phút |

Thời lượng không bao gồm Travel Time.

---

# 6. Khu nhà dân

## 6.1. Identity

```text
location_id: residential_block
district: Basin Core
elevation: E2
distance: Near
primary_role: Tutorial và tài nguyên cơ bản
secondary_role: NPC Event và Roof Route
```

---

## 6.2. Mục tiêu

### Main Objective

- Khảo sát khu vực.
- Thu thập vật tư đầu tiên.
- Gặp Người hàng xóm.

### Optional Objective

- Tìm Dry Clothing.
- Mở Roof Access.
- Thu thập bản đồ khu dân cư.
- Cứu người thân của NPC trong Event sau.

---

## 6.3. Zone

```text
Street Entrance
House Interior
Shared Courtyard
Roof Access
```

---

## 6.4. Entrance

### Street Entrance

- Luôn mở trong Normal và Warning.
- Bị Shallow Flood trong First Rain.
- Bị Deep Flood trong Peak.

### Roof Access

- Yêu cầu Crowbar hoặc Rope.
- Trở thành lối chính trong Escalation.
- Kết nối tuyến mái về phía Trường học.

---

## 6.5. Resource Profile

Controlled Resource:

| Resource       | Khoảng |
| -------------- | -----: |
| Food           |    2–3 |
| Clean Water    |    1–2 |
| Dry Clothing   |    1–2 |
| Battery Charge |    1–2 |
| Wood           |    1–3 |
| Basic Tool     |    0–1 |

Guaranteed:

- Một Dry Clothing Set.
- Một Resource Package gồm Food hoặc Water.
- Một bản đồ khu vực.

---

## 6.6. Hazard theo Phase

| Phase      | Hazard                                     |
| ---------- | ------------------------------------------ |
| Normal     | Không đáng kể                              |
| Warning    | NPC activity                               |
| First Rain | Wet, Shallow Flood                         |
| Black Rain | Contaminated Courtyard                     |
| Escalation | Ground Floor mất khả năng sử dụng          |
| Peak       | Street Entrance gần như không thể tiếp cận |
| Aftermath  | Bùn và Contamination                       |

---

## 6.7. Event Anchor

- Neighbor Introduction.
- Missing Relative.
- Roof Signal.
- Abandoned Supplies.
- House Collapse nhẹ.

---

## 6.8. Return Hook

- Roof Access mở Route mới.
- Người thân NPC xuất hiện sau Phase Black Rain.
- Một căn nhà khóa cần chìa khóa.
- Đồ trên tầng cao chỉ tiếp cận khi có Rope.
- Có thể dùng làm Observation Point phụ.

---

## 6.9. Persistent State

```text
residents_rescued
roof_route_opened
ground_floor_flooded
residential_supplies_depleted
```

---

# 7. Cửa hàng tiện lợi

## 7.1. Identity

```text
location_id: convenience_store
district: Commercial Lowlands
elevation: E1
distance: Near
primary_role: Food và Clean Water đầu game
secondary_role: Resource Race
```

---

## 7.2. Mục tiêu

### Main Objective

- Thu hồi Food và Clean Water trước khi khu vực bị ngập hoặc bị loot.

### Optional Objective

- Mở kho giao hàng.
- Thu hồi Waterproof Container.
- Xử lý xung đột với nhóm dân thường.

---

## 7.3. Zone

```text
Front Shop
Back Storage
Delivery Bay
Roof Sign Access
```

---

## 7.4. Entrance

### Front Entrance

- Nhanh.
- Dễ tiếp cận trong Warning.
- Bị ngập sớm.

### Delivery Bay

- Yêu cầu Crowbar hoặc mã khóa.
- Ít bị người khác loot.
- Chứa Resource giá trị cao hơn.

### Roof Sign Access

- Chỉ dùng sau Black Rain.
- Yêu cầu Rope.
- Không phù hợp mang Large Object.

---

## 7.5. Resource Profile

| Resource            | Khoảng |
| ------------------- | -----: |
| Food                |    4–6 |
| Clean Water         |    3–5 |
| Battery Charge      |    1–2 |
| Container           |    1–2 |
| Waterproof Material |    0–2 |

Guaranteed nếu đến trước khi hết Warning:

- Hai Clean Water.
- Ba Food.

Nếu tới muộn:

- Một phần Resource chuyển sang Taken hoặc Contaminated.

---

## 7.6. Resource Race

Location State theo thời gian:

```text
Stocked
Crowded
Partially Looted
Flooded
Depleted
```

Người chơi không bị buộc phải tới đây.

Nguồn thay thế:

- Khu nhà dân.
- Trường học.
- Water Purifier.

---

## 7.7. Hazard theo Phase

| Phase      | Hazard                        |
| ---------- | ----------------------------- |
| Warning    | Crowd và Resource Competition |
| First Rain | Shallow Flood                 |
| Black Rain | Medium Flood, Contamination   |
| Escalation | Front Shop Deep Flood         |
| Peak       | Inaccessible                  |
| Aftermath  | Contaminated và Depleted      |

---

## 7.8. Event Anchor

- Resource Rush.
- Civilian Dispute.
- Delivery Bay Collapse.
- Floating Supply Package.

---

## 7.9. Return Hook

- Delivery Bay mở khi cửa cuốn hỏng.
- Vật tư trên mái có thể xuất hiện qua Event.
- Location trở thành Landmark cho Low Route.
- Có thể dùng để kiểm tra Flood State khu thương mại.

---

## 7.10. Persistent State

```text
store_looted_level
delivery_bay_open
civilian_group_relation
commercial_flood_confirmed
```

---

# 8. Hiệu thuốc

## 8.1. Identity

```text
location_id: pharmacy
district: Commercial Lowlands
elevation: E1
distance: Medium
primary_role: Medicine
secondary_role: Medical NPC và Black Water Intel
```

---

## 8.2. Mục tiêu

### Main Objective

- Thu hồi vật tư điều trị.

### Optional Objective

- Cứu Nhân viên y tế.
- Mở kho thuốc tầng trên.
- Thu thập tài liệu về dấu hiệu nhiễm nước.
- Cắt nguồn điện khỏi khu vực ngập.

---

## 8.3. Zone

```text
Front Retail
Consultation Room
Upper Pharmacy Storage
Electrical Room
Rear Alley
```

---

## 8.4. Entrance

### Front Entrance

- Dễ tiếp cận đầu game.
- Bị ngập nhanh.

### Rear Alley

- Dài hơn.
- Ít nước hơn.
- Có Structural Risk.

### Upper Window

- Yêu cầu Rope.
- Cho phép tiếp cận kho thuốc sau khi tầng dưới bị khóa.

---

## 8.5. Resource Profile

| Resource          | Khoảng |
| ----------------- | -----: |
| Medicine          |    3–4 |
| Bandage           |    2–3 |
| Antiseptic        |    1–2 |
| Purification Item |    1–2 |
| Clean Water       |    0–1 |

Guaranteed:

- Hai lần điều trị cơ bản.
- Một Antiseptic.
- Một tài liệu Medical Intel.

---

## 8.6. Hazard theo Phase

| Phase      | Hazard                    |
| ---------- | ------------------------- |
| Warning    | Cửa khóa một phần         |
| First Rain | Nước vào Front Retail     |
| Black Rain | Black Water Exposure      |
| Escalation | Electrified Water         |
| Peak       | Tầng dưới Inaccessible    |
| Aftermath  | Medicine tầng thấp bị phá |

---

## 8.7. Electrical Hazard

Nguồn:

- Tủ điện tầng dưới.
- Thiết bị bị ngập.

Giải pháp:

- Cắt nguồn từ Electrical Room.
- Đi qua Upper Window.
- Chờ khu vực mất điện.
- Dùng Equipment cách điện phù hợp.

---

## 8.8. Event Anchor

- Medical Rescue.
- Electrified Water.
- Locked Medicine Storage.
- Sick Civilian.
- Contaminated Supply.

---

## 8.9. Return Hook

- Nhân viên y tế mở kho thuốc.
- Upper Storage chỉ mở bằng chìa khóa hoặc Skill.
- Tài liệu có thể mở Treatment Option.
- Có thể quay lại lấy Medicine sau khi điện bị cắt.

---

## 8.10. Persistent State

```text
medical_npc_rescued
pharmacy_power_cut
upper_storage_opened
medicine_stock_depleted
```

---

# 9. Gara điện nước

## 9.1. Identity

```text
location_id: utility_garage
district: Utility Fringe
elevation: E2
distance: Medium
primary_role: Tool, Material và Pump Part
secondary_role: Technician NPC và Blueprint
```

---

## 9.2. Mục tiêu

### Main Objective

- Thu hồi Tool và Pump Part.

### Optional Objective

- Cứu Kỹ thuật viên thoát nước.
- Khôi phục Workshop Power.
- Mở Blueprint.
- Vận chuyển Portable Pump hoặc Generator Part.

---

## 9.3. Zone

```text
Front Workshop
Tool Cage
Service Pit
Parts Storage
Office Mezzanine
Rear Maintenance Yard
```

---

## 9.4. Entrance

### Front Shutter

- Yêu cầu Power hoặc Crowbar.
- Phù hợp vận chuyển Large Object.

### Side Door

- Dễ vào.
- Không đưa được Large Object qua.

### Rear Yard

- Kết nối Utility Route.
- Là đầu của Elevated Service Link.

---

## 9.5. Resource Profile

| Resource             | Khoảng |
| -------------------- | -----: |
| Metal                |    4–6 |
| Pump Part            |    2–3 |
| Electronic Component |    2–3 |
| Battery Charge       |    2–3 |
| Fuel                 |    1–3 |
| Tool                 |    2–4 |
| Rope                 |      1 |

Guaranteed:

- Một Pump Part.
- Một Tool mở lối.
- Một Electronic Component.

Large Object:

- Portable Pump.
- Generator Component.
- Heavy Tool Case.

Mỗi lượt chỉ cần xuất hiện một đến hai Large Object chính.

---

## 9.6. Hazard theo Phase

| Phase      | Hazard                 |
| ---------- | ---------------------- |
| Warning    | Locked access          |
| First Rain | Service Pit ngập       |
| Black Rain | Tool contamination     |
| Escalation | Structural Instability |
| Peak       | Rear Yard Deep Flood   |
| Aftermath  | Partial Collapse       |

---

## 9.7. Structural Risk

Service Pit và mái Workshop chịu rủi ro cao.

Cảnh báo:

- Tiếng kim loại biến dạng.
- Vết nứt.
- Nước dâng từ hố sửa xe.

Người chơi có thể:

- Gia cố tạm.
- Tránh Zone.
- Thu hồi nhanh Resource.
- Chấp nhận Location bị mất.

---

## 9.8. Event Anchor

- Technician Rescue.
- Workshop Power Restore.
- Service Pit Flood.
- Structural Collapse.
- Heavy Equipment Recovery.

---

## 9.9. Return Hook

- Cần hai người để vận chuyển Large Object.
- Workshop Power mở Blueprint.
- Shortcut tới Trường học được xây từ Rear Yard.
- Tool Cage cần mã hoặc NPC Skill.

---

## 9.10. Persistent State

```text
technician_rescued
workshop_power_restored
tool_cage_opened
service_link_started
garage_collapse_state
```

---

# 10. Trường học

## 10.1. Identity

```text
location_id: school
district: Civic Ridge
elevation: E3
distance: Medium
primary_role: Temporary Shelter và NPC Rescue
secondary_role: Food Reserve và Observation Point
```

---

## 10.2. Mục tiêu

### Main Objective

- Khảo sát tầng trên làm Temporary Shelter.
- Phản ứng tín hiệu cầu cứu.

### Optional Objective

- Cứu nhóm dân thường.
- Mở kho thực phẩm.
- Khôi phục lối lên mái.
- Thiết lập Elevated Service Link.

---

## 10.3. Zone

```text
Main Gate
Ground Hall
Classroom Wing
Cafeteria
Upper Hall
Gym Storage
Roof
```

---

## 10.4. Entrance

### Main Gate

- Dễ tiếp cận trong Warning.
- Tầng trệt ngập trong Black Rain.

### Side Stairwell

- Cần dọn vật cản.
- Giữ khả năng tiếp cận lâu hơn.

### Roof Access

- Có thể tiếp cận bằng Rope.
- Là tuyến cứu hộ trong Escalation.

### Elevated Service Link

- Kết nối Gara.
- Yêu cầu xây dựng.

---

## 10.5. Resource Profile

| Resource       | Khoảng |
| -------------- | -----: |
| Food           |    3–5 |
| Clean Water    |    1–3 |
| Fabric         |    2–4 |
| Medical Supply |    1–2 |
| Rope           |    1–2 |
| Wood           |    2–4 |

Guaranteed:

- Một Resource Package đủ hỗ trợ Temporary Shelter.
- Một Rope hoặc Material cho Roof Access.

---

## 10.6. Temporary Shelter Requirement

Để kích hoạt:

```text
upper_hall_secured == true
AND
safe_entry_available == true
AND
basic_storage_installed == true
```

Nâng cấp tùy chọn:

- Additional Bed.
- Emergency Water Storage.
- Medical Corner.
- Signal Light.

---

## 10.7. Hazard theo Phase

| Phase      | Hazard                          |
| ---------- | ------------------------------- |
| Warning    | Civilian activity               |
| First Rain | Main Gate Shallow Flood         |
| Black Rain | Ground Hall ngập                |
| Escalation | Stairwell blocked, Roof Rescue  |
| Peak       | Ground Floor mất hoàn toàn      |
| Aftermath  | Temporary Shelter vẫn hoạt động |

---

## 10.8. Event Anchor

- School Rescue Signal.
- Civilian Group.
- Roof Evacuation.
- Temporary Shelter Activation.
- Food Reserve Dispute.

---

## 10.9. Return Hook

- Temporary Shelter.
- NPC còn lại được sơ tán tới đây.
- Roof Observation Point.
- Elevated Service Link.
- Forced Evacuation Destination.

---

## 10.10. Persistent State

```text
temporary_shelter_active
school_survivor_count
roof_access_open
food_storage_state
service_link_complete
```

---

# 11. Trạm bơm khu vực

## 11.1. Identity

```text
location_id: regional_pump_station
district: Utility Fringe
elevation: E0
distance: Far
primary_role: Infrastructure Choice
secondary_role: Pump Part và Flood Control
```

---

## 11.2. Mục tiêu

### Main Objective

Chọn một trong hai:

```text
Restore Regional Pump
OR
Salvage Pump Components
```

### Optional Objective

- Cứu Kỹ thuật viên nếu chưa gặp tại Gara.
- Cắt nguồn điện khu vực.
- Mở tuyến bảo trì.
- Thu thập Drainage Intel.

---

## 11.3. Zone

```text
Outer Flood Channel
Control Room
Generator Room
Pump Hall
Maintenance Tunnel
Upper Platform
```

---

## 11.4. Entrance

### Main Service Road

- Nhanh.
- Ngập sớm.
- Chịu Current Strength cao.

### Upper Platform

- Yêu cầu Rope hoặc Route từ Utility Garage.
- Phù hợp sau Black Rain.

### Maintenance Tunnel

- Chỉ mở bằng Intel.
- Có nguy cơ Backflow.
- Có thể trở thành Shortcut cục bộ.

---

## 11.5. Resource Profile

| Resource             | Khoảng |
| -------------------- | -----: |
| Pump Part            |    2–4 |
| Fuel                 |    2–3 |
| Metal                |    2–4 |
| Electronic Component |    1–2 |
| Drainage Intel       |      1 |

Resource nhận được phụ thuộc lựa chọn khôi phục hay tháo linh kiện.

---

## 11.6. Restore Option

Yêu cầu baseline:

```text
2 Pump Part
2 Electronic Component
2 giờ Active Work
Power available
```

Kết quả:

- Utility Fringe Flood giảm.
- Route bị khóa chậm hơn.
- Main Shelter Water Intrusion giảm nhẹ.
- World Impact tăng.

---

## 11.7. Salvage Option

Kết quả:

```text
2–3 Pump Part
1 Electronic Component
1 Metal
```

Hậu quả:

- Trạm bơm không thể khôi phục.
- Flood State khu thấp tăng nhanh.
- Main Shelter có Resource trực tiếp.

---

## 11.8. Hazard theo Phase

| Phase      | Hazard                            |
| ---------- | --------------------------------- |
| Warning    | Mechanical Hazard                 |
| First Rain | Medium Flood                      |
| Black Rain | Deep Contaminated Water           |
| Escalation | Strong Current, Electrified Water |
| Peak       | Inaccessible                      |
| Aftermath  | Structural Damage                 |

---

## 11.9. Event Anchor

- Regional Pump Failure.
- Technician Signal.
- Generator Restart.
- Flood Gate Jam.
- Infrastructure Choice.

---

## 11.10. Return Hook

- Quay lại hoàn thành khôi phục.
- Lấy Resource còn lại sau khi cắt điện.
- Dùng Maintenance Tunnel.
- Kiểm tra Persistent Infrastructure State.

---

## 11.11. Persistent State

```text
regional_pump_restored
regional_pump_salvaged
flood_gate_state
maintenance_tunnel_open
utility_flood_modifier
```

---

# 12. Trạm thời tiết

## 12.1. Identity

```text
location_id: weather_station
district: Civic Ridge
elevation: E4
distance: Far
primary_role: Forecast và Narrative Intel
secondary_role: Electronic Resource
```

---

## 12.2. Mục tiêu

### Main Objective

- Khôi phục hoặc truy xuất dữ liệu dự báo.

### Optional Objective

- Cứu Người vận hành radio.
- Thu thập Signal Data.
- Mang thiết bị về Shelter.
- Giữ trạm hoạt động tới Peak.

---

## 12.3. Zone

```text
Access Road
Observation Deck
Control Room
Equipment Room
Antenna Platform
```

---

## 12.4. Entrance

### Access Road

- Ít ngập.
- Chịu Wind Hazard.
- Có vật cản sau Escalation.

### Maintenance Ladder

- Yêu cầu Tool.
- Cho phép tiếp cận Control Room.

### Antenna Platform

- Chỉ an toàn trước Peak.
- Chịu Interference cao.

---

## 12.5. Resource Profile

| Resource             | Khoảng |
| -------------------- | -----: |
| Electronic Component |    2–3 |
| Battery Charge       |    2–4 |
| Antenna Part         |      1 |
| Signal Data          |      1 |
| Forecast Record      |      1 |

Guaranteed:

- Forecast Record.
- Một Electronic Component.
- Một Narrative Clue nếu hoàn thành Main Objective.

---

## 12.6. Strategic Choice

### Duy trì trạm

Yêu cầu:

- Repair.
- Power.
- Người vận hành hoặc Skill phù hợp.

Lợi ích:

- Forecast chính xác hơn.
- Peak Signal được xác minh.
- Information Score tăng.

### Tháo thiết bị

Lợi ích:

- Communication Station dễ xây.
- Nhận Battery và Electronic Component.
- Trạm không còn cung cấp dữ liệu từ xa.

---

## 12.7. Hazard theo Phase

| Phase      | Hazard                        |
| ---------- | ----------------------------- |
| Warning    | Không đáng kể                 |
| First Rain | Wind                          |
| Black Rain | Interference                  |
| Escalation | High Wind, Signal Distortion  |
| Peak       | Antenna Platform Inaccessible |
| Aftermath  | Signal vẫn tồn tại            |

---

## 12.8. Event Anchor

- Operator Distress Signal.
- Forecast Data Recovery.
- Antenna Failure.
- Peak Signal.
- Unknown Transmission.

---

## 12.9. Return Hook

- Cần NPC hoặc Skill để giải mã dữ liệu.
- Trạm có thể tiếp tục gửi Forecast.
- Thiết bị nặng cần quay lại.
- Narrative Signal chỉ xuất hiện trong Peak.

---

## 12.10. Persistent State

```text
weather_station_restored
weather_equipment_salvaged
operator_rescued
forecast_data_acquired
peak_signal_recorded
```

---

# 13. Critical Resource Guarantee

Mỗi lượt chơi phải bảo đảm:

- Clean Water xuất hiện tại ít nhất ba Location.
- Medicine xuất hiện tại ít nhất hai Location.
- Pump Part xuất hiện tại Gara và Trạm bơm.
- Rope xuất hiện tại ít nhất hai Location.
- Electronic Component xuất hiện tại ít nhất ba Location.
- Temporary Shelter có thể kích hoạt mà không cần Loot ngẫu nhiên hiếm.

---

# 14. Location State Update

Location được cập nhật khi:

- Disaster Phase đổi.
- Event xảy ra.
- Người chơi hoàn thành Objective.
- Infrastructure State thay đổi.
- Người chơi vào Location.
- World Clock đạt mốc cục bộ.

Không cần mô phỏng đầy đủ Location khi người chơi không có mặt.

---

# 15. Multiplayer Rule

- Location State là dữ liệu chung.
- Search Point đã depletion không thể được người khác loot lại.
- Người chơi có thể chia Zone để Search.
- Event chỉ tồn tại một lần.
- Large Object có thể yêu cầu hai người mang.
- Personal Observation chỉ được chia sẻ khi có liên lạc.

---

# 16. Dữ liệu hệ thống

```text
location_definition
├── location_id
├── district
├── elevation
├── zones
├── entrances
├── search_points
├── resource_profile
├── hazard_profile
├── event_anchors
├── main_objective
├── optional_objectives
├── return_hooks
└── persistent_flags
```

---

# 17. Phạm vi MVP

Triển khai:

- Bảy Location chính.
- Zone và Entrance riêng.
- Search Point persistent.
- Hazard transition theo Phase.
- Controlled Resource Placement.
- Main và Optional Objective.
- Event Anchor.
- Return Hook.
- Persistent State.

Chưa triển khai:

- Procedural Location.
- Loot respawn.
- Hàng chục tòa nhà phụ.
- Destruction vật lý toàn phần.
- NPC population simulation.
- Faction occupation phức tạp.

---

# 18. Quyết định chốt

- Mỗi Location có một vai trò chiến lược chính.
- Location giá trị cao ở các Phase khác nhau.
- Ordinary Loot không respawn.
- Location đã depletion vẫn có thể giữ Route, Event hoặc Shelter Function.
- Location quan trọng có Alternative Access.
- Cửa hàng và Hiệu thuốc mất khả năng tiếp cận sớm nhất.
- Trường học là Temporary Shelter.
- Trạm bơm tạo lựa chọn World Impact hoặc Shelter Resource.
- Trạm thời tiết tạo Forecast và Narrative Progression.
