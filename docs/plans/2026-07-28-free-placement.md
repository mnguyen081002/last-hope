# Free Placement — thay Build Slot cố định bằng world position tự do

Nguồn quyết định: user 2026-07-28 (xem `docs/02-core-systems/building-and-placement-system-design.md`
đã sửa cùng ngày — thiết kế gốc vốn đã định hướng world position tự do trong Zone, `BuildSystem`
P3 ban đầu chỉ hiện thực hoá sai thành danh sách Slot cố định).

## Phạm vi lần này

**Làm:** Free placement đầy đủ cho Shelter (nơi duy nhất có content thật — 5 Module, 8 Zone).
Zone giờ có world bounds (không phải danh sách Slot đếm sẵn); người chơi chọn Module → chọn
Zone → vào Placement Mode (ghost theo chuột, xanh/đỏ theo hợp lệ) → click để đặt.

**Không làm (nêu rõ, không âm thầm bỏ qua):** Outdoor placement cho Location ngoài trời
(cửa hàng/gara). Lý do: `modules_p3.json` hiện chỉ có 5 Module, cả 5 đều `AllowedZoneIds`
trỏ vào Shelter Zone — **không có Module Outdoor/Hybrid nào tồn tại trong content**. Dựng hệ
`PlaceableZoneDefinition` cho Location mà không có gì để đặt vào đó là code chết, không kiểm
chứng được. Cần quyết định nội dung Module Outdoor trước (vd Barricade, External Pump — đã
liệt kê ví dụ ở building-and-placement-system-design.md mục 4) rồi mới làm phần này.

## Thiết kế

**Data**: `ShelterZoneDefinition` bỏ `BuildSlotIds`, thêm `BoundsMinX/MinY/MaxX/MaxY` (world
position, world tự do trong biên này). `ModuleDefinition` thêm `FootprintRadius` (bán kính va
chạm dùng để check overlap giữa các Module đã đặt — placeholder 0.5, chưa có kích thước sprite
thật). `shelterzones_p3.json` cập nhật theo schema mới.

**Core/State**: `ShelterState.BuildSlots` (Dictionary theo slotId) → `PlacedModules`
(Dictionary theo placementId tự sinh, `NextPlacementId` counter tăng dần). `BuiltModuleState`
thêm `PositionX/PositionY`, `ZoneId`. `ConstructionState` đổi `SlotId` → `ZoneId` +
`PositionX/PositionY`.

**Systems**: `BuildSystem.CanPlaceAt(zoneId, position, moduleId)` — zone tồn tại + position
trong bounds + module cho phép zone + không chồng lấn Module khác (tổng 2 FootprintRadius) +
đủ vật liệu + không có construction khác đang chạy. `StartConstruction` nhận position thay vì
slotId. Hoàn thành construction sinh placementId mới, ghi vào `PlacedModules`.

**Presentation**: `PlacementModeController` mới — bật khi `ShelterPanel` gọi (chọn xong Module
+ Zone), vẽ ghost sprite theo `Camera.main.ScreenToWorldPoint(mouse)` + khung mờ biên Zone,
xanh/đỏ theo `CanPlaceAt`, click trái xác nhận (submit Command), ESC huỷ. Đây là tương tác
chuột đầu tiên trong game (mọi thứ trước giờ dùng phím + OnGUI) — cần input action mới hoặc
đọc `Mouse.current` trực tiếp.

**UI**: `ShelterPanel` đổi luồng — mỗi Zone hiện danh sách Module xây được (nút "Chọn vị trí"
thay vì "Xây" trực tiếp) → đóng panel, bật `PlacementModeController`. Danh sách Module đã xây
hiện theo `PlacedModules` (không còn khớp 1-1 với Slot).

## Người dùng cần test gì

Ghi vào `docs/backlog/NEED-USER-PLAYTEST.md` sau khi code xong: đặt Module ở nhiều vị trí khác
nhau trong Zone, thử đặt ngoài biên Zone (phải bị từ chối), thử đặt chồng lên Module đã có
(phải bị từ chối), ESC huỷ giữa chừng không mất vật liệu.
