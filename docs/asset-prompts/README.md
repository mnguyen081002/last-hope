# Asset Prompts — Last Hope MVP Black Rain

Folder này chứa các file `.md`, mỗi file là **prompt để generate sprite 2D** (AI image generation). Đây là bước đầu của asset pipeline (BL-X-07):

```
AI Concept → AI 2D Generation → Tách nền + cleanup sprite → Chuẩn hóa pixel size/pivot
→ Cắt sprite sheet → Palette normalization → Collider2D → Unity Import (PPU/pivot/sorting)
→ Isometric Camera Review
```

## Quy ước bắt buộc (theo technical-specification.md mục 17–18)

| Thuộc tính | Giá trị |
| --- | --- |
| Scale | **1 Unity Unit = 1 meter**, quy ra pixel qua PPU thống nhất — mọi prompt ghi kích thước thật bằng mét để suy ra pixel size |
| Lưới iso | Ô tile diamond, tỉ lệ 2:1 (rộng gấp đôi cao); prop khai báo footprint theo **số ô**, không theo mét tuyệt đối |
| Pivot prop | Đáy giữa footprint (điểm chạm sàn) — quyết định Y-sort |
| Pivot cửa | Đáy giữa ô cửa |
| Pivot tường | Đáy góc ô tường theo lưới |
| Pivot nhân vật | Giữa hai chân |
| Style | Sprite phẳng vẽ theo góc chiếu isometric, silhouette rõ, không shading phức tạp |
| Camera check | Orthographic 2D không xoay — asset phải đọc rõ ở zoom gameplay thực tế |
| Hướng nhân vật | 8 hướng (khớp `Assets/Art/Production/Character8Direction/`) |
| Kích thước sprite prop nhỏ | 128–256 px cạnh dài |
| Kích thước sprite prop lớn / module tường | 256–512 px |
| Kích thước frame nhân vật | 256 px, nền trong suốt |
| Định dạng | PNG, alpha thật (không viền trắng/matte), không nền |
| Màu sắc | Palette xám lạnh, ẩm ướt, tông đô thị châu Âu sau mưa bão; accent màu cho item gameplay-critical |
| Bối cảnh | **Thành phố châu Âu** (kiến trúc townhouse/khu phố cũ châu Âu, biển hiệu tiếng Anh generic) |

## Template file prompt

Mỗi file `.md` theo cấu trúc:

```markdown
# <Tên asset>

- **Asset ID(s):** <id snake_case, khớp Definition JSON>
- **Category:** character | environment | prop | equipment | vfx | audio
- **Milestone:** P1 | P2 | P3 | P4...
- **Ưu tiên:** cao / trung bình / thấp (theo PRI backlog)

## Generation Prompt (English)
<prompt dùng trực tiếp cho tool AI>

## Ràng buộc kỹ thuật
- Footprint (số ô tile), kích thước sprite (px), pivot
- Variant / số frame animation cần có

## Checklist cleanup (sprite)
- [ ] Tách nền sạch, alpha thật, không viền halo
- [ ] Kích thước pixel đúng tỉ lệ ô tile, PPU thống nhất toàn project
- [ ] Pivot đúng quy ước (đáy giữa footprint)
- [ ] Palette khớp bảng màu chung
- [ ] Collider2D đơn giản khớp footprint, không polygon collider phức tạp
- [ ] Review trong scene dưới camera orthographic 2D ở zoom gameplay
```

## Trạng thái license

Mọi asset AI-generated phải qua kiểm tra license trước P7 (BL-P7-05). Ghi nguồn tool generate vào commit message khi import.

## Index

| File | Milestone | Trạng thái |
| --- | --- | --- |
| `characters/player-character.md` | P1 | Prompt sẵn sàng |
| `environment/main-shelter-interior-kit.md` | P1 | Prompt sẵn sàng |
| `environment/convenience-store-kit.md` | P1 | Prompt sẵn sàng |
| `props/loot-items-p1.md` | P1 | Prompt sẵn sàng |
| `props/equipment-p1.md` | P1 (mở rộng P2) | Prompt sẵn sàng |
| `vfx/` | P2+ | Chưa viết (rain, flood surface) |
| `audio/` | P4+ | Chưa viết (rain layer, alert, drain) |

**Lưu ý P1:** gameplay Sprint 1–6 chạy bằng sprite placeholder (ô màu phẳng) trước; asset từ prompt chỉ thay thế dần khi đã pass cleanup. Không chờ asset để code.

**Đã có sẵn trong repo:** `Assets/Art/Production/` chứa 743 PNG — nhân vật 8 hướng (`Character8Direction/Frames/`), terrain, world prop, loot pack. Kiểm tra chỗ này trước khi generate asset mới.
