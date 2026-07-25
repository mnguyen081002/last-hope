# Plan S15 — Intel + World Map Intel + NPC nền (P4-B)

Thực thi mục "S15" trong `2026-07-24-p3-p4-completion-plan.md`. Ghi các quyết định cụ thể hoá + điểm lệch.

## Scope

1. **IntelState** (`Core/State/`): `WorldState.Intel : IntelState { Records: Dict<subjectId, IntelRecord> }`.
   `IntelRecord { Kind ("route"/"location"/"event"), Confidence (enum Confirmed/Reliable/Uncertain/Unverified), ObservedAtMinute, FloodLevel/CurrentLevel/Closed (payload phẳng cho route — không dùng Dict payload tự do, chưa cần) }`.
2. **IntelRules** (`Core/Rules/`, pure): `EffectiveConfidence(record, now, cfg)` — suy giảm theo tuổi thông tin (Confirmed→Reliable sau 60', →Uncertain sau 180', baseline plan); `Merge(existing, incoming)` — record mới thắng record cũ (ObservedAtMinute mới hơn).
3. **IntelSystem** (`Systems/Intel/`): quan sát trực tiếp = intel Confirmed — subscribe `TravelCompleted` (ghi intel route vừa đi + location đến), `SearchPointOpened` (location hiện tại), `WorldStateReloaded` resync không cần (state nằm trong WorldState). Radio/comms → S16+ (chưa có hệ thống radio).
4. **WorldMapPanel** sửa: chỉ render điều đã biết — route chưa có intel record → hiện "?" (không ETA/flood/current thật); route có intel → hiện số liệu **từ record** kèm tuổi thông tin + confidence suy giảm; route đang đứng cạnh (connected với vị trí hiện tại) luôn được quan sát trực tiếp = cập nhật Confirmed ngay khi mở panel (đứng đó nhìn thấy đầu route). Nút Travel vẫn submit bình thường (command tự validate thật — intel chỉ là hiển thị).
5. **NPC nền**: `NpcDefinition` + `npcs_p4.json` (nguyen_minh: skill electric ×1.5, trust khởi đầu 30, tiêu thụ nước/food); `NpcState` fields thật (Location, HealthState enum Healthy/Injured/Critical/Dead, Hunger/Thirst, Trust, CurrentTaskId, Flags:HashSet) thay stub Id/StatusName; owner `npc:<id>` trong InventoryOwnerResolver (lazy-create `NpcState.Inventory`).
6. Loader: route `npcs_*.json` → `NpcDefinition`; registry + TryGetNpc.
7. SaveVersion→10, manifest→0.11.0. Tests: IntelRulesTests + IntelSystemTests + NpcState roundtrip (~10).

## Lệch / cụ thể hoá so với plan gốc

- **Payload intel phẳng** (FloodLevel/CurrentLevel/Closed nullable) thay vì `Payload: Dict` tự do — chỉ route cần payload ở S15, schema phẳng test dễ hơn; mở rộng khi loại subject mới thật sự cần.
- **Route liền kề vị trí hiện tại luôn Confirmed-fresh khi mở map**: tránh trạng thái phi lý "đứng ngay đầu route mà map hiện ?"; các route xa mới phụ thuộc trí nhớ/tuổi intel.
- Radio forecast trong plan gốc dời sang S16/S17 cùng content (chưa có nguồn radio nào tồn tại ở S15).

## Verification

- Full EditMode suite + build + smoke như mọi sprint.
- **User cần test tay sau S15**: mở map (M) ở shelter — route đi rồi hiện số liệu kèm tuổi (vd "10' ago"), route chưa từng đi hiện "?"; đi 1 chuyến rồi chờ >60' — confidence rơi từ Confirmed xuống Reliable/Uncertain trên map.
