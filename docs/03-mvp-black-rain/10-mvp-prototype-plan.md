# 10-mvp-prototype-plan.md

## 1. Mục tiêu

Tài liệu này xác định kế hoạch prototype cho MVP Siêu Bão Mưa Đen.

Prototype phải kiểm chứng:

- Core Loop có tạo quyết định thú vị hay không.
- Time Pressure có rõ và công bằng hay không.
- Exploration và Shelter có cạnh tranh thời gian hay không.
- Flood và Black Water có thay đổi cách chơi hay không.
- Peak Phase có phản ánh quá trình chuẩn bị hay không.
- Phạm vi MVP có khả thi hay không.

Không xây toàn bộ nội dung trước khi các giả thuyết cốt lõi được kiểm chứng.

---

## 2. Nguyên tắc prototype

### 2.1. Kiểm chứng theo rủi ro

Ưu tiên kiểm chứng:

```text
Core Loop
↓
Time
↓
Resource Decision
↓
Flood Traversal
↓
Shelter Preparation
↓
Disaster Timeline
↓
Content
```

Không ưu tiên:

- Đồ họa hoàn chỉnh.
- Animation phức tạp.
- Narrative Production lớn.
- Multiplayer.
- Hệ thống phụ.

---

### 2.2. Mỗi prototype phải có câu hỏi

Không tạo prototype chỉ để “có hệ thống”.

Mỗi giai đoạn phải xác định:

- Giả thuyết.
- Nội dung tối thiểu.
- Dữ liệu cần đo.
- Điều kiện đạt.
- Điều kiện phải thiết kế lại.

---

### 2.3. Art tối thiểu

Prototype dùng:

- Blockout.
- Primitive.
- Icon tạm.
- UI debug.
- Âm thanh tối thiểu nếu cần kiểm chứng feedback.

Không sản xuất asset hoàn chỉnh trước Prototype 4.

---

# 3. Tổng lộ trình

```text
P0 — Paper Simulation
P1 — Exploration Loop
P2 — Flood and Hazard Loop
P3 — Shelter Loop
P4 — Disaster Vertical Slice
P5 — Full MVP Production
P6 — Integration and Balance
P7 — Release Candidate
```

---

# 4. P0 — Paper Simulation

## 4.1. Mục tiêu

Kiểm chứng Resource Economy và Time Budget trước khi viết nhiều hệ thống.

---

## 4.2. Nội dung

Mô phỏng bằng bảng:

- World Clock.
- Ba Phase rút gọn.
- Một Shelter.
- Ba Location.
- Water, Food, Fuel và Material.
- Travel Time.
- Build Cost.
- Một Peak Check.

---

## 4.3. Kịch bản

Người chơi có:

```text
12 giờ chuẩn bị
3 Location
2 Module có thể xây
1 NPC có thể cứu
```

Không đủ thời gian để hoàn thành toàn bộ.

---

## 4.4. Câu hỏi kiểm chứng

- Người chơi có phải từ bỏ mục tiêu không?
- Có ít nhất hai chiến lược sống sót không?
- Một Resource có trở thành lựa chọn luôn đúng không?
- Cứu NPC có lợi nhưng có chi phí không?
- Peak có phản ánh quyết định trước đó không?

---

## 4.5. Exit Criteria

- Có ít nhất ba chiến lược hợp lệ.
- Không có một thứ tự hành động luôn tối ưu.
- Resource tối thiểu không phụ thuộc một nguồn duy nhất.
- Failure có thể giải thích bằng quyết định trước đó.

---

# 5. P1 — Exploration Loop

## 5.1. Mục tiêu

Kiểm chứng vòng lặp:

```text
Chuẩn bị
→
Di chuyển
→
Search
→
Loot Decision
→
Quay về
→
Storage
```

---

## 5.2. Phạm vi

- Một Shelter blockout.
- Một Route.
- Một Location nhỏ.
- World Clock.
- Player movement.
- Inventory.
- Weight và Volume.
- Search Point.
- Persistent Loot Depletion.
- Return Trip.
- Shelter Storage.

Chưa có:

- Flood.
- NPC.
- Build System hoàn chỉnh.
- Disaster Timeline đầy đủ.

---

## 5.3. Nội dung mẫu

Location:

