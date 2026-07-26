# Player Character — Nhân vật chính

- **Asset ID(s):** `character_player`
- **Category:** character
- **Milestone:** P1 (sprite placeholder dùng trước; asset này thay thế khi qua cleanup)
- **Ưu tiên:** trung bình (không chặn gameplay — placeholder đủ cho Gate P1)

> **Đã có sẵn:** `Assets/Art/Production/Character8Direction/Frames/` — walk 8 hướng × 4 frame.
> Kiểm tra bộ này trước khi generate mới; prompt dưới đây dùng để bổ sung state còn thiếu.

## Generation Prompt (English)

> 2D game sprite of a European urban survivor in their early 30s, gender-neutral silhouette, wearing a worn dark-olive rain jacket with hood down, rolled-up work pants, rubber boots, and a compact backpack. Lean build, practical posture. Drawn in isometric projection as seen from a fixed orthographic 2D camera, three-quarter top-down view. Flat stylized shading, crisp readable silhouette, muted desaturated palette (dark olive, wet asphalt grey, faded navy), slightly weathered clothing with damp stains. No weapons. Strong head/torso/leg separation, minimal fine detail so it reads at small on-screen size. Single character centered, fully transparent background, no ground shadow baked in, no border or outline halo.

### Variant prompts

- **8 hướng:** same character rendered in 8 isometric facing directions (N, NE, E, SE, S, SW, W, NW), consistent proportions and palette across all directions, each on transparent background.
- **Wet variant** (P2): same character, jacket glistening wet, hood up, darker saturation.
- **Overloaded pose** (P2): same character hunched forward carrying a large water container with both hands.

## Ràng buộc kỹ thuật

- Chiều cao nhân vật trong sprite tương ứng **1.7 m** thực tế — quy ra pixel theo PPU chung của project, giữ nhất quán với tile sàn.
- Frame size: 256 px, nền trong suốt, nhân vật căn giữa theo trục ngang.
- Pivot: **giữa hai chân** (điểm chạm sàn) — quyết định Y-sort so với prop.
- Animation baseline: idle, walk, carry-walk (2 tay phía trước), interact, search-loop, incapacitated. Mỗi state × 8 hướng, 4 frame/hướng cho walk.
- Sprite sheet cắt đều theo grid cố định, đặt tên `<state>-<direction>-<frame>.png` (khớp convention `Character8Direction/Frames/`).

## Checklist cleanup (sprite)

- [ ] Nền trong suốt sạch, không viền halo/matte trắng
- [ ] Chiều cao pixel khớp PPU và tỉ lệ tile sàn
- [ ] Pivot giữa hai chân, nhất quán mọi frame (nhân vật không "nhảy" khi đổi frame)
- [ ] 8 hướng cùng tỉ lệ, cùng palette, không lệch sáng tối giữa các hướng
- [ ] Silhouette đọc rõ ở zoom gameplay thực tế trong scene
- [ ] Collider2D tham chiếu: capsule/box khớp footprint chân, không bao cả sprite
