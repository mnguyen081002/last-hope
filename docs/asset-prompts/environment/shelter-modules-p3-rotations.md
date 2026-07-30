# Shelter Modules P3 — Directional Isometric Sprites

- **Asset IDs:** `module_barrier`, `module_pump`, `module_elevated_storage`, `module_purifier`, `module_battery_bank`
- **Category:** environment / gameplay module
- **Milestone:** P3
- **Priority:** high
- **Design source:** `docs/plans/2026-07-30-module-production-placement-loop.md`, section 3.6

## Art decision

| Module | Footprint đề xuất | Art cần tạo | Rotate |
| --- | --- | --- | --- |
| Flood Barrier | 2×1 ô | 4 hướng | Bật |
| Portable Pump | 2×1 ô | 4 hướng | Bật |
| Elevated Storage | 2×1 ô | 4 hướng | Bật |
| Water Purifier | 2×1 ô | 4 hướng | Bật |
| Battery Bank | 1×1 ô, đối xứng | 1 hướng | Ẩn |

Các footprint trên là art target để bảo đảm Rotate nhìn thấy rõ. Cần đồng bộ lại với
`ModuleDefinition` khi schema Width × Height được triển khai.

## Base Generation Prompt (English)

Dùng nguyên khối prompt này và nối thêm **một** Module Brief ở phần sau. Generate từng Module
riêng để giữ consistency giữa bốn hướng tốt hơn.

> Create a production-ready directional sprite set for a 2D isometric survival game. Show exactly the same gameplay module in four physical rotations around its vertical axis: 0°, 90°, 180°, and 270°, corresponding to the module facing NE, SE, SW, and NW on the world grid. The orthographic camera must remain completely fixed; rotate the object, never the camera or canvas. Use a three-quarter top-down isometric projection aligned to a 2:1 diamond grid.
>
> Preserve identical construction, proportions, dimensions, attachments, colors, wear marks, material placement, and lighting across all four rotations. The back must be a believable reverse view of the same object, not a redesigned variant. Keep the footprint contact points perfectly aligned and keep the visual center, scale, vertical height, and bottom-center pivot consistent in every frame. Make front/back orientation readable through functional details such as controls, access panels, hose ports, shelf openings, braces, or latches.
>
> Art direction: a grounded European urban flood-survival setting after days of storms and infrastructure failure; improvised but credible civilian equipment assembled from salvaged wood and steel. Stylized semi-realistic hand-painted 2D game sprite, crisp readable silhouette, simplified material detail, restrained flat-to-soft shading, subtle edge wear, damp stains, chipped paint, muted cold-grey, faded olive, dark steel, aged wood, and restrained rust palette. Use one small functional accent color only where gameplay readability benefits. Match a serious post-disaster tone; no fantasy, no sci-fi, no military weapon styling.
>
> Lighting must be baked consistently from the upper-left of the image in every rotation, with restrained ambient occlusion only. Do not add a cast ground shadow. Each rotation must be isolated with generous transparent padding and no overlap. Output a clean 2×2 contact sheet on a fully transparent background: top-left 0°, top-right 90°, bottom-left 180°, bottom-right 270°. No labels, text, numbers, floor tile, environment, scene, character, UI, border, frame, glow, selection outline, or colored background. True alpha transparency with no white or dark matte halo. Each cell must be suitable for a separate 512×512 PNG after slicing.

## Module Briefs

### `module_barrier` — Flood Barrier, 4 hướng

> A two-tile-long, one-tile-deep removable indoor flood barrier built from thick weathered timber planks reinforced by bolted scrap-steel uprights and diagonal braces. A compressed dark rubber seal runs along the bottom edge. The wet-facing side has stronger cross-bracing and water-pressure plates; the dry-facing side has two clearly visible locking handles and quick-release latches, making front and back readable. Waist-high, sturdy, portable, and believable for sealing the entrance of an old European townhouse. Small faded yellow safety markings, no readable text. Exact gameplay footprint: 2×1 isometric grid cells.

### `module_pump` — Portable Pump, 4 hướng

