# 02-black-rain-resource-economy.md

## 1. Mục tiêu

Tài liệu này xác định lượng tài nguyên, nhu cầu tiêu thụ, nguồn cung và các đánh đổi kinh tế trong MVP Siêu Bão Mưa Đen.

Resource Economy phải đảm bảo:

- Người chơi không thể lấy toàn bộ tài nguyên.
- Có đủ tài nguyên để sống sót khi đưa ra quyết định hợp lý.
- Không có một Location bắt buộc duy nhất.
- Tài nguyên quan trọng có nhiều nguồn và nhiều công dụng.
- Cứu NPC tạo lợi ích và chi phí thực tế.
- Peak Phase phản ánh chất lượng chuẩn bị.
- RNG không quyết định trực tiếp khả năng hoàn thành Chapter.

---

## 2. Baseline cân bằng

Các giá trị trong tài liệu là baseline cho prototype.

Baseline chính:

```text
player_count: 1
expected_recruited_npc: 1–2
maximum_major_npc: 4
chapter_duration: 4 ngày
expected_playtime: 5–8 giờ
```

Multiplayer và số NPC sẽ dùng hệ số mở rộng riêng.

---

## 3. Đơn vị tài nguyên

## Clean Water Unit

```text
1 Water Unit ≈ 0.75 lít nước sử dụng
```

Một Water Unit có thể dùng cho:

- Uống.
- Điều trị.
- Nấu ăn.
- Vệ sinh giới hạn.

---

## Food Unit

```text
1 Food Unit = 1 khẩu phần ăn
```

Food Unit không mô phỏng calorie chi tiết.

---

## Medicine Unit

```text
1 Medicine Unit = 1 lần điều trị cơ bản
```

Các điều trị chuyên biệt có thể yêu cầu item riêng.

---

## Fuel Unit

```text
1 Fuel Unit = 2 giờ vận hành Generator ở tải chuẩn
```

---

## Battery Charge Unit

```text
1 Battery Charge Unit = 1 giờ vận hành thiết bị tải thấp
```

Thiết bị tải trung bình hoặc cao tiêu thụ nhiều hơn.

---

## Material Unit

Material Unit là một đơn vị chế tạo trừu tượng.

Ví dụ:

```text
1 Wood Unit
1 Metal Unit
1 Waterproof Material Unit
1 Electronic Component Unit
```

---

## 4. Nhu cầu sinh tồn cơ bản

### Người chơi

Nhu cầu tối thiểu cho toàn Chapter:

| Resource                | Mức tối thiểu |
| ----------------------- | ------------: |
| Clean Water             |             8 |
| Food                    |             6 |
| Medicine                |             2 |
| Dry Clothing Set        |             1 |
| Light Source            |             1 |
| Flood Mitigation Option |             1 |

Mức này cho phép sống sót nhưng không đảm bảo Outcome tốt.

---

## 5. Nhu cầu theo thời gian

Baseline cho mỗi nhân vật:

```text
Water Consumption:
2 Water Unit / ngày

Food Consumption:
1.5 Food Unit / ngày
```

Consumption được cập nhật theo World Clock.

Hoạt động nặng có thể làm Water Consumption tăng.

---

## 6. NPC Consumption

NPC bắt đầu tiêu thụ tài nguyên Shelter khi được---

## 6. NPC Consumption

NPC bắt đầu tiêu thụ tài nguyên Shelter khi được tiếp nhận.

Baseline:

```text
Water:
1.5 Unit / NPC / ngày

Food:
1 Unit / NPC / ngày
```

NPC có thể mang theo:

```text
1 Water Unit
+
1 Food Unit
```

nếu Event định nghĩa họ vẫn còn vật tư cá nhân.

NPC bị thương hoặc bệnh có thể tiêu thụ thêm:

- Medicine.
- Clean Water.
- Living Capacity.
- Thời gian điều trị.

---

## 7. Occupant-Day

