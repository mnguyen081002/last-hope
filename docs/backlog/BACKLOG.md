# Last Hope — Backlog Tracker (Local)

Tracker tiến độ chính thức, thay thế Jira (đổi quyết định 2026-07-24). Mô tả chi tiết từng item nằm trong `docs/mvp-product-backlog.md` — file này chỉ theo dõi **trạng thái**.

Trạng thái theo đúng quy ước `docs/mvp-product-backlog.md` mục 2.4:

```
Backlog → Ready → In Progress → Verify → Done
```

Cập nhật file này mỗi khi bắt đầu/hoàn thành một item. Ghi chú Jira key cũ (`KAN-xx`) chỉ để tham chiếu lịch sử — không còn thao tác trên Jira nữa.

---

## Reset toàn bộ (2026-07-27)

Toàn bộ code game (`Assets/Game/**`, `Assets/Tests/**`, `Assets/Scenes/**` — 19 sprint, 287
EditMode test, Gate P4 từng PASS về mặt kỹ thuật) đã bị xóa theo yêu cầu của user, sau nhiều
vòng vá lỗi không dứt điểm ở S19 (chuyển 3D→2D). Lý do và phạm vi đầy đủ:
`docs/plans/2026-07-27-full-reset.md`.

Toàn bộ trạng thái bên dưới reset về **Backlog**. Mô tả chi tiết từng item vẫn đúng trong
`docs/mvp-product-backlog.md` (mô tả CÁI CẦN XÂY, không đổi). Implementation note cũ (file
nào, class nào, test nào) đã gỡ vì không còn đúng với code thực tế — muốn tra lại thiết kế
cũ đã làm thế nào, xem git history tại commit `128679e4fd1ffad051c43649a22967afc112ea8a`
(commit cuối trước khi xóa).

---

## P0 — Paper Simulation

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P0-01 | Bảng mô phỏng kinh tế | Backlog | (KAN-10) |
| BL-P0-02 | Kịch bản chuẩn | Backlog | (KAN-11) |
| BL-P0-03 | Chạy mô phỏng đa chiến lược | Backlog | (KAN-12) |
| BL-P0-04 | Phân tích dominant strategy | Backlog | (KAN-13) |
| BL-P0-05 | Chốt baseline số liệu | Backlog | (KAN-14) |

**Gate P0:** chưa chạy.

---

## P1-A — Project Foundation (M0)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-01 | Project setup | Backlog | (KAN-15) |
| BL-P1-02 | Camera isometric | Backlog | (KAN-16) |
| BL-P1-03 | Input + movement | Backlog | (KAN-17) |
| BL-P1-04 | Logging + debug overlay | Backlog | (KAN-18) |
| BL-P1-05 | Build PC đầu tiên | Backlog | (KAN-19) |

## P1-B — Technical Foundation (M1)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-06 | Definition Registry | Backlog | (KAN-20) |
| BL-P1-07 | Runtime World State | Backlog | (KAN-21) |
| BL-P1-08 | World Clock | Backlog | (KAN-22) |
| BL-P1-09 | Simulation Tick | Backlog | (KAN-23) |
| BL-P1-10 | Command Layer | Backlog | (KAN-24) |
| BL-P1-11 | Save Foundation | Backlog | (KAN-25) |
| BL-P1-12 | Debug Panel v1 | Backlog | (KAN-26) |
| BL-P1-13 | Test Foundation | Backlog | (KAN-27) |

**Gate M1:** chưa chạy.

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

## S19 — 2D isometric (kiểu Project Zomboid)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S19 | 2D isometric migration | Backlog | Quyết định vẫn giữ: art 2D (không có pipeline dựng asset 3D). Lần làm lại cần dựng Presentation/EditorTools đúng 2D ngay từ đầu (Tilemap Isometric, CustomAxis sort, Rigidbody2D) thay vì migrate từ 3D như lần trước — xem `docs/00-project-overview/isometric-game-placement-rules.md` trước khi thiết kế placement. |

---

Milestone tiếp theo: bắt đầu lại từ **P0** (nếu muốn chốt lại baseline số liệu trước khi
code) hoặc thẳng **P1-A** (nếu baseline P0 coi như vẫn hợp lệ từ lần làm trước) — cần user
quyết định trước khi bắt đầu.