```text
Cửa hàng tiện lợi prototype
```

Resource:

- Water.
- Food.
- Battery.
- Một vật nặng.
- Một vật cồng kềnh.

---

## 5.4. Giả thuyết

- Search thời gian thực tạo căng thẳng.
- Weight và Volume tạo quyết định loot.
- Travel Time khiến quay về Shelter có chi phí.
- Location depletion không làm người chơi cảm thấy bị trừng phạt vô lý.

---

## 5.5. Telemetry

- Thời gian ở Location.
- Search Point được mở.
- Item nhặt và bỏ lại.
- Carry Load khi quay về.
- Số lần quay lại Location.
- Thời gian Inventory mở.

---

## 5.6. Exit Criteria

- Người chơi chủ động bỏ lại ít nhất một item có giá trị.
- Không cần Inventory Grid để tạo quyết định.
- Search có thể dừng giữa chừng và vẫn hữu ích.
- Một chuyến đi hoàn chỉnh không có thời gian chết kéo dài.
- Loot depletion được lưu đúng.

---

## 5.7. Redesign Trigger

Thiết kế lại nếu:

- Người chơi luôn nhặt toàn bộ.
- Travel chỉ tạo chờ đợi.
- Search không có lý do dừng.
- Inventory thao tác nhiều hơn ra quyết định.
- Quay lại Location không có giá trị nào.

---

# 6. P2 — Flood and Hazard Loop

## 6.1. Mục tiêu

Kiểm chứng Flood State, Black Water Exposure và Route Change.

---

## 6.2. Phạm vi

Giữ toàn bộ P1 và thêm:

- Hai Route.
- Hai Location.
- Flood State.
- Current Strength.
- Wet.
- Body Temperature.
- Black Water Exposure.
- Equipment Protection.
- Route Closure.
- Disaster Phase rút gọn.

---

## 6.3. Timeline rút gọn

```text
Dry
↓
First Rain
↓
Black Rain
↓
Route Closure
```

Thời lượng test:

```text
30–45 phút
```

---

## 6.4. Equipment mẫu

- Áo mưa.
- Ủng.
- Rope.
- Ba lô chống nước.

---

## 6.5. Giả thuyết

- Flood thay đổi Route Decision, không chỉ giảm tốc độ.
- Equipment Protection có giá trị nhưng tạo Carry Cost.
- Black Water tạo áp lực tích lũy rõ ràng.
- Người chơi có thể dự đoán Route Closure.

---

## 6.6. Test Scenario

### Scenario A

- Route ngắn bị ngập.
- Route dài còn an toàn.

### Scenario B

- Người chơi mang nhiều Resource qua Medium Flood.

### Scenario C

- Người chơi thiếu Equipment nhưng cần hoàn thành Event.

### Scenario D

- Route thay đổi trong lúc Player đang ở Location.

---

## 6.7. Exit Criteria

- Người chơi thay Route do Flood.
- Equipment thay đổi quyết định Loadout.
- Hazard không gây Failure tức thời không cảnh báo.
- Return Window được hiểu.
- Route Closure không tạo softlock.
- Exposure có thể xử lý tại Shelter.

---

## 6.8. Redesign Trigger

- Flood chỉ là movement penalty.
- Người chơi luôn chọn Route ngắn.
- Equipment Protection quá mạnh hoặc vô nghĩa.
- Exposure khó hiểu.
- Route đóng khiến người chơi mắc kẹt không có phương án.

---

# 7. P3 — Shelter Loop

## 7.1. Mục tiêu

Kiểm chứng Shelter là một không gian gameplay có quyết định thực tế.

---

## 7.2. Phạm vi

- Main Shelter blockout.
- Ground Floor và Upper Floor.
- Water Intrusion.
- Power Allocation.
- Storage.
- Build Slot.
- Active Task.
- Passive Task.
- Portable Pump.
- Elevated Storage.
- Water Purifier.
- Sleep Simulation.
- Hai Shelter Event.

---

## 7.3. Test Timeline

```text
6 giờ chuẩn bị
↓
6 giờ Peak
```

---

## 7.4. Resource giới hạn

Người chơi không đủ Resource để xây tất cả:

- Pump.
- Elevated Storage.
- Water Purifier.

Chỉ có thể hoàn thiện hai trong ba.

