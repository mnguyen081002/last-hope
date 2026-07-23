# Event System Design

## 1. Mục tiêu

Event System xác định cách các sự kiện được tạo, phát hiện, tiến triển và kết thúc trong Last Hope.

Hệ thống phải đảm bảo:

- Thế giới thay đổi độc lập với người chơi.
- Event tạo ra quyết định, không chỉ cung cấp nội dung ngẫu nhiên.
- Mỗi Event có nguyên nhân, thời hạn và hậu quả rõ ràng.
- Event hoạt động nhất quán với World Clock.
- Event có thể tác động lên Player, NPC, Shelter, Location, Route và Resource Flow.
- Hệ thống hỗ trợ Single-player và Multiplayer.

---

# 2. Nguyên tắc thiết kế

## 2.1. Event phải tạo ra lựa chọn

Một Event hợp lệ phải yêu cầu người chơi lựa chọn ít nhất một trong các yếu tố:

- Có tham gia hay không.
- Tham gia khi nào.
- Dùng tài nguyên nào.
- Ưu tiên cứu người hay bảo vệ Shelter.
- Chọn phần thưởng hoặc hậu quả nào.
- Chấp nhận rủi ro ngắn hạn để đổi lấy lợi ích dài hạn.

Event không tạo ra quyết định không nên được triển khai thành gameplay riêng.

---

## 2.2. Event không chờ người chơi

Mọi Event sử dụng World Clock.

Event có thể:

- Bắt đầu khi người chơi không có mặt.
- Tiến triển trong lúc người chơi khám phá.
- Hết hạn trong lúc người chơi làm việc tại Shelter.
- Xảy ra trong thời gian ngủ.
- Thay đổi trạng thái trước khi người chơi tiếp cận.

---

## 2.3. Event phải có nguyên nhân

Event không được xuất hiện hoàn toàn vô cớ.

Nguồn kích hoạt có thể là:

- Disaster Phase.
- World Clock.
- Hazard.
- Trạng thái Shelter.
- Trạng thái Location.
- Hành động trước đó.
- NPC.
- Resource shortage.
- Một Event khác.

---

## 2.4. Event phải để lại hậu quả

Event có thể thay đổi:

- World State.
- Location State.
- Route State.
- Shelter State.
- NPC State.
- Resource availability.
- Information.
- Quan hệ.
- Disaster Timeline.

Các Event quan trọng phải để lại hậu quả sau khi kết thúc.

---

# 3. Phân loại Event

## 3.1. Global Event

Ảnh hưởng một phần lớn hoặc toàn bộ bản đồ.

Ví dụ:

- Mưa chuyển thành Mưa Đen.
- Nước dâng nhanh.
- Mất điện toàn khu vực.
- Nhiễu điện từ tăng mạnh.
- Disaster chuyển Phase.

Global Event thường do World Clock hoặc Disaster Timeline kích hoạt.

---

## 3.2. Regional Event

Ảnh hưởng một khu vực bản đồ.

Ví dụ:

- Khu thương mại bị ngập.
- Cầu phía đông bị phong tỏa.
- Trạm điện khu nam bị hỏng.
- Một nhóm sống sót chiếm khu dân cư.

---

## 3.3. Location Event

Xảy ra tại một Location cụ thể.

Ví dụ:

- NPC mắc kẹt.
- Kho bị mở.
- Nước tràn vào tầng hầm.
- Cửa chính bị sập.
- Vật tư cứu trợ xuất hiện.

---

## 3.4. Shelter Event

Xảy ra tại Shelter.

Ví dụ:

- Máy bơm bị tắc.
- Nước tràn vào Storage.
- Máy phát hỏng.
- NPC bị bệnh.
- Cửa chắn nước bị rò.
- Radio nhận tín hiệu bất thường.

---

## 3.5. Player Event

Gắn với trạng thái của một người chơi.

Ví dụ:

- Injury trở nặng.
- Black Water Exposure chuyển thành Sick.
- Fatigue đạt mức nguy hiểm.
- Người chơi bị Incapacitated.

---

## 3.6. NPC Event

Gắn với một hoặc nhiều NPC.

Ví dụ:

- NPC rời Shelter.
- NPC yêu cầu tài nguyên.
- NPC phát hiện Location mới.
- NPC mất tích.
- NPC xảy ra xung đột.

---

## 3.7. Opportunity Event

Tạo cơ hội có thời hạn.

Ví dụ:

- Xe cứu trợ gặp nạn.
- Thùng vật tư bị mắc trên mái nhà.
- Một tuyến đường tạm thời mở.
- Một khu vực nước rút trong thời gian ngắn.

Opportunity Event không được xuất hiện như loot respawn.

---

# 4. Cấu trúc Event

Mỗi Event có dữ liệu tối thiểu:

```text
event_id
event_type
scope
priority
state
source
trigger_conditions
start_time
deadline
discovery_sources
participants
affected_targets
available_responses
resolution_rules
success_effects
failure_effects
expiration_effects
persistent_effects
```

---

# 5. Vòng đời Event

```text
Dormant
↓
Triggered
↓
Undiscovered hoặc Discovered
↓
Active
↓
Resolved hoặc Expired
↓
Persistent Consequence
```

---

## Dormant

Event chưa đủ điều kiện kích hoạt.

---

## Triggered

Điều kiện đã đạt.

Event bắt đầu tồn tại trong thế giới.

Người chơi có thể chưa biết.

---

## Undiscovered

Event đang diễn ra nhưng chưa được người chơi phát hiện.

---

## Discovered

Người chơi nhận được thông tin về Event.

Thông tin có thể chưa đầy đủ.

---

## Active

Người chơi có thể tương tác hoặc phản ứng.

---

## Resolved

Event kết thúc do hành động hoặc lựa chọn của người chơi.

---

## Expired

Event hết hạn mà không được xử lý.

---

## Persistent Consequence

Hậu quả được ghi vào World State.

---

# 6. Trigger Condition

Event có thể dùng một hoặc nhiều điều kiện.

## Time Trigger

```text
world_time >= trigger_time
```

## Phase Trigger

```text
current_disaster_phase == target_phase
```

## State Trigger

```text
shelter.water_intrusion >= threshold
```

## Location Trigger

```text
location.current_state == required_state
```

## Player Trigger

```text
player.black_water_exposure >= threshold
```

## Resource Trigger

```text
clean_water <= minimum_value
```

## Decision Trigger

```text
previous_choice == target_choice
```

## Compound Trigger

```text
current_phase == Peak
AND
power_supply == 0
AND
water_intrusion >= High
```

---

# 7. Event Discovery

Event không nhất thiết được hiển thị ngay khi kích hoạt.

Nguồn phát hiện có thể gồm:

- Radio.
- NPC.
- Quan sát trực tiếp.
- Communication Station.
- Bản đồ.
- Âm thanh.
- Dấu hiệu môi trường.
- Một Event khác.

Mỗi Event quan trọng phải có ít nhất hai nguồn phát hiện nếu hợp lý.

---

# 8. Event Information

Thông tin về Event có thể được tiết lộ theo nhiều mức.

## Minimal

Người chơi chỉ biết có sự cố.

## Partial

Biết vị trí hoặc thời hạn, nhưng chưa biết đầy đủ rủi ro.

## Detailed

Biết:

- Vị trí.
- Deadline.
- Hazard.
- Người liên quan.
- Tài nguyên cần thiết.
- Hậu quả dự kiến.

## Confirmed

Thông tin đã được xác minh trực tiếp hoặc bằng nguồn đáng tin.

---

# 9. Deadline

Event có thể có:

```text
start_time
soft_deadline
hard_deadline
```

## Soft Deadline

Sau mốc này, Event trở nên khó hơn.

