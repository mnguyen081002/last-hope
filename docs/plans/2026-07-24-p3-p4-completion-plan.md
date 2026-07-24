# Plan: Hoàn thành P3 (S10–S13) + P4 (S14–S18) — MVP Black Rain

## Context

Giả định Gate P2 đã PASS (hết S9 theo plan `2026-07-24-p1-p2-completion-plan.md`): Condition/Hazard/Equipment/Return Window chạy thật, ScenarioTests A–D xanh. Plan này hoàn thành **Gate P3** (câu hỏi: *Shelter có phải không gian gameplay thật — Build/Power/Task cạnh tranh với Expedition Time — hay chỉ là menu chờ?*) và **Gate P4 GO/NO-GO** (câu hỏi: *các hệ thống có hợp thành một Core Loop rõ, Peak có kiểm tra chuẩn bị, tester có muốn chơi lại theo chiến lược khác không?*). Không content production hàng loạt trước khi P4 đạt Exit Criteria.

**Nguyên tắc đặt code giữ nguyên:** rule thuần deterministic → `Core/Rules/`; orchestrator Tick/Event → `Systems/`; Transform/Input → Presentation; panel → UI (nói chuyện qua EventBus). Mọi số tuning nằm trong `balance.json` + definitions.

**Tách BL-P4-01 (Event Framework, size XL) — chốt:** lõi framework (Definition/Instance/Trigger/Resolution) làm SỚM ở **S13** với 3 Shelter Event làm consumer đầu tiên; **S14** bổ phần còn lại (Discovery, Soft/Hard Deadline, Expiration, Persistent Consequence, Event Chain, off-scene, sleep-interrupt, Event UI); **S17** chỉ còn là content (`events_p4.json`). Không sprint nào ôm trọn XL.

**Phát hiện từ docs:** shelter design cho schema đầy đủ (8 zone, slot/zone, initial state Structural 85/Clean Water 3/Food 2, water model `gain = entrance + drain + structural − pump − passive_drain`, 5 mức Dry→Critical) nhưng KHÔNG có rate/cost/deadline nào — bảng baseline bên dưới là tự chốt.

---

## S10 — Shelter State + Blockout + Water Intrusion (P3-A nền)

**File mới:**
- `Data/Definitions/ShelterZoneDefinition.cs` + `shelterzones_p3.json` — 8 zone {Floor, BuildSlotIds, WaterRisk}; slot theo bảng docs (Entrance 2, GroundStorage 2, Utility 2, WaterProcessing 2, Workshop 1, UpperLiving 3, Roof 1); loader route `shelterzones_*.json`.
- `Core/State/ShelterState` mở rộng (file có sẵn, thay stub): `StructuralIntegrity`, `WaterIntrusion{Level enum Dry/Damp/Shallow/Deep/Critical, Units 0-100}`, `LivingCapacity`, `Occupants`, `BuildSlots:Dict<slotId,BuildSlotState{Locked, ModuleInstanceId}>`, `Modules:Dict`, `PowerState`, `WaterStocks{Clean,Untreated}`, `EventFlags:HashSet<string>`.
- `Core/Rules/WaterIntrusionRules.cs` — công thức docs §21 thuần: inflow theo phase + cờ backflow − pump − passive drain, clamp 0-100 → Level; **một công thức duy nhất** cho system live lẫn forecast UI.
- `Systems/Shelter/WaterIntrusionSystem.cs` — LongTick: đọc phase + module state → cập nhật Units/Level, publish `ShelterWaterChanged` chỉ khi Level đổi; Deep → khóa module điện tầng dưới; Ground Floor mất ≠ game over (chỉ set flag + warning).
- SceneSetup: dựng lại `20_MainShelter` — Ground+Upper, 8 zone marker, 6 Fixed Core Component anchor (Staircase/Pillars/DrainCore/ElectricalBackbone/WaterIntake/AntennaMount — GameObject rỗng + `IInteractable` cho DrainCore), `BuildSlotView` placeholder mỗi slot.
- `Presentation/World/BuildSlotView.cs`, `CoreComponentView.cs` — `[SerializeField] id` bind definition.

**Sửa:** WorldStateSerializer roundtrip fields mới (additive, SaveVersion→6), GameEvents (+`ShelterWaterChanged`, `ShelterStateChanged`), DebugPanel (+xem/cheat shelter state — thiết yếu để tune), balance.json (+`ShelterBalance`). Tests: WaterIntrusionRulesTests (~8), roundtrip.

## S11 — Build & Placement + Task System

