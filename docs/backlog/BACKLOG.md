# Last Hope — Backlog Tracker (Local)

Tracker tiến độ chính thức, thay thế Jira (đổi quyết định 2026-07-24). Mô tả chi tiết từng item nằm trong `docs/mvp-product-backlog.md` — file này chỉ theo dõi **trạng thái**.

Trạng thái theo đúng quy ước `docs/mvp-product-backlog.md` mục 2.4:

```
Backlog → Ready → In Progress → Verify → Done
```

Cập nhật file này mỗi khi bắt đầu/hoàn thành một item. Ghi chú Jira key cũ (`KAN-xx`) chỉ để tham chiếu, không còn thao tác trên Jira.

---

## Ràng buộc khóa cứng — đọc trước khi implement bất cứ gì

**Game là 2D isometric, kiểu Project Zomboid.** Mọi sprint Presentation/EditorTools dựng 2D:
Tilemap Isometric, `SpriteRenderer` + `Collider2D`, `Rigidbody2D` kinematic, camera
orthographic không xoay + `transparencySortMode = CustomAxis`. Không dùng `Rigidbody`,
`CharacterController`, mesh, raycast occlusion.

Trước khi thiết kế placement: đọc `docs/00-project-overview/isometric-game-placement-rules.md`.
Chi tiết kỹ thuật: `docs/00-project-overview/technical-specification.md`.

## Hiện trạng

Chưa có code gameplay nào (`Assets/Game/**`, `Assets/Tests/**`, `Assets/Scenes/**` trống) —
mọi item dưới đây ở trạng thái `Backlog`. Mô tả chi tiết từng item: `docs/mvp-product-backlog.md`.

Sẵn có, dùng lại trực tiếp:

- `Assets/StreamingAssets/Definitions/` — 18 file JSON content + balance, `definition_version 0.14.0`.
- `Assets/Art/` — 743 PNG sprite (nhân vật 8 hướng, terrain, prop, loot).

Chi tiết: bảng trong `CODEMAP.md`.

---

## P0 — Paper Simulation

Bộ số baseline hiện hành nằm trong `Assets/StreamingAssets/Definitions/balance.json`. Chỉ
chạy P0 khi muốn kiểm chứng lại các số đó, không phải điều kiện tiên quyết để bắt đầu P1-A.

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P0-01 | Bảng mô phỏng kinh tế | Backlog | (KAN-10) |
| BL-P0-02 | Kịch bản chuẩn | Backlog | (KAN-11) |
| BL-P0-03 | Chạy mô phỏng đa chiến lược | Backlog | (KAN-12) |
| BL-P0-04 | Phân tích dominant strategy | Backlog | (KAN-13) |
| BL-P0-05 | Chốt baseline số liệu | Backlog | (KAN-14) — số hiện hành trong `balance.json` |

**Gate P0:** chưa chạy.

---

## P1-A — Project Foundation (M0)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-01 | Project setup | Done | 9 asmdef, cây folder, scene sinh bằng `SceneSetup` |
| BL-P1-02 | Camera isometric | Done | User xác nhận bằng mắt 2026-07-27 |
| BL-P1-03 | Input + movement | Done | User xác nhận; đã sửa lỗi đi lọt map (kinematic không tự chặn va chạm — `Rigidbody2D.Cast`) |
| BL-P1-04 | Logging + debug overlay | Done | User xác nhận F1; đã sửa cửa sổ mờ/lệch (`defaultIsNativeResolution`) |
| BL-P1-05 | Build PC đầu tiên | Done | Build Windows + smoke test headless pass (boot → persistent → test room) |

