# Last Hope — MVP Scope

## 1. Tên MVP

```text
Siêu Bão Mưa Đen
```

Tên tiếng Anh:

```text
Black Rain
```

MVP đồng thời là Chapter 1 của Campaign.

---

## 2. Mục tiêu sản phẩm

MVP phải kiểm chứng:

- Core Exploration Loop.
- Áp lực World Clock.
- Resource Trade-off.
- Shelter Preparation.
- Dynamic Route và Location State.
- Hazard Exposure.
- Information System.
- Event Deadline.
- Peak Phase dựa trên quá trình chuẩn bị.
- Outcome phản ánh quyết định.

MVP không nhằm kiểm chứng toàn bộ Campaign hoặc mọi hệ thống dài hạn.

---

## 3. Trải nghiệm mục tiêu

Người chơi bắt đầu trong một khu đô thị bình thường trước khi siêu bão xảy ra.

Trong quá trình chơi:

1. Nhận cảnh báo.
2. Khảo sát Shelter.
3. Thu thập tài nguyên và thông tin.
4. Xây các Module ưu tiên.
5. Thích nghi khi mưa chuyển thành màu đen.
6. Đối mặt với đường ngập và nước ô nhiễm.
7. Quyết định cứu NPC hoặc bảo vệ tài nguyên.
8. Chuẩn bị cho đỉnh lũ.
9. Vận hành Shelter trong Peak Phase.
10. Nhận Outcome dựa trên World State.

---

## 4. Thời lượng mục tiêu

### Một lượt chơi hoàn chỉnh

```text
5–8 giờ thực
```

### Thời lượng trong thế giới

Khoảng:

```text
4 ngày trong game
```

### Tỷ lệ thời gian đề xuất

```text
1 phút thực
=
5 phút trong game
```

Tỷ lệ cuối cùng phải được xác minh bằng prototype.

---

## 5. Disaster Timeline

MVP gồm các Phase:

```text
Normal
Warning
First Rain
Black Rain
Escalation
Peak
Aftermath
```

### Normal

- Tutorial cơ bản.
- Giới thiệu Shelter và NPC.
- Thế giới vẫn hoạt động bình thường.

### Warning

- Khám phá và chuẩn bị.
- Các tài nguyên quan trọng bắt đầu khan hiếm.
- Người chơi chọn ưu tiên đầu tiên.

### First Rain

- Giới thiệu Wet, Cold và Flood Depth.
- Một số Route bắt đầu thay đổi.

### Black Rain

- Nước chuyển thành Black Water.
- Exposure và Contamination trở thành áp lực.
- Nhiễu điện từ tăng.

### Escalation

- Route bị khóa.
- Location tầng thấp bị mất.
- Shelter chịu Water Intrusion.
- Người chơi thực hiện chuyến đi cuối.

### Peak

- Trọng tâm chuyển sang vận hành Shelter.
- Power, Pump, Storage và Safe Zone bị kiểm tra.
- Hạn chế hoạt động bên ngoài.

### Aftermath

- Nước bắt đầu rút.
- Persistent World State được cập nhật.
- Outcome được đánh giá.
- Narrative Hook được mở.

---

## 6. World Scope

### Shelter

```text
1 Main Shelter
1 Temporary Shelter
1 Shelter Site thay thế
```

Main Shelter được thiết kế sẵn với:

- Fixed Core Components.
- Predefined Zones.
- Buildable Slots hoặc Areas.

Người chơi không xây Shelter tự do từ đất trống (không tạo Zone mới/đổi cấu trúc nhà) — nhưng
đặt Module ở vị trí tự do bên trong Zone hợp lệ, không phải chọn từ danh sách Slot cố định
(xem `docs/02-core-systems/building-and-placement-system-design.md`).

### Location

```text
7 Location chính
```

Bao gồm:

1. Khu nhà dân.
2. Cửa hàng tiện lợi.
3. Hiệu thuốc.
4. Gara điện nước.
5. Trường học.
6. Trạm bơm khu vực.
7. Trạm thời tiết.

