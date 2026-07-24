# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Assembly map (dependency một chiều)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI,
                                      Unity.InputSystem, Unity.TextMeshPro, UnityEngine.UI, URP Runtime
```

Presentation và UI (từ S5) đều thêm reference `Unity.InputSystem`, `Unity.TextMeshPro`, `UnityEngine.UI` (cần cho Interaction/InventoryPanel).

## Scene flow

`00_Boot` (BootLoader) → additive `10_GamePersistent` (services + Player/Camera/HUD Canvas sống suốt phiên) → **`SceneFlowController` (S6) load scene gameplay đầu tiên** theo `LocationDefinition.SceneName` của `Player.CurrentLocationId` (mặc định `location_shelter` → `20_MainShelter`), và load/unload lại mỗi khi `TravelCompleted`/`WorldStateReloaded` bắn ra. `90_TestSystems` (Ground/ScaleRef/Light) không còn nằm trong luồng boot tự động — chỉ để mở tay trong Editor kiểm tra scale.

---

## LastHope.Core

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Core/Logging/GameLog.cs` | `GameLog` (static) + `LogCategory` enum | `Info/Warn/Error(LogCategory, string)` | ⬜ |
| `Assets/Game/Core/State/WorldState.cs` | `WorldState` + `RouteState` (S8: real fields — `FloodLevel`/`CurrentLevel`/`Contamination`/`Closed`/`Electrical`(inert)/`Modifiers`(inert, reserved P3+); chỉ là cache hiển thị, HazardSystem ghi mỗi long-tick, BeginTravelCommand KHÔNG đọc field này mà tự tính fresh) + `NpcState`/`ActiveEventState`/`ActiveTaskState` (vẫn stub `Id`+`StatusName`) + `LocationState` (S6) + `ShelterState` (S6) | Root state graph: `WorldTimeMinutes`, `CurrentDisasterPhase`, `RandomSeed`, `RngStreams`, `Player`, `PlaythroughId` (S6), các Dictionary state theo id | 🟡 (gián tiếp qua SearchPointTests/OwnerResolverTests/TravelTests) |
| `Assets/Game/Core/State/SearchPointState.cs` (S6) | `SearchPointState` | `{SearchPointId, Rolled, Inventory:InventoryState}` — roll 1 lần, Inventory là InventoryState đầy đủ nên TransferItemCommand dùng lại nguyên vẹn | ✅ (SearchPointTests) |
| `Assets/Game/Core/State/PlayerState.cs` | `PlayerState` | `ActorId`, `CurrentLocationId`, `Inventory`, **`Condition:PlayerConditionState`** (S7) | ⬜ |
| `Assets/Game/Core/State/PlayerConditionState.cs` (S7) | `PlayerConditionState` + `StatusEffectState` + `IncapacitationState` enum | `Health/Stamina/Fatigue/Hunger/Thirst` 0-100 (Hunger/Thirst: 0=không cần, 100=cạn — ngược chiều Health), `BodyTemperatureC`, `StatusEffects:Dict<id,{Severity,AppliedAtMinute}>`, `Exposures:Dict<hazardId,float>` (cộng dồn, không clamp 0-100), `Incapacitation`, **`TreatingExposure:bool`** (S9, cờ tạm trong phiên `RestAtShelterCommand.TreatExposure`) | ✅ (ConditionOpsTests/ConditionSystemTests/RestAtShelterCommandTests) |
| `Assets/Game/Core/State/InventoryState.cs` | `InventoryState` + `OverloadState` enum | `Items` (instanceId→ItemInstanceState), `CurrentWeightKg/VolumeLiters`, `Overload` (nay được `Systems.Inventory.InventorySystem` set, xem dưới) | ✅ (InventoryRulesTests) |
| `Assets/Game/Core/Rules/ConditionOps.cs` (S7) | `ConditionOps` (static) | Mutator clamp thuần trên `PlayerConditionState`: `ApplyHealth(floor tuỳ chọn)/ApplyStamina/ApplyFatigue/ApplyHunger/ApplyThirst`, `SetStatusSeverity/GetStatusSeverity` (severity≤0 → xoá khỏi dict), `AddExposure/GetExposure` (cộng dồn, floor 0), `ApplyExposureStatusChain` (threshold `BlackWaterExposureThreshold`/`SickExposureThreshold` → status `black_water_exposure`/`sick`, tự xoá khi hồi phục), `RecomputeIncapacitation`/`IsIncapacitated` (Health≤`CollapsedHealthThreshold` → Collapsed). Status id const: `StatusWet/Cold/Bleeding/Sick/BlackWaterExposure/Disoriented` | ✅ (11 test) |
| `Assets/Game/Core/Rules/HazardRules.cs` (S8) | `HazardRules` (static) | `EvaluateRoute(route, phasesByStart, atMinute) → {FloodLevel,CurrentLevel}` — tìm phase hiện tại + phase kế tiếp trong `DisasterPhasesSorted`, lerp band theo tiến độ qua phase kế, trừ `BaseElevationLevel`, clamp 0-`MaxLevel`(4). `ComputeLevel` là hàm thuần tách riêng — DÙNG CHUNG cho HazardSystem (live) và ReturnWindowCalculator (forecast), không drift | ✅ (7 test) |
| `Assets/Game/Core/Rules/TravelRules.cs` (S8) | `TravelRules.EvaluateCrossing` + `CrossingEvaluation` | Tier = max(FloodLevel, CurrentLevel sau khi trừ `EquipmentProtection.CurrentReduction`). Tier≥4 → Impassable (chặn hẳn). Tier 0-3: tra `HazardBalance` array (stamina/exposure/wet/time theo tier), jacket nhân `WetMultiplier` vào wet, boots chặn/giảm exposure theo `BootsBlockLevel`. Thiếu stamina → chỉ warning, KHÔNG chặn (chuyển Fatigue ở BeginTravelCommand) | ✅ (9 test) |
| `Assets/Game/Core/Rules/ReturnWindowCalculator.cs` (S8) | `ReturnWindowCalculator.Evaluate` + `ReturnWindow{MinutesUntilWorse,MinutesUntilImpassable}` | Sample `HazardRules.EvaluateRoute` mỗi 10 phút tới horizon 24h (deterministic, không RNG) — cùng công thức HazardSystem dùng live nên forecast World Map không bao giờ nói sai | ✅ (3 test) |
| `Assets/Game/Core/Rules/EquipmentRules.cs` (S8) | `EquipmentRules` + `EquipmentProtection` (struct) | `SumProtection`/`HasProtection` đọc `ItemDefinition.Protection` của item đang trong `InventoryState.EquipmentSlots`; `ResolveTravelProtection` gộp rope(`current_reduction`)/jacket(`wet_multiplier`)/boots(`exposure_block_level`+`exposure_medium_multiplier`) thành 1 struct cho TravelRules | ✅ (4 test) |
| `Assets/Game/Core/Commands/RestAtShelterCommand.cs` (S9) | `RestAtShelterCommand`, `RestMode` enum | Chỉ dùng được khi `LocationDefinition.IsShelter`. `Rest`: FastForward `ShelterRestMinutes`. `TreatExposure`: cần item Tags chứa "medical" (fail `NoMedicalItem` nếu không) → tiêu thụ 1, áp `ConditionOps.ApplyItemUseEffects` (vd medkit vẫn heal như dùng thường), bật `PlayerConditionState.TreatingExposure` rồi FastForward `ShelterTreatExposureMinutes` — trong lúc đó `ConditionSystem.OnLongTick` tự trừ thêm exposure `ShelterTreatExposureDecayPerLongTick`/long-tick, tắt cờ khi xong. `DryOff`: tức thì, không tốn giờ, xoá status Wet về 0 | ✅ (RestAtShelterCommandTests, 5 test) |
| `Assets/Game/Core/Rules/InventoryRules.cs` | `InventoryRules` (static) | **`EffectiveCapacity(inv,defs,balance)`** (S8, `defs` optional — dry bag equipped ở slot "back" override 10kg/18L thay 15kg/25L mặc định), `ComputeOverload(inv,balance,defs=null)` (dùng EffectiveCapacity), `CanAccept(dest,defs,balance,itemId,qty)` (chặn ở 150% hard cap dùng EffectiveCapacity, container không giới hạn luôn true), `SpeedModifierFor`, `LoadFactorFor` (S6), `IsCapacityLimited(ownerId)` (chỉ "player") | ✅ (8 test cũ + 1 dry-bag) |
| `Assets/Game/Core/State/ItemInstanceState.cs` | `ItemInstanceState` + `ContaminationState`/`WetState` enum | `InstanceId`, `ItemId`, `Quantity`, `Condition`, `Durability`, `Contamination`, `Wet`, `ContainerId` | ⬜ |
| `Assets/Game/Core/State/InventoryOps.cs` | `InventoryOps` (static) | `RecalculateLoad(inv, defs)` (chỉ tính tổng weight/volume, KHÔNG set Overload); `AddItem(inv, defs, itemId, qty, idGen)` (merge stack theo MaxStackSize, không kiểm capacity) | ⬜ |
| `Assets/Game/Core/Random/RngStream.cs` | `RngStream` + `RngStreamState` | xorshift64* trên state `ulong` mutable, `NextInt(min,maxExcl)`, `NextDouble()` | ✅ |
| `Assets/Game/Core/Random/RngService.cs` | `RngService` | `GetStream(name)` — named stream derive từ `WorldState.RandomSeed ⊕ FNV1a64(name)`, state sống trong `WorldState.RngStreams` | ✅ |
| `Assets/Game/Core/Save/WorldStateSerializer.cs` | `WorldStateSerializer` (static) | `Serialize(WorldState)` (indented), `SerializeCanonical(WorldState)` (Formatting.None, dùng cho checksum/deep-compare), `Deserialize(json)`, `Settings` (snake_case, StringEnumConverter, ObjectCreationHandling.Replace) | ✅ |
| `Assets/Game/Core/Save/SaveFile.cs` | `SaveFile`, `SaveSlotInfo` | `SaveFile{SaveVersion,DefinitionVersion,SavedAtUtc,Checksum,SlotId,World(JRaw)}` — World embed verbatim, không re-serialize | ✅ |
| `Assets/Game/Core/Save/SaveService.cs` | `SaveService`, `SaveResult`, `LoadResult` | `Autosave(world)` (round-robin autosave_0/1/2), `SaveToSlot(world,slotId)` (atomic: tmp→verify→backup cũ→rename), `Load(slotId)`, `ListSlots()`. Checksum SHA256 trên world payload canonical | ✅ |
| `Assets/Game/Core/Events/EventBus.cs` | `EventBus` (+ private `EventChannel<T>`) | `Subscribe<T>/Unsubscribe<T>/Publish<T>` — struct event, copy-on-write handler array, zero-boxing | ✅ (gián tiếp qua CommandPipelineTests) |
| `Assets/Game/Core/Events/GameEvents.cs` | `IGameEvent` + 19 struct: `WorldTimeChanged`, `DisasterPhaseChanged`, `RouteStateChanged` (S8: nay `HazardSystem` thật sự publish), `ShelterWarningRaised`, `TaskCompleted`, `EventDiscovered`, `InventoryChanged`, `NpcStateChanged`, `OverloadStateChanged`, `WorldStateReloaded`, `ItemTransferred`, `SearchPointOpened`, `ContainerViewRequested`, `TravelStarted`, `TravelCompleted`, `ConditionChanged`, `StatusEffectChanged` (S7), **`EquipmentChanged`, `WorldMapRequested`** (S8) | ⬜ |
| `Assets/Game/Core/Time/GameTimeUtil.cs` | `GameTimeUtil` (static) | `DayIndex(m)`, `TimeOfDayMinutes(m)`, `Format(m)` — anchor Day 0 17:00 = phút 0 | ⬜ (gián tiếp qua TickSchedulerTests) |
| `Assets/Game/Core/Time/SimulationClock.cs` | `SimulationClock` | `AccumulateRealSeconds(double)`, `TryConsumeMinute()`, `PendingGameSeconds`. **Bank dùng `decimal` nội bộ** (không phải double) — double cộng dồn ~17k lần bị lệch 1 phút/24h, xem comment trong file | ✅ |
| `Assets/Game/Core/Time/TickScheduler.cs` | `TickScheduler` | `SubscribeShort/Long(Action<long>)`, `RegisterThreshold(minute, cb)`, `Advance(clock, maxMinutes)` (bounded catch-up), `FastForward(minutes)` (Sleep/Travel). `AdvanceOneMinute()` private — NƠI DUY NHẤT tăng `WorldTimeMinutes` | ✅ |
| `Assets/Game/Core/Commands/IGameCommand.cs` | `IGameCommand`, `CommandResult`, `CommandErrorCode` (+`NotAtLocation` S6, +**`Incapacitated`, `SlotMismatch`** S7 — SlotMismatch chưa dùng, đặt trước cho S8 EquipItemCommand), `GameContext` | `GameContext{World,Definitions,Events,Rng,Clock}` — bundle inject duy nhất | ✅ |
| `Assets/Game/Core/Commands/CommandProcessor.cs` | `CommandProcessor` | `Submit(IGameCommand) → CommandResult` — stamp WorldTime, Validate→Execute, log lỗi qua GameLog | ✅ |
| `Assets/Game/Core/Commands/InventoryOwnerResolver.cs` | `InventoryOwnerResolver` (**public** static, đổi từ internal ở S6 để UI đọc được) | `TryResolve(ctx, ownerId, out inv)` — scheme: `"player"`, `"searchpoint:<id>"` (KHÔNG tự tạo state, fail nếu chưa `Rolled`), `"shelter_storage:<id>"` (lazy-create), `"location_dropped:<id>"` (lazy-create). Để dành `"npc:<id>"` | ✅ (OwnerResolverTests, 6 test) |
| `Assets/Game/Core/Commands/UseItemCommand.cs` | `UseItemCommand` | Giảm quantity item trong inventory actor, publish `InventoryChanged`. S7: Validate chặn owner=player+Incapacitated trừ item có Tag "medical" (→`Incapacitated`); Execute áp `ItemDefinition.UseEffects` vào `Player.Condition` qua `ConditionOps`, publish `ConditionChanged` nếu có đổi | ✅ |
| `Assets/Game/Core/Commands/TransferItemCommand.cs` | `TransferItemCommand` | Chuyển item giữa 2 owner đã biết; move nguyên instance nếu chuyển hết quantity (giữ Condition/Contamination/Wet), chỉ split khi chuyển một phần. Validate kiểm `InventoryRules.CanAccept` → fail `InventoryFull`; Execute publish thêm `ItemTransferred`. S8: đích=player + item Contaminated + không gloves (`EquipmentRules.HasProtection "handles_contaminated"`) → +exposure "black_water" (cost-not-block, vẫn cho chuyển) | ✅ |
| `Assets/Game/Core/Commands/EquipItemCommand.cs` (S8) | `EquipItemCommand`, `UnequipItemCommand` | Equip: validate `ItemDefinition.EquipSlot` khớp slot yêu cầu (sai → `SlotMismatch`, không equippable → `InvalidTarget`) → ghi `InventoryState.EquipmentSlots[slot]=instanceId` (item vẫn nằm trong Items, equip chỉ là tham chiếu slot, không di chuyển). Unequip: xoá slot (rỗng sẵn → `SlotMismatch`). Cả 2 publish `EquipmentChanged` | ✅ (6 test) |
| `Assets/Game/Core/Commands/StartSleepCommand.cs` | `StartSleepCommand` | `ctx.Clock.FastForward(Minutes)` — **chưa có interrupt-on-event** (chờ Event System M3+) | ⬜ |
| `Assets/Game/Core/Commands/OpenSearchPointCommand.cs` (S6; hybrid loot 2026-07-24) | `OpenSearchPointCommand` | Validate: đúng location (`NotAtLocation`). Execute: roll MỘT LẦN qua stream "loot" (bỏ qua nếu đã `Rolled`) — entry `Guaranteed` luôn spawn, còn lại theo `loot.NextInt(0,100) < entry.Chance`; `OpenTimeMinutes>0` → FastForward, publish `SearchPointOpened`+`ContainerViewRequested` | ✅ (SearchPointTests, 8/8) |
| `Assets/Game/Core/Commands/TaskCommands.cs` | `StartTaskCommand`, `CancelTaskCommand` (vẫn chỉ validate+flag, chờ Shelter Task S10+), `BeginTravelCommand` (S6 đầy đủ; **S8**: Validate thêm chặn Incapacitated + `TravelRules.EvaluateCrossing` không Passable → `RouteBlocked`; Execute: `minutes = ceil(TravelMinutes × loadFactor × crossing.TimeFactor)`, sau FastForward áp cost — stamina (thiếu hụt → Fatigue, không chặn), exposure "black_water", wet — qua `ConditionOps`, publish `ConditionChanged`. Hazard tính FRESH mỗi lần gọi qua `HazardRules`/`EquipmentRules`, KHÔNG đọc `RouteState` cache) | ✅ (TravelTests, 9 test cho BeginTravel) |

