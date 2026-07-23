# Last Hope — MVP Product Backlog

## Siêu Bão Mưa Đen (Black Rain)

---

## 1. Mục tiêu tài liệu

Tài liệu này chuyển trực tiếp đặc tả MVP Siêu Bão Mưa Đen thành backlog sản phẩm có thể thực thi.

Nguồn:

- `docs/03-mvp-black-rain/mvp-design-specification.md`
- `docs/03-mvp-black-rain/10-mvp-prototype-plan.md`
- `docs/mvp-implementation-plan.md`
- `docs/00-project-overview/mvp-scope.md`

Nguyên tắc chi phối toàn bộ backlog:

1. **Trình tự Milestone bám theo Prototype đã chốt: P0 → P7.**
2. **Không sản xuất content trước khi core loop được kiểm chứng.** Gate quyết định là Exit Criteria của P4.
3. Mỗi Milestone là một lát dọc chơi được, có câu hỏi kiểm chứng, có Exit Criteria và Redesign Trigger.
4. Save, Debug Tool và Telemetry là backlog item chính thức, không phải việc phụ.

---

## 2. Quy ước backlog

### 2.1. ID

```text
BL-<Milestone>-<Số thứ tự>
```

Ví dụ: `BL-P1-04` = item thứ 4 của Milestone P1.

Item xuyên suốt (cross-cutting) dùng prefix `BL-X-`.

### 2.2. Ưu tiên

Dùng lớp ưu tiên của Implementation Plan, đổi tên để tránh trùng ký hiệu Prototype:

| Lớp   | Tên gốc              | Ý nghĩa                                        |
| ----- | -------------------- | ---------------------------------------------- |
| PRI-0 | Blocker Foundation   | Không có thì không hệ thống nào chạy           |
| PRI-1 | Core Loop            | Trực tiếp tạo vòng lặp khám phá                |
| PRI-2 | Disaster Gameplay    | Hazard, Shelter, Power — gameplay thảm họa     |
| PRI-3 | Content Framework    | Event, Information, NPC, Outcome, Tutorial     |
| PRI-4 | Production và Polish | Content đầy đủ, art, audio, UI hoàn thiện      |

Quy tắc: **không làm item PRI-4 khi item PRI-0–PRI-2 tương ứng chưa đạt Exit Criteria.**

### 2.3. Kích thước

Ước lượng tương đối cho 1 developer:

- `S` ≤ 1 ngày.
- `M` = 2–3 ngày.
- `L` = 4–7 ngày.
- `XL` = cần tách nhỏ trước khi đưa vào sprint.

### 2.4. Trạng thái

```text
Backlog → Ready → In Progress → Verify → Done
```

Một item chỉ `Done` khi thỏa Definition of Done tương ứng (mục 13).

---

## 3. Mapping Prototype ↔ Implementation Milestone

Trình tự thực thi theo Prototype; các Milestone kỹ thuật M0–M9 được gắn vào đúng Prototype phục vụ nó.

| Prototype Milestone          | Implementation Milestone     | Câu hỏi kiểm chứng chính                                  |
| ---------------------------- | ---------------------------- | --------------------------------------------------------- |
| P0 — Paper Simulation        | (không cần code)             | Resource Economy và Time Budget có tạo quyết định không?  |
| P1 — Exploration Loop        | M0 Setup, M1 Foundation, M2  | Vòng lặp Search–Loot–Return có căng thẳng và đáng giá?    |
| P2 — Flood and Hazard Loop   | M3                           | Flood có thay đổi quyết định, không chỉ giảm tốc độ?      |
| P3 — Shelter Loop            | M4                           | Shelter có phải không gian quyết định, không phải menu?   |
| P4 — Disaster Vertical Slice | M5, M6 **(Go/No-Go Gate)**   | Toàn bộ hệ thống có hợp thành một Chapter đáng chơi lại?  |
| P5 — Full MVP Production     | M7                           | Content đầy đủ có giữ được chất lượng lát dọc không?      |
| P6 — Integration and Balance | M8                           | Ba chiến lược thắng có tồn tại thật không?                |
| P7 — Release Candidate       | M9                           | MVP có đạt tiêu chí phát hành thử nghiệm không?           |

Lưu ý phụ thuộc: P0 chạy **song song** với M0/M1 (P0 không cần code). Mọi Milestone từ P1 trở đi yêu cầu M1 Foundation đạt Exit Criteria.

---

## 4. Milestone P0 — Paper Simulation

**Câu hỏi kiểm chứng:** Với 12 giờ chuẩn bị, 3 Location, 2 Module, 1 NPC — người chơi có buộc phải từ bỏ mục tiêu và có ít nhất hai chiến lược sống sót không?

**Không cần engine. Chạy song song với BL-P1 nhóm Foundation.**

