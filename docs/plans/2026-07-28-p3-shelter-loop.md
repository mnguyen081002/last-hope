# P3 — Shelter Loop (S10–S13 / BL-P3-01..18)

Kế hoạch code cho toàn bộ P3. Nguồn: `docs/mvp-product-backlog.md` mục 7,
`docs/03-mvp-black-rain/04-main-shelter-design.md`, `docs/03-mvp-black-rain/10-mvp-prototype-plan.md`
mục 7, `balance.json` (`shelter`/`power`/`water`), `modules_p3.json`, `shelterzones_p3.json`.

## Phạm vi rút gọn có chủ đích (so với full design doc 04)

`04-main-shelter-design.md` mô tả full MVP scope (6 Core Component vật lý, 3 tầng có cầu
thang thật, Elevated Storage là kho vật lý riêng...). Theo `isometric-game-placement-rules.md`
việc dựng cầu thang + floor-toggle thật là chi phí lớn. Giữ đúng tinh thần "blockout" đã dùng
cho P1-C (location chỉ là scene phẳng + prop tương tác, không phải building vật lý đầy đủ):

- ~~Không dựng multi-floor vật lý~~ **(đảo ngược 2026-07-28 sau khi user yêu cầu review lại)**
  — đã dựng Ground/Upper Floor thật: 2 GameObject root cùng chiếm 1 footprint world, chỉ 1
  active tại một thời điểm (`SetActive`), nối bằng `StaircaseView` (IInteractable, không dùng
  `TriggerCollider2D` — xem `isometric-game-placement-rules.md` mục 5, cũng được sửa lại
  trong đợt này để khớp thực tế code không dùng Tilemap). Roof vẫn chỉ là thuộc tính logic
  (`ShelterZoneDefinition.Floor`) vì không module nào target zone đó.
  **Giới hạn còn lại**: Save/Load không nhớ đang ở tầng nào trong Shelter (giống scope cut P1
  "Save/Load không đổi scene") — load lại luôn về Ground Floor.
- **Elevated Storage không phải kho vật lý riêng** — là modifier: có module này thì
  `StorageContainer` của shelter được miễn Storage Flood Risk event. Không thêm
  `InventoryOwner` mới, không thêm capacity riêng.
- **Task System = Construction** — không dựng abstraction Active/Passive Task tổng quát. Chỉ
  có một loại task cụ thể có số liệu (xây Module), chạy qua `TickScheduler` sẵn có nên tự động
  thoả "Passive Task chạy khi rời Shelter/Sleep" (world time chạy độc lập vị trí người chơi,
  đã đúng từ P1).
- **Drain Core không có 5 trạng thái riêng** — chỉ 1 cờ `DrainBackflowActive` (Event BL-P3-14
  bật/tắt).
- Toàn bộ tương tác qua **một** prop "Shelter Console" (giống Storage) mở `ShelterPanel` liệt
  kê tất cả Zone/Slot, không cần đi bộ tới từng Zone vật lý — khớp quyết định "OnGUI cho P1"
  đã áp dụng cho Inventory/Search/Storage/Travel.

## Số liệu còn thiếu trong `balance.json` — tự đề xuất (giống cách làm P2-B)

Thêm vào `balance.json.shelter` (ghi `_note_p3` disclaimer):
`storage_flood_loss_chance_percent`, `drain_backflow_trigger_chance_percent`,
`drain_backflow_resolve_minutes`, `pump_jam_chance_percent`, `pump_jam_resolve_minutes`,
`sleep_fatigue_recovery_per_hour`, `sleep_min_hours`, `sleep_max_hours`.

## Kiến trúc

**Data**: `ModuleDefinition`, `ShelterZoneDefinition` (Data/Definitions) khớp
`modules_p3.json`/`shelterzones_p3.json`. `BalanceDefinition` thêm `ShelterBalance`/
`PowerBalance`/`WaterBalance` đọc nhóm `shelter`/`power`/`water` (hiện chưa đọc). Registry +
Loader nạp 2 file (hiện đang bỏ qua có chủ đích).

**Core/State**: `WorldState.Shelter` (kiểu `ShelterState`, chỉ 1 shelter trong MVP) —
`StructuralIntegrity`, `WaterIntrusion`, `DrainBackflowActive`, `CleanWater`, `UntreatedWater`,
`BatteryCharge`, `PurifierBatchMinutes`, `PurifierFilterDurability`, `BuildSlots
(Dictionary<slotId, BuiltModuleState>)`, `Construction (ConstructionState?)`. `BuiltModuleState`:
`ModuleId`, `Durability`, `Priority (PowerPriority)`, `IsJammed`. `PlayerState` thêm
`MinutesAtShelterContinuous` (theo dõi để Treat Exposure — field `shelter_treat_exposure_minutes`
balance đã có sẵn từ P2-A, chưa dùng).

**Systems/Shelter/**: `PowerSystem` (allocate theo priority, grid supply theo phase, battery
charge/discharge), `ShelterWaterSystem` (Water Intrusion inflow/outflow + barrier decay +
Water Intake/Purifier batch), `BuildSystem` (validate/start/tick/complete/cancel/dismantle
construction), `ShelterEventSystem` (roll Drain Backflow/Storage Flood Risk/Pump Jam theo
chance balance, resolve qua command). `ShelterDriver` nối vào `TickScheduler.LongTick`, dựng
trong `GameServices.BindWorld` giống `ConditionDriver`.

**Commands**: `StartConstructionCommand`, `CancelConstructionCommand`,
`DismantleModuleCommand`, `SetPowerPriorityCommand`, `ResolveDrainBackflowCommand`,
`RepairPumpJamCommand`, `SleepCommand`.

**ConditionSystem**: mở rộng `UpdateSickFlag` — Exposure tụt dưới ngưỡng thì `IsSick` tự tắt
(comment cũ ghi "cần Shelter treat — P3"). `ConditionDriver`/`ShelterDriver` giảm
`BlackWaterExposure` khi `MinutesAtShelterContinuous >= ShelterTreatExposureMinutes`.

**UI**: `ShelterPanel` (OnGUI) — toàn bộ Zone/Slot, build/dismantle/power priority, Structural/
Water/Power/Water resource overview, banner Active Event + nút resolve. `SleepPanel` (OnGUI) —
chọn số giờ ngủ, xác nhận submit `SleepCommand`.

**Presentation**: `ShelterConsoleView`, `BedView` (2 prop mới trong `20_MainShelter`, giống
`StorageView`).

**DebugTools**: F2 thêm mục Shelter — cheat set Water Intrusion/Battery/trigger event thủ công
để test không phải chờ nhiều giờ game.

## Người dùng cần test gì (playtest 1 lần sau khi xong toàn bộ P3)

Script chi tiết: `docs/plans/2026-07-28-p3-test-scenarios.md` (6 Scenario A–F). Trọng tâm: xây
được tối đa 2/3 Module chính (Pump/Elevated Storage/Purifier) do khan hiếm vật liệu sẵn có;
Power Priority tạo đánh đổi thật; Ground Floor mất không kết thúc game; Storage Flood Risk
buộc chọn bảo vệ resource; Drain Backflow/Pump Jam kích hoạt và giải quyết được; Sleep phục
hồi Fatigue + chữa Black Water Exposure.
