# Plan P1-C — Exploration Gameplay → Gate P1

Phạm vi: BL-P1-14..22 (Interaction, Item, Inventory, Search, Storage, Travel, Location
blockout, Telemetry, Playtest). Tiếp theo Gate M1 đã PASS.

## Thiết kế khóa (đã chốt trong `2026-07-24-mvp-coding-plan.md`, giữ nguyên)

- **Search = container mở tức thì**: E → thấy toàn bộ ngay, Take All + nhặt lẻ. KHÔNG
  progress bar, KHÔNG reveal dần. Toàn bộ `searchpoints_p1.json` có `open_time_minutes: 0`
  nên không cần cơ chế timer — chỉ log cảnh báo nếu JSON sau này khai >0 (chưa cài đặt).
- Nội dung container roll **một lần** lúc mở đầu tiên (stream `"loot"`), đồ không lấy nằm
  lại vĩnh viễn trong `SearchPointState.RemainingItems`.
- Travel: `TickScheduler.FastForward(travelMinutes × loadFactor)`, loadFactor theo tier
  overload từ `balance.json`.
- Carried Object (`item_water_container_20l`, `two_hand_carry: true`) không vào backpack,
  chiếm `InventoryState.CarriedObjectItemId`, chỉ giữ được 1 cái.

## Cắt phạm vi có chủ đích (nêu rõ, không làm ẩn)

- **Interaction không có "hold"**: mọi search point P1 mở tức thì (0 phút) nên hold không
  tạo khác biệt gameplay. Interact = nhấn tức thì trên interactable gần nhất.
- **Không có Equip UI**: `items_p1.json` không có item nào có `equip_slot` (đồ trang bị nằm ở
  `items_p2.json`, P2). `EquipSlot` đã có trong `ItemDefinition` (Data layer từ S2) — hệ
  thống Inventory hỗ trợ được, chỉ chưa cần UI vì P1 không có nội dung dùng tới.
- **Drop không có world pickup vật lý**: đồ bỏ xuống đất (`LocationState.DroppedItems`) hiện
  qua danh sách trong Inventory Panel ("Đồ dưới đất tại đây" + nút Pick up), không phải
  GameObject rơi ngoài world. Exit Criteria Gate P1 chỉ yêu cầu đồ **bỏ lại trong search
  point container** sống qua save/load — không yêu cầu world-drop vật lý.
- **Không animation theo hướng**: `Facing` đã có trên `PlayerController`, sprite 8 hướng đã
  có sẵn trong `Assets/Art/Production/Character8Direction`, nhưng đổi sprite theo hướng
  không nằm trong mô tả BL-P1-14..22 nào — để lại cho polish sau, ghi vào CODEMAP.

## Core/Systems (test được, không đụng engine)

- `InventoryOps`: thêm overload thao tác thẳng trên `List<ItemInstanceState>` (không bọc
  `InventoryState`) để dùng chung cho search point remaining items và storage — không tạo
  type mới, không phá API cũ (`InventoryOps.AddItem(InventoryState, ...)` vẫn giữ, delegate
  xuống overload mới).
- `LocationState.StorageContainer : List<ItemInstanceState>` — kho shelter, không giới hạn
  sức chứa (nhất quán với comment sẵn có trong `InventoryState`).
- `InventorySystem` (Systems): `ComputeLoadTier` (Normal/Light/Heavy overload theo
  `balance.json`), `CanAdd`, `SpeedModifierFor(tier)`. Carried Object route riêng khỏi
  backpack capacity.
- Command mới: `TransferItemCommand` (player ↔ storage ↔ search point remaining — 3 nguồn
  cùng kiểu `List<ItemInstanceState>` sau refactor), `DropItemCommand`, `PickUpItemCommand`,
  `OpenSearchPointCommand`, `TakeAllFromSearchPointCommand`, `BeginTravelCommand`.