| ID       | Hạng mục                        | Mô tả                                                                                   | PRI   | Size |
| -------- | ------------------------------- | --------------------------------------------------------------------------------------- | ----- | ---- |
| BL-P0-01 | Bảng mô phỏng kinh tế           | Spreadsheet: World Clock, 3 Phase rút gọn, Water/Food/Fuel/Material, Travel Time, Build Cost, 1 Peak Check | PRI-0 | M    |
| BL-P0-02 | Kịch bản chuẩn                  | Dựng kịch bản 12h / 3 Location / 2 Module / 1 NPC theo prototype plan mục 4.3            | PRI-0 | S    |
| BL-P0-03 | Chạy mô phỏng đa chiến lược     | Chạy tối thiểu 5 lượt: Resource First, Shelter First, NPC Rescue, Information First, Minimal | PRI-0 | M    |
| BL-P0-04 | Phân tích dominant strategy     | Xác định Resource hoặc thứ tự hành động luôn tối ưu; đề xuất chỉnh số                    | PRI-0 | S    |
| BL-P0-05 | Chốt baseline số liệu           | Ghi baseline Resource Economy + Time Budget vào `08-black-rain-balance-framework.md`     | PRI-0 | S    |

**Exit Criteria (gate P0):**

- Có ít nhất ba chiến lược hợp lệ.
- Không có thứ tự hành động luôn tối ưu.
- Resource tối thiểu không phụ thuộc một nguồn duy nhất.
- Failure giải thích được bằng quyết định trước đó.

---

## 5. Milestone P1 — Exploration Loop

**Câu hỏi kiểm chứng:** Search thời gian thực + Weight/Volume + Travel Time có buộc người chơi bỏ lại đồ giá trị và ra quyết định loot thật không?

### 5.1. Epic P1-A — Project Foundation (M0)

| ID       | Hạng mục                  | Mô tả                                                                    | PRI   | Size |
| -------- | ------------------------- | ------------------------------------------------------------------------ | ----- | ---- |
| BL-P1-01 | Project setup             | Repo, branch strategy, Unity 6000.5.4f1, folder structure theo Implementation Plan mục 6 | PRI-0 | M    |
| BL-P1-02 | Camera isometric          | Orthographic, góc cố định, không xoay; chốt Scale chuẩn                  | PRI-0 | S    |
| BL-P1-03 | Input + movement          | Nhân vật placeholder di chuyển đúng hướng màn hình trong test room       | PRI-0 | S    |
| BL-P1-04 | Logging + debug overlay   | Log foundation, overlay cơ bản, automated test foundation                | PRI-0 | S    |
| BL-P1-05 | Build PC đầu tiên         | Build chạy ổn định — Exit Criteria M0                                    | PRI-0 | S    |

### 5.2. Epic P1-B — Technical Foundation (M1)

| ID       | Hạng mục            | Mô tả                                                                                     | PRI   | Size |
| -------- | ------------------- | ----------------------------------------------------------------------------------------- | ----- | ---- |
| BL-P1-06 | Definition Registry | Load Definition Data, tra cứu ID, validate reference, phát hiện ID trùng, báo lỗi khi khởi động | PRI-0 | M    |
| BL-P1-07 | Runtime World State | Khởi tạo Chapter; Player/Location/Route/Shelter State; Persistent Flag                    | PRI-0 | M    |
| BL-P1-08 | World Clock         | 1 phút thực = 5 phút game; không phụ thuộc framerate; Day/Time of Day; Phase transition hook | PRI-0 | M    |
| BL-P1-09 | Simulation Tick     | Frame Update, Short Tick, Long Tick, Tick subscription                                    | PRI-0 | M    |
| BL-P1-10 | Command Layer       | MoveItem, StartTask, CancelTask, BeginTravel, StartSearch, StopSearch, StartSleep         | PRI-0 | M    |
| BL-P1-11 | Save Foundation     | Serialize/Load World State, Save Version, Autosave test, Random Seed giữ sau Load         | PRI-0 | L    |
| BL-P1-12 | Debug Panel v1      | Chỉnh Clock, Tick Speed, Add Item, View State, Save/Load                                  | PRI-0 | M    |
| BL-P1-13 | Test Foundation     | Unit test: Clock không lệch sau thời gian dài, Tick không chạy đôi, Save/Load giữ World Time, Seed ổn định | PRI-0 | M    |

### 5.3. Epic P1-C — Exploration Gameplay (M2)

Phạm vi content: 1 Main Shelter placeholder, 1 Route, 1 Cửa hàng tiện lợi prototype. Resource mẫu: Water, Food, Battery, 1 vật nặng, 1 vật cồng kềnh.

