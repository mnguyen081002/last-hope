# Isometric Game Placement Rules

> Dành cho Unity 2D isometric, kiểu Project Zomboid — sprite phẳng, camera không xoay.
> Mục tiêu: AI phải đặt công trình, cửa, cầu thang và vật phẩm dễ nhìn và không chặn gameplay.

## 0. Thực tế triển khai hiện tại — đọc trước khi áp dụng phần còn lại

`technical-specification.md` mô tả kiến trúc dự định dùng Unity Tilemap
(`Grid.CellLayout.Isometric`, `TilemapCollider2D`). **Code thực tế không dùng Tilemap ở bất
kỳ đâu** — `SceneSetup.cs` (nơi duy nhất dựng scene) dùng:

```text
Ground   : 1 SpriteRenderer duy nhất, drawMode Tiled, phủ toàn bộ vùng chơi được (BuildGround)
Boundary : 4 BoxCollider2D không renderer quanh biên (BuildBoundary/AddWall)
Prop     : GameObject world position tự do (Vector2 chọn tay) + SpriteRenderer + BoxCollider2D
           nhỏ làm chân đế (BuildWorldProp) — KHÔNG snap theo lưới ô
Pathfinding : chưa có (kể cả flood-fill/A*) — cắt phạm vi có ghi rõ trong
           technical-specification.md dòng 139, kiểm tra path bằng mắt khi dựng scene
Placement   : đặt tay trong SceneSetup.cs, không có pipeline tự động
           (CandidateGenerator/PlacementScorer ở mục 10 dưới đây là kiến trúc dự định, chưa
           triển khai)
```

Các mục bên dưới đã viết lại theo đúng cách triển khai thật ở trên (world position tự do,
`Collider2D` thường, `SetActive` cho floor toggle) — không còn giả định có Tilemap. Không tự
ý dựng Tilemap thật để "làm đúng theo tinh thần isometric" — sẽ lệch khỏi mọi scene hiện có.

---

## 1. Nguyên tắc bắt buộc

Một object chỉ được xem là đặt đúng khi:

```text
Nằm trong vùng Ground (sprite "Ground", kiểm bounds)
+ không chồng collider2D
+ có đường đi
+ tương tác được
+ dễ nhận biết từ camera gameplay (qua Y-sort/sorting layer đúng)
```

AI **MUST NOT**:

- Đặt object đè lên collider khác hoặc ngoài vùng Ground mà không kiểm tra bằng mắt/Scene View.
- Chỉ kiểm tra collider2D overlap mà bỏ qua navigation.
- Đặt decoration trước đường đi gameplay.
- Báo hoàn thành khi chưa kiểm tra visibility (sorting) và navigation.

---

## 2. Grid, anchor và socket (diễn giải theo world position tự do — xem mục 0)

Object gameplay **MUST** được đặt theo một trong ba vai trò sau (không có lưới thật để snap
vào, nhưng vẫn phải khai báo rõ vai trò và các thuộc tính đi kèm):

```text
Grid   : công trình, tường, nội thất lớn (chiếm vùng world lớn, vd Boundary)
Anchor : cửa, cầu thang, điểm chuyển tầng — 1 vị trí world cụ thể, cố định
Socket : vật trên bàn, kệ, tường hoặc máy móc — offset cố định so với object cha
```

Mỗi object **MUST** khai báo:

```text
footprint (kích thước world thực chiếm — bán kính/box collider)
allowed rotations (thường bỏ qua ở MVP — sprite luôn nhìn 1 hướng cố định)
placement type (Grid/Anchor/Socket ở trên)
clearance (khoảng trống world quanh object, đủ cho InteractionDetector + đường đi)
interaction point (vị trí IInteractable, nằm trong bán kính OverlapCircle của InteractionDetector)
sorting layer / order-in-layer
```

AI **MUST NOT** đặt cửa xuyên vào tường hoặc đặt vật chồng lên collider khác mà không kiểm
tra lại trong Scene View.

---

## 3. Camera readability (Y-sort)

Camera 2D isometric **không xoay** — luôn nhìn thẳng, cố định. "Dễ nhận biết" không còn phụ thuộc góc quay camera mà phụ thuộc **thứ tự vẽ (sort order)**.

Object **MUST**:

- Sort đúng theo `Camera.transparencySortMode = CustomAxis` (trục sort khớp tỉ lệ chiếu iso của tile) — object ở vị trí "gần" (Y nhỏ hơn theo world) phải vẽ đè lên object "xa".
- Có sprite silhouette đủ khác biệt với nền/tường xung quanh (không dùng chung 1 màu/texture với sàn).
- Không bị vật khác cùng sorting layer đè nhầm do pivot/order-in-layer sai.
- Xuất hiện đúng lớp khi người chơi tới gần (không lẫn giữa tầng dưới/tầng trên).

