# Kế hoạch code hoàn thiện MVP Black Rain (từ sau Sprint 1)

## Context

Sprint 1 (M0 skeleton) đã xong: URP 17.5.0 + Input System 1.20.0 + Newtonsoft, 8 asmdef, 3 scene, CameraRig/PlayerController/GameLog/DebugOverlay/BootLoader, build Windows chạy sạch (headless smoke test pass). Kế hoạch này: (1) chốt cơ chế track tiến độ rẻ token cho AI, (2) sườn tới hoàn thành P7, (3) chi tiết class-level cho M1 (S2–S4) + M2 (S5–S6) đến Gate P1, kèm bộ số liệu baseline phải tự chốt vì design doc không spec.

**Khác biệt doc vs giả định cũ (đã reconcile, áp dụng trong plan này):**
- Thời gian canonical là **`world_time_minutes` (long)** — không phải seconds. Clock giữ accumulator giây game nội bộ, chỉ bank ra phút nguyên. Anchor: Day 0 17:00 = phút 0.
- Command đổi tên `MoveItem` → **`TransferItem`**, thêm **`UseItem`**.
- Checksum/atomic write/3+1 slot là bổ sung của ta (doc không cấm) — giữ.
- Hàng loạt số P1 doc KHÔNG spec (item weight/volume, carry cap, search time, travel time, placement) → bảng baseline bên dưới, tune qua playtest/P0.

## Quyết định tracking: LOCAL, không Jira

Lý do: AI đọc 1 file ~3KB thay vì nhiều MCP call (mỗi issue ~3-4KB JSON); không phụ thuộc auth; tracker commit cùng code nên không lệch. KAN-6..36 để nguyên làm tham chiếu, không cập nhật nữa.

Cơ chế 3 file (mục tiêu: session AI mới nắm toàn bộ hiện trạng mà không dò code):
1. **`docs/backlog/BACKLOG.md`** — trạng thái mọi BL-item, gate results. Cập nhật cùng commit khi item đổi trạng thái.
2. **`docs/backlog/CODEMAP.md`** — bảng compact: Hệ thống → file paths → public API chính → trạng thái test → ghi chú "chưa làm/mock". Cập nhật cùng commit mỗi khi thêm/đổi hệ thống.
3. **`CLAUDE.md`** — protocol: đầu session đọc BACKLOG.md + CODEMAP.md trước khi làm, không re-scan Assets/; cuối mỗi khối việc cập nhật 2 file này cùng commit, message tham chiếu BL-ID.

## Bước 0 — Commit Sprint 1 + hạ tầng plan/tracking (đang thực hiện)

1. Copy plan này vào `docs/plans/` (xong — file hiện tại). Mọi plan tương lai đều lưu `docs/plans/YYYY-MM-DD-<slug>.md`, không chỉ ở `.claude/plans/`.
2. Tạo `docs/backlog/CODEMAP.md` đợt đầu.
3. Cập nhật `CLAUDE.md` với protocol tracking + quy tắc lưu plan.
4. Commit toàn bộ working tree S1 (packages, asmdef, scripts, scenes, URP asset, BACKLOG.md, CODEMAP.md, CLAUDE.md, plan doc) — message tham chiếu BL-P1-01..05. (Builds/ cũ đã gitignore, không đụng.)

## Sườn tới hoàn thành (gate-driven)

| Giai đoạn | Sprint | Nội dung | Gate chặn |
| --- | --- | --- | --- |
| ✅ S1 | M0 | Skeleton, camera, movement, build | M0: build chạy ✅ (chờ user xác nhận hình ảnh) |
| **S2** | M1 | Definition Registry + WorldState + RNG + Serializer | — |
| **S3** | M1 | Clock + Tick + EventBus + Command Layer | — |
| **S4** | M1 | Save + Debug Panel v1 + full test suite | **Gate M1**: sim 24h, save/load roundtrip, tick chính xác |
| **S5** | M2 | Interaction + Item + Inventory (+UI tối thiểu) | — |
| **S6** | M2 | Search + Storage + Travel + Store blockout + Telemetry | **Gate P1**: playtest bỏ-lại-đồ, depletion qua save |
| S7–S9 | P2 | Condition, Flood/Hazard, Equipment, Return Window | Gate P2 |
| S10–S13 | P3 | Shelter State/Build/Task, 5 Module, Power/Water, Sleep | Gate P3 |
| S14–S18 | P4 | Event, Information, NPC Minh, slice content, playtest ngoài | **Gate Go/No-Go** |
| P5→P7 | prod | Content production → Balance → RC | chỉ mở sau P4 Pass; lập plan chi tiết lại tại mỗi gate |