| ID       | Hạng mục              | Mô tả                                                                                    | PRI   | Size |
| -------- | --------------------- | ---------------------------------------------------------------------------------------- | ----- | ---- |
| BL-P1-14 | Interaction System    | Detect interactable, prompt, hold, cancel, validation                                    | PRI-1 | M    |
| BL-P1-15 | Item System           | Item Definition/Instance, Weight, Volume, Stack, Condition, Wet + Contamination field    | PRI-1 | M    |
| BL-P1-16 | Inventory             | Backpack, Equipment, Quick Slot, Transfer, Drop, Carried Object, Overload — không grid puzzle | PRI-1 | L    |
| BL-P1-17 | Search System         | Search Point, progress thời gian thực, progressive reveal, cancel giữa chừng, **persistent depletion** | PRI-1 | L    |
| BL-P1-18 | Shelter Storage       | Transfer Player ↔ Storage, Capacity, giữ nguyên Item State                               | PRI-1 | M    |
| BL-P1-19 | Route và Travel       | Travel transition tiêu thụ World Time thực, ảnh hưởng bởi Carry Load, Arrival State      | PRI-1 | M    |
| BL-P1-20 | Location: Cửa hàng tiện lợi (blockout) | Blockout + Search Point + Controlled Resource Placement theo kịch bản P1  | PRI-1 | M    |
| BL-P1-21 | Telemetry P1          | Search duration, item nhặt/bỏ lại, Carry Load lúc về, revisit, thời gian mở Inventory    | PRI-1 | S    |
| BL-P1-22 | Playtest vòng P1      | Chạy full slice: Chuẩn bị → Đi → Search → Loot Decision → Về → Cất → Save/Load           | PRI-1 | S    |

**Exit Criteria (gate P1):**

- Người chơi chủ động bỏ lại ít nhất một item giá trị.
- Search dừng giữa chừng vẫn hữu ích.
- Location không hồi loot sau Load; Item giữ Condition/Contamination.
- Một chuyến đi hoàn chỉnh không có thời gian chết kéo dài.
- Không cần Inventory Grid để tạo quyết định.

**Redesign Trigger:** người chơi luôn nhặt toàn bộ; Travel chỉ là chờ đợi; Inventory thao tác nhiều hơn quyết định; quay lại Location không có giá trị.

---

## 6. Milestone P2 — Flood and Hazard Loop

**Câu hỏi kiểm chứng:** Flood có thay đổi Route Decision không? Equipment Protection có đáng Carry Cost không? Black Water có tạo áp lực tích lũy dễ hiểu không?

Giữ toàn bộ P1, thêm: 2 Route, 2 Location, Disaster Phase rút gọn (Dry → First Rain → Black Rain → Route Closure), phiên test 30–45 phút.

### 6.1. Epic P2-A — Player Condition (M3)

| ID       | Hạng mục               | Mô tả                                                                       | PRI   | Size |
| -------- | ---------------------- | ---------------------------------------------------------------------------- | ----- | ---- |
| BL-P2-01 | Player Condition Core  | Health, Stamina, Fatigue, Hunger, Thirst, Body Temperature                   | PRI-2 | L    |
| BL-P2-02 | Status Effect          | Wet, Cold, Bleeding, Sick, Black Water Exposure, Disoriented + Incapacitation cơ bản | PRI-2 | M    |
| BL-P2-03 | Condition UI debug     | Hiển thị chỉ số + Status Effect trên debug UI theo hierarchy mục 14 prototype plan | PRI-2 | S    |

### 6.2. Epic P2-B — Hazard và Route State (M3)

| ID       | Hạng mục               | Mô tả                                                                        | PRI   | Size |
| -------- | ---------------------- | ----------------------------------------------------------------------------- | ----- | ---- |
| BL-P2-04 | Flood State            | Dry / Shallow / Medium / Deep / Impassable trên Route và Location Zone        | PRI-2 | M    |
| BL-P2-05 | Current Strength       | None → Critical; rủi ro khi vượt dòng; Rope giảm rủi ro                       | PRI-2 | M    |
| BL-P2-06 | Black Water Exposure   | Trạng thái nước 3 cấp (Clean Rain / Untreated / Black Water); Exposure tích lũy + xử lý tại Shelter | PRI-2 | L    |
| BL-P2-07 | Electrified Water cục bộ | Hazard Volume cục bộ, có cảnh báo trước, không kill tức thời                | PRI-2 | M    |
| BL-P2-08 | Route Closure          | Route đổi State theo Phase/Clock; đóng không tạo softlock, luôn có phương án  | PRI-2 | M    |
| BL-P2-09 | Disaster Phase rút gọn | Timeline Dry → First Rain → Black Rain → Route Closure điều khiển Hazard State | PRI-2 | M    |

### 6.3. Epic P2-C — Equipment Protection (M3)

| ID       | Hạng mục              | Mô tả                                                                | PRI   | Size |
| -------- | --------------------- | --------------------------------------------------------------------- | ----- | ---- |
| BL-P2-10 | Equipment Protection  | Áo mưa (giảm Wet, tăng weight, Durability), Ủng (giới hạn theo độ sâu), Găng tay, Rope, Ba lô chống nước | PRI-2 | L    |
| BL-P2-11 | Return Window UI      | World Map hiển thị Travel Time, Estimated Return Time, Phase Change Risk, Known Hazard | PRI-2 | M    |
| BL-P2-12 | Content P2            | Route thứ hai + Location thứ hai (blockout), khác biệt cao/thấp để Flood tạo lựa chọn | PRI-2 | M    |
| BL-P2-13 | Test Scenario A–D     | Dựng 4 scenario theo prototype plan mục 6.6 (route ngắn ngập / mang nặng qua Medium Flood / thiếu Equipment / Route đổi khi đang ở Location) | PRI-2 | M    |
| BL-P2-14 | Save Hazard State     | Hazard/Route/Condition State tồn tại sau Save và Load                 | PRI-2 | S    |

