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
| BL-P1-14 | Interaction System | Verify | (KAN-28, S5) `IInteractable`/`InteractionDetector`/`InteractionPrompt`: OverlapSphere 1.6m + cursor raycast tiebreak, phím E tức thì. S6: nay có interactable thật trong scene (SearchPointView×6, ShelterStorageView, TravelPointView) |
| BL-P1-15 | Item System | Verify | (KAN-29, S5) `ItemDefinition` +TwoHandCarry; content thật `items_p1.json` (5 item). Test: ContentValidationTests pass |
| BL-P1-16 | Inventory | Verify | (KAN-30, S5) `InventoryRules`/`InventorySystem` (overload Normal/Light/Heavy, capacity hard-cap 150%), `InventoryPanel` UI (list + 2 thanh, nút Use; Drop chờ sau). Test: 8/8 InventoryRulesTests pass |
| BL-P1-17 | Search System | Verify | (KAN-31, S6; hybrid loot 2026-07-24) `SearchPointState` + `OpenSearchPointCommand`: roll MỘT LẦN qua stream "loot", còn lại nằm nguyên container qua save/load, không re-roll. Loot table hybrid: entry `guaranteed` luôn spawn (khoá floor tài nguyên), entry thường roll theo `chance` 0-100 (thay `weight` cũ vốn là knob chết — mọi entry đều luôn spawn bất kể weight). Test: 8/8 SearchPointTests pass |
| BL-P1-18 | Shelter Storage | Verify | (KAN-32, S6) owner `shelter_storage:<id>` lazy-create, không giới hạn dung lượng; `ContainerPanel` Take/Take All/Store hai chiều. Test: qua OwnerResolverTests |
| BL-P1-19 | Route và Travel | Verify | (KAN-33, S6) `BeginTravelCommand` đầy đủ: kiểm adjacency, loadFactor theo Overload, FastForward, đổi CurrentLocationId, publish TravelStarted/Completed; `SceneFlowController` load/unload scene theo `LocationDefinition.SceneName`. Test: 5/5 TravelTests pass; headless smoke xác nhận load `20_MainShelter` thành công |
| BL-P1-20 | Location: Cửa hàng tiện lợi (blockout) | Verify | (KAN-34, S6) `41_Location_ConvenienceStore` dựng bằng primitive: 6 SearchPointView đúng vị trí, TravelPointView, PlayerSpawnPoint |
| BL-P1-21 | Telemetry P1 | Verify | (KAN-35, S6) `TelemetryLogger` JSONL: travel_started/completed (kèm carry load lúc về), search_opened, item_collected. Test: TelemetryTests xác nhận đúng thứ tự event |
| BL-P1-22 | Playtest vòng P1 | Verify | (KAN-36) Chạy được qua test tự động (depletion-qua-save) + headless smoke; **playtest thật bằng mắt/tay chưa làm** (môi trường AI headless) |

**Gate P1: PASS về mặt kỹ thuật (2026-07-24).** 48/48 EditMode test xanh (bao gồm SearchPointTests, OwnerResolverTests, TravelTests, TelemetryTests); build Windows 0 lỗi; headless smoke 12s xác nhận `SceneFlowController` chuyển scene `20_MainShelter` thành công không exception.

**Còn thiếu để Gate P1 pass đầy đủ theo đúng nghĩa (cần user xác nhận bằng tay):** chưa có ai thực sự chơi qua chuyến Chuẩn bị→Đi→Search→Loot Decision→Về→Cất→Save/Load trong Editor/Player có cửa sổ để cảm nhận "có bỏ lại đồ giá trị không", "search dừng giữa chừng có hữu ích không". Test tự động chỉ xác nhận cơ chế đúng, không xác nhận trải nghiệm.

**Bug phát hiện qua playtest thật (2026-07-24), đã sửa:** nhân vật rơi ra khỏi map ngay khi vào game. Nguyên nhân: race condition giữa `PlayerAvatarSync` (ghi `PositionLocationId` mỗi frame) và `SceneFlowController` (chỉ đặt nhân vật vào spawn point khi `PositionLocationId != CurrentLocationId`) — `PlayerAvatarSync` ghi đè field này trước khi `SceneFlowController` kịp kiểm tra, khiến điều kiện luôn "đã khớp" và spawn-placement không bao giờ chạy; nhân vật kẹt ở toạ độ khởi tạo trong `10_GamePersistent` (scene không có sàn) → rơi mãi. Sửa: `PlayerAvatarSync` chỉ ghi toạ độ X/Y/Z liên tục, KHÔNG tự ý ghi `PositionLocationId` nữa — chỉ `SceneFlowController` (sau khi đặt vào spawn) mới được phép stamp field này. Verify: headless smoke test log `"placed player at spawn (0.00, 0.10, 0.00) for 'location_shelter'"`, 48/48 test vẫn pass.