P0 Paper Simulation (BL-P0-01..05) chạy song song — bảng baseline dưới đây chính là input; có thể dựng spreadsheet/script sim khi user muốn.

---

## M1 chi tiết (S2–S4) — thiết kế đã chốt

### Quyết định kiến trúc khóa
- **JSON**: PascalCase C# ↔ snake_case trên đĩa (`SnakeCaseNamingStrategy`, không đụng dictionary key), `StringEnumConverter`, `TypeNameHandling.None`, `MissingMemberHandling.Ignore`, `ObjectCreationHandling.Replace` (tránh append collection), InvariantCulture. Một settings duy nhất trong `WorldStateSerializer`.
- **RNG**: xorshift64* với state `ulong` nằm trong `WorldState.RngStreams` (named streams: "loot", "events", "npc" — derive từ master seed ⊕ FNV1a(name)). Stream state serialize theo WorldState → load xong tiếp tục sequence bit-exact. (Không dùng System.Random vì không expose state.)
- **EventBus**: struct event + `EventChannel<T>` generic, zero-boxing, copy-on-write handler array. 8 signal: WorldTimeChanged, DisasterPhaseChanged, RouteStateChanged, ShelterWarningRaised, TaskCompleted, EventDiscovered, InventoryChanged, NpcStateChanged.
- **Command**: `IGameCommand {ActorId, TargetId, WorldTime; Validate(ctx)→CommandResult; Execute(ctx)}`; `CommandProcessor.Submit` đồng bộ stamp time → Validate → Execute, lỗi trả `CommandErrorCode` (UI map sau). `GameContext {World, Definitions, Events, Rng}` là bundle inject duy nhất.
- **Tick**: `SimulationDriver` (MonoBehaviour duy nhất đọc Time, trong 10_GamePersistent, clamp delta 1s) → `SimulationClock.AccumulateRealSeconds` (×5, bank giây game) → `TickScheduler.Advance(clock, maxCatchUp=60)`; `AdvanceOneMinute()` là NƠI DUY NHẤT tăng WorldTimeMinutes: short tick mỗi phút, long tick `m%10==0`, threshold hook, publish WorldTimeChanged. `FastForward(minutes)` lặp per-minute cho Sleep/Travel.
- **Save**: `Saves/autosave_{0,1,2}.json` (rotation theo savedAtUtc cũ nhất) + `manual_0.json` + `.bak` mỗi slot. Header {save_version, definition_version, saved_at_utc, checksum=SHA256(world payload), slot_id} + `world` (JRaw). Ghi: serialize → tmp → verify đọc lại → move cũ thành .bak → rename. Load: check version/định nghĩa/checksum → fail có message rõ, không silent reset.