**Exit Criteria (gate P2):**

- Người chơi đổi Route vì Flood (không phải vì bị ép script).
- Equipment thay đổi quyết định Loadout.
- Không có Failure tức thời không cảnh báo; Exposure xử lý được tại Shelter.
- Return Window được hiểu; Route Closure không softlock.

**Redesign Trigger:** Flood chỉ là movement penalty; luôn chọn Route ngắn; Equipment quá mạnh hoặc vô nghĩa; Exposure khó hiểu.

---

## 7. Milestone P3 — Shelter Loop

**Câu hỏi kiểm chứng:** Shelter có phải không gian gameplay thật — nơi Build Choice, Power và Task cạnh tranh với Expedition Time — hay chỉ là menu chờ?

Kịch bản test: 6 giờ chuẩn bị → 6 giờ Peak. Người chơi chỉ hoàn thiện được **hai trong ba**: Pump, Elevated Storage, Water Purifier.

### 7.1. Epic P3-A — Shelter State và Build (M4)

| ID       | Hạng mục               | Mô tả                                                                          | PRI   | Size |
| -------- | ---------------------- | ------------------------------------------------------------------------------- | ----- | ---- |
| BL-P3-01 | Main Shelter blockout  | Ground Floor + Upper Floor, Entrance, Utility, Storage, Upper Safe Area; Fixed Core Component làm Event Anchor | PRI-2 | L    |
| BL-P3-02 | Shelter State          | Structural Integrity, Water Intrusion, Living Capacity, Storage State, Power State, Occupants | PRI-2 | L    |
| BL-P3-03 | Build và Placement     | Build Slot, placement validation, material delivery, construction progress, interruptible, dismantle cơ bản | PRI-2 | L    |
| BL-P3-04 | Task System            | Active Task, Passive Task, Pause/Resume/Cancel, Resource reservation             | PRI-2 | L    |
| BL-P3-05 | Water Intrusion        | Nước vào theo Phase + Drain Core; Ground Floor mất được mà game vẫn tiếp tục     | PRI-2 | M    |

### 7.2. Epic P3-B — Module và Power (M4)

| ID       | Hạng mục            | Mô tả                                                                     | PRI   | Size |
| -------- | ------------------- | -------------------------------------------------------------------------- | ----- | ---- |
| BL-P3-06 | Module: Flood Barrier | Bảo vệ 1 Entrance/Opening, Durability, không chặn Drain Core             | PRI-2 | M    |
| BL-P3-07 | Module: Portable Pump | Cần Power + Drain Output + bảo trì; có thể bị tắc (Pump Jam)             | PRI-2 | M    |
| BL-P3-08 | Module: Elevated Storage | Sức chứa thấp, chiếm Slot Upper Safe Area, không chứa Large Object    | PRI-2 | M    |
| BL-P3-09 | Module: Water Purifier | Xử lý Untreated Water (không xử lý Black Water trực tiếp), cần Power/nhiên liệu + Filter | PRI-2 | M    |
| BL-P3-10 | Module: Battery Bank | Điện dự phòng nhóm Module, dung lượng thấp, cần sạc trước Peak            | PRI-2 | M    |
| BL-P3-11 | Power System        | City Grid, Generator/Battery, Demand, Priority, Allocation                 | PRI-2 | L    |
| BL-P3-12 | Water System        | Clean/Untreated Water, Purification Batch, Filter, Contamination           | PRI-2 | M    |
| BL-P3-13 | Sleep Simulation    | Sleep validation, tick simulation khi ngủ, Event interruption, Resource consumption | PRI-2 | M    |

### 7.3. Epic P3-C — Shelter Event và kiểm chứng (M4)

| ID       | Hạng mục             | Mô tả                                                             | PRI   | Size |
| -------- | -------------------- | ------------------------------------------------------------------ | ----- | ---- |
| BL-P3-14 | Event: Drain Backflow | Drain Core chảy ngược trong Peak test                             | PRI-2 | M    |
| BL-P3-15 | Event: Storage Flood Risk | Storage tầng thấp bị đe dọa, buộc chọn Resource bảo vệ        | PRI-2 | M    |
| BL-P3-16 | Event: Pump Jam      | Chỉ khi có Pump; kiểm tra bảo trì                                  | PRI-2 | S    |
| BL-P3-17 | Kịch bản 2-trong-3   | Cân chỉnh Resource để chỉ hoàn thiện 2/3 Module chính              | PRI-2 | S    |
| BL-P3-18 | Telemetry + Playtest P3 | Đo Build Choice, Power Allocation, thời gian chờ Task; chạy 6h+6h | PRI-2 | S    |

**Exit Criteria (gate P3):**

- Ít nhất ba chiến lược Shelter hợp lệ; không Module nào luôn bắt buộc.
- Người chơi hiểu nguyên nhân Water Intrusion.
- Passive Task chạy khi rời Shelter và trong Sleep.
- Ground Floor Loss không luôn dẫn tới Game Over.
- Power Allocation tạo lựa chọn thực.

