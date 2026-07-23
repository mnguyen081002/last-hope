# Main Shelter Interior Kit — Kit modular nhà 2 tầng

- **Asset ID(s):** `env_shelter_wall_*`, `env_shelter_floor_*`, `env_shelter_stair`, `env_shelter_door_*`, `env_shelter_window_*`, `env_shelter_core_*`
- **Category:** environment (modular kit)
- **Milestone:** P1 blockout (primitive trước), art thay thế trước P4
- **Ưu tiên:** cao (Main Shelter là không gian chơi chính — BL-P3-01 cần blockout chuẩn grid từ P1)

## Bối cảnh thiết kế

Townhouse châu Âu 2 tầng trong khu phố cũ: tầng trệt (entrance, utility, storage) + tầng trên (living, upper safe area). Fixed Core Component làm Event Anchor: tủ điện (fuse box), ống thoát nước sàn tầng hầm/trệt (drain core), bồn nước, cầu dao tổng.

## Generation Prompt (English)

> Modular interior construction kit for an old European townhouse / rowhouse, stylized low-poly game environment. Pieces: plastered masonry wall segments with faded paint and rising damp stains at the base, worn herringbone parquet and stone-tile floor segments, straight wooden staircase with simple carved railing, paneled interior wooden door, heavy front door with transom window, tall casement window with wooden shutters. Fixed fixtures as separate pieces: vintage fuse box with ceramic fuses on wall mount, cast-iron floor drain grate, metal water tank on a stand, tiled kitchen counter with ceramic sink. Muted palette: aged plaster off-white, warm grey stone, faded green-blue painted wood typical of old European flats, damp discoloration near floor level. Clean modular seams, designed to snap on a 0.5 meter grid. Each piece as separate mesh, no background, readable from fixed isometric orthographic camera at 35 degrees pitch and 45 degrees yaw.

## Ràng buộc kỹ thuật

- **Grid:** mọi mảnh snap grid 0.5 m; tường dài 1 m / 2 m; cao **3 m**; cửa cao **2.2 m**.
- Pivot tường: góc dưới theo grid. Pivot cửa: bản lề. Pivot fixture: đáy giữa.
- Poly: tường/sàn ≤ 300 tris/mảnh; fixture 500–1.5k tris.
- Texture: 1024–2048 atlas chung cho kit; shared material tối đa (static batching).
- Mảnh tường cần variant: nguyên / có ô cửa / có ô cửa sổ / góc.
- Tường phía camera sẽ bị wall-fade — mặt trong phải có texture hoàn chỉnh.

## Danh sách mảnh tối thiểu (P1)

| Mảnh | Kích thước | Ghi chú |
| --- | --- | --- |
| Wall straight | 1×3 m, 2×3 m | + variant cửa, cửa sổ |
| Floor tile / parquet | 1×1 m, 2×2 m | trệt + lầu |
| Staircase | ngang 1 m, lên 3 m | kèm chiếu nghỉ |
| Front door | 1×2.2 m | entrance chính — Event Anchor Flood Barrier |
| Interior door | 0.9×2.2 m | |
| Fuse box | 0.4×0.6 m | Event Anchor (Grid Failure) |
| Floor drain | 0.5×0.5 m | Event Anchor (Drain Backflow) |
| Water tank | 1×1×1.2 m | Water System |

## Checklist cleanup (Blender)

- [ ] Snap đúng grid 0.5 m, apply transform
- [ ] Pivot theo quy ước từng loại
- [ ] Không z-fighting khi ghép mảnh liền kề
- [ ] Atlas UV chung, shared material
- [ ] Box collider per mảnh
- [ ] Review ghép thử 1 phòng hoàn chỉnh dưới camera iso
