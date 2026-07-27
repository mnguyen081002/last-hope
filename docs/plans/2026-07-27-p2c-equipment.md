# Plan P2-C phần 1 — Equipment Protection (BL-P2-10)

`items_p2.json` đã có sẵn 5 equipment (jacket/boots/gloves/rope/dry_bag), mỗi cái một
`equip_slot` khác nhau, field `protection` khớp đúng `ItemProtection` đã viết ở S2 (đón đầu).
Chỉ thiếu hệ thống cho phép mặc/tháo và áp hiệu ứng.

## Map protection → nơi áp dụng

| Item | Field | Áp ở đâu |
| --- | --- | --- |
| `item_jacket` (body) | `wet_multiplier: 0.3` | Nhân vào mọi nguồn tăng Wet: mưa ambient (`ConditionSystem`) + crossing (`HazardSystem`) |
| `item_boots` (feet) | `exposure_block_level: 1`, `exposure_medium_multiplier: 0.5` | Chặn hoàn toàn Exposure gain khi băng qua ở tier ≤ block_level; tier cao hơn nhân multiplier. Suy ra ngữ nghĩa (doc không cho công thức) |
| `item_gloves` (hands) | `handles_contaminated: 1` | **Còn treo** — chỉ áp dụng cho `contaminated_handling_exposure_gain`, hành động "xử lý đồ nhiễm bẩn" chưa tồn tại (như đã ghi ở P2-B) |
| `item_rope` (tool) | `current_reduction: 1` | Giảm index Current Strength trước khi tra mảng `current_strength_*` — chính là "Rope giảm rủi ro" đã hoãn ở P2-B, giờ nối được |
| `item_dry_bag` (back) | `backpack_capacity_kg/liters` | Cộng thẳng vào `InventoryState.CapacityKg/Liters` lúc equip, trừ lại lúc unequip |

## Kiến trúc

- `Core/State/PlayerState.cs` — thêm `Equipped: Dictionary<EquipSlot,string>`.
- `Systems/Equipment/EquipmentSystem.cs` — `TryEquip`/`TryUnequip` (mutate state, dry_bag
  cộng/trừ capacity), `ComputeWetMultiplier`, `ComputeBootsProtection`, `ComputeCurrentReduction`.
- `Systems/Commands/EquipItemCommand.cs`, `UnequipItemCommand.cs`.
- `ConditionSystem.ApplyShortTick` / `HazardSystem.ApplyCrossingCost` / `ApplyCurrentCrossing`
  — thêm tham số protection **có default value** (1f/0/1f) để không phải sửa lại toàn bộ
  test/call site cũ, chỉ test mới cho equipment mới cần truyền giá trị khác default.
- `ConditionDriver`, `TravelSystem.Travel` — tính protection qua `EquipmentSystem` rồi truyền
  vào các hàm trên.
- `InventoryPanel` (UI) — nút Equip/Unequip cho item có `EquipSlot != None`.

## Verification

Compile → EditMode test (equip/unequip đúng slot, dry_bag đổi capacity, wet multiplier
giảm đúng tỉ lệ, boots block đúng tier, rope giảm current index) → sinh scene → build →
smoke test.

## User cần tự test bằng mắt

- Inventory Panel: nhặt jacket/boots/rope/dry_bag, bấm Equip — capacity có tăng khi mặc
  dry_bag không, Unequip có trả lại đúng không (nếu đang overload sau unequip, item phải
  bị từ chối tháo — không được tháo nếu tháo ra sẽ tràn túi).
- Mặc jacket, cheat mưa (Disaster Phase FirstRain) — Wet tăng chậm hơn rõ rệt so với không mặc.
