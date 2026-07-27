# Plan P2-B phần 1 — Flood State trên Route (BL-P2-04, một phần BL-P2-06)

`hazard-framework-design.md` (978 dòng) mô tả phạm vi rất rộng (Intensity 6 cấp, Protection
đa nguồn, Forecast, Structural Collapse, Electromagnetic Interference...). `balance.json`
chỉ có số cho đúng **1 mảnh**: `hazard.crossing_*` (4 phần tử) + `contaminated_handling_exposure_gain`.
Slice này chỉ làm đúng phần có số thật.

## Cắt phạm vi có chủ đích

- **Chỉ Flood State + crossing cost.** Current Strength (BL-P2-05), Electrified Water
  (BL-P2-07), Route Closure (BL-P2-08), Disaster Phase (BL-P2-09) — `balance.json` không có
  field nào cho 4 mục này. Không bịa số.
- Mapping đã chốt với user: mảng 4 phần tử = **Dry(0)/Shallow(1)/Medium(2)/Deep(3)**.
  **Impassable là trạng thái thứ 5 riêng, không nằm trong mảng — chặn hoàn toàn, không đi
  qua được.** Tránh softlock bằng route thay thế (BL-P2-12, content P2, chưa làm — hiện tại
  chỉ có 1 route `route_shelter_store` nên chưa test được kịch bản có route thay thế).
- Chưa có Disaster Phase để tự động nâng flood theo thời gian — thêm cheat trong Debug Panel
  để set flood state thủ công, test được cơ chế trước khi P2-B phần 2 (Disaster Phase) nối
  vào làm nó tự động.

## Thiết kế

- `Core/State/RouteState.cs` — enum `FloodState {Dry, Shallow, Medium, Deep, Impassable}` +
  `RouteState { FloodState Flood = Dry }` + `WorldState.Routes: Dictionary<string,RouteState>`
  + `GetOrCreateRoute`.
- `Data/Definitions/BalanceDefinition.cs` — thêm `HazardBalance` khớp 1:1
  `balance.json.hazard` (mảng float 4 phần tử, Newtonsoft deserialize thẳng).
- `Systems/Hazard/HazardSystem.cs` — hàm thuần: `IsPassable`, `FloodIndex` (Dry=0..Deep=3,
  Impassable không có index), `ApplyCrossingCost` (Stamina -=, BlackWaterExposure +=,
  Wet +=, clamp), `TimeFactor`.
- `Systems/Travel/TravelSystem.cs` — `ComputeTravelMinutes` nhân thêm `TimeFactor` (chồng
  với loadFactor hiện có — cố ý, mang nặng + băng qua nước sâu phải nặng hơn cả hai cộng
  lại, đúng tinh thần "hậu quả tăng dần"). `Travel` áp `ApplyCrossingCost` một lần.
- `Systems/Commands/BeginTravelCommand.cs` — Validate reject nếu route `Impassable`
  (`CommandErrorCode.NotAllowedNow`).
- `DebugTools/Panel/DebugPanel.cs` — cheat cycle flood state của `route_shelter_store` để
  test không cần chờ Disaster Phase.

## Verification

Compile → EditMode test (crossing cost đúng theo index, Impassable chặn Travel, time
factor nhân đúng cả hai chiều loadFactor×floodFactor, Dry vẫn có wet_gain=10 nhỏ) → sinh
scene → build → smoke test.

## User cần tự test bằng mắt

- F2 Debug Panel: cheat đổi flood state route Shelter↔Store, đi qua thử — Stamina/Wet/
  Exposure có đổi theo tier không, thời gian di chuyển có tăng theo tier không.
- Đặt Impassable rồi thử tương tác Travel Point — phải bị từ chối, không đi qua được.