**Redesign Trigger:** người chơi chỉ đứng chờ Task; Pump giải quyết toàn bộ Flood; Elevated Storage bảo vệ quá nhiều; Shelter Event chỉ là repair spam.

---

## 8. Milestone P4 — Disaster Vertical Slice ⛔ GO/NO-GO GATE

**Câu hỏi kiểm chứng:** Các hệ thống có hợp thành một Core Loop rõ, Peak có kiểm tra chuẩn bị, và người chơi có muốn chơi lại theo chiến lược khác không?

Thời lượng slice: 60–90 phút. **Đây là mốc quyết định có sản xuất Full MVP hay không. Không content production hàng loạt trước khi P4 đạt Exit Criteria.**

### 8.1. Epic P4-A — Event System (M5)

| ID       | Hạng mục            | Mô tả                                                                              | PRI   | Size |
| -------- | ------------------- | ----------------------------------------------------------------------------------- | ----- | ---- |
| BL-P4-01 | Event Framework     | Definition, Instance, Trigger, Discovery, Soft/Hard Deadline, Resolution, Expiration, Persistent Consequence | PRI-3 | XL → tách | 
| BL-P4-02 | Event ngoài Scene   | Event kích hoạt và hết hạn khi người chơi vắng mặt; hoạt động trong Sleep Simulation | PRI-3 | M    |
| BL-P4-03 | Event UI            | Cảnh báo + Deadline hiển thị rõ, công bằng                                          | PRI-3 | M    |

### 8.2. Epic P4-B — Information System (M5)

| ID       | Hạng mục          | Mô tả                                                              | PRI   | Size |
| -------- | ----------------- | ------------------------------------------------------------------- | ----- | ---- |
| BL-P4-04 | Intel Record      | Intel + Confidence + Information Age; Intel có thể lỗi thời         | PRI-3 | M    |
| BL-P4-05 | World Map Intel   | Map chỉ hiển thị điều đã biết; marker, Forecast, Route/Event Intel  | PRI-3 | M    |

### 8.3. Epic P4-C — NPC đầu tiên (M5)

| ID       | Hạng mục           | Mô tả                                                                     | PRI   | Size |
| -------- | ------------------ | --------------------------------------------------------------------------- | ----- | ---- |
| BL-P4-06 | NPC Framework + Nguyễn Minh | Recruitment, Consumption, Trust đơn giản, Shelter Task, Expedition Support, Event Chain, Injury/Death State | PRI-3 | L    |
| BL-P4-07 | NPC Resource Pressure | NPC tiêu thụ tài nguyên và có chi phí thật (chống "NPC luôn đúng")       | PRI-3 | S    |

### 8.4. Epic P4-D — Vertical Slice Content (M6)

| ID       | Hạng mục              | Mô tả                                                                     | PRI   | Size |
| -------- | --------------------- | --------------------------------------------------------------------------- | ----- | ---- |
| BL-P4-08 | 4 Disaster Phase      | Warning → Black Rain → Escalation → Peak (+ Aftermath ngắn) trong 60–90 phút | PRI-3 | L    |
| BL-P4-09 | 3 Location slice      | Cửa hàng tiện lợi, Gara điện nước, Trường học (blockout có lighting)        | PRI-3 | L    |
| BL-P4-10 | 2 Route + 1 Shortcut  | Commercial Route, Residential Route, Shortcut đơn giản                      | PRI-3 | M    |
| BL-P4-11 | Temporary Shelter đơn giản | Tầng trên trường học, kích hoạt qua khảo sát + Basic Storage           | PRI-3 | M    |
| BL-P4-12 | 6 Main Event slice    | Storm Warning, Black Rain Transition, School Rescue, Grid Failure, Drain Backflow, Pump Jam / Storage Flood | PRI-3 | L    |
| BL-P4-13 | 3 Outcome + Report    | Stable Survival, Forced Evacuation, Collapse + Causal Outcome Report v1     | PRI-3 | M    |
| BL-P4-14 | Save/Load full slice  | Toàn bộ State (Event, NPC, Hazard, Shelter) giữ đúng qua Save/Load          | PRI-3 | M    |
| BL-P4-15 | Art P4 tối thiểu      | Blockout + lighting, âm thanh Hazard cơ bản (Rain, Drain, Pump, Alert), UI có hierarchy | PRI-4 | L    |
| BL-P4-16 | Playtest ngoài team   | Tổ chức tester ngoài; đo tỷ lệ muốn chơi lại chiến lược khác                | PRI-3 | M    |

**Exit Criteria (Go/No-Go Gate):**

- Slice hoàn thành từ đầu tới cuối, không softlock, Save/Load ổn định.
- Ít nhất ba Outcome khả thi; ít nhất hai chiến lược Shelter.
- Một lượt không thể hoàn thành mọi mục tiêu.
- Event Deadline công bằng; tester hiểu nguyên nhân Outcome.
- Ít nhất `60%` tester muốn thử chiến lược khác.

**Nếu không đạt: thiết kế lại, không chuyển sang P5.**

**Scope Cut nội bộ P4 (cắt theo thứ tự):** Shortcut → Temporary Shelter nâng cấp → NPC Expedition Support → Optional Event → Signal Narrative → Advanced Contamination. **Không cắt:** World Clock, Search, Inventory Decision, Flood Route Change, Shelter Preparation, Peak, Outcome.