### S5 — sửa 3 vấn đề user phát hiện (2026-07-24)
1. **Vị trí nhân vật giờ được lưu**: `PlayerState.PositionX/Y/Z/PositionLocationId` + `PlayerAvatarSync` (ghi mỗi frame, áp lại khi load qua `WorldStateReloaded`). Test: PlayerPositionSaveTests pass.
2. **DebugPanel Load giờ có picker**: liệt kê `SaveService.ListSlots()` thành nút bấm, giữ ô nhập tay làm fallback.
3. **Content JSON thật đã có**: `items_p1.json`, `locations_p1.json`, `routes_p1.json`, `searchpoints_p1.json`, `balance.json` — "Add Item" trong Debug Panel giờ dùng được với id thật (vd `item_water_bottle`).

**Sự cố phát sinh + đã sửa (S5):** TextMeshPro báo NullReferenceException lúc runtime vì thiếu TMP Essential Resources (chưa từng import) — đã import qua script `TmpSetup.cs` (batchmode, không dùng `-quit` vì ImportPackage là async).

**Sự cố phát sinh + đã sửa (S6):** `Tests.EditMode.asmdef` có `overrideReferences:true` nên liệt kê "Newtonsoft.Json" vào `references` không đủ (đó chỉ match asmdef khác theo tên, Newtonsoft là plugin DLL thuần) — phải thêm `"Newtonsoft.Json.dll"` vào `precompiledReferences`.

---

### Search hybrid loot (2026-07-24, sau Gate P1)

Đổi cơ chế loot table từ "mọi entry luôn spawn, chỉ quantity random" (weight vốn là knob chết) sang hybrid tường minh: `guaranteed:true` → luôn spawn (designer khoá floor tài nguyên sống còn, VD nước/đồ ăn); entry thường → roll theo `chance` 0-100 (đồ giá trị: pin, toolbox, container 20L). Không đổi state schema, không cần bump SaveVersion. Plan: `docs/plans/2026-07-24-search-hybrid-loot.md`. 52/52 EditMode test pass (4 test mới: guaranteed-always, chance-0-never, chance-100-always, chance-50-deterministic-per-seed).

**Cần user test bằng tay:** vào cửa hàng mở cả 6 điểm — kệ nước/đồ khô phải LUÔN có hàng; new game vài lần (seed khác) — pin/toolbox/container 20L lúc có lúc không.