**File mới:**
- `Data/Definitions/ModuleDefinition.cs` + `modules_p3.json` — 5 module {SlotZoneKinds, Materials:Dict<itemId,qty>, BuildMinutes, PowerDemand, MaxDurability, Tags}; loader route `modules_*.json`.
- `Core/State/ModuleState.cs` — {InstanceId, ModuleId, SlotId, Progress 0-100, Durability, Active}.
- `Core/Rules/BuildRules.cs` — `ValidatePlacement` (slot đúng zone, không Locked/occupied, không chặn Drain Core), `HasMaterials`, `DismantleRefund` (50%).
- `Core/State/ActiveTaskState` fields thật (thay stub): {TaskId, Kind Active/Passive, TargetId, Progress, Status Running/Paused, ReservedItems:List, RequiredWorker}. **Resource reservation = chuyển material vào inventory owner mới `task:<taskId>`** (thêm vào InventoryOwnerResolver, lazy-create) — cancel trả về storage, TransferItemCommand dùng lại nguyên vẹn.
- `Core/Commands/BuildCommands.cs` — `StartBuildCommand` (validate → reserve material → tạo task Passive), `DismantleModuleCommand`; **StartTaskCommand/CancelTaskCommand có sẵn viết body thật** + `PauseTaskCommand`/`ResumeTaskCommand`.
- `Systems/Tasks/TaskSystem.cs` — LongTick: task Passive tự chạy (kể cả khi player vắng mặt/ngủ — chạy trong FastForward vì là LongTick handler); task Active cần player tại shelter; Progress đủ → consume reservation, spawn ModuleState, publish `ModuleCompleted`/`TaskStateChanged`.
- `UI/Shelter/BuildPanel.cs` — list slot/module, material đủ-thiếu, nút Build/Pause/Cancel/Dismantle; toggle B.

**Sửa:** CommandErrorCode (+`SlotOccupied`, `SlotLocked`, `MissingMaterials`, `TaskNotFound`, `TaskNotRunning`), GameEvents (+`TaskStateChanged`, `BuildProgressChanged`, `ModuleCompleted`), InventoryOwnerResolver (+`task:<id>`), items_p3.json (material: wood/scrap/pump_part/purifier_unit/filter/fuel_can — loot đặt ở store + gara S17, tạm cho vào searchpoints hiện có), SaveVersion→7. Tests: BuildRulesTests + TaskTests (~12, gồm passive-chạy-khi-FastForward).

## S12 — Power + Water + Sleep Simulation

**File mới:**
- `Core/Rules/PowerRules.cs` — `Allocate(supply, demands, priorities) → allocations` thuần deterministic: City Grid (∞ tới khi flag `grid_down`) → Battery; thiếu → cắt theo priority từ thấp.
- `Core/State/PowerState.cs` — {GridAvailable, BatteryCharge, Priorities:List<moduleInstanceId>, LastAllocations}.
- `Systems/Shelter/PowerSystem.cs` — LongTick: tính supply/demand, sạc battery khi grid còn (thừa supply), publish `PowerStateChanged` khi allocation đổi; module mất điện → Active=false.
- `Systems/Shelter/WaterSystem.cs` — LongTick: Purifier active + đủ power/fuel + filter → batch 3 Untreated→3 Clean/60 phút; filter mòn theo batch; lấy Untreated qua Water Intake khi mưa (+1/giờ).
- `Core/Commands/ShelterCommands.cs` — `SetPowerPriorityCommand`, `StartPurifyBatchCommand`, `CollectWaterCommand`.
- `Core/Commands/StartSleepCommand` viết lại: validate (có Bed, zone không ngập Deep+, không Incapacitated) → FastForward **có interrupt**.
- `Core/Time/TickScheduler`: thêm overload `FastForward(minutes, Func<long,bool> interrupt)` (additive, không đổi API cũ) — kiểm tra sau mỗi phút; S12 wake-condition = ShelterWaterChanged lên Deep/Critical; S14 nối vào Event priority.
- `UI/Shelter/ShelterPanel.cs` — power allocation (kéo priority), water stocks, task list; toggle N.

**Sửa:** CommandErrorCode (+`NoPower`, `NoFilter`, `NothingToPurify`, `NoBedAvailable`, `UnsafeToSleep`), GameEvents (+`PowerStateChanged`, `WaterStocksChanged`, `SleepStarted`, `SleepInterrupted`, `SleepEnded`), balance.json (+power/water section), SaveVersion→8. Tests: PowerRulesTests + WaterSystemTests + SleepInterruptTests (~12).

## S13 — Event Framework lõi + 3 Shelter Event + 2-trong-3 → **Gate P3**

