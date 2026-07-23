# Asset Prompts — Last Hope MVP Black Rain

Folder này chứa các file `.md`, mỗi file là **prompt để generate asset 3D** (AI 3D generation như Meshy/Tripo, hoặc concept image cho Blender blockout). Đây là bước đầu của asset pipeline (BL-X-07):

```
AI Concept → AI 3D Generation / Blender Blockout → Blender Cleanup → Scale/Pivot
→ Topology/UV → Material → Collider → Unity Import → Isometric Camera Review
```

## Quy ước bắt buộc (theo technical-specification.md mục 17–18)

| Thuộc tính | Giá trị |
| --- | --- |
| Scale | **1 Unity Unit = 1 meter** — mọi prompt phải ghi kích thước thật bằng mét |
| Modular grid | Base 0.5 m; tường bội số 1 m; sàn cao 3 m; cửa cao 2.2 m |
| Pivot prop | Đáy giữa |
| Pivot cửa | Tại bản lề |
| Pivot tường | Góc dưới của modular grid |
| Pivot nhân vật | Giữa hai chân |
| Style | Stylized low/mid-poly, silhouette rõ, đọc được từ camera isometric |
| Camera check | Orthographic, pitch 35.264°, yaw 45° — asset phải đọc rõ ở góc này |
| Texture nhỏ (prop nhỏ) | 512–1024 |
| Texture prop lớn / module | 1024–2048 |
| Texture nhân vật | 2048 |
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
- Kích thước thật (m), pivot, poly budget ước lượng, texture size
- Variant cần có

## Checklist cleanup (Blender)
- [ ] Scale đúng mét, apply transform
- [ ] Pivot đúng quy ước
- [ ] Topology sạch, xóa mặt thừa/không nhìn thấy từ camera iso
- [ ] UV không chồng, material theo convention chung
- [ ] Collider đơn giản (box/capsule), không mesh collider phức tạp
- [ ] Review dưới camera orthographic 35.264°/45°
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

**Lưu ý P1:** gameplay Sprint 1–6 chạy bằng primitive blockout trước; asset từ prompt chỉ thay thế dần khi đã pass cleanup. Không chờ asset để code.
