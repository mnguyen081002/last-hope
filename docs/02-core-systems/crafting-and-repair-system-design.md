# Crafting and Repair System Design

## 1. Mục tiêu

Hệ thống xác định cách người chơi:

- Chế tạo vật phẩm.
- Sửa Tool và Equipment.
- Gia công Material.
- Tháo dỡ vật phẩm.
- Quản lý Recipe và Workstation.

---

## 2. Nguyên tắc thiết kế

- Crafting giải quyết vấn đề, không tạo loot vô hạn.
- Recipe cần nguồn gốc hợp lý.
- Crafting diễn ra trong World Clock.
- Công việc có thể bị gián đoạn.
- Tool và Workstation ảnh hưởng kết quả.
- Repair không khôi phục vô hạn.

---

## 3. Recipe

Mỗi Recipe có:

```text
recipe_id
output
materials
required_tools
required_workstation
required_skill
work_duration
stages
failure_conditions
```

---

## 4. Recipe Source

Recipe được mở từ:

- Knowledge ban đầu.
- Blueprint.
- NPC.
- Tháo thiết bị.
- Tài liệu.
- Event.

Không mở Recipe bằng Character Level.

---

## 5. Crafting Type

### Hand Crafting

- Không cần Workstation.
- Recipe đơn giản.
- Hiệu quả thấp.
- Có thể thực hiện tại nhiều vị trí.

### Workstation Crafting

- Cần Workshop hoặc Module.
- Cho phép item phức tạp.
- Có thể cần Power.
- Hiệu quả cao hơn.

### Machine Processing

- Passive Task.
- Ví dụ lọc nước, sạc pin.
- Tiếp tục khi người chơi rời đi.
- Có Operating Cost.

---

## 6. Crafting Phase

```text
Prepare
Process
Assemble
Test
Complete
```

Recipe đơn giản có thể chỉ dùng một giai đoạn.

---

## 7. Interrupted Crafting

Khi bị gián đoạn:

- Tiến độ được lưu nếu giai đoạn cho phép.
- Material đã tiêu thụ không tự hoàn lại.
- Item chưa hoàn thành có thể chiếm không gian.
- Hazard có thể làm hỏng công việc.

---

## 8. Material Quality

MVP sử dụng Condition thay vì nhiều cấp chất lượng nguyên liệu.

Material bị:

- Wet.
- Contaminated.
- Damaged.

có thể:

- Không dùng được.
- Cần xử lý.
- Làm Output có Condition thấp.
- Tăng thời gian Craft.

---

## 9. Repair

Repair yêu cầu:

```text
target_item
repair_material
required_tool
required_skill
repair_duration
repair_limit
```

Repair khôi phục Durability nhưng không luôn về tối đa.

---

## 10. Repair Limit

Mỗi lần Repair có thể làm giảm Maximum Durability.

Ví dụ:

```text
current_max_durability
→
current_max_durability - repair_degradation
```

Điều này ngăn một vật phẩm được sửa vô hạn.

---

## 11. Repair State

```text
Operational
Worn
Damaged
Broken
Irreparable
```

Broken không nhất thiết Irreparable.

Irreparable chỉ xuất hiện khi:

- Maximum Durability quá thấp.
- Thiếu bộ phận cốt lõi.
- Hazard phá hủy hoàn toàn.
- Repair thất bại nghiêm trọng có cảnh báo.

---

## 12. Salvage

Người chơi có thể tháo vật phẩm để lấy:

- Material.
- Component.
- Spare Part.

Salvage là Active Task.

Kết quả phụ thuộc:

- Item Condition.
- Tool.
- Skill.
- Hazard.
- Thời gian.

Salvage phá hủy item gốc.

---

## 13. Improvised Crafting

Một số Recipe có phiên bản tạm thời.

Ví dụ:

```text
Improvised Flood Barrier
```

Đặc điểm:

- Xây nhanh.
- Vật liệu phổ biến.
- Durability thấp.
- Hiệu quả thấp.
- Phù hợp tình huống khẩn cấp.