## P1-B — Technical Foundation (M1)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-06 | Definition Registry | Done | Đọc 18 file JSON thật, gom toàn bộ lỗi thay vì fail-first |
| BL-P1-07 | Runtime World State | Done | `WorldState` + Player/Location/SearchPoint/Inventory state |
| BL-P1-08 | World Clock | Done | `SimulationClock` bank phút nguyên, 24h không drift |
| BL-P1-09 | Simulation Tick | Done | `TickScheduler` — nơi duy nhất tăng `WorldTimeMinutes` |
| BL-P1-10 | Command Layer | Done | Pipeline + `UseItemCommand`. Command gameplay khác thêm ở S5-S6 |
| BL-P1-11 | Save Foundation | Done | Checksum SHA256, atomic write, .bak, autosave rotation 3 slot |
| BL-P1-12 | Debug Panel v1 | Done | User xác nhận F2 |
| BL-P1-13 | Test Foundation | Done | 51 EditMode test xanh |

**Gate M1: PASS** (2026-07-27) — 51/51 test tự động + user xác nhận bằng mắt (camera,
movement, tường biên, Y-sort, F1, F2).

## P1-C — Exploration Gameplay (M2)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-14 | Interaction System | Done | Hold+cancel qua `InteractionDetector`; user xác nhận |
| BL-P1-15 | Item System | Done | `ItemDefinition`/`ItemInstanceState` từ S2, dùng xuyên P1-C |
| BL-P1-16 | Inventory | Done | Overload/Carried Object — test tự động + user xác nhận |
| BL-P1-17 | Search System | Done | `SearchSystem`/`SearchPanel` — test tự động + user xác nhận |
| BL-P1-18 | Shelter Storage | Done | `StorageView`/`StoragePanel` — user xác nhận |
| BL-P1-19 | Route và Travel | Done | `TravelSystem`/`TravelPointView` — test tự động + user xác nhận |
| BL-P1-20 | Location: Cửa hàng tiện lợi (blockout) | Done | 6 search point khớp `searchpoints_p1.json` |
| BL-P1-21 | Telemetry P1 | Done | `TelemetryLogger` — JSONL `persistentDataPath/Telemetry`, event-driven qua EventBus |
| BL-P1-22 | Playtest vòng P1 | Done | User xác nhận 2026-07-27 |

**Gate P1: PASS** (2026-07-27) — 84 EditMode test + playtest thật của user.

Sau playtest, 2 chỉnh sửa UX theo góp ý user:
- Mọi panel (Inventory/Search/Storage) đóng được bằng **ESC** (action `Close`, đã có sẵn
  trong `GameControls.inputactions`, chỉ chưa dùng tới) hoặc **nhấn lại đúng phím/tương tác
  đã mở nó** (toggle) — không chỉ có nút "Đóng" trên UI.
- Search point **chỉ cần giữ phím "cạy" ở lần mở đầu tiên**; `SearchPointState.Rolled` đã
  `true` thì các lần tương tác sau mở tức thì (`SearchPointView.HoldDurationSeconds` trả 0
  nếu đã Rolled) — hợp lý vì thao tác khó chỉ xảy ra một lần, không phải mỗi lần quay lại.

---

## P2 — Flood and Hazard Loop

### P2-A — Player Condition (M3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P2-01 | Player Condition Core | Done | Health/Stamina/Fatigue/Hunger/Thirst/BodyTemp — user đã verify; `Injury` **chưa làm** (balance.json không có số) |
| BL-P2-02 | Status Effect | Done | Wet/Cold/BlackWaterExposure→Sick — user đã verify + chỉnh tốc độ Sick; `Bleeding`/`Disoriented` **chưa làm** (không có số trong balance.json) |
| BL-P2-03 | Condition UI debug | Done | Mục Condition trong F2 Debug Panel — user đã xem |

