# LAST HOPE — MVP DESIGN SPECIFICATION

## 1. Tổng quan

### Tên MVP

**Siêu Bão Mưa Đen**

### Tên tiếng Anh

**Black Rain**

### Vai trò

MVP vừa là:

- Vertical slice kiểm chứng gameplay cốt lõi.
- Chapter 1 của campaign.
- Nền tảng kỹ thuật cho các Disaster Chapter sau.

### Thể loại

Survival, exploration, resource management, shelter preparation.

### Chế độ mục tiêu

- Single-player là phạm vi triển khai chính.
- Dữ liệu và World Clock phải tương thích với Multiplayer trong tương lai.
- MVP chưa triển khai networking hoặc co-op hoàn chỉnh.

---

## 2. Player Fantasy

Người chơi là một cư dân bình thường bị mắc kẹt trong thành phố khi một siêu bão bất thường xuất hiện.

Ban đầu, đây chỉ là một cảnh báo thời tiết nghiêm trọng. Sau đó:

- Mưa chuyển thành màu đen.
- Nước dâng từ cống và tầng thấp.
- Thiết bị điện hoạt động không ổn định.
- Radio thu được tín hiệu không xác định.
- Các tuyến đường quen thuộc bị chia cắt.
- Shelter trở thành nơi duy nhất có thể duy trì sự sống.

Fantasy chính:

> Quan sát một thế giới bình thường dần biến thành thảm họa, chuẩn bị trong thời gian giới hạn và sống sót qua đỉnh lũ bằng những quyết định đã đưa ra trước đó.

---

## 3. Trải nghiệm cốt lõi

Trải nghiệm ưu tiên của MVP:

1. Khám phá trong áp lực thời gian.
2. Quản lý tài nguyên và sức chứa.
3. Chuẩn bị Shelter.
4. Thích ứng với bản đồ đang thay đổi.
5. Ứng phó với hậu quả trong Peak Phase.

Câu hỏi trung tâm:

> Tôi nên mạo hiểm thêm bao nhiêu trước khi nước dâng khiến tôi không thể quay về?

---

## 4. Phạm vi thời lượng

### Thời lượng một lượt chơi

Mục tiêu:

```text
5–8 giờ chơi
```

### Thời lượng thế giới

MVP diễn ra trong khoảng:

```text
4 ngày trong game
```

Bao gồm:

- Một đoạn thế giới bình thường ngắn.
- Một ngày cảnh báo và chuẩn bị.
- Một ngày mưa và leo thang.
- Một đêm Peak Phase.
- Một giai đoạn Aftermath ngắn.

### Tỷ lệ thời gian

Sử dụng tỷ lệ mặc định của Time System:

```text
1 phút thực
=
5 phút trong game
```

Không tăng tốc hoặc nhảy thời gian ngoài giấc ngủ.

---

## 5. Cấu trúc Disaster Timeline

## Phase 0 — Normal

### Thời gian

```text
Ngày 0
17:00–22:00
```

### Mục tiêu

- Giới thiệu nhân vật.
- Giới thiệu Shelter.
- Giới thiệu khu vực dân cư.
- Dạy di chuyển, tương tác và Inventory.
- Thiết lập các NPC chính.
- Gieo dấu hiệu bất thường.

### Trạng thái thế giới

- Đường phố còn hoạt động.
- Điện và nước vẫn có.
- Phần lớn Location chưa bị Hazard ảnh hưởng.
- Tin tức chỉ cảnh báo một cơn bão mạnh.

### Nội dung bắt buộc

- Trở về Shelter.
- Kiểm tra nguồn điện.
- Cất một số vật phẩm.
- Gặp NPC đầu tiên.
- Nhận cảnh báo thời tiết.

---

## Phase 1 — Warning

### Thời gian

```text
Ngày 1
06:00–18:00
```

### Mục tiêu

- Thu thập tài nguyên.
- Khảo sát Shelter.
- Chọn nâng cấp đầu tiên.
- Thu thập Forecast.
- Xác định các tuyến đường ưu tiên.

### Trạng thái thế giới

- Một số cửa hàng đông người hoặc đã bắt đầu bị loot.
- Mưa chưa nghiêm trọng.
- Tuyến đường vẫn mở.
- Nước sạch và vật liệu chống ngập có giá trị tăng nhanh.

### Quyết định chính

- Ưu tiên nước sạch hay vật liệu.
- Gia cố Shelter hay thực hiện chuyến đi xa.
- Giúp NPC hay giữ tài nguyên.
- Thu thập thông tin hay tập trung vào thiết bị.

---

## Phase 2 — First Rain

### Thời gian

