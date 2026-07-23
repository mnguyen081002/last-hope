# Core Design Scope

## 1. Mục đích

Tài liệu này xác định phạm vi của **Core Design** trong Last Hope.

Mục tiêu là:

- Giữ các quyết định cốt lõi nhất quán giữa các Chapter.
- Tránh đưa nội dung riêng của MVP vào hệ thống chung.
- Tránh lặp nội dung giữa `01-core-design` và `02-core-systems`.
- Xác định những quy tắc không được phá khi mở rộng game.

---

# 2. Định nghĩa Core Design

Core Design mô tả:

- Trải nghiệm nền tảng của người chơi.
- Cấu trúc gameplay chính.
- Các loại quyết định cốt lõi.
- Quan hệ giữa Exploration, Resource, Shelter, Information và Consequence.
- Các quy tắc dùng chung cho toàn bộ Campaign.

Core Design không mô tả chi tiết cách từng hệ thống được tính toán hoặc triển khai.

---

# 3. Nội dung thuộc Core Design

Core Design bao gồm:

- Game Design Pillars.
- Player Experience Goals.
- Core Gameplay Loop.
- Expedition Loop.
- Shelter Loop.
- Day Loop.
- Chapter Loop.
- Recovery Loop.
- Core Design Invariants.
- Ranh giới trách nhiệm giữa các nhóm tài liệu.

Các nội dung này phải áp dụng được cho mọi Disaster Chapter.

---

# 4. Nội dung không thuộc Core Design

## 4.1. Core Systems

Các nội dung sau thuộc `02-core-systems`:

- Công thức.
- Trạng thái hệ thống.
- Điều kiện kích hoạt.
- Data Model.
- Quy tắc cập nhật.
- Tương tác chi tiết giữa các hệ thống.

Ví dụ:

```text
Core Design:
Mọi hành động quan trọng đều có chi phí thời gian.

Time System:
World Clock vận hành và cập nhật hành động như thế nào.
```

---

## 4.2. MVP Content

Các nội dung sau thuộc `03-mvp-black-rain`:

- Disaster Timeline cụ thể.
- Location cụ thể.
- Route cụ thể.
- NPC cụ thể.
- Event cụ thể.
- Shelter Layout.
- Module sử dụng trong MVP.
- Resource Budget.
- Hazard của Siêu Bão Mưa Đen.
- Điều kiện Outcome cụ thể.

Core Design không được hard-code nội dung của Mưa Đen.

---

## 4.3. Campaign Content

Các nội dung sau thuộc `04-campaign`:

- Cấu trúc xuyên Chapter.
- Chapter Transition.
- Persistent World State dài hạn.
- Narrative Arc.
- Disaster Escalation.
- Quy tắc chuyển Resource và Progression giữa Chapter.

---

## 4.4. Technical Design

Các nội dung sau không thuộc Core Design:

- Kiến trúc code.
- Networking.
- Database.
- Save File Format.
- Replication.
- AI Implementation.
- Performance Optimization.
- Engine-specific Design.

---

# 5. Core Design Invariants

Core Invariant là quy tắc nền tảng không được thay đổi trong một Chapter riêng lẻ.

## 5.1. Một World Clock

Toàn bộ thế giới sử dụng một World Clock.

Không tồn tại đồng hồ riêng cho từng người chơi.

---

## 5.2. World Clock luôn chạy

Trong gameplay thông thường:

- Không Pause World Clock.
- Không Fast Forward.
- Không Time Acceleration.
- Không Time Skip.

Giấc ngủ là ngoại lệ duy nhất.

---

## 5.3. Mọi Expedition đều có chi phí

Mỗi chuyến đi phải tiêu tốn ít nhất một phần của:

- Thời gian.
- Player Condition.
- Equipment.
- Carry Capacity.
- Resource.
- Shelter Opportunity.

---

## 5.4. Exploration và Shelter cạnh tranh thời gian

Thời gian dành cho Exploration không thể đồng thời dùng cho:

- Build.
- Repair.
- Treatment.
- Processing.
- Rest.
- Shelter Maintenance.

