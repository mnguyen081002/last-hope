# 03-black-rain-world-map.md

## 1. Mục tiêu

Tài liệu này xác định cấu trúc World Map của MVP Siêu Bão Mưa Đen.

World Map phải:

- Là công cụ ra quyết định.
- Thay đổi theo Disaster Timeline.
- Thể hiện sự khác biệt về độ cao.
- Tạo nhiều tuyến tiếp cận.
- Buộc người chơi cân nhắc thời gian, Hazard và Carry Load.
- Không cho phép ghé toàn bộ Location trong một lượt.
- Hỗ trợ Shelter Relocation và Event.

---

## 2. Phạm vi bản đồ

MVP sử dụng một khu vực thành phố nhỏ, mật độ cao.

Cấu trúc:

```text
1 Main Shelter
1 Temporary Shelter
7 Main Location
3 Main Route
1 Unlockable Shortcut
4 District
```

Bản đồ không phải Open World liên tục toàn thành phố.

Bản đồ được triển khai dưới dạng:

```text
Location Node
+
Playable Route Segment
+
Local Zone
```

---

## 3. District

## Basin Core

Đặc điểm:

- Độ cao trung bình thấp.
- Có Main Shelter.
- Là trung tâm kết nối các Route.
- Bắt đầu ngập từ First Rain.
- Chịu Drain Backflow trong Escalation.

Location:

- Main Shelter.
- Khu nhà dân.

---

## Commercial Lowlands

Đặc điểm:

- Độ cao thấp.
- Nguồn Food, Water và Medicine lớn.
- Dễ tiếp cận đầu game.
- Ngập nhanh nhất.
- Bị khóa gần như hoàn toàn trong Peak.

Location:

- Cửa hàng tiện lợi.
- Hiệu thuốc.

---

## Utility Fringe

Đặc điểm:

- Có hạ tầng điện nước.
- Nhiều Tool, Fuel và Pump Part.
- Hazard điện và dòng chảy cao.
- Có thể ảnh hưởng Flood State của toàn khu vực.

Location:

- Gara điện nước.
- Trạm bơm khu vực.

---

## Civic Ridge

Đặc điểm:

- Độ cao cao.
- Ít ngập.
- Khoảng cách xa.
- Tuyến tiếp cận dài.
- Có Temporary Shelter và Forecast.

Location:

- Trường học.
- Trạm thời tiết.

---

## 4. Elevation Tier

Mỗi Location và Route Segment có Elevation Tier.

```text
E0: Drainage Level
E1: Lowland
E2: Mid-Level
E3: High Ground
E4: Ridge
```

Elevation ảnh hưởng:

- Thời điểm ngập.
- Flood Depth.
- Current Strength.
- Khả năng sơ tán.
- Giá trị chiến lược trong Peak.

---

## 5. Sơ đồ bản đồ

```text
                         [Trạm thời tiết]
                               E4
                                |
                                |
                           [Trường học]
                               E3
                            /        \
                           /          \
              [Khu nhà dân]          [Gara điện nước]
                    E2                      E2
                      \                    /
                       \                  /
                        [Main Shelter]
                              E2
                               |
                               |
                    [Cửa hàng tiện lợi]
                              E1
                               |
                          [Hiệu thuốc]
                              E1
                               |
                       [Trạm bơm khu vực]
                              E0
```

Unlockable Shortcut:

```text
Gara điện nước
↔
Trường học
```

Shortcut sử dụng lối bảo trì trên cao và cầu tạm.

---

## 6. Main Shelter Position

Main Shelter nằm tại Basin Core, Elevation `E2`.

Vai trò:

- Điểm xuất phát.
- Trung tâm lưu trữ.
- Điểm nối ba Route chính.
- Vị trí có khả năng duy trì tới Peak nếu được chuẩn bị.

Nhược điểm:

- Drain Core nối với hệ thống thấp hơn.
- Tầng dưới có thể ngập.
- Không nằm trên vị trí cao nhất.
- Forced Evacuation cần di chuyển về Trường học.