```text
Ngày 1
18:00
đến
Ngày 2
10:00
```

### Mục tiêu

- Trải nghiệm Hazard đầu tiên.
- Bắt đầu quản lý Wet và Body Temperature.
- Xử lý sự cố Shelter nhẹ.
- Điều chỉnh kế hoạch dựa trên tuyến đường mới.

### Trạng thái thế giới

- Đường thấp bắt đầu ngập.
- Điện chập chờn.
- Một số Location thay đổi lối vào.
- Các NPC bắt đầu di chuyển hoặc cầu cứu.

### Hazard

- Flood Depth cấp 1–2.
- Current Strength cấp 0–1.
- Wet.
- Cold Risk.
- Electromagnetic Interference mức thấp.

---

## Phase 3 — Black Rain

### Thời gian

```text
Ngày 2
10:00–22:00
```

### Mục tiêu

- Giới thiệu Black Water Exposure.
- Buộc người chơi thay đổi Equipment.
- Làm Information System trở nên không hoàn toàn đáng tin.
- Bắt đầu áp lực di chuyển tài nguyên lên cao.

### Trạng thái thế giới

- Mưa chuyển màu đen.
- Nước mới ngập trở thành Black Water.
- Một số thiết bị mất ổn định.
- Radio nhận tín hiệu không xác định.
- Các Location tầng thấp bắt đầu mất khả năng tiếp cận.

### Hazard

- Flood Depth cấp 2–3.
- Current Strength cấp 1–2.
- Black Water Exposure.
- Electrified Water cục bộ.
- Electromagnetic Interference mức trung bình.

---

## Phase 4 — Escalation

### Thời gian

```text
Ngày 2
22:00
đến
Ngày 3
18:00
```

### Mục tiêu

- Đưa ra quyết định cuối về Shelter.
- Hoàn thành chuyến đi cuối cùng.
- Chọn tài nguyên cần bảo vệ.
- Chuẩn bị cho Peak Phase.

### Trạng thái thế giới

- Một số tuyến đường bị khóa vĩnh viễn.
- Các Location xa trở nên khó tiếp cận.
- Shelter bắt đầu chịu Water Intrusion liên tục.
- Opportunity Event cuối xuất hiện.
- NPC chưa được cứu có nguy cơ mất tích.

### Hazard

- Flood Depth cấp 3–4.
- Current Strength cấp 2–4.
- Black Water Exposure cao.
- Electromagnetic Interference mức cao.
- Structural Collapse Risk tại các công trình yếu.

---

## Phase 5 — Peak

### Thời gian

```text
Ngày 3
18:00
đến
Ngày 4
06:00
```

### Mục tiêu

- Vận hành Shelter.
- Xử lý sự cố.
- Phân bổ Power.
- Bảo vệ Safe Zone.
- Duy trì nước sạch và khả năng cư trú.

### Trạng thái thế giới

- Phần lớn tuyến đường không còn an toàn.
- Hoạt động ngoài Shelter chỉ dành cho tình huống khẩn cấp.
- Máy bơm, Power và Storage trở thành hệ thống trọng yếu.
- Event liên tục kiểm tra các lựa chọn chuẩn bị.

### Hazard

- Flood Depth cấp 4–5.
- Current Strength cấp 3–5.
- Nhiễu điện từ nghiêm trọng.
- Structural Integrity giảm theo sự cố.
- Black Water có thể xâm nhập Shelter.

---

## Phase 6 — Aftermath

### Thời gian

```text
Ngày 4
06:00–12:00
```

### Mục tiêu

- Đánh giá thiệt hại.
- Xử lý hậu quả cuối.
- Xác định NPC sống sót.
- Thu thập Narrative Hook.
- Tính Chapter Outcome.

### Trạng thái thế giới

- Mưa giảm.
- Nước bắt đầu rút tại một số khu vực.
- Nhiều Location chuyển sang Persistent State mới.
- Shelter có thể còn sử dụng được, bị hư hỏng hoặc bị mất.
- Tín hiệu bất thường để lại manh mối cho Chapter sau.

---

## 6. Core Gameplay Loop của MVP

```text
Kiểm tra Shelter và Forecast
↓
Xác định nhu cầu ưu tiên
↓
Chuẩn bị Equipment và Inventory
↓
Chọn Location và Route
↓
Khám phá, Search và xử lý Hazard
↓
Quyết định tiếp tục hoặc quay về
↓
Mang Resource về Shelter
↓
Phân loại, xử lý và lưu trữ
↓
Build, Repair hoặc vận hành Module
↓
Ngủ hoặc tiếp tục hoạt động
↓
World State thay đổi
```

---

## 7. Mục tiêu sống sót

