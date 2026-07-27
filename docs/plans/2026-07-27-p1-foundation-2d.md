# Plan P1 — Foundation 2D isometric → Gate P1

Phạm vi: BL-P1-01..22 (P1-A skeleton, P1-B technical foundation → Gate M1, P1-C exploration
gameplay → Gate P1). P2/P3/P4 dùng plan riêng, viết tại mỗi gate.

Thiết kế class-level M1/M2 bám theo `2026-07-24-mvp-coding-plan.md` (vẫn khớp schema JSON
hiện có). Plan này ghi phần **khác** so với tài liệu đó: ràng buộc 2D và việc dùng lại
Definition JSON sẵn có.

## Ràng buộc nền

- **2D isometric**: Tilemap Isometric, `SpriteRenderer` + `Collider2D`, `Rigidbody2D`
  kinematic, camera orthographic không xoay, `transparencySortMode = CustomAxis` với
  `transparencySortAxis = (0,1,0)`.
- **PPU 100, 1 unit = 1 mét** — khớp `spritePixelsToUnits: 100` của art đã import.
- **Pivot**: art hiện có pivot center (0.5, 0.5). Sprite gắn làm **child** của entity root,
  offset lên trên để root transform nằm ở chân → Y-sort theo root, không phải theo tâm sprite.
- **Definition JSON không viết lại**: `Assets/StreamingAssets/Definitions/` đã có 18 file,
  schema snake_case, `definition_version 0.14.0`. Data layer phải đọc khớp schema này.

## Assembly

```
LastHope.Data      (không dep)
LastHope.Core      → Data
LastHope.Systems   → Core, Data
LastHope.Presentation / LastHope.UI / LastHope.DebugTools → Systems, Core, Data
LastHope.EditorTools (Editor-only) → tất cả
LastHope.Tests.EditMode / .PlayMode → tất cả
```

## S1 — P1-A skeleton (BL-P1-01..05)

| Item | File | Nội dung |
| --- | --- | --- |
| BL-P1-01 | 9 `.asmdef` + cây folder `Assets/Game/**` | ép dependency một chiều |
| BL-P1-04 | `Core/Diagnostics/GameLog.cs` | log có category + level, tắt được theo category |
| BL-P1-02 | `Presentation/Camera/CameraRig.cs` | orthographic follow X/Y, zoom qua `orthographicSize`, set `transparencySortMode`/`Axis` |
| BL-P1-03 | `Presentation/Player/PlayerController.cs` | `Rigidbody2D` kinematic, Move map thẳng world X/Y, `SpeedModifier` hook |
| BL-P1-04 | `DebugTools/Overlay/DebugOverlay.cs` | F1 toggle, FPS, vị trí X/Y |
| BL-P1-01 | `Presentation/Boot/BootLoader.cs` | `00_Boot` → additive `10_GamePersistent` |
| — | `EditorTools/SceneSetup.cs` | menu `Last Hope/Build Sprint 1 Scenes`, sinh scene bằng code (scene không commit tay) |
| BL-P1-05 | `EditorTools/BuildScript.cs` | build Windows player qua batchmode |

Input đọc từ `Assets/Input/GameControls.inputactions` có sẵn (map `Gameplay`, action `Move`
Vector2 / `Zoom` Axis / `Interact` Button...) qua `InputActionAsset` serialize — không dùng
C# wrapper codegen.

Scene sinh bằng code, không sửa tay: `00_Boot`, `10_GamePersistent`, `90_TestSystems`.

## S2–S4 — P1-B → Gate M1 (BL-P1-06..13)

Theo `2026-07-24-mvp-coding-plan.md` mục "M1 chi tiết": Definition Registry + WorldState +
RNG + Serializer (S2), Clock + Tick + EventBus + Command Layer (S3), Save + Debug Panel v1 +
test suite (S4). Điểm bắt buộc khác plan gốc: `DefinitionLoader` phải parse đúng 18 file JSON
đang có, không tự định nghĩa schema mới.

**Gate M1:** sim 24h không drift, save/load roundtrip bit-exact, tick chính xác, 6 file test
EditMode xanh.

## S5–S6 — P1-C → Gate P1 (BL-P1-14..22)

Theo `2026-07-24-mvp-coding-plan.md` mục "M2 chi tiết": Interaction + Item + Inventory (S5),
Search + Storage + Travel + Location blockout + Telemetry (S6). Content lấy từ `items_p1.json`,
`locations_p1.json`, `routes_p1.json`, `searchpoints_p1.json` — không tự chế số mới.

**Gate P1:** một chuyến không vét sạch location; đồ bỏ lại nằm nguyên container qua save/load;
playtest xác nhận có quyết định bỏ-lại-đồ.

## Verification mỗi sprint

Batchmode compile 0 lỗi → EditMode test → build Windows → smoke test headless → cập nhật
`BACKLOG.md` + `CODEMAP.md` cùng commit.

> Batchmode cần **đóng Unity Editor** (lock `Temp/UnityLockfile`).

## User cần tự test bằng mắt (sau S1)

- Mở `00_Boot`, Play: có tự load additive `10_GamePersistent` không.
- Nhân vật hiện đúng sprite, camera căn giữa nhân vật, không lệch.
- WASD di chuyển đúng hướng world (lên = +Y màn hình).
- Cuộn chuột zoom có nấc hợp lý, không quá nhanh/chậm.
- F1 bật/tắt Debug Overlay, FPS và toạ độ X/Y cập nhật.
- Sprite nhân vật sort đúng khi đi trước/sau vật thể khác (kiểm ở S6 khi có prop).