Ví dụ:

- Nước dâng cao hơn.
- NPC bị thương nặng hơn.
- Tuyến tiếp cận bị giới hạn.
- Phần thưởng giảm.

## Hard Deadline

Sau mốc này, Event kết thúc hoặc chuyển trạng thái khác.

Ví dụ:

- NPC mất tích.
- Location bị ngập hoàn toàn.
- Vật tư bị cuốn đi.
- Cầu bị sập.

Event có deadline phải cung cấp dấu hiệu trước khi hết hạn.

---

# 10. Event Response

Mỗi Event có thể cho phép nhiều phản ứng.

Ví dụ:

```text
Cứu NPC
Bỏ qua
Gửi NPC khác
Cung cấp tài nguyên từ xa
Đợi điều kiện thuận lợi hơn
```

Mỗi phản ứng có thể yêu cầu:

```text
required_items
required_skills
required_people
required_time
hazard_cost
resource_cost
```

Không phải Event nào cũng cần hội thoại lựa chọn.

Nhiều Event có thể được giải quyết trực tiếp bằng hành động trong thế giới.

---

# 11. Event Resolution

Kết quả Event được xác định từ:

- Hành động người chơi.
- Thời gian hoàn thành.
- Tài nguyên đã sử dụng.
- Trạng thái Player.
- Hazard hiện tại.
- NPC tham gia.
- Trạng thái Shelter hoặc Location.
- Quyết định trước đó.

Không sử dụng một lần tung xác suất duy nhất để quyết định toàn bộ kết quả.

---

# 12. Kiểm soát ngẫu nhiên

RNG chỉ nên dùng để tạo biến thể.

Có thể ngẫu nhiên hóa:

- Location xảy ra Event.
- Thời điểm trong một khoảng cho phép.
- Loại tài nguyên phụ.
- NPC tham gia.
- Một số hậu quả nhỏ.
- Tuyến tiếp cận còn mở.

Không nên ngẫu nhiên hóa hoàn toàn:

- Tài nguyên bắt buộc để thắng.
- Event cốt truyện chính.
- Điều kiện sống sót.
- Hậu quả lớn không có cảnh báo.
- Việc người chơi thành công hay thất bại sau khi đã chuẩn bị đúng.

---

# 13. Event Chain

Một số Event có thể tạo thành chuỗi.

```text
Radio nhận tín hiệu cầu cứu
↓
Phát hiện NPC mắc kẹt
↓
Cứu hoặc bỏ qua
↓
NPC gia nhập hoặc mất tích
↓
Ảnh hưởng Shelter và Chapter Outcome
```

Mỗi Event Chain cần:

```text
chain_id
current_step
branch_state
persistent_flags
completion_state
```

MVP chỉ nên sử dụng chuỗi ngắn từ hai đến bốn bước.

---

# 14. Event khi ngủ

Trong lúc ngủ, hệ thống mô phỏng:

- Shelter State.
- Passive Task.
- Hazard.
- NPC.
- Resource Consumption.
- Active Event.
- Disaster Timeline.

Event có thể:

## Không đánh thức người chơi

Ví dụ:

- Máy lọc hoàn thành.
- Pin sạc đầy.
- Mưa tăng nhẹ.

## Đánh thức một phần

Ví dụ:

- NPC trực đêm phát hiện rò nước.
- Radio nhận tín hiệu quan trọng.

## Đánh thức bắt buộc

Ví dụ:

- Shelter bị ngập.
- Cháy.
- Structural Integrity giảm nghiêm trọng.
- Disaster chuyển Phase.
- Máy bơm dừng trong Peak Phase.

Khi bị đánh thức, World Clock dừng mô phỏng giấc ngủ tại thời điểm Event xảy ra.

---

# 15. Shelter Event Rule

Shelter Event phải dựa trên trạng thái thực tế.

Ví dụ:

```text
Pump Jam Event
```

