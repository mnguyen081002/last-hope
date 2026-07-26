# Isometric Game Placement Rules

> Dành cho Unity 2D isometric (Tilemap), kiểu Project Zomboid — sprite phẳng trên lưới iso, camera không xoay.
> Mục tiêu: AI phải đặt công trình, cửa, cầu thang và vật phẩm đúng ô tile, dễ nhìn và không chặn gameplay.

## 1. Nguyên tắc bắt buộc

Một object chỉ được xem là đặt đúng khi:

```text
Đúng ô lưới
+ có tile sàn hỗ trợ
+ không chồng collider2D
+ có đường đi
+ tương tác được
+ dễ nhận biết từ camera gameplay (qua Y-sort/sorting layer đúng)
```

AI **MUST NOT**:

- Đặt object bằng world position đoán, không snap theo lưới tile.
- Chỉ kiểm tra trong Scene View.
- Chỉ kiểm tra collider2D overlap.
- Đặt decoration trước đường đi gameplay.
- Báo hoàn thành khi chưa kiểm tra visibility (sorting) và navigation.

---

## 2. Grid, anchor và socket

Object gameplay **MUST** được đặt bằng một trong ba cách, luôn snap theo `Grid.CellLayout.Isometric`:

```text
Grid   : công trình, tường, nội thất lớn (chiếm N×M ô tile)
Anchor : cửa, cầu thang, điểm chuyển tầng — 1 ô tile cụ thể
Socket : vật trên bàn, kệ, tường hoặc máy móc — offset cố định trong ô cha
```

Mỗi object **MUST** khai báo:

```text
footprint (số ô tile chiếm, không phải kích thước world tuyệt đối)
allowed rotations (thường chỉ 0°/90°/180°/270° trên lưới iso)
placement type
clearance (số ô trống xung quanh)
interaction point
sorting layer / order-in-layer
```

AI **MUST NOT** đặt cửa xuyên vào tường hoặc đặt vật nhỏ bằng world position gần đúng không snap theo ô.

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

Cửa gameplay **MUST** có:

```text
InsideApproachPoint (ô tile)
OutsideApproachPoint (ô tile)
InteractionPoint
Sorting layer riêng biệt so với tường (để không bị tường đè sai lúc mở)
FrontClearance / BackClearance (ô tile trống)
```

Cửa chỉ hợp lệ khi:

- Có tile sàn ở cả hai phía.
- Player đi tới được (không bị `TilemapCollider2D` chặn ô approach).
- Không bị vật cản chặn (collider2D khác đè lên ô cửa).
- Cánh cửa mở không xuyên collider2D khác.
- Cửa mở không khóa kín lối đi (grid pathfinding vẫn có đường qua).
- Người chơi nhận biết được cửa từ hướng tiếp cận (silhouette/màu khác tường).

Cửa quan trọng **SHOULD** có ít nhất hai tín hiệu:

- Khung cửa rõ trên sprite.
- Màu hoặc sprite khác tường.
- Interaction prompt hiện khi trong tầm (đã có `InteractionDetector`/`IInteractable`).
- Outline hoặc highlight khi player ở gần.

---

## 5. Cầu thang và chuyển tầng

2D isometric không có độ dốc vật lý — cầu thang là **1 tile trigger** đổi tầng hiển thị tức thời, không di chuyển liên tục theo cao độ.

Cầu thang **MUST** có:

```text
BottomAccessPoint (ô tile, tầng dưới)
TopAccessPoint (ô tile, tầng trên)
TriggerCollider2D (kích hoạt đổi tầng khi player đi vào ô)
FloorSwitchTarget (floor index / sorting layer nhóm tầng trên bật/tắt)
```

Cầu thang chỉ hợp lệ khi:

- Cả hai đầu có tile sàn hỗ trợ đúng tầng tương ứng.
- Player và NPC (khi có visual) đi qua được.
- Không dẫn ra ngoài level bounds.
- Đổi tầng đúng: object/zone của tầng cũ ẩn, tầng mới hiện (không hiện chồng cả hai tầng cùng lúc).
- Có visual/label rõ ràng đây là điểm chuyển tầng (không lẫn với tile sàn thường).

---

## 6. Visibility và tầng

Critical object gồm:

```text
cửa chính
lối ra
cầu thang / điểm chuyển tầng
objective bắt buộc
```

Critical object **MUST** sort đúng lớp và không bị object cùng ô che khuất hoàn toàn theo order-in-layer.

Đa tầng (Ground/Upper, theo `ShelterZoneDefinition.Floor` đã có sẵn trong Data) xử lý bằng:

```text
Floor visibility toggle: chỉ tầng hiện tại của player active/hiện, tầng khác ẩn hoặc mờ
```

**KHÔNG** dùng raycast occlusion, wall fade hay roof hide — camera 2D không có khái niệm "vật cản giữa camera và object", chỉ có sort order và floor toggle.