AI **MUST NOT** hard-code order-in-layer cố định cho object di chuyển được (player, NPC) — order phải tính động theo vị trí Y mỗi frame.

---

## 4. Cửa và lối vào

Game hiện tại chưa có cửa vật lý riêng (mở/đóng) — location chuyển cảnh qua `TravelPointView`
(đổi scene) hoặc chuyển tầng qua Anchor loại staircase (mục 5). Nếu sau này dựng cửa thật
trong một scene (không đổi scene/tầng, chỉ chặn/mở lối), áp dụng:

```text
InsideApproachPoint (world position)
OutsideApproachPoint (world position)
InteractionPoint
Sorting layer riêng biệt so với tường (để không bị tường đè sai lúc mở)
FrontClearance / BackClearance (world position trống, đủ bán kính InteractionDetector)
```

Cửa chỉ hợp lệ khi:

- Nằm trong vùng Ground ở cả hai phía.
- Player đi tới được (không bị `Collider2D` khác chặn vị trí approach).
- Không bị vật cản chặn (collider2D khác đè lên vị trí cửa).
- Cánh cửa mở không xuyên collider2D khác.
- Cửa mở không khóa kín lối đi duy nhất (kiểm tra bằng mắt — chưa có pathfinding tự động).
- Người chơi nhận biết được cửa từ hướng tiếp cận (silhouette/màu khác tường).

Cửa quan trọng **SHOULD** có ít nhất hai tín hiệu:

- Khung cửa rõ trên sprite.
- Màu hoặc sprite khác tường.
- Interaction prompt hiện khi trong tầm (đã có `InteractionDetector`/`IInteractable`).
- Outline hoặc highlight khi player ở gần.

---

## 5. Cầu thang và chuyển tầng

2D isometric không có độ dốc vật lý — nhưng đổi tầng **là leo dần liên tục qua một vùng cầu
thang duy nhất, không phải một điểm bấm phím hay một đường kẻ đổi tức thời**, kiểu Z-level
Project Zomboid (game tham chiếu chính của doc này): tiến độ leo nhích theo đúng vị trí Y thật
của player trong vùng (đi lùi thì tiến độ tụt lại), chỉ đổi tầng thật (đổi Collider2D/tương
tác) khi tiến độ chạm đỉnh. **Không** dùng `IInteractable`/`InteractionDetector` cho cầu
thang — đây là ngoại lệ có chủ đích duy nhất không dùng phím tương tác, vì đổi tầng là di
chuyển thuần tuý (không mở UI, không tốn thời gian game, không có gì cần xác nhận) — khác về
bản chất với Search/Storage/Travel/ShelterConsole (đều có hệ quả thật, xứng đáng cần bấm phím
để tránh nhầm).

**Không dùng `Collider2D`/`OnTrigger*2D` cho logic phát hiện player trong vùng** — dù đây là
lựa chọn "chuẩn Unity" trực giác nhất, thực tế gặp lỗi khó chẩn đoán (không lên được cầu
thang, không rõ Enter/Stay có bắn đủ hay không do phụ thuộc lịch physics step). Dùng khoảng
cách thuần (so vị trí X/Y player với vùng) trong `MonoBehaviour.Update()` — chạy chắc chắn
mỗi frame, không phụ thuộc physics engine, dễ suy luận và debug hơn cho một logic thuần vị trí
không cần lực/va chạm thật.

Cầu thang **MUST** có:

```text
1 vùng hình chữ nhật duy nhất (world position, KHÔNG cần Collider2D) trải dài từ bottomY (tầng
  dưới) tới topY (tầng trên) — KHÔNG đặt trong GameObject root của Ground/Upper Floor (nếu có
  Collider2D thì sẽ bị tắt khi tầng tương ứng Dimmed/Hidden — cầu thang phải luôn phát hiện
  được player từ cả hai phía, không phụ thuộc tầng nào)
Component (StaircaseZone) mỗi Update() so sánh vị trí X/Y player với vùng — tính tiến độ =
  InverseLerp(bottomY, topY, vị trí Y player), publish qua PlayerFloorState.SetClimbProgress —
  không đổi tầng thật cho tới khi tiến độ chạm 1; rời vùng giữa chừng phải huỷ, giữ nguyên tầng cũ
```

Cầu thang chỉ hợp lệ khi:

- Vùng nằm trong vùng Ground của cả hai tầng liên quan (bottomY/topY hợp lý).
- Không dẫn ra ngoài level bounds.
- Đi ngược giữa chừng (chưa chạm đỉnh) phải huỷ leo, không kẹt nửa vời.
- Có visual rõ ràng đây là khu vực cầu thang, phủ đúng kích thước cả vùng (không phải icon
  nhỏ như prop thường) — không cần prompt tương tác vì không bấm phím.