Để vượt qua MVP, nhóm phải đạt điều kiện tối thiểu khi Peak Phase kết thúc:

```text
player_alive == true
AND
safe_zone_available == true
AND
shelter_habitable == true
AND
minimum_clean_water_available == true
```

Có thể hoàn thành bằng:

- Giữ Main Shelter hoạt động.
- Chuyển sang Shelter thay thế.
- Forced Evacuation thành công.

---

## 8. Mục tiêu bắt buộc

Người chơi phải xử lý bốn nhóm mục tiêu.

### Shelter Survival

- Duy trì ít nhất một Safe Zone.
- Ngăn Water Intrusion đạt mức Critical.
- Duy trì một nguồn Power hoặc phương án thay thế.
- Bảo vệ một phần tài nguyên sống còn.

### Resource Survival

- Có đủ nước sạch cho Peak Phase.
- Có đủ thức ăn tối thiểu.
- Có vật tư điều trị cơ bản.
- Có khả năng vận hành hoặc thay thế máy bơm.

### Personal Survival

- Không để Health về `0`.
- Kiểm soát Fatigue.
- Tránh Black Water Exposure nghiêm trọng.
- Giữ Equipment thiết yếu hoạt động.

### Disaster Response

- Chuẩn bị trước khi Peak bắt đầu.
- Xử lý Shelter Event trọng yếu.
- Quyết định sơ tán nếu Main Shelter không còn khả năng duy trì.

---

## 9. Mục tiêu tùy chọn

Người chơi không thể hoàn thành toàn bộ trong một lượt.

Các mục tiêu tùy chọn:

- Cứu NPC tại trường học.
- Khôi phục trạm bơm khu vực.
- Thu thập dữ liệu tại trạm thời tiết.
- Xác minh tín hiệu radio bất thường.
- Bảo vệ toàn bộ Clean Storage.
- Thiết lập Temporary Shelter.
- Giữ Main Shelter không bị mất Lower Floor.
- Thu thập vật chất lạ từ nước đen.

Các mục tiêu tùy chọn ảnh hưởng Outcome và Persistent World State.

---

## 10. Main Shelter

### Loại Shelter

Nhà dân hai tầng nằm trong khu vực thấp đến trung bình.

### Ưu điểm

- Gần nhiều Location đầu game.
- Có tầng trên làm Safe Zone.
- Có Drain Core.
- Có Electrical Backbone.
- Có không gian cho Water Processing.
- Có mái phù hợp đặt Communication Module.

### Nhược điểm

- Tầng dưới có nguy cơ ngập.
- Drain Core có thể chảy ngược.
- Storage ban đầu nằm tại tầng thấp.
- Nguồn điện phụ thuộc lưới trong giai đoạn đầu.
- Khả năng di chuyển vật lớn lên tầng trên hạn chế.

---

## 11. Shelter Layout

### Fixed Core Components

```text
Main Staircase
Structural Pillars
Drain Core
Electrical Backbone
Water Intake Point
Roof Antenna Mount
```

Các Core Component không thể:

- Di chuyển.
- Tháo dỡ.
- Thay thế.

### Zone

```text
Entrance
Ground Floor Storage
Living Area
Utility Area
Workshop Area
Water Processing Area
Upper Safe Area
Roof
```

### Buildable Capacity

Shelter có số Build Slot giới hạn.

| Zone                  | Build Slot |
| --------------------- | ---------: |
| Entrance              |          2 |
| Ground Floor Storage  |          2 |
| Utility Area          |          2 |
| Workshop Area         |          1 |
| Water Processing Area |          2 |
| Upper Safe Area       |          3 |
| Roof                  |          1 |

Người chơi không đủ tài nguyên và thời gian để sử dụng toàn bộ Slot trong một lượt.

---

## 12. Shelter Module của MVP

### Flood Barrier

Vai trò:

- Giảm nước vào từ một điểm cụ thể.

Giới hạn:

- Chỉ bảo vệ một Entrance hoặc Opening.
- Có Durability.
- Không ngăn nước từ Drain Core.

---

### Portable Water Pump

Vai trò:

- Giảm Water Intrusion.

Yêu cầu:

- Power.
- Drain Output.
- Bảo trì.
- Không bị debris làm tắc.

---

### Elevated Storage

Vai trò:

- Bảo vệ vật phẩm khỏi nước.

Giới hạn:

- Sức chứa thấp.
- Chiếm Slot ở Upper Safe Area.
- Không chứa được mọi Large Object.

---

### Water Purifier

Vai trò:

- Xử lý Untreated Water.

Giới hạn:

- Không xử lý Black Water trực tiếp.
- Cần Power hoặc nhiên liệu.
- Cần Filter hoặc Consumable Part.

