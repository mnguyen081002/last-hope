# Fix cầu thang leo dần — 2 bug quan sát được

## Bug quan sát được (user báo 2026-07-29)

1. **Sau khi leo hết lên tầng trên, tầng vừa rời (Ground) hoàn toàn rõ nét trở lại** — sai
   thiết kế: tầng dưới phải vẫn còn mờ nhẹ (35%, không va chạm được), không phải rõ hẳn.
2. **Cầu thang không đối xứng giữa 2 đầu**: đi từ đầu 1 (chân cầu thang, tầng dưới) thì mờ dần
   đúng như kỳ vọng; đi từ đầu 2 (đỉnh cầu thang, tầng trên) thì **vừa vào đã mờ nhất ngay**,
   không mờ dần.

## Nguyên nhân gốc (kiến trúc, không phải lỗi vặt)

`PlayerFloorState` hiện suy hướng leo (`climbingUp`) bằng cách so `TransitioningToFloor` với
`upperFloor` — **hướng leo được suy ra một lần lúc `BeginClimb`, dựa trên `CurrentFloor` tại
thời điểm đó**, rồi cache lại. Vấn đề:

- Nếu người chơi đứng nán lại đúng biên vùng (y ≈ bottomY hoặc topY) sau khi `CompleteClimb`
  đã chạy (`TransitioningToFloor` về null) nhưng **vẫn còn nằm trong vùng hình học** (`inside`
  vẫn true) — code hiện tại không có nhánh nào chỉnh lại state, để `CurrentFloor` "chốt" đúng
  theo `Update()` tiếp theo chỉ khi có cạnh vào/ra (`inside != wasInside`). Đứng yên trong vùng
  sau khi hoàn tất không kích hoạt gì thêm — nhưng bước lùi nhẹ rồi tiến lại (không ra khỏi
  vùng hẳn) có thể để lại `CurrentFloor` lệch so với vị trí hình học thật.
- Việc này khiến `climbingUp` (suy 1 lần, cache) đôi khi không khớp hướng di chuyển thật —
  đúng cơ chế gây ra "vào từ đầu 2 nhưng progress bị tính như đang leo lên" (ngược hướng thật).

**Kết luận**: đừng suy "hướng leo" từ state cũ (`CurrentFloor`/`TransitioningToFloor`) nữa.
Tính tiến độ **thuần theo hình học** mỗi frame — không cache hướng.

## Thiết kế lại

`PlayerFloorState` đổi từ mô hình "TransitioningToFloor + ClimbProgress theo hướng suy ra"
sang mô hình **blend thuần vị trí, không có khái niệm hướng**:

```text
CurrentFloor      — tầng đã "chốt" khi không đứng trong vùng cầu thang nào
BlendLowerFloor   — null khi không blend; khác null khi đang trong 1 StaircaseZone
BlendUpperFloor
BlendT            — 0 = hoàn toàn BlendLowerFloor, 1 = hoàn toàn BlendUpperFloor
                    (LUÔN tính lại từ vị trí Y hiện tại, không phụ thuộc lịch sử)
```

`StaircaseZone.Update()` mỗi frame, nếu `inside`:

```csharp
float t = Mathf.InverseLerp(bottomY, topY, pos.y); // thuần hình học, không phụ thuộc hướng
player.UpdateBlend(lowerFloor, upperFloor, t);
```

Rời vùng (`inside` chuyển false): `player.EndBlend()` — chốt `CurrentFloor` theo `BlendT` lúc
rời (>= 0.5 thì chốt `upperFloor`, ngược lại `lowerFloor`), xoá blend.

`FloorRenderController.Refresh()` đọc `IsBlending`/`BlendLowerFloor`/`BlendUpperFloor`/`BlendT`
thay `TransitioningToFloor`/`ClimbProgress` — logic nội suy alpha giữ nguyên, chỉ đổi nguồn dữ
liệu sang mô hình không có "hướng" lưu trữ.

**Vì sao sửa được cả 2 bug:**

- Bug 1 (rõ hẳn sau khi leo xong): `EndBlend()` chốt đúng `CurrentFloor`, sau đó
  `FloorRenderController` dùng lại nhánh nhị phân đã đúng từ trước (tầng dưới Dimmed) — không
  còn phụ thuộc việc `Changed` có bắn đúng lúc unwind hay không.
- Bug 2 (không đối xứng 2 đầu): `t` tính thuần từ `Y` mỗi frame, không suy hướng từ state cũ —
  vào từ đầu nào cũng ra đúng % theo đúng vị trí hình học, không có "hướng bị cache sai".

## Việc cần làm

1. Viết lại `PlayerFloorState.cs`: bỏ `TransitioningToFloor`/`BeginClimb`/`SetClimbProgress`/
   `CancelClimb`, thêm `BlendLowerFloor`/`BlendUpperFloor`/`BlendT`/`IsBlending`/`UpdateBlend`/
   `EndBlend`. Giữ `TeleportToFloor`/`ResetFloor` (Placement Mode + đổi scene vẫn cần).
2. Viết lại `StaircaseZone.Update()` theo mô hình mới — vẫn không dùng Collider2D (giữ quyết
   định từ lần sửa trước, không liên quan bug lần này).
3. Viết lại `FloorRenderController.Refresh()` đọc field mới.
4. Sửa `PlacementModeController` nếu có gọi API cũ (`SetFloor` đã đổi tên `TeleportToFloor` từ
   trước, không đụng gì thêm — kiểm tra lại cho chắc).
5. Verify: compile → 220 EditMode test → build Windows → smoke test headless.
6. Cập nhật `isometric-game-placement-rules.md` mục 5-6, `BACKLOG.md`/`CODEMAP.md` (đây là lần
   sửa cầu thang thứ 5 — ghi rõ để không lặp lại nhầm lẫn).

## Người dùng cần test lại

Đi cả 2 chiều nhiều lần, đặc biệt: đi hết lên rồi đứng yên xem tầng dưới có giữ mờ không; đi
vào từ đỉnh cầu thang xem có mờ dần đúng hướng không; đi nửa chừng rồi quay đầu ở nhiều vị trí
khác nhau (không chỉ giữa vùng) xem có nhất quán không.