---

## 6. Visibility và tầng (Z-level kiểu Project Zomboid)

Critical object gồm:

```text
cửa chính
lối ra
cầu thang / điểm chuyển tầng
objective bắt buộc
```

Critical object **MUST** sort đúng lớp và không bị object cùng ô che khuất hoàn toàn theo order-in-layer.

Đa tầng (Ground/Upper...) xử lý bằng **Z-level**: mỗi GameObject root của một tầng gắn
`FloorLevel(floor: int)`. `FloorRenderController` đọc tầng hiện tại của player
(`PlayerFloorState`) và áp cho từng `FloorLevel` theo hiệu số:

```text
diff = floor_của_root - floor_hiện_tại_player

diff == 0  (tầng hiện tại) : Full     — alpha 1, Collider2D bật, sortingOrder gốc
diff == -1 (tầng ngay dưới): Dimmed   — alpha thấp (~0.35), Collider2D tắt (đi xuyên, không va
                                        chạm/tương tác), sortingOrder đẩy xuống dưới hẳn tầng
                                        hiện tại (không dùng Y-sort thường vì 2 tầng cùng
                                        world position, sẽ lẫn lộn nếu chỉ dựa Y)
diff > 0 hoặc diff < -1    : Hidden   — SetActive(false) hoàn toàn (không thấy tầng trên đầu,
                                        không thấy tầng xa hơn 1 tầng dưới)
```

**Trong lúc đang leo cầu thang** (`PlayerFloorState.TransitioningToFloor` khác null): hai tầng
liên quan (tầng đang rời và tầng đang tới) KHÔNG áp công thức nhị phân trên — alpha nội suy
liên tục theo `ClimbProgress` (0→1): tầng đang rời mờ dần từ 1 xuống Dimmed, tầng đang tới rõ
dần từ Dimmed lên 1. Va chạm (Collider2D) đổi tại đúng mốc `ClimbProgress == 0.5`. Đây là cách
tạo cảm giác "đang leo dần" thay vì đổi tầng tức thời ở một điểm.

Đây **là** dạng "wall fade" có chủ đích (khác quy tắc "KHÔNG dùng wall fade" ở các mục khác
— ngoại lệ riêng cho floor-below, vì đây chính là cách Project Zomboid tạo cảm giác đứng trên
tầng thật: thấy mờ mờ bố cục tầng dưới qua sàn, không phải hoàn toàn tách biệt hai không gian).

AI **MUST NOT** để tầng trên hiện khi đứng dưới tầng đó (không thể nhìn xuyên mái) hoặc để 2
tầng cùng có Collider2D bật (gây va chạm/tương tác nhầm giữa 2 tầng).

---

## 7. Navigation và interaction

Chưa có pathfinding tự động (flood-fill/A*) — cắt phạm vi có ghi rõ trong
`technical-specification.md` dòng 139. Sau mỗi object có thể chặn đường, **kiểm tra bằng mắt
trong Scene View** (đứng ở PlayerSpawn, dò mắt tới từng prop tương tác):

```text
PlayerSpawn -> Interactable chính (Storage/Console/TravelPoint/SearchPoint...)
PlayerSpawn -> Staircase (nếu có tầng trên)
BottomAccessPoint -> TopAccessPoint (sau khi đổi tầng)
```

Placement bị từ chối nếu:

- Không còn đường đi rõ ràng bằng mắt (object mới chặn hết lối duy nhất).
- Approach point nằm đè lên collider khác.
- Interaction point nằm ngoài bán kính `InteractionDetector` (`Physics2D.OverlapCircleNonAlloc`).
- Cửa hoặc nội thất chặn đường bắt buộc.

---

## 8. Ground và level bounds

Mọi vị trí player có thể đi tới **MUST** có:

```text
Nằm trong bounds sprite "Ground" (SpriteRenderer Tiled, xem BuildGround trong SceneSetup.cs)
Không bị Collider2D nào chặn tại đúng vị trí đó
Approach point hợp lệ
Khoảng an toàn quanh mép map
```

Level bounds bao bọc bằng 4 `BoxCollider2D` không renderer quanh biên (`BuildBoundary`/
`AddWall` trong `SceneSetup.cs`). Không có trục trọng lực nên không có khái niệm "rơi khỏi
map", nhưng vẫn phải chặn player đi ra ngoài footprint thiết kế.

Ground phải bao phủ:

```text
walkable area
+ object footprint
+ approach point
+ staircase (cả 2 đầu, nếu có tầng trên)
+ safety margin quanh biên
```

