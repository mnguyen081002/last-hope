# Core Gameplay Loop

## 1. Mục đích

Tài liệu này định nghĩa chuỗi hoạt động cốt lõi mà người chơi liên tục thực hiện trong **Last Hope**.

Core Gameplay Loop phải kết nối:

- Exploration.
- Resource Management.
- Shelter Preparation.
- Information.
- Event.
- Persistent Consequences.

---

# 2. Câu hỏi chiến lược trung tâm

> Tôi nên mạo hiểm thêm bao nhiêu hôm nay để tăng khả năng sống sót khi Disaster đạt đỉnh?

Mọi vòng lặp gameplay phải hỗ trợ câu hỏi này.

---

# 3. Core Loop tổng quát

```text
Đánh giá World State
↓
Xác định nhu cầu ưu tiên
↓
Lập kế hoạch
↓
Chuẩn bị Loadout
↓
Exploration hoặc Shelter Work
↓
Thu thập Resource và Information
↓
Xử lý hậu quả
↓
Cập nhật Shelter
↓
Nghỉ hoặc ngủ
↓
World State tiếp tục thay đổi
```

---

# 4. Giai đoạn 1 — Đánh giá trạng thái

Người chơi kiểm tra:

- World Clock.
- Disaster Phase.
- Shelter State.
- Player Condition.
- Resource dự trữ.
- Active Event.
- Intel mới.
- Route và Location đã biết.

Mục tiêu là xác định vấn đề cần xử lý trước.

Ví dụ:

```text
Clean Water thấp
Pump đang hỏng
Route phía đông sắp bị khóa
NPC cần cứu trước 18:00
```

---

# 5. Giai đoạn 2 — Chọn ưu tiên

Người chơi chọn một hoặc một số mục tiêu chính:

- Tìm Resource.
- Thu thập Information.
- Cứu NPC.
- Xử lý Event.
- Build hoặc Repair.
- Điều trị và hồi phục.
- Chuẩn bị Relocation.
- Duy trì Shelter.

Không đủ thời gian để hoàn thành mọi mục tiêu.

---

# 6. Giai đoạn 3 — Lập kế hoạch

Người chơi quyết định:

- Đi hay ở lại Shelter.
- Chọn Location nào.
- Chọn Route nào.
- Mang Equipment nào.
- Dành bao nhiêu Carry Capacity cho Resource.
- Mốc thời gian phải quay về.
- Event nào có thể bỏ qua.
- Task nào giao cho NPC.

Kế hoạch phải dựa trên Information hiện có, không dựa trên toàn bộ World State thật.

---

# 7. Giai đoạn 4 — Chuẩn bị Loadout

Người chơi chọn:

- Backpack.
- Protection Equipment.
- Tool.
- Consumable.
- Quick Access Item.
- Carry Capacity dự phòng.

Trade-off chính:

```text
Protection
vs
Tool
vs
Consumable
vs
Không gian mang Resource
```

Loadout phải thay đổi theo:

- Location.
- Route.
- Hazard.
- Mục tiêu.
- Thời gian dự kiến.

---

# 8. Giai đoạn 5A — Expedition Loop

```text
Rời Shelter
↓
Di chuyển theo Route
↓
Quan sát Hazard
↓
Điều chỉnh hướng đi
↓
Vào Location
↓
Search hoặc thực hiện mục tiêu
↓
Đánh giá thời gian và trạng thái
↓
Tiếp tục hoặc quay về
↓
Vận chuyển Resource về Shelter
```

## Decision Gate chính

Trong Expedition, người chơi phải liên tục đánh giá:

- Có tiếp tục Search không?
- Có đổi Route không?
- Có bỏ bớt vật phẩm không?
- Có xử lý Event phát sinh không?
- Có đủ Condition để quay về không?
- Có nên chấp nhận Hazard để tiết kiệm thời gian không?

Expedition kết thúc khi:

- Người chơi chủ động quay về.
- Mục tiêu hoàn thành.
- Carry Load đạt giới hạn.
- Hazard trở nên quá nguy hiểm.
- Route bị khóa.
- Player Condition không còn an toàn.