---

### Drying Station

Vai trò:

- Xử lý Wet Equipment.
- Giảm Cold Risk.
- Bảo vệ Tool và vật phẩm điện.

---

### Communication Station

Vai trò:

- Nhận Forecast.
- Phát hiện Event.
- Theo dõi tín hiệu bất thường.

Giới hạn:

- Phụ thuộc Power.
- Bị ảnh hưởng bởi Electromagnetic Interference.

---

### Battery Bank

Vai trò:

- Cấp điện dự phòng cho một nhóm Module.

Giới hạn:

- Dung lượng thấp.
- Cần sạc trước Peak.
- Không thể cấp điện toàn Shelter.

---

## 13. Shelter Build Choice

MVP phải đảm bảo người chơi không thể hoàn thiện toàn bộ Module.

Ba hướng chuẩn bị chính:

### Flood Control

```text
Flood Barrier
+
Portable Water Pump
+
Drain Maintenance
```

Ưu điểm:

- Giảm nguy cơ mất tầng dưới.

Điểm yếu:

- Tiêu hao Power và vật liệu lớn.

---

### Resource Preservation

```text
Elevated Storage
+
Water Purifier
+
Drying Station
```

Ưu điểm:

- Giữ tài nguyên và khả năng hồi phục.

Điểm yếu:

- Water Intrusion vẫn có thể tăng nhanh.

---

### Information and Evacuation

```text
Communication Station
+
Battery Bank
+
Temporary Shelter Preparation
```

Ưu điểm:

- Nhận Forecast tốt hơn.
- Có phương án sơ tán.

Điểm yếu:

- Main Shelter yếu hơn trong Peak.

Người chơi có thể kết hợp một phần các hướng, nhưng không được tối đa hóa cả ba.

---

## 14. Shelter thay thế

### Địa điểm

Tầng trên của trường học.

### Loại

Temporary Shelter có thể nâng thành Emergency Evacuation Site.

### Ưu điểm

- Độ cao lớn.
- Có nhiều Living Capacity.
- Gần điểm cứu NPC.
- Ít nguy cơ ngập trực tiếp.

### Nhược điểm

- Xa Main Shelter.
- Không có Water Processing hoàn chỉnh.
- Storage thấp.
- Khó vận chuyển Large Object.
- Power không ổn định.

### Điều kiện kích hoạt

- Người chơi phải khảo sát trường học.
- Mở lối lên tầng cao.
- Thiết lập Basic Storage.
- Xác định tuyến di chuyển an toàn.

---

## 15. Location List

MVP có bảy Location chính.

| Location          | Vai trò chính                 | Khoảng cách |
| ----------------- | ----------------------------- | ----------- |
| Khu nhà dân       | Tutorial, tài nguyên cơ bản   | Gần         |
| Cửa hàng tiện lợi | Thức ăn và nước nhanh         | Gần         |
| Hiệu thuốc        | Thuốc và vật tư điều trị      | Trung bình  |
| Gara điện nước    | Tool, vật liệu, Pump Part     | Trung bình  |
| Trường học        | NPC, Temporary Shelter        | Trung bình  |
| Trạm bơm khu vực  | Flood Control, Infrastructure | Xa          |
| Trạm thời tiết    | Forecast, tín hiệu bất thường | Xa          |

Mỗi lượt chơi dự kiến ghé:

```text
4–6 Location chính
```

Không yêu cầu khám phá toàn bộ.

---

## 16. Location Detail Summary

## Khu nhà dân

### Vai trò

- Tutorial Exploration.
- Nguồn thức ăn, nước và quần áo.
- Giới thiệu NPC.

### Hazard Progression

- Phase đầu an toàn.
- Tầng thấp ngập từ Black Rain.
- Một số nhà mất khả năng tiếp cận trong Escalation.

### Return Hook

- NPC quay lại.
- Mái nhà trở thành tuyến di chuyển.
- Vật phẩm bị bỏ lại cần thu hồi.

---

## Cửa hàng tiện lợi

### Vai trò

- Nguồn Consumable đầu game.
- Tạo áp lực ưu tiên khám phá sớm.

### Hazard Progression

- Bị người khác loot trong Warning.
- Tầng bán hàng bị ngập trong Black Rain.
- Kho trên cao chỉ tiếp cận được từ cửa sau.

### Return Hook

- Delivery Storage mở sau khi cửa cuốn hỏng.
- Opportunity Event vật tư trôi dạt.

---

## Hiệu thuốc

### Vai trò

- Medicine.
- Bandage.
- Water Purification Item.
- Thông tin về Black Water Exposure.

### Hazard Progression