---

## 9. Milestone P5 — Full MVP Production

**Điều kiện vào:** P4 đạt toàn bộ Exit Criteria.

Content Budget (khóa cứng, không mở rộng): 7 Location, 3 Route + 1 Shortcut, Main Shelter hoàn chỉnh (8 Zone, 6 Core Component, 7 Module), Temporary Shelter, 4 NPC, 14 Main Event, 8–12 Optional Event, 7 Disaster Phase, 5 Outcome.

### 9.1. Epic P5-A — Data Foundation hoàn thiện

| ID       | Hạng mục            | Mô tả                                                    | PRI   | Size |
| -------- | ------------------- | --------------------------------------------------------- | ----- | ---- |
| BL-P5-01 | Definition Data đầy đủ | Toàn bộ Item/Resource/Recipe/Location/Route/Hazard/Event/NPC/Phase/Module ở dạng data | PRI-3 | L    |
| BL-P5-02 | Validation + Save Version | Data validation tự động; save migration trong cùng version line | PRI-3 | M    |
| BL-P5-03 | Debug Tool mở rộng  | Chuyển Phase, kích hoạt Event, thay đổi Hazard, Teleport   | PRI-3 | M    |

### 9.2. Epic P5-B — Location Production (theo đúng thứ tự, mỗi Location đạt DoD trước khi làm cái tiếp theo)

| ID       | Location            | Nội dung chính                                            | PRI   | Size |
| -------- | ------------------- | ---------------------------------------------------------- | ----- | ---- |
| BL-P5-04 | 1. Khu nhà dân      | Tutorial, tài nguyên cơ bản, giới thiệu NPC, Return Hook mái nhà | PRI-4 | L    |
| BL-P5-05 | 2. Cửa hàng tiện lợi | Nâng từ slice: Delivery Storage, Opportunity Event        | PRI-4 | M    |
| BL-P5-06 | 3. Gara điện nước   | Nâng từ slice: Generator/Large Pump Component, Workshop Blueprint | PRI-4 | M    |
| BL-P5-07 | 4. Trường học       | Nâng từ slice: NPC Rescue đầy đủ, Temporary Shelter đầy đủ, Rescue Point mái | PRI-4 | L    |
| BL-P5-08 | 5. Hiệu thuốc       | Medicine, Pharmacy Storage cần chìa khóa/Tool, Electrified Water | PRI-4 | L    |
| BL-P5-09 | 6. Trạm bơm khu vực | Quyết định lớn: Khôi phục trạm (chậm ngập khu vực) OR tháo linh kiện (lợi Shelter, ngập nhanh hơn) | PRI-4 | L    |
| BL-P5-10 | 7. Trạm thời tiết   | Forecast chính xác, Signal Data, Narrative Hook; quyết định lấy thiết bị OR duy trì trạm | PRI-4 | L    |

### 9.3. Epic P5-C — NPC Production (thứ tự cố định)

| ID       | NPC                      | Vai trò                                                  | PRI   | Size |
| -------- | ------------------------ | --------------------------------------------------------- | ----- | ---- |
| BL-P5-11 | 1. Nguyễn Minh (hàng xóm) | Hoàn thiện từ P4: Construction skill, người thân cần cứu | PRI-4 | M    |
| BL-P5-12 | 2. Trần Mai (y tế)       | Treatment Option, thông tin Black Water, Medical Station  | PRI-4 | L    |
| BL-P5-13 | 3. Lê Hùng (kỹ thuật thoát nước) | Sửa Pump, Trạm bơm Intel, Pump Blueprint          | PRI-4 | L    |
| BL-P5-14 | 4. Phạm An (vận hành radio) | Communication Station, xác minh tín hiệu, Narrative Hook | PRI-4 | L    |

### 9.4. Epic P5-D — Event và Narrative Production (Main trước, Optional sau, Narrative Hook cuối)

| ID       | Hạng mục               | Mô tả                                                            | PRI   | Size |
| -------- | ---------------------- | ------------------------------------------------------------------ | ----- | ---- |
| BL-P5-15 | 14 Main Event          | Đủ Main Event Set theo design spec mục 26, phủ 7 Phase             | PRI-4 | XL → tách theo Phase | 
| BL-P5-16 | Shelter Critical Event | Drain Backflow, Pump Jam, Storage Flood đầy đủ nhánh               | PRI-4 | M    |
| BL-P5-17 | NPC Event Chain        | Chuỗi Event của 4 NPC (bao gồm người thân của Minh)                | PRI-4 | L    |
| BL-P5-18 | 8–12 Optional Event    | Chỉ sản xuất sau khi Main Event xong; theo Event Budget            | PRI-4 | L    |
| BL-P5-19 | Narrative Hook         | Signal Data, Revelation trạm thời tiết, Ending Hook (không giải thích nguồn gốc) | PRI-4 | M    |

### 9.5. Epic P5-E — Shelter và Disaster hoàn chỉnh

