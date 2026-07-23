# Last Hope — Design Principles

## 1. Every Expedition Has a Cost

Mọi chuyến đi phải tiêu tốn nhiều hơn thời gian di chuyển.

Chi phí có thể gồm:

- World Time.
- Stamina.
- Fatigue.
- Hunger và Thirst.
- Equipment Durability.
- Carry Capacity.
- Exposure.
- Công việc Shelter bị bỏ lỡ.
- Event có thể hết hạn.

Không tồn tại chuyến đi hoàn toàn miễn phí.

---

## 2. Preparation Wins, Not Luck

Thành công phải chủ yếu đến từ:

- Thông tin.
- Kế hoạch.
- Equipment.
- Shelter Preparation.
- Resource Allocation.
- Route Selection.
- Thời điểm hành động.

RNG chỉ tạo biến thể, không quyết định thắng thua chính.

Game không được phá hủy một kế hoạch hợp lý bằng kết quả ngẫu nhiên không có cảnh báo.

---

## 3. Information Is a Resource

Thông tin phải có:

- Nguồn.
- Thời điểm quan sát.
- Confidence.
- Thời hạn sử dụng.
- Chi phí thu thập.

Thông tin giúp người chơi:

- Chọn Route.
- Tránh Hazard.
- Phát hiện Event.
- Ưu tiên Location.
- Chuẩn bị Shelter.
- Quyết định sơ tán.

World Map không hiển thị toàn bộ trạng thái thực của thế giới.

---

## 4. Every Important Resource Has Multiple Uses

Tài nguyên quan trọng phải cạnh tranh giữa nhiều nhu cầu.

Ví dụ:

```text
Battery
→
Flashlight
OR
Radio
OR
Communication Station
OR
Emergency Lighting
```

Mỗi tài nguyên cốt lõi nên có:

- Ít nhất hai nguồn hợp lý.
- Ít nhất hai công dụng quan trọng.

Không tạo tài nguyên chỉ dùng cho một mục tiêu bắt buộc duy nhất nếu không có lý do thiết kế rõ ràng.

---

## 5. Disaster Is the Final Exam

Warning và Escalation là giai đoạn người chơi chuẩn bị.

Peak Phase là giai đoạn kiểm tra:

- Module đã xây.
- Tài nguyên đã tích trữ.
- NPC đã cứu.
- Thông tin đã thu thập.
- Route đã mở.
- Shelter Site đã khảo sát.
- Các quyết định trước đó.

Peak không nên giới thiệu hệ thống cốt lõi mới.

---

## 6. Time Is Always Moving

Toàn bộ game sử dụng một World Clock.

Trong gameplay bình thường:

- Không Pause World Clock.
- Không Fast Forward.
- Không Time Acceleration.
- Không Time Skip.
- Không cộng trực tiếp thời gian vào World Clock.

Giấc ngủ là ngoại lệ duy nhất cho phép mô phỏng và chuyển World Clock tới thời điểm thức dậy.

---

## 7. The World Does Not Wait

Thế giới tiếp tục thay đổi khi người chơi không có mặt.

- Event tiến triển.
- NPC di chuyển.
- Route bị khóa.
- Hazard tăng.
- Location thay đổi.
- Shelter tiêu thụ tài nguyên.
- Passive Task tiếp tục hoạt động.

Người chơi không phải trung tâm của mọi hoạt động trong thế giới.

---

## 8. No Arbitrary Loot Respawn

Loot thông thường không tự hồi sinh.

Một Location đã được tìm kiếm phải giữ trạng thái depletion.

Tài nguyên mới chỉ xuất hiện khi có nguyên nhân trong thế giới:

- Xe cứu trợ gặp nạn.
- Công trình sập làm lộ kho.
- NPC bỏ lại tài nguyên.
- Một Zone mới được mở.
- Vật phẩm bị nước cuốn tới.
- Hạ tầng được khôi phục.

Quay lại Location phải có lý do ngoài loot respawn.

---

## 9. Revisit Through Change, Not Reset

Location có thể đáng quay lại khi:

- Có Tool mới.
- Có Intel mới.
- Route mới mở.
- Disaster thay đổi lối vào.
- NPC hoặc Event xuất hiện.
- Một Zone mới có thể tiếp cận.
- Location có chức năng chiến lược mới.

Location State phải tồn tại lâu dài.

---

## 10. Shelter Is a Strategic Space

Shelter không phải menu hoặc vùng an toàn tuyệt đối.

Shelter có:

```text
Fixed Core Components
+
Predefined Zones
+
Buildable Slots / Areas
```

Người chơi:

- Không xây dựng hoàn toàn tự do.
- Không di chuyển hoặc tháo dỡ Core Component.
- Chỉ đặt Module trong Zone hợp lệ.
- Phải bảo trì, vận hành và bảo vệ hệ thống.

Mọi Module đều có:

- Build Cost.
- Build Time.
- Operating Cost.
- Maintenance Cost.
- Failure State.

---

## 11. Staying Home Must Be a Real Choice

Ở Shelter không được là hành động bỏ lượt.

Người chơi có thể:

- Xây dựng.
- Sửa chữa.
- Xử lý nước.
- Làm khô Equipment.
- Điều trị.
- Phân tích Intel.
- Sắp xếp Storage.
- Vận hành thiết bị.
- Phân công NPC.
- Nghỉ ngơi.

