# Module Production & Placement Loop — Thiết kế Gameplay

Vai trò: Senior Gameplay Designer. Đây là **thiết kế**, không phải implementation plan — không có
pseudocode/API. Xác nhận với user (2026-07-30): tài liệu này **thay thế** flow Free Placement hiện
tại của `BL-P3-03` (đặt vị trí trước → chờ xây tại chỗ), chuyển sang **Production tách rời khỏi vị
trí đặt**, cộng thêm **Snap Grid** và **Rotate**.

**Quyết định chốt (2026-07-30, user trả lời trực tiếp mục 7 cũ)** — toàn bộ câu hỏi mở đã được giải
quyết, tài liệu dưới đây phản ánh các quyết định này, không còn phương án lửng lơ:

1. Claim cộng packed item vào **túi Player** (không phải Storage).
2. Cell size lưới: đề xuất cụ thể **1.0 world unit**, xem mục 3.4.
3. Rotate **có** đổi sprite theo hướng — user tự tạo art, xem mục 3.6.
4. Multi-slot Production queue: **để P4**, không làm ở lần triển khai đầu tiên.
5. "Ready to Claim" là **trạng thái tách biệt hoàn toàn khỏi Inventory** — không phải packed item
   nằm sẵn trong túi chờ xác nhận.
6. Tháo Module đang bị Event tác động: **không chặn**, tự động clear Event liên quan tại thời điểm
   Tháo — xem mục 4 (Remove).

## 0. Đối chiếu với hệ thống đang chạy (bắt buộc đọc trước khi implement)

Hiện trạng (`docs/backlog/CODEMAP.md`, `BuildSystem.cs`, `PlacementModeController.cs`):

- **Thứ tự hiện tại**: chọn Zone + Module trong `ShelterPanel` → vào Placement Mode → ghost tại vị
  trí world tự do → click xác nhận → `StartConstructionCommand` bắt đầu **tại đúng vị trí đó** →
  chờ `BuildMinutes` → `ConstructionCompleted` tự sinh `BuiltModuleState` thẳng vào thế giới. Không
  có bước Claim, không có "Module trong túi".
- **Đã có sẵn và tái dùng được**: `ModuleDefinition.PackedItemId` (5 item `item_packed_*`) — hiện
  chỉ sinh ra khi **Tháo** một Module đã xây, đặt lại tức thì qua `RedeployModule`/`CanRedeployAt`
  (không tốn Materials/BuildMinutes). Cơ chế "vật phẩm gói → đặt tức thì từ Inventory" **đã tồn
  tại**, chỉ chưa dùng cho đường sản xuất mới.
- **Free Placement là quyết định có chủ đích** (2026-07-28): world position liên tục, không lưới,
  không Slot cố định. Tài liệu này **đảo ngược một phần** quyết định đó bằng Snap Grid — không bỏ
  world-free, mà thêm bước quantize logic trước khi validate (mục 3.4).
