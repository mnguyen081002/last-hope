# Plan S14 — Event Framework hoàn chỉnh + Event UI (P4-A)

Thực thi mục "S14" trong `2026-07-24-p3-p4-completion-plan.md`. Ghi lại các điểm **lệch so với plan gốc** và scope chính xác.

## Scope

1. **EventDefinition** thêm field data-driven tối thiểu:
   - `requires_discovery` (bool, default false) — instance khởi đầu Undiscovered, UI không thấy, không resolve được; deadline chỉ arm khi **discovered** (khớp plan gốc "School Rescue: soft/hard từ discovery").
   - `next_event_id` — Event Chain: force-trigger (bỏ qua trigger conditions) khi event này Resolved HOẶC Expired.
   - `expiration_shelter_flags` / `expiration_persistent_flags` — Persistent Consequence khi quá hard deadline: add vào `ShelterState.EventFlags` / set true trong `WorldState.PersistentFlags`. Có flag nào ⇒ trạng thái cuối là `PersistentConsequence`, không thì `Expired`.
2. **ActiveEventState** thêm `SoftDeadlineMinute` (long?), `SoftDeadlineNotified` (bool) — serialize tự động qua Newtonsoft (additive).
3. **EventSystem** mở rộng: trigger → Undiscovered/Active; discovery pass (shelter-scope: player đứng tại location `IsShelter`); soft deadline → publish `EventDeadlineApproaching` một lần; hard deadline → Expire (áp expiration flags, publish `EventExpired`, chain); subscribe `EventResolved` để chain khi resolve. **Off-scene mặc định**: không đọc vị trí player trừ discovery check.
4. **Sleep-interrupt theo Event priority** (event-system-design.md §14): `EventTriggerRules.ShouldWakeSleeper(priority, atShelter)` — Critical luôn wake, Major wake khi ngủ tại shelter, còn lại ngủ tiếp. `StartSleepCommand` thêm điều kiện wake: có event mới trigger sau lúc bắt đầu ngủ thoả rule.
5. **UI**: `EventToast` (banner top-center 4s/thông báo, nghe 5 event lifecycle) + `EventPanel` (phím **V**, pattern lifecycle-safe như ShelterPanel: list event Active kèm priority + countdown deadline + nút response → `ResolveEventCommand`; log ngắn các event đã kết thúc).
6. `ResolveEventCommand`: chặn resolve event Undiscovered (`EventNotDiscovered`).
7. Wiring: `GameControls.inputactions` +`ToggleEvents` (V); `SceneSetup` +EventPanel/EventToast; `ControlsLegend` +V; DebugPanel hiện cả event Undiscovered + arm soft deadline trong force-trigger cheat.
8. `DefinitionLoader.ValidateEvents`: `next_event_id` dangling ⇒ lỗi load.
9. Content: `events_p3.json` — pump_jam thêm `soft_deadline_minutes: 30`.
10. SaveVersion→9, manifest→0.10.0. Tests mới `EventLifecycleTests` (~13).

## Lệch so với plan gốc (có chủ đích)

- **Deadline check mỗi LongTick thay vì `RegisterThreshold`**: threshold đăng ký trong TickScheduler không được serialize — save/load sẽ mất deadline đã đăng ký, phải re-register khi Resync. Check trong OnLongTick (granularity 10 phút, deadline đều ≥30') đơn giản hơn và save-safe. Không đổi API TickScheduler.
- **Persistent Consequence chỉ mutate flags (chưa mutate RouteState/LocationState)**: chưa có content nào cần đóng route khi expire — S17 (Grid Failure, route events) sẽ nối flag → RouteState khi content thật tồn tại.
- **Discovery nguồn duy nhất là "tại chỗ"** (đứng ở shelter): radio/NPC là nguồn S15 (Intel/comms), chưa có hệ thống mang tin.

## Verification

- Full EditMode suite (221 cũ + ~13 mới) + build Windows + headless smoke.
- **User cần test tay sau S14**: phím V mở Event Panel, resolve event qua panel (không cần F2 nữa); toast hiện khi event trigger/expire; ngủ dài (Debug F2 Sleep 480') trong lúc pump_jam force-triggered → có bị đánh thức không (pump_jam priority Critical); pump_jam để quá 60' không resolve → expired, pump kẹt vĩnh viễn.