## P2 — Flood and Hazard Loop

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S7 | Condition + Phase timeline | Verify | `PlayerConditionState`/`ConditionOps`/`ConditionSystem` (Health/Stamina/Fatigue/Hunger/Thirst/BodyTemp/StatusEffects/Exposures/Incapacitation); `DisasterPhaseDefinition`+`phases_p2.json`(4 phase)+`DisasterPhaseSystem` (resync trên construct và `WorldStateReloaded`); `UseItemCommand` áp `UseEffects` + chặn khi Incapacitated trừ item tag "medical"; `LocationDefinition.IsShelter` (mới) cho body-temp regen/wet-dry tại shelter. SaveVersion→2, manifest→0.3.0. Test: 80/80 EditMode pass (28 test mới: ConditionOps, DisasterPhaseSystem, ConditionSystem, UseItem UseEffects/Incapacitated) |
| S8 | Hazard/Flood + Equipment + Travel risk | Verify | `HazardRules` (flood/current level 0-4, 1 công thức dùng chung live+forecast), `TravelRules.EvaluateCrossing` (deterministic, chặn ở tier 4, còn lại là chi phí + cảnh báo), `ReturnWindowCalculator` (sample 10 phút/bước), `EquipmentRules` (đọc Protection của item đang equip: rope giảm current, jacket giảm wet, boots chặn/giảm exposure, gloves miễn exposure khi cầm đồ bẩn); `EquipItemCommand`/`UnequipItemCommand` (slot body/feet/hands/back/tool, dùng `InventoryState.EquipmentSlots` có sẵn); `HazardSystem` (cache RouteState mỗi long-tick, hiển thị — không dùng để validate); `BeginTravelCommand` validate Passable+Incapacitated, áp cost sau FastForward; `TransferItemCommand` cầm đồ contaminated không gloves → +exposure (cost-not-block); `WorldMapPanel` (phím M) liệt kê route nối từ vị trí hiện tại kèm ETA/flood/current/return-window; `TravelPointView` đổi từ submit travel thẳng sang publish `WorldMapRequested`; `InventoryRules.EffectiveCapacity` (dry bag override 10kg/18L). `items_p2.json` (jacket/boots/gloves/rope/dry_bag/medkit). SaveVersion→3, manifest→0.4.0. Test: 116/116 EditMode pass (36 test mới: HazardRules, TravelRules, ReturnWindowCalculator, EquipmentRules, EquipItem/Unequip, dry bag capacity, contaminated handling, Travel hazard integration) |
| S9 | Shelter recovery + HUD + Scenario A-D | Verify | `RestAtShelterCommand` (mode Rest/TreatExposure/DryOff — Treat tiêu thụ 1 item tag "medical", bật cờ `TreatingExposure` cho `ConditionSystem` decay exposure −5/long-tick trong phiên FastForward 60'; DryOff tức thì, không tốn giờ); `ConditionOps.ApplyItemUseEffects` (rút ra dùng chung giữa UseItemCommand và RestAtShelterCommand); `ConditionHud` (HUD luôn hiện: 4 thanh Health/Stamina/Hunger/Thirst + badge status); fix quan trọng: rain-wet ambient (ConditionSystem ShortTick) giờ áp `EquipmentRules` wet_multiplier của jacket — trước S9 jacket chỉ có tác dụng lúc crossing route, không có tác dụng khi đứng ngoài mưa; DebugPanel +Rest/Treat/DryOff cheat +phase-jump cheat. `ScenarioTests.cs` 6 test (A: dry chỉ đói/khát; B×2: mưa không jacket → wet→cold, có jacket → không; C×2: qua Medium không boots → exposure→sick, có boots+gloves → sạch; D: lỡ return window → route Impassable, route cao hơn vẫn đi được). balance.json bổ sung section `hazard` (thiếu sót từ S8, giờ mới ghi tường minh — giá trị khớp default cũ, hành vi không đổi). SaveVersion→4, manifest→0.5.0. Test: **127/127 EditMode pass** (11 test mới: RestAtShelterCommandTests 5 + ScenarioTests 6) |

**Gate P2: PASS (2026-07-24, user xác nhận bằng tay sau fix lifecycle bug).** 127/127 EditMode test (Condition, Disaster Phase, Hazard, Travel Rules, Equipment, Scenario A-D); build Windows 0 lỗi; user tự chơi thử xác nhận travel cost theo tier hazard, Debug Panel dùng được sau fix scroll-clip.

Milestone tiếp theo: **P3 — Shelter Loop (S10–S13)**, plan chi tiết ở `docs/plans/2026-07-24-p3-p4-completion-plan.md`. Bắt đầu thực hiện 2026-07-24.

## P3 — Shelter Loop (S10–S13 → Gate P3)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S10 | Shelter State + Water Intrusion + blockout | Verify | `ShelterZoneDefinition`+`shelterzones_p3.json` (8 zone, 13 build slot, main-shelter-design.md §6-14); `ShelterState` mở rộng (StructuralIntegrity/WaterIntrusion/LivingCapacity/Occupants/BuildSlots/WaterStocks/EventFlags — PowerState/Modules chờ S12/S11); `WaterIntrusionRules` (pure, inflow theo `RainIntensity` phase − passive drain − pump, pump/backflow luôn 0/false tới S11-S13 nối); `WaterIntrusionSystem` (LongTick, seed+ensure-slots idempotent qua Resync, publish `ShelterWaterChanged` chỉ khi Level đổi, flag `lower_floor_power_locked`/`ground_floor_lost` tại Deep/Critical, tự xoá khi hồi phục); scene `20_MainShelter` dựng lại 2 tầng (6 zone Ground + 2 zone Upper, ramp tự tính góc, 6 Core Component trong đó DrainCore có tương tác đọc water intrusion); DebugPanel +Shelter section (view + cheat add water); balance.json +section `shelter`; manifest→0.6.0, SaveVersion→5 (không nhảy lên 6 như plan gốc — theo đúng version thật hiện tại là 4, +1 tuần tự). Test: **149/149 EditMode pass** (22 test mới: WaterIntrusionRulesTests 7 + WaterIntrusionSystemTests 6 + ContentValidationTests/SaveRoundTripTests cập nhật); build Windows 0 lỗi 1 warning; headless smoke xác nhận boot sạch + `SceneFlowController` load `20_MainShelter` mới không exception. **Chưa xác nhận bằng mắt** (ramp lên Upper Floor leo được không, zone/label đọc được không) — cần user tự mở Editor/build |
| S11 | Build & Placement + Task System | Verify | `ModuleDefinition`+`modules_p3.json` (5 module: Barrier/Pump/Elevated Storage/Purifier/Battery Bank, cost+build time theo bảng baseline plan); `items_p3_materials.json` (wood/scrap/pump_part/purifier_unit/filter) + gắn tạm vào loot `searchpoint_back_room` (guaranteed wood/scrap, chance pump_part/purifier_unit/filter — chỗ thật S17). `ShelterState.Modules:Dict<id,ModuleState>`; `ActiveTaskState` viết lại thật (TaskId/Kind/TargetId/ModuleId/Progress/Status/RequiredWorker, bỏ `Id`/`StatusName` cũ) + owner mới `task:<id>` (WorldState.TaskInventories, lazy qua InventoryOwnerResolver) là nguồn sự thật duy nhất cho vật liệu đã reserve (không tách list riêng). `BuildRules` (pure: ValidatePlacement/HasMaterials/DismantleRefund 50%, không phụ thuộc GameContext — theo đúng convention TravelRules/HazardRules). `StartBuildCommand`/`DismantleModuleCommand` (mới) + `StartTaskCommand`/`CancelTaskCommand` viết lại thật + `PauseTaskCommand`/`ResumeTaskCommand` (mới). `TaskSystem` (LongTick, Passive task luôn chạy kể cả FastForward lúc vắng mặt; Active task cần RequiredWorker tại shelter — chưa ai dùng nhánh Active trong S11, để sẵn cho S12). `WaterIntrusionSystem` nối thật Pump (đếm module Active tag "pump") + Barrier (chặn 70% inflow bảng, durability −2/long-tick khi ngập, tự vô hiệu khi durability=0). `BuildPanel` UI (phím B, cùng pattern lifecycle-safe CanvasGroup/ExclusivePanelOpened như ContainerPanel/WorldMapPanel — cố tình copy nguyên pattern sau bug 2026-07-24). SaveVersion→6, manifest→0.7.0. Test: **171/171 EditMode pass** (22 test mới: BuildRulesTests 11 + BuildCommandsTests 9 + WaterIntrusionRulesTests +2 barrier); build Windows 0 lỗi; headless smoke sạch. **Chưa xác nhận bằng mắt** (bấm B có mở panel đúng không, Build/Pause/Cancel/Dismantle bấm được không) |
| S12 | Power + Water + Sleep Simulation | Verify | `PowerRules` (pure: Grid trước rồi Battery, ưu tiên Critical>High>Normal>Disabled, Disabled không bao giờ có điện dù dư công suất; battery đo bằng unit-phút, xả tối đa 3 đơn vị/long-tick, sạc 2 đơn vị/long-tick khi Grid dư) + `PowerSystem` (LongTick, đọc `grid_down` flag — chưa ai set tới S17 nên Grid luôn sẵn; chỉ set `Active` cho module có PowerDemand>0, Barrier/Elevated Storage không bị đụng tới). `ShelterState.Power:ShelterPowerState{BatteryCharge,Priorities}` (cuối cùng thêm — trì hoãn từ S10 vì class chưa tồn tại). `WaterSystem` (LongTick, chỉ tích Untreated Water thụ động khi mưa — batch Purify là lệnh người chơi chủ động, không tự động). `ShelterCommands.cs`: `SetPowerPriorityCommand`, `StartPurifyBatchCommand` (cùng dạng validate-rồi-FastForward như RestAtShelterCommand, không phải Active task có giám sát — tái dùng `ModuleState.Durability` làm "tuổi thọ filter", 1 filter = 3 batch), `CollectWaterCommand` (đổi Clean Water trừu tượng thành `item_water_bottle` cụ thể, tái dùng item P1 có sẵn thay vì bịa item mới). `StartSleepCommand` viết lại: chặn Incapacitated + shelter ngập Deep+ (`UnsafeToSleep`; `NoBedAvailable` khai báo nhưng chưa dùng — chưa có Bed module thật), FastForward có interrupt (`TickScheduler.FastForward(minutes, Func<long,bool>)` overload mới, không đổi API cũ) — thức dậy sớm nếu ngập lên Deep giữa lúc ngủ. `ShelterPanel` UI (phím N: cycle priority từng module, Purify Batch, Collect Water, tóm tắt task — chi tiết Build/Pause/Cancel vẫn ở BuildPanel/B tránh trùng lặp UI). SaveVersion→7, manifest→0.8.0. Test: **196/196 EditMode pass** (25 test mới: PowerRulesTests 8 + ShelterCommandsTests 10 + SleepInterruptTests 7); build Windows 0 lỗi; headless smoke sạch. **Chưa xác nhận bằng mắt** (phím N mở panel đúng không, priority cycle/Purify/Collect bấm được không, ngủ qua Debug Panel có thức dậy sớm khi ngập không) |
| S13 | Event Framework lõi + 3 Shelter Event + 2-trong-3 | Verify | `EventDefinition`+`events_p3.json` (3 event: Drain Backflow — Phase BlackWater+State WaterIntrusion≥Damp; Storage Flood Risk — State≥Shallow; Pump Jam — cần Pump active + roll 1%/long-tick qua stream "events"; schema đầy đủ theo event-system-design.md §4/§25 nhưng Priority/EventType/Scope chỉ hiển thị, chưa enforce Event Budget). Trigger model là AND phẳng các field optional (không phải cây Compound Trigger đầy đủ — chưa đủ nội dung để cần cây thật). `ActiveEventState` viết lại thật (EventInstanceId/EventId/State/TriggeredAtMinute/DeadlineMinute/ChosenResponse) + `EventLifecycleState` enum đủ 8 trạng thái tài liệu nhưng S13 chỉ đi qua Triggered(ngầm)→Active→Resolved (bỏ qua Undiscovered/Discovered — chưa có Event Discovery; Deadline lưu nhưng chưa enforce — cả hai chờ S14). `EventTriggerRules` (pure) + `EventSystem` (LongTick, dedup theo EventId, publish `EventTriggered`). `ResolveEventCommand` (switch nhỏ theo response id thay vì resolution_rules data-driven đầy đủ — chỉ 3 event×1 response mỗi cái, chưa đủ để cần rules engine). Cờ `drain_backflow_active`/`pump_jammed` chuyển từ `ShelterEventFlags` (Core.State, không phải `WaterIntrusionSystem` — Core không được phụ thuộc Systems, phát hiện lúc build circular reference) — `WaterIntrusionSystem` đọc cờ này để tính inflow/đếm pump thật. DebugPanel +Events section (force-trigger cheat + resolve button per active event). `ScenarioTests` +Scenario E (Gate P3): budget 9 wood/6 scrap/1 pump_part/1 purifier_unit/2 filter/4 battery đủ xây Barrier+bất kỳ 2/4 module khác — test 3 tổ hợp cụ thể qua command+FastForward thật (không mô phỏng "sống qua Peak" vì chưa có Outcome System, đó là S18). SaveVersion→8, manifest→0.9.0. Test: **221/221 EditMode pass** (25 test mới: EventTriggerRulesTests 12 + EventCommandsTests 9 + ScenarioTests +Scenario E 3 tổ hợp — 1 bug tự phát hiện+sửa: test dedup ban đầu không cô lập event nên bị storage_flood_risk kích hoạt chung, đã tách riêng); build Windows 0 lỗi; headless smoke sạch. **Chưa xác nhận bằng mắt** — xem Gate P3 bên dưới |

**Gate P3: PASS về mặt kỹ thuật (2026-07-24).** 221/221 EditMode test; build Windows 0 lỗi; headless smoke sạch qua toàn bộ hệ thống P3 (Water Intrusion/Build/Task/Power/Water/Sleep/Event) khởi tạo không exception. Scenario E xanh (budget hỗ trợ 3 chiến lược 2-module khác nhau).

### Bug phát hiện qua playtest thật (2026-07-24, sau S13) — đã sửa

User báo camera lệch không vào trọng tâm người chơi, zoom chuột rất chậm, các thanh chỉ số (Condition HUD, Inventory weight/volume) không thấy đổi hình. Cả 3 đều là bug tồn tại từ lúc các hệ thống này được viết (P1/P1-C), chưa ai từng nhìn bằng mắt tới giờ:

1. **Camera lệch**: `CameraRig` offset theo-target `(0,12,-12)` không thực sự nằm trên trục nhìn của rotation cố định (pitch 35.264°/yaw 45°) — offset đúng cho rotation này phải có thành phần X khác 0. Sửa: tính offset trực tiếp từ rotation (`rotation * Vector3.back * distance`) thay vì hard-code, để 2 giá trị không bao giờ lệch nhau nữa.
2. **Zoom chậm**: nhân input scroll chuột với `Time.deltaTime` — nhưng scroll là xung 1-frame mỗi nấc, không phải trục analog giữ liên tục, nên nhân với deltaTime (~16ms) làm giá trị gần như triệt tiêu. Sửa: bỏ `Time.deltaTime`, chỉnh `zoomSpeed` lại làm bước nhảy cố định mỗi nấc.
3. **Thanh chỉ số không đổi hình** (ConditionHud 4 thanh + InventoryPanel weight/volume): set `Image.type = Filled` nhưng chưa từng gán `sprite` — Unity's `Image.OnPopulateMesh` âm thầm fallback vẽ full-rect bỏ qua `type`/`fillAmount` hoàn toàn khi `sprite == null`. Mọi thanh trong game tĩnh từ lúc viết ra tới giờ. Sửa: thêm `UiLayout.WhiteSprite()` (sprite trắng 1x1 cache sẵn), gán vào cả 2 chỗ tạo thanh.

Verify: 221/221 test vẫn pass (không có test tự động cho vùng Presentation/UI này), build 0 lỗi, headless smoke sạch. **User cần tự xác nhận lại bằng mắt**: camera có thực sự căn giữa người chơi chưa, zoom cảm giác đã ổn chưa (giá trị `zoomSpeed` mới là ước lượng, có thể cần chỉnh thêm), thanh máu/đói/khát và weight/volume có nhúc nhích khi giá trị đổi không.

**Bug nghiêm trọng phát hiện ngay sau đó — rơi vĩnh viễn ra khỏi map, đã sửa:** ground plane `20_MainShelter` chỉ 15x15m (±7.5), trong khi ramp+platform Upper Floor nằm ở z=-8 đến -13 — HOÀN TOÀN ngoài rìa ground plane, không có gì đỡ bên dưới. Đi lệch khỏi ramp (rộng 2.5m) ở đoạn ngoài z=-7.5, hoặc đi ra bất kỳ rìa nào khác của ground 15x15, rơi vào khoảng không vô hạn, không cách nào tự quay lại. Sửa 2 lớp: (1) ground plane phóng to lên 18x30m (x:±9, z:±15), đủ phủ toàn bộ layout kèm biên độ; (2) `PlayerController` giờ nhớ vị trí grounded gần nhất, nếu rơi xuống dưới y=-15 thì tự teleport về đó + reset vertical velocity — lưới an toàn chung, không chỉ vá riêng chỗ này vì blockout ramp/platform còn thô, có thể còn hở chỗ khác. Đã kiểm lại toán rotation của ramp độc lập — đúng, không phải nguyên nhân (forward vector sau rotation khớp chính xác hướng chuẩn hoá giữa 2 điểm đầu-cuối).

**User góp ý tiếp — chặn trước thay vì cứu sau, đã sửa:** thay lưới an toàn (bắt lại sau khi rơi) bằng tường vô hình (`BoxCollider` không `MeshRenderer`) bao quanh rìa ground plane mỗi scene (`SceneSetup.CreateBoundaryWalls`) — chặn hẳn không cho đi qua, giữ lưới an toàn cũ làm lớp phòng thủ thứ 2. Đồng thời phát hiện thêm 2 lý do ramp không thấy được: (1) chưa từng có `WorldLabel` như mọi marker khác — thêm màu nâu + label "Ramp"; (2) `WorldLabel` cũ tính lại hướng nhìn mỗi frame theo `Camera.main` dù camera góc cố định không xoay — chỉ vị trí đổi theo người chơi, khiến label hơi "đuổi theo" nhân vật; sửa thành tính 1 lần theo góc camera cố định. **User góp ý thêm: tên label toàn dạng snake_case ("shelter_entrance"), không phải tên hiển thị được** — thêm `WorldLabel.Prettify()` (snake_case → Title Case), áp cho mọi label (zone/core component/build slot/search point).

**Bug tự phát sinh ngay sau đó (kèm screenshot user gửi), đã sửa:** fix "hết đuổi theo nhân vật" ở trên dùng nhầm `Vector3.back` thay vì `Vector3.forward` khi tính hướng camera cố định — khiến TOÀN BỘ chữ trong scene bị lật ngược (mirror). Code cũ (đúng) tính `label.position - camera.position` = đúng hướng camera tự nhìn (`Vector3.forward` dưới rotation của nó), không phải hướng ngược lại. Đã sửa lại đúng dấu.

**User góp ý tiếp: tên entity trong game vẫn còn dạng x_y_z, đã sửa mở rộng:** lần Prettify trước chỉ sửa world label (chữ nổi 3D) — bỏ sót `BuildPanel`/`ShelterPanel`/`InventoryPanel`/`ContainerPanel` (panel người chơi thực sự đọc khi chơi), vẫn hiện "module_barrier", "slot_utility_area_1" nguyên văn. `InventoryPanel`/`ContainerPanel` còn có bug ẩn: fallback về `ItemDefinition.DisplayNameKey` nhưng KHÔNG content file nào từng set field này — nên fallback luôn rơi về raw id. Thêm `LastHope.Core.Text.DisplayName` (đặt ở Core vì `LastHope.UI` không reference `LastHope.Presentation`, nơi `WorldLabel.Prettify` cũ sống) — `Prettify()` (snake_case→Title Case) + `PrettifyWithoutPrefix()`. Theo đúng ví dụ user đưa: "module_barrier" → "Module Barrier" (giữ nguyên từ loại, không bỏ prefix) cho tên entity đứng độc lập; chỉ bỏ prefix khi context đã có heading riêng (world label "Slot\n...", dòng item trong túi đồ).

**User báo bug: không thấy thanh nước trong Shelter Panel/Debug Panel giảm khi Pump có điện, đã sửa:** hoá ra `ShelterPanel` (N) chưa từng hiển thị `WaterIntrusion` (mực nước lụt) — dòng tổng quan chỉ có Battery/Clean Water/Untreated, nên user không thể thấy Pump có hiệu lực hay không dù cơ chế `WaterIntrusionRules.ComputeDelta` trừ `pumpOutput` đã đúng và có test (`ComputeDelta_ActivePump_SubtractsPumpOutput`). `DebugPanel` (F2) vẫn luôn có dòng `Water {Level} ({Units}/100)` đọc trực tiếp state mỗi frame. Đã thêm `Flood {Level} ({Units}/100)` vào dòng tổng quan của `ShelterPanel` và subscribe thêm `ShelterWaterChanged` để panel rebuild khi mực nước đổi.

**Gate P3: PASS đầy đủ (user xác nhận bằng tay 2026-07-25).** Playtest thật xác nhận shelter loop chơi được; các bug UI phát hiện trong quá trình playtest (camera/zoom/thanh chỉ số/label/tên entity/thanh nước ShelterPanel) đã sửa hết như ghi chú phía trên.

**Review code P3 trước khi vào P4 (2026-07-25):** kiến trúc pure-rules/Systems/Commands nhất quán, save additive, không thấy bug logic mới. 1 điểm đã sửa: thứ tự khởi tạo system trong `GameBootstrapper` (WaterIntrusion trước Power) làm pump mới cấp điện chỉ có hiệu lực từ long-tick SAU — đảo lại Task → Power → WaterIntrusion để hiệu lực ngay trong tick. Ghi nhận không sửa: `ResolveEventCommand`/`RestAtShelterCommand` FastForward trong Execute là pattern có chủ đích; EventSystem đốt 1 lượt RNG stream "events" mỗi long-tick cho event có chance-gate kể cả khi điều kiện khác chưa thoả (deterministic, không phải bug).

Milestone tiếp theo: **P4 — Vertical Slice (S14–S18 → Gate GO/NO-GO)**, plan chi tiết đã có ở `docs/plans/2026-07-24-p3-p4-completion-plan.md`. Bắt đầu S14 2026-07-25.

## P4 — Vertical Slice (S14–S18 → Gate GO/NO-GO)

| ID | Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- | --- |
| S14 | Event Framework hoàn chỉnh + Event UI | Verify | Plan riêng `docs/plans/2026-07-25-s14-event-framework.md`. `EventDefinition` +`RequiresDiscovery`/`NextEventId`/`ExpirationShelterFlags`/`ExpirationPersistentFlags`; `ActiveEventState` +`SoftDeadlineMinute`/`SoftDeadlineNotified`; `EventSystem` lifecycle đầy đủ (Undiscovered→discovery tại shelter với deadline arm từ discovery, soft deadline → `EventDeadlineApproaching` 1 lần, hard deadline → Expire + Persistent Consequence flags + chain `NextEventId`; chain cả khi Resolve qua subscribe `EventResolved`; deadline check mỗi LongTick thay vì RegisterThreshold — save-safe, xem plan); sleep-wake theo Priority (`ShouldWakeSleeper`: Critical luôn/Major tại shelter/Standard không — tính cả event Undiscovered); `ResolveEventCommand` chặn Undiscovered (`EventNotDiscovered`); UI mới `EventPanel` (phím V, resolve trong game thật) + `EventToast` (banner top-center); pump_jam +soft_deadline 30'. SaveVersion→9, manifest→0.10.0. Test: **236/236 EditMode pass** (15 mới: EventLifecycleTests); scene regenerate sạch; build Windows Succeeded 0 lỗi 1 warning; headless smoke 15s: boot → spawn player không exception. **Chưa xác nhận bằng mắt** (phím V, toast, wake-khi-ngủ) — cần user test tay |
| S15 | Intel + World Map Intel + NPC nền | Backlog | Plan riêng `docs/plans/2026-07-25-s15-intel-npc-foundation.md` |
| S16 | Nguyễn Minh đầy đủ + NPC pressure | Backlog | (BL-P4 mô tả trong `docs/mvp-product-backlog.md` mục 8) |
| S17 | Slice content: 4 phase + 3 location + route + 6 event | Backlog | |
| S18 | Outcome + Causal Report + Save full + Art tối thiểu | Backlog | Gate GO/NO-GO là quyết định của user sau external playtest |

### Bug phát hiện qua playtest thật (2026-07-24, sau S9) — đã sửa

User báo "phím điều khiển có vẻ không hoạt động" (I/Tab, E không phản hồi). Phát hiện 3 lỗi lifecycle MonoBehaviour, tồn tại từ S6 (không phải do S7-S9):

1. **`ContainerPanel`** — `Awake()` tự gọi `gameObject.SetActive(false)` → Unity **không bao giờ gọi `Start()`** của chính GameObject đó (gotcha Unity đã biết) → subscription `ContainerViewRequested` không bao giờ đăng ký được → **UI Search/Storage chưa từng mở ra từ S6**, dù lệnh vẫn chạy đúng ngầm (loot roll, item transfer). Sửa: dời `SetActive(false)` xuống cuối `Start()`.
2. **`InventoryPanel`** (phím I/Tab) — resolve input action trong `Start()`, nhưng `OnEnable()` (gọi `Enable()`) chạy TRƯỚC `Start()` trong lifecycle → action không bao giờ thực sự bật → phím không phản hồi. Sửa: resolve action trong `Awake()`.
3. Cùng file, `SetActive(false)`/`(true)` để ẩn/hiện panel bằng chính `Update()` của nó — một khi ẩn thì `Update()` không chạy nữa, không bao giờ mở lại được. Sửa: dùng `CanvasGroup` (alpha/interactable/blocksRaycasts) thay vì `SetActive`.
4. **`WorldMapPanel`** (phím M, viết ở S8) — dính cả 3 lỗi trên do copy nhầm pattern. Sửa tương tự.

**Phát hiện thêm:** `Assets/Scenes/*.unity` chưa được `SceneSetup.BuildAll()` regenerate từ sau S6 (commit `e963c35`), dù `SceneSetup.cs` đã đổi qua S7-S9 (thêm `ConditionHud`, `WorldMapPanel`, sửa `TravelPointView`) — nghĩa là các thành phần này **chưa từng tồn tại trong scene thật** cho tới khi chạy lại `BuildAll()` lần này. Đã regenerate + verify lại 127/127 test + build 0 lỗi/0 warning + headless smoke sạch.

**Bug thêm phát hiện lúc user tự verify Gate P2 (2026-07-24), đã sửa:** `DebugPanel` (F2) dùng `GUILayout.BeginArea` cao cố định 640px không có scroll bao ngoài — nội dung đã phình to qua S7-S9 (Equipment, Rest at Shelter, Phase jump 4 nút, danh sách save slot tăng dần) vượt quá 640px bị `GUI.BeginGroup` clip cả hình lẫn input, nên phần cuối panel (Save/Load, State dump) không bấm được. Sửa: bọc toàn bộ nội dung trong `GUILayout.BeginScrollView` ngoài. Đồng thời "Add Item" trước đây bỏ qua `InstanceId` trả về từ `InventoryOps.AddItem`, buộc phải tự dò trong State dump JSON mới lấy được instance id để Equip — nay tự điền `_equipItemInstanceId`/`_equipSlot` ngay sau khi Add.

## Milestone sau nữa (P3: S10–S13 → Gate P3; P4: S14–S18 → Gate GO/NO-GO)

Plan chi tiết đã có trong `docs/plans/2026-07-24-p3-p4-completion-plan.md` (viết 2026-07-24, giả định P2 PASS): Shelter State/Build/Task/Water Intrusion (S10–S11), Power/Water/Sleep (S12), Event Framework lõi + 3 Shelter Event + kịch bản 2-trong-3 (S13, Gate P3), Event Framework hoàn chỉnh (S14), Intel + NPC nền (S15), Nguyễn Minh (S16), slice content 3 location/6 event (S17), Outcome + Causal Report + external playtest (S18, Gate GO/NO-GO). Event Framework XL tách qua S13/S14/S17. Bảng backlog chi tiết (BL-P3-01..18, BL-P4-01..16 — mô tả trong `docs/mvp-product-backlog.md` mục 7–8) sẽ thêm vào file này khi bắt đầu S10.
