# 06-black-rain-npc-design.md

## 1. Mục tiêu

Tài liệu này xác định bốn NPC quan trọng trong MVP Siêu Bão Mưa Đen.

NPC phải:

- Có vai trò gameplay riêng.
- Tạo cả lợi ích và chi phí.
- Có Event Chain ngắn.
- Có điều kiện cứu hoặc tuyển dụng rõ ràng.
- Ảnh hưởng Shelter, Resource Economy hoặc Information.
- Có Outcome tồn tại sau Chapter.
- Không bắt buộc phải cứu toàn bộ để hoàn thành MVP.

---

## 2. NPC Set

| NPC         | Vai trò chính             | Skill                         |
| ----------- | ------------------------- | ----------------------------- |
| Nguyễn Minh | Hàng xóm, Shelter Support | Construction                  |
| Trần Mai    | Nhân viên y tế            | Medical                       |
| Lê Hùng     | Kỹ thuật viên thoát nước  | Water Processing, Electronics |
| Phạm An     | Người vận hành radio      | Communication, Navigation     |

Tên có thể thay đổi trong giai đoạn Narrative Production.

---

## 3. Nguyên tắc cân bằng NPC

Mỗi NPC phải cung cấp:

```text
1 khả năng chiến lược
+
1 Event Chain
+
1 chi phí duy trì
+
1 rủi ro hoặc giới hạn
```

Không NPC nào được thay thế hoàn toàn một hệ thống gameplay.

Ví dụ:

- NPC kỹ thuật giúp sửa Pump nhanh hơn.
- Người chơi vẫn cần Tool, Part và thời gian.

---

## 4. Trạng thái NPC

```text
Unknown
Located
In Danger
Rescued
Recruited
Sheltered
Missing
Departed
Dead
```

NPC State được lưu trong World State.

---

## 5. Thuộc tính chung

```text
npc_id
display_name
current_location
health
fatigue
hunger
thirst
injuries
skills
traits
trust
loyalty
current_task
recruitment_state
event_chain_state
```

---

# 6. Nguyễn Minh

## 6.1. Identity

```text
npc_id: npc_neighbor_minh
display_name: Nguyễn Minh
age: 34
initial_location: Khu nhà dân
primary_role: Shelter Support
primary_skill: Construction
```

Minh sống gần Main Shelter và là người đầu tiên người chơi gặp.

Anh có kinh nghiệm sửa chữa dân dụng nhưng không phải kỹ sư chuyên nghiệp.

---

## 6.2. Gameplay Role

Minh hỗ trợ:

- Xây Flood Barrier.
- Di chuyển vật nặng.
- Gia cố Shelter.
- Dọn vật cản.
- Vận chuyển Resource.

---

## 6.3. Skill

```text
Construction: 2
Scavenging: 1
Medical: 0
Electronics: 0
Navigation: 1
```

Tác động:

- Construction Task nhanh hơn.
- Giảm nhẹ hao Wood.
- Có thể hỗ trợ hai người mang Large Object.

---

## 6.4. Trait

```text
trait: Dependable
```

Tác động:

- Sẵn sàng phản ứng với Shelter Event khi Trust đủ.
- Có thể tự chuyển Resource lên tầng cao trong Peak.
- Ít khả năng từ chối Task bảo vệ Shelter.

---

## 6.5. Chi phí

- `1.5 Water Unit / ngày`.
- `1 Food Unit / ngày`.
- Chiếm một Living Capacity.
- Có Event liên quan người thân.

---

## 6.6. Event Chain

### Step 1 — Neighbor Introduction

Phase:

```text
Normal
```

Minh cảnh báo:

- Drain khu dân cư thoát chậm.
- Người thân của anh đang ở một căn nhà khác.

Người chơi có thể:

- Mời Minh hỗ trợ chuẩn bị.
- Từ chối.
- Yêu cầu anh tự lo.

---

### Step 2 — Missing Relative

Phase:

```text
Black Rain
```

Người thân của Minh mắc kẹt trong Khu nhà dân.

Lựa chọn:

- Cùng Minh đi cứu.
- Gửi Minh đi một mình.
- Hoãn.
- Từ chối.

---

### Step 3 — Consequence

#### Cứu thành công

- Trust tăng cao.
- Minh trở thành NPC ổn định.
- Có thêm một người cần tài nguyên hoặc người thân được sơ tán sang Trường học.

#### Gửi Minh đi một mình

- Minh có thể bị Injury.
- Có nguy cơ mất tích tùy Route State.
- Không dùng RNG hoàn toàn; kết quả dựa trên Hazard và Equipment được cấp.

