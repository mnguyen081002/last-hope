# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Reset toàn bộ (2026-07-27)

Toàn bộ `Assets/Game/**` và `Assets/Tests/**` đã bị xóa (xem `docs/plans/2026-07-27-full-reset.md`). File này reset về khung rỗng — chưa có assembly/class/file nào tồn tại. Bản đầy đủ trước reset (19 sprint, toàn bộ hệ thống P0-P4 + S19) tra ở git history, commit `128679e4fd1ffad051c43649a22967afc112ea8a`.

---

## Assembly map (dự kiến, chưa dựng lại)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Systems, Presentation, DebugTools, UI
```

Dependency một chiều: `Data → Core → Systems → Presentation/UI/DebugTools`. Test assembly tham chiếu tất cả. `EditorTools` chỉ Editor-only.

## Scene flow

Chưa dựng lại. Thiết kế cũ (tham khảo khi làm lại): `00_Boot` → additive `10_GamePersistent`
(services + Player/Camera/HUD sống suốt phiên) → `SceneFlowController` load scene gameplay
theo `LocationDefinition.SceneName`.

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

## Ghi chú thiết kế cần nhớ khi làm lại

- Placement (grid/anchor/socket, sort order, floor toggle...) phải theo đúng
  `docs/00-project-overview/isometric-game-placement-rules.md`.
- Art 2D isometric kiểu Project Zomboid (quyết định S19, vẫn giữ) — dựng thẳng Tilemap
  Isometric + SpriteRenderer/Collider2D ngay từ đầu, không migrate từ 3D nữa.
- `docs/mvp-product-backlog.md` mô tả chi tiết từng item — đọc cùng `BACKLOG.md` trước khi
  bắt đầu implement lại.
