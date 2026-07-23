# Information System Design

## 1. Mục tiêu

Information System xác định cách người chơi thu thập, đánh giá và sử dụng thông tin để đưa ra quyết định sinh tồn.

Hệ thống phải đảm bảo:

- Thông tin có giá trị tương đương tài nguyên vật lý.
- Người chơi không biết toàn bộ trạng thái thế giới.
- Thông tin giúp giảm rủi ro nhưng không loại bỏ sự bất định.
- Thông tin có nguồn, thời điểm và độ tin cậy rõ ràng.
- Hệ thống hoạt động trong Single-player và Multiplayer.
- Có thể dùng lại cho nhiều Disaster.

---

# 2. Phạm vi thông tin

Thông tin có thể mô tả:

- Vị trí Location.
- Trạng thái tuyến đường.
- Mức Hazard.
- Tài nguyên có khả năng xuất hiện.
- Event đang diễn ra.
- NPC hoặc nhóm sống sót.
- Thời tiết.
- Disaster Timeline.
- Trạng thái hạ tầng.
- Độ an toàn của vị trí Shelter.
- Cơ hội di dời Shelter.

---

# 3. Nguyên tắc thiết kế

## 3.1. Không hiển thị toàn bộ thông tin từ đầu

World Map ban đầu chỉ chứa những gì nhân vật hợp lý có thể biết.

Thông tin mới được mở qua:

- Khám phá trực tiếp.
- Quan sát.
- Radio.
- NPC.
- Ghi chú.
- Bản đồ.
- Thiết bị.
- Event.

---

## 3.2. Mọi thông tin đều có nguồn

Mỗi thông tin phải ghi nhận:

```text
source
observed_time
confidence
expiration_time
```

Người chơi cần biết thông tin đến từ đâu và còn đáng tin hay không.

---

## 3.3. Thông tin có thể lỗi thời

Thế giới tiếp tục thay đổi sau khi thông tin được thu thập.

Ví dụ:

```text
10:00
Tuyến phía đông còn mở

↓

14:00
Nước dâng

↓

Thông tin cũ không còn chính xác
```

Thông tin lỗi thời không tự biến mất nhưng phải được đánh dấu.

---

## 3.4. Bất định phải có giới hạn

Thông tin không chính xác có thể tồn tại, nhưng không được tạo kết quả hoàn toàn ngẫu nhiên.

Sai lệch phải đến từ nguyên nhân rõ ràng:

- Nguồn không đáng tin.
- Nhiễu điện từ.
- Thông tin đã cũ.
- Người cung cấp không quan sát trực tiếp.
- Disaster thay đổi nhanh.

---

# 4. Đơn vị thông tin

Mỗi đơn vị thông tin được gọi là `Intel Record`.

```text
intel_id
intel_type
subject_id
source_id
observed_time
received_time
confidence
expiration_time
content
map_effect
verified
```

Một đối tượng có thể có nhiều Intel Record từ các nguồn khác nhau.

---

# 5. Loại thông tin

## Location Intel

Cho biết:

- Vị trí.
- Lối vào.
- Zone đã biết.
- Tài nguyên dự kiến.
- NPC hiện diện.
- Mức depletion được quan sát.

---

## Route Intel

Cho biết:

- Tuyến đường còn mở hay không.
- Độ sâu nước.
- Cường độ dòng chảy.
- Cầu hoặc đường có nguy cơ sập.
- Tuyến thay thế.
- Thời gian di chuyển dự kiến.

---

## Hazard Intel

Cho biết:

- Loại Hazard.
- Intensity.
- Phạm vi ảnh hưởng.
- Thời điểm dự kiến thay đổi.
- Protection cần thiết.

---

## Resource Intel

Cho biết:

- Nguồn tài nguyên có khả năng tồn tại.
- Số lượng ước tính.
- Điều kiện tiếp cận.
- Nguy cơ bị người khác lấy trước.

Resource Intel không đảm bảo loot chắc chắn trừ khi nguồn đã được xác minh trực tiếp.

---

## Event Intel

Cho biết:

- Event đang diễn ra.
- Vị trí.
- Deadline.
- Rủi ro.
- Phần thưởng hoặc hậu quả dự kiến.

---

## Disaster Intel

Cho biết:

- Phase hiện tại.
- Thời điểm chuyển Phase dự kiến.
- Cường độ mưa.
- Mực nước dự báo.
- Khu vực chịu ảnh hưởng.
- Mức độ tin cậy của dự báo.

---

## Shelter Intel

Cho biết:

- Mức an toàn của vị trí hiện tại.
- Nguy cơ ngập.
- Tuyến sơ tán.
- Vị trí Shelter tiềm năng.
- Khả năng tiếp cận tài nguyên.
- Hạ tầng có thể sử dụng.

---

# 6. Nguồn thông tin

## Quan sát trực tiếp

Độ tin cậy cao tại thời điểm quan sát.

Ví dụ:

- Nhìn thấy đường bị ngập.
- Kiểm tra một Location.
- Quan sát dây điện rơi xuống nước.
- Đo mực nước tại Shelter.

Thông tin có thể nhanh chóng lỗi thời.