| ID       | Hạng mục                  | Mô tả                                                       | PRI   | Size |
| -------- | ------------------------- | ------------------------------------------------------------- | ----- | ---- |
| BL-P5-20 | 8 Zone + 7 Module đầy đủ  | Thêm Drying Station, Communication Station; đủ Build Slot theo bảng design spec mục 11 | PRI-4 | L    |
| BL-P5-21 | Forced Evacuation         | Mất Main Shelter → sơ tán sang Temporary Shelter như một Outcome hợp lệ | PRI-4 | L    |
| BL-P5-22 | 7 Disaster Phase đầy đủ   | Normal → Aftermath, đúng timeline 4 ngày in-game, World Map Transition theo bảng mục 18 | PRI-4 | L    |
| BL-P5-23 | 5 Outcome + Report đầy đủ | Exceptional/Stable/Barely/Forced Evacuation/Collapse + Causal Outcome Report | PRI-4 | M    |
| BL-P5-24 | Electromagnetic Interference đầy đủ | Information Pressure + Equipment Pressure + Narrative; không phá thiết bị không cảnh báo | PRI-4 | M    |
| BL-P5-25 | Tutorial Flow             | Dạy theo Phase (mục 31 design spec); **Peak không giới thiệu hệ thống mới** | PRI-4 | L    |

**Exit Criteria (gate P5):**

- Toàn bộ Chapter hoàn thành được; 7 Location, 4 NPC có Outcome, 14 Main Event, ≥8 Optional Event hoạt động.
- 5 Outcome tính đúng.
- Không Resource bắt buộc nào phụ thuộc một nguồn RNG duy nhất.

---

## 10. Milestone P6 — Integration and Balance

**Điều kiện vào:** P5 đạt Exit Criteria.

| ID       | Hạng mục              | Mô tả                                                                    | PRI   | Size |
| -------- | --------------------- | -------------------------------------------------------------------------- | ----- | ---- |
| BL-P6-01 | Balance pass 1        | Travel/Search Time, Carry Load, Water/Food, Fuel/Power                     | PRI-4 | L    |
| BL-P6-02 | Balance pass 2        | Module, Hazard, Event Deadline, NPC Consumption, Outcome Threshold         | PRI-4 | L    |
| BL-P6-03 | Test Matrix — Strategy | Chạy 6 hướng: Resource First, Shelter First, Information First, NPC Rescue, Minimal Preparation, Forced Evacuation | PRI-4 | L    |
| BL-P6-04 | Test Matrix — Player/Shelter State | Healthy/Injured/High Fatigue/High Exposure/Overloaded × Pump/Storage/Communication/No Generator/Lower Floor Lost | PRI-4 | L    |
| BL-P6-05 | Playtest tối thiểu    | 20 internal + 10 external playthrough trước khi khóa Balance baseline      | PRI-4 | XL (theo lịch) |
| BL-P6-06 | Bug fix theo Priority | Blocker (softlock, save hỏng, Clock sai, dup item) → Critical (Outcome sai, NPC State sai, Sleep bỏ Event) → Major | PRI-4 | liên tục |
| BL-P6-07 | Save migration fix    | Save tương thích trong cùng Release line                                   | PRI-4 | M    |

**Exit Criteria (gate P6):**

- Ít nhất ba chiến lược thắng.
- Không Module không-tutorial nào được xây trong trên `90%` lượt.
- Không Location tùy chọn nào bắt buộc trong phần lớn lượt thắng.
- Collapse không chủ yếu do thiếu thông tin.
- Resource cuối Chapter không dư thừa nghiêm trọng.
- Forced Evacuation là Outcome hợp lệ.

---

## 11. Milestone P7 — Release Candidate

**Điều kiện vào:** P6 đạt Exit Criteria, không còn Blocker mở.

| ID       | Hạng mục         | Mô tả                                                                      | PRI   | Size |
| -------- | ---------------- | ---------------------------------------------------------------------------- | ----- | ---- |
| BL-P7-01 | Art polish       | Thay placeholder trọng yếu, chuẩn hóa material, cleanup AI asset, LOD/collider, wall fade, VFX mưa-ngập, lighting theo Phase | PRI-4 | XL → tách | 
| BL-P7-02 | Audio đầy đủ     | Rain Layer, Drain Warning, Pump, Electrical Hazard, Structural Warning, Radio Interference, Event Alert, Ambient theo District | PRI-4 | L    |
| BL-P7-03 | UI/UX cuối       | Tutorial, Shelter Overview, World Map, Deadline, Intel Confidence, Inventory, Power Allocation, Outcome Report, Settings, Accessibility cơ bản | PRI-4 | XL → tách |
| BL-P7-04 | Technical polish | Performance, memory, loading, save corruption handling, crash logging, packaging, input remapping, resolution | PRI-4 | L    |
| BL-P7-05 | License audit    | Không asset AI nào chưa kiểm tra license và cleanup; Credits + legal        | PRI-4 | M    |
| BL-P7-06 | Release check    | Chạy toàn bộ Release Criteria (Implementation Plan mục 15) + MVP Completion Criteria 15 điểm (Prototype Plan mục 18) | PRI-4 | M    |

