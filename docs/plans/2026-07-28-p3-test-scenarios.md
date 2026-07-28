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
3. Đứng tại Shelter (Ground Floor), đi bộ về phía góc có vùng cầu thang (không cần bấm phím —
   đi ngang qua tự động đổi tầng) để lên Upper Floor. Tương tác giường (Bed) — panel Ngủ hiện
   ra, kéo slider chọn 6 giờ, bấm "Ngủ".
4. Sau khi ngủ: Fatigue phải thấp hơn trước lúc ngủ (F2 mục Condition).
5. Black Water Exposure phải giảm dần (6 giờ ngủ tại Shelter vượt xa ngưỡng
   `shelter_treat_exposure_minutes` = 60 phút) — nếu Exposure tụt dưới 70, `Sick` phải tự
   chuyển về `False` (trước P3 cờ này không tự tắt).

## Scenario G — Z-level đổi tầng kiểu Project Zomboid (BL-P3-01, dựng lại lần 2 sau review 2026-07-28)

1. Từ Ground Floor (điểm spawn mặc định), đi bộ về góc có vùng cầu thang (khu vực gần vị trí
   Bed cũ trước đây) — **không cần bấm phím**, đi qua là tự đổi tầng. Storage/Console/
   TravelPoint (đồ Ground Floor) phải hiện **mờ đi** (không biến mất hẳn — thấy lờ mờ bố cục
   tầng dưới qua sàn), Bed + vùng cầu thang xuống (đồ Upper Floor) hiện rõ nét.
2. Thử đi lại gần vị trí Storage khi đang ở Upper Floor — không tương tác được (E không có tác
   dụng, đúng "tầng dưới không va chạm/tương tác được khi đang đứng Dimmed").
3. Đi bộ ngược lại qua vùng cầu thang xuống — quay lại đúng Ground Floor rõ nét, Upper Floor
   giờ mờ đi (không biến mất hẳn).
4. Đi qua lại nhanh nhiều lần gần ranh giới hai vùng trigger — không được xảy ra hiện tượng
   "nhấp nháy" đổi tầng liên tục (oscillation) — hai vùng trigger đã thiết kế lệch nhau để
   tránh việc này, nếu vẫn thấy nhấp nháy là bug cần báo lại.
5. **Giới hạn đã biết**: Save/Load không nhớ đang ở tầng nào — F2 Save rồi Load trong lúc ở
   Upper Floor sẽ về lại Ground Floor (giống scope cut "Save/Load không đổi scene" ở P1).

## Scenario H — Free Placement: đặt Module tự do trong Zone (BL-P3-03, viết lại 2026-07-28)

1. Nhặt đủ vật liệu cho Portable Pump (`item_pump_part`, `item_scrap` — F2 thêm nhanh nếu cần),
   chuyển vào Kho Shelter. Mở Shelter Console → mục `utility_area` → bấm "Chọn vị trí" cạnh
   `module_pump`. Panel đóng lại, thấy khung mờ trắng (biên Zone) + ô vuông theo con trỏ chuột.
2. Di chuột ra ngoài khung mờ — ô vuông phải chuyển **đỏ** (không đặt được). Di chuột vào trong
   khung — chuyển **xanh**. Click trái khi đang xanh — Module bắt đầu xây tại đúng vị trí vừa
   click (mở lại Shelter Console, thấy "Đang xây module_pump tại utility_area").
3. Thử lại từ đầu (chọn vị trí Module khác, hoặc Pump nếu đã tháo) — lần này bấm ESC giữa
   chừng thay vì click — không có gì được xây, **vật liệu không bị mất** (kiểm tra lại Kho
   Shelter còn đủ số lượng cũ).
4. Xây 1 Module xong (đợi F2 `+1h`), mở lại Chọn vị trí cho Module thứ hai cùng Zone, thử click
   ngay sát vị trí Module thứ nhất (trong bán kính ~1 đơn vị) — ô vuông phải đỏ (Overlapping),
   không đặt chồng được. Lùi chuột ra xa hơn — chuyển xanh, đặt được bình thường.
5. Chọn vị trí cho Module ở Zone `upper_living` (Elevated Storage, nếu đủ vật liệu) trong khi
   đang đứng ở Ground Floor — game phải tự chuyển camera/view lên Upper Floor (không cần tự đi
   cầu thang trước) để thấy đúng khung Zone đang chọn.

## Verification

Compile → 220 EditMode test → sinh 6 scene → build Windows → smoke test headless (boot only)
— đã chạy, tất cả pass. 8 Scenario trên cần chơi tay, không có gì thêm
để tự động hoá.

## User cần tự test bằng mắt

Toàn bộ 8 Scenario trên. Đối chiếu Exit Criteria Gate P3 (`docs/backlog/BACKLOG.md` mục P3):
ít nhất ba chiến lược Shelter hợp lệ (Scenario A); không Module nào luôn bắt buộc (Scenario
A); hiểu nguyên nhân Water Intrusion (Scenario C); Passive Task chạy khi rời Shelter/Sleep
(ngầm định qua kiến trúc tick, không có bước riêng để "thấy" — nếu muốn xác nhận, bắt đầu xây
một Module rồi rời Shelter đi Travel, quay lại xem còn bao nhiêu phút thay vì đứng yên tại
chỗ mới thấy tiến độ); Ground Floor Loss không luôn Game Over (Scenario C); Power Allocation
tạo lựa chọn thực (Scenario B). Toàn bộ số liệu Event/Sleep là tự đề xuất — góp ý nếu cảm thấy
quá nhanh/chậm/dễ/khó.
