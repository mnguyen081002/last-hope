# Hazard Framework Design

## 1. Mục tiêu

Hazard Framework xác định cách các mối nguy trong môi trường tác động lên:

- Người chơi.
- NPC.
- Vật phẩm.
- Shelter.
- Location.
- Tuyến đường.
- Thiết bị.

Framework phải dùng được cho nhiều Disaster mà không cần thiết kế lại hệ thống nền tảng.

---

# 2. Nguyên tắc thiết kế

## 2.1. Hazard là trạng thái môi trường

Hazard không chỉ là vùng gây sát thương.

Mỗi Hazard phải có khả năng thay đổi:

- Cách người chơi di chuyển.
- Trang bị cần mang.
- Thời gian có thể ở lại.
- Tuyến đường có thể sử dụng.
- Giá trị của tài nguyên.
- Trạng thái của Location và Shelter.

---

## 2.2. Hazard phải có dấu hiệu nhận biết

Người chơi phải có khả năng phát hiện hoặc dự đoán Hazard qua:

- Hình ảnh.
- Âm thanh.
- Thời tiết.
- Thiết bị đo.
- Radio.
- NPC.
- Bản đồ.
- Dấu vết trong môi trường.

Hazard ẩn chỉ được sử dụng khi có phương tiện hợp lý để phát hiện.

---

## 2.3. Protection giảm rủi ro, không loại bỏ hoàn toàn rủi ro

Trang bị bảo vệ có thể:

- Giảm Exposure.
- Giảm sát thương.
- Trì hoãn Status Effect.
- Cho phép tiếp cận vùng nguy hiểm hơn.

Protection không nên khiến Hazard trở nên hoàn toàn vô nghĩa.

---

## 2.4. Hazard phải tạo lựa chọn

Một Hazard tốt phải tạo ít nhất một quyết định:

- Đi đường ngắn nguy hiểm hay đường dài an toàn.
- Tiếp tục khám phá hay quay về.
- Mang bảo hộ hay dành sức chứa cho tài nguyên.
- Dùng thiết bị bảo vệ ngay hay giữ lại.
- Cứu người hay tránh khu vực nguy hiểm.

---

## 2.5. Hậu quả phải tăng dần

Hazard không nên gây thất bại tức thời nếu người chơi chưa có cơ hội phản ứng.

Cấu trúc chung:

```text
Safe
↓
Warning
↓
Exposed
↓
Dangerous
↓
Critical
```

Ngoại lệ chỉ áp dụng cho nguy hiểm đã được cảnh báo rõ, như:

- Dòng nước cực mạnh.
- Sập công trình.
- Điện cao thế.
- Khu vực cấm tiếp cận.

---

# 3. Phân loại Hazard

## 3.1. Environmental Hazard

Mối nguy tồn tại trong môi trường.

Ví dụ:

- Nước ngập.
- Dòng chảy.
- Phóng xạ.
- Cháy.
- Khí độc.
- Nhiệt độ cực đoan.
- Sương ăn mòn.

---

## 3.2. Exposure Hazard

Mối nguy tích lũy theo thời gian tiếp xúc.

Ví dụ:

- Nước đen.
- Phóng xạ.
- Lạnh.
- Nhiệt.
- Bào tử.
- Khí độc.

---

## 3.3. Instant Hazard

Mối nguy gây hậu quả ngay khi xảy ra.

Ví dụ:

- Điện giật.
- Vật thể rơi.
- Sóng nước mạnh.
- Sập sàn.
- Cháy nổ.

Instant Hazard phải có tín hiệu cảnh báo hoặc nguyên nhân rõ ràng.

---

## 3.4. Infrastructure Hazard

Mối nguy phát sinh từ hạ tầng.

Ví dụ:

- Dây điện ngập nước.
- Cống chảy ngược.
- Cầu yếu.
- Máy phát quá tải.
- Tường chắn nước hỏng.
- Đường hầm bị ngập.

---

## 3.5. Information Hazard

Mối nguy ảnh hưởng khả năng đánh giá tình hình.

Ví dụ:

- Nhiễu radio.
- Bản đồ lỗi thời.
- Thiết bị đo sai.
- Tín hiệu giả.
- Dự báo không đầy đủ.

Information Hazard không trực tiếp gây sát thương nhưng làm tăng nguy cơ quyết định sai.

---