**File mới:**
- `Data/Definitions/EventDefinition.cs` + `events_p3.json` — schema đầy đủ theo event-system-design §4 (id/type/scope/priority/trigger_conditions/deadline/responses/success- failure- expiration- persistent_effects) nhưng S13 **chỉ tiêu thụ** Trigger→Active→Resolved; 3 event: Drain Backflow (Phase+State trigger), Storage Flood Risk (WaterIntrusion≥Shallow trigger), Pump Jam (chỉ khi có Pump, roll stream `"events"` theo giờ-chạy-không-bảo-trì).
- `Core/State/ActiveEventState` fields thật (thay stub): {EventId, State enum đủ 8 trạng thái vòng đời, TriggeredAtMinute, DeadlineMinute, ChosenResponse}.
- `Core/Rules/EventTriggerRules.cs` — evaluate Time/Phase/State/Compound trigger thuần trên WorldState (deterministic; phần Chance qua stream `"events"` truyền vào).
- `Systems/Events/EventSystem.cs` — LongTick evaluate Dormant→Triggered; effect apply qua mutator ShelterState/flags; publish `EventTriggered`/`EventResolved`.
- `Core/Commands/ResolveEventCommand.cs` — chọn response (vd Pump Jam → task sửa 15 phút; Storage Flood → chuyển đồ lên Elevated Storage).
- `Tests/EditMode/ScenarioTests` +Scenario E: "6h prep + 6h Peak, budget material chỉ đủ 2/3 module" chạy thuần command+FastForward, assert cả 3 cách chọn 2-module đều sống qua Peak (3 chiến lược hợp lệ).

**Sửa:** TelemetryLogger (+build_started/completed, power_alloc_changed, task_idle_time, water_level_change), CommandErrorCode (+`EventNotActive`, `ResponseUnavailable`), GameEvents (+`EventTriggered`, `EventResolved`), DebugPanel (+trigger-event cheat), tuning pass theo bảng baseline, SaveVersion→9.

**Gate P3 check:** Scenario E xanh + PassiveTask-trong-sleep test xanh + user playtest 6h+6h: hiểu vì sao nước vào, Power Allocation phải chọn, Ground Floor mất vẫn chơi tiếp, không đứng chờ task.

## S14 — Event Framework hoàn chỉnh + Event UI (P4-A)

- `Systems/Events/EventSystem` mở rộng: Discovery (Undiscovered→Discovered qua nguồn: tại chỗ/radio/NPC), Soft/Hard Deadline qua `RegisterThreshold`, Expired + expiration_effects, Persistent Consequence → `WorldState.EventFlags` + RouteState/LocationState mutation, Event Chain (`next_event_id`), **off-scene mặc định** (system không đọc vị trí player trừ trigger Location) — test riêng: event trigger+expire khi player ở location khác và trong sleep FF.
- Sleep-interrupt nối Event priority (event-system §14): Critical → wake bắt buộc; Major → wake nếu tại shelter; Standard → không wake.
- `UI/Events/EventPanel.cs` — banner cảnh báo + log + đồng hồ deadline (soft vàng/hard đỏ); `UI/Events/EventToast.cs`.
- Sửa: GameEvents (+`EventDeadlineApproaching`, `EventExpired`, dùng lại `EventDiscovered` có sẵn), CommandErrorCode (+`EventNotDiscovered`), SaveVersion→10. Tests: lifecycle đủ 8 trạng thái + off-scene + expire-trong-sleep (~12).

## S15 — Intel + World Map Intel (P4-B) + NPC nền

- `Core/State/IntelState.cs` — {Records:Dict<subjectId, IntelRecord{Kind, Confidence Confirmed/Reliable/Uncertain/Unverified, ObservedAtMinute, Payload}}; `Core/Rules/IntelRules.cs` — Confidence suy giảm theo Information Age (bảng baseline), merge record mới thắng record cũ.
- `Systems/Intel/IntelSystem.cs` — quan sát trực tiếp (TravelCompleted/SearchPointOpened → intel Confirmed), radio/comms + event discovery → intel Uncertain/Reliable.
- `UI/Map/WorldMapPanel` sửa: **chỉ render điều đã biết** — route/flood/event marker từ IntelState thay vì đọc RouteState thật; forecast từ intel radio; chỗ chưa biết hiện "?" + tuổi thông tin.
- NPC nền: `Data/Definitions/NpcDefinition.cs` + `npcs_p4.json` (Nguyễn Minh: skill electric ×1.5, trait, chi phí); `Core/State/NpcState` fields thật theo npc-framework §3 (rút gọn: Location, HealthState Healthy/Injured/Critical/Dead, Hunger/Thirst, Trust 0-100, CurrentTaskId, Flags); owner `npc:<id>` kích hoạt trong InventoryOwnerResolver.
- Sửa: GameEvents (+`IntelUpdated`, dùng lại `NpcStateChanged`), SaveVersion→11. Tests: IntelRulesTests + NpcState roundtrip (~10).

