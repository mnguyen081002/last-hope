# Plan S18 — Outcome + Causal Report + Save full + Art tối thiểu → Gate P4 GO/NO-GO

Thực thi mục "S18" trong `2026-07-24-p3-p4-completion-plan.md`. Ghi rõ chỗ cụ thể hoá do 2 cắt scope ở S17 (không có shelter thứ 2 mô phỏng đầy đủ) ảnh hưởng trực tiếp lên định nghĩa "Forced Evacuation".

## Scope

1. **`DecisionLogEntry`** (`Core/State/`): `{Minute, DecisionId, Payload}`; `WorldState.DecisionLog:List<DecisionLogEntry>`. Helper `Core/Commands/DecisionLog.Append(ctx, decisionId, payload)` — gọi từ đúng 4 chỗ plan liệt kê: `ResolveEventCommand` (decisionId="event", payload=responseId), `StartBuildCommand` (decisionId="build", payload=ModuleId), `RecruitNpcCommand` (decisionId="recruit", payload=npc id), `EvacuateCommand` mới (decisionId="evacuate").
2. **`SliceBalance`** (`BalanceConfig` — cuối cùng có nội dung thật, S17 từng bỏ vì chưa có field cụ thể): `EvacuationLocationId` (mặc định `location_school`), `MinCleanWaterForStableSurvival`.
3. **`OutcomeRules`** (`Core/Rules/`, pure): `Evaluate(world, mainShelter, cfg) → Outcome{StableSurvival, ForcedEvacuation, Collapse}`. **Định nghĩa cụ thể hoá do thiếu shelter thứ 2 (S17 cắt "Temporary Shelter nâng cấp"):**
   - Collapse: player `Incapacitated` (`ConditionOps.IsIncapacitated`) — không có state "chết" riêng, dùng đúng state terminal duy nhất hiện có.
   - Nếu KHÔNG collapse nhưng `ShelterEventFlags.GroundFloorLost`: có `PersistentFlags["evacuated"]` VÀ player đang ở `SliceBalance.EvacuationLocationId` → **Forced Evacuation**; ngược lại → **Collapse** (mất shelter mà không thoát ra được).
   - Nếu shelter còn ở được: `WaterStocks.Clean >= MinCleanWaterForStableSurvival` → **Stable Survival**; dưới ngưỡng → **Collapse** (sống sót nhưng không đủ tài nguyên không tính là kết thúc tốt, theo đúng nghĩa "≥ tài nguyên tối thiểu" là điều kiện bắt buộc của Stable trong plan gốc).
4. **`OutcomeSystem`** (`Systems/Outcome/`): LongTick — chỉ đánh giá 1 lần (`PersistentFlags["outcome_reached"]`, resync qua `WorldStateReloaded` để không đánh giá lại sau load), điều kiện fail tức thời (player Incapacitated) HOẶC đã tới phase cuối cùng trong `DisasterPhasesSorted` (đọc động, không hardcode id — không phụ thuộc "phase_p4_end" cụ thể) → `OutcomeRules.Evaluate` → publish `OutcomeReached`. **Không dùng `RegisterThreshold`** (lý do giống S14: không serialize được, save/load giữa chừng sẽ mất) — check mỗi long-tick.
5. **`EvacuateCommand`** (`Core/Commands/`): validate player đang ở `location_shelter` (qua `Balance.NewGame.StartLocationId`) + shelter đã `GroundFloorLost` (không cho "evacuate" khi shelter còn ổn — không có gì để thoát). Execute: set `PersistentFlags["evacuated"]=true`, **xoá sạch `ShelterState.Storage.Items`** ("bỏ storage lại" theo đúng plan gốc), log decision, publish `EvacuationDeclared`. Người chơi tự đi tới `location_school` bằng `BeginTravelCommand` có sẵn — `EvacuateCommand` không tự di chuyển player (tái dùng cơ chế travel, không viết lại).
6. **`OutcomeReportPanel`** (`UI/Outcome/`, cùng pattern code-built Canvas/TMP như ShelterPanel/EventPanel): tự mở khi nghe `OutcomeReached` (không cần phím riêng, giống `WorldMapPanel.Open()` mở qua event) — hiện tên Outcome, danh sách `DecisionLog` (Major Decisions), tài nguyên còn lại (Clean/Untreated Water, số Module), NPC Outcome (mỗi NPC recruited: tên/Health/Trust), Shelter Outcome (WaterIntrusion Level). Đóng bằng Esc.
7. GameEvents +`OutcomeReached{Outcome}`, +`EvacuationDeclared`. `Outcome` enum đặt ở `Core/State` (cùng chỗ `WaterIntrusionLevel`/`NpcHealthState`).
8. **Art tối thiểu — CẮT `HazardAudioController`**: cần file audio thật (người cung cấp), không có asset nào trong repo — giữ đúng ghi chú "cần file audio thật, người cung cấp" từ plan gốc, không tự bịa placeholder âm thanh. Lighting/UI hierarchy pass: bỏ qua (không có hạng mục cụ thể nào chưa làm — mọi panel đã dùng `UiLayout` nhất quán từ S8).
9. Không bump SaveVersion thêm field mới thật sự cần persist ngoài `DecisionLog`/`PersistentFlags` (additive, Dictionary/List sẵn có schema-tolerant) — **SaveVersion→12** (đánh dấu rõ ràng có field mới `WorldState.DecisionLog`, dù additive).
10. Test: `OutcomeRulesTests` (~9, đủ 3 nhánh + biên), `SliceRoundtripTests.cs` (load content thật, FastForward giữa chừng → save → load → FastForward tiếp → `OutcomeReached` bắn ra, không exception; + 1 test xác nhận `ActiveEventState` sống sót qua save/load giữa chừng lúc event đang Active).

## Gate P4 GO/NO-GO

Theo đúng plan gốc: **quyết định GO/NO-GO là của người, không phải test.** AI chỉ xác nhận điều kiện kỹ thuật (SliceRoundtripTests xanh, AI tự chạy slice headless 3 lần theo 3 "chiến lược" khác nhau — vd (a) không làm gì đặc biệt, (b) recruit Minh sớm + nuôi đủ, (c) cố tình để ngập rồi evacuate — xác nhận ra 3 outcome khác nhau không softlock). External playtest + tỷ lệ replay-intent là việc của người, ngoài phạm vi AI.

## Verification

- Full EditMode suite + build Windows + headless smoke.
- **User cần làm sau S18**: tự chơi/dùng Debug Panel dẫn tới cả 3 outcome ít nhất 1 lần (đặc biệt Forced Evacuation — cần đủ ngập tới GroundFloorLost rồi evacuate + tới trường), xác nhận `OutcomeReportPanel` hiện đúng, đọc được, không bị lỗi layout. Đây là bước quyết định GO/NO-GO cho P4 — AI không tự quyết được.