---

## 9. Thứ tự dựng level

AI **MUST** dựng theo thứ tự (khớp thứ tự gọi hàm trong `SceneSetup.cs`):

```text
1. Level bounds (BuildBoundary — 4 BoxCollider2D)
2. Ground sprite (BuildGround — SpriteRenderer Tiled)
3. Đường đi chính (chừa khoảng trống khi đặt prop ở bước sau)
4. Cửa, cầu thang/điểm chuyển tầng (nếu đa tầng — xem mục 5)
5. Sorting layer / Y-sort setup
6. Z-level (nếu đa tầng — FloorLevel + FloorRenderController, xem mục 6)
7. Object tương tác (BuildStorage/BuildSearchPoint/BuildTravelPoint/...)
8. Nội thất lớn
9. Decoration
```

Sau khi đặt nội thất lớn hoặc decoration, phải kiểm tra lại:

```text
sort order đúng
navigation (path còn thông, kiểm bằng mắt)
interaction access
```

---

## 10. Placement pipeline

Chưa triển khai — hiện tại đặt bằng world position chọn tay khi viết `SceneSetup.cs`, không
có pipeline tự động sinh/chấm điểm candidate. Nếu về sau cần tự động hoá (nhiều content hơn,
dễ đặt lệch), quy trình tham khảo:

```text
PlacementRequest
    -> CandidateGenerator (theo vị trí world rời rạc, không phải ô tile)
    -> GroundValidator (trong bounds sprite Ground)
    -> CollisionValidator (Collider2D)
    -> PathValidator (kiểm tra bằng mắt tới khi có pathfinding thật)
    -> SortOrderValidator
    -> InteractionValidator
    -> PlacementScorer
    -> PrefabSpawner (SpriteRenderer + Collider2D)
```

Tới khi có pipeline: AI **MUST** tự kiểm tra checklist mục 12 bằng mắt trước khi báo hoàn
thành, không giả định "đặt trong SceneSetup.cs là tự động đúng".

---

## 11. Quy tắc dành cho AI coding agent

```text
1. Kiểm tra gameplay camera (không xoay), level bounds, ground sprite và navigation trước khi sửa scene.
2. Đặt object bằng world position tự do (không có Tilemap để snap) — nhưng phải nằm trong bounds Ground và khai báo rõ Grid/Anchor/Socket (mục 2).
3. Dùng grid, anchor hoặc socket.
4. Dựng đường đi trước decoration.
5. Sort order tính động theo vị trí Y cho object di chuyển được — không hard-code.
6. Cửa, cầu thang và lối vào phải dễ nhận biết qua sorting/silhouette, không phụ thuộc góc camera.
7. Kiểm tra cả hai phía cửa và cả hai đầu cầu thang/điểm chuyển tầng.
8. Đa tầng dùng Z-level (`FloorLevel` + `FloorRenderController`): tầng hiện tại Full, tầng ngay dưới Dimmed (mờ, không va chạm), còn lại Hidden — xem mục 6.
9. Kiểm tra lại sort order sau khi đặt nội thất và decoration.
10. Cầu thang/điểm chuyển tầng dùng vùng trigger đi-qua-là-đổi-tầng (`Collider2D.isTrigger` + `OnTriggerEnter2D`) — **ngoại lệ duy nhất** không dùng `IInteractable`, vì đổi tầng là di chuyển thuần tuý không có hệ quả cần xác nhận (mục 5).
11. Reject object không có path (kiểm bằng mắt), sort sai lớp hoặc không tương tác được.
12. Không báo hoàn thành trước khi các kiểm tra bắt buộc pass.
```

---

## 12. Definition of Done

Một critical object chỉ hoàn thành khi:

```text
[ ] Nằm trong level bounds, trong vùng Ground
[ ] Không chồng Collider2D
[ ] Footprint đúng (bán kính/box collider hợp lý)
[ ] Có path hợp lệ (kiểm bằng mắt)
[ ] Approach point tiếp cận được
[ ] Interaction hoạt động (trong bán kính InteractionDetector, qua IInteractable — trừ cầu thang dùng trigger, xem mục 5)
[ ] Sort order đúng, không bị object khác đè sai lớp
[ ] Z-level đúng nếu thuộc tầng trên/dưới (Full/Dimmed/Hidden qua FloorRenderController, không phải chỉ SetActive nhị phân)
[ ] Không bị decoration chặn
```

Quy tắc kết luận:

```text
Đúng vị trí (trong Ground, không chồng collider) nhưng sort sai lớp = sai.
Sort đúng nhưng không tiếp cận được = sai.
Tiếp cận được nhưng phá đường đi = sai.
```