# 4. Thành phần của Hazard

Mỗi Hazard có cấu trúc:

```text
hazard_id
hazard_type
intensity
area
exposure_rate
affected_targets
protection_tags
environmental_effects
status_effects
warning_signs
update_rule
duration
source
```

---

# 5. Intensity

Intensity đại diện cho mức nguy hiểm hiện tại.

Baseline:

| Intensity | Trạng thái | Ý nghĩa                        |
| --------: | ---------- | ------------------------------ |
|         0 | Safe       | Không có ảnh hưởng             |
|         1 | Low        | Có thể hoạt động với ít rủi ro |
|         2 | Moderate   | Cần chuẩn bị                   |
|         3 | High       | Exposure tăng nhanh            |
|         4 | Severe     | Chỉ nên vào với bảo hộ phù hợp |
|         5 | Critical   | Không phù hợp để hoạt động lâu |

Intensity có thể thay đổi theo:

- World Clock.
- Disaster Phase.
- Weather.
- Địa hình.
- Event.
- Hành động của người chơi.
- Trạng thái hạ tầng.

---

# 6. Exposure

Exposure đại diện cho mức tiếp xúc tích lũy với một Hazard.

Công thức nền tảng:

```text
Exposure Gain
=
Hazard Intensity
×
Exposure Rate
×
Protection Modifier
×
Action Modifier
```

Trong đó:

- `Hazard Intensity`: mức nguy hiểm của khu vực.
- `Exposure Rate`: tốc độ tích lũy của loại Hazard.
- `Protection Modifier`: mức giảm từ trang bị.
- `Action Modifier`: ảnh hưởng từ hành động hiện tại.

Ví dụ:

- Đứng yên trong nước đen gây Exposure thấp.
- Bơi hoặc ngã trong nước làm Exposure tăng nhanh.
- Có vết thương hở làm tăng Exposure.
- Mặc đồ chống nước làm giảm Exposure.

---

# 7. Exposure Threshold

Exposure tạo hậu quả theo ngưỡng.

```text
0–24
Không ảnh hưởng đáng kể

25–49
Cảnh báo

50–74
Status Effect nhẹ

75–99
Status Effect nghiêm trọng

100
Critical Effect
```

Mỗi Hazard có thể định nghĩa ngưỡng riêng.

Exposure có thể:

- Giảm tự nhiên.
- Cần điều trị.
- Không thể loại bỏ hoàn toàn.
- Chuyển thành Status Effect kéo dài.

---

# 8. Protection

Protection đến từ:

- Equipment.
- Tool.
- Shelter.
- Vehicle.
- Consumable.
- Environmental Cover.

Mỗi nguồn Protection có:

```text
protected_hazard_tags
protection_modifier
durability_cost
coverage
operating_requirement
```

---

## 8.1. Equipment Protection

Ví dụ:

- Áo mưa.
- Ủng chống nước.
- Găng tay.
- Mũ bảo hộ.
- Đồ giữ nhiệt.

Equipment Protection có thể giảm theo:

- Durability.
- Condition.
- Thời gian tiếp xúc.
- Loại Hazard.
- Mức Intensity.

---

## 8.2. Shelter Protection

Protection của Shelter phụ thuộc vào:

- Vị trí.
- Structural Integrity.
- Zone.
- Module.
- Power.
- Water Intrusion.
- Maintenance.

Shelter không cung cấp Protection giống nhau cho mọi Zone.

---

## 8.3. Environmental Protection

Một số vị trí có thể giảm Hazard:

- Tầng cao.
- Mái che.
- Phòng kín.
- Khu vực khô.
- Tường chắn.
- Địa hình cao.

Environmental Protection có thể thay đổi khi Disaster tiến triển.

---

# 9. Hazard Area

Hazard có thể tồn tại ở nhiều cấp không gian.

## World Region

Ảnh hưởng một khu vực lớn trên bản đồ.

## Route Segment

Ảnh hưởng một đoạn đường.

## Location

Ảnh hưởng toàn bộ địa điểm.

## Zone

Ảnh hưởng một khu vực bên trong Location hoặc Shelter.

## Dynamic Volume

Vùng Hazard có thể di chuyển hoặc mở rộng theo thời gian.

Ví dụ:

- Dòng nước.
- Khói.
- Cháy.
- Nước đen tràn.
- Sương độc.

---