Exploration và Shelter Work phải cạnh tranh trực tiếp về thời gian.

---

## 12. Real-time Action, Persistent Progress

Search, Build, Repair và Treatment diễn ra trong World Clock.

Các hành động lớn:

- Có tiến độ.
- Có thể bị gián đoạn.
- Lưu trạng thái.
- Không tự hoàn thành bằng Time Skip.

Passive Machine có thể tiếp tục hoạt động khi người chơi rời đi.

---

## 13. Hazard Changes Decisions

Hazard không chỉ gây sát thương.

Hazard phải ảnh hưởng ít nhất một yếu tố:

- Route.
- Movement.
- Equipment.
- Carry Load.
- Time.
- Search.
- Shelter.
- Item Condition.
- Information.
- Event.

Protection giảm rủi ro nhưng không loại bỏ hoàn toàn Hazard.

---

## 14. Consequences Escalate Gradually

Hậu quả phải tiến triển theo nhiều mức:

```text
Safe
↓
Warning
↓
Impaired
↓
Dangerous
↓
Critical
```

Người chơi phải có:

- Dấu hiệu cảnh báo.
- Thời gian phản ứng.
- Phương án giảm thiệt hại.

Instant Failure chỉ hợp lệ khi nguy hiểm đã được cảnh báo rõ.

---

## 15. Failure Must Be Understandable

Thất bại phải nêu nguyên nhân chính.

Không chỉ hiển thị:

```text
Game Over
```

Game phải giải thích:

- Hệ thống nào đã thất bại.
- Tài nguyên nào đã thiếu.
- Event hoặc Hazard nào tạo ra hậu quả.
- Người chơi còn có thể làm gì khác trước đó.

Không cần luôn tái hiện toàn bộ chuỗi nguyên nhân nếu điều đó làm báo cáo khó đọc.

---

## 16. Partial Failure Is Valid

Không phải mọi sai lầm đều kết thúc game.

Hậu quả có thể gồm:

- Mất tầng Shelter.
- Mất Storage.
- NPC tử vong hoặc rời đi.
- Route bị khóa.
- Resource bị ô nhiễm.
- Forced Evacuation.
- Outcome thấp hơn.
- Chapter tiếp theo khó hơn.

Game Over chỉ xảy ra khi không còn phương án sinh tồn hợp lý.

---

## 17. The Player Cannot Save Everything

Người chơi không thể đồng thời:

- Thu thập mọi tài nguyên.
- Cứu mọi NPC.
- Xây mọi Module.
- Khám phá mọi Location.
- Hoàn thành mọi Event.
- Bảo vệ toàn bộ Storage.

Các lựa chọn phải tạo sự khác biệt rõ trong Outcome và World State.

---

## 18. Progression Unlocks Options

Progression ưu tiên mở khả năng mới hơn tăng chỉ số.

Unlock phải có nguyên nhân hợp lý:

```text
Knowledge
+
Blueprint
+
Tool
+
Workstation
=
New Capability
```

Không sử dụng Character Level làm điều kiện mở khả năng kỹ thuật không liên quan.

---

## 19. Campaign Story Is Linear, Strategy Is Not

Thứ tự Disaster Chapter được xác định trước.

Trong mỗi Chapter, người chơi tự quyết định:

- Hướng khám phá.
- Thứ tự Location.
- Shelter Strategy.
- NPC Priority.
- Resource Allocation.
- Event Response.
- Relocation.

Không tồn tại một lộ trình tối ưu duy nhất.

---

## 20. Multiplayer Compatibility Is Architectural

MVP có thể là Single-player nhưng Core System phải tránh thiết kế cần làm lại hoàn toàn khi thêm Multiplayer.

Nguyên tắc:

- Một World Clock.
- World State chung.
- Shelter State chung.
- Event State chung.
- Player Condition riêng.
- Inventory riêng.
- Exposure riêng.
- Shared Intel và Personal Observation.
- Không Pause World Clock khi mở Inventory.
- Chỉ mô phỏng giấc ngủ nhanh khi toàn bộ người chơi đủ điều kiện.

MVP không cần triển khai networking.

---

## 21. Scope Before Scale

Ưu tiên:

- Bản đồ nhỏ nhưng có nhiều thay đổi.
- Ít Location nhưng có Return Hook.
- Ít NPC nhưng có vai trò rõ.
- Ít Module nhưng cạnh tranh tài nguyên.
- Ít Hazard nhưng có tương tác rõ.
- Ít Event nhưng có hậu quả lâu dài.

Không mở rộng số lượng nội dung trước khi Core Loop được prototype và kiểm chứng.

---

## 22. Tiêu chuẩn đánh giá hệ thống

Một hệ thống chỉ nên được giữ lại khi đáp ứng ít nhất một điều kiện:

- Tạo quyết định mới.
- Làm quyết định hiện có rõ hơn.
- Tạo hậu quả có thể hiểu.
- Tăng giá trị của thông tin.
- Tăng giá trị của chuẩn bị.
- Tạo tương tác giữa các hệ thống.
- Hỗ trợ Narrative hoặc Persistent World State.

Không giữ hệ thống chỉ vì tăng mức độ mô phỏng.