### P2-B — Hazard và Route State (M3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P2-04 | Flood State | Done | Dry/Shallow/Medium/Deep/Impassable trên **Route** (chưa làm Zone trong Location — không có nội dung nào cần) — user đã verify 2026-07-28 |
| BL-P2-05 | Current Strength | Done | User đã verify 2026-07-28, chấp nhận số hiện hành (sweep %/tier). Rope giảm rủi ro **chưa làm** (cần Equipment System P2-C thực sự mặc đồ) |
| BL-P2-06 | Black Water Exposure | Done | Nguồn tăng Exposure qua hazard crossing xong (nối vào field trống từ P2-A); user đã verify 2026-07-28. `contaminated_handling_exposure_gain` — chưa có action "xử lý đồ nhiễm bẩn" để dùng tới |
| BL-P2-07 | Electrified Water cục bộ | Done | User đã verify 2026-07-28, chấp nhận số hiện hành. Instant Hazard, set thủ công qua Debug Panel (chưa có nguồn hạ tầng tự động — Power/Grid thuộc P3) |
| BL-P2-08 | Route Closure | Done | User đã verify 2026-07-28, chấp nhận rủi ro softlock còn lại (xem ghi chú Scenario D bên dưới). `RouteDefinition.ClosesAtPhase` đè Flood thành Impassable theo Disaster Phase |
| BL-P2-09 | Disaster Phase rút gọn | Done | User đã verify 2026-07-28, chấp nhận mốc thời gian hiện hành. Dry → First Rain → Black Rain → Route Closure, suy thuần từ world time |

### P2-C — Equipment Protection (M3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P2-10 | Equipment Protection | Done | `EquipmentSystem` + `EquipItemCommand`/`UnequipItemCommand` — jacket giảm Wet, boots chặn/giảm Exposure, rope giảm Current index, dry_bag đổi capacity. User đã verify 2026-07-28. Gloves (`handles_contaminated`) **vẫn treo** — chưa có action xử lý đồ nhiễm bẩn (như đã ghi ở P2-B) |
| BL-P2-11 | Return Window UI | Done | Phạm vi rút gọn cho P2 (quyết định cùng user 2026-07-28): **không** dựng World Map đầy đủ (`docs/03-mvp-black-rain/03-black-rain-world-map.md` là phạm vi P4). Nhấn E ở `TravelPointView` mở `TravelConfirmPanel` — Travel Time một chiều, Estimated Return Time (khứ hồi), Known Hazard, cảnh báo nếu Disaster Phase dự kiến đổi trước khi quay lại. User đã verify 2026-07-28 |
| BL-P2-12 | Content P2 | Done | `location_garage` + `route_shelter_garage` — scene `42_Location_UtilityGarage` + `TravelPointView` thứ hai ở Shelter. `route_shelter_store` (thấp/ngắn) `closes_at_phase: route_closure`; `route_shelter_garage` (cao/dài, 35 phút) không set. User đã verify 2026-07-28 |
| BL-P2-13 | Test Scenario A–D | Done | Kịch bản playtest tay, script đầy đủ ở `docs/plans/2026-07-28-p2-test-scenarios.md`. User đã chạy hết A–D 2026-07-28, chấp nhận rủi ro còn lại ở Scenario D (xem ghi chú bên dưới) |
| BL-P2-14 | Save Hazard State | Done | `WorldState.Routes` dùng chung `WorldStateSerializer` sẵn có (giống Locations) — tự động sống qua save/load |

**Gate P2: PASS** (2026-07-28) — 166 EditMode test + user đã playtest toàn bộ P2-A/B/C
(Flood, Current, Exposure, Electrified, Route Closure, Disaster Phase, Equipment, Return
Window UI, Content P2, Scenario A–D).
Exit Criteria: đổi Route vì Flood ✅; Equipment thay đổi Loadout ✅; không Failure tức thời
thiếu cảnh báo ✅; Return Window dễ hiểu ✅ (phạm vi rút gọn); Route Closure không softlock
⚠ **rủi ro chấp nhận có kiểm soát** — xem ghi chú Scenario D bên dưới.

## Ghi chú rủi ro softlock từ Scenario D — chấp nhận, không chặn Gate