### File mới theo assembly (~24 class)
- **Data** (7): `Definitions/{DefinitionBase, ItemDefinition, LocationDefinition, RouteDefinition, SearchPointDefinition}.cs`, `DefinitionRegistry.cs`, `DefinitionLoader.cs` (load theo prefix file `items_*.json`..., `manifest.json` chứa definitionVersion; validate GOM TOÀN BỘ lỗi: ID trùng, dangling ref — không fail-first).
- **Core/State** (5): `WorldState.cs` (kèm stub RouteState/LocationState/ShelterState/NpcState/ActiveEventState/ActiveTaskState), `PlayerState.cs`, `InventoryState.cs`, `ItemInstanceState.cs`, `InventoryOps.cs` (RecalculateLoad/AddItem primitives).
- **Core/Time** (3): `SimulationClock.cs`, `TickScheduler.cs`, `GameTimeUtil.cs` (DayIndex/TimeOfDay từ anchor 17:00).
- **Core/Events** (2): `EventBus.cs`, `GameEvents.cs`.
- **Core/Commands** (3 nhóm): `IGameCommand.cs` (+CommandResult/ErrorCode/GameContext), `CommandProcessor.cs`, command classes: `TransferItemCommand`, `UseItemCommand`, `OpenSearchPointCommand` (thay StartSearch/StopSearch — search mở tức thì, xem S6), `StartSleepCommand` đủ logic M1; `StartTaskCommand`/`CancelTaskCommand`/`BeginTravelCommand` validate + flag, body đầy đủ ở M2.
- **Core/Random** (2): `RngStream.cs`, `RngService.cs`.
- **Core/Save** (3): `WorldStateSerializer.cs`, `SaveFile.cs`, `SaveService.cs`.
- **Systems** (3): `Registry/GameServiceRegistry.cs` (static, fixed set), `Boot/GameBootstrapper.cs` (composition root trong 10_GamePersistent: load defs từ StreamingAssets, fail-fast, dựng services, register), `Boot/SimulationDriver.cs` (Update loop + DebugTimeScale/Paused + autosave định kỳ 300s).
- **DebugTools** (1): `Panel/DebugPanel.cs` (F2, OnGUI: set clock qua FastForward, time scale slider, add item, state dump, save/load).

### Test EditMode (6 file — đây là Gate M1)
`ClockAccumulationTests` (24h vary delta không drift), `TickSchedulerTests` (100 phút = 100 short/10 long, không double-fire, catch-up bounded giữ remainder, FastForward per-minute, threshold fire đúng 1 lần), `DefinitionLoaderTests` (gom đủ lỗi, load fixture hợp lệ), `RngServiceTests` (same seed same sequence, stream độc lập không shift nhau, state sống qua roundtrip), `SaveRoundTripTests` (canonical JSON bằng nhau, .bak đúng, checksum hỏng bị từ chối, definition version mismatch bị từ chối, rotation 0→1→2→0), `CommandPipelineTests` (validate fail không mutate, UseItem publish InventoryChanged).

---

## M2 chi tiết (S5–S6) — Gate P1

