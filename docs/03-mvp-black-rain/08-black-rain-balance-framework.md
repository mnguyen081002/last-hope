# 08-black-rain-balance-framework.md

## 1. Mục tiêu

Tài liệu này xác định cách cân bằng MVP Siêu Bão Mưa Đen.

Balance Framework phải kiểm soát:

- Áp lực thời gian.
- Nguồn cung và tiêu thụ tài nguyên.
- Tốc độ Hazard tiến triển.
- Giá trị của Location.
- Hiệu quả Shelter Module.
- Khả năng cứu NPC.
- Độ khó Peak Phase.
- Tỷ lệ các Outcome.
- Khả năng tránh softlock.

Các giá trị trong tài liệu là baseline prototype, không phải số liệu cuối cùng.

---

## 2. Mục tiêu trải nghiệm

Một lượt chơi cân bằng phải tạo ra các cảm giác sau:

### Warning

```text
Tôi có nhiều việc cần làm hơn số thời gian hiện có.
```

### Black Rain

```text
Kế hoạch ban đầu không còn hoàn toàn phù hợp.
```

### Escalation

```text
Tôi phải từ bỏ một số mục tiêu.
```

### Peak

```text
Kết quả hiện tại phản ánh những gì tôi đã chuẩn bị.
```

### Aftermath

```text
Tôi hiểu vì sao mình sống sót hoặc thất bại.
```

---

## 3. Difficulty Target

MVP cân bằng trước cho độ khó chuẩn.

Mục tiêu với người chơi đã hiểu hệ thống:

| Outcome              | Tỷ lệ mục tiêu |
| -------------------- | -------------: |
| Exceptional Survival |         10–15% |
| Stable Survival      |         35–45% |
| Barely Survived      |         25–35% |
| Forced Evacuation    |         10–20% |
| Collapse             |          5–15% |

Trong lần chơi đầu, tỷ lệ Barely Survived và Forced Evacuation có thể cao hơn.

---

## 4. Balance Layer

```text
Time Budget
Resource Budget
Travel Budget
Inventory Budget
Hazard Budget
Shelter Capacity
Event Budget
Recovery Budget
Outcome Threshold
```

Không cân bằng từng hệ thống riêng biệt mà bỏ qua tác động giữa chúng.

---

# 5. Time Budget

## 5.1. Tổng thời gian hoạt động

Disaster Timeline:

```text
Ngày 0, 17:00
đến
Ngày 4, 12:00
```

Tổng World Time:

```text
91 giờ
```

Trong đó người chơi cần ngủ và nghỉ.

Thời gian hoạt động thực tế dự kiến:

```text
50–60 giờ trong game
```

Tương đương:

```text
10–12 giờ thực nếu chơi toàn bộ thời gian
```

Do người chơi sử dụng Sleep Simulation, thời lượng thực mục tiêu vẫn là:

```text
5–8 giờ
```

---

## 5.2. Expedition Budget

Mục tiêu mỗi lượt:

```text
5–8 Expedition
```

Phân bố:

| Phase      | Expedition mục tiêu |
| ---------- | ------------------: |
| Normal     |                 0–1 |
| Warning    |                 2–3 |
| First Rain |                 1–2 |
| Black Rain |                 1–2 |
| Escalation |                   1 |
| Peak       |        0–1 khẩn cấp |

Một Expedition trung bình:

```text
45–100 phút trong game
```

không tính Search chuyên sâu tại Location lớn.

---

## 5.3. Shelter Work Budget

Tổng Active Work cần thiết để hoàn thành mọi Module phải vượt thời gian khả dụng.

Baseline:

```text
Tổng thời gian xây toàn bộ Module:
18–24 giờ trong game

Thời gian Shelter Work khả dụng thực tế:
8–14 giờ trong game
```

Người chơi chỉ có thể hoàn thiện khoảng:

```text
45–65%
```

nội dung nâng cấp.

---

## 5.4. Time Pressure Rule

Không tạo áp lực bằng cách:

- Ẩn deadline.
- Tăng Travel Time bất ngờ.
- Khóa Route không cảnh báo.
- Buộc người chơi lặp lại thao tác.

Áp lực phải đến từ:

