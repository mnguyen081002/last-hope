# Production tách khỏi Placement + Ready to Claim — Implementation Notes

Bản tóm tắt implementation cho thiết kế ở
`docs/plans/2026-07-30-module-production-placement-loop.md` (đọc file đó trước — đây chỉ ghi lại
đã code gì, không lặp lại lý do thiết kế).

## Bối cảnh

User playtest phát hiện 2 lỗi: (1) Production vẫn bắt chọn vị trí trước khi sản xuất, (2) nhân
vật đứng im sau khi Production xong. Điều tra: (1) không phải regression — code trước đó vẫn là
flow cũ (BL-P3-03), thiết kế mới (viết cùng ngày) chưa được code. (2) không tìm ra nguyên nhân
qua đọc code tĩnh — cần user test lại sau khi (1) được sửa.

## Đã làm

- **Production tách khỏi Placement**: `BuildSystem.StartConstruction(moduleId)` không còn
  x/y/zoneId/rotation. `ConstructionState` chỉ còn `ModuleId`/`MinutesRemaining`/`Paused`.
- **Ready to Claim**: `ShelterState.ReadyToClaim` (Dictionary<ModuleId,int>) mới, độc lập khỏi
  `Construction`. `ApplyShortTick` khi xong tăng `ReadyToClaim` thay vì tự tạo `BuiltModuleState`.
  `ClaimProductionCommand` (mới) cộng packed item vào túi Player qua `BuildSystem.ClaimProduction`.
- **Packed item chuyển hẳn sang túi Player**: cả nguồn Claim lẫn Tháo (`DismantleModule`) đều
  cộng vào `world.Player.Inventory` (trước đó vào Shelter Storage). `CanRedeployAt`/
  `RedeployModule` đọc/trừ từ Player. `DismantleModuleCommand` thêm `BuildSystem.CanDismantle`
  chặn khi túi đầy (edge case "Remove" trong design doc).
- **Zone không còn chọn trước lúc Placement**: `BeginPlacementMode` chỉ còn mang `ModuleId`.
  `PlacementModeController` tự resolve Zone mỗi frame từ `ModuleDefinition.AllowedZoneIds` lọc
  theo tầng đang đứng (`PlayerFloorState.CurrentFloor`) + vị trí chuột
  (`ShelterZoneDefinition.Contains`). Không còn auto-teleport tầng lúc mở Placement Mode —
  `module_elevated_storage` (2 Zone, 2 tầng) giờ tự resolve theo tầng người chơi đang đứng, đi
  cầu thang đổi tầng ngay trong lúc ghost đang mở nếu cần Zone tầng kia.
- **UI**: `ShelterPanel` — bỏ danh sách "Chọn vị trí"/"Đặt lại (×N)" theo từng Zone, thay bằng
  khu "Sản xuất" (flat list mọi Module, nút "Sản xuất") + khu "Ready to Claim" (nút "Nhận") ở đầu
  panel. `InventoryPanel` — thêm nút "Đặt" cạnh packed item (dùng
  `BuildSystem.TryFindModuleByPackedItem`), publish `BeginPlacementMode` + đóng panel, theo đúng
  pattern nút "Mặc" đã có.
- **Fix phụ tìm được khi review**: `PlacedModuleHoverMenu.OnGUI` gọi `PointerOverUI.MarkHover(true)`
  vô điều kiện (không check `rect.Contains(mousePos)` như mọi panel khác) — đã sửa cho nhất quán.
  Không chắc đây là nguyên nhân bug #2 (đứng im) — `PointerOverUI` hiện chỉ ảnh hưởng camera
  zoom, chưa có bằng chứng nó ảnh hưởng movement.
- **Save**: `SaveFile.CurrentSaveVersion` 2 → 3 (đổi schema `ConstructionState`, thêm
  `ReadyToClaim`) — không viết migration, dự án chưa release (fail-fast pattern có sẵn).
- **Bug thật tìm được qua test, đã sửa**: packed item (`item_packed_*`) là `two_hand_carry: true`
  trong content — nằm ở `InventoryState.CarriedObjectItemId`, không phải `Slots`. `CanRedeployAt`/
  `RedeployModule` bản đầu dùng thẳng `InventoryOps` (chỉ đọc/ghi `Slots`, không biết TwoHandCarry)
  nên sẽ **luôn báo không có Module đã gói** dù vừa Claim/Tháo xong — tính năng Đặt sẽ không hoạt
  động được nếu không sửa. Sửa: `CanRedeployAt` dùng `InventoryOwnerOps.CountOf` (đã biết phân
  biệt Carried vs Slots), `RedeployModule` dùng helper mới `RemovePackedItemFromPlayer` (check
  `ItemDefinition.TwoHandCarry` rồi xoá đúng chỗ). Phát hiện qua 242 EditMode test batchmode —
  6 test ban đầu fail đúng vào các case Claim/Dismantle→Player inventory, dẫn tới tìm ra gốc rễ.

## Không làm trong lần này

- **Snap Grid** (mục 3.4 design doc — cell size 1.0, overlay lưới mờ) — chưa implement, không
  liên quan tới 2 bug đã báo. Ghi vào BACKLOG.md để không quên.
- Rotate/footprint hộp — đã có sẵn trước khi bắt đầu việc này (không phải phần việc này làm).
- Không thêm nút "Đặt" trùng lặp trong `ShelterPanel` (design doc gợi ý "không chỉ InventoryPanel")
  — chỉ đặt ở `InventoryPanel`, vì Placement không còn gắn 1 Zone cụ thể nên hiển thị theo Zone
  trong ShelterPanel không còn hợp lý.

## Cần user test

1. Sản xuất 1 Module tại Shelter Console — xác nhận **không** bị hỏi chọn vị trí/Zone.
2. Rời Shelter, quay lại sau khi hết giờ (hoặc Sleep fast-forward) — xác nhận "Ready to Claim"
   hiện đúng, bấm "Nhận" cộng đúng packed item vào túi (mở Inventory kiểm tra).
3. Từ Inventory bấm "Đặt" cạnh packed item — ghost hiện, đặt được vào đúng Zone, không tốn thêm
   vật liệu/thời gian.
4. Test riêng `module_elevated_storage` — đứng tầng Ground thấy ghost hợp lệ ở `ground_storage`,
   đi cầu thang lên tầng Upper thấy ghost chuyển sang hợp lệ ở `upper_living`.
5. Tháo 1 Module đã đặt — packed item về đúng túi Player, đặt lại được ngay.
6. **Trọng tâm**: lặp lại đúng kịch bản đã gây "nhân vật đứng im" trước đây (sản xuất → chờ xong)
   — xác nhận còn tái hiện không. Nếu còn, mở Console Unity ngay lúc đó, chụp lại toàn bộ log/
   exception xuất hiện đúng thời điểm đứng im để điều tra tiếp (đọc code tĩnh không tìm ra được).
