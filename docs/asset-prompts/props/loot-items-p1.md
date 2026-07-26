# Loot Items P1 — Resource mẫu cho Exploration Loop

- **Asset ID(s):** `item_water_bottle`, `item_canned_food`, `item_battery`, `item_toolbox` (vật nặng), `item_water_container_20l` (vật cồng kềnh)
- **Category:** prop (loot)
- **Milestone:** P1 — đúng 5 resource mẫu của backlog mục 5.3 (Water, Food, Battery, 1 nặng, 1 cồng kềnh)
- **Ưu tiên:** cao (quyết định loot là câu hỏi kiểm chứng của Gate P1 — item phải phân biệt được ngay bằng mắt)

> **Đã có sẵn:** `Assets/Art/Production/Loot/` và `LootPackE/` — kiểm tra trước khi generate mới.

## Generation Prompt (English)

> Set of 5 survival loot item sprites drawn in isometric projection for a fixed orthographic 2D camera, flat stylized shading, European urban context, muted palette with one clear accent color per item for instant recognition at small on-screen size: (1) 500ml plastic water bottle with blue label; (2) canned food tin with faded red-orange paper label, slightly dented; (3) chunky D-cell battery pair with yellow-black wrap; (4) heavy steel toolbox, dark red, closed, with sturdy handle and visible latches; (5) bulky translucent 20-liter water container jug with carry handle and blue cap. Clean silhouettes, minimal surface detail, slight wear, consistent lighting direction across the set. Each as a separate sprite on a fully transparent background, no baked shadow, no outline halo.

## Ràng buộc kỹ thuật

| Item | Kích thước thật | Sprite | Gameplay (tham chiếu Definition) | Accent |
| --- | --- | --- | --- | --- |
| `item_water_bottle` | 0.07×0.07×0.22 m | 128 px | nhẹ, stack được | xanh dương |
| `item_canned_food` | 0.08×0.08×0.11 m | 128 px | nhẹ, stack được | đỏ cam |
| `item_battery` | 0.06×0.03×0.06 m | 128 px | nhẹ, stack được | vàng-đen |
| `item_toolbox` | 0.5×0.25×0.3 m | 256 px | **nặng** (weight cao, volume vừa) | đỏ sẫm |
| `item_water_container_20l` | 0.35×0.35×0.5 m | 256 px | **cồng kềnh** (volume cao, Carried Object 2 tay) | trắng đục + xanh |

- Pivot: đáy giữa footprint (đặt trên sàn/kệ, khớp Y-sort).
- Mỗi item phải đọc được ở ~40 px trên màn hình 1080p ở zoom gameplay — silhouette + accent màu quan trọng hơn chi tiết.
- Icon UI: cắt lại từ chính sprite world này, không vẽ icon riêng ở P1.

## Checklist cleanup (sprite)

- [ ] Kích thước pixel đúng bảng, tỉ lệ tương đối giữa 5 item hợp lý
- [ ] Nền trong suốt sạch, không viền halo
- [ ] Pivot đáy giữa, nhất quán cả set
- [ ] Hướng ánh sáng thống nhất cả set
- [ ] Phân biệt được cả 5 item ở zoom xa nhất của camera gameplay
- [ ] Collider2D đơn giản khớp footprint