---

# 9. Giai đoạn 5B — Shelter Loop

```text
Kiểm tra Shelter
↓
Phân loại Resource
↓
Xử lý đồ Wet hoặc Contaminated
↓
Build, Repair hoặc vận hành Module
↓
Điều trị và hồi phục
↓
Phân bổ Power và Resource
↓
Chuẩn bị cho giai đoạn tiếp theo
```

Ở lại Shelter phải tạo tiến trình thực tế.

Các hoạt động chính:

- Build.
- Repair.
- Process Water.
- Dry Equipment.
- Treat Injury.
- Organize Storage.
- Analyze Intel.
- Maintain Module.
- Assign NPC.
- Prepare Relocation.
- Rest.

Shelter Work cạnh tranh trực tiếp với Exploration về World Time.

---

# 10. Giai đoạn 6 — Xử lý Resource

Resource mang về Shelter đi qua chuỗi:

```text
Inspection
↓
Cleaning hoặc Isolation
↓
Processing
↓
Storage
↓
Consumption hoặc Construction
```

Người chơi phải quyết định:

- Resource nào dùng ngay.
- Resource nào dự trữ.
- Resource nào cần xử lý.
- Resource nào dành cho Module.
- Resource nào dành cho NPC.
- Resource nào không đáng giữ.

---

# 11. Giai đoạn 7 — Xử lý hậu quả

Sau mỗi hoạt động, game cập nhật:

- Player Condition.
- Inventory.
- Equipment Condition.
- Shelter State.
- NPC State.
- Event State.
- Location State.
- Route State.
- Resource Availability.
- Intel.
- World State.

Hậu quả có thể xuất hiện ngay hoặc ở giai đoạn sau.

---

# 12. Giai đoạn 8 — Nghỉ và ngủ

## Nghỉ ngắn

- Hồi Stamina.
- Giảm nhẹ Fatigue.
- World Clock tiếp tục chạy.
- Không mô phỏng nhanh thời gian.

## Ngủ

- Là ngoại lệ duy nhất cho phép mô phỏng thời gian nhanh.
- World State tiếp tục được cập nhật.
- Event có thể đánh thức người chơi.
- Resource tiếp tục được tiêu thụ.
- Passive Task tiếp tục hoạt động.

Ngủ là quyết định chiến lược vì người chơi từ bỏ khả năng phản ứng trong một khoảng thời gian.

---

# 13. Moment-to-Moment Loop

Trong hoạt động trực tiếp, người chơi lặp lại:

```text
Quan sát
↓
Di chuyển
↓
Phát hiện nguy hiểm hoặc cơ hội
↓
Tương tác
↓
Đánh giá kết quả
↓
Tiếp tục hoặc rút lui
```

Moment-to-Moment Loop phải:

- Có phản hồi rõ.
- Dùng World Clock.
- Cho phép gián đoạn.
- Không tự động hoàn thành hành động dài.

---

# 14. Day Loop

```text
World Update
↓
Đánh giá nhu cầu
↓
Chọn ưu tiên
↓
Exploration hoặc Shelter Work
↓
Xử lý Event
↓
Phân bổ Resource
↓
Nghỉ hoặc ngủ
↓
Disaster tiến triển
```

Mỗi ngày phải tạo ít nhất một thay đổi đáng kể trong:

- Hazard.
- Route.
- Location.
- Shelter.
- Event.
- Resource Demand.
- Information.

---

# 15. Chapter Loop

```text
Normal
↓
Warning
↓
Escalation
↓
Peak
↓
Aftermath
↓
Outcome
```

## Normal

- Thiết lập thế giới.
- Giới thiệu Shelter, NPC và Location.
- Cung cấp trạng thái ổn định ban đầu.

## Warning

- Thu thập Resource.
- Thu thập Intel.
- Xác định điểm yếu.
- Chọn hướng chuẩn bị.

## Escalation