Improvised Item không thay thế hoàn toàn item tiêu chuẩn.

---

## 14. Crafting Choice

Recipe phải cạnh tranh tài nguyên.

Ví dụ:

```text
Metal Component
→
Sửa máy bơm
OR
Xây Signal Stabilizer
OR
Sửa Generator
```

Không nên có Material chỉ dùng cho một Recipe quan trọng duy nhất.

---

## 15. Workstation

Mỗi Workstation có:

```text
supported_recipe_tags
condition
power_demand
tool_slots
efficiency
```

Workstation hỏng hoặc mất Power có thể:

- Làm Task dừng.
- Tăng thời gian.
- Giới hạn Recipe.

---

## 16. Crafting Queue

MVP không cần hàng đợi Active Task tự động.

Passive Machine có thể có Queue giới hạn.

Ví dụ:

```text
Water Purifier Queue
Battery Charging Queue
Drying Queue
```

---

## 17. Multiplayer Rule

- Crafting Task là dữ liệu chung nếu dùng Workstation chung.
- Material được khóa khi Task bắt đầu.
- Người khác có thể tiếp tục Task bị dừng.
- Nhiều người có thể hỗ trợ Recipe phù hợp.
- Không nhân đôi Output do đồng bộ mạng.

---

## 18. UI Requirement

UI phải hiển thị:

- Output.
- Material cần.
- Material hiện có.
- Tool.
- Workstation.
- Duration.
- Condition dự kiến.
- Operating Cost.
- Tiến độ.
- Hậu quả khi hủy.

---

## 19. Dữ liệu hệ thống

### Recipe Definition

```text
recipe_id
output_definition
output_quantity
materials
required_tools
required_workstation
required_skill
duration
stages
```

### Crafting Task

```text
crafting_task_id
recipe_id
owner
workstation_id
consumed_materials
current_stage
progress
output_condition
state
```

### Repair Task

```text
repair_task_id
item_instance_id
materials
tool
duration
durability_restore
max_durability_loss
state
```

---

## 20. Phạm vi MVP

Triển khai:

- Hand Crafting.
- Workshop Crafting.
- Passive Machine Processing.
- Recipe Unlock.
- Interrupted Task.
- Repair.
- Repair Limit.
- Salvage.
- Improvised Recipe.
- Multiplayer-compatible Task State.

Chưa triển khai:

- Crafting tree lớn.
- Chất lượng item nhiều cấp.
- Tự động hóa sản xuất phức tạp.
- Dây chuyền sản xuất.
- Recipe ngẫu nhiên.
- Nâng cấp Tool nhiều tầng.

---

## 21. Quyết định chốt

- Crafting diễn ra trong World Clock.
- Recipe được mở bằng Knowledge, Blueprint hoặc NPC.
- Active Crafting cần người thực hiện.
- Machine Processing là Passive Task.
- Repair không thể duy trì item vô hạn.
- Salvage phá hủy item gốc.
- Improvised Recipe hỗ trợ tình huống khẩn cấp.
- Recipe quan trọng phải cạnh tranh tài nguyên.

---

# Core Design Completion Status

Core Design hiện bao gồm:

```text
Gameplay Pillars
Core Loop
Time System
Player Condition
Inventory and Equipment
Resource Flow
Location System
World Map Framework
Shelter Framework
Hazard Framework
Information System
Event System
Win, Lose and Outcome
Progression
Building and Placement
Shelter Site and Relocation
NPC Framework
Crafting and Repair
```

Core Design đã đủ để chuyển sang thiết kế chi tiết MVP Siêu Bão Mưa Đen.

Các con số cụ thể về:

- Thời lượng.
- Chi phí.
- Tốc độ tiêu hao.
- Số lượng tài nguyên.
- Công suất Module.
- Mức Hazard.
- Số Event.
- Số Location.

sẽ được chốt trong tài liệu MVP và Balance Framework, không thuộc Core Framework.
