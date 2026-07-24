# Plan: Search hybrid loot — core đảm bảo + roll phần phụ

## Context

Quyết định thiết kế (2026-07-24, sau khi review các phương án roll / cố định / time-investment / condition-random): cơ chế search chuyển sang **hybrid** — mỗi search point có phần loot **đảm bảo** (designer khoá tổng tài nguyên sống còn trên map, không bao giờ có seed unwinnable) và phần loot **roll theo xác suất** (giữ replay value + tính đánh cược của mỗi chuyến đi).

**Phát hiện khi đọc code hiện tại** (`OpenSearchPointCommand.RollLoot`): hệ thống hiện tại KHÔNG phải weighted roll như tên gọi. `LootEntry.Weight` chỉ được check `> 0` — mọi entry luôn spawn, chỉ quantity là roll uniform [Min, Max]. Nghĩa là:
- Presence guarantee đã tồn tại ngầm (mọi entry luôn xuất hiện) → không có rủi ro seed xui thiếu nước.
- `Weight` là knob chết, gây hiểu nhầm.
- Cái còn thiếu là chiều "may rủi": chưa có entry nào *có thể không xuất hiện* → mọi lần chơi, mọi search point ra đúng cùng danh sách loại item.

Plan này làm cho hai chiều đó **tường minh**: entry đảm bảo thì khai báo `guaranteed`, entry may rủi thì có `chance` thật sự.

## Thiết kế chốt

`LootEntry` mới:

| Field | JSON (snake_case) | Ý nghĩa |
| --- | --- | --- |
| `Guaranteed` (bool, default false) | `guaranteed` | `true` → luôn spawn, bỏ qua Chance |
| `Chance` (int 0–100, default 100) | `chance` | entry không guaranteed: xác suất % xuất hiện (roll 1 lần qua stream "loot") |
| `MinQuantity`/`MaxQuantity` | giữ nguyên | quantity uniform [Min, Max] như cũ (áp dụng cho cả 2 loại entry) |
| ~~`Weight`~~ | ~~`weight`~~ | **XOÁ** — knob chết, thay bằng Chance để không ai hiểu nhầm là weighted-selection |

Logic roll (vẫn MỘT LẦN lúc first open, vẫn stream "loot", vẫn persist qua save — toàn bộ hành vi depletion/không-re-roll giữ nguyên):

```
foreach entry in LootTable:
    if (!entry.Guaranteed && loot.NextInt(0, 100) >= entry.Chance) continue;
    quantity = Min == Max ? Min : loot.NextInt(Min, Max + 1);
    AddItem(...)
```

Baseline resource floor của map = tổng `MinQuantity` các entry guaranteed — designer đọc thẳng từ JSON, không cần chạy mô phỏng.

## File thay đổi

1. `Assets/Game/Data/Definitions/SearchPointDefinition.cs` — `LootEntry`: xoá `Weight`, thêm `Guaranteed` + `Chance` (default 100). Cập nhật doc comment.
2. `Assets/Game/Core/Commands/OpenSearchPointCommand.cs` — `RollLoot` theo logic trên. Cập nhật doc comment (không còn "rolls its loot table" mơ hồ).
3. `Assets/Game/Data/DefinitionLoader.cs` — validation: `chance` ngoài [0, 100] → gom vào `Errors` (theo pattern gom-toàn-bộ-lỗi hiện có). `guaranteed:true` + `chance` khác default thì chance bị bỏ qua — không tính lỗi, không cần warning.
4. `Assets/StreamingAssets/Definitions/searchpoints_p1.json` — cập nhật 6 search point:
   - Nước + đồ ăn (kệ nước ×2, kệ đồ khô ×2): `guaranteed: true`, giữ Min/Max hiện tại → floor tài nguyên như cũ.
   - Pin: `chance` 50–70 tuỳ điểm.
   - Toolbox (quầy), container 20L (kho): `chance: 40–50` — đồ giá trị lớn thành "may ra có", đúng chất gamble.
   - Xoá field `weight` khỏi JSON.
5. `Tests/EditMode/SearchPointTests.cs` — sửa fixture theo schema mới + thêm test:
   - guaranteed luôn xuất hiện (chạy nhiều seed);
   - `chance: 0` không bao giờ xuất hiện, `chance: 100` luôn xuất hiện;
   - cùng seed → cùng kết quả (determinism giữ nguyên);
   - 4 test cũ (roll-một-lần, persist qua save, không re-roll, NotAtLocation) giữ hành vi.
6. `docs/backlog/BACKLOG.md` + `docs/backlog/CODEMAP.md` — cập nhật mô tả BL-P1-17 cùng commit.

**Save compatibility:** không đổi gì trong `SearchPointState` — save cũ load bình thường (điểm đã Rolled giữ nguyên inventory cũ, điểm chưa mở sẽ roll theo bảng mới). Không cần bump SaveVersion.

**Không đụng:** UI (`ContainerPanel`), Telemetry, `InventoryOwnerResolver`, resolver scheme, S7 plan (Condition system không liên quan search).

## Verification

- Full EditMode suite xanh (48 test hiện tại + ~3 test mới).
- Build Windows 0 lỗi + headless smoke như quy trình chuẩn.

**User cần test bằng tay (trong Editor/build):**
1. Vào cửa hàng, mở cả 6 điểm — kệ nước/đồ khô phải LUÔN có nước/đồ ăn.
2. New game vài lần (seed khác) — pin/toolbox/container 20L lúc có lúc không, số lượng dao động.
3. Save giữa chừng → load → điểm đã mở giữ nguyên đồ còn lại, không roll lại.