Ở lại Shelter phải là một lựa chọn gameplay hợp lệ.

---

## 5.5. Information không hoàn chỉnh

Người chơi không biết toàn bộ World State.

Information phải có:

- Nguồn.
- Thời điểm.
- Confidence.
- Khả năng lỗi thời.

---

## 5.6. Resource phải tạo Trade-off

Resource quan trọng phải có nhiều công dụng và nguồn cung giới hạn.

Người chơi không thể tối đa hóa mọi nhu cầu trong một lượt chơi.

---

## 5.7. Không Loot Respawn tùy ý

Loot thông thường không tự hồi sinh.

Resource mới chỉ xuất hiện khi có nguyên nhân hợp lý trong World State.

---

## 5.8. Shelter không an toàn tuyệt đối

Shelter có thể:

- Hư hỏng.
- Thiếu Resource.
- Mất Module.
- Mất Zone.
- Bị Hazard ảnh hưởng.
- Buộc phải sơ tán.

---

## 5.9. Peak kiểm tra quá trình chuẩn bị

Peak Phase phải phản ánh:

- Resource đã thu thập.
- Module đã xây.
- Information đã có.
- NPC đã cứu.
- Shelter State.
- Quyết định trước đó.

Peak không được vô hiệu hóa quá trình chuẩn bị bằng một cơ chế tách biệt.

---

## 5.10. Persistent Consequences

Các quyết định quan trọng phải thay đổi ít nhất một trạng thái:

- Player State.
- NPC State.
- Shelter State.
- Location State.
- Route State.
- Event State.
- Resource State.
- Campaign State.

---

## 5.11. Partial Failure phải tồn tại

Phần lớn sai lầm không được gây Game Over ngay lập tức.

Game phải cho phép:

```text
Tổn thất
↓
Điều chỉnh kế hoạch
↓
Phương án thay thế
↓
Outcome khác
```

---

## 5.12. Campaign tuyến tính, chiến lược phi tuyến

Thứ tự Chapter được xác định trước.

Trong mỗi Chapter, người chơi phải có quyền chọn:

- Location.
- Route.
- Shelter Strategy.
- Resource Priority.
- NPC Priority.
- Event Response.
- Relocation.

---

## 5.13. Multiplayer Compatibility

MVP có thể là Single-player nhưng Core Design không được phụ thuộc vào:

- Đồng hồ riêng của người chơi.
- Pause cá nhân.
- Time Skip cá nhân.
- World State cục bộ không thể đồng bộ.
- Hành động khóa toàn bộ thế giới.

---

# 6. Trách nhiệm của từng nhóm tài liệu

| Nhóm tài liệu         | Trách nhiệm                             |
| --------------------- | --------------------------------------- |
| `00-project-overview` | Định nghĩa sản phẩm và Campaign         |
| `01-core-design`      | Định nghĩa trải nghiệm và Gameplay Loop |
| `02-core-systems`     | Định nghĩa quy tắc vận hành             |
| `03-mvp-black-rain`   | Áp dụng Core vào MVP                    |
| `04-campaign`         | Định nghĩa tiến trình xuyên Chapter     |
| `05-future-chapters`  | Lưu concept chưa được chốt              |

---

# 7. Quy tắc mở rộng Core System

Một Core System mới chỉ nên được thêm khi:

1. Phục vụ ít nhất một Game Design Pillar.
2. Tạo quyết định mới.
3. Không trùng trách nhiệm với hệ thống hiện có.
4. Có ảnh hưởng rõ đến Core Gameplay Loop.
5. Có thể dùng lại trong nhiều Chapter.
6. Không chỉ giải quyết một vấn đề nội dung cục bộ.
7. Không làm tăng độ phức tạp mà không tăng giá trị gameplay.

Nếu chỉ phục vụ một Disaster cụ thể, nó nên nằm trong tài liệu Chapter.

---

# 8. Quy tắc thêm nội dung Chapter

Location, Event, NPC, Resource hoặc Module mới phải:

- Phục vụ ít nhất một Pillar.
- Xuất hiện rõ trong Core Gameplay Loop.
- Có chi phí hoặc giới hạn.
- Tạo thay đổi trạng thái.
- Không phụ thuộc vào Loot Respawn.
- Không phá Core Invariant.
- Có vai trò khác với nội dung đã tồn tại.

