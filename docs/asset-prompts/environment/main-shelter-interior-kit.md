# Main Shelter Interior Kit — Kit modular nhà 2 tầng

- **Asset ID(s):** `env_shelter_wall_*`, `env_shelter_floor_*`, `env_shelter_stair`, `env_shelter_door_*`, `env_shelter_window_*`, `env_shelter_core_*`
- **Category:** environment (modular kit)
- **Milestone:** P1 blockout (sprite placeholder trước), art thay thế trước P4
- **Ưu tiên:** cao (Main Shelter là không gian chơi chính — BL-P3-01 cần blockout chuẩn grid từ P1)

## Bối cảnh thiết kế

Townhouse châu Âu 2 tầng trong khu phố cũ: tầng trệt (entrance, utility, storage) + tầng trên (living, upper safe area). Fixed Core Component làm Event Anchor: tủ điện (fuse box), ống thoát nước sàn tầng trệt (drain core), bồn nước, cầu dao tổng.

## Generation Prompt (English)

> Modular interior construction kit for a 2D isometric game, old European townhouse / rowhouse, drawn in isometric projection for a fixed orthographic 2D camera, flat stylized shading. Pieces: plastered masonry wall segments with faded paint and rising damp stains at the base, worn herringbone parquet and stone-tile floor tiles, straight wooden staircase with simple carved railing, paneled interior wooden door, heavy front door with transom window, tall casement window with wooden shutters. Fixed fixtures as separate pieces: vintage fuse box with ceramic fuses on wall mount, cast-iron floor drain grate, metal water tank on a stand, tiled kitchen counter with ceramic sink. Muted palette: aged plaster off-white, warm grey stone, faded green-blue painted wood typical of old European flats, damp discoloration near floor level. Floor pieces drawn as 2:1 isometric diamond tiles that tile seamlessly; wall pieces drawn along the two visible isometric axes with clean seams. Each piece as a separate sprite on a fully transparent background, consistent lighting direction, no baked shadow, no outline halo.

## Ràng buộc kỹ thuật

- **Grid:** sàn là tile diamond tỉ lệ 2:1, tile seamless. Tường bám 2 trục iso, dài 1 ô / 2 ô. Chiều cao vẽ: tường **3 m**, cửa **2.2 m** (quy ra pixel theo PPU chung).
- Pivot tường: đáy ô tường theo lưới. Pivot cửa: đáy giữa ô cửa. Pivot fixture: đáy giữa footprint.
- Sprite size: tường/sàn 256 px, fixture 128–256 px. Atlas chung cho kit.
- Mảnh tường cần variant: nguyên / có ô cửa / có ô cửa sổ / góc (2 chiều góc).
- **Không** thiết kế wall-fade: tầng trên/dưới xử lý bằng floor visibility toggle (xem `isometric-game-placement-rules.md` mục 6). Tường vẽ ở dạng thấy được từ camera cố định, không cần mặt khuất.

## Danh sách mảnh tối thiểu (P1)

| Mảnh | Footprint | Ghi chú |
| --- | --- | --- |
| Wall straight | 1 ô, 2 ô (mỗi trục iso) | + variant cửa, cửa sổ |
| Wall corner | 1 ô | 2 chiều góc |
| Floor tile / parquet | 1 ô | trệt + lầu |
| Staircase | 1×3 ô | kèm chiếu nghỉ; hướng lên phải quay về phía camera nhìn thấy |
| Front door | 1 ô | entrance chính — Event Anchor Flood Barrier |
| Interior door | 1 ô | |
| Fuse box | 1 ô (gắn tường) | Event Anchor (Grid Failure) |
| Floor drain | 1 ô | Event Anchor (Drain Backflow) |
| Water tank | 1×1 ô | Water System |

## Checklist cleanup (sprite)

- [ ] Tile sàn seamless, khớp chính xác lưới diamond 2:1
- [ ] Nền trong suốt sạch, không viền halo giữa các tile liền kề
- [ ] Pivot theo quy ước từng loại
- [ ] Hướng ánh sáng thống nhất toàn kit
- [ ] Sort order đúng khi player đi trước/sau tường và cầu thang
- [ ] Collider2D per mảnh khớp footprint
- [ ] Review ghép thử 1 phòng hoàn chỉnh trong scene ở zoom gameplay
