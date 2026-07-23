# Last Hope — Backlog Tracker (Local)

Tracker tiến độ chính thức, thay thế Jira (đổi quyết định 2026-07-24). Mô tả chi tiết từng item nằm trong `docs/mvp-product-backlog.md` — file này chỉ theo dõi **trạng thái**.

Trạng thái theo đúng quy ước `docs/mvp-product-backlog.md` mục 2.4:

```
Backlog → Ready → In Progress → Verify → Done
```

Cập nhật file này mỗi khi bắt đầu/hoàn thành một item. Ghi chú Jira key cũ (`KAN-xx`) chỉ để tham chiếu lịch sử — không còn thao tác trên Jira nữa.

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
| BL-P1-01 | Project setup | Verify | (KAN-15) URP 17.5.0 + Input System 1.20.0 + Newtonsoft 3.2.1 + physics 3D resolve sạch; 8 asmdef dựng đúng dependency 1 chiều; build batchmode compile 0 lỗi |
| BL-P1-02 | Camera isometric | Verify | (KAN-16) `CameraRig.cs`: orthographic, pitch 35.264°/yaw 45° cố định, zoom qua Input System. Đã dựng trong scene, headless smoke test không lỗi. **Chưa xác nhận bằng mắt** (môi trường headless) — cần mở Editor kiểm tra góc nhìn/scale thực tế |
| BL-P1-03 | Input + movement | Verify | (KAN-17) `PlayerController.cs` + `GameControls.inputactions` (Move/Zoom/Interact). CharacterController framerate-độc lập, SpeedModifier hook sẵn cho Carry Load/Flood. Headless smoke test: Awake/OnEnable không lỗi. **Chưa test di chuyển thực tế bằng bàn phím** — cần Editor/Player có cửa sổ |
| BL-P1-04 | Logging + debug overlay | Verify | (KAN-18) `GameLog.cs` (Boot/World/Input/Save/Debug category) + `DebugOverlay.cs` (F1 toggle, FPS, vị trí). Log "[Boot] Boot started." xuất hiện đúng trong smoke test |
| BL-P1-05 | Build PC đầu tiên | Verify | (KAN-19) `BuildScript.cs` → `Builds/Windows/LastHope.exe`, build Succeeded, 0 lỗi, 3 warning không xác định nội dung. Headless smoke test: Boot → GamePersistent → 90_TestSystems load đủ 3 scene, không exception. **Chưa chạy có cửa sổ để nhìn hình** |

## P1-B — Technical Foundation (M1)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| BL-P1-06 | Definition Registry | Verify | (KAN-20) `DefinitionLoader`/`DefinitionRegistry` xong, gom toàn bộ lỗi (duplicate id, dangling ref). Test: 2/2 pass. Chưa có content JSON thật, chỉ fixture test |
| BL-P1-07 | Runtime World State | Verify | (KAN-21) `WorldState` + state con (Player/Inventory/ItemInstance) xong; Route/Location/Shelter/Npc/Event/Task chỉ stub id+status, mở rộng dần theo hệ thống tương ứng |
| BL-P1-08 | World Clock | Verify | (KAN-22) `SimulationClock` (1s thực=5s game, bank decimal chống lệch) + `GameTimeUtil` (anchor Day0 17:00). Test: 2/2 pass |
| BL-P1-09 | Simulation Tick | Verify | (KAN-23) `TickScheduler` (Short/Long tick, threshold hook, catch-up bounded, FastForward) + `SimulationDriver` chạy thật trong scene. Test: 5/5 pass |
| BL-P1-10 | Command Layer | Verify | (KAN-24) `IGameCommand`/`CommandProcessor`/`EventBus` + 7 command (UseItem, TransferItem đủ logic; StartSleep fast-forward; StartTask/CancelTask/BeginTravel chỉ validate+flag). Test: 2/2 pass |
| BL-P1-11 | Save Foundation | Verify | (KAN-25) `SaveFile`/`SaveService`: atomic write (tmp→verify→backup→rename), checksum SHA256, autosave round-robin 3 slot + manual, reject rõ ràng khi version/checksum sai. Test: 5/5 pass |
| BL-P1-12 | Debug Panel v1 | Verify | (KAN-26) `DebugPanel.cs` (F2): clock, pause/timescale, add item, save/load/autosave, state dump. Compile sạch + wiring scene xác nhận, chưa có test tự động (UI) |
| BL-P1-13 | Test Foundation | Verify | (KAN-27) Toàn bộ 19 EditMode test pass: Clock (2), Tick (5), Definition (2), RNG (3), Command (2), Save (5) |

**Gate M1: PASS (2026-07-24).** 19/19 EditMode test xanh; build Windows Development thành công (0 lỗi); headless smoke test 10s: boot → load definitions → World Clock/Tick chạy ngầm không exception. Chưa xác nhận bằng mắt (môi trường AI headless) — user nên tự mở Editor/build một lần để xem Debug Panel (F2) và Debug Overlay (F1) hiển thị đúng.

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

**Gate P1:** chưa đạt.

---

## Milestone tiếp theo (P2+)

Chưa breakdown — sẽ thêm bảng mới vào file này khi Gate P1 pass (theo `docs/mvp-product-backlog.md` mục 6 trở đi).