- Nhiều mục tiêu hợp lệ.
- World State thay đổi.
- Resource Competition.
- Recovery.
- Carry Load.
- Event Deadline.

---

# 6. Resource Balance

## 6.1. Supply Ratio

```text
World Accessible Supply
=
140–160% Minimum Survival Need
```

Expected Retrieval:

```text
70–80% Accessible Supply
```

Expected Useful Resource sau mất mát:

```text
60–70% Accessible Supply
```

---

## 6.2. Survival Margin

Người chơi chuẩn bị hợp lý phải có biên sai số:

```text
15–25%
```

trên nhu cầu tối thiểu.

Biên sai số cho phép:

- Mất một chuyến đi.
- Một phần Storage bị ngập.
- Một NPC cần điều trị.
- Một Resource Batch bị Contaminated.

---

## 6.3. Resource Scarcity Tier

### Critical

- Clean Water.
- Flood Mitigation.
- Safe Zone.
- Emergency Power trong một số chiến lược.

### Strategic

- Fuel.
- Battery.
- Medicine.
- Pump Part.
- Electronic Component.

### Supporting

- Wood.
- Metal.
- Fabric.
- Dry Clothing.
- Tool.

Critical Resource luôn có nhiều phương án thay thế.

---

# 7. Water Balance

## 7.1. Consumption

Baseline:

```text
Player:
2 Water Unit / ngày

NPC:
1.5 Water Unit / ngày
```

Water Consumption tăng tối đa `25%` khi:

- Hoạt động nặng.
- Fatigue cao.
- Carry Load lớn.
- Điều trị.

---

## 7.2. Purification

```text
1 Untreated Water
→
1 Clean Water

Duration:
60 phút trong game

Power:
2

Filter:
1 charge / 2 Water
```

Water Purifier phải đủ hữu ích nhưng không tạo nước miễn phí.

---

## 7.3. Water Failure Band

### Too Easy

- Người chơi thường còn trên `8 Water Unit` sau Peak.
- Không cần Water Purifier.
- Không cần lựa chọn giữa NPC và Resource.

### Target

- Người chơi vào Peak với `4–10 Water Unit`, tùy số người.
- Có thể sống sót nếu một phần Water bị mất.
- Water Purifier là một lựa chọn mạnh nhưng không bắt buộc.

### Too Hard

- Người chơi thường hết Water trước Peak dù đã khám phá hợp lý.
- Một nguồn bị mất khiến Chapter không thể hoàn thành.
- Cứu một NPC luôn dẫn tới thiếu Water.

---

# 8. Food Balance

Food tạo áp lực thấp hơn Water.

Target:

- Người chơi không cần ăn quá thường xuyên.
- Không bị Health Damage vì Hunger trước cuối Chapter nếu đã thu thập cơ bản.
- Cứu nhiều NPC làm Food trở thành vấn đề rõ ràng.

Food phải nặng và cồng kềnh đủ để cạnh tranh Inventory, nhưng không nặng hơn Water trên mỗi ngày sử dụng.

---

# 9. Power Balance

## 9.1. Generator

```text
Output: 4 Power
Fuel: 1 Unit / 2 giờ
```

## 9.2. Battery Bank

```text
Capacity: 8 Power Charge
```

## 9.3. Peak

Peak kéo dài `12 giờ`.

Không nên có đủ Fuel để:

- Chạy Pump.
- Water Purifier.
- Drying Station.
- Communication Station.
- Lighting.

đồng thời trong toàn Peak.

Target:

```text
Người chơi duy trì được 1 Module tải cao
+
1–2 Module tải thấp
```

---

# 10. Module Balance

## 10.1. Flood Barrier

Mục tiêu:

- Giảm Entrance Inflow từ `40–60%`.
- Không ảnh hưởng Drain Backflow.
- Có Durability đủ vượt một Peak nếu được bảo trì.

Không được làm Shelter miễn nhiễm với Flood.

---

## 10.2. Portable Pump

Mục tiêu:

- Có thể giữ Ground Floor ở mức ổn định khi Inflow trung bình.
- Không đủ chống Critical Backflow một mình.
- Cần Power và Maintenance.
- Khi tắc, người chơi có thời gian phản ứng.

---

## 10.3. Elevated Storage

Mục tiêu:

- Chỉ bảo vệ `30–50%` tổng Resource Volume dự kiến.
- Buộc người chơi chọn Resource ưu tiên.
- Không chứa Large Object lớn.

---

## 10.4. Water Purifier

Mục tiêu:

- Tạo đủ Water để hỗ trợ Survival Reserve.
- Không xử lý Black Water cơ bản.
- Cạnh tranh Power với Pump.

---

## 10.5. Communication Station

Mục tiêu:

- Không trực tiếp tăng Survival Resource.
- Giảm rủi ro thông qua Forecast và Event Discovery.
- Có thể mở Outcome cao hơn.
- Không bắt buộc để sống sót.

---

## 10.6. Drying Station

Mục tiêu:

- Giảm thời gian phục hồi Wet và Cold.
- Bảo vệ Equipment.
- Không phải Module bắt buộc nếu người chơi có đủ Dry Clothing và nguồn nhiệt.

---

# 11. Hazard Balance

## 11.1. Hazard Exposure

Công thức nền:

```text
Exposure Gain
=
Intensity
× Exposure Rate
× Protection Modifier
× Action Modifier
```

Không dùng Exposure Rate giống nhau cho mọi Hazard.

---

## 11.2. Protection Modifier

Baseline:

| Protection     | Modifier |
| -------------- | -------: |
| Không bảo vệ   |      1.0 |
| Bảo vệ nhẹ     |     0.75 |
| Bảo vệ phù hợp |      0.5 |
| Bảo vệ mạnh    |      0.3 |

Protection không giảm xuống `0`.

---

## 11.3. Black Water Target

### Tiếp xúc ngắn

- Không gây Status Effect ngay.
- Có cảnh báo.

### Một chuyến đi thiếu bảo hộ

- Tạo Exposure đáng kể.
- Cần Cleaning hoặc Rest.

### Nhiều chuyến liên tục

- Có nguy cơ Sick.
- Giảm khả năng hồi phục.

### Tiếp xúc nghiêm trọng

- Có vết thương hở.
- Bơi.
- Ở trong Deep Water.

có thể tạo Injury hoặc Status Effect nhanh hơn.

---

## 11.4. Flood Traversal

| Flood State | Travel Modifier | Stamina Pressure |
| ----------- | --------------: | ---------------- |
| Dry         |             1.0 | Không            |
| Shallow     |             1.2 | Thấp             |
| Medium      |             1.5 | Trung bình       |
| Deep        |             2.0 | Cao              |
| Impassable  |    Không thể đi | —                |

Deep Water không nên được dùng cho phần lớn tuyến bắt buộc.

---

# 12. Carry Load Balance

|      Load | Tác động                    |
| --------: | --------------------------- |
|     0–50% | Không phạt                  |
|    51–75% | Stamina Cost tăng nhẹ       |
|   76–100% | Travel Time và Stamina tăng |
|  101–120% | Không chạy, hạn chế leo     |
| Trên 120% | Không thể nhặt thêm         |

Target:

- Người chơi thường rời Location với `70–100%` Load.
- Một chuyến đi tốt vẫn phải bỏ lại Resource.
- Large Object cần chuyến riêng hoặc NPC hỗ trợ.

---

# 13. Search Balance

Search Point:

| Loại     | Thời gian thực mục tiêu |
| -------- | ----------------------: |
| Quick    |                2–5 giây |
| Standard |               5–10 giây |
| Deep     |              10–18 giây |
| Special  |              15–30 giây |

Loot xuất hiện dần.

Target:

- Người chơi có thể dừng sớm.
- Search sâu tăng Reward nhưng tốn thời gian và Exposure.
- Không mọi Search Point đều cần Search tối đa.

---

# 14. NPC Balance

## 14.1. NPC Value

Mỗi NPC phải:

- Giảm chi phí hoặc thời gian trong lĩnh vực riêng.
- Mở ít nhất một lựa chọn.
- Không loại bỏ hoàn toàn một Resource Requirement.

---

## 14.2. NPC Cost

NPC Resource Consumption phải đủ để người chơi cân nhắc, nhưng không biến cứu NPC thành lựa chọn luôn sai.

Target:

- Cứu `1–2 NPC` là lựa chọn cân bằng.
- Cứu `3–4 NPC` yêu cầu chuẩn bị tốt.
- Không cứu NPC vẫn có thể hoàn thành Chapter nhưng mất lợi thế và Outcome.

---

## 14.3. NPC Work

NPC hiệu quả trung bình:

```text
70–100% Player Efficiency
```

NPC có Skill chuyên môn:

```text
110–140% Player Efficiency
```

trong Task phù hợp.

NPC vẫn cần nghỉ và Resource.

---

# 15. Event Balance

## 15.1. Deadline

Event quan trọng phải cho thời gian:

```text
Travel tới mục tiêu
+
Thời gian xử lý
+
10–30% Safety Margin
```

Event không được thiết kế để người chơi phải biết trước vị trí mà chưa có Intel.

---

## 15.2. Critical Event

Critical Event phải có ít nhất hai phương án:

- Giải quyết trực tiếp.
- Chấp nhận mất một hệ thống và chuyển chiến lược.

Ví dụ:

```text
Pump Jam
→
Repair Pump
OR
Abandon Lower Floor
```

---

## 15.3. Event Reward

Event Reward không luôn là Resource.

Có thể là:

- NPC.
- Intel.
- Route.
- Time advantage.
- Reduced Hazard.
- Outcome Flag.

---

# 16. Location Balance

Mỗi Location cần có:

```text
1 Primary Value
1 Secondary Value
1 Hazard Identity
1 Return Hook
```

Không Location nào cung cấp mọi thứ.

---

## 16.1. Visit Rate Target

| Location          | Tỷ lệ người chơi ghé mục tiêu |
| ----------------- | ----------------------------: |
| Khu nhà dân       |                       90–100% |
| Cửa hàng tiện lợi |                        65–85% |
| Hiệu thuốc        |                        65–85% |
| Gara điện nước    |                        70–90% |
| Trường học        |                        60–85% |
| Trạm bơm          |                        35–60% |
| Trạm thời tiết    |                        30–55% |

Nếu một Location luôn đạt trên `95%` ngoài tutorial, kiểm tra xem nó có quá bắt buộc hay không.

---

# 17. Route Balance

Mỗi Route phải có giá trị riêng:

### Route A

- Reward cao sớm.
- Mất sớm.

### Route B

- An toàn hơn.
- NPC và Evacuation.

### Route C

- Tool và Infrastructure.
- Hazard kỹ thuật cao.

Không Route nào luôn nhanh và an toàn hơn các Route khác.

---

# 18. Recovery Balance

## Stamina

- Hồi nhanh khi nghỉ ngắn.
- Không thay thế Sleep.

## Fatigue

- Giảm chủ yếu bằng Sleep.
- Một đêm ngủ đủ phải tạo khác biệt rõ.

## Injury

- Điều trị giảm hậu quả nhưng cần thời gian.
- Không hồi hoàn toàn ngay.

## Wet và Cold

- Có thể xử lý tại Shelter.
- Thiếu Dry Clothing hoặc nguồn nhiệt làm hồi phục chậm.

---

# 19. Failure Balance

## 19.1. Không tạo Failure tức thời

Trừ Hazard đã được cảnh báo rõ.

Các Failure lớn cần chuỗi:

```text
Warning
↓
Degradation
↓
Critical State
↓
Failure
```

---

## 19.2. Recoverable Failure

Các thất bại có thể tiếp tục:

- Mất Ground Floor.
- Mất Power.
- Mất Communication.
- Mất một NPC.
- Mất một Route.
- Mất Resource dự phòng.

---

## 19.3. Terminal Failure

Chỉ gồm:

- Toàn bộ Player chết.
- Không còn Safe Zone.
- Không thể sơ tán.
- Không còn khả năng duy trì nhu cầu sống còn tối thiểu.

---

# 20. Anti-Softlock Checklist

Mỗi build phải kiểm tra:

- Có thể lấy Water từ ít nhất hai nguồn.
- Có thể xử lý Peak không cần Communication Station.
- Có thể hoàn thành không cần Regional Pump.
- Có thể hoàn thành không cần cứu NPC.
- Có thể sơ tán không cần Generator.
- Không có Tool bắt buộc chỉ xuất hiện qua RNG.
- Không có Event bắt buộc bị khóa bởi một Event tùy chọn.
- Không có Route duy nhất bị khóa mà không có Alternative Access.

