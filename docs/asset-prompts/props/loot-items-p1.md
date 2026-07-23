# Loot Items P1 — Resource mẫu cho Exploration Loop

- **Asset ID(s):** `item_water_bottle`, `item_canned_food`, `item_battery`, `item_toolbox` (vật nặng), `item_water_container_20l` (vật cồng kềnh)
- **Category:** prop (loot)
- **Milestone:** P1 — đúng 5 resource mẫu của backlog mục 5.3 (Water, Food, Battery, 1 nặng, 1 cồng kềnh)
- **Ưu tiên:** cao (quyết định loot là câu hỏi kiểm chứng của Gate P1 — item phải phân biệt được ngay bằng mắt)

## Generation Prompt (English)

> Set of 5 survival loot props, stylized low-poly game items, European urban context, muted palette with one clear accent color per item for instant recognition from a fixed isometric camera: (1) 500ml plastic water bottle with blue label; (2) canned food tin with faded red-orange paper label, slightly dented; (3) chunky D-cell battery pair with yellow-black wrap; (4) heavy steel toolbox, dark red, closed, with sturdy handle and visible latches; (5) bulky translucent 20-liter water container jug with carry handle and blue cap. Clean silhouettes, minimal surface detail, slight wear. Each as separate mesh, no background.

## Ràng buộc kỹ thuật

| Item | Kích thước thật | Gameplay (tham chiếu Definition) | Accent |
| --- | --- | --- | --- |
| `item_water_bottle` | 0.07×0.07×0.22 m | nhẹ, stack được | xanh dương |
| `item_canned_food` | 0.08×0.08×0.11 m | nhẹ, stack được | đỏ cam |
| `item_battery` | 0.06×0.03×0.06 m | nhẹ, stack được | vàng-đen |
| `item_toolbox` | 0.5×0.25×0.3 m | **nặng** (weight cao, volume vừa) | đỏ sẫm |
| `item_water_container_20l` | 0.35×0.35×0.5 m | **cồng kềnh** (volume cao, Carried Object 2 tay) | trắng đục + xanh |

- Poly: 100–500 tris/item. Texture: 512/item hoặc atlas 1024 chung.
- Pivot: đáy giữa (đặt trên sàn/kệ không lún).
- Mỗi item cần đọc được ở kích thước ~40px trên màn hình 1080p ở zoom gameplay — silhouette + accent màu quan trọng hơn chi tiết.
- Icon UI: render chính mesh này trên nền trong suốt ở góc iso (không cần vẽ icon riêng ở P1).

## Checklist cleanup (Blender)

- [ ] Kích thước đúng bảng trên, apply transform
- [ ] Pivot đáy giữa
- [ ] ≤ 500 tris, không mặt thừa
- [ ] Phân biệt được cả 5 item ở zoom xa nhất của camera gameplay
- [ ] Box collider đơn giản