#### Từ chối

- Trust giảm mạnh.
- Minh có thể rời Shelter.
- Có thể xuất hiện lại tại Trường học.

---

## 6.7. Recruitment

Minh gia nhập nếu:

- Người chơi giúp anh trong Normal hoặc Warning.
- Shelter còn Living Capacity.
- Trust không ở mức Hostile.

Không yêu cầu hoàn thành toàn bộ Event người thân để tuyển ban đầu.

---

## 6.8. Shelter Task

- Build Flood Barrier.
- Move Storage.
- Reinforce Structural Point.
- Clear Drain.
- Operate Manual Pump.
- Guard Entrance.

---

## 6.9. Expedition Role

- Mang thêm vật liệu.
- Hỗ trợ Large Object.
- Dọn vật cản.
- Giảm thời gian xây Outdoor Module.

---

## 6.10. Outcome

### Positive

- Sống sót.
- Người thân được cứu.
- Trust cao.
- Mở `Reliable Builder` Campaign Flag.

### Mixed

- Minh sống nhưng rời nhóm.
- Người thân mất tích.
- Quan hệ bị giảm.

### Negative

- Minh chết hoặc mất tích.
- Shelter mất Construction Support.
- Event được ghi vào Outcome Report.

---

# 7. Trần Mai

## 7.1. Identity

```text
npc_id: npc_medical_mai
display_name: Trần Mai
age: 29
initial_location: Hiệu thuốc
primary_role: Medical Support
primary_skill: Medical
```

Mai là nhân viên y tế đang hỗ trợ người bị thương tại Hiệu thuốc khi bão bắt đầu.

---

## 7.2. Gameplay Role

Mai hỗ trợ:

- Điều trị Injury.
- Giảm Medicine Consumption.
- Xử lý Black Water Exposure.
- Xác định Resource y tế.
- Mở Medical Station Option.

---

## 7.3. Skill

```text
Medical: 3
Water Processing: 1
Scavenging: 1
Construction: 0
```

Tác động:

- Điều trị nhanh hơn.
- Một Medicine Unit có hiệu quả cao hơn.
- Phát hiện sớm Sick và Infection Risk.
- Mở Treatment Option không có cho người chơi bình thường.

---

## 7.4. Trait

```text
trait: Triage Specialist
```

Tác động:

- Khi nhiều nhân vật bị thương, Mai xác định thứ tự điều trị tối ưu.
- Giảm nguy cơ NPC chết trong Shelter Event.
- Không làm tăng Health miễn phí.

---

## 7.5. Chi phí

- `1.5 Water Unit / ngày`.
- `1 Food Unit / ngày`.
- Cần Clean Living Area.
- Có xu hướng ưu tiên người bị thương khác trước bản thân.

---

## 7.6. Event Chain

### Step 1 — Medical Distress

Phase:

```text
First Rain hoặc Black Rain
```

Người chơi nhận tín hiệu từ Hiệu thuốc.

Mai đang:

- Chăm sóc một người bị thương.
- Bị kẹt bởi nước và điện.

---

### Step 2 — Rescue Decision

Người chơi có thể:

- Cắt điện và cứu cả hai.
- Chỉ đưa Mai ra.
- Cung cấp Medicine rồi rời đi.
- Bỏ qua.

---

### Step 3 — Ethical Cost

#### Cứu cả hai

- Tốn thời gian và Resource.
- Trust của Mai tăng.
- Shelter có thêm một người phụ thuộc tạm thời.

#### Chỉ cứu Mai

- Mai gia nhập nhưng Trust thấp.
- Có thể từ chối một số Task.
- Event được ghi vào Outcome.

#### Cung cấp Medicine

- Mai ở lại Location lâu hơn.
- Có thể tới Trường học sau.
- Không gia nhập ngay.

---

## 7.7. Recruitment

Mai gia nhập nếu:

- Được cứu khỏi Hiệu thuốc.
- Hoặc gặp lại tại Trường học sau khi được hỗ trợ.
- Shelter có Clean Living Area.
- Người chơi không cố ý bỏ mặc bệnh nhân trong tình huống có thể cứu.

---

## 7.8. Shelter Task

- Treat Injury.
- Monitor Sick NPC.
- Manage Medical Storage.
- Clean Contaminated Wound.
- Operate Medical Station.

---

## 7.9. Expedition Role

- Giảm rủi ro Injury trở nặng.
- Xử lý Bleeding tại hiện trường.
- Xác định Medicine Search Point.
- Không phù hợp mang vật nặng.

---

## 7.10. Unlock