- Khu bán hàng bị ngập.
- Kho thuốc ở tầng cao vẫn an toàn.
- Electrified Water có thể xuất hiện.

### Return Hook

- Pharmacy Storage cần chìa khóa hoặc Tool.
- NPC y tế có thể mở thêm lựa chọn.

---

## Gara điện nước

### Vai trò

- Tool.
- Pump Part.
- Battery.
- Material.
- Blueprint.

### Hazard Progression

- Nước tràn vào hố sửa xe.
- Tool điện có nguy cơ hỏng.
- Structural Collapse Risk tăng trong Escalation.

### Return Hook

- Generator hoặc Large Pump Component cần hai người mang.
- Workshop Blueprint mở sau khi cấp Power.

---

## Trường học

### Vai trò

- NPC Rescue.
- Temporary Shelter.
- Nguồn thức ăn dự trữ hạn chế.
- Điểm cao chiến lược.

### Hazard Progression

- Tầng trệt ngập.
- Cầu thang chính bị chặn.
- Mái nhà trở thành Rescue Point.

### Return Hook

- Thiết lập Shelter.
- Đón NPC.
- Sử dụng như điểm sơ tán trong Peak.

---

## Trạm bơm khu vực

### Vai trò

- Giảm tốc độ nước dâng tại một khu vực.
- Cung cấp Pump Part.
- Mở tuyến đường trong Escalation.

### Hazard Progression

- Nước sâu.
- Electrified Water.
- Thiết bị cần Power hoặc sửa chữa.

### Quyết định chính

```text
Khôi phục trạm bơm
OR
Tháo linh kiện mang về Shelter
```

Khôi phục trạm bơm giúp khu vực nhưng tốn thời gian và tài nguyên.

Tháo linh kiện giúp Main Shelter nhưng làm World Map ngập nhanh hơn.

---

## Trạm thời tiết

### Vai trò

- Forecast chính xác.
- Disaster Intel.
- Narrative Hook.
- Signal Data.

### Hazard Progression

- Tuyến tiếp cận bị khóa sớm.
- Nhiễu điện từ cao.
- Cần Power để truy xuất dữ liệu.

### Quyết định chính

```text
Lấy thiết bị về Shelter
OR
Duy trì trạm hoạt động
```

---

## 17. Route Structure

MVP có ba tuyến chính.

### Tuyến thấp

- Nhanh.
- Đi qua khu thương mại.
- Ngập sớm.
- Có nhiều Resource.

### Tuyến dân cư

- Dài hơn.
- Ít Hazard đầu game.
- Có nhiều NPC Event.
- Bị chia cắt trong Escalation.

### Tuyến cao

- Xa.
- Dẫn tới trường học và trạm thời tiết.
- Ít ngập.
- Khó vận chuyển vật nặng.

Ngoài ra có một đường tắt mở khóa qua:

- Mái nhà.
- Cầu tạm.
- Hoặc lối bảo trì tại trạm bơm.

---

## 18. World Map Transition

| Phase      | Thay đổi chính                                        |
| ---------- | ----------------------------------------------------- |
| Normal     | Toàn bộ tuyến chính mở                                |
| Warning    | Một số Location đông hoặc bị loot                     |
| First Rain | Tuyến thấp giảm tốc độ                                |
| Black Rain | Tầng thấp bị ô nhiễm                                  |
| Escalation | Hai Route Segment bị khóa                             |
| Peak       | Chỉ tuyến cao và Shelter Route còn khả dụng           |
| Aftermath  | Một số tuyến mở lại, Location chuyển Persistent State |

World Map chỉ cập nhật theo Intel người chơi đã thu thập.

---

## 19. Resource Categories

### Survival Resource

- Clean Water.
- Food.
- Medicine.
- Dry Clothing.

### Shelter Resource

- Wood.
- Metal.
- Waterproof Material.
- Fuel.
- Battery.
- Filter.
- Pump Part.

### Tool

- Hammer.
- Crowbar.
- Rope.
- Flashlight.
- Multitool.
- Water Test Kit.

### Information Resource

- Forecast.
- Route Intel.
- Shelter Site Intel.
- Signal Data.
- Blueprint.

---

## 20. Resource Economy Target

### Nhu cầu tối thiểu trước Peak

Cho một người chơi:

| Resource                          |              Mức tối thiểu |
| --------------------------------- | -------------------------: |
| Clean Water                       |                   8 đơn vị |
| Food                              |                   6 đơn vị |
| Medicine                          |                   2 đơn vị |
| Dry Clothing Set                  |                          1 |
| Emergency Light Source            |                          1 |
| Pump hoặc Flood Mitigation Option |                          1 |
| Safe Storage Capacity             | Đủ cho tài nguyên sống còn |