Chỉ có thể xảy ra khi:

- Máy bơm đang hoạt động.
- Nước có nhiều debris.
- Module Condition thấp hoặc chưa bảo trì.
- Disaster Phase đủ nghiêm trọng.

Không tạo sự cố thiết bị hoàn toàn không liên quan đến trạng thái module.

---

# 16. Location Event Rule

Location Event phải tôn trọng:

- Exploration State.
- Loot Depletion.
- Current Disaster State.
- NPC State.
- Route Accessibility.
- World Clock.

Ví dụ:

Một Location đã bị ngập hoàn toàn không thể xuất hiện Event yêu cầu vào tầng hầm nếu không có tuyến tiếp cận hợp lý.

---

# 17. Event và Resource Flow

Event có thể:

- Tạo nguồn tài nguyên mới hợp lý.
- Phá hủy tài nguyên.
- Di chuyển tài nguyên.
- Làm tài nguyên bị ô nhiễm.
- Thay đổi nhu cầu tài nguyên.
- Mở một nguồn tài nguyên mới.

Event không được dùng để hồi sinh loot cũ.

---

# 18. Event và Information System

Event có thể tồn tại mà người chơi chưa biết.

Information System quyết định:

- Người chơi có phát hiện Event hay không.
- Nhận được bao nhiêu thông tin.
- Thông tin có còn mới hay không.
- Nguồn thông tin có đáng tin hay không.
- Event có được chia sẻ trong Multiplayer hay không.

---

# 19. Event và Multiplayer

Event State là dữ liệu chung.

Nguyên tắc:

- Một Event chỉ tồn tại một lần trong World State.
- Tất cả người chơi cùng chia sẻ deadline.
- Nhiều người có thể xử lý các phần khác nhau của cùng Event.
- Event không dừng khi một người mở Inventory.
- Event có thể yêu cầu phối hợp.
- Kết quả phải được đồng bộ cho toàn bộ nhóm.

Ví dụ:

```text
Player A
Cứu NPC

Player B
Giữ dây

Player C
Vận hành máy bơm tại Shelter
```

---

# 20. Event Priority

Mỗi Event có một mức ưu tiên nội bộ.

```text
Critical
Major
Standard
Ambient
```

## Critical

Có thể gây thất bại hoặc thay đổi lớn nếu bỏ qua.

## Major

Ảnh hưởng đáng kể đến World State hoặc Chapter Outcome.

## Standard

Tạo lựa chọn tài nguyên hoặc tiến trình.

## Ambient

Tạo phản hồi thế giới nhưng không yêu cầu xử lý riêng.

Không nên có quá nhiều Critical Event cùng lúc.

---

# 21. Event Budget

Mỗi giai đoạn chỉ nên có số lượng Event giới hạn.

Baseline MVP:

| Loại Event |  Số Event đồng thời |
| ---------- | ------------------: |
| Critical   |                 0–1 |
| Major      |                 1–2 |
| Standard   |                 2–4 |
| Ambient    | Không giới hạn cứng |

Event Budget giúp tránh:

- Quá tải thông tin.
- Deadline chồng chéo.
- Mọi lựa chọn đều trở thành khẩn cấp.
- Người chơi không hiểu nguyên nhân thất bại.

---

# 22. Event của MVP Siêu Bão Mưa Đen

MVP cần các nhóm Event sau:

## Disaster Event

- Mưa bắt đầu.
- Mưa chuyển thành Mưa Đen.
- Nước dâng nhanh.
- Nhiễu điện từ tăng.
- Đỉnh lũ.
- Nước bắt đầu rút.

## Route Event

- Đường bị ngập.
- Cầu bị chặn.
- Cống trào ngược.
- Tuyến tắt được mở.
- Dây điện rơi xuống nước.

## Location Event

- NPC mắc kẹt.
- Kho bị lộ.
- Tầng thấp bị ngập.
- Location bị nhóm khác chiếm.
- Vật tư bị cuốn sang khu vực khác.

