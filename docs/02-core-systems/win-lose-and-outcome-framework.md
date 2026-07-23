# Win, Lose and Outcome Framework

## 1. Mục tiêu

Framework này xác định:

- Khi nào nhân vật thất bại.
- Khi nào Shelter thất bại.
- Khi nào Chapter kết thúc.
- Cách đánh giá kết quả.
- Cách giải thích nguyên nhân thành công hoặc thất bại.
- Cách hỗ trợ Single-player và Multiplayer.

---

## 2. Nguyên tắc thiết kế

### 2.1. Thất bại phải được giải thích rõ ràng

Game không được chỉ hiển thị:

```text
Game Over
```

Kết quả phải nêu rõ nguyên nhân chính dẫn đến thất bại.

Ví dụ:

```text
Shelter không còn khả năng duy trì cư trú do thiếu nước sạch.
```

---

### 2.2. Không phải mọi sai lầm đều gây thua ngay

Sai lầm có thể tạo:

- Mất tài nguyên.
- Injury.
- NPC rời đi.
- Location bị mất.
- Shelter suy giảm.
- Ending kém hơn.

Game chỉ kết thúc khi khả năng tiếp tục sinh tồn không còn hợp lý.

---

### 2.3. Chapter Outcome không chỉ có thắng hoặc thua

Một Chapter có thể kết thúc ở nhiều mức:

```text
Exceptional Survival
Stable Survival
Barely Survived
Forced Evacuation
Collapse
```

---

## 3. Player Failure

Nhân vật thất bại khi:

```text
health <= 0
```

Hoặc khi bị Incapacitated và không còn phương án cứu hợp lệ.

### Single-player

Game kết thúc nếu:

- Nhân vật duy nhất tử vong.
- Không có NPC hoặc cơ chế cứu hộ khả dụng.
- Không còn nhân vật điều khiển thay thế.

### Multiplayer

Một người chơi tử vong không nhất thiết kết thúc Chapter.

Chapter kết thúc khi:

- Toàn bộ người chơi tử vong.
- Toàn bộ người chơi bị Incapacitated và không thể được cứu.
- Nhóm không còn khả năng duy trì Shelter hoặc sơ tán.

---

## 4. Shelter Failure

Shelter thất bại khi không còn khả năng hỗ trợ sinh tồn.

Các điều kiện chính:

### Structural Collapse

```text
structural_integrity <= 0
```

### Critical Flooding

Toàn bộ Zone cư trú bị ngập hoặc không thể tiếp cận.

### Resource System Collapse

Nhóm không còn khả năng cung cấp nhu cầu sống còn trong khoảng thời gian tối thiểu.

Ví dụ:

- Không còn nguồn nước sử dụng được.
- Không còn phương án xử lý nước.
- Không còn Power cho hệ thống bắt buộc trong Peak Phase.

### Uninhabitable State

Shelter không đáp ứng các điều kiện tối thiểu:

```text
safe_zone_available == false
OR
living_area_accessible == false
```

Shelter Failure không luôn gây Game Over.

Nếu còn thời gian và tuyến đường hợp lệ, người chơi có thể:

- Di dời.
- Sơ tán.
- Chuyển sang Shelter tạm.
- Chấp nhận Outcome thấp hơn.

---

## 5. Forced Evacuation

Forced Evacuation xảy ra khi Shelter hiện tại không còn duy trì được nhưng nhóm vẫn còn khả năng rời đi.

Điều kiện:

- Có Shelter Site hoặc điểm sơ tán đã biết.
- Có tuyến đường khả dụng.
- Có đủ thời gian.
- Có ít nhất một nhân vật còn khả năng di chuyển.

Forced Evacuation gây:

- Mất phần lớn Storage.
- Mất các Module cố định.
- Giảm Chapter Outcome.
- Tăng rủi ro cho Chapter tiếp theo.
- Có thể mất NPC không đủ khả năng di chuyển.

---

## 6. Chapter Completion

