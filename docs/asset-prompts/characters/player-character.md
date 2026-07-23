# Player Character — Nhân vật chính

- **Asset ID(s):** `character_player`
- **Category:** character
- **Milestone:** P1 (placeholder capsule dùng trước; asset này thay thế khi qua cleanup)
- **Ưu tiên:** trung bình (không chặn gameplay — capsule đủ cho Gate P1)

## Generation Prompt (English)

> Full-body 3D character of a European urban survivor in their early 30s, gender-neutral silhouette, wearing a worn dark-olive rain jacket with hood down, rolled-up work pants, rubber boots, and a compact backpack. Lean build, practical posture. Stylized low-poly game character, clean topology for rigging, T-pose, muted desaturated color palette (dark olive, wet asphalt grey, faded navy), slightly weathered clothing with damp stains. No weapons. Designed to read clearly from a fixed isometric orthographic camera at 35 degrees pitch — strong silhouette, distinct head/torso/leg separation, no fine surface details. Neutral facial features, low-poly hair. Single character, no background, no props in hands.

### Variant prompts

- **Wet variant** (P2): same character, jacket glistening wet, hood up, darker saturation.
- **Overloaded pose reference** (concept only): same character hunched forward carrying a large water container with both hands.

## Ràng buộc kỹ thuật

- Chiều cao: **1.7 m** (pivot giữa hai chân, đứng trên y=0).
- Poly budget: 8k–15k tris (character duy nhất luôn trên màn hình — được phép cao nhất project).
- Texture: 2048, một material chính.
- Rig: humanoid Unity, không cần finger bones.
- Animation baseline cần có (có thể generate/mua riêng, retarget): idle, walk, run, carry-walk (2 tay phía trước), interact-crouch, search-loop (lục lọi), incapacitated.
- Movement KHÔNG dùng root motion (tech spec mục 6) — animation in-place.

## Checklist cleanup (Blender)

- [ ] Cao đúng 1.7 m, apply transform
- [ ] Pivot giữa hai chân tại y=0
- [ ] Topology sạch quanh khớp vai/hông cho rig
- [ ] UV không chồng, 1 material
- [ ] Silhouette đọc rõ ở camera orthographic 35.264°/45° với zoom gameplay
- [ ] Capsule collider tham chiếu: radius 0.3 m, height 1.7 m