---

## 7.5. Event

- Drain Backflow.
- Storage Flood Risk.
- Pump Jam nếu có Pump.

---

## 7.6. Giả thuyết

- Shelter Task cạnh tranh với Expedition Time.
- Power Priority tạo lựa chọn.
- Elevated Storage buộc chọn Resource bảo vệ.
- Lower Floor có thể bị mất mà game vẫn tiếp tục.
- Peak phản ánh Build Choice.

---

## 7.7. Exit Criteria

- Có ít nhất ba chiến lược Shelter hợp lệ.
- Không có một Module luôn bắt buộc.
- Người chơi hiểu nguyên nhân Water Intrusion.
- Passive Task hoạt động khi rời Shelter và trong Sleep.
- Ground Floor Loss không luôn dẫn tới Game Over.
- Power Allocation tạo lựa chọn thực.

---

## 7.8. Redesign Trigger

- Người chơi chỉ đứng chờ Task.
- Pump giải quyết toàn bộ Flood.
- Elevated Storage bảo vệ quá nhiều.
- Power không tạo đánh đổi.
- Shelter Event chỉ là repair spam.

---

# 8. P4 — Disaster Vertical Slice

## 8.1. Mục tiêu

Kiểm chứng toàn bộ trải nghiệm Chapter trong phiên rút gọn.

---

## 8.2. Thời lượng

```text
60–90 phút
```

---

## 8.3. Phạm vi

- Main Shelter.
- Temporary Shelter đơn giản.
- Ba Location:

  - Cửa hàng tiện lợi.
  - Gara điện nước.
  - Trường học.

- Hai Route chính.
- Một Shortcut đơn giản.
- Một NPC.
- Bốn Disaster Phase:

  - Warning.
  - Black Rain.
  - Escalation.
  - Peak.

- Sáu Event.
- Ba Outcome.

---

## 8.4. Hệ thống

- World Clock.
- Sleep.
- Inventory.
- Search.
- Flood.
- Contamination.
- Shelter Module.
- Power.
- NPC Task.
- Event Deadline.
- Route Closure.
- Outcome Report.
- Save và Load.

---

## 8.5. Main Event

- Storm Warning.
- Black Rain Transition.
- School Rescue.
- Grid Failure.
- Drain Backflow.
- Pump Jam hoặc Storage Flood.

---

## 8.6. Giả thuyết

- Các hệ thống kết hợp thành một Core Loop rõ.
- Người chơi có thể lập kế hoạch và điều chỉnh.
- Peak kiểm tra chuẩn bị.
- Outcome Report giải thích kết quả.
- Người chơi muốn chơi lại theo chiến lược khác.

---

## 8.7. Exit Criteria

- Vertical Slice có thể hoàn thành từ đầu tới cuối.
- Có ít nhất ba Outcome khả thi.
- Có ít nhất hai chiến lược Shelter.
- Một lượt không thể hoàn thành mọi mục tiêu.
- Event Deadline công bằng.
- Không softlock.
- Save và Load giữ đúng State.
- Ít nhất `60%` tester muốn thử chiến lược khác.

---

## 8.8. Scope Cut

Nếu P4 quá lớn, cắt theo thứ tự:

1. Shortcut.
2. Temporary Shelter nâng cấp.
3. NPC Expedition Support.
4. Optional Event.
5. Signal Narrative.
6. Advanced Contamination.

Không cắt:

- World Clock.
- Search.
- Inventory Decision.
- Flood Route Change.
- Shelter Preparation.
- Peak.
- Outcome.

---

# 9. P5 — Full MVP Production

## 9.1. Mục tiêu

Mở rộng Vertical Slice thành toàn bộ nội dung MVP.

---

## 9.2. Content Scope

- Bảy Location.
- Ba Route.
- Một Shortcut.
- Main Shelter hoàn chỉnh.
- Temporary Shelter.
- Bốn NPC.
- 14 Main Event.
- 8–12 Optional Event.
- Bảy Disaster Phase.
- Năm Outcome.

---

## 9.3. Production Order

### Step 1 — Data Foundation

- Definition Data.
- Validation.
- Save Version.
- Debug Tool.

### Step 2 — Main Shelter

- Zone.
- Core Component.
- Module.
- Event Anchor.

