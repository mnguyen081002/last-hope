# Game Design Pillars

## 1. Mục đích

Tài liệu này định nghĩa các trụ cột gameplay cốt lõi của **Last Hope**.

Mọi hệ thống, Location, Event, NPC và Chapter phải phục vụ ít nhất một Pillar.

Last Hope sử dụng năm Pillar:

1. Exploration Under Pressure
2. Resource Trade-offs
3. Shelter Preparation
4. Information-Driven Decisions
5. Persistent Consequences

---

# 2. Exploration Under Pressure

## Định nghĩa

Mọi chuyến khám phá đều diễn ra dưới áp lực từ:

- World Clock.
- Hazard.
- Player Condition.
- Carry Load.
- Equipment Durability.
- Event Deadline.
- Công việc đang chờ tại Shelter.

Người chơi phải liên tục quyết định:

```text
Tiếp tục khám phá
OR
Quay về an toàn
Yêu cầu thiết kế
Di chuyển và Search đều tiêu tốn thời gian.
Route phải có ưu và nhược điểm.
Inventory không đủ để mang mọi thứ.
Chuyến về có thể nguy hiểm hơn chuyến đi.
Thế giới tiếp tục thay đổi khi người chơi khám phá.
Không được phép
Search nhận toàn bộ loot ngay lập tức.
Route khác nhau nhưng không khác gameplay.
Exploration không có rủi ro hoặc chi phí.
Người chơi luôn có thể lấy hết tài nguyên trong một chuyến.
3. Resource Trade-offs
Định nghĩa

Mỗi Resource quan trọng phải có nhiều công dụng và nguồn cung giới hạn.

Ví dụ:

Fuel
→
Generator
OR
Water Pump
OR
Water Purifier

Sử dụng Resource cho một mục tiêu phải làm giảm khả năng phục vụ mục tiêu khác.

Yêu cầu thiết kế

Mỗi Resource quan trọng nên có:

Ít nhất hai nguồn hợp lý.
Ít nhất hai công dụng.
Chi phí thu thập hoặc xử lý.
Chi phí vận chuyển hoặc lưu trữ.
Ít nhất một Resource Sink.
Không được phép
Resource quan trọng chỉ có một nguồn duy nhất.
Loot Respawn không có nguyên nhân.
Storage hoặc Inventory vô hạn.
Repair hoặc Crafting tạo tài nguyên vô hạn.
Một lựa chọn sử dụng Resource luôn tối ưu.
4. Shelter Preparation
Định nghĩa

Shelter là trung tâm chiến lược nơi Resource, Information và thời gian được chuyển thành khả năng sống sót.

Shelter không phải:

Menu nâng cấp.
Kho đồ vô hạn.
Khu vực an toàn tuyệt đối.
Yêu cầu thiết kế
Shelter có Zone và Module.
Module có Build Cost, Build Time và điều kiện vận hành.
Shelter có thể hư hỏng từng phần.
Active Task cạnh tranh thời gian với Exploration.
Passive Task tiếp tục hoạt động theo World Clock.
Người chơi không thể xây mọi Module trong một lượt.
Peak Phase phải kiểm tra quá trình chuẩn bị.
Không được phép
Module hoạt động vĩnh viễn mà không có chi phí.
Shelter miễn nhiễm Hazard.
Ở lại Shelter đồng nghĩa không có gameplay.
Peak Phase bỏ qua trạng thái Shelter.
Relocation diễn ra tức thời qua menu.
5. Information-Driven Decisions
Định nghĩa

Người chơi không biết toàn bộ World State.

Information giúp người chơi:

Chọn Location.
Chọn Route.
Chuẩn bị Equipment.
Dự đoán Hazard.
Phát hiện Event.
Quyết định gia cố hoặc sơ tán Shelter.
Yêu cầu thiết kế

Mỗi Intel phải có:

source
observed_time
confidence
expiration_time

Information có thể:

Không đầy đủ.
Lỗi thời.
Bị nhiễu.
Mâu thuẫn với nguồn khác.

Sự bất định phải có nguyên nhân rõ ràng.

Không được phép
World Map hiển thị toàn bộ trạng thái thời gian thực.
Quest Marker xuất hiện không có nguồn.
Forecast luôn chính xác tuyệt đối.
NPC biết toàn bộ thế giới.
Information chỉ tồn tại như collectible.
6. Persistent Consequences
Định nghĩa

Quyết định của người chơi phải để lại hậu quả trong:

Expedition.
Ngày hiện tại.
Chapter.
Các Chapter sau.

World State phải ghi nhớ hành động của người chơi.

Yêu cầu thiết kế

Các trạng thái cần được duy trì:

Location State.
Loot Depletion.
Route State.
NPC State.
Shelter Damage.
Resource Loss.
Event Consequence.
Relationship.
Knowledge.
Blueprint.
Chapter Outcome.

Phần lớn thất bại phải cho phép người chơi tiếp tục bằng một phương án kém thuận lợi hơn.

Ví dụ:

Main Shelter bị mất
↓
Di chuyển tới Temporary Shelter
↓
Mất phần lớn tài nguyên
↓
Vẫn sống sót với Outcome thấp hơn
Không được phép
Location tự reset.
NPC hoặc Resource tự phục hồi tùy ý.
Sai lầm nhỏ gây Game Over tức thời.
Lựa chọn chỉ thay đổi hội thoại mà không ảnh hưởng gameplay.
Chapter sau bỏ qua hoàn toàn hậu quả Chapter trước.
7. Quan hệ giữa các Pillar
Information
↓
Giúp chọn mục tiêu và Route

Exploration
↓
Tạo Resource và Intel

Resource Trade-offs
↓
Buộc người chơi chọn ưu tiên

Shelter Preparation
↓
Chuyển tài nguyên thành khả năng sống sót

Persistent Consequences
↓
Thay đổi World State

World State mới
↓
Tạo áp lực và thông tin mới

Các Pillar phải hoạt động như một vòng liên kết, không phải các hệ thống độc lập.

8. Kiểm tra tính năng mới

Mỗi tính năng mới phải trả lời được:

Phục vụ Pillar nào?
Tạo quyết định gì?
Có chi phí gì?
Thay đổi trạng thái nào?
Xuất hiện ở đâu trong Core Gameplay Loop?
Hậu quả có thể được người chơi hiểu không?
Có cần thiết cho Core hoặc MVP không?

Tính năng không tạo quyết định, chi phí hoặc hậu quả không nên được đưa vào Core.

9. Quyết định chốt
Last Hope sử dụng năm Game Design Pillar.
Exploration Under Pressure là trải nghiệm trực tiếp ưu tiên cao nhất.
Resource phải tạo Trade-off.
Shelter là trung tâm chuẩn bị và ứng phó.
Information là Resource chiến lược.
Quyết định phải tạo Persistent Consequence.
Mọi hệ thống và nội dung quan trọng phải phục vụ ít nhất một Pillar.
Không hard-code nội dung của một Disaster cụ thể vào Core Pillar.
```