Đơn vị cụ thể được cân bằng trong prototype.

### Nguồn cung thế giới

Tổng nguồn cung có thể tiếp cận:

```text
140–160%
```

so với nhu cầu sống sót tối thiểu.

Người chơi chỉ có khả năng thực tế thu hồi:

```text
70–80%
```

do:

- Thời gian.
- Carry Load.
- Hazard.
- Route Closure.
- NPC.
- Event.

---

## 21. Resource Competition

Các tài nguyên quan trọng phải cạnh tranh nhiều công dụng.

### Fuel

Dùng cho:

- Generator.
- Portable Pump.
- Water Purifier.
- Drying Station.

### Battery

Dùng cho:

- Flashlight.
- Radio.
- Communication Station.
- Emergency Lighting.

### Metal Component

Dùng cho:

- Pump Repair.
- Flood Barrier.
- Battery Bank.
- Signal Stabilization.

### Clean Water

Dùng cho:

- Uống.
- Điều trị.
- Nấu ăn.
- Làm sạch giới hạn.

---

## 22. Player Condition trong MVP

Chỉ số sử dụng:

```text
Health
Stamina
Fatigue
Hunger
Thirst
Body Temperature
Carry Load
```

Status Effect chính:

```text
Wet
Cold
Bleeding
Sick
Black Water Exposure
Disoriented
```

MVP không sử dụng:

- Stress System.
- Mental Health.
- Dinh dưỡng chi tiết.
- Mô phỏng từng bộ phận cơ thể đầy đủ.

---

## 23. Equipment trọng tâm

### Áo mưa

- Giảm Wet.
- Tăng trọng lượng.
- Có Durability.

### Ủng chống nước

- Giảm Black Water Exposure trong nước nông.
- Không bảo vệ khi nước vượt quá chiều cao ủng.

### Găng tay

- Giảm Exposure khi xử lý vật phẩm ô nhiễm.

### Đèn pin

- Cải thiện khám phá.
- Tiêu hao Battery.
- Có thể bị nhiễu.

### Dây thừng

- Giảm rủi ro Current Strength.
- Hỗ trợ cứu NPC.
- Hỗ trợ Multiplayer.

### Ba lô chống nước

- Bảo vệ item.
- Sức chứa thấp hơn ba lô khung.

---

## 24. Hazard Implementation

MVP không mô phỏng chất lỏng vật lý toàn bản đồ.

Flood được triển khai bằng:

```text
Route Flood State
Location Zone Water Level
Shelter Water Intrusion
Dynamic Hazard Volume cục bộ
```

### Flood State

```text
Dry
Shallow
Medium
Deep
Impassable
```

### Current State

```text
None
Weak
Moderate
Strong
Critical
```

### Black Water State

```text
Clean Rainwater
Untreated Floodwater
Black Water
```

Hazard State thay đổi theo:

- Disaster Phase.
- World Clock.
- Trạm bơm.
- Event.
- Shelter Module.
- Route Elevation.

---

## 25. Electromagnetic Interference

Nhiễu điện từ có ba chức năng chính.

### Information Pressure

- Forecast bị trì hoãn.
- Radio mất đoạn.
- Intel giảm Confidence.

### Equipment Pressure

- Máy bơm điện có thể gián đoạn.
- Pin tiêu hao nhanh hơn.
- Communication Station mất ổn định.

### Narrative Function

- Xuất hiện tín hiệu không xác định.
- Gợi ý Mưa Đen không phải hiện tượng thời tiết bình thường.

Nhiễu không được tùy tiện phá thiết bị mà không có cảnh báo.

---

## 26. Main Event Set

MVP sử dụng các Event bắt buộc sau:

| Event                           | Phase      |
| ------------------------------- | ---------- |
| Cảnh báo siêu bão               | Normal     |
| Nguồn điện chập chờn            | Warning    |
| Cửa hàng bắt đầu bị loot        | Warning    |
| Mưa đầu tiên                    | First Rain |
| Mưa chuyển màu đen              | Black Rain |
| Tín hiệu cầu cứu tại trường học | Black Rain |
| Trạm bơm gặp sự cố              | Black Rain |
| Mất điện khu vực                | Escalation |
| Drain Core chảy ngược           | Escalation |
| Tuyến thấp bị khóa              | Escalation |
| Máy bơm Shelter bị tắc          | Peak       |
| Storage có nguy cơ ngập         | Peak       |
| Tín hiệu bất thường xuất hiện   | Peak       |
| Nước bắt đầu rút                | Aftermath  |

---

## 27. Optional Event Pool