Mai có thể mở:

```text
Medical Station
Advanced Wound Cleaning
Black Water Exposure Treatment
```

Người chơi vẫn cần Blueprint, Material và Medicine tương ứng.

---

## 7.11. Outcome

### Positive

- Mai sống.
- Bệnh nhân được cứu.
- Mở Campaign Knowledge về Contamination Treatment.

### Mixed

- Mai sống nhưng Trust thấp.
- Medical Support bị giới hạn.

### Negative

- Mai chết hoặc mắc kẹt.
- Medicine vẫn có thể được loot nhưng không có Skill Support.
- Sick Event trong Peak khó xử lý hơn.

---

# 8. Lê Hùng

## 8.1. Identity

```text
npc_id: npc_technician_hung
display_name: Lê Hùng
age: 41
initial_location: Gara điện nước hoặc Trạm bơm
primary_role: Flood Control
primary_skill: Water Processing, Electronics
```

Hùng là kỹ thuật viên bảo trì hệ thống thoát nước khu vực.

Location ban đầu của Hùng được chọn trong hai biến thể có kiểm soát.

---

## 8.2. Gameplay Role

Hùng hỗ trợ:

- Sửa Portable Pump.
- Khôi phục Trạm bơm.
- Xử lý Drain Backflow.
- Giảm Failure Risk của Utility Module.
- Cung cấp Drainage Intel.

---

## 8.3. Skill

```text
Water Processing: 3
Electronics: 2
Construction: 1
Navigation: 1
```

Tác động:

- Pump Repair nhanh hơn.
- Giảm Pump Part cần cho một số Task.
- Phát hiện Drain Event sớm.
- Xác định Route có nguy cơ Backflow.

---

## 8.4. Trait

```text
trait: Infrastructure Expert
```

Tác động:

- Hiển thị hậu quả dự kiến của lựa chọn Restore hoặc Salvage tại Trạm bơm.
- Mở Emergency Drain Seal.
- Không tự cung cấp Resource.

---

## 8.5. Chi phí

- `1.5 Water Unit / ngày`.
- `1 Food Unit / ngày`.
- Cần nghỉ sau Task kỹ thuật dài.
- Có thể từ chối tháo hủy Trạm bơm nếu Trust thấp.

---

## 8.6. Event Chain

### Variant A — Gara

Hùng bị mắc trong Service Pit khi nước bắt đầu dâng.

Người chơi có thể:

- Dùng Pump tạm.
- Dùng Rope.
- Cắt điện.
- Bỏ qua.

### Variant B — Trạm bơm

Hùng đang cố giữ hệ thống hoạt động.

Người chơi phải lựa chọn:

- Giúp khôi phục.
- Thuyết phục anh tháo linh kiện.
- Đưa anh rời đi.
- Bỏ mặc.

---

## 8.7. Recruitment

Hùng gia nhập nếu:

- Được cứu.
- Người chơi có kế hoạch bảo vệ Shelter hoặc dân cư hợp lý.
- Trust không bị phá bởi lựa chọn ích kỷ rõ ràng.

Nếu người chơi tháo linh kiện Trạm bơm:

- Hùng vẫn có thể gia nhập nếu được thuyết phục bằng tình trạng Shelter.
- Trust khởi đầu thấp hơn.

---

## 8.8. Shelter Task

- Operate Pump.
- Repair Drain Core.
- Maintain Generator.
- Inspect Electrical Backbone.
- Build Emergency Seal.
- Process Untreated Water.

---

## 8.9. Expedition Role

- Mở Maintenance Access.
- Giảm thời gian sửa thiết bị.
- Phát hiện Hazard điện.
- Không có lợi thế y tế.

---

## 8.10. Unlock

```text
Portable Pump Blueprint
Emergency Drain Seal
Pump Maintenance Procedure
Regional Drainage Intel
```

---

## 8.11. Outcome

### Positive

- Hùng sống.
- Trạm bơm được khôi phục hoặc Shelter Pump duy trì tốt.
- Mở Infrastructure Knowledge.

### Mixed

- Hùng sống nhưng Trạm bơm bị tháo.
- Shelter mạnh hơn nhưng khu vực chịu thiệt hại cao hơn.

### Negative

- Hùng chết hoặc mất tích.
- Pump Event khó xử lý hơn.
- Trạm bơm không thể đạt kết quả tối ưu.

---

# 9. Phạm An

## 9.1. Identity

```text
npc_id: npc_radio_an
display_name: Phạm An
age: 26
initial_location: Trạm thời tiết hoặc Trường học
primary_role: Information và Communication
primary_skill: Communication, Navigation
```