---

## 7. Main Route

## Route A — Low Commercial Route

```text
Main Shelter
↓
Cửa hàng tiện lợi
↓
Hiệu thuốc
↓
Trạm bơm khu vực
```

### Đặc điểm

- Nhanh.
- Nhiều tài nguyên sống còn.
- Ngập sớm.
- Dễ xuất hiện Electrified Water.
- Bị khóa trước Peak.

### Vai trò

Đây là Route có giá trị cao nhất trong Warning và First Rain.

---

## Route B — Residential Route

```text
Main Shelter
↓
Khu nhà dân
↓
Trường học
↓
Trạm thời tiết
```

### Đặc điểm

- Dài hơn Route A.
- Có nhiều NPC Event.
- Ít ngập hơn.
- Là tuyến sơ tán chính.
- Một số đoạn chịu Wind Hazard.

### Vai trò

Đây là Route cân bằng giữa an toàn, NPC và Information.

---

## Route C — Utility Route

```text
Main Shelter
↓
Gara điện nước
↓
Trạm bơm khu vực
```

### Đặc điểm

- Nguồn Tool và Shelter Material chính.
- Có Hazard điện.
- Có Current Strength cao gần trạm bơm.
- Cho phép tiếp cận Infrastructure Objective.

### Vai trò

Đây là Route phục vụ chiến lược Flood Control và Technology.

---

## 8. Unlockable Shortcut

### Tên

```text
Elevated Service Link
```

### Kết nối

```text
Gara điện nước
↔
Trường học
```

### Điều kiện mở

- Khảo sát cả hai đầu tuyến.
- Có Rope.
- Có Wood hoặc Metal Support.
- Hoàn thành Outdoor Construction Task.
- Current Strength tại điểm đặt chưa đạt Critical.

### Chi phí baseline

```text
2 Wood
1 Rope
1 Metal
90 phút Active Work
```

### Lợi ích

- Kết nối Utility Route với Civic Ridge.
- Tránh quay lại Main Shelter.
- Mở tuyến sơ tán thay thế.
- Cho phép vận chuyển Resource từ Gara tới Trường học.
- Giữ giá trị sau khi Low Route bị khóa.

### Giới hạn

- Không vận chuyển được Large Object quá nặng.
- Có thể bị khóa trong Peak do Wind.
- Cần kiểm tra Condition sau Event lớn.

---

## 9. Location Travel Time

Thời gian dưới đây được tính từ Main Shelter trong trạng thái Route bình thường.

| Location          | Route ngắn nhất | Travel Time một chiều |
| ----------------- | --------------- | --------------------: |
| Khu nhà dân       | Residential     |               15 phút |
| Cửa hàng tiện lợi | Commercial      |               20 phút |
| Gara điện nước    | Utility         |               30 phút |
| Hiệu thuốc        | Commercial      |               40 phút |
| Trường học        | Residential     |               45 phút |
| Trạm bơm          | Utility         |               60 phút |
| Trạm thời tiết    | Residential     |               85 phút |

Travel Time tăng theo:

- Flood State.
- Carry Load.
- Injury.
- Fatigue.
- Obstacle.
- Route Event.

---

## 10. Route Segment Baseline

| Segment                          | Base Time |
| -------------------------------- | --------: |
| Main Shelter → Khu nhà dân       |   15 phút |
| Khu nhà dân → Trường học         |   30 phút |
| Trường học → Trạm thời tiết      |   40 phút |
| Main Shelter → Cửa hàng tiện lợi |   20 phút |
| Cửa hàng tiện lợi → Hiệu thuốc   |   20 phút |
| Hiệu thuốc → Trạm bơm            |   35 phút |
| Main Shelter → Gara điện nước    |   30 phút |
| Gara → Trạm bơm                  |   30 phút |
| Gara → Trường học qua Shortcut   |   25 phút |

---

## 11. Travel Modifier

```text
actual_travel_time
=
base_travel_time
× flood_modifier
× carry_modifier
× condition_modifier
+
obstacle_time
```