Mỗi lượt chơi chọn một phần các Event sau:

- Xe cứu hộ gặp nạn.
- Vật tư trôi dạt.
- NPC yêu cầu nước sạch.
- Cầu tạm bị hỏng.
- Một khu vực rút nước ngắn hạn.
- Hiệu thuốc bị nhóm khác chiếm.
- Thiết bị tại trạm thời tiết phát tín hiệu.
- NPC tại Shelter bị bệnh.
- Generator quá nhiệt.
- Location bị sập một phần.

Event Pool phải tuân theo Event Budget.

---

## 28. NPC Set

MVP có bốn NPC quan trọng.

### Người hàng xóm

Vai trò:

- Giới thiệu quan hệ NPC.
- Có kỹ năng Construction cơ bản.
- Có thể hỗ trợ Shelter.

Chi phí:

- Tiêu thụ tài nguyên.
- Có người thân cần cứu.

---

### Nhân viên y tế

Vai trò:

- Mở Treatment Option.
- Cung cấp thông tin về Black Water.
- Tăng hiệu quả Medical Station.

Vị trí:

- Hiệu thuốc hoặc điểm cứu hộ.

---

### Kỹ thuật viên thoát nước

Vai trò:

- Hỗ trợ sửa máy bơm.
- Cung cấp Trạm bơm Intel.
- Mở Pump Blueprint.

Vị trí:

- Gara hoặc trạm bơm.

---

### Người vận hành radio

Vai trò:

- Cải thiện Communication Station.
- Xác minh tín hiệu bất thường.
- Mở Narrative Hook.

Vị trí:

- Trạm thời tiết hoặc trường học.

Mỗi lượt chơi có thể không cứu được toàn bộ bốn NPC.

---

## 29. Narrative Structure

### Setup

- Thành phố chuẩn bị đối phó một siêu bão.
- Người chơi tin rằng tình hình vẫn có thể kiểm soát.

### Escalation

- Mưa chuyển màu đen.
- Hạ tầng hoạt động sai quy luật.
- Tín hiệu không xác định xuất hiện.

### Revelation

- Dữ liệu từ trạm thời tiết cho thấy Mưa Đen không hình thành từ hệ thống bão thông thường.
- Nước chứa vật chất hoặc tín hiệu không xác định.

### Ending Hook

Một trong các manh mối được phát hiện:

- Mẫu vật có cấu trúc nhân tạo.
- Tín hiệu lặp lại theo chu kỳ.
- Dữ liệu cho thấy hiện tượng xuất hiện tại nhiều khu vực khác.
- Một vật thể được ghi nhận phía trên tầng mây.

MVP không giải thích nguồn gốc cuối cùng.

---

## 30. Outcome của MVP

### Exceptional Survival

- Main Shelter còn ổn định.
- Safe Zone không bị mất.
- Có nước và thức ăn dự phòng.
- Cứu ít nhất ba NPC.
- Thu thập Signal Data.
- Trạm bơm hoặc trạm thời tiết còn hoạt động.

### Stable Survival

- Nhân vật sống.
- Shelter còn khả năng cư trú.
- Có tài nguyên tối thiểu.
- Hoàn thành ít nhất một mục tiêu tùy chọn lớn.

### Barely Survived

- Nhân vật sống.
- Shelter hư hỏng nặng.
- Tài nguyên gần cạn.
- Mất Lower Floor hoặc NPC quan trọng.

### Forced Evacuation

- Main Shelter bị mất.
- Người chơi tới Shelter thay thế thành công.
- Phần lớn Storage bị bỏ lại.

### Collapse

- Nhân vật tử vong.
- Không còn Shelter hợp lệ.
- Không thể sơ tán.
- Không còn nước hoặc Safe Zone trong Peak.

---

## 31. Tutorial Flow

### Normal Phase

Dạy:

- Movement.
- Interaction.
- Inventory.
- Storage.
- Shelter Zone.

### Warning Phase

Dạy:

- World Map.
- Intel.
- Search.
- Carry Load.
- Build Placement.
- Active Task.

### First Rain

Dạy:

- Wet.
- Body Temperature.
- Flood Depth.
- Route State.

### Black Rain

Dạy:

- Exposure.
- Protection.
- Contamination.
- Information Confidence.

### Escalation

Dạy:

- Shelter Power Priority.
- Background Task.
- Event Deadline.
- NPC Task.

### Peak

Không giới thiệu hệ thống mới.

Peak chỉ kiểm tra những gì đã học.

---

## 32. Save System

MVP sử dụng:

- Autosave định kỳ.
- Autosave khi chuyển Disaster Phase.
- Autosave khi ngủ.
- Autosave khi vào hoặc rời Shelter.
- Một Manual Save Slot trong Single-player.

