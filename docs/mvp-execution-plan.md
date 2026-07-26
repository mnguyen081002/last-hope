# Last Hope — MVP Execution Plan

## Kế hoạch triển khai từ Product Backlog

Tài liệu này là kế hoạch thực thi trực tiếp của `docs/mvp-product-backlog.md`. Backlog trả lời "làm gì và tiêu chí gì"; tài liệu này trả lời "làm theo thứ tự nào, trong cấu trúc code nào, bắt đầu từ đâu".

---

## 1. Trạng thái xuất phát

Chưa có code gameplay: `Assets/Game/`, `Assets/Tests/`, `Assets/Scenes/` trống.

### Đã có sẵn, không dựng lại

- `Packages/` + `Library/` — toàn bộ package đã cài (URP 2D Renderer, Input System, Tilemap, Physics2D, 2D Sprite, Newtonsoft, Test Framework).
- `ProjectSettings/` — Unity 6000.5.4f1, đã cấu hình 2D.
- `Assets/Art/` — 743 PNG sprite (nhân vật 8 hướng, terrain, world prop, loot).
- `Assets/StreamingAssets/Definitions/` — 18 file JSON content + balance, `definition_version 0.14.0`.
- `Assets/Audio/` — placeholder. `Assets/Input/GameControls.inputactions`.
- `scripts/*.py` — tool xử lý art pipeline (phục vụ BL-X-07), không phải gameplay code.
- `docs/` — toàn bộ thiết kế.

### Kết luận

Code gameplay bắt đầu từ **con số không**, nhưng project setup (engine, package, art, content data) đã có sẵn — Milestone M0 rút gọn đáng kể (bỏ qua tạo project, cài package), và không phải viết lại Definition JSON.

**Ràng buộc render:** game là **2D isometric** (Tilemap Isometric, `SpriteRenderer` + `Collider2D`, `Rigidbody2D` kinematic, camera orthographic không xoay + Y-sort `CustomAxis`). Xem `docs/00-project-overview/isometric-game-placement-rules.md` trước khi thiết kế placement.

---

## 2. Cấu trúc code mới

### 2.1. Assembly Definition

Tách assembly ngay từ đầu để ép ranh giới phụ thuộc một chiều:

```text
LastHope.Core          — World Clock, Runtime State, Tick, Command Layer, Save.
                          Không phụ thuộc assembly nào khác. Không tham chiếu UnityEngine.UI.
LastHope.Data          — Definition Registry, Definition types, validation.
                          Phụ thuộc: không (Core tham chiếu Data, không ngược lại).
LastHope.Systems       — Inventory, Search, Travel, Condition, Hazard, Shelter, Task,
                          Power, Water, Event, Information, NPC, Outcome.
                          Phụ thuộc: Core, Data.
LastHope.Presentation  — Scene binding, interaction prompt, camera, VFX hook.
                          Phụ thuộc: Systems.
LastHope.UI            — HUD, Inventory UI, World Map, Debug Panel view.
                          Phụ thuộc: Systems (đọc State qua interface, không ghi trực tiếp).
LastHope.DebugTools    — Debug Panel logic, cheat command.
                          Phụ thuộc: Core, Data, Systems.
LastHope.Tests.EditMode / LastHope.Tests.PlayMode
```

Quy tắc cứng (từ backlog BL-X-05, nguyên tắc 4.3 Implementation Plan):

- Gameplay logic không đọc/ghi UI.
- Mọi State quan trọng nằm trong Runtime World State, không nằm trong Scene object.
- Mọi thay đổi State đi qua Command Layer.

### 2.2. Folder

```text
Assets/
  Game/
    Core/            (LastHope.Core)
    Data/            (LastHope.Data)
    Systems/         (LastHope.Systems)
    Presentation/    (LastHope.Presentation)
    UI/              (LastHope.UI)
    DebugTools/      (LastHope.DebugTools)
  GameData/
    Definitions/     (Item, Location, Route, Hazard, Event, NPC, Phase, Module — JSON/SO)
    Balance/
  Scenes/
    Boot.unity
    Shelters/
    Locations/
    Routes/
  Art/               (giữ nguyên)
  Audio/             (giữ nguyên)
  Tests/
    EditMode/
    PlayMode/
```

### 2.3. Định dạng Definition Data

Chốt trước khi viết Registry (quyết định cần khóa — Implementation Plan mục 3): **ScriptableObject cho authoring + serialize sang JSON cho Save/validation**, hoặc thuần JSON. Đề xuất: **thuần JSON trong `Assets/GameData/Definitions`** vì team 1 người, dễ diff trong git, dễ validate tự động, dễ cho Multiplayer sau này. Quyết định cuối ghi vào ADR ngắn trong `docs/00-project-overview/technical-specification.md`.

---

## 3. Lịch triển khai theo Sprint

Sprint = 1 tuần (team 1 dev). Thời lượng là baseline quản lý phạm vi, không phải cam kết.

### Giai đoạn A — Foundation + Exploration (P0 + P1) · ~6 tuần

| Sprint | Nội dung                                                                 | Backlog                  |
| ------ | ------------------------------------------------------------------------ | ------------------------ |
| S1     | **P0 Paper Simulation** (song song) + setup lại skeleton: folder, asmdef, Boot scene, camera isometric, input, movement, logging, build PC | BL-P0-01…05, BL-P1-01…05 |
| S2     | Definition Registry, Runtime World State                                 | BL-P1-06, BL-P1-07       |
| S3     | World Clock, Simulation Tick, Command Layer                              | BL-P1-08…10              |
| S4     | Save Foundation, Debug Panel v1, Unit test foundation → **Gate M1**: chạy 24h World Time không lỗi, Save/Load giữ đúng State | BL-P1-11…13              |
| S5     | Interaction, Item, Inventory                                             | BL-P1-14…16              |
| S6     | Search + depletion, Storage, Travel, Cửa hàng tiện lợi blockout, Telemetry → **Gate P1** playtest | BL-P1-17…22              |