### Flood Modifier

| Flood State |     Modifier |
| ----------- | -----------: |
| Dry         |          1.0 |
| Shallow     |          1.2 |
| Medium      |          1.5 |
| Deep        |          2.0 |
| Impassable  | Không thể đi |

Các giá trị là baseline prototype.

---

## 12. Location Summary

## Khu nhà dân

```text
Elevation: E2
Distance: Near
Primary Resource: Food, Water, Clothing
Primary Function: Tutorial và NPC
```

Lối vào thay đổi:

- Cửa chính trong giai đoạn đầu.
- Cửa sổ tầng hai hoặc mái khi nước dâng.

---

## Cửa hàng tiện lợi

```text
Elevation: E1
Distance: Near
Primary Resource: Food, Clean Water
Primary Function: Early Resource Race
```

Location mất giá trị nhanh nếu người chơi tới muộn.

---

## Hiệu thuốc

```text
Elevation: E1
Distance: Medium
Primary Resource: Medicine
Primary Function: Treatment Preparation
```

Kho tầng trên vẫn tồn tại sau khi khu bán hàng bị ngập.

---

## Gara điện nước

```text
Elevation: E2
Distance: Medium
Primary Resource: Tool, Metal, Pump Part
Primary Function: Shelter Technology
```

Có Large Object và Blueprint cần quay lại.

---

## Trường học

```text
Elevation: E3
Distance: Medium
Primary Resource: NPC, Food Reserve
Primary Function: Temporary Shelter
```

Là điểm sơ tán chính nếu Main Shelter thất bại.

---

## Trạm bơm khu vực

```text
Elevation: E0
Distance: Far
Primary Resource: Pump Part, Fuel
Primary Function: Infrastructure Choice
```

Đây là Location nguy hiểm nhất về Flood và Electrical Hazard.

---

## Trạm thời tiết

```text
Elevation: E4
Distance: Far
Primary Resource: Forecast, Electronics
Primary Function: Information và Narrative
```

Ít nguy cơ ngập nhưng có Wind và Interference cao.

---

## 13. Route State theo Disaster Phase

| Segment                     | Warning | First Rain | Black Rain | Escalation        | Peak              |
| --------------------------- | ------- | ---------- | ---------- | ----------------- | ----------------- |
| Main → Khu nhà dân          | Dry     | Shallow    | Shallow    | Medium            | Deep              |
| Khu nhà dân → Trường học    | Dry     | Dry        | Shallow    | Medium            | Deep có điều kiện |
| Trường học → Trạm thời tiết | Dry     | Dry        | Dry        | Wind Risk         | Khóa do Wind      |
| Main → Cửa hàng             | Dry     | Shallow    | Medium     | Deep              | Impassable        |
| Cửa hàng → Hiệu thuốc       | Dry     | Shallow    | Deep       | Impassable        | Impassable        |
| Hiệu thuốc → Trạm bơm       | Dry     | Medium     | Deep       | Impassable        | Impassable        |
| Main → Gara                 | Dry     | Shallow    | Medium     | Medium            | Deep              |
| Gara → Trạm bơm             | Dry     | Medium     | Deep       | Strong Current    | Impassable        |
| Gara → Trường học           | Chưa mở | Chưa mở    | Có thể xây | Mở nếu hoàn thành | Có điều kiện      |

Bảng thể hiện baseline trước khi áp dụng Player Action Modifier.

---

## 14. Infrastructure Modifier

### Trạm bơm được khôi phục

Tác động:

- `Gara → Trạm bơm` giảm một mức Flood State.
- `Hiệu thuốc → Trạm bơm` bị khóa chậm hơn.
- Water Pressure tại Utility Fringe giảm.
- Main Shelter Water Intrusion tăng chậm hơn trong Escalation.

### Trạm bơm bị tháo linh kiện

Tác động:

- Utility Fringe ngập nhanh hơn.
- Route tới Trạm bơm bị khóa sớm.
- Người chơi nhận Pump Part cho Main Shelter.