## LastHope.Data

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Data/Definitions/DefinitionBase.cs` | `DefinitionBase` (abstract) | `Id`, `DisplayNameKey`, `DataVersion` | ⬜ |
| `Assets/Game/Data/Definitions/ItemDefinition.cs` | `ItemDefinition` | `Category`, `BaseWeightKg`, `BaseVolumeLiters`, `MaxStackSize`, `MaxDurability`, `WaterResistance`, `Tags`, `TwoHandCarry` (S5), `UseEffects:Dict<string,float>` (S7), `EquipSlot` (S7 placeholder, **dùng thật từ S8** bởi `EquipItemCommand`: "body"/"feet"/"hands"/"back"/"tool"), `Protection:Dict<string,float>` (S7 placeholder, **dùng thật từ S8** — key: `current_reduction` rope, `wet_multiplier` jacket, `exposure_block_level`+`exposure_medium_multiplier` boots, `handles_contaminated` gloves, `backpack_capacity_kg`+`backpack_capacity_liters` dry bag. **Không có `ProtectionSpec.cs` riêng** — dict đã đủ, tránh trùng lặp cấu trúc, xem ghi chú cuối file) | 🟡 (qua ContentValidationTests) |
| `Assets/Game/Data/Definitions/LocationDefinition.cs` | `LocationDefinition` | `SearchPointIds`, `ConnectedRouteIds`, `SceneName` (S6), **`IsShelter`** (S7, data-driven cho ConditionSystem body-temp regen/wet-dry — `location_shelter` là location duy nhất `true`) | 🟡 |
| `Assets/Game/Data/Definitions/DisasterPhaseDefinition.cs` (S7) | `DisasterPhaseDefinition` | `StartMinute`(long), `FloodBandMin/Max`, `CurrentBandMin/Max` (chưa dùng tới S8 Hazard), `BlackWater`, `RainIntensity`. Content `phases_p2.json`: 4 phase (dry@0, first_rain@30, black_rain@80, route_closure@140) | 🟡 (qua ContentValidationTests + DisasterPhaseSystemTests) |
| `Assets/Game/Data/Definitions/RouteDefinition.cs` | `RouteDefinition` | `FromLocationId`, `ToLocationId`, `TravelMinutes`, **`BaseElevationLevel`** (S8, trừ vào flood/current band trước khi clamp — route cao ráo ít ngập hơn) | 🟡 |
| `Assets/Game/Data/Definitions/SearchPointDefinition.cs` | `SearchPointDefinition` + `LootEntry` | `LocationId`, `OpenTimeMinutes` (mặc định 0 — search mở tức thì), `LootTable` (List\<LootEntry\>: ItemId/**Guaranteed**/**Chance**(0-100)/MinQuantity/MaxQuantity — hybrid 2026-07-24: Guaranteed luôn spawn, còn lại roll theo Chance; **`Weight` đã xoá** — knob cũ chết, mọi entry luôn spawn bất kể giá trị) | 🟡 |
| `Assets/Game/Data/BalanceConfig.cs` (S5; +`ConditionBalance` S7; +`HazardBalance` S8) | `BalanceConfig`, `InventoryBalance`, `TravelBalance`, `NewGameBalance`, `ConditionBalance`, `HazardBalance` | Object config duy nhất — capacity, overload, speed modifier, travel load factor, start location, condition (thirst/hunger/fatigue/stamina/body-temp/wet/cold/exposure/starvation/collapsed **+ shelter_rest_minutes/shelter_treat_exposure_minutes/shelter_treat_exposure_decay_per_long_tick S9**), hazard (crossing stamina/exposure/wet/time theo tier, contaminated handling exposure) | ✅ (BalanceLoadTests) |
| `Assets/Game/Data/DefinitionRegistry.cs` | `DefinitionRegistry` | `DefinitionVersion`, `Balance`, `Items/Locations/Routes/SearchPoints/DisasterPhases` (S7, IReadOnlyDictionary), **`DisasterPhasesSorted:IReadOnlyList`** (S8, sort 1 lần theo `StartMinute` lúc construct — `DisasterPhaseSystem`/`HazardSystem`/`ReturnWindowCalculator`/`BeginTravelCommand` đều dùng chung list này, không tự sort riêng), `TryGetItem/Location/Route/SearchPoint/DisasterPhase`. Ctor: `disasterPhases` param optional (default empty dict) | ✅ (qua DefinitionLoaderTests) |
| `Assets/Game/Data/DefinitionLoader.cs` | `DefinitionLoader` (static) | `Load(directoryPath) → DefinitionLoadResult{Success,Registry,Errors}`. Routing theo prefix file: `manifest.json`, `items_*.json`, `locations_*.json`, `routes_*.json`, `searchpoints_*.json`, `balance.json` (S5), **`phases_*.json`** (S7 — rỗng hợp lệ; nếu có thì bắt buộc unique `start_minute` + có phase tại phút 0). Gom TOÀN BỘ lỗi (duplicate id, dangling ref, missing id) — không fail-first | ✅ |

## LastHope.Systems

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Systems/Registry/GameServiceRegistry.cs` | `GameServiceRegistry` (static) | `Register<T>`, `Get<T>`, `TryGet<T>`, `Clear()` — service locator giới hạn, chỉ `GameBootstrapper` ghi | ⬜ |
| `Assets/Game/Systems/Boot/GameBootstrapper.cs` | `GameBootstrapper` (MonoBehaviour, sống trong `10_GamePersistent`) | Composition root: load Definitions từ `StreamingAssets/Definitions`, fail-fast nếu lỗi (dừng boot, `enabled=false`), tạo `WorldState` mới + seed, set `Player.CurrentLocationId = Balance.NewGame.StartLocationId` (S5), dựng toàn bộ Core service + `InventorySystem` (S5) + `DisasterPhaseSystem`, `ConditionSystem` (S7) + **`HazardSystem`** (S8), đăng ký vào `GameServiceRegistry` | ⬜ (verify qua headless smoke test, chưa có PlayMode test) |
| `Assets/Game/Systems/Boot/SimulationDriver.cs` | `SimulationDriver` (MonoBehaviour) | Cầu nối Unity Time → Core: đọc service ở `Start()` (không phải `Awake()`, tránh phụ thuộc thứ tự component), `Update()` clamp delta 1s, gọi `SimulationClock.AccumulateRealSeconds` + `TickScheduler.Advance`. `DebugPaused`/`DebugTimeScale` cho tooling | ⬜ (verify qua headless smoke test 10s không exception) |
| `Assets/Game/Systems/Inventory/InventorySystem.cs` (S5) | `InventorySystem` (plain C#) | Nghe `InventoryChanged` (chỉ owner "player") → `InventoryRules.ComputeOverload` → nếu đổi thì set `Inventory.Overload` + publish `OverloadStateChanged`. `RecomputeAll()` gọi 1 lần lúc boot | ✅ (qua InventoryRulesTests) |
| `Assets/Game/Systems/Disaster/DisasterPhaseSystem.cs` (S7) | `DisasterPhaseSystem` (plain C#) | Sắp `DisasterPhases` theo `StartMinute`; construct + nghe `WorldStateReloaded` → resync (recompute phase hiện tại từ `WorldTimeMinutes` + đăng ký lại threshold tương lai qua `ctx.Clock.RegisterThreshold`). `TransitionTo` idempotent (no-op nếu phase không đổi) nên double-register threshold sau reload vô hại | ✅ (DisasterPhaseSystemTests, 5 test) |
| `Assets/Game/Systems/Condition/ConditionSystem.cs` (S7) | `ConditionSystem` (plain C#) | ShortTick: stamina regen (×0.5 khi status `black_water_exposure`/`sick`), wet gain khi mưa ngoài trời (**S9: nhân thêm `EquipmentRules` wet_multiplier — jacket giờ CHỐNG ĐƯỢC mưa cả lúc đứng yên, không chỉ lúc crossing route**)/khô tại shelter, body temp drift, cold status hysteresis. LongTick: hunger/thirst/fatigue accrual, **S9: nếu `PlayerConditionState.TreatingExposure` → trừ thêm exposure trước khi tính chain**, `ConditionOps.ApplyExposureStatusChain`, health decay khi đói/khát cạn (floor 1) + khi sick (không floor). Nghe `TravelCompleted` → +fatigue cố định. Nguồn "black_water" exposure thật (S8): `BeginTravelCommand`, `TransferItemCommand` | ✅ (ConditionSystemTests 9 + ScenarioTests B 2 test) |
| `Assets/Game/Systems/Hazard/HazardSystem.cs` (S8) | `HazardSystem` (plain C#) | Mỗi long-tick + `WorldStateReloaded`: tính lại `RouteState` cho MỌI route qua `HazardRules.EvaluateRoute`, publish `RouteStateChanged` CHỈ khi Flood/Current/Contamination/Closed đổi. Đây CHỈ là cache hiển thị cho World Map — có thể trễ tới 9 phút; validate travel thật nằm ở `BeginTravelCommand` (tự tính fresh, không đọc RouteState) | ⬜ (verify gián tiếp qua TravelTests + headless smoke) |
| `Assets/Game/Systems/Telemetry/TelemetryLogger.cs` (S6) | `TelemetryLogger` | JSONL `persistentDataPath/Telemetry/session_*.jsonl` (`File.AppendAllText`, không giữ file handle). Nghe `TravelStarted/Completed` (kèm carry load lúc về), `SearchPointOpened`, `ItemTransferred` (chỉ log khi đích = player → `item_collected`). `Log()` public cho UI gọi thêm (chưa dùng — `item_left_behind`/`inventory_open_time` để dành cho `ContainerPanel`/`InventoryPanel` sau) | ✅ (TelemetryTests) |

## LastHope.DebugTools (bổ sung S4)

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Panel/DebugPanel.cs` | `DebugPanel` (MonoBehaviour, OnGUI, F2) | Xem World Time, Fast-forward clock, Pause/TimeScale, Condition cheat (S7), Equipment (S8, qua command thật), **Rest at Shelter** (S9: nút Rest/Treat Exposure/Dry Off → `RestAtShelterCommand` thật, không bypass), **Phase jump cheat** (S9: nút nhảy thẳng tới từng phase trong `DisasterPhasesSorted`, set `WorldTimeMinutes` rồi publish `WorldStateReloaded` để `DisasterPhaseSystem`/`HazardSystem` resync), Add Item (bypass), Travel cheat, Save/Autosave + Load theo slot, state tree dump | ⬜ (chưa test tự động, chỉ verify code compile + wiring scene) |

## LastHope.Presentation

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Presentation/Camera/CameraRig.cs` | `CameraRig` | Orthographic iso cố định (pitch 35.264°/yaw 45°), zoom qua Input System action "Zoom". `SetTarget(Transform)`, `SetInputActions(InputActionAsset)` | ⬜ (chỉ headless smoke test, chưa unit test) |
| `Assets/Game/Presentation/Player/PlayerController.cs` | `PlayerController` | CharacterController, di chuyển theo hướng camera (screen-relative), framerate-độc lập. `SpeedModifier` (S5: nay được `PlayerAvatarSync` set theo `OverloadStateChanged`), `SetCameraTransform`, `SetInputActions` | ⬜ |
| `Assets/Game/Presentation/Player/PlayerAvatarSync.cs` (S5) | `PlayerAvatarSync` | **Presentation-write exemption** có chủ đích: `LateUpdate` ghi transform→`PlayerState.PositionX/Y/Z` mỗi frame. **KHÔNG tự ghi `PositionLocationId`** (fix bug rơi-khỏi-map 2026-07-24 — field này chỉ được `SceneFlowController` stamp sau khi đặt spawn xong, tránh race). Nghe `WorldStateReloaded` → teleport lại nếu `PositionLocationId==CurrentLocationId` (khác thì để yên, `SceneFlowController` lo). Nghe `OverloadStateChanged` → set `PlayerController.SpeedModifier` | ⬜ |
| `Assets/Game/Presentation/Interaction/IInteractable.cs` (S5) | `IInteractable` | `PromptText`, `CanInteract(ctx)`, `Interact(ctx,processor)` — tương tác tức thì (E), docs không spec hold-duration | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionDetector.cs` (S5) | `InteractionDetector` | `OverlapSphere` bán kính 1.6m mỗi 0.15s + cursor raycast tiebreak (ưu tiên object con trỏ trỏ vào). Đọc action "Interact" có sẵn, `Current`/`TargetChanged`. S6: nay có interactable thật (SearchPointView×6, ShelterStorageView, TravelPointView×2) | ⬜ |
| `Assets/Game/Presentation/Interaction/InteractionPrompt.cs` (S5) | `InteractionPrompt` | TextMeshProUGUI "E — {prompt}", nghe `InteractionDetector.TargetChanged` | ⬜ |
| `Assets/Game/Presentation/World/SearchPointView.cs` (S6) | `SearchPointView` (`IInteractable`) | `[SerializeField] searchPointId` bind với `SearchPointDefinition`. Interact → `OpenSearchPointCommand` | ⬜ |
| `Assets/Game/Presentation/World/ShelterStorageView.cs` (S6) | `ShelterStorageView` (`IInteractable`) | `[SerializeField] shelterId`. Interact → publish `ContainerViewRequested` trực tiếp (không cần command để "nhìn") | ⬜ |
| `Assets/Game/Presentation/World/TravelPointView.cs` | `TravelPointView` (`IInteractable`) | S6: submit `BeginTravelCommand` thẳng cho 1 route hardcode. **S8: đổi hẳn** — Interact → publish `WorldMapRequested`, không còn giữ `routeId` (đã xoá field/`SetRouteId`, `WorldMapPanel` tự đọc `LocationDefinition.ConnectedRouteIds` của vị trí hiện tại) | ⬜ |
| `Assets/Game/Presentation/World/PlayerSpawnPoint.cs` (S6) | `PlayerSpawnPoint` | Marker rỗng — `SceneFlowController` dùng khi không có vị trí save khớp scene mới | ⬜ |
| `Assets/Game/Presentation/Boot/BootLoader.cs` | `BootLoader` (MonoBehaviour, sống trong `00_Boot`) | S6: chỉ load `10_GamePersistent` additive — KHÔNG còn hard-code scene gameplay thứ 2 (đó là việc của `SceneFlowController`) | ⬜ |
| `Assets/Game/Presentation/Boot/GamePersistentMarker.cs` | `GamePersistentMarker` | `DontDestroyOnLoad` cho root scene `10_GamePersistent`, chặn instance thứ 2 | ⬜ |
| `Assets/Game/Presentation/Boot/SceneFlowController.cs` (S6) | `SceneFlowController` | Chủ sở hữu DUY NHẤT vòng đời scene gameplay: nghe `TravelCompleted`/`WorldStateReloaded` → load additive scene theo `LocationDefinition.SceneName` của `Player.CurrentLocationId`, unload scene cũ, đặt player ở `PlayerSpawnPoint` nếu `PositionLocationId != CurrentLocationId`. **Là nơi DUY NHẤT được phép stamp `player.PositionLocationId = CurrentLocationId`** sau khi đặt xong (fix bug rơi-khỏi-map — xem PlayerAvatarSync). Log ở mỗi bước, verify qua headless smoke: `"placed player at spawn (0.00, 0.10, 0.00) for 'location_shelter'"` | ⬜ (verify qua smoke test, chưa PlayMode test) |

## LastHope.UI

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/UI/Inventory/InventoryPanel.cs` (S5) | `InventoryPanel` | Panel code-built hoàn toàn (không prefab): list phẳng item + 2 thanh weight/volume màu theo Overload (xanh/cam/đỏ), nút **Use** (qua `UseItemCommand`). **Drop vẫn chưa làm** (owner `location_dropped:` đã tồn tại từ S6 nhưng UI Drop button chưa nối). Toggle phím I/Tab | ⬜ |
| `Assets/Game/UI/Container/ContainerPanel.cs` (S6) | `ContainerPanel` | 1 panel dùng chung search point + shelter storage. Nghe `ContainerViewRequested` → hiện list container (đọc qua `InventoryOwnerResolver`, chỉ đọc — mọi mutate qua `TransferItemCommand`): nút Take/Take All. Nếu owner là `shelter_storage:` → hiện thêm khối "Your Inventory" với nút Store (2 chiều) | ⬜ |
| `Assets/Game/UI/Hud/ConditionHud.cs` (S9) | `ConditionHud` | HUD luôn hiện (không toggle) — 4 thanh Health/Stamina/Hunger/Thirst + dòng badge status (`StatusEffects` keys + "collapsed" nếu Incapacitated). Rebuild khi nghe `ConditionChanged`. Đây là bản player-facing của Condition section trong DebugPanel (S7 làm trước để verify bằng F2 trước khi xây UI thật) | ⬜ (chưa test tự động, chỉ verify code compile + wiring scene) |
| `Assets/Game/UI/Map/WorldMapPanel.cs` (S8) | `WorldMapPanel` | Nghe `WorldMapRequested` (từ `TravelPointView`) hoặc phím **M** trực tiếp. Liệt kê `LocationDefinition.ConnectedRouteIds` của vị trí hiện tại: mỗi route hiện đích, ETA (tính lại `TravelRules`/`InventoryRules.LoadFactorFor`), Flood/Current level, cảnh báo IMPASSABLE, return window (`ReturnWindowCalculator`) — nút Travel submit `BeginTravelCommand` rồi tự rebuild list | ⬜ (chưa test tự động, chỉ verify code compile + wiring scene) |

## LastHope.DebugTools

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `DebugOverlay` | OnGUI overlay, toggle **F1**: FPS, world position, build version. Tự tìm GameObject tag "Player" nếu chưa gán. **Chưa hiển thị World Clock/State** (đó là `DebugPanel` ở S4, phím F2) | ⬜ |

## LastHope.EditorTools (Editor-only, không build vào Player)

| File | Class | API chính | Ghi chú |
| --- | --- | --- | --- |
| `Assets/Game/EditorTools/SceneSetup.cs` | `SceneSetup` | `[MenuItem] BuildAll()` — dựng lại 5 scene từ code, đăng ký Build Settings: `00_Boot`, `10_GamePersistent` (Player+Camera+HUD Canvas: EventSystem, InputSystemUIInputModule, **ConditionHud** S9, InteractionPrompt, InventoryPanel, ContainerPanel, WorldMapPanel, SceneFlowController), `90_TestSystems` (chỉ Ground/Light, không tự động load), `Shelters/20_MainShelter` (ShelterStorageView "shelter_main", TravelPointView, PlayerSpawnPoint), `Locations/41_Location_ConvenienceStore` (6 SearchPointView đúng id JSON, TravelPointView, PlayerSpawnPoint) | Chạy lại bất cứ khi nào cần tái tạo scene từ đầu (deterministic) |
| `Assets/Game/EditorTools/RenderPipelineSetup.cs` | `RenderPipelineSetup` | `[MenuItem] Setup()` — tạo `Assets/Settings/LastHope_URP.asset` + Renderer, gán Graphics+Quality, Linear color space | Đã chạy 1 lần, asset đã tồn tại — chạy lại thì tái sử dụng asset cũ (idempotent) |
| `Assets/Game/EditorTools/BuildScript.cs` | `BuildScript` | `[MenuItem] BuildWindowsDevelopment()` → `Builds/Windows/LastHope.exe`, Mono, Development build | Dùng làm smoke test nhanh sau mỗi sprint |
| `Assets/Game/EditorTools/TmpSetup.cs` (S5) | `TmpSetup` | `[MenuItem] ImportEssentials()` — import "TMP Essential Resources.unitypackage" từ PackageCache (TMP Settings + LiberationSans SDF font). **Chạy KHÔNG kèm `-quit`** — `AssetDatabase.ImportPackage` là async, dùng callback `importPackageCompleted` để tự `EditorApplication.Exit` | Đã chạy 1 lần, asset đã có ở `Assets/TextMesh Pro/` — không cần chạy lại trừ khi asset đó bị xoá |

## Input

| File | Nội dung |
| --- | --- |
| `Assets/Input/GameControls.inputactions` | Action map "Gameplay": `Move` (Vector2, WASD composite), `Zoom` (Axis, scroll), `Interact` (Button, E — `InteractionDetector`), `ToggleInventory` (Button, I/Tab — `InventoryPanel`), **`ToggleMap`** (Button, phím M — `WorldMapPanel`, S8) |

## Data định nghĩa game — CONTENT THẬT + SCENE ĐÃ NỐI (S5+S6+S7+S8+S9)

`Assets/StreamingAssets/Definitions/` — `manifest.json` (`definition_version: 0.5.0`) + `README.md` + content thật:
- `items_p1.json` — 5 item: `item_water_bottle` (0.8kg/1.0L st4, `use_effects.thirst:-40`), `item_canned_food` (0.4/0.5 st8, `use_effects.hunger:-50`), `item_battery` (0.15/0.2 st10), `item_toolbox` (8/12 st1), `item_water_container_20l` (18/30 st1, `two_hand_carry:true`).
- `items_p2.json` (S8, mới) — 6 item trang bị: `item_jacket` (body, `wet_multiplier:0.3`), `item_boots` (feet, `exposure_block_level:1`+`exposure_medium_multiplier:0.5`), `item_gloves` (hands, `handles_contaminated:1`), `item_rope` (tool, `current_reduction:1`), `item_dry_bag` (back, `backpack_capacity_kg:10`+`backpack_capacity_liters:18`), `item_medkit` (tag "medical", `use_effects.health:50`). **Chưa gắn vào searchpoint nào** — chưa có cách nhặt trong gameplay thật, chỉ debug Add Item.
- `locations_p1.json` — `location_shelter` (scene `20_MainShelter`, `is_shelter:true` S7), `location_convenience_store` (scene `41_Location_ConvenienceStore`), nối bởi 1 route.
- `routes_p1.json` — `route_shelter_store` 25 phút game, `base_elevation_level` mặc định 0 (S8, field mới).
- `searchpoints_p1.json` — 6 điểm gắn với `location_convenience_store` (2 kệ nước, 2 kệ khô guaranteed, quầy/kho có entry theo `chance` — xem hybrid loot S6-follow-up 2026-07-24).
- `balance.json` — khớp default `BalanceConfig` + section `condition` (S7, +3 field shelter recovery S9) + section `hazard` (S9, viết tường minh — giá trị khớp default C# đã có từ S8, hành vi không đổi).
- `phases_p2.json` — 4 `DisasterPhaseDefinition`: dry@0, first_rain@30, black_rain@80, route_closure@140 (game-minute) — S8: nay thật sự lái Flood/Current level qua `HazardRules`.

✅ Từ S6: content này KHÔNG CÒN chỉ là dữ liệu — mỗi search point/route/location đều có GameObject/scene thật dùng nó (xác nhận qua headless smoke: boot → SceneFlowController load đúng `20_MainShelter`).

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems` → `Shelters/20_MainShelter` → `Locations/41_Location_ConvenienceStore`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- **M1 (S2-S4) PASS**, **S5/S6 PASS → Gate P1 PASS (2026-07-24)**; **S7/S8 PASS**; **S9 PASS → Gate P2 PASS về mặt kỹ thuật (2026-07-24)**: 127/127 EditMode test, build Windows 0 lỗi, headless smoke xác nhận boot sạch với toàn bộ hệ thống P2. P2 (Condition/Disaster Phase/Hazard/Equipment/Travel risk/Shelter recovery/Scenario A-D) coi như xong về kỹ thuật — xem `docs/backlog/BACKLOG.md` mục Gate P2 để biết phần cần user tự chơi thử xác nhận.
- `DebugOverlay` (F1) và `DebugPanel` (F2) là 2 file khác nhau — F1 luôn hiện (FPS/vị trí), F2 toggle riêng (World Time/Save/Condition cheat/Equipment/Rest at Shelter/Phase jump/Add Item/Travel cheat/state dump).
- `StartTaskCommand`/`CancelTaskCommand` vẫn chỉ validate + ghi flag/log, KHÔNG có effect thật — đó là việc của Shelter Task (S10+, xem `docs/plans/2026-07-24-p3-p4-completion-plan.md`). `BeginTravelCommand` ĐÃ đầy đủ từ S6+S8.
- `StartSleepCommand` fast-forward clock nhưng KHÔNG kiểm tra event/interrupt (chờ S12 viết lại — Sleep Simulation nằm trong plan P3, không phải P2). Ngủ hiện tại vẫn tích luỹ hunger/thirst/fatigue bình thường qua ShortTick/LongTick trong lúc FastForward — không có gì đặc biệt khác `RestAtShelterCommand.Rest` ngoài thời lượng.
- `black_water` exposure có 3 nguồn thật: crossing route ngập (`BeginTravelCommand` S8), cầm đồ Contaminated không gloves (`TransferItemCommand` S8), và giảm qua `RestAtShelterCommand.TreatExposure` (S9) — không còn dormant.
- `items_p2.json` (jacket/boots/gloves/rope/dry_bag/medkit) **vẫn chưa gắn vào searchpoint/loot table nào** — chưa có cách người chơi NHẶT được trong gameplay thật, chỉ test qua debug Add Item + unit test. Đặt loot thật là việc content của P3+.
- **Không có `ProtectionSpec.cs` riêng** dù plan gốc S8 có nhắc tên file này — dùng `ItemDefinition.Protection:Dict<string,float>` (có sẵn từ S7) đủ biểu diễn mọi hiệu ứng cần, tránh 1 lớp cấu trúc song song.
- `RouteState` (cache hiển thị, HazardSystem ghi mỗi long-tick) và giá trị THẬT dùng để validate travel (`BeginTravelCommand` tự tính qua `HazardRules` tại đúng thời điểm) là 2 con đường tách biệt có chủ đích — đừng sửa `BeginTravelCommand` để đọc `RouteState` tưởng là tối ưu, nó sẽ tái tạo lại vấn đề "stale 9 phút".
- `RouteState.Electrical`/`Modifiers` (S8) là field rỗng chưa ai dùng — đặt trước cho Power System/Shelter Event (P3+).
- **Chưa playtest thật bằng tay/mắt** — mọi xác nhận ở trên là test tự động + headless smoke (không render hình). User nên tự mở Editor hoặc chạy `Builds/Windows/LastHope.exe` để chơi thử theo đúng kịch bản Gate P2 (30-45 phút): đổi route vì flood, đổi loadout trang bị, hiểu return window trên World Map (M), xử lý exposure tại shelter (Rest/Treat/Dry Off qua F2), xem ConditionHud hiển thị đúng góc trái trên màn hình.
- `InventoryPanel` chưa có nút Drop dù owner `location_dropped:` đã hoạt động được — chỉ thiếu nút UI.
- `ContainerPanel`/`InventoryPanel` chưa gọi `TelemetryLogger.Log()` cho `item_left_behind`/`inventory_open_time` — API có sẵn, chỉ chưa được UI gọi tới.
- Chưa có interactable trong world để trigger `RestAtShelterCommand`/`StartSleepCommand` — cả 2 chỉ gọi được qua Debug Panel/test tới lúc này, giống tiền lệ Sleep từ M1. Interactable thật (vd tương tác với giường/khu nghỉ) là việc UI/content của P3+.
- **P3 (Shelter Loop, S10-S13) và P4 (Vertical Slice, S14-S18) CHƯA VIẾT GÌ** — plan chi tiết đã có ở `docs/plans/2026-07-24-p3-p4-completion-plan.md`.

## Ghi chú kỹ thuật quan trọng (tránh dò lại code để hiểu "tại sao")

- RNG dùng xorshift64* tự viết (không dùng `System.Random`) vì cần expose state để serialize và tiếp tục sequence bit-exact sau load — xem `RngStream.cs`.
- `DefinitionLoader` không ném exception cho lỗi data (chỉ throw nếu JSON không đọc được, và exception đó cũng bị bắt + gom vào `Errors`). Gọi `Load()` luôn trả về `DefinitionLoadResult`, không bao giờ throw ra ngoài với input hợp lệ về mặt cấu trúc file. `balance.json` là NGOẠI LỆ — thiếu/lỗi parse fallback default, không tính vào `Errors`.
- Naming JSON trên đĩa là **snake_case**, nhưng C# property là PascalCase — đừng thêm `[JsonProperty]` thủ công, `SnakeCaseNamingStrategy` tự convert.
- **TextMeshPro cần "TMP Essential Resources" import trước khi dùng** — nếu thiếu, mọi `TextMeshProUGUI.Awake()` throw NullReferenceException lúc runtime (không phải lúc compile!). Đã import (`Assets/TextMesh Pro/`), đừng xoá folder đó.
- `PlayerAvatarSync` là **ngoại lệ có chủ đích** của nguyên tắc "mọi thay đổi state qua Command" — vị trí liên tục (continuous data) không phải rule, ghi thẳng vào `PlayerState.Position*` mỗi frame. Đừng dùng pattern này cho state rời rạc khác.
- SceneSetup giờ tạo UI 100% bằng code (không prefab) — `InventoryPanel`/`InteractionPrompt` tự dựng hierarchy trong `Awake()`/`BuildLayout()`, không phụ thuộc asset `.prefab` nào.
