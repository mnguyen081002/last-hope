# Plan: Hoàn thành P1 (S5–S6) + P2 (S7–S9) — MVP Black Rain

## Context

Gate M1 đã PASS (commit `bfe496b`): Core simulation loop (Clock/Tick/Command/Save/RNG) chạy thật, 19/19 test. Plan này hoàn thành **Gate P1** (Exploration Loop chơi được: Interaction, Inventory rules, Search "thấy hết-lấy hết", Storage, Travel, content thật, telemetry) và **Gate P2** (Condition, Status Effect, Flood/Hazard, Equipment Protection, Return Window, phase timeline rút gọn).

**3 phản hồi user phải sửa ngay trong S5:**
1. Save không lưu vị trí nhân vật → thêm `PositionX/Y/Z + PositionLocationId` vào PlayerState; `PlayerAvatarSync` (Presentation) ghi transform → state mỗi frame (ngoại lệ presentation-write có ghi chú — dữ liệu liên tục, không phải rule), apply lại khi load/vào scene.
2. Load phải gõ tay slot id → DebugPanel dùng `SaveService.ListSlots()` render nút bấm từng slot.
3. "Add Item" không có item thật → S5 ship content JSON thật (`items_p1.json`...).

**Nguyên tắc đặt code (chốt):** rule thuần deterministic → `Core/Rules/` (test không cần Unity); orchestrator subscribe Tick/Event → `Systems/`; đụng Transform/Input/Time → Presentation/UI. UI không tham chiếu Presentation — nói chuyện qua EventBus.

**Phát hiện từ docs:** spec chỉ cho schema/enum, KHÔNG có số tuning nào (decay rate, exposure threshold, travel cost... đều "cân bằng trong prototype") → toàn bộ bảng baseline bên dưới là tự chốt, nằm trong `balance.json` + definitions để tune không sửa code.

## Bước 0
- Copy plan này → `docs/plans/2026-07-24-p1-p2-completion-plan.md` (quy tắc plan lưu trong project).

---

## S5 — Interaction + Inventory rules + UI + fixes (3–4 khối commit)