Resource Economy sử dụng `Occupant-Day` để tính nhu cầu động.

```text
occupant_day
=
số người trong Shelter
×
thời gian cư trú theo ngày
```

Ví dụ:

```text
1 NPC được cứu vào Ngày 2 lúc 12:00
và ở lại đến Ngày 4 lúc 06:00

≈ 1.75 Occupant-Day
```

Hệ thống không tính NPC như đã cư trú từ đầu Chapter.

---

## 8. Survival Reserve

Để đạt Stable Survival, người chơi nên có khi Peak bắt đầu:

```text
Water Reserve:
nhu cầu của nhóm trong 18 giờ

Food Reserve:
ít nhất 1 khẩu phần mỗi người

Medicine Reserve:
ít nhất 1 lần điều trị

Emergency Power:
ít nhất 4 giờ cho hệ thống trọng yếu
```

Exceptional Survival yêu cầu lượng dự phòng cao hơn.

---

## 9. Tổng nguồn cung

Tổng nguồn tài nguyên có thể tiếp cận trong thế giới phải bằng:

```text
140–160%
```

nhu cầu sống sót tối thiểu của một lượt chơi dự kiến.

Người chơi chỉ có khả năng thực tế thu hồi:

```text
70–80%
```

do:

- World Clock.
- Route Closure.
- Carry Load.
- Hazard.
- NPC Event.
- Location Depletion.
- Opportunity Cost.

---

## 10. Supply Baseline

Nguồn cung tiềm năng cho một lượt chơi Single-player:

| Resource             | Tổng tiềm năng |
| -------------------- | -------------: |
| Clean Water          |          14–18 |
| Untreated Water      |           8–12 |
| Food                 |          12–16 |
| Medicine             |            5–7 |
| Dry Clothing Set     |            3–4 |
| Fuel                 |            7–9 |
| Battery Charge       |           8–12 |
| Filter Charge        |            5–7 |
| Wood                 |          16–20 |
| Metal                |          12–16 |
| Waterproof Material  |          10–14 |
| Electronic Component |            6–9 |
| Pump Part            |            3–5 |
| Rope                 |            2–4 |

Không phải toàn bộ lượng này xuất hiện trong cùng trạng thái sạch hoặc dễ tiếp cận.

---

## 11. Resource Availability

Mỗi nguồn tài nguyên có thể ở trạng thái:

```text
Available
Restricted
At Risk
Contaminated
Destroyed
Taken
```

Trạng thái thay đổi theo:

- Disaster Phase.
- Event.
- Location State.
- Player Action.
- NPC Action.

---

## 12. Nguồn tài nguyên theo Location

| Location          | Resource chính                | Resource phụ                |
| ----------------- | ----------------------------- | --------------------------- |
| Khu nhà dân       | Food, Water, Clothing         | Battery, Tool               |
| Cửa hàng tiện lợi | Food, Clean Water             | Battery, Container          |
| Hiệu thuốc        | Medicine, Bandage             | Purification Item           |
| Gara điện nước    | Tool, Metal, Pump Part        | Battery, Fuel               |
| Trường học        | Food, Water, Fabric           | Rope, Medical Supply        |
| Trạm bơm          | Pump Part, Fuel               | Metal, Infrastructure Intel |
| Trạm thời tiết    | Electronic Component, Battery | Forecast, Signal Data       |

Mỗi Resource bắt buộc phải có ít nhất hai nguồn hợp lý.

---

## 13. Clean Water Economy

Clean Water có bốn nguồn chính:

- Nước đóng chai.
- Water Storage ban đầu.
- Untreated Water được xử lý.
- NPC hoặc Event.

### Nước ban đầu tại Shelter

```text
3 Water Unit
```

Đủ cho giai đoạn đầu nhưng không đủ vượt Peak.

### Water Purifier

Baseline:

```text
Input:
1 Untreated Water

Output:
1 Clean Water

Duration:
60 phút trong game

Power Demand:
2

Filter Cost:
1 Filter Charge / 2 Water Unit
```

Water Purifier cơ bản không xử lý Black Water.

---

## 14. Untreated Water

Untreated Water có thể lấy từ:

- Nguồn nước máy trước khi hệ thống hỏng.
- Bể chứa.
- Rain Collection trước khi Black Rain bắt đầu.
- Một số container kín.

Sau khi Phase Black Rain bắt đầu:

- Rain Collection không còn tạo Untreated Water an toàn.
- Nước ngoài trời mới thu được mặc định là Black Water.
- Container đã đóng kín vẫn giữ trạng thái trước đó.

---

## 15. Food Economy

Food được chia thành:

```text
Ready Food
Cookable Food
Perishable Food
Contaminated Food
```

MVP không cần hệ thống nấu ăn phức tạp.

Food có thể yêu cầu:

- Không xử lý.
- Nguồn nhiệt.
- Clean Water.
- Thời gian chuẩn bị.

Perishable Food mất giá trị nhanh hơn khi:

- Power mất.
- Storage bị ngập.
- Shelter Cleanliness thấp.

---

## 16. Medicine Economy

Medicine gồm:

```text
Bandage
Basic Medicine
Antiseptic
Special Treatment Item
```

### Phân bổ dự kiến

- Hiệu thuốc là nguồn lớn nhất.
- Khu nhà dân và trường học cung cấp nguồn phụ.
- NPC y tế có thể tăng hiệu quả sử dụng.

Medicine không được dùng để hồi Health tức thời một cách toàn diện.

Medicine xử lý:

- Bleeding.
- Infection Risk.
- Sick.
- Injury Recovery.

---

## 17. Power Economy

MVP dùng Power Demand trừu tượng.

### Nguồn điện

#### City Grid

- Có sẵn trong Normal.
- Không ổn định trong First Rain.
- Thất bại trong Escalation.

#### Generator

```text
Power Output: 4
Fuel Consumption: 1 Fuel Unit / 2 giờ
```

#### Battery Bank

```text
Maximum Capacity: 8 Power Charge
```

Battery Bank cần được sạc trước hoặc bằng Generator dư tải.

---

## 18. Module Power Demand

| Module                | Power Demand |
| --------------------- | -----------: |
| Communication Station |            1 |
| Emergency Lighting    |            1 |
| Water Purifier        |            2 |
| Drying Station        |            2 |
| Portable Water Pump   |            3 |
| Battery Charging      |            2 |
| Signal Stabilizer     |            2 |

Generator không thể vận hành toàn bộ Module cùng lúc.

---

## 19. Peak Power Requirement

Peak kéo dài `12 giờ`.

Để chạy Generator liên tục trong toàn Peak:

```text
6 Fuel Unit
```

Người chơi không bắt buộc phải chạy liên tục.

Chiến lược hợp lệ:

- Chạy Pump theo chu kỳ.
- Dùng Battery cho Communication.
- Tắt Drying Station.
- Chỉ lọc nước trước Peak.
- Chấp nhận mất Lower Floor để tiết kiệm Fuel.
- Sơ tán thay vì bảo vệ Main Shelter.

---

## 20. Material Economy

Material chính:

```text
Wood
Metal
Waterproof Material
Electronic Component
Pump Part
Fabric
Rope
Container
Filter
```

Mỗi Material quan trọng phải có nhiều công dụng.

---

## 21. Shelter Module Cost

Baseline prototype:

| Module                | Chi phí                                           |
| --------------------- | ------------------------------------------------- |
| Flood Barrier         | 3 Wood, 3 Waterproof Material, 1 Metal            |
| Elevated Storage      | 4 Wood, 2 Metal                                   |
| Water Purifier        | 2 Filter, 1 Container, 1 Electronic Component     |
| Drying Station        | 2 Wood, 1 Metal, 1 Electronic Component           |
| Communication Station | 2 Electronic Component, 1 Battery, 1 Antenna Part |
| Battery Bank          | 3 Battery, 2 Electronic Component                 |
| Portable Pump Setup   | 1 Pump, 1 Pump Part, 1 Hose                       |