---

## 15. Route Hazard

Mỗi Route Segment có thể chứa:

```text
flood_state
current_strength
contamination_state
electrical_hazard
structural_risk
wind_risk
visibility
```

Route không chỉ có trạng thái mở hoặc đóng.

Một Route còn mở vẫn có thể:

- Tốn nhiều thời gian.
- Yêu cầu Equipment.
- Không phù hợp để mang vật nặng.
- Không phù hợp với NPC bị thương.

---

## 16. Return Route Rule

Khi lập Expedition, World Map phải đánh giá cả chiều đi và chiều về.

Thông tin hiển thị:

```text
Estimated Arrival Time
Estimated Return Time
Expected Phase Change
Known Route Risk
Intel Age
```

Nếu Disaster Phase dự kiến thay đổi trước khi trở về, hệ thống phải cảnh báo:

```text
Tuyến có thể không còn sử dụng được khi quay lại.
```

Không đảm bảo cảnh báo chính xác nếu Intel đã lỗi thời.

---

## 17. Route Closure

Route Closure có ba loại:

### Temporary Closure

Có thể mở lại sau:

- Event.
- Nước rút.
- Cắt nguồn điện.
- Dọn vật cản.

### Phase Closure

Bị khóa khi Disaster đạt Phase xác định.

### Permanent Chapter Closure

Không thể sử dụng lại trong Chapter.

Ví dụ:

- Cầu sập.
- Công trình đổ.
- Dòng nước đạt Critical.

---

## 18. Alternative Access

Location quan trọng phải có ít nhất hai phương thức tiếp cận khi hợp lý.

Ví dụ:

### Hiệu thuốc

- Cửa chính qua Low Route.
- Lối tầng trên qua tòa nhà kế bên.

### Trường học

- Cầu thang chính.
- Lối mái.
- Elevated Service Link.

### Gara

- Cửa trước.
- Cửa bảo trì.
- Shortcut từ Trường học.

Alternative Access có thể yêu cầu:

- Tool.
- Rope.
- Information.
- Construction Task.
- Disaster Phase phù hợp.

---

## 19. Shelter Site

## Main Shelter

```text
Elevation: E2
Type: Main Shelter
Water Risk: Medium
Access: High
Build Capacity: High
```

## Trường học

```text
Elevation: E3
Type: Temporary Shelter
Water Risk: Low
Access: Medium
Build Capacity: Low
```

## Trạm thời tiết

Không phải Shelter chính trong MVP.

Có thể dùng như Emergency Shelter ngắn hạn nếu Route bị khóa, nhưng:

- Living Capacity thấp.
- Không có Water Processing.
- Không phù hợp Forced Evacuation toàn nhóm.

---

## 20. Map Knowledge

Ban đầu người chơi biết:

- Main Shelter.
- Khu nhà dân.
- Cửa hàng tiện lợi.
- Trường học.
- Các Route dân cư cơ bản.

Người chơi chưa biết đầy đủ:

- Trạng thái Trạm bơm.
- Lối vào phụ của Hiệu thuốc.
- Shortcut.
- Điều kiện Trạm thời tiết.
- Hazard điện.
- Shelter Quality của Trường học.

---

## 21. Map Reveal

Thông tin được mở qua:

- Exploration.
- NPC.
- Radio.
- Document.
- Communication Station.
- Observation Point.
- Event.

World Map chỉ hiển thị trạng thái mới nhất mà người chơi biết.

Mỗi thông tin Route cần:

```text
last_observed_time
confidence
known_flood_state
known_hazard
known_accessibility
```

---

## 22. Observation Point

MVP có hai Observation Point:

### Main Shelter Roof

Cho phép quan sát:

- Basin Core.
- Commercial Lowlands.
- Mức nước tổng quát.

### School Roof

Cho phép quan sát:

- Civic Ridge.
- Utility Fringe.
- Một số Route Closure.
- Tín hiệu cứu hộ.

Observation tiêu tốn thời gian và có thể bị Wind Hazard ảnh hưởng.