Chapter kết thúc khi Disaster chuyển sang trạng thái kết thúc và nhóm đã xử lý giai đoạn cuối.

Ví dụ với Siêu Bão Mưa Đen:

```text
Peak Phase kết thúc
↓
Aftermath bắt đầu
↓
Shelter còn khả năng cư trú hoặc nhóm sơ tán thành công
↓
Chapter Outcome được đánh giá
```

Chapter không kết thúc ngay khi đồng hồ đạt một mốc nếu vẫn còn Event bắt buộc đang diễn ra.

---

## 7. Outcome Dimensions

Kết quả Chapter được đánh giá theo các nhóm sau:

```text
Player Survival
Shelter Condition
Resource Stability
NPC Survival
World Impact
Information Acquired
Persistent Damage
```

---

## 8. Outcome Level

### Exceptional Survival

Điều kiện tổng quát:

- Nhóm sống sót.
- Shelter còn ổn định.
- Nước và nhu yếu phẩm còn dự phòng.
- Phần lớn NPC quan trọng sống sót.
- Thu thập được thông tin cốt truyện chính.
- Không phải di dời khẩn cấp.

### Stable Survival

Điều kiện:

- Nhóm sống sót.
- Shelter vẫn sử dụng được.
- Tài nguyên thấp nhưng có thể phục hồi.
- Một phần mục tiêu phụ thất bại.

### Barely Survived

Điều kiện:

- Nhóm sống sót.
- Shelter bị hư hỏng nặng.
- Tài nguyên gần cạn.
- Có Injury hoặc mất NPC.
- Chapter tiếp theo bắt đầu trong trạng thái bất lợi.

### Forced Evacuation

Điều kiện:

- Nhóm sống sót.
- Shelter bị mất.
- Phải rời khu vực.
- Phần lớn Storage và Module bị bỏ lại.

### Collapse

Điều kiện:

- Toàn bộ nhân vật tử vong.
- Không còn nơi trú hợp lệ.
- Không thể sơ tán.
- Khả năng sinh tồn bị phá vỡ hoàn toàn.

---

## 9. Causal Outcome Report

Cuối Chapter, hệ thống phải hiển thị:

```text
Major Decisions
Major Consequences
Resources Preserved
Resources Lost
NPC Outcome
Shelter Outcome
World State Changes
Chapter Transition Effects
```

Ví dụ:

```text
Bạn giữ máy phát hoạt động cho máy bơm.

Kết quả:
- Lower Floor không bị mất.
- Water Processing Area tiếp tục hoạt động.
- Communication Station bị tắt.
- Bạn bỏ lỡ tín hiệu cứu hộ tại khu phía đông.
```

---

## 10. Hidden Score

Game có thể dùng điểm nội bộ để phân loại Outcome.

Không hiển thị điểm tổng trực tiếp.

Các nhóm điểm:

```text
survival_score
shelter_score
resource_score
npc_score
information_score
world_score
```

Điểm chỉ phục vụ:

- Phân loại Ending.
- Chọn trạng thái Chapter tiếp theo.
- Cân bằng.
- Telemetry.

---

## 11. Multiplayer Outcome

Outcome được tính cho toàn nhóm.

Có thể ghi nhận thêm đóng góp cá nhân:

- Resource delivered.
- NPC rescued.
- Shelter tasks completed.
- Information discovered.
- Teammates rescued.

Đóng góp cá nhân không thay đổi Outcome chung nếu không có tác động thực tế đến World State.

---

## 12. Phạm vi MVP

Triển khai:

- Player Death.
- Incapacitation Failure.
- Shelter Failure.
- Forced Evacuation.
- Chapter Completion.
- Năm Outcome Level.
- Causal Outcome Report.
- Persistent consequence flags.

Chưa triển khai:

- Nhiều ending cốt truyện phức tạp.
- Điểm thành tích trực tuyến.
- Xếp hạng người chơi.
- Hệ thống permadeath campaign hoàn chỉnh.