# 10. Hazard Update

Hazard được cập nhật theo:

- World Clock.
- Disaster Phase.
- Event.
- Weather.
- Hành động người chơi.
- Trạng thái module hoặc hạ tầng.

Không cần cập nhật toàn bộ Hazard mỗi frame.

Baseline:

```text
Local Hazard
→
Cập nhật liên tục khi người chơi ở gần

World Hazard
→
Cập nhật theo mốc thời gian

Shelter Hazard
→
Cập nhật theo module và World Clock
```

---

# 11. Hazard Interaction

Các Hazard có thể tương tác với nhau.

Ví dụ:

```text
Nước ngập
+
Dây điện hỏng
=
Electrified Water
```

```text
Wet
+
Nhiệt độ thấp
=
Cold Exposure tăng nhanh
```

```text
Black Water
+
Open Wound
=
Infection Risk tăng
```

```text
Flooding
+
Structural Damage
=
Collapse Risk
```

MVP chỉ nên sử dụng một số tổ hợp rõ ràng và dễ dự đoán.

---

# 12. Hazard tác động lên người chơi

Hazard có thể ảnh hưởng:

- Health.
- Stamina.
- Fatigue.
- Body Temperature.
- Movement Speed.
- Action Efficiency.
- Injury.
- Status Effect.
- Orientation.

Hazard không nên luôn gây mất Health trực tiếp.

Ưu tiên chuỗi hậu quả:

```text
Hazard
↓
Exposure hoặc trạng thái bất lợi
↓
Suy giảm khả năng hoạt động
↓
Health Damage nếu không xử lý
```

---

# 13. Hazard tác động lên vật phẩm

Hazard có thể thay đổi:

- Condition.
- Durability.
- Contamination State.
- Khả năng sử dụng.
- Giá trị tài nguyên.

Ví dụ:

- Nước làm hỏng thiết bị điện.
- Nước đen làm ô nhiễm vật phẩm.
- Ẩm làm hỏng thực phẩm.
- Va đập làm vỡ container.
- Nhiễu điện từ làm thiết bị hoạt động không ổn định.

---

# 14. Hazard tác động lên Shelter

Hazard có thể:

- Giảm Structural Integrity.
- Tăng Water Intrusion.
- Làm mất Power.
- Làm hỏng module.
- Làm Zone không thể sử dụng.
- Làm ô nhiễm Storage.
- Buộc người chơi di dời Shelter.

Vị trí Shelter ảnh hưởng trực tiếp đến Hazard Profile.

Ví dụ:

| Vị trí    | Lợi ích           | Rủi ro                    |
| --------- | ----------------- | ------------------------- |
| Tầng hầm  | Kín, dễ phòng thủ | Ngập nhanh                |
| Tầng trệt | Dễ tiếp cận       | Dễ bị nước xâm nhập       |
| Tầng cao  | Ít ngập           | Khó vận chuyển tài nguyên |
| Mái nhà   | Quan sát tốt      | Phơi nhiễm thời tiết      |
| Vùng cao  | An toàn trước lũ  | Xa tài nguyên             |

---

# 15. Hazard tác động lên World Map

Hazard có thể:

- Chặn tuyến đường.
- Tăng chi phí di chuyển.
- Mở tuyến mới.
- Chuyển đổi lối vào Location.
- Làm Location bị cô lập.
- Buộc người chơi sử dụng thiết bị đặc biệt.
- Thay đổi giá trị chiến lược của khu vực.

World Map phải hiển thị trạng thái Hazard dựa trên lượng thông tin người chơi đã thu thập.

---

# 16. Detection

Người chơi có thể phát hiện Hazard qua ba cấp.

## Direct Detection

Quan sát trực tiếp:

- Nước sâu.
- Dòng chảy.
- Cháy.
- Công trình nứt.
- Dây điện tóe lửa.

## Tool Detection

Dùng thiết bị:

- Radio.
- Máy đo mực nước.
- Thiết bị kiểm tra nước.
- Cảm biến điện.
- Thiết bị đo Hazard của chapter khác.

## Information Detection

Nhận thông tin từ:

- NPC.
- Bản đồ.
- Dự báo.
- Event.
- Communication Station.

---

# 17. Hazard Forecast

Một số Hazard có thể được dự báo.

Forecast có:

```text
forecast_area
forecast_time
expected_intensity
confidence
source
expiration_time
```