- **Rotate đã bị cắt khỏi scope MVP** trước đây (`isometric-game-placement-rules.md` mục 2: "allowed
  rotations thường bỏ qua ở MVP — sprite luôn nhìn 1 hướng cố định"). Thêm lại rotate kéo theo một
  vấn đề dữ liệu quan trọng: `ModuleDefinition.FootprintRadius` là **bán kính hình tròn** — xoay một
  hình tròn không đổi gì. Rotate chỉ có ý nghĩa nếu đổi sang footprint dạng hộp (Width × Height).
  Đây là thay đổi model dữ liệu, không phải chi tiết vặt — nêu rõ ở mục 5.

---

## 1. Nguyên tắc thiết kế

1. **Production tách rời hoàn toàn khỏi vị trí đặt.** Sản xuất tại Shelter Console không cần chọn
   Zone/vị trí trước — chỉ chọn Module muốn làm.
2. **Thời gian chờ nằm ở Production, không nằm ở Placement.** Đặt từ Inventory ra thế giới là hành
   động tức thời (giống Redeploy hiện tại) — vì Materials + BuildMinutes đã trả xong lúc sản xuất.
3. **Claim là hành động chủ động của người chơi**, không tự động cộng vào Inventory khi hết giờ —
   đúng yêu cầu gốc, tạo một nhịp quay lại Shelter kiểm tra.
4. **Snap Grid là lưới logic ẩn, không phải Tilemap.** Không vẽ lưới toàn bản đồ, không đổi cách
   render Ground (`SpriteRenderer` Tiled hiện tại giữ nguyên) — lưới chỉ dùng để quantize toạ độ
   trước khi kiểm tra hợp lệ, hiển thị mờ trong lúc Placement Mode đang mở.
5. **Rotate là rời rạc 90°/lần**, đổi hitbox, hướng tương tác **và sprite** — user sẽ tự tạo art theo
   hướng, xem mục 3.6.
6. Giữ nguyên các quyết định đơn giản hoá đã có: Tháo/Đặt lại reset Durability về 100%, chỉ 1 Main
   Shelter, chỉ 1 tác vụ sản xuất chạy cùng lúc trừ khi đổi ở mục 6.

---

## 2. Gameplay Loop tổng thể

```text
                    ┌─────────────────────┐
                    │  Shelter Console     │
                    │  chọn Module muốn    │
                    │  sản xuất            │
                    └──────────┬───────────┘
                               │ đủ vật liệu? (trừ ngay)
                               ▼
                    ┌─────────────────────┐
                    │  PRODUCTION          │  Passive Task, tick ShortTick,
                    │  (đang sản xuất)     │  chạy dù rời Shelter/Sleep
                    └──────────┬───────────┘
                               │ hết BuildMinutes
                               ▼
                    ┌─────────────────────┐
                    │  READY TO CLAIM      │  hiện badge/thông báo tại Console
                    └──────────┬───────────┘
                               │ player bấm "Nhận" tại Console
                               ▼
                    ┌─────────────────────┐
                    │  INVENTORY           │  packed item (PackedItemId),
                    │  (đã claim)          │  stack như item thường
                    └──────────┬───────────┘
                               │ chọn item, bấm "Đặt"
                               ▼
                    ┌─────────────────────┐
        ┌──────────►│  PLACEMENT MODE      │◄──────────┐
        │ ESC/huỷ   │  ghost + snap grid   │  rotate(R) │
        │           │  + validate          │  (vòng lại)│
        └───────────┴──────────┬───────────┴────────────┘
                               │ click trái, vị trí hợp lệ
                               ▼
                    ┌─────────────────────┐
                    │  PLACED IN WORLD     │  trừ 1 packed item khỏi Inventory
                    └──────────┬───────────┘
                               │ hover-menu → "Tháo"
                               ▼
                    ┌─────────────────────┐
                    │  INVENTORY           │  quay lại vòng "chọn item, bấm Đặt"
                    │  (đã tháo)           │
                    └─────────────────────┘
```

---

## 3. Chi tiết từng bước

### 3.1 Production

- Trigger: tại Shelter Console — giữ nguyên nguyên tắc "mọi tương tác Shelter qua 1 prop", không
  thêm điểm tương tác mới.
- Điều kiện bắt đầu: đủ Materials, trừ ngay lúc bắt đầu (giữ `HasEnoughMaterials` hiện có).
- **Không còn cần chọn Zone/vị trí** để bắt đầu — khác biệt cốt lõi so với hiện tại.
- Song song: MVP đề xuất giữ **1 slot Production cùng lúc** (giữ nguyên constraint hiện tại). Đây
  là điểm dễ nới rộng sau — xem mục 6.
- Hoàn thành: chuyển trạng thái "Ready to Claim", **không** tự thêm vào Inventory.

### 3.2 Claim

- Điều kiện: đứng tại Shelter Console, có ≥1 sản phẩm Ready to Claim.
- Hành động: nút "Nhận" trong `ShelterPanel` → cộng 1 `PackedItemId` vào **túi Player** (quyết định
  chốt — không phải Storage). Dùng `InventorySystem.CanAdd/Add` sẵn có, chịu overload/hard cap như
  mọi item khác — không có đường tắt bỏ qua giới hạn tải trọng.
- "Ready to Claim" là trạng thái riêng, **không phải packed item nằm sẵn trong túi chờ xác nhận** —
  packed item chỉ thực sự tồn tại (và tính vào tải trọng/Inventory) sau khi bấm "Nhận" (xem mục 5,
  quyết định #5).
- Nhiều sản phẩm Ready cùng lúc (nếu mục 6 mở rộng multi-queue ở P4): claim từng cái hoặc "Nhận tất
  cả".

### 3.3 Từ Inventory chọn Place

- Danh sách packed item hiện trong Storage/Inventory panel, mỗi item có nút "Đặt".
- Bấm "Đặt" → publish `BeginPlacementMode` (event đã có sẵn) → vào Placement Mode.
- **Không tốn thêm Materials/BuildMinutes** — đã trả hết lúc Production. Về bản chất giống hệt
  luồng Redeploy hiện tại, chỉ khác nguồn gốc packed item (từ Production thay vì từ Tháo).

### 3.4 Ghost Preview + Snap Grid

- Ghost bám chuột, đổi màu xanh/đỏ theo hợp lệ, khung mờ biên Zone — giữ nguyên UX hiện có.
- **Snap Grid**: toạ độ chuột được quantize về tâm ô lưới gần nhất *trước khi* gọi validate. Lưới là
  khái niệm logic, không phải Tilemap — không đổi cách render Ground.
- **Cell size đề xuất: 1.0 world unit.** Căn cứ: `ModuleDefinition.FootprintRadius` mặc định hiện
  tại là 0.5 (đường kính 1 unit) — cả 5 Module trong `modules_p3.json` đều dùng giá trị mặc định này,
  nên 1 unit khớp đúng kích thước hiện có, không cần đổi lại các Module đã cân bằng. Zone bounds hiện
  tại rộng khoảng 7–10 × 7–8 unit → lưới 1 unit cho ra ~50–80 ô mỗi Zone, đủ tự do để không cảm giác
  gò bó như Slot cố định cũ, nhưng đủ thô để các Module thẳng hàng nhìn gọn. Đặt field cấu hình
  `shelter.build_grid_cell_size` trong `balance.json` (không hard-code), mặc định 1.0 — số này vẫn
  nên xác nhận lại bằng playtest thật, không phải con số cuối cùng bất biến.
- Overlay lưới: chỉ hiện trong Zone đang chọn, chỉ khi Placement Mode đang mở (giữ tinh thần
  world-free ở phần render — lưới là công cụ đặt, không phải bản chất thế giới).

### 3.5 Validation

Giữ nguyên các điều kiện đã có (`CanPlaceGeometry`): trong bounds Zone + không chồng Module khác +
không chồng Fixed Core Component. Thêm:

- Vị trí đã snap phải **vẫn còn trong bounds Zone** sau khi quantize (ô gần biên có thể snap ra
  ngoài — phải re-clamp hoặc reject, không tự động kéo vào trong).
- Overlap check phải dùng **hình chữ nhật đã xoay** (không còn là hình tròn — xem mục 5).

### 3.6 Rotate

- Phím tắt (R) hoặc scroll khi đang ghost, xoay 90°/lần, không giới hạn góc tự do.
- Re-validate ngay sau mỗi lần xoay — bounding box đổi trục dài/ngắn (vd Module 2×1 xoay thành 1×2)
  có thể làm vị trí đang hợp lệ trở thành overlap, hoặc ngược lại.
- Interaction point (nếu Module có, vd hướng cần đứng để tương tác) xoay theo cùng góc.
- **Sprite đổi theo hướng** (quyết định chốt — user tự sản xuất art): mỗi Module cho phép Rotate cần
  tối đa **4 biến thể sprite** (0°/90°/180°/270°), không phải xoay transform của 1 sprite duy nhất —
  vì camera isometric cố định không xoay, xoay thẳng transform một sprite vẽ theo góc chiếu iso sẽ
  cho hình sai phối cảnh (giống lý do `Character8Direction` dùng frame vẽ riêng cho từng hướng thay
  vì xoay 1 sprite). Ghost Preview cũng phải đổi đúng frame sprite theo góc đang chọn, không chỉ đổi
  màu xanh/đỏ.
- Không phải Module nào cũng cần đủ 4 hướng — Module có hình khối đối xứng hoặc không có mặt
  "trước/sau" ý nghĩa (vd Battery Bank) có thể chỉ cần 1 sprite và ẩn nút Rotate (xem edge case mục
  4). Quyết định module nào rotatable + cần bao nhiêu hướng art là việc content, không chốt cứng ở
  tài liệu thiết kế này — nhưng khi có yêu cầu art cụ thể cho Module nào, xác nhận trước với người
  tạo sprite để tránh vẽ dư/thiếu hướng.

### 3.7 Cancel

- ESC bất kỳ lúc nào trong Placement Mode → thoát, không mutate `WorldState`, item vẫn nguyên trong
  Inventory. Không có chi phí huỷ.

### 3.8 Confirm/Place

- Click trái tại vị trí hợp lệ → trừ 1 packed item khỏi Inventory/Storage, tạo `BuiltModuleState`
  tại đúng ô lưới đã snap + rotation đã chọn, publish event tương đương
  `ConstructionCompleted`/`ModuleRedeployed` để `PlacedModuleRenderer` đồng bộ.

### 3.9 Remove

- Giữ nguyên world-space hover-menu hiện có (`PlacedModuleHoverMenu`) — rê chuột dừng trên Module,
  nút "Tháo".
- Hoàn 1 packed item về **túi Player** (đồng bộ với Claim ở mục 3.2 — cả hai nguồn packed item đều
  vào cùng một nơi, không tách Storage/Player theo nguồn gốc). Giữ đúng logic Dismantle hiện tại về
  Durability — reset 100%, không giữ hao mòn cũ, đã là simplification chốt trước đó.
- Nếu Module đang bị Event tác động (vd Pump Jam) tại thời điểm Tháo: **không chặn**, tự động clear
  Event/trạng thái treo liên quan tới đúng instance đó cùng lúc — xem edge case mục 4.
- Quay lại trạng thái Inventory — có thể "Đặt" lại ngay từ đầu mục 3.3.

### 3.10 Save/Load

Cần persist:

- **Production đang chạy**: Module đang sản xuất + **thời điểm hoàn thành tuyệt đối** theo
  `WorldTimeMinutes` (không lưu "còn lại bao nhiêu phút" — cộng dồn dễ lệch nếu load ở một
  `WorldTimeMinutes` khác lúc save).
- **Ready to Claim**: số lượng theo loại Module, tồn tại độc lập với Production queue (không tự mất
  nếu không claim ngay).
- **Inventory packed items**: đã tự động sống qua `InventoryState`/`WorldStateSerializer` sẵn có —
  không cần cơ chế mới.
- **Placed modules**: `ShelterState.PlacedModules` đã có — cần thêm field Rotation vào
  `BuiltModuleState`.
- **Placement Mode đang mở**: **không cần lưu** — transient, giống nguyên tắc hiện có ("Presentation
  không qua Command/WorldState/save" áp dụng cho `PlacementModeController`). Nếu save giữa lúc đang
  ghost rồi load lại: thoát Placement Mode, item vẫn nguyên trong Inventory vì chưa commit gì — an
  toàn, không cần xử lý đặc biệt.

---

## 4. Edge case

### Production

- **Không đủ vật liệu giữa chừng**: không thể xảy ra vì trừ ngay lúc bắt đầu (giữ nguyên nguyên tắc
  hiện có) — không cần xử lý "công trình dừng vì thiếu vật liệu".
- **Player không ở Shelter lúc Production chạy/hoàn thành**: tiếp tục chạy bình thường (Passive Task
  qua `ShortTick` sẵn có) — không có gì đặc biệt, giống Construction hiện tại.
- **Bắt đầu Production thứ hai khi đang có 1 cái chạy**: chặn (giữ MVP 1-slot), hiện lý do
  "Đang sản xuất" — trừ khi mục 6 (multi-queue) được chọn.
- **Huỷ Production giữa chừng**: KHÔNG có trong yêu cầu gốc (chỉ có Cancel ở bước Placement, không
  phải Production) — nêu rõ đây là scope cut có chủ đích, không tự thêm tính năng huỷ sản xuất giữa
  chừng trừ khi được yêu cầu (đúng nguyên tắc "Simplicity First").

### Claim

- **Ready to Claim tồn đọng nhiều loại/nhiều đợt**: hiện danh sách, claim từng cái hoặc "Nhận tất
  cả" — không có hạn/hết hạn (Module không tự mất nếu không claim).
- **Túi Player quá tải/hết chỗ lúc Claim** (`InventorySystem.CanAdd` false): chặn Claim, báo lý do
  "Không đủ chỗ trong túi" — Ready to Claim vẫn giữ nguyên trạng thái chờ, không mất, người chơi
  claim sau khi giải phóng bớt tải trọng (drop/store bớt đồ khác). Không tự động rớt đồ xuống đất
  hay ép nhận gây Overload/Blocked ngoài ý muốn.

### Đặt từ Inventory / Ghost Preview

- **Đứng ngoài mọi Zone hợp lệ khi mở Placement Mode**: ghost đỏ toàn thời gian, hiện lý do "Ngoài
  vùng cho phép" — giữ hành vi hiện có.
- **Đổi tầng (floor) giữa lúc đang Placement Mode**: giữ hành vi hiện có
  (`PlacementModeController` tự `TeleportToFloor` theo Zone đang chọn) — không đổi.
- **Click chuột khi đang có Panel khác mở đè lên** (Inventory/Shelter Panel): phải tôn trọng
  `PointerOverUI` sẵn có — click trên panel không được lọt xuống thành click đặt Module.
- **Camera zoom/pan giữa lúc ghost đang hiện**: ghost phải bám đúng world position dưới chuột mỗi
  frame (không lệch theo camera) — đã là hành vi hiện có sau bug fix `worldCamera` tường minh.

### Snap Grid

- **Module có kích thước không chia hết cho cell size 1.0**: với 5 Module hiện có (đều mặc định
  đường kính 1 unit) sẽ luôn khớp chẵn; nếu về sau thêm Module kích thước lẻ (vd 1.5 unit), chấp
  nhận lệch tâm nhẹ trong ô, không cố ép đổi cell size chỉ vì 1 Module ngoại lệ.
- **Snap gần biên Zone khiến vị trí đã quantize rớt ra ngoài bounds**: reject (ghost đỏ), không tự
  kéo vào trong — tránh đặt sai ý người chơi.
- **Module footprint lớn chiếm nhiều ô**: overlap check phải xét toàn bộ vùng chữ nhật, không chỉ
  tâm ô.
- **Grid lưới cố định toàn Zone hay theo góc Zone riêng**: đề xuất neo lưới theo góc
  `BoundsMinX/MinY` của từng Zone (mỗi Zone có gốc lưới riêng) — tránh lưới lệch nhau giữa các Zone
  cạnh nhau gây cảm giác "không thẳng hàng" dù đều đã snap.

### Rotate

- **Xoay làm ghost đang hợp lệ thành overlap** (đổi trục dài/ngắn): re-validate ngay, không giữ
  trạng thái "hợp lệ" cũ đã lỗi thời.
- **Giới hạn góc**: chỉ 0°/90°/180°/270° — khớp đúng số sprite tối đa (4 hướng) sẽ được vẽ, không hỗ
  trợ góc tự do.
- **Module chỉ có 1 sprite/không có mặt trước-sau ý nghĩa** (vd Battery Bank): ẩn hẳn nút Rotate
  trong Placement Mode thay vì cho xoay mà không thấy gì đổi — tránh gây hiểu lầm "xoay không có tác
  dụng gì". Chỉ Module có đủ art theo hướng mới bật Rotate.
- **Thiếu sprite cho một hướng cụ thể** (art chưa vẽ kịp): Placement Mode không được cho xoay tới
  hướng thiếu art — hoặc chặn hẳn Rotate cho Module đó tới khi đủ 4 hướng, tránh hiện ghost lỗi/trống
  hình.
- **Rotate rồi Cancel**: không lưu lại góc đã chọn cho lần đặt tiếp theo (mỗi lần mở Placement Mode
  bắt đầu lại từ góc mặc định 0°) — tránh trạng thái ẩn khó nhớ giữa các lần đặt.

### Remove

- **Túi Player quá tải/hết chỗ lúc Tháo**: chặn Tháo (giữ đúng nguyên tắc "remove trả về inventory"
  — không cho Tháo nếu không trả về được), báo lý do "Không đủ chỗ trong túi".
- **Tháo Module đang trong trạng thái đặc biệt** (vd Water Purifier Filter Durability đã hao mòn):
  giữ simplification hiện có — reset về 100% lúc Tháo/Đặt lại, không giữ hao mòn (đã chốt trước đó,
  không đổi ở tài liệu này).
- **2 Module cùng loại gộp vào Inventory**: packed item là item thường, stack theo quy tắc
  `balance.json` sẵn có — không cần logic riêng.
- **Tháo Module đang là mục tiêu của Event** (vd Pump đang bị Pump Jam) — **quyết định chốt**: không
  chặn Tháo. Vì bản thân `BuiltModuleState` bị xoá khỏi thế giới khi Tháo (chỉ còn packed item trừu
  tượng trong túi), mọi trạng thái Event gắn với đúng instance đó (`Jammed`, v.v.) tự động clear cùng
  lúc — không cần popup xác nhận riêng, không để lại state Event mồ côi trỏ tới Module không còn tồn
  tại. Đơn giản hơn thêm luật chặn mới, và tránh được lớp bug "Event tồn tại nhưng Module biến mất".

### Save/Load

- **Save giữa lúc Placement Mode đang mở**: an toàn, xem mục 3.10 — không có state cần lưu riêng.
- **Save giữa lúc Production đang chạy**: bắt buộc lưu mốc `WorldTimeMinutes` hoàn thành tuyệt đối,
  không lưu số phút còn lại tương đối.
- **Save sau khi Production hoàn thành nhưng chưa Claim**: "Ready to Claim" phải là trạng thái persist
  riêng biệt, độc lập khỏi Production queue (không được để logic load nhầm coi Ready-to-Claim là
  "chưa xong" hoặc tự động mất).
- **Load vào game ở đúng lúc Production đáng lẽ đã xong** (world time đã vượt mốc hoàn thành trong
  lúc offline nếu có FastForward/Sleep): phải tính toán lại đúng như tick bình thường — Production
  chuyển Ready to Claim ngay khi load nếu mốc thời gian đã qua, không cần chờ tick tiếp theo.

---

## 5. Việc cần đổi so với hệ thống hiện tại (vì đây là bản thay thế BL-P3-03)

Liệt kê ở mức thiết kế — không viết code:

1. **Tách `StartConstructionCommand` hiện tại thành 2 khái niệm**: Production (không có x,y, không
   gắn Zone) và Placement (có x,y, tức thời, không tốn Materials/BuildMinutes). Hiện tại 2 việc này
   đang gộp làm một.
2. **`ConstructionCompleted` hiện tự sinh `BuiltModuleState` thẳng vào thế giới** — phải đổi thành:
   Production hoàn thành chỉ chuyển 1 trạng thái nội bộ "Ready to Claim" (đếm theo `ModuleDefinition`
   id, **không** phải packed item thật, không nằm trong `InventoryState`) → bấm "Nhận" mới thật sự
   gọi `InventorySystem.Add` cộng `PackedItemId` vào túi Player. Ready-to-Claim cần một state mới
   trong `ShelterState` (khác `ConstructionState`, sống độc lập, không tự mất nếu không claim).
3. **`ModuleDefinition.FootprintRadius` (hình tròn) phải đổi sang footprint dạng hộp** (Width ×
   Height, hoặc box collider size) — bắt buộc để Rotate có ý nghĩa. Đây là thay đổi schema dữ liệu,
   ảnh hưởng `modules_p3.json` và mọi chỗ đọc `FootprintRadius` trong `BuildSystem`.
4. **`ShelterPanel`**: bỏ luồng "chọn Zone + Module → Chọn vị trí" hiện tại cho đường xây mới, thay
   bằng "chọn Module → xác nhận sản xuất" (không chọn Zone/vị trí ở bước này) + thêm khu "Ready to
   Claim" + nút "Nhận" (cộng vào túi Player). Đường "Đặt lại (×N)" hiện có (Redeploy) trở thành nền
   tảng chung cho cả Claim-then-place lẫn Dismantle-then-place — nên hợp nhất UI hai đường này (đều
   là "packed item trong túi Player → Đặt"), thêm nút "Đặt" vào `InventoryPanel` (không chỉ
   `ShelterPanel`) vì packed item giờ nằm ở túi Player.
5. **`PlacementModeController`**: thêm snap-to-grid quantize trước validate (cell size 1.0, xem mục
   3.4), thêm input Rotate, thêm overlay lưới mờ trong Zone, đổi ghost từ 1 sprite cố định sang chọn
   đúng frame theo góc rotate đang chọn.
6. **`BuiltModuleState`**: thêm field Rotation.
7. **`ModuleDefinition`**: thêm cờ `IsRotatable` + tối đa 4 sprite theo hướng (thay vì 1 sprite cố
   định như hiện tại) — cần cho cả ghost trong Placement Mode lẫn `PlacedModuleRenderer` khi đã đặt.
8. **`balance.json`**: thêm field cell size lưới `shelter.build_grid_cell_size`, mặc định 1.0.

---

## 6. Đề xuất cải tiến (không bắt buộc — gắn nhãn rõ để không lẫn với yêu cầu gốc)

- **Multi-slot Production queue** — **quyết định chốt (2026-07-30): để dành P4**, không làm trong
  lần triển khai đầu tiên của loop này. Ghi nhận lại để không quên, không phải scope hiện tại. Khi
  làm: cho phép xếp hàng nhiều Module cùng lúc thay vì chặn cứng ở 1 slot; đánh đổi UI phức tạp hơn
  (danh sách hàng chờ) + risk balance (rush nhiều Module cùng lúc nếu đủ vật liệu).
- **Thông báo khi Production xong**: badge/icon nhấp nháy tại Console, hoặc toast khi world time
  vượt mốc hoàn thành trong lúc player đang ở gần Shelter — tránh người chơi quên quay lại Claim.
- **Preview "chiếm bao nhiêu ô" ngay ở bước chọn sản xuất** (trước khi tốn thời gian sản xuất) —
  tránh trường hợp sản xuất xong mới phát hiện không còn chỗ đặt hợp lệ trong Zone.
- **Hotkey rotate qua scroll wheel** ngoài phím R — chuẩn UX của nhiều game xây dựng góc nhìn
  isometric/top-down.

---

## 7. Quyết định đã chốt (2026-07-30, trả lời trực tiếp từ user)

| # | Câu hỏi | Quyết định |
| --- | --- | --- |
| 1 | Claim vào Storage hay túi Player? | **Túi Player** — cả Claim (3.2) và Remove/Tháo (3.9) đều trả về cùng một nơi. |
| 2 | Cell size lưới? | **1.0 world unit** — khớp `FootprintRadius` mặc định hiện tại (đường kính 1), field `shelter.build_grid_cell_size` trong `balance.json`. |
| 3 | Rotate có cần sprite theo hướng? | **Có** — tối đa 4 sprite/hướng mỗi Module rotatable, user tự tạo art. Module không có art đủ hướng thì ẩn Rotate. |
| 4 | Multi-slot Production queue ngay lần này? | **Không — để P4.** Ghi nhận ở mục 6, không phải scope hiện tại. |
| 5 | Ready to Claim tách biệt khỏi Inventory hay chỉ là UI xác nhận? | **Tách biệt hoàn toàn** — không phải packed item nằm sẵn trong túi; Claim mới thật sự gọi `InventorySystem.Add`. |
| 6 | Chặn Tháo Module đang bị Event tác động? | **Không chặn** — Tháo xoá `BuiltModuleState`, tự động clear Event/trạng thái treo gắn với instance đó cùng lúc, không cần popup riêng. |

Không còn câu hỏi mở nào chưa giải quyết trong phạm vi tài liệu này.

---

## 8. Cần user test gì sau khi implement

- Sản xuất 1 Module, rời Shelter, quay lại sau khi hết giờ — xác nhận Ready to Claim vẫn còn, Claim
  đúng số lượng.
- Claim rồi đặt từ Inventory, xác nhận không tốn thêm vật liệu/thời gian lần hai.
- Đặt gần biên Zone, kiểm tra snap có đẩy ra ngoài bounds hay reject đúng.
- Đặt 2 Module cạnh nhau bằng lưới, xác nhận thẳng hàng nhìn hợp lý (đánh giá cell size có phù hợp
  không).
- Rotate Module hình chữ nhật ngay sát biên/Module khác — xác nhận re-validate đúng khi xoay làm đổi
  bounding box.
- Cancel giữa chừng (ESC) — xác nhận item vẫn còn nguyên trong Inventory, không mất đồ.
- Tháo Module đã đặt, xác nhận packed item quay lại đúng chỗ (túi Player), đặt lại được ngay.
- Rotate Module có đủ 4 sprite hướng — xác nhận ghost và Module đã đặt hiện đúng frame theo góc đã
  chọn (không lệch hướng so với hitbox thật).
- Save/Load giữa lúc đang sản xuất (chưa xong) — Load lại đúng thời gian còn lại theo world time
  tuyệt đối, không bị cộng dồn sai.
- Save/Load khi có Ready to Claim chưa nhận — Load lại vẫn còn nguyên, chưa mất, và không lẫn vào
  Inventory thật (kiểm tra tải trọng túi không đổi cho tới khi thật sự bấm Nhận).
- Túi Player quá tải/hết chỗ lúc Claim và lúc Tháo — xác nhận bị chặn đúng, báo lý do rõ, không mất
  đồ hay bị ép Overload ngoài ý muốn.
- Tháo Module đang bị Event tác động (vd Pump đang Jam) — xác nhận Tháo không bị chặn và Event liên
  quan biến mất cùng lúc, không còn banner Event "mồ côi" trỏ tới Module đã không còn.