---

## 13. Quyết định chốt

- Thất bại phải được giải thích rõ ràng.
- Shelter Failure không luôn gây Game Over.
- Forced Evacuation là một Outcome riêng.
- Chapter Outcome có nhiều mức.
- Kết quả dựa trên World State, không chỉ Player Health.
- Multiplayer sử dụng Outcome chung.
- Báo cáo cuối Chapter phải thể hiện quyết định và hậu quả.

---

# Progression Framework Design

## 1. Mục tiêu

Progression Framework xác định cách người chơi phát triển:

- Trong một Chapter.
- Giữa các Chapter.
- Qua trang bị, kiến thức, quan hệ và World State.

Progression phải tạo thêm lựa chọn, không chỉ tăng chỉ số.

---

## 2. Các lớp tiến trình

```text
Immediate Progression
Chapter Progression
Campaign Progression
World Progression
```

---

## 3. Immediate Progression

Tiến trình trong một phiên chơi ngắn.

Ví dụ:

- Tìm được công cụ.
- Mở Zone.
- Hoàn thành Module.
- Khôi phục thiết bị.
- Phát hiện tuyến đường.
- Xác minh thông tin.

Immediate Progression phải tạo lợi ích có thể sử dụng ngay.

---

## 4. Chapter Progression

Tiến trình trong phạm vi một Disaster Chapter.

Bao gồm:

- Shelter phát triển.
- Location được mở.
- Resource Flow thay đổi.
- NPC gia nhập.
- Disaster Intel được cải thiện.
- Các phương án sinh tồn mới xuất hiện.

Chapter Progression có thể bị mất một phần khi Chapter kết thúc.

---

## 5. Campaign Progression

Tiến trình tồn tại qua nhiều Chapter.

Bao gồm:

```text
knowledge
blueprints
relationships
persistent_tools
character_traits
world_flags
shelter_legacy
```

Campaign Progression không được làm Chapter sau mất cân bằng.

---

## 6. Knowledge Progression

Knowledge là tiến trình bền vững nhất.

Ví dụ:

- Biết cách xử lý nước hiệu quả.
- Biết cách gia cố Module.
- Hiểu tín hiệu bất thường.
- Biết cấu trúc một Location.
- Biết đặc điểm một Hazard.

Knowledge có thể mở:

- Recipe.
- Build option.
- Intel interpretation.
- Dialog option.
- Route solution.

---

## 7. Blueprint Progression

Blueprint mở quyền xây hoặc chế tạo.

Blueprint có thể nhận từ:

- Tài liệu.
- NPC.
- Tháo thiết bị.
- Hoàn thành Event.
- Nghiên cứu vật phẩm.

Blueprint không tự cung cấp tài nguyên.

Người chơi vẫn cần:

- Vật liệu.
- Tool.
- Thời gian.
- Workstation.

---

## 8. Character Progression

MVP không dùng skill tree lớn.

Mỗi nhân vật có thể phát triển bằng:

```text
skill proficiency
traits
experience tags
```

Ví dụ:

- Construction.
- Medical.
- Navigation.
- Water Processing.
- Electronics.

Skill tăng hiệu quả, không mở khả năng siêu nhiên.

Tác động có thể gồm:

- Hành động nhanh hơn.
- Giảm hao vật liệu.
- Phát hiện thêm thông tin.
- Giảm rủi ro Injury.
- Sửa chữa hiệu quả hơn.

---

## 9. Trait

Trait là đặc điểm có tác động rõ ràng.

Ví dụ:

```text
Strong Swimmer
Mechanic
Field Medic
Light Sleeper
Hydrologist
```

Trait không nên chỉ tăng phần trăm nhỏ.

Trait phải thay đổi cách giải quyết tình huống.

Ví dụ:

```text
Light Sleeper
→
Phát hiện Shelter Event sớm hơn khi ngủ
```

---

## 10. Relationship Progression

