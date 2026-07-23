# 01-black-rain-disaster-definition.md

## 1. Mục tiêu

Tài liệu này định nghĩa Siêu Bão Mưa Đen như một Disaster cụ thể được triển khai trên Core Framework của Last Hope.

Tài liệu xác định:

- Bản chất gameplay của Disaster.
- Disaster Timeline.
- Hazard chính.
- Quy tắc thay đổi thế giới.
- Tác động lên Player, Shelter, Location, Route và Information.
- Điều kiện kết thúc Disaster.
- Phạm vi triển khai trong MVP.

---

## 2. Disaster Identity

```text
disaster_id: black_rain
chapter_id: chapter_01
display_name: Siêu Bão Mưa Đen
english_name: Black Rain
duration: 4 ngày trong game
primary_environment: Thành phố ngập lụt
```

Siêu Bão Mưa Đen bắt đầu như một cơn bão lớn thông thường, sau đó xuất hiện các hiện tượng bất thường:

- Mưa chuyển sang màu đen.
- Nước dâng nhanh hơn dự báo.
- Nước trào ngược từ hệ thống cống.
- Thiết bị điện và radio mất ổn định.
- Nước mưa và nước ngập bị ô nhiễm.
- Tín hiệu không xác định xuất hiện trong nhiễu điện từ.

Nguồn gốc thật sự của hiện tượng không được giải thích hoàn toàn trong MVP.

---

## 3. Vai trò trong Campaign

Siêu Bão Mưa Đen là Chapter đầu tiên của campaign.

Chapter này phải:

- Bắt đầu từ thế giới bình thường.
- Giới thiệu Core Gameplay Loop.
- Thiết lập các hệ thống sinh tồn cơ bản.
- Cho thấy Disaster có thể thay đổi World Map.
- Giới thiệu Narrative Hook cho các Chapter sau.
- Không yêu cầu kiến thức từ Chapter trước.

---

## 4. Trải nghiệm mục tiêu

Người chơi phải cảm nhận được ba giai đoạn tâm lý:

```text
Chuẩn bị trước một cơn bão
↓
Nhận ra thảm họa không còn bình thường
↓
Sống sót bằng những quyết định đã đưa ra trước đó
```

Câu hỏi trung tâm:

> Tôi còn đủ thời gian để thực hiện thêm một chuyến đi trước khi tuyến đường trở nên không thể quay về hay không?

---

## 5. Nguyên tắc Disaster

### 5.1. Disaster tiến triển theo World Clock

Disaster Phase được quyết định bởi World Clock.

Hành động của người chơi có thể:

- Làm chậm hậu quả cục bộ.
- Bảo vệ một khu vực.
- Giữ một tuyến đường mở lâu hơn.
- Giảm thiệt hại tại Shelter.
- Cải thiện khả năng dự báo.

Người chơi không thể ngăn cơn bão xảy ra hoặc dừng Global Timeline.

---

### 5.2. Disaster thay đổi bản đồ

Khi Disaster tiến triển:

- Tuyến đường bị ngập.
- Lối vào Location thay đổi.
- Tầng thấp bị mất.
- Mái nhà và khu vực cao trở nên quan trọng.
- Location an toàn có thể trở thành Hazard Zone.
- Một số tài nguyên bị phá hủy hoặc ô nhiễm.

---

### 5.3. Preparation quyết định Peak Phase

Peak Phase không giới thiệu hệ thống mới.

Peak chỉ kiểm tra:

- Shelter đã được gia cố hay chưa.
- Nước sạch đã được chuẩn bị hay chưa.
- Power có được phân bổ hợp lý hay không.
- Tài nguyên quan trọng đã được chuyển lên cao hay chưa.
- Người chơi có phương án sơ tán hay không.
- NPC đã được cứu hoặc phân công như thế nào.

---

### 5.4. Hazard phải có dấu hiệu cảnh báo

Các thay đổi lớn phải được báo trước qua ít nhất một trong các nguồn:

- Forecast.
- Radio.
- Mực nước.
- Âm thanh.
- Dấu hiệu môi trường.
- NPC.
- Communication Station.
- Disaster Phase.

Không khóa một tuyến đường quan trọng mà không có dấu hiệu hợp lý.

---

## 6. Disaster Timeline

## Phase 0 — Normal

```text
Bắt đầu: Ngày 0, 17:00
Kết thúc: Ngày 0, 22:00
```

### Trạng thái

- Không mưa hoặc mưa rất nhẹ.
- Điện và nước thành phố còn hoạt động.
- Toàn bộ Route chính còn mở.
- Không có Black Water.
- Electromagnetic Interference chưa đáng kể.

### Chức năng gameplay

- Tutorial.
- Giới thiệu Shelter.
- Giới thiệu NPC.
- Thiết lập thói quen thế giới bình thường.
- Gieo dấu hiệu bất thường đầu tiên.

Từ `22:00` đến `06:00`, thế giới giữ trạng thái Normal trong quá trình ngủ.

---

## Phase 1 — Warning

```text
Bắt đầu: Ngày 1, 06:00
Kết thúc: Ngày 1, 18:00
```

### Trạng thái

- Cảnh báo siêu bão được phát.
- Người dân bắt đầu thu gom vật tư.
- Một số Location đông hoặc bị loot.
- Mưa chưa gây ngập nghiêm trọng.
- Route vẫn sử dụng được.

### Áp lực chính

- Thời gian chuẩn bị.
- Cạnh tranh tài nguyên.
- Chọn Location ưu tiên.
- Chọn hướng nâng cấp Shelter đầu tiên.

---

## Phase 2 — First Rain

```text
Bắt đầu: Ngày 1, 18:00
Kết thúc: Ngày 2, 10:00
```

### Trạng thái

- Mưa lớn bắt đầu.
- Khu vực thấp xuất hiện nước ngập.
- Drain Core có dấu hiệu hoạt động bất thường.
- Điện bắt đầu chập chờn.
- Radio mất tín hiệu trong thời gian ngắn.

### Hazard

```text
Rain Intensity: 2–3
Flood Depth: 0–2
Current Strength: 0–1
Interference: 1
Contamination: 0–1
```

Nước trong giai đoạn này là Untreated Floodwater, chưa phải toàn bộ Black Water.

---

## Phase 3 — Black Rain

```text
Bắt đầu: Ngày 2, 10:00
Kết thúc: Ngày 2, 22:00
```

### Trạng thái

- Mưa chuyển màu đen.
- Nước mới tích tụ chuyển sang Black Water.
- Thiết bị điện hoạt động không ổn định.
- Một số Forecast không còn chính xác.
- Tín hiệu bất thường bắt đầu xuất hiện.

### Hazard

```text
Rain Intensity: 3–4
Flood Depth: 1–3
Current Strength: 1–2
Interference: 2
Contamination: 2–3
```

### Thay đổi gameplay

- Equipment chống nước trở thành thiết yếu.
- Vật phẩm bị ngâm có thể bị ô nhiễm.
- Location tầng thấp bắt đầu mất giá trị.
- Người chơi phải di chuyển tài nguyên lên Elevated Storage.
- Route Intel cũ có thể trở nên lỗi thời nhanh.

---

## Phase 4 — Escalation

```text
Bắt đầu: Ngày 2, 22:00
Kết thúc: Ngày 3, 18:00
```

### Trạng thái

- Nước tiếp tục dâng.
- Một số Route Segment bị khóa.
- Nhiễu điện từ tăng mạnh.
- Lưới điện khu vực ngừng hoạt động.
- Shelter chịu Water Intrusion liên tục.
- NPC chưa được cứu có thể mất tích.

### Hazard

```text
Rain Intensity: 4–5
Flood Depth: 2–4
Current Strength: 2–4
Interference: 3
Contamination: 3–4
```

