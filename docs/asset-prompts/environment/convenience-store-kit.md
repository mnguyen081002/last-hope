# Convenience Store Kit — Cửa hàng tiện lợi (Location P1)

- **Asset ID(s):** `env_store_shelf_*`, `env_store_counter`, `env_store_fridge`, `env_store_backroom_rack`, `env_store_signage`
- **Category:** environment (location kit)
- **Milestone:** P1 — Location đầu tiên (BL-P1-20, KAN-34)
- **Ưu tiên:** cao (Location duy nhất của Gate P1; Search Point gắn vào kệ/quầy/kho)

## Bối cảnh thiết kế

Cửa hàng tiện lợi góc phố kiểu châu Âu (corner shop / mini-market) sau nhiều ngày mưa lớn mất điện: kệ xáo trộn, hàng rơi, nước rò vào từ cửa. Mỗi loại kệ là một **Search Point** khác nhau (kệ đồ uống, kệ đồ khô, tủ lạnh chết điện, quầy thu ngân, kho sau).

## Generation Prompt (English)

> Modular corner-shop interior kit for a 2D isometric game, small European urban mini-market after days of storm blackout, drawn in isometric projection for a fixed orthographic 2D camera, flat stylized shading. Pieces: double-sided metal gondola shelving unit partially stocked with scattered packaged goods, wall shelf unit, dead glass-door beverage fridge with dark interior, checkout counter with small shelf and non-working register, back-room steel storage rack with cardboard boxes, hanging shop sign panels. Slightly disordered look: fallen boxes, empty shelf gaps, a thin wet sheen along the floor line. Muted palette with a few faded brand-color accents (no real brands, generic European packaging shapes). Each piece as a separate sprite on a fully transparent background, consistent lighting direction, proportions snapping to a 2:1 isometric diamond tile grid, silhouette readable at gameplay zoom, no baked shadow, no outline halo.

## Ràng buộc kỹ thuật

- Footprint theo ô tile: kệ gondola **2×1 ô** mỗi module, ghép được thành dãy. Tủ lạnh 2×1 ô. Quầy 3×1 ô. Rack kho 2×1 ô.
- Chiều cao vẽ trong sprite: kệ ~1.5 m, tủ lạnh ~2 m, quầy ~1 m, rack ~2 m (quy ra pixel theo PPU chung).
- Sprite size: 256–512 px cạnh dài mỗi mảnh; atlas chung cho cả kit.
- Pivot: đáy giữa footprint — quyết định Y-sort với player.
- Hàng trên kệ là phần của sprite kệ (không phải item nhặt được — loot spawn từ Search Point logic).
- Variant mỗi kệ: đầy / vơi / rỗng — thể hiện **progressive depletion** trực quan sau khi search.

## Checklist cleanup (sprite)

- [ ] Footprint khớp đúng số ô tile, cạnh dưới trùng đường diamond của lưới
- [ ] Nền trong suốt sạch, không viền halo
- [ ] Pivot đáy giữa footprint
- [ ] 3 variant depletion mỗi loại kệ (đầy/vơi/rỗng) cùng pivot, cùng kích thước — swap sprite không lệch
- [ ] Hướng ánh sáng thống nhất toàn kit
- [ ] Collider2D khớp footprint từng mảnh
- [ ] Review dãy kệ tạo lối đi rõ ràng, sort order đúng khi player đi trước/sau kệ