---

# 21. Balance Knob

Các thông số được phép điều chỉnh:

```text
resource_quantity
resource_weight
consumption_rate
travel_time
search_duration
build_duration
hazard_exposure_rate
module_efficiency
power_demand
fuel_consumption
event_deadline
route_closure_time
npc_consumption
recovery_rate
```

Không thay nhiều nhóm thông số cùng lúc khi test.

---

# 22. Tuning Order

Thứ tự cân bằng:

1. Core Loop có vui và rõ hay không.
2. Travel và Search Time.
3. Carry Load.
4. Resource Availability.
5. Water và Food Consumption.
6. Shelter Module Efficiency.
7. Hazard Exposure.
8. Event Deadline.
9. NPC Cost.
10. Outcome Threshold.

Không cân bằng Outcome trước khi Core Loop ổn định.

---

# 23. Test Scenario

## Scenario A — Resource First

- Ưu tiên Cửa hàng và Hiệu thuốc.
- Ít xây Flood Control.
- Kiểm tra khả năng mất Ground Floor nhưng vẫn sống.

## Scenario B — Shelter First

- Ưu tiên Gara và Pump.
- Ít Food và NPC.
- Kiểm tra Peak ổn định nhưng Resource thấp.

## Scenario C — Information First

- Ưu tiên Trường học và Trạm thời tiết.
- Chuẩn bị Evacuation.
- Kiểm tra Main Shelter yếu nhưng Outcome vẫn hợp lệ.

## Scenario D — NPC Rescue

- Cứu ba đến bốn NPC.
- Kiểm tra Resource Pressure và Living Capacity.

## Scenario E — Poor Preparation

- Thực hiện ít Expedition.
- Không xây Module trọng yếu.
- Kiểm tra Failure có cảnh báo và hợp lý.

---

# 24. Telemetry

```text
playtime
world_time_at_phase
expedition_count
location_visit_count
resource_collected
resource_consumed
resource_destroyed
water_at_peak
food_at_peak
fuel_at_peak
module_built
module_usage_time
route_selected
event_result
npc_rescued
npc_death
player_injury
player_death
shelter_failure
outcome_level
```

---

# 25. Playtest Questionnaire

Sau mỗi lượt test cần hỏi:

- Bạn hiểu tại sao Route bị khóa không?
- Bạn có cảm thấy có đủ thông tin để chuẩn bị không?
- Bạn có từng chờ đợi mà không có quyết định nào không?
- Resource nào luôn thiếu?
- Resource nào không có giá trị?
- Module nào bạn luôn xây?
- Module nào bạn không bao giờ xây?
- Bạn có hiểu nguyên nhân Outcome không?
- Bạn có cảm thấy một Failure nào không công bằng không?
- Bạn có muốn chơi lại theo chiến lược khác không?

---

# 26. Exit Criteria

Balance MVP đạt mức chấp nhận khi:

- Ít nhất ba chiến lược hoàn thành Chapter.
- Không Module nào đạt tỷ lệ xây trên `90%` nếu không phải tutorial.
- Không Location tùy chọn nào bắt buộc trong trên `90%` lượt thắng.
- Collapse không chủ yếu đến từ thiếu thông tin.
- Người chơi có thể giải thích nguyên nhân thất bại.
- Resource còn lại cuối Chapter không quá dư thừa.
- Peak tạo áp lực nhưng không chỉ là chuỗi repair bắt buộc.
- Forced Evacuation là kết quả khả thi, không phải Failure giả.

---

# 27. Quyết định chốt

- Cân bằng trước cho Single-player.
- Các con số hiện tại là baseline prototype.
- Nguồn cung có biên sai số nhưng không đủ lấy toàn bộ.
- Water, Time và Power là ba áp lực chính.
- Peak kiểm tra chuẩn bị, không tạo thử thách hoàn toàn mới.
- Cứu NPC phải vừa có lợi vừa tạo Resource Pressure.
- Failure lớn phải có cảnh báo và phương án giảm thiệt hại.
- Balance được điều chỉnh bằng telemetry và playtest, không chỉ bằng spreadsheet.
