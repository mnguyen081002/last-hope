# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Hiện trạng

**Chưa có assembly/class/file C# nào.** `Assets/Game/**`, `Assets/Tests/**`, `Assets/Scenes/**` trống.

Data và art thì đã có sẵn — xem 2 mục ngay dưới trước khi định viết lại content từ đầu.

---

## Content data có sẵn

`Assets/StreamingAssets/Definitions/` — 18 file JSON, `definition_version 0.14.0`, dùng lại trực tiếp. Schema trên đĩa là **snake_case** — code Data layer phải đọc theo convention này (`SnakeCaseNamingStrategy`, xem `docs/plans/2026-07-24-mvp-coding-plan.md` mục "Quyết định kiến trúc khóa").

| File | Nội dung | Dùng ở sprint |
| --- | --- | --- |
| `manifest.json` | `definition_version` | Definition Registry (BL-P1-06) |
| `balance.json` | inventory cap/overload, travel load factor, condition rates, new_game | baseline số liệu, xuyên suốt |
| `items_p1.json`, `items_p2.json`, `items_p3_materials.json` | item def: weight/volume/stack/use_effects | P1, P2, P3 |
| `locations_p1.json`, `locations_p4.json` | location def | P1, P4 |
| `routes_p1.json`, `routes_p4.json` | route def | P1, P4 |
| `searchpoints_p1.json`, `searchpoints_p4.json` | search point + loot table | P1, P4 |
| `modules_p3.json`, `shelterzones_p3.json` | build module + shelter zone | P3 |
| `events_p3.json`, `events_p4.json`, `events_p4_minh.json` | event def | P3, P4 |
| `npcs_p4.json`, `phases_p4.json` | NPC + disaster phase timeline | P4 |

## Art có sẵn

`Assets/Art/` — **743 PNG sprite**. Đáng chú ý: `Production/Character8Direction/Frames/` (walk 8 hướng × 4 frame), `Production/Terrain*`, `Production/World*`, `Production/Loot*`. Prompt sinh asset: `docs/asset-prompts/`.

---

## Assembly map (thiết kế mục tiêu, chưa dựng)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI
```

Dependency một chiều: `Data → Core → Systems → Presentation/UI/DebugTools`. Test assembly tham chiếu tất cả. `EditorTools` chỉ Editor-only.

## Scene flow (thiết kế mục tiêu, chưa dựng)

`00_Boot` → additive `10_GamePersistent` (services + Player/Camera/HUD sống suốt phiên) →
`SceneFlowController` load scene gameplay theo `LocationDefinition.SceneName`.

---

## LastHope.Core

Chưa có gì.

## LastHope.Data

Chưa có gì.

## LastHope.Systems

Chưa có gì.

## LastHope.Presentation

Chưa có gì.

## LastHope.UI

Chưa có gì.

## LastHope.DebugTools

Chưa có gì.

## LastHope.EditorTools

Chưa có gì.

---

## Ghi chú thiết kế bắt buộc

- **2D isometric là ràng buộc khóa cứng**: Tilemap Isometric, `SpriteRenderer` +
  `Collider2D`, `Rigidbody2D` kinematic, camera orthographic không xoay +
  `transparencySortMode = CustomAxis`. Không `Rigidbody`/`CharacterController`/mesh/raycast
  occlusion. Chi tiết: `technical-specification.md`.
- Placement (grid/anchor/socket, sort order, floor toggle...) phải theo đúng
  `docs/00-project-overview/isometric-game-placement-rules.md`.
- Definition JSON đã có sẵn (bảng trên) — code Data layer phải khớp schema snake_case hiện
  hành, không tự đặt schema mới rồi phải viết lại content.
- `docs/mvp-product-backlog.md` mô tả chi tiết từng item — đọc cùng `BACKLOG.md` trước khi
  bắt đầu implement.