## Shelter Event

- Máy bơm bị tắc.
- Nước tràn vào Storage.
- Mất điện.
- Máy lọc nước hỏng.
- NPC bị bệnh.
- Cửa chắn nước bị rò.

## Opportunity Event

- Xe cứu hộ gặp nạn.
- Vật tư trôi dạt.
- Tín hiệu từ mái nhà.
- Một khu vực tạm thời rút nước.

---

# 23. Ví dụ Event

```text
event_id: shelter_pump_jam_01
event_type: shelter
priority: critical
source: debris_accumulation

trigger_conditions:
  current_phase: Peak
  pump_state: Operational
  pump_condition_max: Damaged
  water_intrusion_min: Deep Flood

start_time:
  world_clock_current

soft_deadline:
  +20 minutes

hard_deadline:
  +60 minutes

available_responses:
  - clear_pump
  - switch_to_backup
  - abandon_lower_floor

success_effects:
  - pump_operational
  - water_intrusion_reduced

failure_effects:
  - lower_floor_lost
  - wet_storage_damaged

persistent_effects:
  - shelter_lower_floor_state_changed
```

---

# 24. UI Requirement

Event UI phải hiển thị:

- Tên Event.
- Vị trí.
- Mức ưu tiên.
- Thời điểm phát hiện.
- Deadline đã biết.
- Nguồn thông tin.
- Trạng thái hiện tại.
- Lựa chọn hoặc mục tiêu.
- Hậu quả đã biết.
- Thông tin chưa xác minh.

Critical Event cần cảnh báo rõ nhưng không được chiếm toàn bộ màn hình nếu chưa yêu cầu phản ứng tức thời.

---

# 25. Dữ liệu hệ thống

## Event Instance

```text
event_instance_id
event_definition_id
state
priority
scope
source
start_time
soft_deadline
hard_deadline
discovery_state
participants
affected_targets
selected_response
resolution_state
persistent_flags
```

## Event Definition

```text
event_definition_id
event_type
trigger_conditions
discovery_sources
available_responses
resolution_rules
success_effects
failure_effects
expiration_effects
```

## Event Chain

```text
chain_id
current_step
branch_state
active_event
persistent_flags
completion_state
```

---

# 26. Phạm vi MVP

Triển khai:

- Global Event.
- Location Event.
- Shelter Event.
- Opportunity Event.
- Trigger theo thời gian, Phase và State.
- Event Discovery.
- Soft Deadline và Hard Deadline.
- Event Resolution.
- Persistent Consequence.
- Event khi ngủ.
- Event Chain ngắn.
- Event Budget.
- Dữ liệu tương thích Multiplayer.

Chưa triển khai:

- Event generation hoàn toàn tự động.
- Chuỗi nhiệm vụ dài.
- Hệ thống hội thoại phân nhánh lớn.
- Mô phỏng NPC ngoài màn hình chi tiết.
- Event cạnh tranh giữa nhiều faction.
- Hệ thống đạo đức riêng.

---

# 27. Quyết định chốt

- Event luôn sử dụng World Clock.
- Event có thể bắt đầu và kết thúc khi người chơi không có mặt.
- Mỗi Event phải có nguyên nhân và hậu quả rõ ràng.
- Event quan trọng có deadline và dấu hiệu cảnh báo.
- RNG chỉ dùng để tạo biến thể, không quyết định kết quả chính.
- Event có thể tồn tại trước khi người chơi phát hiện.
- Event quan trọng nên có nhiều nguồn phát hiện.
- Shelter Event phụ thuộc trạng thái Shelter và Module.
- Location Event phải tôn trọng trạng thái Location.
- Event không được dùng để hồi sinh loot.
- Event State được chia sẻ trong Multiplayer.
- MVP giới hạn số Event khẩn cấp đồng thời bằng Event Budget.