`location_convenience_store` chỉ nối **một** route (`route_shelter_store`) về shelter. Route
này có `closes_at_phase: route_closure`. Nếu player đang đứng ở cửa hàng đúng lúc world time
vượt mốc RouteClosure (900 phút = 15 tiếng), route đóng **vĩnh viễn** — hết đường về shelter
từ cửa hàng. `TravelConfirmPanel` (BL-P2-11) cảnh báo trước khi đi nếu khứ hồi dự kiến vượt
mốc đổi phase, nhưng không chặn — người chơi vẫn có thể bấm "Xác nhận đi".

**Quyết định 2026-07-28 (user):** chấp nhận rủi ro còn lại, không xử lý thêm cho P2 — buffer
15 tiếng game rất dài so với phiên chơi thật 30-45 phút nên khó gặp trong thực tế, và cảnh báo
đã có trước khi đi. Nếu muốn xử lý chặt hơn sau này (exempt chiều về khỏi Route Closure, chặn
hẳn thay vì chỉ cảnh báo) thì mở lại thành item riêng, không phải BL-P2-11.

## Fix kèm theo phát sinh khi user playtest P2 (không thuộc phạm vi gốc của item liên quan)

Content P2 (BL-P2-12): mỗi cổng ra vào ở Shelter giờ có `PlayerSpawnPoint` riêng, chọn theo
route vừa đi qua (`TravelStarted` event) — trước chỉ có 1 spawn cố định cho cả 2 cổng.

Equipment (BL-P2-10), 2026-07-27:

1. F2 Debug Panel cuộn chuột để xem mục "Túi đồ" (nơi gõ item id) đồng thời làm camera
   zoom theo — IMGUI (`OnGUI`) và Input System đọc scroll wheel là hai đường tách biệt,
   `Event.current` không chặn được Input System. Thêm `Core/UI/PointerOverUI.cs` (cờ dùng
   chung: panel OnGUI báo con trỏ có đang ở trong rect của nó không, `CameraRig` đọc để bỏ
   qua input zoom khi đang thao tác panel) — áp cho cả 4 panel OnGUI (Debug/Inventory/
   Search/Storage).
2. F2 Debug Panel cao cố định 760px — Game view nhỏ (Play trong Editor) bị cắt, không cuộn
   tới được mục "Túi đồ". Đã đổi thành co theo `Screen.height`.
3. F2 Debug Panel: thêm ô tìm kiếm + danh sách toàn bộ item (lọc theo tên, bấm "Thêm" trực
   tiếp) thay vì phải gõ tay đúng id — theo yêu cầu user.
4. `InventoryPanel`: thêm thanh progress hiển thị tải trọng (đầy tới **hard cap** 1.5×, không
   phải sức chứa gốc — thấy được còn bao nhiêu khoảng overload trước khi bị `Blocked`), màu
   theo `LoadTier`.
5. `UnequipItemCommand.Validate` trước chỉ kiểm tra slot có đồ, không kiểm tra tháo ra có tràn
   túi không — đã thêm `EquipmentSystem.CanUnequip` (kiểm tra thuần, không mutate) để từ chối
   đúng lúc (`NotEnoughCapacity`) thay vì để `Execute` âm thầm no-op, `InventoryPanel` hiện
   thông báo khi bị từ chối.

**P2-A đã user verify** (2026-07-27). Một chỉnh sửa sau verify: tốc độ Sick
(`sick_decay_per_minute`, trước là `sick_health_decay_per_long_tick`) đổi từ 0.5/10 phút
game sang 0.4/phút game (quy đổi từ "mỗi 30 giây thực" ở timescale mặc định ×5), và mở rộng
áp dụng cho cả Thirst/Hunger tăng theo, không chỉ Health giảm — theo yêu cầu user.

---

## P3 — Shelter Loop (S10–S13 → Gate P3)

Kế hoạch code: `docs/plans/2026-07-28-p3-shelter-loop.md` — **đọc mục "Phạm vi rút gọn có chủ
đích"** trước khi playtest, giải thích rõ vì sao không có cầu thang/multi-floor vật lý, Elevated
Storage không phải kho riêng, và mọi tương tác đi qua 1 prop "Shelter Console".

