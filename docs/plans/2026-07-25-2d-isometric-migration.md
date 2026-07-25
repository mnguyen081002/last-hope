# Chuyển Presentation layer từ 3D isometric sang 2D isometric (kiểu Project Zomboid)

## Context

Dự án hiện là Unity **3D** isometric (`technical-specification.md` §1-2: `Project Type: 3D`, camera ortho pitch 35.264°/yaw 45°, `CharacterController` 3D, collider 3D). Lý do đổi sang 2D: không có khả năng render/tạo asset 3D (không Blender/3D pipeline khả dụng cho 1 dev). 2D isometric (sprite phẳng đặt trên lưới iso, kiểu Project Zomboid) chỉ cần art 2D — dễ tạo hơn nhiều so với model 3D.

**Phạm vi thực tế nhỏ hơn vẻ ngoài của nó:** `LastHope.Core`/`Data`/`Systems` (toàn bộ simulation, command, rule — ~18 sprint đã build) là C# thuần, không đụng UnityEngine 3D API nào → **không đổi gì**. Việc đổi 3D→2D chỉ nằm gọn trong `LastHope.Presentation` + `EditorTools` (dựng scene) + 2 file tài liệu + `Packages/manifest.json`. Đã audit toàn bộ codebase: no NPC presentation tồn tại (NavMeshAgent trong tech-spec chưa từng được implement) → không cần migrate NavMesh, chỉ cần loại khỏi package baseline dự kiến.

**Giả định đã chốt** (nêu rõ để user xác nhận khi duyệt plan): **True 2D**, không phải "2.5D" (giữ physics 3D, chỉ đổi mesh→sprite billboard) — vì lý do đổi là *không dựng được asset 3D*, và "giống Project Zomboid" đúng nghĩa là engine 2D thật (SpriteRenderer/Tilemap/Physics2D), không phải 3D giả trang 2D.

---

## 1. Tài liệu (đổi định hướng trước, đúng quy ước CLAUDE.md)

### `docs/00-project-overview/technical-specification.md`
- §1: `Project Type: 3D` → `2D`.
- §2 (Camera): bỏ pitch/yaw 3D — camera 2D ortho cố định, không xoay, nhìn thẳng trục Z. "Isometric" nằm ở art (tile vẽ theo góc chiếu iso) + Y-sort, không phải góc camera xoay.
- §4 (Render Pipeline): ghi rõ dùng **URP 2D Renderer** (`Renderer2DData`), Light2D cho hiệu ứng mưa/đêm thay baked lighting 3D. Bỏ mục "Shadow Distance/Stylized low-poly material".
- §6 (Physics): Player = `Rigidbody2D` + `Collider2D` (không `CharacterController`, không gravity — top-down 2D không có rơi/nhảy, bỏ hẳn khái niệm "slope/step"). NPC = chưa implement gì cả (giữ nguyên tình trạng "chưa có visual"); khi cần, dùng steering đơn giản tới target trước, hoãn pathfinding phức tạp tới khi content thật sự cần (tránh phụ thuộc NavMeshAgent2D/plugin ngoài).
- §17 (Asset Convention): scale đổi từ "1 Unity Unit = 1 meter" 3D sang quy ước pixel-per-unit + kích thước tile iso (vd 64×32 px/tile — điền cụ thể lúc có art thật). Pivot/modular grid diễn đạt lại theo lưới ô vuông/diamond 2D.
- §23/24: bỏ `Physics module 3D`, `AI Navigation` khỏi baseline; xác nhận `physics2d` (đã có sẵn trong manifest) + thêm `Tilemap` module.

### `docs/00-project-overview/isometric-game-placement-rules.md`
Viết lại toàn bộ cho 2D (tài liệu hiện ghi rõ "Dành cho Unity 3D isometric" — thay bằng "Dành cho Unity 2D isometric (Tilemap)"). Thay khái niệm:
- Grid/anchor/socket → ô tile lưới iso (`Grid.CellLayout.Isometric`) + `footprint` tính bằng số ô, không world position 3D.
- "Camera readability / occlusion / wall fade / roof hide" (dựa raycast 3D che khuất) → thay bằng **Y-sort** (`Camera.transparencySortMode = CustomAxis`) và cơ chế ẩn/hiện tầng (tầng trên hiện/ẩn theo `ShelterZoneDefinition.Floor` hiện có, không phải raycast occlusion).
- "Cầu thang/ramp liên tục nối cao độ" → tile cầu thang chuyển tầng tức thời (đổi `sortingLayer`/index tầng), không có độ dốc vật lý.
- Ground/level bounds bằng `TilemapCollider2D` + `CompositeCollider2D` thay `BoxCollider` 3D quanh biên.
- Giữ nguyên phần "Placement pipeline" (§10) và "Definition of Done" (§12) ở mức khái niệm — chỉ đổi validator nào đọc world 3D sang world 2D.