Forecast không nhất thiết chính xác hoàn toàn.

Độ tin cậy phụ thuộc vào:

- Nguồn thông tin.
- Thiết bị.
- Nhiễu.
- Disaster Phase.
- Thời gian kể từ khi nhận thông tin.

---

# 18. Hazard Recovery

Recovery phụ thuộc vào loại Hazard.

## Tự giảm theo thời gian

Ví dụ:

- Wet.
- Disoriented.
- Mệt do nhiệt.

## Cần điều kiện phù hợp

Ví dụ:

- Cold cần nguồn nhiệt.
- Black Water Exposure cần làm sạch.
- Khói cần rời khu vực.

## Cần điều trị

Ví dụ:

- Sick.
- Infection.
- Burns.
- Exposure nghiêm trọng.

Hazard Recovery phải sử dụng World Clock.

---

# 19. Hazard riêng của Siêu Bão Mưa Đen

## 19.1. Flood Depth

Flood Depth xác định độ sâu của nước.

| Mức | Trạng thái | Tác động                       |
| --: | ---------- | ------------------------------ |
|   0 | Dry        | Di chuyển bình thường          |
|   1 | Ankle Deep | Giảm nhẹ tốc độ                |
|   2 | Knee Deep  | Tốn Stamina                    |
|   3 | Waist Deep | Hạn chế di chuyển và mang đồ   |
|   4 | Chest Deep | Có nguy cơ bị cuốn             |
|   5 | Submerged  | Phải bơi hoặc không thể đi qua |

Flood Depth thay đổi theo:

- Độ cao địa hình.
- Lượng mưa.
- Hệ thống thoát nước.
- Disaster Phase.
- Máy bơm.
- Cửa chắn nước.

---

## 19.2. Current Strength

Current Strength đại diện cho lực dòng nước.

| Mức | Tác động                   |
| --: | -------------------------- |
|   0 | Không có dòng              |
|   1 | Di chuyển chậm             |
|   2 | Stamina tiêu hao cao       |
|   3 | Có nguy cơ mất thăng bằng  |
|   4 | Có nguy cơ bị cuốn         |
|   5 | Không thể vượt qua an toàn |

Rủi ro phụ thuộc vào:

- Carry Load.
- Stamina.
- Injury.
- Equipment.
- Số người hỗ trợ.
- Dây hoặc điểm bám.

---

## 19.3. Black Water Contamination

Black Water gây Exposure khi:

- Tiếp xúc trực tiếp.
- Bơi.
- Có vết thương hở.
- Uống nước.
- Mang vật phẩm bị nhiễm vào Shelter.

Protection:

- Ủng.
- Găng tay.
- Áo chống nước.
- Container chống nước.
- Khu xử lý vật phẩm tại Shelter.

---

## 19.4. Electromagnetic Interference

Electromagnetic Interference ảnh hưởng:

- Radio.
- Communication Station.
- Máy bơm điện.
- Thiết bị chiếu sáng.
- Pin.
- Thiết bị định vị.

Các mức:

```text
Stable
Distorted
Unreliable
Interrupted
Disabled
```

Nhiễu không gây hỏng thiết bị trong mọi trường hợp.

Nó có thể:

- Giảm hiệu suất.
- Làm gián đoạn.
- Làm sai thông tin.
- Tăng nguy cơ failure.
- Kích hoạt tín hiệu bất thường.

---

## 19.5. Electrified Water

Electrified Water xuất hiện khi:

- Nước tiếp xúc nguồn điện đang hoạt động.
- Dây điện hoặc thiết bị bị hỏng.
- Trạm điện bị ngập.

Đây là Instant Hazard.

Người chơi có thể xử lý bằng:

- Cắt nguồn điện.
- Chọn tuyến khác.
- Chờ nguồn điện mất.
- Dùng thiết bị cách điện phù hợp.

Hazard phải có dấu hiệu:

- Tia điện.
- Âm thanh.
- Thiết bị cảnh báo.
- Vật thể hoặc NPC bị ảnh hưởng.

---

## 19.6. Structural Collapse

Collapse Risk tăng khi:

- Nước sâu.
- Dòng chảy mạnh.
- Structural Integrity thấp.
- Vật nặng tập trung.
- Công trình đã hư hỏng.

Các mức:

```text
Stable
Damaged
Unstable
Imminent Collapse
Collapsed
```

Người chơi phải nhận được dấu hiệu trước khi sập:

- Âm thanh nứt.
- Rung.
- Bụi.
- Vật thể rơi.
- Bề mặt biến dạng.

---

# 20. Hazard Event

Hazard Event là thay đổi cục bộ hoặc đột ngột.

Ví dụ:

- Nước dâng nhanh.
- Cống trào ngược.
- Dây điện rơi xuống nước.
- Máy bơm bị tắc.
- Cầu bắt đầu sập.
- Nhiễu điện từ tăng mạnh.
- Dòng nước đổi hướng.

Hazard Event phải dựa trên:

- Disaster Phase.
- Vị trí.
- World Clock.
- Trạng thái hạ tầng.
- Hành động trước đó.
- Điều kiện môi trường.

---

# 21. Multiplayer Rule

Trong Multiplayer:

- Hazard State là dữ liệu chung.
- Mỗi người chơi có Exposure riêng.
- Equipment Protection được tính riêng.
- Người chơi có thể hỗ trợ nhau vượt Hazard.
- Một người có thể giữ dây hoặc vận hành thiết bị cho người khác.
- Hazard tiếp tục hoạt động khi một người mở Inventory hoặc thực hiện Task.
- Không có người chơi nào được làm dừng Hazard hoặc World Clock.

---

# 22. UI Requirement

UI phải hiển thị:

- Hazard hiện tại.
- Intensity.
- Exposure.
- Protection.
- Dấu hiệu sắp chuyển mức.
- Status Effect liên quan.
- Tuyến đường bị ảnh hưởng.
- Cảnh báo thiết bị hoặc Shelter.

Không hiển thị mọi Hazard dưới dạng thanh.

Đề xuất:

- Exposure tích lũy dùng thanh hoặc mức.
- Instant Hazard dùng cảnh báo trực tiếp.
- Hazard khu vực hiển thị trên bản đồ khi đã được phát hiện.
- Hazard chưa xác định hiển thị dưới dạng thông tin không đầy đủ.

---

# 23. Dữ liệu hệ thống

## Hazard Instance

```text
hazard_instance_id
hazard_id
hazard_type
source
area
intensity
exposure_rate
affected_targets
start_time
duration
update_rule
warning_state
```

## Exposure State

```text
target_id
hazard_id
current_exposure
exposure_level
protection_modifier
active_status_effects
last_update_time
```

## Protection Source

```text
source_id
protected_hazard_tags
protection_modifier
durability
coverage
active_state
```

---

# 24. Phạm vi MVP

Triển khai:

- Hazard Area.
- Hazard Intensity.
- Exposure.
- Protection Modifier.
- Exposure Threshold.
- Flood Depth.
- Current Strength.
- Black Water Contamination.
- Electromagnetic Interference.
- Electrified Water.
- Structural Collapse.
- Hazard tác động lên Player, Item, Shelter và Route.
- Hazard Forecast cơ bản.
- Hazard Event theo Disaster Phase.

Chưa triển khai:

- Mô phỏng chất lỏng vật lý toàn bản đồ.
- Lan truyền Hazard theo mô phỏng khoa học chi tiết.
- Hàng chục loại Hazard tương tác.
- Hệ thống dự báo phức tạp.
- Hazard chiến đấu chuyên sâu.
- Phá hủy công trình hoàn toàn động.

---

# 25. Quyết định chốt

- Hazard Framework là hệ thống chung cho mọi Disaster.
- Hazard có Intensity, Area, Exposure và Protection.
- Hazard phải thay đổi quyết định di chuyển, trang bị hoặc thời gian.
- Protection chỉ giảm rủi ro, không xóa hoàn toàn Hazard.
- Hậu quả tăng dần và phải có dấu hiệu cảnh báo.
- Hazard có thể tác động lên Player, Item, Shelter, Location và Route.
- Các Hazard có thể tương tác nhưng MVP chỉ dùng các tổ hợp rõ ràng.
- Flood Depth, Current Strength và Black Water Contamination là các Hazard chính của MVP.
- Electromagnetic Interference chủ yếu tác động lên thông tin và thiết bị.
- Vị trí Shelter quyết định Hazard Profile của Shelter.
- Multiplayer sử dụng Hazard State chung và Exposure riêng cho từng người chơi.
