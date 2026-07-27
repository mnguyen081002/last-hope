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
| BL-P1-14 | Interaction System | Backlog | (KAN-28) |
| BL-P1-15 | Item System | Backlog | (KAN-29) |
| BL-P1-16 | Inventory | Backlog | (KAN-30) |
| BL-P1-17 | Search System | Backlog | (KAN-31) |
| BL-P1-18 | Shelter Storage | Backlog | (KAN-32) |
| BL-P1-19 | Route và Travel | Backlog | (KAN-33) |
| BL-P1-20 | Location: Cửa hàng tiện lợi (blockout) | Backlog | (KAN-34) |
| BL-P1-21 | Telemetry P1 | Backlog | (KAN-35) |
| BL-P1-22 | Playtest vòng P1 | Backlog | (KAN-36) |

**Gate P1:** chưa chạy.

---

## P2 — Flood and Hazard Loop

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S7 | Condition + Phase timeline | Backlog | |
| S8 | Hazard/Flood + Equipment + Travel risk | Backlog | |
| S9 | Shelter recovery + HUD + Scenario A-D | Backlog | |

**Gate P2:** chưa chạy.

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
