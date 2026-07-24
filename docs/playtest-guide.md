# Hướng dẫn chơi — Last Hope (build blockout P1, 2026-07-24)

Đây là **build blockout kỹ thuật**: toàn bộ vật thể trong scene là primitive Unity (Plane/Cube/Cylinder/Capsule) không texture, không màu phân biệt riêng — nhận diện vật thể qua **hình khối** và **tên GameObject** (hiện trong state dump), không phải qua hình ảnh/icon. Item trong UI hiện tên id thô (`item_water_bottle`...) vì chưa gán tên hiển thị. Mục đích build này là xác nhận **cơ chế** chơi được, chưa phải trải nghiệm hình ảnh cuối.

## 1. Khởi động

Chạy `Builds/Windows/LastHope.exe`, hoặc mở Editor và Play từ scene `00_Boot`.

Vào game, bạn xuất hiện tại **Shelter** (`20_MainShelter`), map nhỏ mặt phẳng xám.

## 2. Điều khiển

| Phím | Hành động |
| --- | --- |
| `W A S D` | Di chuyển (hướng theo góc camera, không theo hướng nhân vật) |
| Cuộn chuột (scroll) | Zoom camera in/out |
| `E` | Tương tác với vật thể gần nhất đang nhắm tới (Search / Storage / Travel) |
| `I` hoặc `Tab` | Bật/tắt bảng Inventory (túi đồ) |
| `F1` | Bật/tắt Debug Overlay (FPS, vị trí, build version) — luôn khả dụng |
| `F2` | Bật/tắt Debug Panel (cheat/dev tool — xem mục 7) |

Camera cố định góc isometric (nhìn chéo 45°, không xoay được) — chỉ zoom.

## 3. Nhận diện vật thể trong thế giới

Không có model/texture theo loại — mỗi loại điểm tương tác là **một hình khối cố định**, không đổi theo nội dung bên trong:

| Hình khối | Ý nghĩa | Prompt khi lại gần (nhấn E) |
| --- | --- | --- |
| Capsule (viên nang đứng) | Chính nhân vật (bạn) | — |
| Cube dẹt (1.5×1×0.5m) | **Điểm Search** (kệ đồ, quầy, kho) | "E — Search" |
| Cube (1×1×1m) | **Kho chứa đồ ở Shelter** (Shelter Storage) | "E — Storage" |
| Cylinder (trụ) | **Điểm Travel** (đi sang địa điểm khác) | "E — Travel" |

Prompt "E — ..." hiện giữa màn hình khi bạn đứng trong bán kính ~1.6m của vật thể (và nếu có nhiều vật gần nhau, con trỏ chuột trỏ vào vật nào sẽ ưu tiên vật đó).

**Không có cách nhận biết từ xa bên trong một điểm Search có gì** — phải tương tác (E) để mở ra danh sách đồ.

## 4. Bản đồ hiện có

Chỉ có 2 địa điểm, nối bằng 1 tuyến đường:

```
[Shelter] --route_shelter_store (25 phút game)--> [Cửa hàng tiện lợi]
```

- **Shelter** (`20_MainShelter`): 1 Kho chứa đồ (cube) + 1 điểm Travel (cylinder) đi ra cửa hàng.
- **Cửa hàng tiện lợi** (`41_Location_ConvenienceStore`): 6 điểm Search (cube dẹt) + 1 điểm Travel (cylinder) quay về shelter.

## 5. Search — cách mở đồ (đã đổi cơ chế 2026-07-24)

Đứng cạnh 1 trong 6 kệ trong cửa hàng, nhấn E:

- Lần **đầu tiên** mở, đồ trong đó được xác định (roll một lần, không đổi lại sau đó):
  - **Kệ nước ×2 và kệ đồ khô ×2**: LUÔN có hàng (3 nước hoặc 3 đồ ăn mỗi kệ) — đây là nguồn tài nguyên đảm bảo, không bao giờ trắng tay.
  - **Quầy thu ngân**: luôn có 1 đồ ăn, và **có thể** có thêm pin (60% cơ hội).
  - **Kho phía sau** (mở mất 2 phút game): **có thể** có toolbox, bình nước 20L, thêm nước, thêm pin — mỗi thứ là may rủi riêng, không đảm bảo có.
- Panel **Container** hiện bên phải màn hình: nút **Take** (lấy từng loại) hoặc **Take All** (lấy hết).
- Đồ không lấy hết vẫn **nằm nguyên trong kệ** — quay lại lấy sau vẫn còn, không bị roll lại, không biến mất.
- Mở lại đã roll: chỉ hiện panel với đồ còn lại, không random thêm gì mới.

