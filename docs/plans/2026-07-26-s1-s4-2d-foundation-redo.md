# S1-S4 Foundation Redo (2D isometric) — 2026-07-26

## Bối cảnh

S19 (2026-07-25) chuyển Presentation layer từ 3D isometric sang 2D isometric kiểu Project
Zomboid. Sau đó có nhiều vòng vá lỗi liên tiếp (camera lệch tâm, zoom sai, label mirror-flip,
rơi khỏi map, tên entity snake_case, thanh nước không hiện...) và commit mới nhất
(`a716ff8`, "Redesign Placement System with grid snapping and validation") vẫn đang sửa tầng
sinh scene, chưa từng được xác nhận bằng mắt trong Editor.

User quyết định: xóa và viết lại từ đầu 4 hạng mục foundation gốc (S1-S4 = BL-P1-01..04)
thay vì tiếp tục vá, để có một nền 2D chắc chắn trước khi build lại phần còn lại.

**Đã kiểm tra trước khi xóa:** đọc lại `CameraRig.cs`, `PlayerController.cs`,
`PlayerAvatarSync.cs`, `InteractionDetector.cs`, `WorldLabel.cs`, `DebugOverlay.cs` — cả 6
file đã convert 2D đúng, không thấy bug logic rõ ràng (đã nêu với user, user vẫn chọn xóa
viết lại S1-S4 theo đúng ý ban đầu, không cần chẩn đoán thêm).

## Phạm vi

**Giữ nguyên (không đụng — theo đúng xác nhận của user: tái sử dụng được vì không phụ thuộc
2D/3D):**
- `LastHope.Core` / `LastHope.Data` / `LastHope.Systems` — toàn bộ logic gameplay S5-S18,
  287 test xanh, không đổi lúc migrate 2D.
- `LastHope.UI` / `LastHope.DebugTools/Panel` — UGUI screen-space panel, không phụ thuộc
  world 2D/3D.
- `Assets/Game/Presentation/World/*` (WorldLabel, SearchPointView, ShelterStorageView,
  TravelPointView, BuildSlotView, CoreComponentView, DrainCoreView, PlayerSpawnPoint),
  `Assets/Game/Presentation/Interaction/*`, `Assets/Game/Presentation/Boot/*` — thuộc S5+
  (Interaction System, scene content), không phải S1-S4, đã convert 2D sạch.
- `Assets/Game/EditorTools/SceneSetup.cs`, `GridPlacementSystem.cs`,
  `RenderPipelineSetup.cs`, `BuildScript.cs`, `TmpSetup.cs` — không xóa; chỉ chạy lại
  `Last Hope/Build Sprint 1 Scenes` sau khi API Camera/Player/DebugOverlay giữ nguyên tên
  public để không phải sửa wiring.
- Packages/ProjectSettings — đã đúng cấu hình 2D (tilemap, 2d.sprite, URP unified
  renderer) từ S19, không cần đổi.

**Xóa và viết lại (S1-S4):**
| Sprint gốc | File | Lý do |
| --- | --- | --- |
| S2 — Camera isometric (BL-P1-02) | `Assets/Game/Presentation/Camera/CameraRig.cs` | |
| S3 — Input + movement (BL-P1-03) | `Assets/Game/Presentation/Player/PlayerController.cs`, `PlayerAvatarSync.cs` | |
| S4 — Logging + debug overlay (BL-P1-04) | `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `GameLog.cs` (Core) giữ nguyên — logging logic không phải Presentation |

S1 (Project setup) — audit, không xóa: package manifest/asmdef/URP asset đã đúng, xóa viết
lại sẽ không có gì khác biệt (rủi ro làm gãy asmdef reference vô ích).

## Thiết kế lại

Giữ nguyên public API (tên class, field serialize, method) của 3 file để
`SceneSetup.cs`/scene hiện có không cần sửa wiring — GameObject nào từng gắn `CameraRig` /
`PlayerController` / `DebugOverlay` vẫn hoạt động sau khi regenerate scene.

- **CameraRig**: orthographic 2D cố định, không xoay, follow target X/Y, zoom qua
  `orthographicSize`, `transparencySortMode = CustomAxis`.
- **PlayerController**: `Rigidbody2D` kinematic, input Move map thẳng world X/Y (top-down,
  không grid-lock — đúng kiểu Project Zomboid, object mới snap lưới chứ player thì không),
  `SpeedModifier` hook cho Overload/Flood.
- **PlayerAvatarSync**: ghi `PlayerState.PositionX/Y` mỗi frame, áp lại từ state khi
  `WorldStateReloaded`, không tự ý đổi `PositionLocationId`.
- **DebugOverlay**: F1 toggle, FPS, vị trí 2D (X/Y, không Z).

## Việc cần làm

1. Xóa 4 file trên (+ .meta).
2. Viết lại từ đầu, giữ namespace/tên class/public surface như cũ.
3. Chạy `Last Hope/Build Sprint 1 Scenes` (batchmode) để regenerate toàn bộ scene.
4. Chạy batchmode compile + EditMode test suite — kỳ vọng không đổi (287 test, vì Core
   không đụng).
5. Build Windows Development, headless smoke test.
6. Cập nhật `BACKLOG.md`/`CODEMAP.md`.

## User cần tự test bằng tay trong Editor

- Mở scene `20_MainShelter`, nhấn Play: camera có căn giữa nhân vật không, WASD di chuyển
  đúng hướng world không.
- Zoom chuột cảm giác có ổn không (nấc rõ ràng, không quá nhanh/chậm).
- F1 bật/tắt Debug Overlay, số liệu FPS/Pos có cập nhật không.
- Đi thử quanh map, kiểm tra có đi lọt ra ngoài biên/ground không (đã có boundary wall từ
  S19, không đổi lần này).
- Xác nhận `GridPlacementSystem`/`SceneSetup` mới nhất (đã có từ trước, không đổi lần này)
  đặt object đúng ô lưới, không chồng lấn — đây là phần chưa từng xác nhận bằng mắt.