## S16 — Nguyễn Minh đầy đủ + NPC pressure (P4-C)

- `Core/Commands/NpcCommands.cs` — `RecruitNpcCommand` (điều kiện trust + chỗ ở LivingCapacity), `AssignNpcTaskCommand` (shelter task qua TaskSystem — NPC là RequiredWorker cho Active task), `SendNpcExpeditionCommand` (đi lấy đồ 1 route đã biết — nằm ĐẦU danh sách scope-cut, giữ tối giản).
- `Systems/Npc/NpcSystem.cs` — LongTick: tiêu thụ nước/food từ shelter storage (thiếu → Trust giảm, đói → Injured→Critical→Dead), làm task, injury khi shelter ngập Deep tại zone NPC.
- Event chain Minh 3 bước (`events_p4_minh.json`) chạy trên framework S14: Neighbor Introduction → Missing Relative (soft/hard deadline thật) → Consequence 3 nhánh.
- Sửa: CommandErrorCode (+`NpcUnavailable`, `NpcNotRecruited`, `CapacityFull`), GameEvents (+`NpcRecruited`, `NpcDied`), ConditionHud/ShelterPanel hiện occupant, SaveVersion→12. Tests: consumption-pressure (NPC ăn thật vào budget 2-trong-3), chain 3 nhánh, injury/death (~12).

## S17 — Slice content: 4 phase + 3 location + route + 6 event (P4-D nửa đầu)

- `phases_p4.json` — timeline slice theo bảng baseline (60–90 phút thực); `locations_p4.json`/`routes_p4.json` (+Shortcut `RequiresIntel:true` — chỉ đi được khi có intel từ khảo sát), `searchpoints_p4.json` (gara: pump_part/tool; trường: đồ y tế + temp shelter).
- SceneSetup: `42_Location_UtilityGarage`, `43_Location_School` (tầng trên = Temporary Shelter: `SurveyPoint` interactable → kích hoạt `shelter_school` trong `WorldState.Shelters` — **ShelterState chuyển thành Dict<id,ShelterState>**, migration additive từ single), route thứ 2 + shortcut vào WorldMapPanel.
- `events_p4.json` — 6 main event trên framework: Storm Warning (Normal/Major), Black Rain Transition (Critical), School Rescue (soft/hard deadline — event chính kiểm tra "một lượt không làm hết"), Grid Failure (Escalation → flag `grid_down`, Power System tự phản ứng), Drain Backflow + Pump Jam/Storage Flood (re-tune từ S13 theo phase P4).
- Sửa: DisasterPhaseSystem đọc phases_p4, BalanceConfig (+`SliceBalance`), manifest → 0.4.0, SaveVersion→13. Tests: content validation (dangling ref 3 location/6 event), timeline FastForward end-to-end không exception (~8).

## S18 — Outcome + Report + Save full + Art tối thiểu → **Gate P4 GO/NO-GO**

- `Core/State/DecisionLogState.cs` — command/event lớn append {Minute, DecisionId, Payload} (nguồn dữ liệu Causal Report — ghi từ ResolveEvent/Build/Recruit/Evacuate).
- `Core/Rules/OutcomeRules.cs` — evaluate 3 outcome thuần trên WorldState: Stable Survival (sống + shelter ở được + ≥ tài nguyên tối thiểu) / Forced Evacuation (mất Main Shelter + tới `shelter_school` thành công) / Collapse (chết hoặc không còn shelter hợp lệ).
- `Systems/Outcome/OutcomeSystem.cs` — threshold cuối Aftermath hoặc điều kiện fail tức thời → publish `OutcomeReached`; `Core/Commands/EvacuateCommand.cs` (bỏ storage lại, đổi shelter chính).
- `UI/Outcome/OutcomeReportPanel.cs` — Causal Outcome Report v1: Major Decisions → Consequences, Resources Preserved/Lost, NPC Outcome, Shelter Outcome (format ví dụ trong win-lose doc §9).
- `Tests/EditMode/SliceRoundtripTests.cs` — full slice: chạy tới giữa Escalation bằng command+FF → save → load → chạy tiếp → cùng outcome; save giữa event-active/NPC-task/purify-batch.
- Art tối thiểu: SceneSetup lighting pass (ambient theo phase), `Presentation/Audio/HazardAudioController.cs` (loop rain/drain/pump/alert theo GameEvents — **cần file audio thật, người cung cấp**), UI hierarchy pass (font size/nhóm panel). SaveVersion→14.
- External playtest (BL-P4-16): build + telemetry sẵn (`outcome_reached`, `replay_intent` survey ngoài game) — tổ chức là việc của người.