### S5 — Interaction + Item + Inventory
- **Systems/Inventory/InventorySystem.cs**: rule CanAdd (weight/volume/overload), stack merge chỉ khi condition/contamination bằng nhau, TransferItem đầy đủ (player↔container↔storage), Drop → ghi vào `LocationState.DroppedItems`.
- **Presentation/Interaction/**: `IInteractable`, `InteractionDetector` (overlap quanh player + raycast con trỏ), prompt hold/cancel.
- **UI/InventoryPanel.cs** (uGUI): list item + 2 thanh weight/volume + drop/transfer. Nghe InventoryChanged, gửi command, không ghi state.
- Definition JSON đợt đầu vào `StreamingAssets/Definitions/`: `manifest.json`, `items_p1.json`, `locations_p1.json`, `routes_p1.json`, `searchpoints_p1.json` theo bảng baseline dưới.

### S6 — Search + Storage + Travel + Content + Telemetry

**Thiết kế Search** (chốt cùng user 2026-07-24) — "thấy hết, lấy hết, quyết định ở sức chứa":
- Search Point = container. Tương tác (E) → panel mở **tức thì**, hiện **toàn bộ** item bên trong; nút Take All + nhặt lẻ từng món.
- KHÔNG progress bar, KHÔNG reveal dần. `SearchPointDefinition.openTimeMinutes` mặc định **0** — cần lever thời gian sau playtest thì chỉnh JSON, không sửa code.
- Nội dung container roll MỘT LẦN khi mở đầu tiên (stream "loot", deterministic theo seed) → ghi vào `LocationState.SearchPointStates[id] = { rolled: bool, remainingItems: [] }`. Đồ không lấy **nằm nguyên trong container vĩnh viễn** (sống qua save/load) → lý do quay lại location cụ thể ("quay lại lấy toolbox với túi rỗng").
- Exit Criteria P1 diễn giải lại (đã cập nhật vào BACKLOG.md): "search dừng giữa chừng vẫn hữu ích" → "một chuyến không thể vét sạch location; phần đã lấy vẫn tạo giá trị; đồ bỏ lại chờ được quay lại lấy".
- **Systems/Search/SearchSystem.cs**: OpenSearchPoint (roll lần đầu qua stream "loot") + TakeItem/TakeAll (qua TransferItemCommand, validate capacity).
- **Systems/Travel/TravelSystem.cs**: BeginTravel validate route+state → transition screen → `TickScheduler.FastForward(travelMinutes × loadFactor)` → đổi CurrentLocationId + load scene. loadFactor: 1.0 thường / 1.25 overload nhẹ / 1.5 overload nặng.
- **Storage**: `ShelterState.StorageContainer` + TransferItem hai chiều + capacity.
- **Systems/Telemetry/TelemetryLogger.cs**: JSONL vào persistentDataPath/Telemetry — subset P1: time_spent, travel_started/completed, search_completed, item_collected (+left_behind qua diff), inventory open time.
- **Scene**: mở rộng `SceneSetup` dựng `41_Location_ConvenienceStore` (blockout primitive: 6 search point) + `20_MainShelter` placeholder (storage + exit).

### Bảng baseline TỰ CHỐT (doc không spec — tune qua P0/playtest, tất cả nằm trong JSON)

| Thông số | Giá trị | Ghi chú |
| --- | --- | --- |
| Carry cap (backpack mặc định) | 15 kg / 25 L | overload >100%: speed ×0.6; >130%: ×0.35; cứng 150% không nhặt thêm |
| item_water_bottle | 0.8 kg / 1.0 L, stack 4 | = 1 Water Unit (0.75L) |
| item_canned_food | 0.4 kg / 0.5 L, stack 8 | = 1 Food Unit |
| item_battery | 0.15 kg / 0.2 L, stack 10 | = 1 Battery Charge |
| item_toolbox (vật nặng) | 8 kg / 12 L, stack 1 | giá trị build tương lai — mồi quyết định bỏ lại |
| item_water_container_20l (cồng kềnh) | 18 kg / 30 L, Carried Object 2 tay | không vào backpack |
| Search: openTimeMinutes | **0** (mở tức thì) | lever JSON, tăng nếu playtest thấy vét quá nhanh |
| Store: 6 search point | kệ nước ×2 (3+3 water), kệ khô ×2 (3+3 food), quầy (2 battery+1 food), kho (toolbox+container+2 water+2 battery) | tổng ~20kg/35L > carry cap 15kg/25L → Take All thất bại → triage |
| Travel shelter↔store | 25 phút game/chiều × loadFactor | fast-forward tick-by-tick |

### Gate P1 checklist (đã cập nhật theo search design mới)
Người chơi bỏ lại ≥1 item giá trị (Take All fail → triage); một chuyến không vét sạch location, phần lấy được vẫn giá trị; đồ bỏ lại nằm nguyên container qua save/load (test tự động: open → take một phần → save → load → container còn đúng phần còn lại, không re-roll); quay lại location có mục đích; không thời gian chết dài; không cần grid.

## Giai đoạn B–E: giữ outline cũ (S7–S9 P2, S10–S13 P3, S14–S18 P4 Go/No-Go, P5–P7 production) — chi tiết hóa lại tại mỗi gate, thêm bảng mới vào BACKLOG.md khi gate trước pass.

## Verification
- Mỗi sprint: Unity batchmode compile 0 lỗi + chạy EditMode tests qua `-runTests -testPlatform EditMode` + build Windows player + smoke test headless; cập nhật BACKLOG.md + CODEMAP.md cùng commit.
- Gate M1 (cuối S4): toàn bộ 6 test file xanh; manual: set clock → add item → save → restart → load → RNG sequence tiếp tục đúng.
- Gate P1 (cuối S6): test tự động depletion-qua-save + user playtest thực tế bằng build (AI không nhìn được hình — user xác nhận cảm giác loot decision).