### P3-A — Shelter State và Build (M4)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P3-01 | Main Shelter blockout | Verify | 8 Zone (`shelterzones_p3.json`) hiển thị trong `ShelterPanel` (không dựng phòng riêng từng Zone). **Có Ground/Upper Floor vật lý thật kiểu Z-level Project Zomboid** (2 GameObject root cùng footprint, `FloorLevel`/`FloorRenderController`: tầng hiện tại rõ nét, tầng dưới hiện mờ không va chạm được, tầng trên ẩn hẳn) — đi qua vùng cầu thang (trigger, không bấm phím) là đổi tầng. 2026-07-28, dựng lại 2 lần sau khi user review — bản đầu chỉ SetActive nhị phân + bấm E, giờ mới đúng cảm giác PZ. Roof vẫn chỉ là logic (không module nào target zone `roof`) |
| BL-P3-02 | Shelter State | Verify | `ShelterState` — Structural Integrity, Water Intrusion, Clean/Untreated Water, Battery, Build Slots. Living Capacity/Occupants/Cleanliness/Security **chưa làm** (không có nội dung nào cần tới — không NPC, không multi-occupant trong P3) |
| BL-P3-03 | Build và Placement | Verify | Viết lại theo Free Placement (2026-07-28, `docs/plans/2026-07-28-free-placement.md`): `ShelterZoneDefinition` có world bounds thay Slot ID cố định, `BuildSystem.CanPlaceAt` validate world position + overlap (`ModuleDefinition.FootprintRadius`), `PlacementModeController` (Presentation) hiện ghost theo chuột + khung mờ biên Zone, click xác nhận. **Outdoor placement (Location ngoài trời) chưa làm** — không có Module Outdoor/Hybrid nào trong content để đặt (`modules_p3.json` cả 5 Module đều target Shelter), cần quyết định nội dung trước |
| BL-P3-04 | Task System | Verify | Gộp vào Construction (không dựng abstraction Active/Passive Task riêng) — chạy qua `TickScheduler.ShortTick` sẵn có nên tự "Passive" (tiếp tục dù rời Shelter/Sleep) |
| BL-P3-05 | Water Intrusion | Verify | `ShelterWaterSystem` — inflow theo Disaster Phase, Barrier giảm inflow + tự decay, Pump giảm, Ground Floor khóa (Pump/Purifier ngừng) khi `WaterIntrusion >= DeepThreshold`. Chưa test qua playtest |

### P3-B — Module và Power (M4)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P3-06 | Module: Flood Barrier | Verify | `module_barrier` — giảm inflow theo `barrier_block_fraction`, Durability tự decay mỗi Long Tick |
| BL-P3-07 | Module: Portable Pump | Verify | `module_pump` — cần Power, giảm Water Intrusion; Pump Jam (BL-P3-16) làm ngừng hoạt động tới khi sửa |
| BL-P3-08 | Module: Elevated Storage | Verify | **Đơn giản hoá có chủ đích** — không phải kho vật lý riêng, chỉ là modifier miễn nhiễm Storage Flood Risk Event cho `StorageContainer` hiện có |
| BL-P3-09 | Module: Water Purifier | Verify | `module_purifier` — batch Untreated→Clean theo `purify_batch_minutes`/`purify_batch_size`, Filter Durability giảm dần, ngừng ở 0% (chưa có action thay Filter mới — module coi như hỏng, phải Tháo/Xây lại) |
| BL-P3-10 | Module: Battery Bank | Verify | `module_battery_bank` — không có logic riêng, chỉ tồn tại để tốn Build Slot/vật liệu; Battery Charge là field chung `ShelterState.BatteryCharge` không phụ thuộc module này có được xây hay không (đơn giản hoá — chưa gate theo "phải có Battery Bank mới có chỗ chứa điện") |
| BL-P3-11 | Power System | Verify | `PowerSystem.Allocate` — Priority Critical→High→Normal→Disabled, Grid Supply theo Disaster Phase (Stable/Stable/Nửa/0), Battery xả/sạc phần dư-thiếu |
| BL-P3-12 | Water System | Verify | Gộp vào `ShelterWaterSystem`/`WaterBalance` — Water Intake thụ động, Purifier batch. Contamination **chưa làm** (không có action "xử lý đồ nhiễm bẩn" liên quan, tương tự ghi chú P2-B) |
| BL-P3-13 | Sleep Simulation | Verify | `SleepCommand` — FastForward qua `TickScheduler` (mọi hệ thống khác tick bình thường), cộng thêm hồi Fatigue theo `sleep_fatigue_recovery_per_hour`. Cũng nối `MinutesAtShelterContinuous` → Treat Black Water Exposure tại Shelter (field balance có sẵn từ P2-A, giờ mới dùng tới) |