Không cho phép Save Scumming trở thành cách duy nhất xử lý Event.

Các Event có thể sử dụng Seed được lưu trong Save.

---

## 33. MVP Scope

### Bắt buộc triển khai

- Một World Clock.
- Day/Night Cycle.
- Sleep Simulation.
- Real-time Search.
- Real-time Build.
- Inventory và Equipment.
- Player Condition.
- Shelter Zone và Fixed Core Component.
- Sáu đến bảy Shelter Module.
- Flood Hazard.
- Black Water Exposure.
- Electromagnetic Interference.
- Bảy Location chính.
- Ba Route chính.
- Bốn NPC.
- Main Event Set.
- Optional Event Pool.
- Năm Outcome Level.
- Causal Outcome Report.

### Không thuộc MVP

- Combat chuyên sâu.
- Firearm.
- Vehicle Driving.
- Fluid Simulation toàn bản đồ.
- Dynamic Building Destruction.
- Xây Shelter từ đất trống.
- Full Multiplayer Networking.
- Faction System hoàn chỉnh.
- Skill Tree.
- Procedural Map.
- Random Loot Respawn.
- Campaign Chapter 2.
- Giải thích hoàn chỉnh nguồn gốc Mưa Đen.

---

## 34. Content Budget

| Nội dung          | Số lượng |
| ----------------- | -------: |
| Main Shelter      |        1 |
| Temporary Shelter |        1 |
| Location chính    |        7 |
| Route chính       |        3 |
| Đường tắt         |        1 |
| NPC quan trọng    |        4 |
| Shelter Module    |        7 |
| Main Event        |       14 |
| Optional Event    |     8–12 |
| Hazard chính      |        4 |
| Outcome Level     |        5 |

Không mở rộng số lượng trước khi Core Loop được prototype và kiểm chứng.

---

## 35. Prototype Milestone

## Prototype 1 — Exploration Loop

Kiểm chứng:

- Di chuyển.
- Search.
- Inventory.
- Carry Load.
- Quay về Shelter.

Sử dụng:

- Một Shelter.
- Một Location.
- Một Route.
- Không có Disaster Timeline đầy đủ.

---

## Prototype 2 — Flood Loop

Kiểm chứng:

- Flood Depth.
- Wet.
- Black Water Exposure.
- Route State.
- Equipment Protection.

Sử dụng:

- Hai Location.
- Một tuyến thay đổi theo thời gian.

---

## Prototype 3 — Shelter Loop

Kiểm chứng:

- Water Intrusion.
- Power Priority.
- Portable Pump.
- Elevated Storage.
- Active và Passive Task.

---

## Prototype 4 — Disaster Slice

Kiểm chứng:

- Warning.
- Black Rain.
- Escalation.
- Peak.
- Event Deadline.
- Outcome.

Thời lượng mục tiêu:

```text
60–90 phút
```

---

## Prototype 5 — Full MVP

Tích hợp:

- Toàn bộ Location.
- NPC.
- Event.
- Resource Economy.
- Shelter thay thế.
- Narrative Hook.
- Outcome Report.

---

## 36. Tiêu chí hoàn thành MVP

MVP được xem là hoàn thành khi:

1. Người chơi có thể hoàn thành toàn bộ Disaster Timeline.
2. Có ít nhất ba chiến lược Shelter khả thi.
3. Người chơi không thể thu thập toàn bộ tài nguyên trong một lượt.
4. World Map thay đổi buộc người chơi điều chỉnh kế hoạch.
5. Peak Phase phản ánh rõ các quyết định trước đó.
6. Không có thất bại lớn xảy ra mà không có cảnh báo.
7. Có ít nhất ba Outcome khác biệt có thể đạt được.
8. Các Location đã loot không tự hồi sinh tài nguyên.
9. Event có thể diễn ra khi người chơi không có mặt.
10. Một lượt chơi hoàn chỉnh kéo dài khoảng 5–8 giờ.
11. Game có thể hoàn thành mà không cần cứu mọi NPC.
12. MVP để lại Narrative Hook rõ ràng cho Chapter tiếp theo.

---

## 37. Thứ tự tài liệu triển khai tiếp theo

Từ tài liệu tổng thể này, cần viết lần lượt:

```text
01-black-rain-disaster-definition.md
02-black-rain-resource-economy.md
03-black-rain-world-map.md
04-main-shelter-design.md
05-black-rain-location-design.md
06-black-rain-npc-design.md
07-black-rain-event-list.md
08-black-rain-balance-framework.md
09-mvp-technical-specification.md
10-mvp-prototype-plan.md
```