---

## 23. Event Anchor trên bản đồ

| Khu vực        | Event chính                       |
| -------------- | --------------------------------- |
| Main Shelter   | Drain Backflow, Pump Jam          |
| Khu nhà dân    | Neighbor Event                    |
| Cửa hàng       | Resource Rush, Looted State       |
| Hiệu thuốc     | Medical Rescue, Electrified Water |
| Gara           | Technician Event, Structural Risk |
| Trường học     | Rescue Signal, Temporary Shelter  |
| Trạm bơm       | Infrastructure Choice             |
| Trạm thời tiết | Forecast Data, Peak Signal        |

---

## 24. Expedition Planning

World Map phải hỗ trợ người chơi chọn:

- Destination.
- Route.
- Expected Travel Time.
- Equipment Recommendation.
- Known Hazard.
- Event Deadline.
- Carry Capacity Reserve.
- Safe Return Window.

Game không tự động chọn Route tối ưu.

---

## 25. Multi-Location Expedition

Người chơi có thể ghé nhiều Location trong một chuyến.

Ví dụ:

```text
Main Shelter
↓
Cửa hàng tiện lợi
↓
Hiệu thuốc
↓
Main Shelter
```

Chi phí tăng:

- Carry Load.
- Fatigue.
- Route Risk.
- Khả năng Disaster Phase thay đổi.
- Khả năng Event hết hạn.

Mục tiêu cân bằng:

```text
1–2 Location / Expedition
```

Không khuyến khích quét toàn bộ District trong một chuyến.

---

## 26. Multiplayer Map Rule

Trong Multiplayer:

- World Map dùng chung World State.
- Personal Observation không tự động chia sẻ khi mất liên lạc.
- Người chơi có thể ở các District khác nhau.
- Route Closure áp dụng cho toàn bộ nhóm.
- Shortcut là công trình chung.
- Expedition Marker có thể được chia sẻ.

---

## 27. Dữ liệu hệ thống

## World Map

```text
map_id
districts
locations
route_segments
shelter_sites
observation_points
shortcut_state
```

## Location Node

```text
location_id
district_id
elevation
location_state
known_state
entrances
resource_profile
hazard_profile
event_anchors
shelter_site_data
```

## Route Segment

```text
route_id
from_location
to_location
base_travel_time
elevation_profile
flood_state
current_strength
contamination_state
hazard_flags
access_requirements
closure_state
last_intel_time
```

---

## 28. Phạm vi MVP

Triển khai:

- Bốn District.
- Bảy Main Location.
- Một Main Shelter.
- Một Temporary Shelter.
- Ba Main Route.
- Một Shortcut.
- Elevation Tier.
- Route Flood State.
- Route Closure.
- Alternative Access.
- Observation Point.
- Map Knowledge.
- Expedition Planning.

Chưa triển khai:

- Thành phố Open World lớn.
- Procedural Map.
- Fast Travel.
- Vehicle Driving.
- Route Generation tự động.
- Nhiều Shelter hoạt động đồng thời.
- Mô phỏng giao thông.
- Fluid Simulation liên tục toàn bản đồ.

---

## 29. Quyết định chốt

- World Map là một mạng Location và Route Segment.
- Main Shelter nằm tại Elevation `E2`.
- Commercial Lowlands ngập đầu tiên.
- Civic Ridge là tuyến sơ tán chính.
- Route A ưu tiên tài nguyên sống còn.
- Route B ưu tiên NPC và Shelter thay thế.
- Route C ưu tiên Tool và Infrastructure.
- Shortcut kết nối Gara với Trường học.
- World Map chỉ hiển thị kiến thức người chơi đã thu thập.
- Travel Time và Return Window phải được đánh giá trước Expedition.
- Peak khóa phần lớn Route thấp nhưng vẫn giữ một phương án sơ tán có điều kiện.

07-black-rain-event-list.md 08-black-rain-balance-framework.md 09-mvp-technical-specification.md 10-mvp-prototype-plan.md