### Step 3 — Location Production

Thứ tự:

1. Khu nhà dân.
2. Cửa hàng tiện lợi.
3. Gara điện nước.
4. Trường học.
5. Hiệu thuốc.
6. Trạm bơm.
7. Trạm thời tiết.

### Step 4 — NPC

1. Minh.
2. Mai.
3. Hùng.
4. An.

### Step 5 — Event

- Main Event trước.
- Optional Event sau.
- Narrative Hook cuối.

### Step 6 — Balance

- Time.
- Resource.
- Module.
- Hazard.
- Outcome.

---

# 10. P6 — Integration and Balance

## 10.1. Mục tiêu

Ổn định toàn bộ Chapter.

---

## 10.2. Test Matrix

### Strategy

- Resource First.
- Shelter First.
- Information First.
- NPC Rescue.
- Minimal Preparation.
- Forced Evacuation.

### Player State

- Healthy.
- Injured.
- High Fatigue.
- High Exposure.
- Overloaded.

### Shelter State

- Pump Strategy.
- Storage Strategy.
- Communication Strategy.
- No Generator.
- Lower Floor Lost.

---

## 10.3. Required Playthrough

Tối thiểu:

```text
20 internal playthrough
+
10 external playthrough
```

trước khi khóa Balance baseline.

Không cần mỗi tester hoàn thành mọi chiến lược.

---

## 10.4. Bug Priority

### Blocker

- Không thể hoàn thành Chapter.
- Save bị hỏng.
- Event không thể resolve.
- Route softlock.
- Item duplication.
- World Clock sai.

### Critical

- Outcome sai.
- Resource bắt buộc không xuất hiện.
- NPC State sai.
- Sleep bỏ qua Event.
- Shelter Module không cập nhật.

### Major

- Balance nghiêm trọng.
- UI không hiển thị cảnh báo.
- Location State không đồng bộ.

---

# 11. P7 — Release Candidate

## 11.1. Mục tiêu

Xác nhận MVP đạt tiêu chí phát hành thử nghiệm.

---

## 11.2. Yêu cầu

- Chapter hoàn thành ổn định.
- Không còn Blocker.
- Save tương thích trong cùng Release Candidate.
- Tutorial không giới thiệu hệ thống trong Peak.
- Performance đạt mục tiêu.
- Outcome Report chính xác.
- Credits và legal asset hoàn chỉnh.
- Setting cơ bản hoạt động.

---

# 12. Prototype Asset Policy

## P0–P3

- Primitive.
- Placeholder.
- Debug UI.
- Text tạm.

## P4

- Blockout có lighting.
- Âm thanh Hazard cơ bản.
- UI có hierarchy rõ.
- Một số asset đại diện.

## P5 trở đi

- Bắt đầu art production.
- Không thay Core Layout nếu không có lý do gameplay.
- Asset ưu tiên theo thời gian xuất hiện và tần suất sử dụng.

---

# 13. Audio Prototype

Âm thanh cần kiểm chứng sớm cho:

- Rain Intensity.
- Drain Backflow.
- Pump State.
- Electrical Hazard.
- Structural Warning.
- Event Alert.
- Radio Interference.

Một số Hazard cần âm thanh để cảnh báo công bằng.

---

# 14. UI Prototype

UI ưu tiên:

1. World Time.
2. Player Condition.
3. Carry Load.
4. Event Deadline.
5. Route Hazard.
6. Shelter Warning.
7. Power Allocation.
8. Intel Age.

Không hoàn thiện art UI trước khi information hierarchy được test.

---

# 15. Technical Milestone

## Foundation Complete

- World Clock.
- Runtime State.
- Save.
- Definition Data.
- Command Layer.
- Simulation Tick.

## Gameplay Complete

- Exploration.
- Inventory.
- Hazard.
- Shelter.
- Event.
- NPC.
- Outcome.

## Content Complete

- Location.
- Event.
- Narrative.
- Balance Data.

## Release Complete

- Optimization.
- Bug Fix.
- Accessibility.
- Final UX.
- Packaging.

---

# 16. Risk Register

## Risk 1 — Real-time Task gây chờ đợi

Giảm rủi ro bằng:

- Task có thể gián đoạn.
- Nhiều Task song song.
- Passive Machine.
- Task Duration ngắn có chủ đích.
- Shelter luôn có nhiều lựa chọn.