---

## Radio

Cung cấp:

- Dự báo.
- Cảnh báo.
- Tín hiệu cầu cứu.
- Thông tin cứu hộ.
- Tín hiệu bất thường.

Radio phụ thuộc vào:

- Power.
- Chất lượng thiết bị.
- Nhiễu điện từ.
- Antenna.
- Signal Stabilizer.

---

## NPC

NPC có thể cung cấp:

- Tin đồn.
- Thông tin địa phương.
- Vị trí tài nguyên.
- Tuyến đường.
- Cảnh báo nhóm đối địch.
- Địa điểm Shelter tiềm năng.

Độ tin cậy phụ thuộc vào:

- NPC đã quan sát trực tiếp hay chưa.
- Thời điểm quan sát.
- Quan hệ với người chơi.
- Động cơ của NPC.

---

## Tài liệu

Bao gồm:

- Bản đồ giấy.
- Ghi chú.
- Hồ sơ.
- Hướng dẫn kỹ thuật.
- Lịch vận hành.
- Mã khóa.

Tài liệu thường đáng tin về cấu trúc cố định nhưng không phản ánh trạng thái hiện tại.

---

## Thiết bị

Ví dụ:

- Máy đo mực nước.
- Cảm biến thời tiết.
- Thiết bị kiểm tra nước.
- Camera.
- Trạm quan trắc.

Thiết bị có thể cung cấp dữ liệu chính xác nhưng cần:

- Power.
- Bảo trì.
- Kết nối.
- Vị trí lắp đặt phù hợp.

---

# 7. Confidence

Mỗi Intel Record có một mức độ tin cậy.

```text
Confirmed
Reliable
Uncertain
Unverified
```

## Confirmed

Được người chơi quan sát hoặc xác minh trực tiếp gần đây.

## Reliable

Đến từ nguồn đáng tin nhưng chưa được kiểm chứng trực tiếp.

## Uncertain

Có dấu hiệu hợp lý nhưng thiếu dữ liệu hoặc đã bắt đầu lỗi thời.

## Unverified

Tin đồn hoặc tín hiệu chưa xác định.

Confidence không phải xác suất loot chính xác và không cần hiển thị bằng phần trăm.

---

# 8. Information Age

Độ mới của thông tin được tính từ World Clock.

```text
Information Age
=
Current World Time
-
Observed Time
```

Thông tin được phân loại:

```text
Current
Aging
Outdated
Invalid
```

Ngưỡng phụ thuộc vào loại thông tin.

Ví dụ:

- Layout tòa nhà lỗi thời chậm.
- Trạng thái tuyến đường lỗi thời nhanh.
- Hazard Forecast có thời hạn cụ thể.
- Resource Intel có thể mất giá trị khi địa điểm bị loot.

---

# 9. Xác minh thông tin

Người chơi có thể xác minh bằng:

- Quan sát trực tiếp.
- So sánh nhiều nguồn.
- Dùng thiết bị.
- Liên lạc lại với NPC.
- Đến điểm quan sát.
- Khôi phục trạm quan trắc.

Khi nhiều nguồn trùng khớp, Confidence tăng.

Khi các nguồn mâu thuẫn, hệ thống phải hiển thị cả hai thay vì tự chọn một kết quả.

---

# 10. World Map Integration

World Map chỉ hiển thị thông tin người chơi đã biết.

Mỗi Location hoặc Route có thể hiển thị:

- Trạng thái gần nhất.
- Thời điểm cập nhật.
- Confidence.
- Hazard đã biết.
- Event đang hoạt động.
- Ghi chú của người chơi.

Ví dụ:

```text
Tuyến phía đông

Trạng thái: Có thể đi qua
Cập nhật lần cuối: 2 giờ trước
Confidence: Uncertain
Nguy cơ: Nước đang dâng
```

Không hiển thị trạng thái thời gian thực nếu người chơi không có nguồn dữ liệu phù hợp.

---

# 11. Disaster Forecast

Forecast là Intel Record đặc biệt.

```text
forecast_id
affected_area
forecast_start
forecast_end
expected_intensity
confidence
source
last_update_time
```

Forecast có thể dự đoán:

- Lượng mưa.
- Mực nước.
- Dòng chảy.
- Nhiễu điện từ.
- Thời điểm Disaster chuyển Phase.

Forecast tốt giúp người chơi:

- Chọn thời điểm khám phá.
- Di chuyển tài nguyên.
- Gia cố Shelter.
- Di dời Shelter.
- Tránh tuyến sắp bị cô lập.

---

# 12. Electromagnetic Interference

Nhiễu điện từ có thể ảnh hưởng đến:

- Chất lượng tín hiệu radio.
- Thời gian nhận thông tin.
- Confidence.
- Khả năng cập nhật từ xa.
- Hoạt động của thiết bị quan trắc.

Nhiễu không được tự động biến thông tin đúng thành thông tin giả.

Nó có thể:

- Làm mất một phần nội dung.
- Trì hoãn cập nhật.
- Giảm độ tin cậy.
- Tạo tín hiệu không xác định.
- Ngăn xác minh nguồn.

---