---

## 2. `Packages/manifest.json`
- Bỏ `com.unity.modules.physics` (vật lý 3D — không còn `CharacterController`/collider 3D nào dùng nữa).
- Giữ `com.unity.modules.physics2d` (đã có sẵn, hiện ghi chú "chưa dùng" — nay dùng thật).
- Thêm `com.unity.modules.tilemap` (Tilemap runtime, built-in module) + `com.unity.2d.sprite` (Sprite Editor cho asset 2D sau này).
- Không thêm `AI Navigation` (dừng kế hoạch cũ, xem lý do ở mục Context).

---

## 3. `EditorTools/RenderPipelineSetup.cs`
Đổi `UniversalRendererData` → `Renderer2DData` (URP 2D Renderer) khi tạo `LastHope_URP.asset`, để có Light2D/Shadow Caster 2D cho hiệu ứng mưa/đêm sau này. Giữ nguyên phần Linear color space + gán Graphics/Quality.

---

## 4. `Presentation/Camera/CameraRig.cs`
Viết lại: bỏ `transform.rotation = Quaternion.Euler(35.264, 45, 0)` và toàn bộ logic offset xoay theo pitch/yaw. Camera 2D ortho đứng yên hướng `-Z`, follow target theo X/Y: `desiredPosition = target.position + Vector3(0,0,-10)`. Thêm `_camera.transparencySortMode = TransparencySortMode.CustomAxis` + `transparencySortAxis` (vector diagonal khớp tỉ lệ tile iso, vd `(0, 1, 0.26)` — tinh chỉnh khi có tile thật) để sprite tự sort đúng theo "xa/gần" iso. Zoom qua `orthographicSize` giữ nguyên logic hiện có.

## 5. `Presentation/Player/PlayerController.cs`
Viết lại di chuyển: `Rigidbody2D` (kinematic, di chuyển bằng `MovePosition`) thay `CharacterController`. Bỏ hẳn: gravity, `_verticalVelocity`, `fallResetY`/`_lastGroundedPosition` (không còn khái niệm "rơi khỏi map" trong top-down 2D — nguyên cụm logic fall-recovery bị loại bỏ, không phải lỗi cần giữ). `Flatten()` không cần nữa vì input Vector2 map thẳng sang X/Y thế giới (không cần chiếu theo hướng camera — camera 2D không xoay nên "hướng camera" luôn là hướng world cố định). `moveDirection = input` trực tiếp.

## 6. `Presentation/Player/PlayerAvatarSync.cs`
Đổi `CharacterController` → `Rigidbody2D`. Ghi `PositionX/PositionY` từ `transform.position` (X,Y thật của 2D). `PositionZ` không còn ý nghĩa — xoá field này khỏi `Core/State/PlayerState.cs` (orphan do chính thay đổi này tạo ra, đúng nguyên tắc "xoá thứ do thay đổi của mình làm thừa"), và grep toàn repo trước khi xoá để chắc không chỗ nào khác đọc `PositionZ` (test, serializer generic nên không cần sửa riêng).

## 7. `Presentation/Interaction/InteractionDetector.cs`
`Physics.OverlapSphereNonAlloc` → `Physics2D.OverlapCircleNonAlloc`. Cursor raycast tie-break: `cam.ScreenToWorldPoint(mouse)` (bỏ Z hoặc đặt Z=0) rồi `Physics2D.OverlapPoint`, so khớp với candidate thay vì `Physics.Raycast`.

## 8. `Presentation/World/WorldLabel.cs`
Đơn giản hoá: camera 2D không xoay nữa → bỏ hẳn `FacingRotation` (tính từ Euler 35.264/45) — label chỉ cần `Quaternion.identity` (hoặc `TextMeshPro` render mặc định luôn hướng camera vì camera nhìn thẳng trục Z không đổi).

