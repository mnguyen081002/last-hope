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

**Gate P2: PASS về mặt kỹ thuật (2026-07-24).** 127/127 EditMode test (Condition, Disaster Phase, Hazard, Travel Rules, Equipment, Scenario A-D); build Windows 0 lỗi; headless smoke xác nhận boot sạch với toàn bộ hệ thống P2 (ConditionSystem/DisasterPhaseSystem/HazardSystem) khởi tạo không exception.

**Còn thiếu để Gate P2 pass đầy đủ (cần user xác nhận bằng tay, theo đúng verification section của plan):** chơi thử build 30-45 phút thực tế — đổi route vì flood dâng, đổi loadout trang bị (jacket/boots/gloves/rope/dry bag) theo tình huống, hiểu return window trên World Map, không bị softlock, xử lý exposure được tại shelter (Rest/Treat/DryOff). AI headless không tự xác nhận được cảm giác chơi, chỉ xác nhận cơ chế đúng qua test.

Milestone tiếp theo: **P3 — Shelter Loop (S10–S13)**, plan chi tiết đã có ở `docs/plans/2026-07-24-p3-p4-completion-plan.md`.

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
