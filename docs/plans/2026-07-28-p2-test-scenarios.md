# Plan P2-C phần 3 — Test Scenario A–D (BL-P2-13)

Theo prototype plan mục 6.6. Đây là kịch bản **playtest tay** (giống BL-P1-22), không phải
tính năng mới — chỉ cần một sửa nhỏ để test được đủ cả 4 kịch bản.

## Sửa trước khi test được

F2 Debug Panel mục Hazard trước đây hard-code chỉ chỉnh được `route_shelter_store`
(`const string TestRouteId`). Từ BL-P2-12 có thêm `route_shelter_garage`, cần chỉnh được cả
hai route mới test được Scenario A/D. Đã sửa: `DebugPanel` liệt kê **mọi route trong
DefinitionRegistry** thành nút chọn, control Flood/Current/Electrified áp cho route đang
chọn (mặc định `route_shelter_store`). Cũng hiện `closes_at_phase` của route đang chọn.

## Điều chỉnh phạm vi so với prototype plan gốc

Prototype plan viết ở mức toàn bộ MVP (giả định đã có Event Framework — P3/P4). Ở mốc P2
hiện tại **chưa có Event system** (`docs/backlog/BACKLOG.md` S13/S14 vẫn Backlog), nên
Scenario C được diễn giải lại: thay "hoàn thành Event" bằng "hoàn thành mục tiêu thực tế duy
nhất có ở P2" (nhặt đủ loot ở garage/store) — giữ đúng tinh thần "thiếu Equipment vẫn phải
hoàn thành việc", không giả bộ có Event.

## Phát hiện khi thiết kế Scenario D — rủi ro softlock thật

`location_convenience_store` chỉ nối **một** route (`route_shelter_store`) về shelter. Route
này giờ có `closes_at_phase: route_closure`. Nếu player đang đứng ở cửa hàng đúng lúc world
time vượt mốc RouteClosure (900 phút = 15 tiếng), route đóng **vĩnh viễn** (thời gian chỉ
tăng, không có cơ chế mở lại) — không còn đường nào về shelter từ cửa hàng. Đây đúng là
"Redesign Trigger" đã ghi sẵn trong `docs/03-mvp-black-rain/10-mvp-prototype-plan.md` mục
6.8: "Route đóng khiến người chơi mắc kẹt không có phương án".

**Không sửa preemptive trong item này** — đúng việc Scenario D cần kiểm chứng là chính rủi ro
này, chưa chắc gặp phải trong nhịp chơi thật (buffer 15 tiếng game rất dài so với phiên chơi
30-45 phút thật). Cơ chế giảm rủi ro đúng đắn là BL-P2-11 (Return Window UI — cảnh báo
ETA/risk *trước khi* đi), hiện vẫn Backlog. Nếu user chạy Scenario D và thấy đây là vấn đề
thật cần xử lý ngay, quay lại bàn hướng sửa (vd. exempt chiều về, hoặc cảnh báo sớm) thay vì
tự quyết ở đây.

## 4 kịch bản

### Scenario A — Route ngắn ngập, route dài an toàn

1. F2 → chọn route `store` → Impassable (hoặc tua `+8h` nhiều lần tới Disaster Phase
   RouteClosure).
2. Từ Shelter, thử tương tác Travel Point đi cửa hàng — phải bị từ chối.
3. Đi gara (route dài hơn, không tự đóng) — phải đi được bình thường.

### Scenario B — Mang nặng qua Medium Flood

1. F2 → thêm nhiều item nặng (vd. `item_toolbox` ×3, `item_water_container_20l`) tới khi
   Load Tier = Heavy.
2. F2 → chọn route bất kỳ → Medium.
3. Travel qua route đó — thời gian phải nhân dồn cả `load_factor_heavy` (1.5×) và
   `flood_time_factor` của Medium (không phải chỉ lấy số lớn hơn — logic này đã có test tự
   động, đây là xác nhận bằng mắt).

### Scenario C — Thiếu Equipment vẫn phải hoàn thành mục tiêu (thay cho Event, xem điều chỉnh phạm vi ở trên)

1. Đảm bảo không mặc equipment nào (tháo hết nếu có).
2. F2 → route đi gara → Deep + Current Strong.
3. Travel qua đó, tới gara, nhặt hết loot 2 search point (`searchpoint_garage_workbench`,
   `searchpoint_garage_shelf`).
4. Quay lại Shelter (route khác hoặc cùng route nếu chưa đóng) — kiểm tra Health/Stamina
   không tụt xuống 0 đột ngột không cảnh báo (Exit Criteria "Hazard không gây Failure tức thời
   thiếu cảnh báo").
5. Về Shelter — Wet phải giảm dần/về 0 (`ConditionSystem.UpdateWet` tại shelter). **Lưu ý**:
   Exposure/Sick **chưa có cơ chế xử lý tại Shelter** (cần P3 — "Shelter treat", đã ghi ở
   P2-A) — nếu Sick đã bật, nó **không tự tắt**, đây là giới hạn đã biết, không phải bug mới.

### Scenario D — Route đổi trong lúc đang ở Location

1. Đi tới cửa hàng (route `store`, chưa đóng).
2. Trong lúc đang đứng ở cửa hàng, F2 → route `store` → Impassable (mô phỏng đổi route giữa
   chừng, không cần đợi tua giờ thật).
3. Thử tương tác Travel Point quay về shelter — phải bị từ chối, có thông báo rõ ràng
   (`CommandErrorCode.NotAllowedNow`), không phải im lặng/crash.
4. **Đây chính là rủi ro softlock nêu trên** — quan sát xem có cách nào khác về không (hiện
   tại: không, vì cửa hàng chỉ nối 1 route). Ghi nhận cảm giác có bị "mắc kẹt khó chịu" không,
   quyết định sau có cần xử lý sớm hơn BL-P2-11 hay chấp nhận rủi ro tạm thời.

## Verification

Compile → EditMode test (không có gì mới cần test tự động — thuần playtest) → build → smoke
test headless (boot only, các kịch bản trên cần chơi tay).

## User cần tự test bằng mắt

Toàn bộ 4 kịch bản trên. Sau khi chạy xong, đối chiếu với Exit Criteria Gate P2
(`docs/backlog/BACKLOG.md` mục P2-C):
đổi Route vì Flood; Equipment thay đổi Loadout; không Failure tức thời thiếu cảnh báo;
Route Closure không softlock (**xem phát hiện Scenario D ở trên — có thể chưa đạt tiêu chí
này**); Return Window dễ hiểu (chưa làm — BL-P2-11).