---

## Bảng baseline P3/P4 (tự chốt — tất cả trong balance.json/definitions)

| Nhóm | Giá trị |
| --- | --- |
| Water Intrusion (units 0-100; Damp≥10 Shallow≥30 Deep≥60 Critical≥85) | inflow/long-tick(10'): FirstRain +2, BlackRain +4, Escalation +6, Peak +9; Drain Backflow +6 thêm; passive drain −2 (0 khi backflow); Pump −6 (cần 2 power); Barrier chặn 70% entrance inflow, durability 100 −2/long-tick khi ngập |
| Module (cost / phút build) | Barrier: 4 wood+2 scrap/60' · Pump: pump_part+2 scrap/45' · Elevated Storage: 3 wood/40' (30kg/50L, không TwoHandCarry) · Purifier: purifier_unit+filter/50' · Battery Bank: 2 battery+1 scrap/30' |
| Power | demand: Pump 2, Purifier 2, Lighting 1, Comms 1; Grid supply 6 (tới flag `grid_down`); Battery: dung lượng 360 unit-phút, xả tối đa 3, sạc 2/phút khi grid thừa |
| Water | Purifier batch: 3 Untreated→3 Clean/60'; filter mòn 3 batch; Intake +1 Untreated/giờ khi mưa; khởi đầu Clean 3, Food 2 (theo docs) |
| NPC (Minh) | tiêu thụ = player (2 nước+1.5 food/ngày); Trust start 30, +5 nuôi đủ/ngày, −10 đói, ≥50 mới nhận Expedition; task electric ×1.5 |
| Event deadline | School Rescue: soft 90' hard 180' từ discovery; Storage Flood: cảnh báo 30' trước Deep; Pump Jam: roll 20%/4h-chạy-không-bảo-trì (stream `events`), task sửa 15'; Intel Age: Confirmed→Reliable 60', →Uncertain 180' |
| Kịch bản 2-trong-3 | material trong world: 9 wood, 6 scrap, 1 pump_part, 1 purifier_unit, 2 filter, 4 battery — đủ đúng 2 module lớn + Barrier; prep 6h game ≈ 4-5 chuyến travel |
| Slice timeline (75-90' thực ≈ 450-500' active + sleep FF) | Normal@0 → Warning@120 → FirstRain@300 → BlackRain@480 → Escalation@640 → Peak@760 → Aftermath@880 → kết thúc@960; 1 cửa sổ sleep khuyến nghị trong FirstRain |
| Save policy | SaveVersion 6→14 (bump mỗi sprint đổi schema), mọi thay đổi additive; ShelterState→Dict migration ở S17 có test load save cũ |

## Verification

- Mỗi sprint: compile 0 lỗi + full EditMode suite + build Windows + headless smoke; cập nhật BACKLOG.md/CODEMAP.md cùng commit (S10 ~8 test mới, S11 ~12, S12 ~12, S13 scenario E + ~8, S14 ~12, S15 ~10, S16 ~12, S17 ~8, S18 roundtrip + outcome ~10).
- **Gate P3** (cuối S13): Scenario E (2-trong-3, cả 3 cặp module đều sống) + passive-task-trong-sleep test xanh + user playtest 6h+6h.
- **Gate P4** (cuối S18): SliceRoundtripTests xanh + AI tự chạy slice headless bằng command 3 lần theo 3 chiến lược → 3 outcome khác nhau, không softlock; sau đó external playtest ≥60% muốn chơi lại khác — **quyết định GO/NO-GO là của người, không phải test**.
- Điểm cần mắt/tai người (AI headless KHÔNG tự xác nhận được): blockout/lighting 2 tầng shelter + 3 location, âm thanh hazard (kể cả việc cung cấp file audio), cảm giác deadline "công bằng", UI hierarchy đọc được, toàn bộ BL-P4-16 và tỷ lệ replay-intent.
- Scope-cut nội bộ P4 nếu trễ (đúng thứ tự backlog dòng 318): Shortcut → Temporary Shelter nâng cấp → `SendNpcExpeditionCommand` → Optional Event → Signal Narrative → Advanced Contamination. Không cắt: Clock/Search/Inventory Decision/Flood Route Change/Shelter Prep/Peak/Outcome.