An là người vận hành hệ thống radio và quan trắc thời tiết bán chuyên.

---

## 9.2. Gameplay Role

An hỗ trợ:

- Communication Station.
- Forecast.
- Event Discovery.
- Signal Verification.
- Shared Intel.
- Route Planning.

---

## 9.3. Skill

```text
Communication: 3
Navigation: 2
Electronics: 1
Medical: 0
```

Tác động:

- Tăng Confidence của Radio Intel.
- Giảm ảnh hưởng Interference.
- Phát hiện Event sớm hơn.
- Cải thiện Estimated Return Time.

---

## 9.4. Trait

```text
trait: Signal Analyst
```

Tác động:

- Phân biệt tín hiệu cứu hộ và tín hiệu bất thường.
- Mở Narrative Clue.
- Giảm nguy cơ theo tín hiệu sai hoặc không đầy đủ.

---

## 9.5. Chi phí

- `1.5 Water Unit / ngày`.
- `1 Food Unit / ngày`.
- Cần Power và Communication Equipment để phát huy đầy đủ.
- Hiệu quả thấp nếu Shelter không có Communication Station.

---

## 9.6. Event Chain

### Step 1 — Broken Transmission

Phase:

```text
Black Rain
```

Người chơi nhận tín hiệu không đầy đủ từ:

- Trạm thời tiết.
- Hoặc mái Trường học.

---

### Step 2 — Locate Operator

Người chơi dùng:

- Radio.
- Observation Point.
- NPC Intel.
- Route Exploration.

để xác định vị trí An.

---

### Step 3 — Station Decision

Lựa chọn:

- Giữ Trạm thời tiết hoạt động.
- Tháo thiết bị mang về Shelter.
- Đưa An tới Trường học.
- Đưa An về Main Shelter.

---

### Step 4 — Peak Signal

Nếu An sống và có thiết bị hoạt động:

- Peak Signal được ghi lại.
- Forecast có Confidence cao hơn.
- Narrative Clue được mở.

Nếu không:

- Tín hiệu vẫn xuất hiện nhưng không được xác minh đầy đủ.

---

## 9.7. Recruitment

An gia nhập nếu:

- Được cứu hoặc liên lạc thành công.
- Có Shelter với Power tối thiểu.
- Có Communication Device hoặc kế hoạch khôi phục thiết bị.

An có thể ở lại Trường học thay vì Main Shelter.

---

## 9.8. Shelter Task

- Operate Communication Station.
- Monitor Forecast.
- Verify Distress Signal.
- Maintain Shared Intel.
- Watch Peak Signal.

---

## 9.9. Expedition Role

- Cải thiện Route Intel.
- Giảm khả năng bị lạc khi Disoriented.
- Phát hiện Observation Point.
- Khả năng thể lực thấp.

---

## 9.10. Unlock

```text
Communication Station Upgrade
Signal Stabilization Procedure
Peak Signal Record
Advanced Forecast Interpretation
```

---

## 9.11. Outcome

### Positive

- An sống.
- Peak Signal được ghi lại.
- Forecast Data được bảo toàn.
- Mở Campaign Narrative Flag.

### Mixed

- An sống nhưng thiết bị bị mất.
- Chỉ thu được thông tin một phần.

### Negative

- An mất tích hoặc chết.
- Signal Data không được xác minh.
- Information Score giảm.

---

# 10. NPC Interaction Matrix

| NPC  | Minh                 | Mai                           | Hùng                   | An                  |
| ---- | -------------------- | ----------------------------- | ---------------------- | ------------------- |
| Minh | —                    | Hỗ trợ bảo vệ Medical Area    | Hỗ trợ Construction    | Hỗ trợ dựng Antenna |
| Mai  | Điều trị Injury      | —                             | Điều trị Exposure      | Hỗ trợ Fatigue      |
| Hùng | Xây Pump nhanh hơn   | Cần Clean Water cho Treatment | —                      | Ổn định thiết bị    |
| An   | Cung cấp Event Intel | Phát hiện Medical Signal      | Cảnh báo Drain Failure | —                   |

Tương tác NPC chủ yếu thể hiện qua Task và Event, không cần hệ thống hội thoại lớn.

---

# 11. NPC Shelter Capacity

Main Shelter:

```text
initial_capacity: 2
maximum_capacity: 5
```

Bao gồm Player.

Nếu tuyển toàn bộ bốn NPC:

```text
occupants = 5
```

Người chơi phải xây:

- Additional Bed.
- Expanded Living Area hoặc dùng Temporary Shelter.
- Tăng Food và Water Reserve.