## 6. Túi đồ (Inventory) & giới hạn mang vác

Nhấn `I`/`Tab` mở panel bên trái màn hình:

- Giới hạn túi: **15kg / 25L**.
- 2 thanh tiến trình (weight/volume) đổi màu theo tải:
  - **Xanh lá** — bình thường (≤100%)
  - **Cam** — Light overload (>100%, giảm tốc độ ×0.6)
  - **Đỏ** — Heavy overload (>130%, giảm tốc độ ×0.35)
  - Vượt **150%** (hard cap): không nhặt thêm được nữa (`InventoryFull`)
- Nút **Use** trên mỗi dòng item — hiện chỉ giảm số lượng 1 đơn vị, chưa có hiệu ứng hồi phục (Condition system chưa làm, S7).
- **Chưa có nút Drop** — muốn bỏ bớt đồ, hiện tại phải dùng Debug Panel hoặc chờ bản sau.

Vật cồng kềnh (`item_water_container_20l`, bình 20L) đánh dấu "hai tay" (two-hand carry) trong dữ liệu nhưng UI hiện chưa hiển thị cảnh báo riêng.

## 7. Kho chứa ở Shelter (Storage)

Đứng cạnh cube "ShelterStorage" ở Shelter, nhấn E → panel Container hiện, kèm thêm khối **"Your Inventory"** bên dưới (chỉ container này có, search point không có) — cho phép chuyển đồ **2 chiều**:
- **Take / Take All**: từ kho → túi bạn
- **Store**: từ túi bạn → kho

Kho không giới hạn dung lượng.

## 8. Di chuyển giữa 2 địa điểm (Travel)

Đứng cạnh cylinder "Travel", nhấn E:
- Route mất 25 phút game (nhân hệ số nếu bạn đang overload: Light ×1.25, Heavy ×1.5).
- Thời gian trong game trôi nhanh gấp bội thời gian thực (clock nội bộ tự fast-forward) — bạn sẽ thấy World Time nhảy khi xem Debug Panel.
- Sau khi đến, scene tự chuyển (load địa điểm mới, unload địa điểm cũ), bạn xuất hiện ở điểm spawn của map đó.

## 9. Lưu / Tải game

Chỉ thao tác được qua **Debug Panel** (F2) — chưa có menu Save/Load riêng cho người chơi thường:
- **Save**: gõ tên slot (mặc định `manual_0`) → bấm Save.
- **Autosave**: bấm nút, tự xoay vòng 3 slot.
- **Load**: danh sách các slot đã lưu hiện thành nút bấm sẵn (kèm thời gian lưu) — bấm để tải.
- Vị trí nhân vật, đồ trong túi, đồ còn lại trong các kệ đã search — tất cả được lưu và khôi phục đúng.

## 10. Debug Panel (F2) — công cụ dev, không phải gameplay chính thức

Mở bằng F2, gồm:
- World time hiện tại + nút Fast-forward (nhập số phút).
- Pause / chỉnh Time Scale (0×–10×) của simulation.
- **Add Item** (cheat): gõ id + số lượng → thêm thẳng vào túi, bỏ qua mọi luật. Id hợp lệ: `item_water_bottle`, `item_canned_food`, `item_battery`, `item_toolbox`, `item_water_container_20l`.
- **Travel** (cheat): gõ route id (`route_shelter_store`) → di chuyển ngay, không cần đứng tại điểm Travel.
- Save/Load như mục 9.
- **State dump**: toàn bộ WorldState hiện tại dạng JSON, cuộn xem được — hữu ích để soi giá trị chính xác (tọa độ, quantity, Rolled...) khi debug.

## 11. Giới hạn đã biết của build này

- Không có nhạc/âm thanh, không hiệu ứng hình ảnh.
- Không có UI riêng cho "chưa mở" vs "đã search hết" một kệ — phải nhấn E lại để xem panel.
- Chưa có hệ thống Condition (đói/khát/mệt/ướt/lạnh) — đó là S7 sắp tới.
- Chưa có hazard/flood/thời tiết — bản đồ tĩnh, không có nguy hiểm theo thời gian (S8).
- Chưa playtest cảm giác di chuyển/overload bằng người thật ngoài việc test cơ chế qua code — đánh giá "cảm thấy nặng khi overload" cần bạn tự trải nghiệm.