> A compact heavy-duty electric dirty-water pump mounted on a low welded steel skid frame, with a cylindrical pump housing, protected motor, short capped intake and outlet hose couplings, carry handles, vibration feet, and a small weatherproof control box. The service/control side is clearly readable, while the opposite side exposes the pump housing and protective grille. Used but maintained, damp and scratched, with a restrained faded orange safety accent. No long loose hoses extending outside the footprint. Exact gameplay footprint: 2×1 isometric grid cells.

### `module_elevated_storage` — Raised Storage Rack, 4 hướng

> A handmade raised storage rack built entirely from salvaged timber, keeping critical supplies above floodwater. Two sturdy open shelf bays, thick legs on broad feet, cross-braced rear structure, raised lower platform, and a few generic sealed crates and waterproof containers secured with straps. The accessible shelf face must be clearly different from the braced rear. Slightly uneven reclaimed wood but structurally credible; muted blue-grey container accent, no labels or readable text. Exact gameplay footprint: 2×1 isometric grid cells.

### `module_purifier` — Water Purifier, 4 hướng

> A compact improvised electric water-purification station on a rectangular steel frame: one opaque feed canister, two replaceable vertical filter housings, a small pump, short fixed pipes, a protected control panel, and a clean-water outlet tap. The operator side has the control panel, filter access, and tap; the rear has pipe manifolds and a removable maintenance panel. Functional plumbing must remain logically connected and identical across rotations. Worn off-white and dark steel body with one restrained clean-water blue accent. No leaking water effect and no hoses outside the footprint. Exact gameplay footprint: 2×1 isometric grid cells.

### `module_battery_bank` — Battery Bank, 1 sprite, không Rotate

Dùng prompt riêng sau, không dùng yêu cầu contact sheet 4 hướng ở Base Prompt:

> Create one production-ready sprite of a compact 1×1-cell battery bank for a 2D isometric survival game, viewed from a fixed orthographic three-quarter top-down camera and aligned to a 2:1 diamond grid. Design it as an intentionally rotation-neutral square module: four equal protective steel sides, a centered reinforced top lid, identical corner guards, symmetrical ventilation slots, recessed cable sockets on all four sides, and one small status lamp centered on top. It must have no meaningful front or back so the game can hide the Rotate control. Grounded European urban flood-survival setting; improvised but credible civilian equipment assembled from two large batteries and a salvaged steel enclosure. Stylized semi-realistic hand-painted 2D sprite, crisp silhouette, simplified detail, muted cold-grey and faded olive metal, restrained rust and damp wear, tiny amber status-light accent. Consistent upper-left lighting, no cast ground shadow. Centered with generous padding on a fully transparent background, no text, label, floor tile, environment, character, UI, border, glow, or matte halo. Bottom-center pivot; suitable for one 512×512 PNG.

## Negative Prompt

> different objects in each view, inconsistent design, inconsistent attachments, changing number of parts, changing proportions, camera rotation, camera orbit, perspective view, front view, side view, top-down plan view, fisheye, 3D render, photorealism, voxel art, cartoon outline, cel-shaded anime, fantasy machinery, futuristic sci-fi technology, military weapon, oversized cables, loose hoses outside footprint, ground plane, floor diamond, cast shadow, scenery, room background, water surface, character, hands, text, letters, numbers, logo, watermark, UI, frame, cropped object, overlapping sprites, opaque background, checkerboard baked into image, white fringe, black halo

## Output & Naming

- Sprite rotatable: slice contact sheet thành `module_<name>_r000.png`, `_r090.png`, `_r180.png`, `_r270.png`.
- Battery Bank: `module_battery_bank_r000.png`; cấu hình `IsRotatable = false`.
- Giữ cùng canvas, crop bounds và pivot giữa mọi hướng của cùng một Module.
- Pivot: đáy giữa footprint; không lấy đáy của phần hình nhìn thấy làm pivot nếu nó lệch tâm footprint.
- Review ở camera gameplay thật: silhouette, Y-sort, interaction side và footprint 2×1 ↔ 1×2 sau Rotate.
- Nếu model không giữ được cấu trúc qua 4 hướng, generate `r000` trước rồi dùng image-reference của chính frame đó để tạo tuần tự `r090`, `r180`, `r270`; không chấp nhận bốn thiết kế gần giống nhau.