Ghi chú S1: vì project setup đã tồn tại, phần M0 chỉ còn dựng lại skeleton code + xác nhận Scale chuẩn với art có sẵn trong `Assets/Art`.

**Gate P1** (chặn sang giai đoạn B): người chơi bỏ lại item giá trị; Search dừng giữa chừng vẫn hữu ích; depletion sống qua Save/Load; không thời gian chết kéo dài.

### Giai đoạn B — Flood and Hazard (P2) · ~3 tuần

| Sprint | Nội dung                                                        | Backlog            |
| ------ | --------------------------------------------------------------- | ------------------ |
| S7     | Player Condition + Status Effect + debug UI                     | BL-P2-01…03        |
| S8     | Flood State, Current, Black Water, Electrified Water, Route Closure, Phase rút gọn | BL-P2-04…09 |
| S9     | Equipment Protection, Return Window UI, content P2, Scenario A–D, Save Hazard → **Gate P2** | BL-P2-10…14 |

**Gate P2:** người chơi đổi Route vì Flood; Equipment đổi Loadout; không Failure không cảnh báo; không softlock.

### Giai đoạn C — Shelter (P3) · ~4 tuần

| Sprint | Nội dung                                                       | Backlog            |
| ------ | -------------------------------------------------------------- | ------------------ |
| S10    | Main Shelter blockout, Shelter State, Water Intrusion          | BL-P3-01, 02, 05   |
| S11    | Build/Placement, Task System                                   | BL-P3-03, 04       |
| S12    | 5 Module (Barrier, Pump, Elevated Storage, Purifier, Battery Bank), Power, Water System | BL-P3-06…12 |
| S13    | Sleep Simulation, 3 Shelter Event, kịch bản 2-trong-3, playtest 6h+6h → **Gate P3** | BL-P3-13…18 |

**Gate P3:** ba chiến lược Shelter hợp lệ; Passive Task chạy khi vắng mặt; mất Ground Floor không kết thúc game; Power tạo đánh đổi.

### Giai đoạn D — Vertical Slice (P4) · ~5 tuần ⛔ GO/NO-GO

| Sprint | Nội dung                                                      | Backlog            |
| ------ | ------------------------------------------------------------- | ------------------ |
| S14    | Event Framework (tách nhỏ BL-P4-01) + Event ngoài Scene + UI  | BL-P4-01…03        |
| S15    | Information System, NPC Nguyễn Minh                           | BL-P4-04…07        |
| S16    | 4 Disaster Phase, 3 Location slice, Route + Shortcut, Temporary Shelter | BL-P4-08…11 |
| S17    | 6 Main Event, 3 Outcome + Report, Save full slice, art P4 tối thiểu | BL-P4-12…15 |
| S18    | Playtest ngoài team, sửa theo kết quả → **Gate P4 Go/No-Go**  | BL-P4-16           |

**Gate P4 quyết định toàn dự án:** slice 60–90 phút hoàn chỉnh, ≥3 Outcome, ≥2 chiến lược, không thể làm hết mọi mục tiêu, ≥60% tester muốn chơi lại. **Không Pass → thiết kế lại, không sản xuất content.**

### Giai đoạn E — Production (P5 → P7) · chỉ mở sau khi P4 Pass

- **P5 Full MVP** (~8–12 tuần): theo đúng thứ tự sản xuất trong backlog mục 9 — Data Foundation → 7 Location tuần tự (mỗi cái đạt DoD mới làm tiếp) → 4 NPC tuần tự → Main Event → Optional Event → Narrative Hook → Forced Evacuation + Tutorial.
- **P6 Integration/Balance** (~4–6 tuần): 2 balance pass, Test Matrix 6 chiến lược × trạng thái, 20 internal + 10 external playthrough.
- **P7 Release Candidate** (~4–8 tuần): art/audio/UI polish, technical polish, license audit, release check.

Lịch chi tiết giai đoạn E sẽ được lập lại **sau Gate P4**, dựa trên số liệu thực tế của giai đoạn A–D.

---

## 4. Nhịp làm việc và kỷ luật gate

1. **Cuối mỗi sprint:** build chạy được + Save/Load hoạt động với mọi hệ thống mới. Không có sprint nào kết thúc với build đỏ.
2. **Cuối mỗi giai đoạn:** Gate Review đối chiếu Exit Criteria trong backlog; ghi Pass/Redesign vào backlog. Redesign Trigger kích hoạt → sprint kế tiếp là sprint redesign, không phải sprint tính năng mới.
3. **Mỗi hệ thống mới bắt buộc kèm:** mục Debug Panel, serialize Save, telemetry, test (DoD backlog mục 13).
4. **Content freeze trước P4:** không thêm Location, NPC, Event ngoài phạm vi slice cho tới khi Gate P4 Pass.

---

## 5. Việc bắt đầu ngay (Sprint 1)

1. Chạy P0 Paper Simulation trên spreadsheet (BL-P0-01…03) — tùy chọn, chỉ để kiểm chứng lại số trong `Assets/StreamingAssets/Definitions/balance.json`.
2. Dựng folder + asmdef theo mục 2.
3. Boot scene + camera orthographic 2D (Y-sort CustomAxis) + movement placeholder, xác nhận PPU/Scale với sprite trong `Assets/Art/Production`.
4. Logging + debug overlay + build PC đầu tiên.
5. Chốt và ghi ADR: định dạng Definition Data (đề xuất JSON), cách chia Scene, cách serialize Save.