AI **MUST NOT** để 2 tầng cùng hiện đầy đủ cùng lúc gây rối mắt (trừ khi chủ đích thiết kế "nhìn xuyên tầng" — phải khai báo rõ).

---

## 7. Navigation và interaction

Sau mỗi object có thể chặn đường, hệ thống **MUST** kiểm tra:

```text
PlayerSpawn -> MainEntrance
MainEntrance -> RoomEntrances
MainEntrance -> StairTile
StairTile (dưới) -> StairTile (trên)
RoomEntrance -> CriticalObjects
```

Placement bị từ chối nếu:

- Không còn path (kiểm bằng flood-fill/grid traversal trên các ô không bị `TilemapCollider2D`/`Collider2D` chặn).
- Lối đi hẹp hơn 1 ô tile.
- Approach point nằm trên ô bị chặn.
- Interaction point nằm ngoài bán kính `InteractionDetector` (`Physics2D.OverlapCircleNonAlloc`).
- Cửa hoặc nội thất chặn đường bắt buộc.

---

## 8. Ground và level bounds

Mọi ô tile player có thể đi tới **MUST** có:

```text
Tile sàn (Tilemap layer Ground)
Không bị TilemapCollider2D chặn tại chính ô đó
Approach point hợp lệ
Khoảng an toàn quanh mép map
```

Level bounds bao bọc bằng `TilemapCollider2D`/`CompositeCollider2D` (hoặc `EdgeCollider2D`/`BoxCollider2D` không renderer cho tường biên). Không có trục trọng lực nên không có khái niệm "rơi khỏi map", nhưng vẫn phải chặn player đi ra ngoài footprint thiết kế.

Ground phải bao phủ:

```text
walkable area
+ object footprint
+ approach point
+ stair tile (cả 2 đầu)
+ safety margin quanh biên
```

---

## 9. Thứ tự dựng level

AI **MUST** dựng theo thứ tự:

```text
1. Level bounds (Tilemap + boundary collider2D)
2. Ground tile (Tilemap Isometric layer)
3. Đường đi chính
4. Phòng và tường (tile/sprite tường + Collider2D)
5. Cửa, cầu thang/điểm chuyển tầng
6. Sorting layer / Y-sort setup
7. Floor visibility toggle (đa tầng)
8. Object tương tác
9. Nội thất lớn
10. Decoration
```

Sau khi đặt nội thất lớn hoặc decoration, phải kiểm tra lại:

```text
sort order đúng
navigation (path còn thông)
interaction access
```

---

## 10. Placement pipeline

Mọi object gameplay **MUST** đi qua:

```text
PlacementRequest
    -> CandidateGenerator (theo ô tile)
    -> GroundValidator
    -> CollisionValidator (Collider2D)
    -> PathValidator (grid traversal)
    -> SortOrderValidator
    -> InteractionValidator
    -> PlacementScorer
    -> PrefabSpawner (SpriteRenderer + Collider2D)
```

AI **MUST** chọn candidate hợp lệ có điểm cao nhất.

AI **MUST NOT** spawn critical object nếu không có candidate hợp lệ.

---

## 11. Quy tắc dành cho AI coding agent

```text
1. Kiểm tra gameplay camera (không xoay), level bounds, ground tile và navigation trước khi sửa scene.
2. Không đặt object bằng world position tùy ý — luôn snap theo Grid.CellLayout.Isometric.
3. Dùng grid, anchor hoặc socket.
4. Dựng đường đi trước decoration.
5. Sort order tính động theo vị trí Y cho object di chuyển được — không hard-code.
6. Cửa, cầu thang và lối vào phải dễ nhận biết qua sorting/silhouette, không phụ thuộc góc camera.
7. Kiểm tra cả hai phía cửa và cả hai đầu cầu thang/điểm chuyển tầng.
8. Đa tầng dùng floor visibility toggle, không dùng occlusion.
9. Kiểm tra lại sort order sau khi đặt nội thất và decoration.
10. Reject object không có path, sort sai lớp hoặc không tương tác được.
11. Không báo hoàn thành trước khi các kiểm tra bắt buộc pass.
```

---

## 12. Definition of Done

Một critical object chỉ hoàn thành khi:

```text
[ ] Nằm trong level bounds, snap đúng ô tile
[ ] Có tile sàn hỗ trợ
[ ] Không chồng Collider2D
[ ] Footprint và rotation đúng
[ ] Có path hợp lệ
[ ] Approach point tiếp cận được
[ ] Interaction hoạt động (trong bán kính InteractionDetector)
[ ] Sort order đúng, không bị object khác đè sai lớp
[ ] Floor visibility toggle đúng nếu thuộc tầng trên
[ ] Không bị decoration chặn
```

Quy tắc kết luận:

```text
Đúng ô lưới nhưng sort sai lớp = sai.
Sort đúng nhưng không tiếp cận được = sai.
Tiếp cận được nhưng phá đường đi = sai.
```
