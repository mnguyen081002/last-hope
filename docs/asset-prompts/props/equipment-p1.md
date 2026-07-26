# Equipment P1 — Trang bị người chơi

- **Asset ID(s):** `equip_backpack` (P1); dự kiến P2: `equip_rain_jacket`, `equip_rubber_boots`, `equip_work_gloves`, `equip_rope`, `equip_dry_bag`
- **Category:** equipment
- **Milestone:** P1 (backpack); phần P2 viết prompt sẵn nhưng chỉ generate sau Gate P1
- **Ưu tiên:** trung bình (P1 chỉ cần backpack thể hiện trên nhân vật; số liệu capacity nằm trong Definition JSON)

## Generation Prompt (English) — Backpack (P1)

> 2D game sprite of a mid-size hiking backpack, 30-liter class, worn dark grey with faded orange straps and buckles, side mesh pocket with a water bottle silhouette, slightly scuffed and rain-darkened fabric. Drawn in isometric projection for a fixed orthographic 2D camera, flat stylized shading, crisp silhouette readable at small on-screen size, European urban survival context. Dropped-on-ground state resting on its base. Fully transparent background, no baked shadow, no outline halo.

## Generation Prompt (English) — Equipment set P2 (generate sau Gate P1)

> Set of 5 flood-survival equipment sprites drawn in isometric projection for a fixed orthographic 2D camera, flat stylized shading, European urban context, muted palette with functional color accents: (1) heavy-duty yellow rain jacket with hood, folded on the ground; (2) knee-high dark green rubber boots, pair; (3) reinforced work gloves, grey-orange; (4) coiled climbing rope with carabiner, red; (5) roll-top waterproof dry bag, 20 liter, dark blue. Each as a separate sprite on a fully transparent background, consistent scale and lighting direction across the set, crisp silhouettes, slight wear, no baked shadow, no outline halo.

## Ràng buộc kỹ thuật

| Item | Kích thước thật | Footprint | Ghi chú gameplay |
| --- | --- | --- | --- |
| `equip_backpack` | 0.45×0.25×0.55 m | 1 ô | tăng capacity; bản dropped trên sàn + bản vẽ kèm trên sprite nhân vật |
| `equip_rain_jacket` (P2) | ~0.6×1 m | 1 ô | giảm Wet, tăng weight, có Durability |
| `equip_rubber_boots` (P2) | 0.3×0.12×0.4 m | 1 ô | giới hạn theo Flood Depth |
| `equip_work_gloves` (P2) | 0.25×0.12 m | 1 ô | |
| `equip_rope` (P2) | cuộn Ø0.35 m | 1 ô | giảm rủi ro Current |
| `equip_dry_bag` (P2) | Ø0.25×0.5 m | 1 ô | ba lô chống nước |

- Sprite size: 128–256 px cạnh dài/item, hoặc gộp atlas chung cho cả set.
- Pivot: đáy giữa footprint (bản dropped). Bản đeo trên người vẽ thẳng vào sprite nhân vật, không phải asset rời.
- Accent màu chức năng phải nhất quán giữa sprite trong world và icon UI (icon cắt lại từ chính sprite, không vẽ riêng).

## Checklist cleanup (sprite)

- [ ] Kích thước pixel đúng tỉ lệ ô tile theo bảng
- [ ] Nền trong suốt sạch, không viền halo
- [ ] Pivot đáy giữa footprint
- [ ] Hướng ánh sáng thống nhất toàn set
- [ ] Collider2D đơn giản khớp footprint (bản dropped)
- [ ] Review trong scene ở zoom gameplay — phân biệt được từng equipment
