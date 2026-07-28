# Plan P3 — Test Scenario A–F (Shelter Loop)

Kịch bản playtest tay cho toàn bộ P3 (giống mẫu `2026-07-28-p2-test-scenarios.md`), không
phải tính năng mới. Đọc `docs/plans/2026-07-28-p3-shelter-loop.md` mục "Phạm vi rút gọn có
chủ đích" trước khi test — nhiều thứ cố ý đơn giản hoá so với design doc gốc.

Dùng F2 Debug Panel để tua thời gian/cheat, không cần chơi tay từng phút. Mọi hệ thống P3
(Water/Power/Purifier/Event) tick qua `TickScheduler` sẵn có nên `+1h`/`+8h` chạy đúng logic
thật, không phải giả lập riêng.

## Scenario A — Khan hiếm vật liệu tự tạo "2-trong-3" (BL-P3-17)

1. Từ Shelter, đi cửa hàng, nhặt hết cả 6 search point — đặc biệt `searchpoint_back_room`
   (nguồn duy nhất có `item_wood`, `item_purifier_unit`, `item_filter`, `item_pump_part`).
2. Đi gara, nhặt hết 2 search point (thêm `item_pump_part`, `item_scrap`, `item_filter`).
3. Về Shelter, mở túi đồ, chuyển hết vật liệu vào Kho Shelter (nút Storage cạnh Console).
4. Mở Shelter Console — thử xây lần lượt: Flood Barrier (4 wood + 2 scrap), Elevated Storage
   (3 wood), Portable Pump (1 pump_part + 2 scrap), Water Purifier (1 purifier_unit + 1
   filter), Battery Bank (2 battery + 1 scrap).
5. Ghi nhận: `item_wood` chỉ có 2-4 (roll ngẫu nhiên, một nguồn duy nhất) — không đủ xây cả
   Barrier(4) lẫn Elevated Storage(3) cùng lúc, phải chọn. `item_purifier_unit` chỉ 30% cơ
   hội roll ra — có thể là 0, khiến Purifier không xây được dù đủ tiền đề khác. Đây là hành vi
   **mong đợi**, không phải bug — xác nhận đúng tinh thần "chỉ hoàn thiện 2/3 Module chính".

## Scenario B — Power Priority tạo đánh đổi thật (BL-P3-11)

1. Xây Pump và Purifier (nếu đủ vật liệu ở Scenario A; nếu thiếu, F2 → thêm `item_pump_part`,
   `item_scrap` qua ô tìm item để xây riêng Pump).
2. F2 → `+8h` nhiều lần tới khi Disaster Phase = RouteClosure (mốc 900 phút) — Grid Supply về
   0, chỉ Battery còn cấp được điện.
3. Mở Shelter Console, đặt Pump = Critical, Purifier = Normal (bấm nút Priority để xoay vòng).
   Quan sát Battery cạn dần qua Overview; nếu không đủ cho cả hai, chỉ Pump (Critical) hiện
   "có điện".
4. Đổi Pump = Normal, Purifier = Critical — quan sát module "có điện" đổi theo đúng Priority
   mới, không phải cố định theo thứ tự xây trước/sau.

## Scenario C — Water Intrusion / Ground Floor Loss không kết thúc game (BL-P3-05)

1. F2 → mục Shelter → `+20 Water` vài lần tới khi Water Intrusion ≥ 60 (Deep).
2. Nếu đã xây Pump có điện — quan sát Pump ngừng tác dụng (Ground Floor bị khóa, đúng thiết
   kế "Deep: Module điện tầng dưới bị khóa").
3. Tiếp tục `+20 Water` tới ≥ 85 (Critical) — xác nhận game **không kết thúc**, vẫn di chuyển/
   mở panel bình thường (Exit Criteria "Ground Floor Loss không luôn dẫn tới Game Over").
4. F2 → `Reset Water` — Pump phải hoạt động trở lại (không bị khóa vĩnh viễn).

## Scenario D — Storage Flood Risk buộc chọn bảo vệ resource (BL-P3-08/15)

1. Chuyển vài item vào Kho Shelter.
2. F2 → `+20 Water` tới ≥ 85 (Critical), **chưa** xây Elevated Storage.
3. Mở Shelter Console — banner "⚠ Storage Flood Risk" phải hiện.
4. F2 → `+1h` vài lần (mỗi Long Tick 10 phút có tỉ lệ mất 1 stack ngẫu nhiên trong kho) —
   quan sát đồ trong kho giảm dần.