Cứu toàn bộ NPC là khả thi nhưng đắt.

---

# 12. NPC Resource Pressure

Một NPC được cứu sớm:

- Cung cấp nhiều thời gian lao động hơn.
- Tiêu thụ nhiều Resource hơn.

Một NPC được cứu muộn:

- Tiêu thụ ít Resource hơn.
- Có ít thời gian tạo lợi ích.
- Có thể đã bị Injury.

Đây là đánh đổi cố ý.

---

# 13. Task Assignment

Mỗi NPC chỉ có một `current_task`.

Task có thể là:

```text
Shelter Active Task
Shelter Monitoring
Expedition Support
Rest
Treatment
Unavailable
```

NPC Task sử dụng World Clock.

---

# 14. Trust Level

```text
Distrustful
Neutral
Cooperative
Trusted
Loyal
```

Trust ảnh hưởng:

- Recruitment.
- Task nguy hiểm.
- Intel.
- Event Option.
- Khả năng ở lại trong Forced Evacuation.

Không hiển thị điểm số Trust chính xác.

---

# 15. NPC Refusal

NPC có thể từ chối khi:

- Task vượt khả năng.
- Fatigue quá cao.
- Hazard quá nguy hiểm.
- Trust quá thấp.
- Task mâu thuẫn với mục tiêu cá nhân.

Refusal phải có nguyên nhân rõ ràng.

---

# 16. NPC Incapacitation

NPC có thể bị Incapacitated do:

- Injury.
- Exposure.
- Hypothermia.
- Event.
- Expedition Failure.

Người chơi có thể:

- Sơ cứu.
- Kéo về.
- Bỏ lại.
- Gửi NPC khác hỗ trợ.

NPC không tự hồi phục hoàn toàn ngoài màn hình.

---

# 17. NPC Death Rule

NPC Death chỉ xảy ra khi:

- Event có nguy cơ đã được cảnh báo.
- Không được điều trị.
- Bị bỏ lại trong Hazard.
- Forced Evacuation thất bại.
- World State khiến cứu hộ không còn hợp lý.

Không dùng RNG đơn lẻ để giết NPC quan trọng.

---

# 18. Multiplayer Rule

- NPC State là dữ liệu chung.
- Task assignment được đồng bộ.
- Relationship chính thuộc nhóm.
- Personal dialog flag có thể lưu riêng.
- Không người chơi nào sở hữu NPC.
- Một NPC chỉ có thể tham gia một Expedition Group tại một thời điểm.

---

# 19. Outcome Contribution

| NPC  | Outcome chính                   |
| ---- | ------------------------------- |
| Minh | Shelter Condition               |
| Mai  | Player và NPC Survival          |
| Hùng | Infrastructure và Flood Control |
| An   | Information và Narrative        |

Cứu NPC không tự động tăng Outcome nếu NPC không được duy trì hoặc sử dụng hợp lý.

---

# 20. Dữ liệu hệ thống

```text
black_rain_npc
├── npc_id
├── identity
├── current_location
├── survivor_state
├── recruitment_state
├── health_condition
├── fatigue
├── skills
├── trait
├── trust
├── loyalty
├── current_task
├── shelter_assignment
├── event_chain_state
├── personal_flags
└── persistent_outcome
```

---

# 21. Phạm vi MVP

Triển khai:

- Bốn NPC quan trọng.
- Skill.
- Trait.
- Trust.
- Recruitment.
- Resource Consumption.
- Shelter Task.
- Expedition Support.
- Event Chain từ hai đến bốn bước.
- Incapacitation.
- Persistent Outcome.

Chưa triển khai:

- Romance.
- Hội thoại phân nhánh lớn.
- Lịch sinh hoạt chi tiết.
- Faction Relationship phức tạp.
- Tâm lý chuyên sâu.
- Skill Tree NPC.
- NPC tự tổ chức Expedition ngoài màn hình.

---

# 22. Quyết định chốt

- MVP có bốn NPC quan trọng.
- Mỗi NPC đại diện cho một hướng chiến lược.
- NPC cung cấp lợi ích và tạo thêm Resource Pressure.
- Không cần cứu toàn bộ NPC để hoàn thành Chapter.
- NPC có thể từ chối Task vì lý do rõ ràng.
- NPC quan trọng không chết vì RNG đơn lẻ.
- Main Shelter có thể chứa toàn bộ nhóm nếu được nâng cấp.
- Trường học có thể tiếp nhận NPC khi Main Shelter quá tải.
- NPC Outcome được ghi vào World State và Chapter Report.