### Chức năng gameplay

- Chuyến khám phá cuối.
- Hoàn thành hoặc từ bỏ mục tiêu phụ.
- Chuẩn bị Power.
- Chọn tài nguyên bảo vệ.
- Kích hoạt Temporary Shelter nếu cần.
- Quyết định giữ Main Shelter hay chuẩn bị sơ tán.

---

## Phase 5 — Peak

```text
Bắt đầu: Ngày 3, 18:00
Kết thúc: Ngày 4, 06:00
```

### Trạng thái

- Mưa đạt cường độ cao nhất.
- Nước tại khu vực thấp trở nên không thể vượt qua.
- Hoạt động ngoài Shelter cực kỳ nguy hiểm.
- Shelter Event trở thành áp lực chính.
- Black Water có thể tràn vào các Zone thấp.
- Communication Station nhận tín hiệu cao điểm.

### Hazard

```text
Rain Intensity: 5
Flood Depth: 3–5
Current Strength: 3–5
Interference: 4
Contamination: 4
```

### Gameplay

Người chơi phải:

- Vận hành máy bơm.
- Phân bổ Power.
- Xử lý Drain Core.
- Bảo vệ Safe Zone.
- Di chuyển tài nguyên khi cần.
- Điều trị NPC hoặc Player.
- Sơ tán nếu Shelter không còn khả năng duy trì.

---

## Phase 6 — Aftermath

```text
Bắt đầu: Ngày 4, 06:00
Kết thúc: Ngày 4, 12:00
```

### Trạng thái

- Mưa giảm.
- Nước ngừng dâng.
- Một số khu vực bắt đầu rút nước.
- Black Water và Contamination vẫn tồn tại.
- Hạ tầng chưa phục hồi.
- Persistent World State được xác lập.

### Gameplay

- Đánh giá thiệt hại.
- Xử lý Event cuối.
- Xác nhận NPC Outcome.
- Thu thập Narrative Hook.
- Tính Chapter Outcome.

---

## 7. Disaster State

Disaster sử dụng các biến toàn cục:

```text
current_phase
rain_intensity
rain_type
regional_water_pressure
contamination_level
interference_level
wind_intensity
infrastructure_state
```

---

## 8. Rain State

```text
None
Normal Rain
Heavy Rain
Black Rain
Extreme Black Rain
Weakening Black Rain
```

Rain State ảnh hưởng:

- Wet Gain.
- Visibility.
- Body Temperature.
- Flood progression.
- Item exposure.
- Shelter Water Intrusion.
- Âm thanh môi trường.

---

## 9. Flood Model

MVP không mô phỏng chất lỏng vật lý trên toàn bộ bản đồ.

Flood được biểu diễn bằng ba lớp:

```text
Regional Water Pressure
Route Flood State
Local Zone Water Level
```

### Regional Water Pressure

Đại diện cho xu hướng nước dâng toàn khu vực.

```text
0: Bình thường
1: Hệ thống thoát nước quá tải
2: Ngập cục bộ
3: Ngập khu vực
4: Ngập nghiêm trọng
5: Đỉnh lũ
```

### Route Flood State

```text
Dry
Shallow
Medium
Deep
Impassable
```

### Local Zone Water Level

Được định nghĩa riêng trong từng Location hoặc Shelter Zone.

---

## 10. Flood Update Rule

Flood State được tính từ:

```text
Base Elevation
+
Disaster Phase
+
Regional Water Pressure
+
Infrastructure Modifier
+
Local Event Modifier
```

Ví dụ:

```text
Trạm bơm được khôi phục
→
Giảm một mức Flood State tại khu vực liên quan
```

```text
Drain Core chảy ngược
→
Tăng Water Intrusion tại Main Shelter
```

Người chơi chỉ thay đổi Modifier cục bộ, không thay đổi Global Phase.

---

## 11. Current Strength

Current Strength chỉ xuất hiện tại các Route hoặc Zone có dòng chảy.

