# Full Reset — xóa toàn bộ code game, làm lại từ đầu (2026-07-27)

## Bối cảnh

Sau S19 (chuyển 3D→2D) và nhiều vòng vá liên tiếp không dứt điểm (camera lệch tâm, zoom,
label mirror, rơi khỏi map, "Redesign Placement System" chưa xác nhận bằng mắt), lần đầu
user chỉ yêu cầu xóa-viết-lại 4 hạng mục foundation (S1-S4: project setup/camera/input-
movement/logging-debug overlay — xem `docs/plans/2026-07-26-s1-s4-2d-foundation-redo.md`),
giữ lại `LastHope.Core`/`Data`/`Systems` vì xác nhận tái dùng được (logic thuần, không phụ
thuộc 2D/3D).

Sau khi redo S1-S4, map trong Editor vẫn hiện bản cũ (do chưa chạy lại
`Last Hope/Build Sprint 1 Scenes`) — user quyết định xóa sạch toàn bộ, kể cả phần vừa xác
nhận giữ lại (Core/Data/Systems), để làm lại từ đầu hoàn toàn. Đã hỏi lại rõ ràng vì mâu
thuẫn với quyết định trước đó — user xác nhận đúng ý muốn xóa hết.

## Phạm vi đã xóa

- `Assets/Game/**` toàn bộ (Core, Data, Systems, UI, DebugTools, Presentation, EditorTools,
  Generated) — 19 sprint logic gameplay, 287 EditMode test.
- `Assets/Tests/**` — EditMode/PlayMode test assembly (test code cho phần đã xóa).
- `Assets/Scenes/**` — toàn bộ 7 scene `.unity` (Boot, GamePersistent, TestSystems, 3
  Location, MainShelter).

## Giữ nguyên (không phải "code game", ngoài phạm vi)

- `docs/**` — toàn bộ design doc (`mvp-product-backlog.md`, `main-shelter-design.md`,
  `event-system-design.md`, `technical-specification.md`,
  `isometric-game-placement-rules.md`...) vẫn mô tả đúng **cái cần xây**, chỉ có phần đã
  xây (BACKLOG.md/CODEMAP.md) là reset về chưa làm.
- `Assets/Art/**` (1549 file — Production/Generated/Placeholder, nhiều bộ nhân vật/terrain/
  loot pack có sẵn), `Assets/Audio`, `Assets/Input/GameControls.inputactions`,
  `Assets/Settings` (URP asset), `Assets/TextMesh Pro`, `ProjectSettings/`, `Packages/` —
  tài sản/config không phải code gameplay, không đụng tới.

## Lịch sử cũ tra ở đâu

Toàn bộ code/scene đã xóa vẫn còn nguyên trong git history. Commit cuối cùng trước khi xóa:
`128679e4fd1ffad051c43649a22967afc112ea8a`. Muốn xem lại implementation cũ của bất kỳ hệ
thống nào (VD `WaterIntrusionRules`, `EventSystem`...): `git show 128679e:Assets/Game/...`.
`BACKLOG.md`/`CODEMAP.md` bản đầy đủ trước reset cũng nằm ở commit này.

## Kết quả

- `docs/backlog/BACKLOG.md`: toàn bộ Trạng thái reset về `Backlog`, ghi chú implementation
  cũ gỡ bỏ (không còn đúng vì code không còn tồn tại), chỉ giữ ID/Hạng mục để map với
  `docs/mvp-product-backlog.md`.
- `docs/backlog/CODEMAP.md`: reset về khung rỗng (giữ assembly map dự kiến, xóa hết bảng
  file/class vì chưa có gì).
- Chưa viết code mới trong lần reset này — đây chỉ là dọn sạch. Bắt đầu implement lại từ
  P0/P1-A theo `docs/mvp-product-backlog.md` là bước tiếp theo, khi user sẵn sàng.

## User cần biết

- Unity Editor đang mở project trong lúc xóa — cần đóng Play mode nếu đang chạy, và cho
  Editor Refresh/reload để nhận diện các file đã mất (sẽ thấy nhiều lỗi "missing scene
  reference" trong Build Settings — cần tự dọn Scenes In Build trong Project Settings vì
  đó là asset binary, AI không tự sửa).
- Chưa có bước tiếp theo nào được thực hiện — cần user xác nhận hướng làm lại (theo đúng
  thứ tự P0→P1→... như cũ, hay đổi cách tiếp cận) trước khi AI viết code mới.