---

## Risk 2 — World Clock quá nhanh

Dấu hiệu:

- Người chơi không đọc được UI.
- Không đủ thời gian lập kế hoạch.
- Route Closure xảy ra liên tục.

Giải pháp:

- Điều chỉnh tỷ lệ thời gian.
- Giảm Task Duration.
- Cải thiện Forecast.
- Không thêm Time Acceleration.

---

## Risk 3 — World Clock quá chậm

Dấu hiệu:

- Người chơi đứng chờ.
- Deadline không tạo áp lực.
- Có thể hoàn thành mọi mục tiêu.

Giải pháp:

- Tăng Time Cost hợp lý.
- Giảm Resource Availability.
- Tăng lựa chọn cạnh tranh.
- Không thêm Event ngẫu nhiên chỉ để lấp thời gian.

---

## Risk 4 — Shelter quá phức tạp

Giải pháp:

- Giới hạn Zone.
- Giới hạn Module.
- Dùng Build Slot.
- Không mô phỏng dây và ống chi tiết.

---

## Risk 5 — Flood chỉ là movement penalty

Giải pháp:

- Route Closure.
- Alternative Access.
- Item Contamination.
- Current Strength.
- Shelter Water Intrusion.
- Return Window.

---

## Risk 6 — NPC luôn là lựa chọn đúng

Giải pháp:

- Resource Consumption.
- Living Capacity.
- Event Chain.
- Injury.
- Task limitation.

---

## Risk 7 — Nội dung vượt phạm vi

Giải pháp:

- Giữ Content Budget.
- Không thêm Location trước P4.
- Optional Event chỉ sản xuất sau Main Event.
- Multiplayer chưa thuộc MVP.

---

# 17. Kill Criteria

Một hệ thống bị cắt hoặc thiết kế lại nếu sau hai vòng test:

- Không tạo quyết định.
- Không ảnh hưởng Core Loop.
- Yêu cầu Production Cost quá cao.
- Chức năng trùng hệ thống khác.
- Không thể giải thích rõ cho người chơi.
- Chỉ tạo thao tác lặp lại.

---

# 18. MVP Completion Criteria

MVP hoàn thành khi:

1. Một lượt chơi hoàn chỉnh kéo dài khoảng `5–8 giờ`.
2. Có thể hoàn thành bằng ít nhất ba chiến lược.
3. Người chơi không thể thu thập toàn bộ Resource.
4. Peak phản ánh chuẩn bị.
5. Có năm Outcome hoạt động.
6. Không có Failure lớn không cảnh báo.
7. Save và Load giữ nguyên World State.
8. Location Depletion tồn tại.
9. Event diễn ra ngoài màn hình.
10. Shelter có thể mất Ground Floor mà Chapter vẫn tiếp tục.
11. Forced Evacuation hoạt động.
12. Narrative Hook được mở bằng gameplay.
13. Không cần cứu toàn bộ NPC.
14. Không cần hoàn thành mọi Location.
15. Không có hệ thống ngoài MVP làm Blocker.

---

# 19. Thứ tự thực hiện trực tiếp

```text
1. Paper Simulation
2. World Clock và Runtime State
3. Exploration Loop
4. Inventory và Search
5. Flood Route Prototype
6. Player Condition
7. Shelter Water Intrusion
8. Module và Power
9. Event System
10. Disaster Vertical Slice
11. Save và Load
12. Location Production
13. NPC Production
14. Resource Balance
15. Full Chapter Integration
16. Polish và Release Candidate
```

---

# 20. Quyết định chốt

- Prototype được phát triển theo rủi ro, không theo thứ tự hệ thống hoàn chỉnh.
- P1 phải kiểm chứng Exploration trước khi xây Shelter lớn.
- P2 phải chứng minh Flood thay đổi quyết định.
- P3 phải chứng minh Shelter không phải menu chờ.
- P4 là mốc quyết định có tiếp tục Full MVP hay không.
- Không sản xuất toàn bộ nội dung trước khi P4 đạt Exit Criteria.
- Debug Tool, Save và Telemetry là một phần của kế hoạch prototype.
- Multiplayer, Combat chuyên sâu và Procedural Content không thuộc MVP.