```text
None
Weak
Moderate
Strong
Critical
```

Rủi ro vượt dòng nước phụ thuộc vào:

- Current Strength.
- Flood Depth.
- Stamina.
- Carry Load.
- Injury.
- Rope hoặc điểm bám.
- Số người hỗ trợ.

Current Strength cấp `Critical` được xem là không thể vượt an toàn trong MVP.

---

## 12. Black Water Contamination

Black Water có thể tác động lên:

- Player.
- NPC.
- Item.
- Storage.
- Shelter Zone.
- Nguồn nước.

Exposure tăng nhanh hơn khi:

- Người chơi bơi.
- Có vết thương hở.
- Không có Equipment bảo vệ.
- Mang đồ bị nhiễm sát cơ thể.
- Ở trong Zone ngập trong thời gian dài.

Black Water không gây tử vong tức thời.

Chuỗi hậu quả chính:

```text
Exposure
↓
Status Effect
↓
Khả năng hồi phục giảm
↓
Sick hoặc Health Damage nếu không xử lý
```

---

## 13. Electromagnetic Interference

Interference có năm mức:

```text
0: Stable
1: Distorted
2: Unreliable
3: Interrupted
4: Critical
```

Interference ảnh hưởng:

- Radio.
- Communication Station.
- Forecast.
- Signal quality.
- Máy bơm điện.
- Battery efficiency.
- Lighting.
- Một số Event trigger.

Interference không tự động phá hỏng thiết bị.

Thiết bị chỉ hỏng khi:

- Condition thấp.
- Đang quá tải.
- Bị ngập.
- Event có điều kiện hợp lệ.
- Không được bảo trì.

---

## 14. Wind và Visibility

Wind là Hazard hỗ trợ, không phải hệ thống trung tâm.

Wind ảnh hưởng:

- Thời gian di chuyển ngoài trời.
- Hoạt động trên mái.
- Tốc độ Wet Gain.
- Khả năng nghe tín hiệu môi trường.
- Roof Access.
- Structural Collapse Event.

Trong Peak Phase, một số hoạt động trên mái có thể bị khóa do Wind Intensity.

---

## 15. Infrastructure State

Các hạ tầng chính:

```text
city_power_grid
drainage_network
regional_pump_station
communication_network
road_network
```

Mỗi hạ tầng có trạng thái:

```text
Operational
Unstable
Partial Failure
Failed
```

Hạ tầng được cập nhật bởi:

- Disaster Phase.
- Event.
- Player Action.
- Module State.

---

## 16. Disaster Forecast

Forecast có thể cung cấp:

- Thời điểm mưa tăng.
- Mức nước dự kiến.
- Khu vực bị cô lập.
- Thời điểm mất điện.
- Thời điểm Peak Phase.
- Mức độ nhiễu dự kiến.

Forecast không thay đổi Global Timeline.

Forecast tốt chỉ giúp người chơi biết trước Timeline và Local Modifier chính xác hơn.

---

## 17. Bắt buộc và biến thể

### Thành phần cố định trong mọi lượt chơi

- Mưa chuyển thành Black Rain.
- Lưới điện khu vực mất ổn định.
- Tuyến thấp bị khóa.
- Drain Core chảy ngược.
- Peak Phase xảy ra.
- Tín hiệu bất thường xuất hiện.
- Nước bắt đầu rút trong Aftermath.

### Thành phần có biến thể

- Route Segment bị khóa đầu tiên.
- Location chịu Structural Collapse.
- NPC xuất hiện tại Location nào.
- Opportunity Event.
- Một số vật tư bị phá hủy.
- Thời điểm cục bộ trong khoảng cho phép.

Biến thể không được thay đổi khả năng hoàn thành Chapter một cách ngẫu nhiên.

---

## 18. Event Anchor

Các Event bắt buộc gắn với Disaster:

```text
storm_warning
first_heavy_rain
black_rain_transition
regional_power_failure
regional_pump_failure
drain_backflow
low_route_closure
shelter_pump_jam
peak_signal
rain_weakening
```

Mỗi Event Anchor có thể tạo nhiều biến thể nội dung nhưng phải giữ cùng chức năng gameplay.

---

## 19. Tác động lên hệ thống khác

### Player Condition

- Wet.
- Cold.
- Black Water Exposure.
- Disoriented.
- Fatigue tăng do di chuyển khó khăn.

### Inventory

- Item bị Wet hoặc Contaminated.
- Container chống nước tăng giá trị.
- Carry Load tăng rủi ro trong nước.

### Shelter

- Water Intrusion.
- Power shortage.
- Storage contamination.
- Module failure.
- Structural damage.

### Location

- Zone bị khóa.
- Lối vào thay đổi.
- Loot bị phá hoặc ô nhiễm.
- NPC di chuyển.

### World Map

- Route bị chặn.
- Travel Time tăng.
- Tuyến cao trở thành lối chính.
- Shortcut có giá trị chiến lược.

### Information

- Forecast mất Confidence.
- Radio bị nhiễu.
- Intel nhanh lỗi thời.
- Tín hiệu bất thường xuất hiện.

---

## 20. Điều kiện kết thúc Disaster

Disaster kết thúc gameplay chính khi:

```text
current_phase == Aftermath
AND
peak_events_resolved == true
AND
player_group_state_resolved == true
```

Nhóm được xem là sống sót nếu:

```text
at_least_one_player_alive == true
AND
safe_zone_available == true
AND
minimum_water_requirement_met == true
```

Shelter có thể là:

- Main Shelter.
- Temporary Shelter.
- Emergency Evacuation Site.

---

## 21. Narrative Clue

MVP sử dụng tối đa ba loại manh mối:

```text
signal_data
black_water_sample
weather_station_record
```

Người chơi không cần thu thập toàn bộ để hoàn thành Chapter.

Số lượng và loại manh mối ảnh hưởng:

- Outcome Report.
- Campaign Knowledge.
- Narrative Hook của Chapter sau.

---

## 22. Dữ liệu hệ thống

```text
black_rain_disaster
├── current_phase
├── phase_start_time
├── phase_end_time
├── rain_state
├── rain_intensity
├── regional_water_pressure
├── contamination_level
├── interference_level
├── wind_intensity
├── infrastructure_states
├── regional_modifiers
├── active_event_anchors
└── persistent_flags
```

---

## 23. Phạm vi MVP

Triển khai:

- Bảy Disaster Phase.
- Rain State.
- Regional Water Pressure.
- Route Flood State.
- Local Zone Water Level.
- Current Strength.
- Black Water Contamination.
- Electromagnetic Interference.
- Infrastructure State.
- Forecast.
- Event Anchor.
- Narrative Clue.

Không triển khai:

- Mô phỏng khí tượng khoa học.
- Fluid Simulation toàn bản đồ.
- Wind Physics chi tiết.
- Phá hủy công trình hoàn toàn động.
- Thay đổi Global Timeline bởi người chơi.
- Giải thích đầy đủ nguồn gốc Mưa Đen.

---

## 24. Quyết định chốt

- Disaster tiến triển theo World Clock cố định.
- Người chơi chỉ thay đổi hậu quả cục bộ.
- Black Rain bắt đầu từ Ngày 2 lúc `10:00`.
- Peak diễn ra từ Ngày 3 `18:00` đến Ngày 4 `06:00`.
- Flood được triển khai bằng trạng thái khu vực, Route và Zone.
- Black Water gây Exposure tích lũy.
- Interference gây áp lực lên Information và Equipment.
- Peak Phase chỉ kiểm tra các hệ thống đã được giới thiệu.
- Các Event bắt buộc không phụ thuộc hoàn toàn vào RNG.
- Disaster kết thúc bằng Aftermath và Chapter Outcome.
