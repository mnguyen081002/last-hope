# Plan P2-A — Player Condition Core (BL-P2-01, BL-P2-03)

Phạm vi: 6 chỉ số cốt lõi (Health/Stamina/Fatigue/Hunger/Thirst/BodyTemperature) + Wet +
Cold + Black Water Exposure → Sick + Collapsed. **Không làm Injury (Cut/Bruise/Sprain/
Fracture) và Disoriented trong slice này** — `balance.json` không có field nào cho hai
nhóm đó, trong khi `player-condition-system-design.md` mục 9/13 mô tả chi tiết. Theo đúng
nguyên tắc xuyên suốt dự án (JSON có gì làm nấy, không bịa số cân bằng): implement khi có
số thật, không suy đoán trước.

## Đọc trước khi code

`docs/02-core-systems/player-condition-system-design.md` — thiết kế đầy đủ. Bảng dưới map
từng field `balance.json.condition` sang hành vi cụ thể (thiết kế doc mô tả định tính, số
liệu + ngữ nghĩa chính xác do tôi suy ra và ghi rõ ở đây vì doc không cho công thức):

| Field JSON | Hành vi |
| --- | --- |
| `thirst_per_hour`, `hunger_per_hour` | Tăng đều mỗi phút (chia 60), clamp 0–100 |
| `fatigue_per_long_tick` | Tăng mỗi long tick (10 phút) |
| `fatigue_per_travel` | Cộng thêm **một lần** mỗi chuyến Travel (ngoài tick thường) |
| `stamina_regen_per_minute`, `*_halved_multiplier` | Hồi mỗi phút; giảm còn nửa khi Fatigue≥50 hoặc Thirst≥70 hoặc `IsCold` |
| `body_temp_drift_down_per_minute` | Chỉ tụt khi `Wet ≥ wet_threshold_for_temp_drift` — **wet là nguyên nhân**, không phải trời lạnh ambient (chưa có Disaster Phase để biết trời có mưa) |
| `body_temp_regen_at_shelter_per_minute` | Hồi về 37°C khi ở shelter và không đang bị wet-drift |
| `wet_dry_per_minute_at_shelter` | Wet giảm dần khi ở shelter |
| `wet_gain_per_minute_in_rain` | **Chưa dùng** — cần Disaster Phase (P2-B) biết trời mưa. Field giữ sẵn trong `ConditionBalance` |
| `cold_body_temp_threshold` / `cold_clear_body_temp_threshold` | Hysteresis 2 ngưỡng — `IsCold` bật ở 35°C, chỉ tắt khi lên tới 36°C, tránh nhấp nháy ở biên |
| `black_water_exposure_threshold` | **Chưa dùng** — chỉ có nguồn tăng Exposure qua Hazard crossing (P2-B chưa làm). Field giữ sẵn |
| `sick_exposure_threshold` | `Exposure ≥ 70` → `IsSick = true` (không tự tắt trong slice này — cần Shelter treat, để P3 Sleep) |
| `starvation_health_decay_per_long_tick` / `_floor` | `Hunger≥100 OR Thirst≥100` → Health giảm mỗi long tick, dừng ở floor (không tự chết vì đói) |
| `sick_health_decay_per_long_tick` | `IsSick` → Health giảm mỗi long tick (**không có floor** — Sick nặng có thể chết, khác starvation) |
| `collapsed_health_threshold` | `Health ≤ 5` → Collapsed, chặn hoàn toàn di chuyển (nhị phân, không phải modifier — balance.json không cho số modifier tăng dần nên không bịa) |
| `shelter_rest_minutes`, `shelter_treat_exposure_*` | **Chưa dùng** — thuộc Sleep/Rest command, để P3 |

## Cắt phạm vi có chủ đích

- Stamina chỉ hồi, chưa có tiêu hao (chưa có sprint/vượt dòng nước — cơ chế đó thuộc P2-B).
- Fatigue/Cold/Health **không** nhân vào `PlayerController.SpeedModifier` dạng hệ số —
  `balance.json` không cho số modifier tăng dần cho các trường hợp này (khác Overload đã có
  số rõ ràng `speed_modifier_light/heavy`). Chỉ có **Collapsed** là nhị phân (chặn hẳn di
  chuyển) nên implement được mà không cần bịa số.
- `IsSick` không tự khỏi trong slice này (cần Shelter treat — action chưa tồn tại, để P3).

## Kiến trúc

- `Data/Definitions/BalanceDefinition.cs` — thêm `ConditionBalance` khớp 1:1 JSON.
- `Core/State/PlayerState.cs` — thêm `Health`, `Stamina`, `BodyTemperature`,
  `BlackWaterExposure`, `IsCold`, `IsSick` (flat, khớp style Thirst/Hunger/Fatigue/Wet có sẵn).
- `Systems/Condition/ConditionSystem.cs` — hàm thuần: `ApplyShortTick`, `ApplyLongTick`,
  `IsCollapsed`.
- `Systems/Condition/ConditionDriver.cs` — subscribe `TickScheduler.ShortTick/LongTick`,
  dựng lại mỗi `GameServices.BindWorld` (Ticks cũng dựng lại theo, tránh subscribe kép sau
  load save).
- `Systems/Travel/TravelSystem.cs` — cộng `FatiguePerTravel` sau `FastForward`.
- `Presentation/Player/PlayerOverloadSync.cs` — nhân thêm hệ số Collapsed (0 hoặc 1) vào
  `SpeedModifier` đã có từ Overload.
- `DebugTools/Panel/DebugPanel.cs` — thêm mục Condition: hiển thị đủ 6 chỉ số + Wet/Cold/
  Exposure/Sick/Collapsed, nút cheat cộng thẳng từng chỉ số để test không cần chờ tick thật.

## Verification

Batchmode compile → EditMode test (thirst/hunger rate, fatigue tick+travel, wet dry ở
shelter, body temp drift theo wet, cold hysteresis không nhấp nháy ở biên, starvation floor,
sick decay không floor, collapsed đúng ngưỡng) → sinh scene → build → smoke test headless.

## User cần tự test bằng mắt

- F2 Debug Panel: mục Condition mới — set Wet lên cao, xem BodyTemperature tụt, chờ vài phút
  (hoặc F2 fast-forward) xem `IsCold` bật lên.
- Set Health xuống ≤5 qua cheat — xác nhận nhân vật đứng im, không di chuyển được nữa.
- Đi shelter, xem Wet giảm dần và BodyTemperature hồi lại 37°C.