---

# 9. Quy tắc thay đổi Core Design

Chỉ thay đổi Core Design khi:

- Prototype chứng minh Core Loop không hoạt động.
- Một Pillar không tạo được trải nghiệm mục tiêu.
- Hai hệ thống có trách nhiệm chồng chéo.
- Một Core Invariant gây cản trở toàn bộ Campaign.
- Multiplayer tương lai yêu cầu thay đổi kiến trúc nền tảng.

Không thay đổi Core Design chỉ để:

- Một Location dễ thiết kế hơn.
- Một Event hoạt động thuận tiện hơn.
- Một Chapter có thêm nội dung.
- Giải quyết tạm thời vấn đề cân bằng.

Mọi thay đổi Core phải kiểm tra ảnh hưởng đến:

- Game Design Pillars.
- Player Experience Goals.
- Core Gameplay Loop.
- Core Systems.
- MVP Scope.
- Campaign Structure.

---

# 10. Prototype Baseline

Các giá trị sau chưa phải quyết định cân bằng cuối cùng:

- Tỷ lệ World Time.
- Thời lượng Expedition.
- Sức chứa Inventory.
- Resource Consumption Rate.
- Hazard Exposure Rate.
- Build Duration.
- Event Frequency.
- Số lượng Location.
- Số lượng Resource.
- Peak Duration.

Các giá trị này phải được kiểm chứng trong prototype và tài liệu Balance.

---

# 11. Câu hỏi còn mở

Các câu hỏi cần prototype kiểm chứng:

## 11.1. Expedition Duration

Một Expedition nên kéo dài bao lâu để tạo căng thẳng nhưng không gây mệt mỏi?

## 11.2. Shelter Workload

Bao nhiêu Shelter Task là đủ để ở lại Shelter trở thành lựa chọn có giá trị?

## 11.3. Resource Scarcity

Mức thiếu hụt nào tạo Trade-off mà không làm một sai lầm nhỏ phá hỏng lượt chơi?

## 11.4. Information Uncertainty

Mức độ thông tin lỗi thời hoặc không đầy đủ nào tạo quyết định thay vì gây khó chịu?

## 11.5. Peak Pressure

Peak Phase nên hạn chế Exploration đến mức nào?

## 11.6. Partial Failure

Mức tổn thất nào đủ nghiêm trọng nhưng vẫn cho phép Recovery Loop?

Các câu hỏi này không thay đổi Core Invariant.

---

# 12. Tiêu chí hoàn thiện Core Design

Core Design được xem là hoàn thiện khi:

1. Các Pillar có phạm vi rõ.
2. Player Experience Goals có thể kiểm chứng bằng playtest.
3. Core Gameplay Loop kết nối Exploration và Shelter.
4. Core Invariants không mâu thuẫn.
5. Mỗi Core System có trách nhiệm riêng.
6. Nội dung Disaster cụ thể không bị hard-code vào Core.
7. Partial Failure và Recovery Loop được hỗ trợ.
8. Peak Phase phản ánh quá trình chuẩn bị.
9. Kiến trúc không ngăn Multiplayer tương lai.
10. Các thông số chưa chốt đều có kế hoạch prototype.

---

# 13. Quyết định chốt

- Core Design định nghĩa trải nghiệm và cấu trúc gameplay, không định nghĩa chi tiết hệ thống.
- Core Design phải áp dụng được cho mọi Disaster Chapter.
- Nội dung cụ thể của Siêu Bão Mưa Đen không thuộc Core Design.
- Một World Clock, Resource Trade-off, Shelter Preparation và Persistent Consequences là các Core Invariant.
- Peak Phase phải kiểm tra quá trình chuẩn bị.
- Phần lớn thất bại phải dẫn tới Recovery Loop thay vì Game Over tức thời.
- Multiplayer Compatibility là yêu cầu kiến trúc, không phải phạm vi triển khai MVP.
- Thông số cân bằng chỉ được chốt sau prototype.