# 13. Tín hiệu bất thường

Tín hiệu bất thường là một loại Intel riêng của Siêu Bão Mưa Đen.

Đặc điểm:

- Không xác định nguồn.
- Xuất hiện theo Disaster Phase.
- Có thể chứa dữ liệu hữu ích hoặc gây nhiễu.
- Không được giải thích hoàn toàn trong MVP.

Tín hiệu bất thường phải phục vụ ít nhất một mục đích:

- Cảnh báo Hazard.
- Chỉ dẫn Location.
- Báo trước Event.
- Mở Narrative Hook.

Không sử dụng tín hiệu chỉ để tạo không khí mà không có tác động gameplay.

---

# 14. Communication Station

Communication Station là module Shelter dùng để:

- Nhận bản tin.
- Theo dõi Event.
- Xác minh tín hiệu.
- Liên lạc NPC.
- Cập nhật World Map.

Hiệu quả phụ thuộc vào:

```text
module_condition
power_supply
signal_quality
interference_level
operator
```

Communication Station không tự cung cấp toàn bộ thông tin thế giới.

Nó mở rộng phạm vi và chất lượng nguồn tin.

---

# 15. Chia sẻ thông tin trong Multiplayer

Thông tin được chia thành:

## Shared Intel

Tự động đồng bộ cho cả nhóm:

- Location đã phát hiện.
- Hazard đã xác minh.
- Event.
- Shelter Intel.
- Route Intel.

## Personal Observation

Chỉ thuộc người quan sát cho đến khi:

- Truyền qua radio.
- Trở về Shelter.
- Đánh dấu trên bản đồ chung.
- Chia sẻ trực tiếp với đồng đội.

Khi không có liên lạc ổn định, người chơi ngoài hiện trường không tự động cập nhật toàn bộ thông tin cho nhóm.

---

# 16. Information Cost

Thu thập thông tin phải có chi phí.

Chi phí có thể gồm:

- Thời gian.
- Power.
- Pin.
- Nhiên liệu.
- Di chuyển.
- Rủi ro Hazard.
- Quan hệ NPC.
- Linh kiện sửa thiết bị.

Thông tin có độ chính xác cao thường cần đầu tư lớn hơn.

---

# 17. Event và Information

Event có thể được:

- Phát hiện trước.
- Phát hiện khi đang diễn ra.
- Hoàn toàn bị bỏ lỡ.

Mỗi Event cần định nghĩa:

```text
discovery_sources
minimum_information
deadline
information_updates
expiration_result
```

Event quan trọng phải có ít nhất hai phương thức phát hiện.

---

# 18. UI Requirement

UI phải hiển thị:

- Nội dung thông tin.
- Nguồn.
- Thời điểm quan sát.
- Confidence.
- Tình trạng lỗi thời.
- Deadline nếu có.
- Vị trí liên quan.
- Mâu thuẫn với nguồn khác.

Người chơi phải phân biệt được:

```text
Sự thật đã xác minh
Dự báo
Ước tính
Tin đồn
Tín hiệu không xác định
```

---

# 19. Dữ liệu hệ thống

## Intel Record

```text
intel_id
intel_type
subject_id
source_id
observed_time
received_time
confidence
expiration_time
content
verified
shared_state
```

## Intel Source

```text
source_id
source_type
reliability
location
active_state
interference_modifier
```

## Map Knowledge

```text
subject_id
latest_intel_id
known_location
known_routes
known_hazards
known_resources
known_events
player_notes
```

---

# 20. Phạm vi MVP

Triển khai:

- Intel Record.
- Location Intel.
- Route Intel.
- Hazard Intel.
- Event Intel.
- Disaster Forecast.
- Radio.
- NPC Information.
- Confidence.
- Information Age.
- World Map cập nhật theo thông tin.
- Electromagnetic Interference ảnh hưởng tín hiệu.
- Communication Station.
- Tín hiệu bất thường.
- Dữ liệu hỗ trợ chia sẻ trong Multiplayer.

Chưa triển khai:

- Hệ thống điều tra phức tạp.
- Mạng lưới tình báo NPC.
- Tạo tin đồn hoàn toàn tự động.
- Giải mã tín hiệu chuyên sâu.
- Hệ thống hội thoại phân nhánh lớn.
- Theo dõi mọi NPC theo thời gian thực.

---

# 21. Quyết định chốt

- Người chơi không biết toàn bộ trạng thái thế giới.
- Mọi thông tin có nguồn, thời điểm và Confidence.
- Thông tin có thể lỗi thời khi World Clock tiếp tục chạy.
- World Map chỉ hiển thị kiến thức người chơi đã thu thập.
- Thông tin quan trọng phải có nhiều nguồn phát hiện.
- Nhiễu điện từ làm giảm chất lượng hoặc trì hoãn thông tin, không tùy tiện tạo dữ liệu giả.
- Forecast hỗ trợ quyết định khám phá, gia cố và di dời Shelter.
- Tín hiệu bất thường phải có tác động gameplay hoặc narrative.
- Multiplayer phân biệt Shared Intel và Personal Observation.
- Thu thập thông tin luôn có chi phí.
