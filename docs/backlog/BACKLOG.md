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
| BL-P2-04 | Flood State | Verify | Dry/Shallow/Medium/Deep/Impassable trên **Route** (chưa làm Zone trong Location — không có nội dung nào cần) — test tự động, user cần tự xem qua Debug Panel |
| BL-P2-05 | Current Strength | Backlog | Rủi ro vượt dòng, Rope giảm rủi ro |
| BL-P2-06 | Black Water Exposure | Verify | Nguồn tăng Exposure qua hazard crossing xong (nối vào field trống từ P2-A); `contaminated_handling_exposure_gain` — chưa có action "xử lý đồ nhiễm bẩn" để dùng tới |
| BL-P2-07 | Electrified Water cục bộ | Backlog | Hazard Volume cục bộ, cảnh báo trước |
| BL-P2-08 | Route Closure | Backlog | Route đổi theo Phase/Clock, không softlock |
| BL-P2-09 | Disaster Phase rút gọn | Backlog | Dry → First Rain → Black Rain → Route Closure — cần trước khi nối `wet_gain_per_minute_in_rain` (đã giữ sẵn field ở P2-A) |

### P2-C — Equipment Protection (M3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P2-10 | Equipment Protection | Backlog | Items P2 đã có trong `items_p2.json` (jacket/boots/gloves/rope/dry_bag), chưa có hệ thống dùng `EquipSlot`/`Protection` |
| BL-P2-11 | Return Window UI | Backlog | World Map: travel time, ETA, phase risk |
| BL-P2-12 | Content P2 | Backlog | Route + Location thứ hai (cao/thấp cho Flood chọn) |
| BL-P2-13 | Test Scenario A–D | Backlog | 4 kịch bản theo prototype plan mục 6.6 |
| BL-P2-14 | Save Hazard State | Verify | `WorldState.Routes` dùng chung `WorldStateSerializer` sẵn có (giống Locations) — tự động sống qua save/load, chưa có test round-trip riêng cho Routes (test round-trip chung đã phủ Locations, cùng cơ chế) |

**Gate P2:** chưa chạy — xong P2-A (Condition Core) + phần Flood State của P2-B. Còn lại:
Current Strength (BL-P2-05), Electrified Water (BL-P2-07), Route Closure (BL-P2-08),
Disaster Phase (BL-P2-09) — **không có số trong `balance.json`**, chờ quyết định user (tự
đề xuất số hay để lại). P2-C (Equipment/Return Window/Content/Scenario) chưa bắt đầu.
Exit Criteria: đổi Route vì Flood (không phải ép script); Equipment thay đổi Loadout;
không Failure tức thời thiếu cảnh báo; Return Window dễ hiểu; Route Closure không softlock.

## Cần user verify Flood State (BL-P2-04/06, không chặn tiếp tục code)

F2 Debug Panel có mục Hazard mới — đổi flood state của `route_shelter_store` (route duy
nhất hiện có) rồi thử Travel:

1. Đặt Shallow/Medium/Deep — Stamina/Wet/Exposure có đổi ngay sau khi tới nơi không, thời
   gian di chuyển có tăng theo tier không (Deep phải lâu gấp đôi Dry).
2. Mang nặng (Overload Heavy) + đặt Deep cùng lúc — thời gian phải nhân dồn cả hai (loadFactor
   × floodTimeFactor), không phải chỉ tính cái lớn hơn.
3. Đặt Impassable rồi thử tương tác Travel Point — phải bị từ chối hoàn toàn, không đi được.

**P2-A đã user verify** (2026-07-27). Một chỉnh sửa sau verify: tốc độ Sick
(`sick_decay_per_minute`, trước là `sick_health_decay_per_long_tick`) đổi từ 0.5/10 phút
game sang 0.4/phút game (quy đổi từ "mỗi 30 giây thực" ở timescale mặc định ×5), và mở rộng
áp dụng cho cả Thirst/Hunger tăng theo, không chỉ Health giảm — theo yêu cầu user.

---

## P3 — Shelter Loop (S10–S13 → Gate P3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S10 | Shelter State + Water Intrusion + blockout | Backlog | |
| S11 | Build & Placement + Task System | Backlog | |
| S12 | Power + Water + Sleep Simulation | Backlog | |
| S13 | Event Framework lõi + 3 Shelter Event + 2-trong-3 | Backlog | |

**Gate P3:** chưa chạy.

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

Milestone tiếp theo: **P1-C** (BL-P1-14..22) — Interaction, Item, Inventory, Search, Storage,
Travel, Location blockout, Telemetry → Gate P1.