Chi phí có thể giảm nếu người chơi tìm được Module hoàn chỉnh thay vì chế tạo.

---

## 22. Infrastructure Choice

Tại Trạm bơm, người chơi phải chọn:

### Khôi phục trạm

Chi phí dự kiến:

```text
2 Pump Part
2 Electronic Component
2 giờ Active Work
```

Kết quả:

- Giảm Flood State tại Utility District.
- Giữ một Route mở lâu hơn.
- Giảm tốc độ Water Intrusion khu vực.

### Tháo linh kiện

Kết quả:

```text
2–3 Pump Part
1 Electronic Component
1 Metal
```

Hậu quả:

- Trạm bơm không thể khôi phục.
- Khu vực thấp ngập nhanh hơn.
- Main Shelter có thêm lựa chọn sửa Pump.

---

## 23. Resource Competition

### Fuel

```text
Generator
OR
Portable Pump
OR
Water Processing
```

### Battery

```text
Flashlight
OR
Radio
OR
Communication Station
OR
Battery Bank
```

### Metal

```text
Flood Barrier
OR
Elevated Storage
OR
Pump Repair
OR
Battery Bank
```

### Clean Water

```text
Drinking
OR
Treatment
OR
Cooking
OR
Cleaning
```

### Time

```text
Loot thêm Resource
OR
Xây Module
OR
Xử lý nước
OR
Nghỉ ngơi
OR
Cứu NPC
```

---

## 24. Loot Depletion

Ordinary Loot không respawn.

Mỗi Search Point có:

```text
initial_resource_pool
remaining_resource_pool
contamination_state
destruction_state
```

Khi đã lấy hết Ordinary Loot:

- Search Point chuyển sang Depleted.
- Resource không xuất hiện lại.
- Event mới chỉ có thể tạo tài nguyên nếu có nguyên nhân hợp lý.

---

## 25. Event Resource

Event có thể tạo Resource mới qua:

- Xe cứu hộ gặp nạn.
- Vật tư bị trôi dạt.
- NPC mang theo đồ.
- Công trình sập làm lộ kho.
- Nhóm sống sót bỏ lại vật tư.

Event Resource phải:

- Có nguồn gốc.
- Có thời hạn.
- Có rủi ro.
- Không thay thế nguồn cung chính bắt buộc.

---

## 26. Contamination Loss

Resource có thể mất giá trị do:

- Black Water.
- Storage ngập.
- Container hỏng.
- Đặt sai Zone.
- Không xử lý trước khi đưa vào Clean Storage.

Baseline tổn thất dự kiến trong một lượt chơi trung bình:

```text
10–20% tài nguyên đã thu thập
```

Người chơi chuẩn bị tốt có thể giảm mức này.

---

## 27. Transport Economy

Giá trị thực tế của Resource phụ thuộc vào khả năng mang về.

Người chơi phải cân nhắc:

```text
Resource Value
/
Weight
/
Volume
/
Travel Time
/
Hazard Risk
```

Large Object như:

- Pump.
- Generator.
- Fuel Container lớn.
- NPC bị thương.

cần:

- Carried Object.
- Hai người.
- Nhiều chuyến.
- Route phù hợp.

---

## 28. Anti-Softlock Rule

Resource Economy phải đảm bảo:

- Có ít nhất hai phương án Flood Mitigation.
- Có ít nhất hai nguồn Clean Water hoặc Untreated Water.
- Có ít nhất hai nguồn Medicine.
- Có ít nhất một phương án sơ tán không cần Generator.
- Không yêu cầu Blueprint ngẫu nhiên để hoàn thành Chapter.
- Resource bắt buộc không bị phá hủy hoàn toàn bởi RNG.