### P3-C — Shelter Event và kiểm chứng (M4)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P3-14 | Event: Drain Backflow | Verify | Roll mỗi Long Tick ở Disaster Phase RouteClosure (~Peak/Escalation gộp). Giải quyết qua `ResolveDrainBackflowCommand` (tốn `drain_backflow_resolve_minutes`) |
| BL-P3-15 | Event: Storage Flood Risk | Verify | Kích hoạt khi Water Intrusion ≥ Critical + kho có đồ + chưa có Elevated Storage. Mỗi Long Tick có tỉ lệ mất 1 stack ngẫu nhiên |
| BL-P3-16 | Event: Pump Jam | Verify | Chỉ roll khi Pump đã xây + có điện + chưa kẹt. Giải quyết qua `RepairPumpJamCommand` (tốn `pump_jam_resolve_minutes`) |
| BL-P3-17 | Kịch bản 2-trong-3 | Verify | **Không cần content mới** — khan hiếm `item_wood`/`item_purifier_unit`/`item_filter` trong loot table sẵn có (P1/P2-C) đã tự nhiên tạo giới hạn "chỉ 2/3 Module chính", xác nhận lại khi user playtest |
| BL-P3-18 | Telemetry + Playtest P3 | Backlog | **Chưa nối Telemetry** cho Build/Power/Event (TelemetryLogger P1 chưa mở rộng) — cần quyết định có làm trong P3 hay để P4 trước khi Gate |

**Gate P3:** chưa chạy — toàn bộ BL-P3-01..17 đã có code (220 EditMode test), chờ user
playtest. BL-P3-18 (Telemetry riêng cho P3) **chưa làm** — cần quyết định phạm vi trước khi
đóng Gate. Outdoor placement (một phần BL-P3-03) chưa làm — chờ nội dung Module Outdoor.

## Cần user playtest Shelter Loop (P3)

Danh sách gộp toàn bộ hạng mục chưa playtest: `docs/backlog/NEED-USER-PLAYTEST.md`. Script
chi tiết từng bước: `docs/plans/2026-07-28-p3-test-scenarios.md` (Scenario A–H).

---

## P4 — Vertical Slice (S14–S18 → Gate GO/NO-GO)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S14 | Event Framework hoàn chỉnh + Event UI | Backlog | |
| S15 | Intel + World Map Intel + NPC nền | Backlog | |
| S16 | Nguyễn Minh đầy đủ + NPC pressure | Backlog | |
| S17 | Slice content: 4 phase + 3 location + route + 6 event | Backlog | |
| S18 | Outcome + Causal Report + Save full + Art tối thiểu | Backlog | |

**Gate P4:** chưa chạy.

---

Milestone tiếp theo: **chạy Gate P3** — toàn bộ P3 đã có code (BL-P3-01..17), chờ user
playtest theo `docs/plans/2026-07-28-p3-test-scenarios.md` rồi quyết định BL-P3-18
(Telemetry P3, còn Backlog). Sau đó mới sang **P4 — Vertical Slice**.