- `SearchSystem`: roll loot table qua `RngService.Stream("loot")` — `guaranteed` luôn thêm,
  `chance` roll theo `NextChance`, số lượng `NextIntInclusive(min,max)`.
- `TravelSystem`: validate route nối đúng location hiện tại, tính `loadFactor` theo tier,
  `FastForward`, đổi `CurrentLocationId`, publish `LocationChanged`.
- Event mới: `LocationChanged` (đã có sẵn từ S3), `SearchPointOpened` (đã có), thêm
  `ItemPickedUp`, `ItemLeftBehind` cho Telemetry bám theo.

## Presentation (cần user xác nhận bằng mắt, không test tự động được)

- `IInteractable` + `InteractionDetector` (OverlapCircle quanh player, ưu tiên gần nhất) +
  prompt text đơn giản (OnGUI tạm, chưa cần world-space UI đẹp).
- `SearchPointView`, `StorageView`, `TravelPointView` — Presentation component nối
  interaction với Command tương ứng, đọc `id` định nghĩa từ Inspector.
- `PlayerAvatarSync` — ghi `PlayerState.PositionX/Y` mỗi frame, áp lại từ state khi
  `WorldStateReloaded`, không tự đổi `PositionLocationId`.
- `SceneFlowController` — thay `BootLoader` hard-code `90_TestSystems`: đọc
  `LocationDefinition.SceneName` theo `WorldState.Player.CurrentLocationId` lúc boot, và
  load lại theo `LocationChanged` lúc travel (unload scene cũ, load additive scene mới, đặt
  lại vị trí player theo `PlayerSpawnPoint` marker trong scene đích).
- UI (uGUI, TextMeshPro): `InventoryPanel` (list + weight/volume bar + Drop/Transfer),
  `SearchPanel` (list + Take/Take All).
- Scene mới qua `SceneSetup`: `20_MainShelter` (storage + travel point ra store), 
  `41_Location_ConvenienceStore` (6 search point khớp `searchpoints_p1.json` + travel point
  về shelter). `90_TestSystems` giữ nguyên làm scene test Y-sort/movement, không còn là scene
  boot mặc định.

## Telemetry (BL-P1-21)

`TelemetryLogger` (DebugTools hoặc Systems) ghi JSONL vào `persistentDataPath/Telemetry`,
subscribe `EventBus`: `time_spent` (định kỳ), `travel_started/completed`, `search_completed`,
`item_collected`/`item_left_behind` (diff giữa lúc mở và lúc rời search point), thời gian mở
Inventory Panel.

## Verification

Mỗi khối (Core/Systems trước, Presentation sau) đều: batchmode compile → EditMode test →
(sau Presentation) sinh scene + build Windows + smoke test headless.

**Gate P1** cuối cùng cần **user tự playtest** bằng build thật (BL-P1-22) — AI không thấy
hình, không đánh giá được "cảm giác loot decision". Checklist đưa cho user ở cuối plan khi
Presentation xong.

## User cần tự test bằng mắt (sau khi Presentation xong)

- Từ Main Shelter, đi tới Convenience Store qua Travel Point — có tốn thời gian game không
  (xem đồng hồ nhảy), scene có đổi đúng không.
- Tương tác 6 search point: E → thấy list ngay (không progress bar), Take All có khi nào báo
  đầy/thất bại không (kệ kho ~20kg/35L > carry cap 15kg/25L phải buộc triage).
- Đóng game, mở lại (hoặc F2 Save/Load): search point đã mở giữ nguyên phần chưa lấy.
- Mang `item_water_container_20l` (Carried Object) — có chiếm ô riêng, không lẫn vào backpack.
- Về lại Shelter, chuyển đồ vào Storage — Storage không giới hạn sức chứa.
- Mở Inventory Panel (phím ToggleInventory) — số kg/L đúng, Overload có giảm tốc độ đi
  không khi vượt ngưỡng.