Nếu một nguồn bị mất, phương án thay thế phải đắt hơn hoặc nguy hiểm hơn, không hoàn toàn miễn phí.

---

## 29. Outcome Resource Threshold

### Exceptional Survival

- Nước còn ít nhất `4 Water Unit` sau Peak.
- Food còn ít nhất `2 Food Unit` cho nhóm.
- Medicine còn dự phòng.
- Ít hơn `20%` Storage bị mất.
- Ít nhất một Infrastructure Objective hoàn thành.

### Stable Survival

- Nhu cầu sống còn được đáp ứng.
- Shelter còn hoạt động.
- Không còn dự phòng lớn.

### Barely Survived

- Water hoặc Food gần cạn.
- Storage tổn thất lớn.
- Module trọng yếu hỏng.
- Có Injury chưa hồi phục.

### Forced Evacuation

- Chỉ mang được Survival Resource ưu tiên.
- Phần lớn Shelter Material và Module bị bỏ lại.

---

## 30. Multiplayer Scaling

Nguồn tài nguyên không tăng tuyến tính hoàn toàn theo số người.

Baseline:

```text
Total Survival Demand
=
Base Demand
×
Player Count
```

Nguồn cung thế giới:

```text
World Supply Multiplier
=
1
+
0.7 × (additional_players)
```

Ví dụ:

| Player | Demand | Supply Multiplier |
| -----: | -----: | ----------------: |
|      1 |   100% |              100% |
|      2 |   200% |              170% |
|      3 |   300% |              240% |
|      4 |   400% |              310% |

Co-op bù phần thiếu bằng:

- Khả năng chia vai trò.
- Mang được nhiều đồ hơn.
- Xây nhanh hơn.
- Truy cập Event khó hơn.
- Cứu nhau khi Incapacitated.

Các giá trị này chỉ dùng khi Multiplayer được triển khai.

---

## 31. Telemetry cần theo dõi

Prototype cần ghi nhận:

```text
resource_collected
resource_consumed
resource_destroyed
resource_left_behind
location_depletion
shelter_module_built
fuel_usage
water_shortage_time
inventory_overload_time
npc_resource_cost
```

Các chỉ số quan trọng:

- Người chơi chết vì thiếu tài nguyên nào.
- Resource nào luôn bị bỏ qua.
- Location nào luôn được ưu tiên.
- Module nào luôn được xây.
- Người chơi còn bao nhiêu tài nguyên khi Peak bắt đầu.
- Bao nhiêu Resource bị mất do Contamination.

---

## 32. Phạm vi MVP

Triển khai:

- Water, Food, Medicine.
- Fuel và Battery.
- Material chính.
- Resource Consumption theo World Clock.
- NPC Consumption.
- Water Purification.
- Power Allocation.
- Module Cost.
- Persistent Depletion.
- Contamination Loss.
- Event Resource.
- Supply Distribution theo Location.

Chưa triển khai:

- Thị trường.
- Tiền tệ.
- Trading Economy lớn.
- Farming.
- Sản xuất tài nguyên vô hạn.
- Dinh dưỡng chi tiết.
- Chất lượng Material nhiều cấp.

---

## 33. Quyết định chốt

- Resource Economy được cân bằng cho một người chơi và một đến hai NPC.
- Nguồn cung tiềm năng bằng khoảng `140–160%` nhu cầu tối thiểu.
- Người chơi chỉ có khả năng thu hồi khoảng `70–80%`.
- Clean Water là tài nguyên sống còn chính.
- Fuel là tài nguyên chiến lược của Peak Phase.
- Metal, Battery và Electronic Component phải cạnh tranh nhiều công dụng.
- Ordinary Loot không respawn.
- Cứu NPC làm tăng nhu cầu tài nguyên.
- Resource bắt buộc phải có nguồn thay thế.
- Các con số sẽ được xác minh bằng prototype và telemetry.