**File mới:**
- `Data/BalanceConfig.cs` — mọi tunable: `InventoryBalance{15kg/25L, Light>100%, Heavy>130%, HardCap 150%, speed ×0.6/×0.35}`, `TravelBalance{loadFactor 1.0/1.25/1.5}`, `NewGameBalance{StartLocationId}`. Load từ `balance.json` (1 object; thiếu file → dùng default + warning).
- `Core/Rules/InventoryRules.cs` — `ComputeOverload` (ratio = max(weight%, vol%)), `CanAccept` (post-add ≤ 150% cả 2 trục), `SpeedModifierFor`, `IsCapacityLimited` (chỉ "player").
- `Systems/Inventory/InventorySystem.cs` — nghe `InventoryChanged` → recompute Overload → publish `OverloadStateChanged`.
- `Presentation/Player/PlayerAvatarSync.cs` — sync transform↔state (fix #1) + nghe `OverloadStateChanged` → set `PlayerController.SpeedModifier`.
- `Presentation/Interaction/{IInteractable, InteractionDetector, InteractionPrompt}.cs` — OverlapSphere ~1.6m + cursor-ray tiebreak, phím E (action có sẵn), interact tức thì (docs không spec hold).
- `UI/Inventory/InventoryPanel.cs` — list phẳng + 2 thanh weight/volume màu theo Overload, nút Use/Drop, toggle I/Tab (action mới). Code-built, không prefab.
- Content: `items_p1.json` (5 item theo bảng baseline; container 20L có field mới `TwoHandCarry`), `locations_p1.json` (+field mới `SceneName`), `routes_p1.json`, `searchpoints_p1.json`, `balance.json`. manifest → 0.2.0.

**Sửa file có sẵn:** PlayerState (+position), ItemDefinition (+TwoHandCarry), LocationDefinition (+SceneName), DefinitionLoader/Registry (+Balance), TransferItemCommand (validate capacity → `InventoryFull`; publish `ItemTransferred`), GameEvents (+`OverloadStateChanged`, `WorldStateReloaded`, `ItemTransferred`), DebugPanel (slot picker — fix #2; publish `WorldStateReloaded` sau load), GameBootstrapper (+InventorySystem, StartLocationId), GameControls.inputactions (+ToggleInventory), **SceneSetup: chuyển Player+Camera sang 10_GamePersistent (persistent avatar — tiền đề đổi scene S6) + HUD canvas code-built (EventSystem + InputSystemUIInputModule)**.

## S6 — Search + Storage + Travel + Telemetry + scenes → **Gate P1**

**Owner-id scheme (mở khóa mọi container qua TransferItemCommand có sẵn):** `player` · `searchpoint:<id>` (chỉ resolve khi đã Rolled) · `shelter_storage:<id>` (lazy-create, không giới hạn P1/P2) · `location_dropped:<locId>` (lazy) · để dành `npc:<id>`.

**File mới:**
- `Core/State/SearchPointState.cs` — `{SearchPointId, Rolled, Inventory:InventoryState}` — chứa InventoryState nên TransferItem dùng lại nguyên vẹn.
- `Core/Commands/OpenSearchPointCommand.cs` — validate đúng location; roll MỘT LẦN qua stream "loot" (qty = NextInt(Min,Max+1)); `OpenTimeMinutes>0` → FastForward; publish `SearchPointOpened` + `ContainerViewRequested`.
- `Systems/Telemetry/TelemetryLogger.cs` — JSONL `persistentDataPath/Telemetry/session_*.jsonl` (AppendAllText, crash-safe), SessionId/PlaythroughId, nghe TravelStarted/Completed (+carry_load_on_return), SearchPointOpened, ItemTransferred; `Log()` public cho UI (inventory_open_time, item_left_behind).
- `Presentation/World/{SearchPointView, ShelterStorageView, TravelPointView, PlayerSpawnPoint}.cs` — MonoBehaviour + `[SerializeField] string id` bind với definition.
- `Presentation/Boot/SceneFlowController.cs` — nghe `TravelCompleted`/`WorldStateReloaded` → load/unload additive scene theo `LocationDefinition.SceneName`, đặt player (vị trí save nếu khớp location, không thì SpawnPoint). Chủ sở hữu duy nhất của vòng đời gameplay scene.
- `UI/Container/ContainerPanel.cs` — 1 panel dùng chung searchpoint + storage: Take/Take All/Store qua TransferItemCommand; đóng searchpoint → log item_left_behind.

**Sửa:** WorldState (LocationState +SearchPointStates +DroppedItems; ShelterState +Storage; +PlaythroughId), InventoryOwnerResolver (scheme trên), **BeginTravelCommand body đầy đủ** (adjacency → `factor` theo Overload → `TravelStarted` → FastForward(ceil(25×factor)) → đổi CurrentLocationId → `TravelCompleted`), GameEvents (+4 event), BootLoader (scene đầu qua SceneFlowController), SceneSetup (+`20_MainShelter`, `41_Location_ConvenienceStore` blockout với 6 SearchPointView + TravelPoint + SpawnPoint, layer "Interactable"), DebugPanel (+nút Travel cheat).

**Gate P1 check:** test depletion-qua-save tự động (open → take một phần → serialize/deserialize → còn đúng phần, không re-roll) + user playtest build: Take All fail → triage → bỏ đồ lại → quay lại lấy được.

## S7 — Condition + Phase timeline (P2 nền)

- `Core/State/PlayerConditionState.cs` — Health/Stamina/Fatigue 0-100, Hunger/Thirst 0-100 (0=no, need-meter), BodyTemperatureC=36.5, StatusEffects{id→{Severity,AppliedAt}} (wet/cold/bleeding/sick/black_water_exposure/disoriented), Exposures{hazard→Accumulated}, Incapacitation{None,Collapsed}.
- `Core/Rules/ConditionOps.cs` — mutator clamp thuần: Apply/AddStatusSeverity/AddExposure (threshold → status chain)/IsIncapacitated.
- `Data/Definitions/DisasterPhaseDefinition.cs` + `phases_p2.json` — {StartMinute, FloodBandMin/Max, CurrentBandMin/Max, BlackWater, RainIntensity}; loader route `phases_*.json`, validate sorted + có phase tại phút 0.
- `Systems/Disaster/DisasterPhaseSystem.cs` — recompute phase hiện tại lúc construct (đúng sau load), RegisterThreshold cho các phase TƯƠNG LAI → publish `DisasterPhaseChanged`.
- `Systems/Condition/ConditionSystem.cs` — ShortTick: stamina regen (×0.5 khi exposure status), body temp drift (xuống khi wet>50 lúc mưa, lên tại shelter), wet khô tại shelter; LongTick: hunger/thirst/fatigue accrual, status chain (temp<35→cold; exposure≥40→black_water_exposure; ≥70→sick), đói/khát → health decay **floor ≥1, không chết tức thì** — Collapsed khi Health≤5.
- Sửa: PlayerState (+Condition), ItemDefinition (+UseEffects dict, +EquipSlot, +Protection), UseItemCommand (apply UseEffects; chặn khi Incapacitated trừ đồ y tế), CommandErrorCode (+NotAtLocation, Incapacitated, SlotMismatch), GameEvents (+ConditionChanged, StatusEffectChanged), DebugPanel (đọc + cheat set stat — thiết yếu để tune), balance.json (+condition section).

## S8 — Hazard/Flood + Equipment + Travel risk

- `Core/Rules/HazardRules.cs` — flood level = round(lerp(bandMin,bandMax,phaseProgress)) − BaseElevationLevel, clamp 0-4; 1 công thức dùng chung cho HazardSystem (live) + ReturnWindowCalculator (dự báo) — không drift.
- `Core/Rules/TravelRules.cs` — `EvaluateCrossing → {Passable, StaminaCost, ExposureGain, WetGain, TimeFactor, Warnings}` — **deterministic, không RNG chết**: Impassable/Critical/Closed = chặn; còn lại là chi phí tài nguyên + cảnh báo (thiếu stamina → chuyển thành Fatigue, không chặn).
- `Core/Rules/ReturnWindowCalculator.cs` — sample HazardRules theo bước 10 phút trên timeline deterministic → {MinutesUntilWorse, MinutesUntilImpassable}.
- `Core/Commands/EquipItemCommand.cs` — slot body/feet/hands/back/tool (EquipmentSlots có sẵn); dry bag override capacity qua InventoryRules.
- `Systems/Hazard/HazardSystem.cs` — LongTick cập nhật RouteState từ phase, publish `RouteStateChanged` chỉ khi đổi.
- `UI/Map/WorldMapPanel.cs` — mỗi route: ETA, flood/current, equipment warning, return window; nút Travel. Toggle M.
- `Data/Definitions/ProtectionSpec.cs` + `items_p2.json` — jacket (wet ×0.3), boots (chặn exposure ≤Shallow, Medium ×0.5), gloves (cầm đồ bẩn), rope (current −1 band), dry bag (10kg/18L, bảo vệ đồ), medkit.
- Sửa: RouteState fields thật (Flood/Current/Contamination/Electrical/Closure/Modifiers), RouteDefinition (+BaseElevationLevel), BeginTravelCommand (validate Passable + Incapacitated; apply cost sau FF), TransferItemCommand (cầm đồ contaminated không gloves → cho phép nhưng +exposure — triết lý cost-not-block), GameEvents (+EquipmentChanged, WorldMapRequested), SceneSetup (+WorldMapPanel, +ToggleMap).

## S9 — Shelter recovery + HUD + Scenario A–D → **Gate P2**

- `Core/Commands/RestAtShelterCommand.cs` — mode Rest/TreatExposure/DryOff (Treat cần medkit, tiêu thụ; bật cờ cho ConditionSystem decay exposure −5/long-tick).
- `UI/Hud/ConditionHud.cs` — thanh health/stamina/hunger/thirst + badge status (làm sau cùng để S7/S8 verify qua DebugPanel trước).
- `Tests/EditMode/ScenarioTests.cs` — 4 scenario A–D thuần GameContext + command + FastForward (A: dry chỉ đói/khát; B: mưa không jacket → wet→cold, có jacket → không; C: qua Medium không boots → exposure→sick, có boots+gloves → sạch; D: lỡ return window → route Impassable, còn phương án khác).
- Tuning pass từ telemetry + scenario, DebugPanel +phase-jump cheat.

---

## Bảng baseline P2 (tự chốt — tất cả trong balance.json/definitions)

| Nhóm | Giá trị |
| --- | --- |
| Thirst/Hunger | +3.33/+3.1 mỗi giờ game (→ 2 nước + 1.5 đồ ăn/ngày đúng resource economy); bottle −40 thirst, can −50 hunger |
| Fatigue/Stamina | fatigue +0.2/long-tick + 8/chuyến travel; stamina regen +1/phút (×0.5 khi exposure); ngủ: fatigue −10/giờ |
| Body temp | −0.05°C/phút khi wet>50 lúc mưa; +0.1 tại shelter; Cold <35.0, hết >36.0 |
| Đói/khát cạn | health −0.5/long-tick, floor ≥1; Collapsed khi Health ≤5 — không bao giờ chết tức thì |
| Crossing cost theo flood | stamina +0/5/15/30/chặn; exposure +0/5/15/30/—; wet +10/30/60/90/—; time ×1.0/1.2/1.5/2.0/— |
| Exposure threshold | ≥40 → status (regen ×0.5); ≥70 → sick (health −0.5/long-tick); shelter treat: −5/long-tick |
| Phase timeline P2 (session 30–45 phút thực = 150–225 phút game) | dry@0 → first_rain@30 (Flood 0-2) → black_rain@80 (1-3, black water) → route_closure@140 (2-4); route elevation 1 → Impassable ~phút 170 = deadline thật giữa session |
| Save policy | SaveVersion bump mỗi sprint đổi schema (2/3/4/5), load chấp nhận ≤ current; definition mismatch → warn không refuse; mọi thay đổi state đều additive |

## Verification
- Mỗi sprint: compile 0 lỗi + full EditMode suite + build Windows + headless smoke; cập nhật BACKLOG.md/CODEMAP.md cùng commit (S5 ~10 test mới, S6 ~12, S7 ~8, S8 ~12, S9 scenario 4 + roundtrip).
- **Gate P1** (cuối S6): test depletion-qua-save xanh + user chạy build: một chuyến không vét hết store (20kg/35L > 15/25), Take All → triage, quay lại lấy đồ bỏ lại, save/load giữ đúng.
- **Gate P2** (cuối S9): ScenarioTests A–D xanh + user chạy build 30-45 phút: đổi route vì flood, equipment đổi loadout, hiểu return window, không softlock, exposure xử được tại shelter.
- Điểm cần mắt người: HUD/Map/Container panel hiển thị, cảm giác di chuyển khi overload — AI headless không tự xác nhận được.