## 9. `EditorTools/SceneSetup.cs` (thay đổi lớn nhất, 572 dòng, dựng 7 scene)
Thay pattern lặp lại nhiều lần trong file:
- `GameObject.CreatePrimitive(PrimitiveType.Plane)` (Ground) → `Grid` + `Tilemap` (`CellLayout.Isometric`) + `TilemapCollider2D`/`CompositeCollider2D` cho sàn/tường biên.
- `GameObject.CreatePrimitive(Cube/Cylinder/Capsule)` (search point, storage, travel point, zone marker, core component, player visual...) → `GameObject` + `SpriteRenderer` (dùng texture đơn sắc sinh runtime, giữ đúng tinh thần "blockout bằng màu" hiện tại — không chặn bởi việc chưa có art thật) + `Collider2D` tương ứng (`BoxCollider2D`/`CircleCollider2D`).
- `CreateBoundaryWalls`/`CreateWall`: `BoxCollider` 3D không renderer → `BoxCollider2D`/`EdgeCollider2D` không `SpriteRenderer`.
- `CreateRamp`: bỏ hình học dốc 3D — thay bằng 1 tile "Stairs" đặt tại điểm nối, chạm vào là đổi tầng hiển thị tức thời (set active/sorting layer cho `Zone`/`BuildSlot` của tầng đó), không còn phép tính pitch/yaw từ 2 điểm.
- Player trong `BuildGamePersistentScene`: `CharacterController` → `Rigidbody2D` (kinematic) + `CapsuleCollider2D`; visual capsule mesh → `SpriteRenderer`.
- Camera setup: bỏ set `rotation = Euler(35.264, 45, 0)`, giữ `orthographic = true` + `orthographicSize`.
- Các hàm còn lại (`BuildHudCanvas`, UI panel wiring, `BuildBootScene`) — **không đổi**, thuộc uGUI, không liên quan 3D/2D.

Các `*View.cs` khác (`SearchPointView`, `ShelterStorageView`, `TravelPointView`, `BuildSlotView`, `CoreComponentView`, `DrainCoreView`, `PlayerSpawnPoint`) — **không cần sửa logic**, chỉ đổi loại Collider mà `SceneSetup.cs` gắn kèm khi tạo GameObject cho chúng.

---

## Thứ tự thực hiện
1. Lưu plan này vào `docs/plans/2026-07-25-2d-isometric-migration.md` (đúng quy ước CLAUDE.md).
2. Cập nhật 2 tài liệu (`technical-specification.md`, `isometric-game-placement-rules.md`).
3. `Packages/manifest.json` + `RenderPipelineSetup.cs`.
4. `CameraRig.cs` → `PlayerController.cs` → `PlayerAvatarSync.cs` (+ xoá `PlayerState.PositionZ`) → `InteractionDetector.cs` → `WorldLabel.cs`.
5. `SceneSetup.cs` (phần lớn công sức) — sau đó chạy lại `Last Hope/Setup URP Pipeline` rồi `Last Hope/Build Sprint 1 Scenes` trong Unity Editor để tái tạo 7 scene bằng code mới.
6. Cập nhật `docs/backlog/BACKLOG.md` (thêm mục, đánh dấu Done khi xong) + `docs/backlog/CODEMAP.md` (Presentation table đổi mô tả theo 2D), commit theo BL-ID.

## Verification — người dùng cần test gì
Mở Unity Editor, chạy `Last Hope/Setup URP Pipeline` rồi `Last Hope/Build Sprint 1 Scenes`, sau đó Play từ `00_Boot`:
- Camera hiển thị góc iso cố định, không xoay, zoom bằng scroll vẫn hoạt động.
- WASD di chuyển nhân vật (giờ là sprite 2D) mượt, đúng hướng world (không còn lệch theo hướng camera).
- Đi tới biên map bị chặn bởi collider 2D (không có khái niệm "rơi khỏi map" nữa — xác nhận không còn bug rơi bản đồ vì đã bỏ cơ chế đó).
- Y-sort đúng: đi qua/đứng sau vật thể (search point, storage, travel point) sprite nhân vật che/bị che đúng theo vị trí Y, không hiện sai lớp.
- Phím E tương tác vẫn mở được Search Point / Shelter Storage / Travel World Map như cũ (S6/S8 hành vi không đổi).
- Đi cầu thang lên Upper Floor trong `20_MainShelter` chuyển tầng đúng (zone/build slot tầng trên hiện đúng).
- Chạy lại EditMode/PlayMode test suite (Unity Test Runner) — kỳ vọng **toàn bộ pass không đổi** vì Core/Data/Systems không bị sửa.
