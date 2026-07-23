# Convenience Store Kit — Cửa hàng tiện lợi (Location P1)

- **Asset ID(s):** `env_store_shelf_*`, `env_store_counter`, `env_store_fridge`, `env_store_backroom_rack`, `env_store_signage`
- **Category:** environment (location kit)
- **Milestone:** P1 — Location đầu tiên (BL-P1-20, KAN-34)
- **Ưu tiên:** cao (Location duy nhất của Gate P1; Search Point gắn vào kệ/quầy/kho)

## Bối cảnh thiết kế

Cửa hàng tiện lợi góc phố kiểu châu Âu (corner shop / mini-market) sau nhiều ngày mưa lớn mất điện: kệ xáo trộn, hàng rơi, nước rò vào từ cửa. Mỗi loại kệ là một **Search Point** khác nhau (kệ đồ uống, kệ đồ khô, tủ lạnh chết điện, quầy thu ngân, kho sau).

## Generation Prompt (English)

> Modular corner-shop interior kit, small European urban mini-market after days of storm blackout, stylized low-poly game environment. Pieces: double-sided metal gondola shelving unit partially stocked with scattered packaged goods, wall shelf unit, dead glass-door beverage fridge with dark interior, checkout counter with small shelf and non-working register, back-room steel storage rack with cardboard boxes, hanging shop sign panels. Slightly disordered look: fallen boxes, empty shelf gaps, a thin wet sheen on the lower 20cm. Muted palette with a few faded brand-color accents (no real brands, generic European packaging shapes). Each piece separate mesh, snap-friendly proportions on a 0.5 meter grid, silhouette readable from fixed isometric orthographic camera at 35 degrees pitch.

## Ràng buộc kỹ thuật

- Kệ gondola: **1×0.5×1.5 m** mỗi module, ghép được thành dãy. Pivot đáy giữa.
- Tủ lạnh: 1×0.7×2 m. Quầy: 1.5×0.6×1 m. Rack kho: 1×0.5×2 m.
- Poly: 300–1.2k tris/mảnh. Texture: atlas 1024–2048 chung kit.
- Hàng trên kệ là phần của mesh kệ (không phải item nhặt được — loot spawn từ Search Point logic, không phải từ mesh).
- Variant mỗi kệ: đầy / vơi / rỗng — thể hiện **progressive depletion** trực quan sau khi search.

## Checklist cleanup (Blender)

- [ ] Kích thước đúng mét, apply transform
- [ ] Pivot đáy giữa
- [ ] 3 variant depletion mỗi loại kệ (đầy/vơi/rỗng) cùng pivot — swap mesh không lệch
- [ ] Atlas UV chung
- [ ] Box collider bao ngoài từng mảnh
- [ ] Review dãy kệ tạo lối đi rõ ràng dưới camera iso (không che khuất player)