- Hazard tăng.
- Route thay đổi.
- Resource khó tiếp cận.
- Sai lầm bắt đầu tạo hậu quả.

## Peak

- Kiểm tra Shelter Preparation.
- Hạn chế khả năng sửa sai.
- Buộc người chơi ưu tiên hệ thống sống còn.

## Aftermath

- Đánh giá thiệt hại.
- Cập nhật Persistent World State.
- Xác định Chapter Outcome.
- Chuẩn bị chuyển sang Chapter tiếp theo.

---

# 16. Recovery Loop

Phần lớn thất bại cục bộ phải cho phép người chơi tiếp tục.

```text
Mất Resource hoặc Module
↓
Đánh giá thiệt hại
↓
Tìm phương án thay thế
↓
Giảm mục tiêu
↓
Chấp nhận rủi ro hoặc Outcome thấp hơn
```

Phương án phục hồi có thể gồm:

- Dùng Resource thay thế.
- Bỏ mục tiêu tùy chọn.
- Chuyển Shelter Strategy.
- Di dời.
- Nhờ NPC hỗ trợ.
- Chấp nhận mất một Zone.
- Giảm mức dự trữ.

Recovery Loop không được xóa hậu quả đã xảy ra.

---

# 17. Multiplayer Loop tương lai

Trong Multiplayer, các vòng lặp có thể diễn ra song song:

```text
Player A
Exploration

Player B
Shelter Work

Player C
Event Response
```

Nguyên tắc:

- Tất cả dùng chung World Clock.
- World State và Shelter State được chia sẻ.
- Mỗi người có Player Condition và Inventory riêng.
- Một nhóm có thể chia vai trò.
- Việc phối hợp không được loại bỏ Resource Trade-off.
- Ngủ nhanh chỉ xảy ra khi toàn bộ người chơi đủ điều kiện.

---

# 18. Input và Output của Core Loop

| Giai đoạn    | Input chính                | Output chính                   |
| ------------ | -------------------------- | ------------------------------ |
| Đánh giá     | World State, Intel         | Nhu cầu ưu tiên                |
| Lập kế hoạch | Mục tiêu, thời gian, Route | Kế hoạch hành động             |
| Chuẩn bị     | Equipment, Inventory       | Loadout                        |
| Exploration  | Route, Hazard, Condition   | Resource, Intel, Consequence   |
| Shelter Work | Resource, Module, Time     | Shelter Progress               |
| Recovery     | Damage, Failure            | Phương án thay thế             |
| Sleep        | Shelter Safety, Fatigue    | Time Progression, World Update |
| Outcome      | Persistent State           | Chapter Result                 |

---

# 19. Core Loop Validation

Core Loop đạt yêu cầu khi:

1. Exploration và Shelter Work cùng cạnh tranh World Time.
2. Resource thu được ngoài thế giới có mục đích sử dụng rõ tại Shelter.
3. Information thay đổi quyết định.
4. Người chơi thường xuyên phải chọn tiếp tục hoặc quay về.
5. Peak phản ánh trạng thái được tạo từ các vòng lặp trước.
6. Thất bại cục bộ mở Recovery Loop thay vì luôn gây Game Over.
7. World State thay đổi ngay cả khi người chơi không tương tác trực tiếp.
8. Không có hoạt động cốt lõi tồn tại độc lập khỏi các hệ thống khác.

---

# 20. Quyết định chốt

- Core Loop bắt đầu bằng đánh giá World State và kết thúc bằng World State mới.
- Exploration và Shelter Work là hai nhánh hoạt động chính.
- Mọi hoạt động quan trọng sử dụng World Clock.
- Resource và Information là output chính của Exploration.
- Shelter Progress là kết quả chính của việc xử lý Resource.
- Peak Phase kiểm tra trạng thái được tạo ra từ toàn bộ Chapter Loop.
- Recovery Loop cho phép thích ứng sau thất bại cục bộ.
- Multiplayer cho phép các vòng lặp diễn ra song song nhưng dùng chung World State.
- Core Loop không phụ thuộc vào nội dung của một Disaster cụ thể.