Quan hệ NPC được biểu diễn bằng:

```text
trust
dependency
conflict
loyalty
```

Không cần hiển thị toàn bộ bằng số.

Quan hệ thay đổi do:

- Cứu hoặc bỏ qua NPC.
- Chia tài nguyên.
- Phân công công việc.
- Giữ hoặc phá lời hứa.
- Đưa ra quyết định ảnh hưởng nhóm.

Quan hệ có thể mở:

- Thông tin.
- Skill.
- Hỗ trợ.
- Event.
- Shelter Site.
- Ending.

---

## 11. Persistent World Progression

Thế giới ghi nhớ:

- Location đã bị phá.
- Tuyến đường đã mở.
- NPC còn sống.
- Faction đã hình thành.
- Shelter đã mất.
- Thiết bị đã khôi phục.
- Thông tin đã phát hiện.
- Hazard còn tồn tại.

World Progression không phải phần thưởng trực tiếp nhưng làm Campaign phản ánh hành động của người chơi.

---

## 12. Carry-over Rule

Không phải mọi tài nguyên được chuyển sang Chapter sau.

### Giữ lại

- Knowledge.
- Blueprint.
- Quan hệ.
- Một số Tool.
- Một số Equipment.
- Character Trait.
- World State.

### Giữ một phần

- Food.
- Water.
- Medicine.
- Fuel.
- Material.
- Spare Parts.

### Có thể mất

- Module cố định.
- Vật phẩm trong Zone bị phá.
- Resource bị hỏng.
- Temporary Shelter Upgrade.
- Disaster-specific consumable.

---

## 13. Anti-Hoarding Rule

Chapter sau không được bị phá cân bằng bởi tích trữ Chapter trước.

Các cơ chế kiểm soát:

- Storage giới hạn.
- Vật phẩm có Condition.
- Chi phí vận chuyển giữa Shelter.
- Module cố định không thể mang.
- Disaster mới thay đổi giá trị tài nguyên.
- Một phần Resource dùng cho giai đoạn hậu quả.

Không dùng reset tài nguyên hoàn toàn.

---

## 14. Progression Choice

Người chơi không thể tối đa hóa mọi hướng.

Các hướng chính:

```text
Shelter
Exploration
Technology
Community
Information
Character Capability
```

Đầu tư vào một hướng phải làm giảm khả năng đầu tư hướng khác trong cùng Chapter.

---

## 15. Unlock Rule

Unlock phải có nguyên nhân trong thế giới.

Không dùng:

```text
Đạt Level 5
→
Tự biết chế máy bơm
```

Dùng:

```text
Tìm bản thiết kế
+
Có Workshop
+
Có Tool phù hợp
→
Có thể chế máy bơm
```

---

## 16. Multiplayer Progression

### Shared Progression

- Blueprint.
- Shelter Module.
- World State.
- Shared Intel.
- NPC relationship với nhóm.

### Personal Progression

- Skill.
- Trait.
- Equipment.
- Personal relationship flags nếu cần.

Không khóa Blueprint chung vào người chơi đang offline.

---

## 17. Phạm vi MVP

Triển khai:

- Blueprint.
- Skill proficiency đơn giản.
- Trait.
- Relationship flags.
- Persistent World State.
- Carry-over data structure.
- Unlock dựa trên thông tin và điều kiện.

Chưa triển khai:

- Skill tree lớn.
- Level nhân vật.
- Experience point truyền thống.
- Perk rarity.
- Respec.
- Meta-currency.

---

## 18. Quyết định chốt

- Progression ưu tiên khả năng mới hơn tăng chỉ số.
- Knowledge và Blueprint là tiến trình bền vững.
- Character Progression dùng Skill và Trait, không dùng Level.
- World State là một phần của Progression.
- Không reset toàn bộ tài nguyên giữa Chapter.
- Storage, vận chuyển và Condition kiểm soát tích trữ.
- Multiplayer chia Shared và Personal Progression.
