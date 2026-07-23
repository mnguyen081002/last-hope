# Definitions

Definition Data của game (Item, Location, Route, SearchPoint, ...) ở dạng JSON thuần — xem `docs/00-project-overview/technical-specification.md` mục 9 (ADR 2026-07-24).

`LastHope.Data.DefinitionRegistry` (Sprint 2, BL-P1-06) load toàn bộ file `.json` trong folder này lúc `00_Boot`.

Quy ước ID: snake_case, ổn định vĩnh viễn (`item_clean_water`, `location_convenience_store`). Chưa có file định nghĩa nào — sẽ thêm ở Sprint 2.
