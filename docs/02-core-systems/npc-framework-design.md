# NPC Framework Design

## 1. Mục tiêu

NPC Framework xác định cách NPC:

- Tồn tại trong thế giới.
- Tiêu thụ tài nguyên.
- Cung cấp thông tin và kỹ năng.
- Tham gia Shelter Task.
- Phản ứng với quyết định.
- Tạo hậu quả dài hạn.

---

## 2. Nguyên tắc thiết kế

- NPC không phải phần thưởng miễn phí.
- Mỗi NPC phải có giá trị và chi phí.
- NPC có trạng thái riêng.
- NPC không cần mô phỏng đầy đủ khi ngoài màn hình.
- Quyết định liên quan NPC phải ảnh hưởng World State.
- MVP ưu tiên số lượng ít nhưng có vai trò rõ.

---

## 3. NPC Data

```text
npc_id
current_location
health_state
condition
fatigue
hunger
thirst
skills
traits
trust
loyalty
current_task
relationship_flags
availability
```

---

## 4. NPC Role

NPC có thể cung cấp:

- Lao động.
- Skill.
- Intel.
- Event.
- Shelter Site.
- Recipe.
- Quan hệ với nhóm khác.

NPC không nhất thiết phải gia nhập Shelter để có giá trị.

---

## 5. Survivor State

```text
Unknown
Located
In Danger
Rescued
Sheltered
Missing
Departed
Dead
```

Trạng thái được lưu lâu dài.

---

## 6. Recruitment

NPC có thể gia nhập khi:

- Được cứu.
- Được cung cấp tài nguyên.
- Trust đủ cao.
- Shelter có Capacity.
- Người chơi hoàn thành điều kiện riêng.

Người chơi có thể từ chối tiếp nhận.

---

## 7. NPC Cost

Mỗi NPC tiêu thụ:

- Thức ăn.
- Nước.
- Living Capacity.
- Không gian ngủ.
- Vật tư điều trị.
- Thời gian quản lý.

NPC bị thương hoặc phụ thuộc có chi phí cao hơn.

---

## 8. NPC Skill

Các Skill chính:

```text
Construction
Medical
Electronics
Navigation
Water Processing
Scavenging
```

Skill ảnh hưởng:

- Task Duration.
- Resource Efficiency.
- Event Option.
- Information Quality.
- Failure Risk.

---

## 9. NPC Task

NPC có thể thực hiện:

- Active Task.
- Passive Monitoring.
- Shelter Duty.
- Expedition Support.

NPC không thể làm nhiều Task cùng lúc.

Task làm tăng:

- Fatigue.
- Hunger.
- Thirst.
- Injury Risk.

---

## 10. NPC Autonomy

NPC có thể:

- Từ chối Task nguy hiểm.
- Yêu cầu nghỉ.
- Rời Shelter.
- Tự phản ứng với Critical Event.
- Cứu người khác nếu phù hợp Trait.

Autonomy phụ thuộc vào:

- Trust.
- Loyalty.
- Condition.
- Trait.
- Mức nguy hiểm.

---

## 11. Trust

Trust thay đổi khi:

- Giữ lời hứa.
- Chia tài nguyên.
- Cứu NPC.
- Bỏ mặc NPC.
- Phân công công việc nguy hiểm.
- Bảo vệ người thân của NPC.

Trust mở hoặc khóa:

- Intel.
- Recruitment.
- Task.
- Event.
- Ending.

---

## 12. Conflict

Conflict có thể xuất hiện khi:

- Tài nguyên thiếu.
- Shelter quá tải.
- Quyết định gây bất đồng.
- NPC bị ưu tiên không công bằng.
- Trust thấp.

MVP không cần hệ thống hội thoại xung đột lớn.

Conflict có thể thể hiện qua:

- Task efficiency giảm.
- NPC từ chối.
- NPC rời đi.
- Event ngắn.

---

## 13. NPC Expedition

NPC có thể tham gia chuyến đi.

Vai trò:

- Mang đồ.
- Hỗ trợ vượt Hazard.
- Điều trị.
- Mở lựa chọn Skill.
- Cứu người bị Incapacitated.

NPC không được tự động loot toàn Location ngoài màn hình trong MVP.

---

## 14. Off-screen NPC

NPC ngoài màn hình được cập nhật theo mốc World Clock.

Mô phỏng tối thiểu:

```text
location
task
condition
resource_consumption
event_exposure
```

Không mô phỏng di chuyển từng bước khi người chơi không quan sát.

---

## 15. NPC Death

NPC có thể tử vong do:

- Event.
- Hazard.
- Không điều trị.
- Bị bỏ lại.
- Forced Evacuation.
- Expedition Failure.

NPC Death phải tạo Persistent Flag.

---

## 16. Multiplayer Rule

- NPC thuộc World State chung.
- Task assignment được đồng bộ.
- Không một người chơi sở hữu riêng NPC.
- Relationship chính được tính với nhóm.
- Một số Personal Flag có thể tồn tại cho hội thoại.

---

## 17. Phạm vi MVP

Triển khai:

- 4–6 NPC quan trọng.
- Skill.
- Trait.
- Trust đơn giản.
- Recruitment.
- Shelter Task.
- Expedition Support.
- Resource Consumption.
- Missing, Departed và Dead State.
- Event liên quan NPC.

Chưa triển khai:

- Hội thoại phân nhánh lớn.
- Romance.
- Lịch sinh hoạt chi tiết.
- Faction AI phức tạp.
- NPC tự xây dựng chiến lược.
- Hệ thống nhu cầu tâm lý chuyên sâu.

---

## 18. Quyết định chốt

- NPC tạo cả lợi ích và chi phí.
- NPC có Skill, Trait, Condition và Trust.
- NPC Task sử dụng World Clock.
- NPC có thể từ chối hoặc rời đi.
- NPC State tồn tại lâu dài.
- MVP sử dụng ít NPC có vai trò rõ.
- Off-screen NPC dùng mô phỏng theo trạng thái.

---