### Route

```text
3 Route chính
1 Shortcut mở khóa
```

Các Route thay đổi theo:

- World Clock.
- Disaster Phase.
- Flood State.
- Event.
- Trạng thái trạm bơm.

---

## 7. Shelter Scope

### Fixed Core Components

Main Shelter có các thành phần bất biến:

```text
Main Staircase
Structural Pillars
Drain Core
Electrical Backbone
Water Intake Point
Roof Antenna Mount
```

Các thành phần này:

- Không thể di chuyển.
- Không thể tháo dỡ.
- Là điểm neo của Event và Hazard.

### Module

MVP triển khai khoảng bảy Module:

1. Flood Barrier.
2. Portable Water Pump.
3. Elevated Storage.
4. Water Purifier.
5. Drying Station.
6. Communication Station.
7. Battery Bank.

Người chơi không đủ thời gian và tài nguyên để tối đa hóa toàn bộ Module trong một lượt.

---

## 8. Hazard Scope

MVP triển khai:

### Flood Depth

```text
Dry
Shallow
Medium
Deep
Impassable
```

### Current Strength

```text
None
Weak
Moderate
Strong
Critical
```

### Black Water Contamination

Tác động lên:

- Player.
- Injury.
- Item.
- Storage.
- Shelter.

### Electromagnetic Interference

Tác động lên:

- Radio.
- Forecast.
- Communication Station.
- Thiết bị điện.
- Event Information.

### Hazard phụ

- Wet.
- Cold.
- Electrified Water.
- Structural Collapse Risk.

MVP không sử dụng fluid simulation toàn bản đồ.

---

## 9. Player System Scope

MVP triển khai:

```text
Health
Stamina
Fatigue
Hunger
Thirst
Body Temperature
Carry Load
```

Injury và Status Effect chính:

```text
Cut
Bruise
Sprain
Wet
Cold
Bleeding
Sick
Black Water Exposure
Disoriented
Incapacitated
```

MVP không triển khai:

- Tâm lý chuyên sâu.
- Dinh dưỡng chi tiết.
- Hệ thống bệnh phức tạp.
- Mô phỏng từng bộ phận cơ thể đầy đủ.

---

## 10. Inventory và Equipment Scope

Triển khai:

- Weight.
- Volume.
- Equipment Slot.
- Quick Access.
- Backpack.
- Carried Object.
- Container.
- Durability.
- Condition.
- Water Resistance.
- Contamination State.

Không triển khai inventory grid xếp hình.

Equipment trọng tâm:

- Áo mưa.
- Ủng chống nước.
- Găng tay.
- Đèn pin.
- Dây thừng.
- Ba lô chống nước.
- Tool xây dựng và sửa chữa.

---

## 11. Resource Scope

### Survival Resource

- Clean Water.
- Food.
- Medicine.
- Dry Clothing.

### Shelter Resource

- Wood.
- Metal.
- Waterproof Material.
- Fuel.
- Battery.
- Filter.
- Pump Part.

### Tool

- Hammer.
- Crowbar.
- Rope.
- Flashlight.
- Multitool.
- Water Test Kit.

### Information Resource

- Forecast.
- Route Intel.
- Shelter Intel.
- Blueprint.
- Signal Data.

Không có Random Loot Respawn.

Location đã được loot chỉ có tài nguyên mới khi World Event tạo ra nguồn hợp lý.

---

## 12. NPC Scope

MVP sử dụng khoảng bốn NPC quan trọng:

1. Người hàng xóm.
2. Nhân viên y tế.
3. Kỹ thuật viên thoát nước.
4. Người vận hành radio.

NPC có:

- Skill.
- Trait.
- Trust.
- Condition.
- Resource Consumption.
- Shelter Task.
- Expedition Support.
- Persistent State.

Người chơi không thể đảm bảo cứu toàn bộ NPC trong một lượt.

---

## 13. Event Scope

### Main Event

Khoảng:

```text
14 Event bắt buộc hoặc có kiểm soát
```

Bao gồm:

- Cảnh báo siêu bão.
- Mưa đầu tiên.
- Mưa chuyển màu đen.
- Mất điện.
- Drain Core chảy ngược.
- Trạm bơm gặp sự cố.
- Route bị khóa.
- Shelter Pump bị tắc.
- Storage có nguy cơ ngập.
- Tín hiệu bất thường.
- Nước bắt đầu rút.

### Optional Event

Khoảng:

```text
8–12 Event
```

Mỗi lượt chơi chỉ sử dụng một phần Event Pool.

Event phải tuân theo Event Budget.

---

## 14. Information Scope

Triển khai:

- Location Intel.
- Route Intel.
- Hazard Intel.
- Event Intel.
- Disaster Forecast.
- Information Age.
- Confidence.
- Radio.
- Communication Station.
- NPC Information.
- Tín hiệu bất thường.

World Map chỉ hiển thị thông tin người chơi đã biết.

---

## 15. Time Scope

Triển khai:

- Một World Clock.
- Một tốc độ thời gian cố định.
- Day/Night Cycle.
- Timed Action.
- Background Task.
- Event Deadline.
- Disaster Timeline.
- Sleep Simulation.

Không triển khai:

- Pause World Clock trong gameplay.
- Fast Forward.
- Time Acceleration.
- Time Skip ngoài giấc ngủ.
- Đồng hồ riêng cho từng người chơi.

---

## 16. Progression Scope

Triển khai:

- Blueprint.
- Knowledge.
- Skill Proficiency đơn giản.
- Trait.
- NPC Relationship Flag.
- Persistent World State.
- Chapter Outcome Flag.

Không triển khai:

- Character Level.
- Skill Tree lớn.
- Experience Point truyền thống.
- Meta-currency.

---

## 17. Outcome Scope

MVP có năm Outcome:

```text
Exceptional Survival
Stable Survival
Barely Survived
Forced Evacuation
Collapse
```

Outcome dựa trên:

- Player Survival.
- Shelter Condition.
- Resource Stability.
- NPC Survival.
- Information Acquired.
- World Impact.
- Persistent Damage.

Cuối Chapter phải có Causal Outcome Report.

---

## 18. Single-player và Multiplayer

MVP triển khai Single-player.

Dữ liệu hệ thống phải hỗ trợ Multiplayer trong tương lai:

- World Clock chung.
- World State chung.
- Shelter State chung.
- Event State chung.
- Inventory riêng.
- Player Condition riêng.
- Exposure riêng.
- Shared Intel và Personal Observation.

MVP không triển khai networking hoàn chỉnh.

---

## 19. Ngoài phạm vi MVP

Không triển khai:

- Combat chuyên sâu.
- Firearm.
- Vehicle Driving.
- Procedural Map.
- Dynamic Building Destruction toàn phần.
- Fluid Simulation toàn bản đồ.
- Faction System hoàn chỉnh.
- Full Multiplayer.
- Xây Shelter từ đất trống.
- Campaign Chapter 2.
- Giải thích hoàn chỉnh nguồn gốc Mưa Đen.
- Random Loot Respawn.

---

## 20. Tiêu chí hoàn thành

MVP được xem là hoàn thành khi:

1. Người chơi có thể hoàn thành toàn bộ Disaster Timeline.
2. Peak Phase phản ánh rõ quá trình chuẩn bị.
3. Có ít nhất ba hướng chuẩn bị Shelter khả thi.
4. Người chơi không thể thu thập toàn bộ tài nguyên.
5. Route và Location thay đổi buộc người chơi điều chỉnh kế hoạch.
6. Location đã loot không tự hồi tài nguyên.
7. Event có thể tiến triển khi người chơi không có mặt.
8. Không có thất bại lớn không được cảnh báo.
9. Có ít nhất ba Outcome có thể đạt được.
10. Một lượt chơi kéo dài khoảng 5–8 giờ.
11. Có thể hoàn thành mà không cứu mọi NPC.
12. Ending để lại Narrative Hook cho Chapter tiếp theo.