**Exit Criteria (Release):** không còn Blocker; Chapter hoàn thành ổn định; Save đáng tin cậy; Tutorial không dạy trong Peak; Outcome Report chính xác; performance đạt target.

---

## 12. Backlog xuyên suốt (Cross-cutting)

Các item này sống qua nhiều Milestone, được kiểm tra lại tại mỗi gate:

| ID      | Hạng mục            | Quy tắc                                                                          |
| ------- | ------------------- | --------------------------------------------------------------------------------- |
| BL-X-01 | Save System         | Tích hợp từ P1 (M1). Mỗi hệ thống mới phải serialize được ngay khi hoàn thành, không dồn về cuối |
| BL-X-02 | Debug Tool          | Xây cùng hệ thống, không chờ cuối. Mỗi hệ thống mới thêm mục điều khiển vào Debug Panel |
| BL-X-03 | Telemetry           | Mỗi Prototype có danh sách chỉ số đo riêng; telemetry là điều kiện DoD             |
| BL-X-04 | Test Pyramid        | Unit (Clock, Resource, Inventory, Power, Event condition, Outcome, Save) + Integration (Sleep+Event, Travel+Closure, Search+Save, Pump+Power Loss, NPC+Consumption, Storage+Contamination) |
| BL-X-05 | Data-driven guard   | Không hard-code Item/Location/Event/NPC/Phase/Module trong gameplay logic          |
| BL-X-06 | Multiplayer-ready data | World Clock/State/Shelter/Event State chung; Inventory/Condition/Exposure riêng — nhưng KHÔNG code networking trong MVP |
| BL-X-07 | Asset pipeline      | AI Concept → Blender Cleanup → Scale/Pivot → Collider → Engine → Isometric Review; chỉ chạy hàng loạt sau P4 |

---

## 13. Definition of Done (áp cho mọi item)

**Hệ thống:** Definition Data + Runtime State tồn tại; Command validate; Save/Load hoạt động; Debug Tool hỗ trợ; UI hiển thị trạng thái cần thiết; có test tự động phù hợp; có ít nhất một gameplay scenario dùng nó; Telemetry ghi; không softlock đã biết.

**Location:** Blockout + Entrance + Zone + Search Point + Controlled Placement hoàn chỉnh; Depletion lưu đúng; Hazard transition + Event Anchor + Return Hook + Alternative Access hoạt động; Save/Load đúng State; performance đạt.

**NPC:** Definition + Runtime State; Recruitment, Consumption, Skill, Trait, Shelter Task, Event Chain, Injury/Death, Persistent Outcome; Save/Load; không phụ thuộc RNG để sống/chết.

**Event:** Trigger + Discovery + Deadline hoạt động; Critical Event có ít nhất hai phản ứng hợp lý; Success/Failure/Expiration xử lý; chạy ngoài Scene và trong Sleep; UI cảnh báo rõ; Telemetry.

---

## 14. Quản trị phạm vi

### 14.1. Kill Criteria (mọi hệ thống)

Cắt hoặc thiết kế lại sau hai vòng test nếu: không tạo quyết định; không ảnh hưởng Core Loop; Production Cost quá cao; trùng chức năng hệ thống khác; không giải thích được cho người chơi; chỉ tạo thao tác lặp.

### 14.2. Scope Cut Order toàn dự án (khi vượt tiến độ)

1. Optional Event thứ chín trở đi.
2. Alternative NPC dialog.
3. Advanced Signal Narrative.
4. Trạm thời tiết nhiều biến thể.
5. Shortcut nâng cấp.
6. NPC Expedition AI nâng cao.
7. Crafting Recipe phụ.
8. Shelter Module ít dùng.
9. Temporary Shelter Upgrade phụ.

**Không bao giờ cắt:** World Clock, Exploration Loop, Inventory Decision, Flood Route Change, Shelter Preparation, Peak Phase, Save/Load, Outcome, Forced Evacuation tối thiểu.

### 14.3. Ngoài phạm vi (từ chối đưa vào backlog)

Combat chuyên sâu, Firearm, Vehicle, Fluid Simulation toàn bản đồ, Dynamic Destruction, xây Shelter từ đất trống, Full Multiplayer Networking, Faction System, Skill Tree, Procedural Map, Random Loot Respawn, Chapter 2, giải thích nguồn gốc Mưa Đen.

---

## 15. Quy trình vận hành backlog

1. **Gate Review cuối mỗi Milestone:** đối chiếu Exit Criteria; ghi kết quả (Pass / Redesign / Cut) vào tài liệu này. Không mở Milestone sau khi gate chưa Pass.
2. **Redesign Trigger kích hoạt:** dừng thêm item mới của Milestone đó, tạo item redesign, chạy lại vòng test.
3. **Item `XL` phải tách** thành item `S/M/L` trước khi vào sprint.
4. **Thứ tự trong Milestone:** PRI thấp hơn làm trước; item content luôn sau item hệ thống mà nó phụ thuộc.
5. **P4 là gate tài chính-phạm vi:** mọi việc sản xuất asset hàng loạt, art production và content P5 chỉ được lên lịch sau khi P4 Pass.
