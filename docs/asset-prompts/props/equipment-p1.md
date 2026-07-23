# Equipment P1 — Trang bị người chơi

- **Asset ID(s):** `equip_backpack` (P1); dự kiến P2: `equip_rain_jacket`, `equip_rubber_boots`, `equip_work_gloves`, `equip_rope`, `equip_dry_bag`
- **Category:** equipment
- **Milestone:** P1 (backpack); phần P2 viết prompt sẵn nhưng chỉ generate sau Gate P1
- **Ưu tiên:** trung bình (P1 chỉ cần backpack thể hiện trên nhân vật; số liệu capacity nằm trong Definition JSON)

## Generation Prompt (English) — Backpack (P1)

> Mid-size hiking backpack, 30-liter class, worn dark grey with faded orange straps and buckles, side mesh pocket with a water bottle silhouette, slightly scuffed and rain-darkened fabric. Stylized low-poly game prop, European urban survival context, clean silhouette readable from a fixed isometric orthographic camera. Two states: worn-on-back version fitted to a 1.7 m humanoid character, and dropped-on-ground version. No background.

## Generation Prompt (English) — Equipment set P2 (generate sau Gate P1)

> Set of 5 flood-survival equipment props, stylized low-poly, European urban context, muted palette with functional color accents: (1) heavy-duty yellow rain jacket with hood, hanging and folded variants; (2) knee-high dark green rubber boots, pair; (3) reinforced work gloves, grey-orange; (4) coiled climbing rope with carabiner, red; (5) roll-top waterproof dry bag, 20 liter, dark blue. Each as separate mesh, clean silhouettes, slight wear, no background.

## Ràng buộc kỹ thuật

| Item | Kích thước thật | Ghi chú gameplay |
| --- | --- | --- |
| `equip_backpack` | 0.45×0.25×0.55 m | tăng capacity; version đeo trên lưng + version rơi trên sàn cùng pivot đáy giữa |
| `equip_rain_jacket` (P2) | treo ~0.6×1 m | giảm Wet, tăng weight, có Durability |
| `equip_rubber_boots` (P2) | 0.3×0.12×0.4 m | giới hạn theo Flood Depth |
| `equip_work_gloves` (P2) | 0.25×0.12 m | |
| `equip_rope` (P2) | cuộn Ø0.35 m | giảm rủi ro Current |
| `equip_dry_bag` (P2) | Ø0.25×0.5 m | ba lô chống nước |

- Poly: 200–800 tris/item. Texture: 512–1024/item hoặc atlas chung.
- Pivot: đáy giữa (bản dropped); bản worn theo attach point trên rig nhân vật.
- Accent màu chức năng phải nhất quán giữa prop 3D và icon UI (render từ mesh).

## Checklist cleanup (Blender)

- [ ] Kích thước đúng bảng, apply transform
- [ ] Pivot đúng quy ước 2 bản (dropped / worn)
- [ ] Bản worn fit rig nhân vật 1.7 m không xuyên mesh
- [ ] ≤ 800 tris, UV sạch
- [ ] Box collider bản dropped
- [ ] Review dưới camera iso — phân biệt được từng equipment ở zoom gameplay
