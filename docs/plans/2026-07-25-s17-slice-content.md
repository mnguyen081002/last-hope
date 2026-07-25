# Plan S17 — Slice content: phase/location/route/event P4 (P4-D nửa đầu)

Thực thi mục "S17" trong `2026-07-24-p3-p4-completion-plan.md`. **2 cắt scope có chủ đích tiếp theo** (đúng thứ tự đã duyệt sẵn trong plan gốc, sau khi S16 đã cắt `SendNpcExpeditionCommand`):

- **"Shortcut" (route +`RequiresIntel`) — CẮT.** 2 route mới (Gara/Trường học) là route thường, không có route tắt yêu cầu intel.
- **"Temporary Shelter nâng cấp" — CẮT.** `location_school` là location `IsShelter:true` (ngủ/hồi phục được như bất kỳ shelter nào qua `LocationDefinition.IsShelter` có sẵn từ S7) nhưng KHÔNG phải shelter thứ 2 được mô phỏng đầy đủ (không có `ShelterState` riêng, không Water/Power/Build System riêng cho trường học) — việc đó đòi hỏi tổng quát hoá `WaterIntrusionSystem`/`PowerSystem`/`TaskSystem` từ "1 shelter cứng `MainShelterId`" sang "lặp mọi shelter", một refactor lớn không nằm trong phạm vi 1 sprint content.

## Scope thực hiện

1. **`phases_p4.json` thay `phases_p2.json`** (xoá — không phải "dead code không liên quan", đây chính là nội dung sprint S17 mô tả kế thừa: "S17 — Slice content: 4 phase"; không test nào đọc file JSON thật, chỉ dùng fixture in-memory, xác nhận trước khi xoá). 8 phase theo bảng baseline P3/P4 plan: Normal@0 → Warning@120 → FirstRain@300 → BlackRain@480 → Escalation@640 → Peak@760 → Aftermath@880 → End@960. `ShelterBalance.InflowByRainIntensity` nối dài `[0,2,4,6]` → `[0,2,4,6,9]` (thêm index 4 cho rain_intensity=4 ở Peak — additive, không đổi 4 giá trị đầu, `WaterIntrusionRulesTests` dùng fixture riêng không đụng).
2. **`locations_p4.json`**: `location_garage` (Utility Garage, 2 search point), `location_school` (School, `is_shelter:true`, 2 search point).
3. **`routes_p4.json`**: `route_shelter_garage` (35'), `route_shelter_school` (30').
4. **`searchpoints_p4.json`**: garage (pump_part/toolbox/scrap/filter — tái dùng item đã có từ P3), school (medkit/nước/đồ ăn — tái dùng item đã có từ P2/P1). Không cần item mới.
5. **Scene**: `42_Location_UtilityGarage`, `43_Location_School` — blockout primitive giống hệt pattern `BuildConvenienceStoreScene` (Ground/Light/SearchPointView×2/TravelPointView/PlayerSpawnPoint/CreateBoundaryWalls). Đăng ký vào `EditorBuildSettings.scenes`.
6. **`events_p4.json`** — 4 event chính mới (Drain Backflow/Pump Jam/Storage Flood từ S13 giữ nguyên, không cần "re-tune" vì đã phase-agnostic — check trigger qua `TriggerRequiresBlackWater`/`TriggerStateMinLevel`, không hardcode phase id):
   - `event_storm_warning` (Major, trigger `phase_p4_warning`, response "ack").
   - `event_black_rain_transition` (Critical, trigger `phase_p4_black_rain`, response "ack").
   - `event_school_rescue` (Major, `RequiresDiscovery` tại `location_school`, trigger gate `phase_p4_escalation`, soft 60'/hard 120', response "rescue"/"leave", hết hạn → flag `school_survivor_lost`).
   - `event_grid_failure` (Critical, trigger `phase_p4_escalation`, tag mới **"grid_failure"** — `EventSystem.Trigger()` thêm nhánh set `PersistentFlags["grid_down"]=true` ngay lúc trigger, giống cách "drain_backflow"/"pump_jam" set `ShelterEventFlags` — `PowerSystem` đã đọc flag `"grid_down"` sẵn từ S12 nên không cần sửa PowerSystem).
   - `ResolveEventCommand` +case "rescue"/"leave" (set `PersistentFlags` school_survivor_rescued/abandoned, cùng pattern "greet"/"help_search" ở S16).
7. Manifest → 0.13.0. **KHÔNG bump SaveVersion** — không field `WorldState` nào đổi shape (chỉ Definitions/content mới + 1 `PersistentFlags` key mới "grid_down"/"school_survivor_*", Dictionary sẵn có).
8. Test: cập nhật `ContentValidationTests` (đếm lại locations/routes/searchpoints/phases/events); `EventCommandsTests`/`EventLifecycleTests` không đổi (fixture riêng); thêm `SliceTimelineTests.cs` — load content THẬT, `FastForward` hết 960' xác nhận không exception + đi qua đủ 8 phase theo đúng thứ tự.

## Verification

- Full EditMode suite + build Windows + headless smoke.
- **Cần user tự mở Editor xác nhận bằng mắt** (AI headless không tự nhìn được): 2 scene mới dựng đúng không (ground/search point/travel point vị trí hợp lý, không lọt rìa map — theo đúng bài học 2026-07-24 boundary wall), World Map (M) hiện được 2 route mới sau khi quan sát.
