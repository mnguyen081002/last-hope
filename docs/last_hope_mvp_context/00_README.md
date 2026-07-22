# Last Hope — Gameplay MVP Context Pack

Bộ tài liệu này được tách thành các file Markdown độc lập để AI hoặc Codex có thể đọc nhanh và hiểu đúng context thiết kế của **Last Hope**.

## Mục tiêu sử dụng

- Làm nguồn context cho Codex.
- Giữ các quyết định gameplay đã khóa.
- Hạn chế AI tự suy diễn sai thiết kế.
- Cho phép mở rộng từng hệ thống độc lập.
- Dùng làm nền cho prototype, task breakdown và implementation plan.

## Thứ tự đọc đề xuất

1. `01_game_vision_and_pillars.md`
2. `02_core_loop.md`
3. `03_time_and_action_system.md`
4. `04_economy_design.md`
5. `05_resource_flow_design.md`
6. `06_world_map_design.md`
7. `07_location_design_framework.md`
8. `08_radiation_and_expedition_risk.md`
9. `09_shelter_and_survival_strategies.md`
10. `10_storm_phase.md`
11. `11_multiplayer_rules.md`
12. `12_mvp_scope_and_content_budget.md`
13. `13_playtest_metrics.md`
14. `14_locked_decisions.md`

## Tên dự án

**Last Hope**

## Premise

Người chơi có một khoảng thời gian giới hạn để chuẩn bị trước khi một cơn bão phóng xạ quét qua khu vực. Họ phải thám hiểm, thu thập tài nguyên, quản lý phóng xạ, nâng cấp nơi trú ẩn và quyết định khi nào nên ngừng mạo hiểm để tập trung chuẩn bị cho cơn bão.

## Trải nghiệm ưu tiên

1. Ra quyết định căng thẳng.
2. Quản lý tài nguyên.
3. Chuẩn bị nơi trú ẩn.

Mọi hệ thống mới phải phục vụ ít nhất một trong ba ưu tiên này.

## Quy tắc dành cho AI

Khi đề xuất tính năng mới:

- Không phá vỡ các quyết định đã khóa.
- Không thêm cơ chế chỉ để tăng độ phức tạp.
- Không dùng loot respawn truyền thống.
- Không tạo vật phẩm bắt buộc duy nhất.
- Không biến game thành mô phỏng thời gian thực nặng.
- Không để người chơi có thể lấy toàn bộ tài nguyên trong một lượt.
- Ưu tiên quyết định có đánh đổi rõ ràng.
