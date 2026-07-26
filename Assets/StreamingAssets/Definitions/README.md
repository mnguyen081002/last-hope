# Definitions

Definition Data của game (Item, Location, Route, SearchPoint, ...) ở dạng JSON thuần — xem `docs/00-project-overview/technical-specification.md` mục 9.

`LastHope.Data.DefinitionRegistry` (BL-P1-06) load toàn bộ file `.json` trong folder này lúc `00_Boot`.

## Quy ước

- **ID:** snake_case, ổn định vĩnh viễn (`item_water_bottle`, `location_convenience_store`).
- **Field name trên đĩa:** snake_case. Code C# dùng PascalCase, map qua `SnakeCaseNamingStrategy` — không đổi convention này, toàn bộ content hiện có phụ thuộc vào nó.
- **Version:** `manifest.json` giữ `definition_version`, hiện là `0.14.0`. Tăng khi đổi schema theo cách phá vỡ save cũ.

## File hiện có

| File | Nội dung |
| --- | --- |
| `manifest.json` | `definition_version` |
| `balance.json` | inventory cap/overload, travel load factor, condition rates, new_game |
| `items_p1.json`, `items_p2.json`, `items_p3_materials.json` | item def: weight/volume/stack/use_effects |
| `locations_p1.json`, `locations_p4.json` | location def |
| `routes_p1.json`, `routes_p4.json` | route def |
| `searchpoints_p1.json`, `searchpoints_p4.json` | search point + loot table |
| `modules_p3.json`, `shelterzones_p3.json` | build module + shelter zone |
| `events_p3.json`, `events_p4.json`, `events_p4_minh.json` | event def |
| `npcs_p4.json`, `phases_p4.json` | NPC + disaster phase timeline |