5. Xây Elevated Storage (nếu đủ vật liệu) rồi lặp lại bước 2-4 — banner không hiện nữa / kho
   không mất đồ dù Water Intrusion vẫn Critical.

## Scenario E — Drain Backflow + Pump Jam Event (BL-P3-14/16)

1. F2 → `+8h` tới Disaster Phase RouteClosure.
2. F2 → `+1h` vài lần — Drain Backflow có thể tự kích hoạt (roll mỗi Long Tick), banner đỏ
   hiện trong Shelter Console, Water Intrusion tăng nhanh hơn khi active (thêm
   `backflow_inflow`). Nếu chưa thấy sau vài lần bấm, F2 có nút "Bật Drain Backflow" thủ công
   để test luôn phần giải quyết.
3. Bấm "Xử lý" trong Shelter Console — tốn khoảng 20 phút game (world time nhảy), banner biến
   mất.
4. Nếu đã xây Pump có điện — F2 có nút "Bật Pump Jam" thủ công (hoặc chờ tự roll qua vài Long
   Tick). Bấm "Sửa" trong Shelter Console — tốn khoảng 15 phút, hết kẹt, Pump hoạt động lại.

## Scenario F — Sleep hồi Fatigue + chữa Black Water Exposure (BL-P3-13)

1. Ghi lại Fatigue hiện tại (F2 mục Condition).
2. F2 → `+50 Exposure` để Black Water Exposure vượt ngưỡng Sick (70) — `Sick:True` phải hiện.
3. Đứng tại Shelter (Ground Floor), tương tác Cầu thang ("Lên gác") — camera/player chuyển
   lên Upper Floor (đổi tầng thật, `StaircaseView`). Tương tác giường (Bed) — panel Ngủ hiện
   ra, kéo slider chọn 6 giờ, bấm "Ngủ".
4. Sau khi ngủ: Fatigue phải thấp hơn trước lúc ngủ (F2 mục Condition).
5. Black Water Exposure phải giảm dần (6 giờ ngủ tại Shelter vượt xa ngưỡng
   `shelter_treat_exposure_minutes` = 60 phút) — nếu Exposure tụt dưới 70, `Sick` phải tự
   chuyển về `False` (trước P3 cờ này không tự tắt).

## Scenario G — Cầu thang đổi tầng (BL-P3-01, dựng lại sau review 2026-07-28)

1. Từ Ground Floor (điểm spawn mặc định), tương tác Cầu thang "Lên gác" — Storage/Console/
   TravelPoint (đồ Ground Floor) phải biến mất khỏi màn hình, chỉ còn Bed + Cầu thang "Xuống
   dưới" (đồ Upper Floor).
2. Tương tác Cầu thang "Xuống dưới" — quay lại đúng Ground Floor như cũ (Storage/Console/
   TravelPoint hiện lại, Bed biến mất).
3. Thử Travel đi cửa hàng/gara trong lúc đang ở Upper Floor (nếu tương tác được TravelPoint từ
   xa — không nên, vì đã bị ẩn/không active) — xác nhận không cách nào tương tác nhầm đồ Ground
   Floor khi đang ở Upper.
4. **Giới hạn đã biết**: Save/Load không nhớ đang ở tầng nào — F2 Save rồi Load trong lúc ở
   Upper Floor sẽ về lại Ground Floor (giống scope cut "Save/Load không đổi scene" ở P1).

## Verification

Compile → 219 EditMode test (166 P1/P2 + 53 P3) → sinh 6 scene → build Windows → smoke test
headless (boot only) — đã chạy, tất cả pass. 6 Scenario trên cần chơi tay, không có gì thêm
để tự động hoá.

## User cần tự test bằng mắt

Toàn bộ 6 Scenario trên. Đối chiếu Exit Criteria Gate P3 (`docs/backlog/BACKLOG.md` mục P3):
ít nhất ba chiến lược Shelter hợp lệ (Scenario A); không Module nào luôn bắt buộc (Scenario
A); hiểu nguyên nhân Water Intrusion (Scenario C); Passive Task chạy khi rời Shelter/Sleep
(ngầm định qua kiến trúc tick, không có bước riêng để "thấy" — nếu muốn xác nhận, bắt đầu xây
một Module rồi rời Shelter đi Travel, quay lại xem còn bao nhiêu phút thay vì đứng yên tại
chỗ mới thấy tiến độ); Ground Floor Loss không luôn Game Over (Scenario C); Power Allocation
tạo lựa chọn thực (Scenario B). Toàn bộ số liệu Event/Sleep là tự đề xuất — góp ý nếu cảm thấy
quá nhanh/chậm/dễ/khó.
