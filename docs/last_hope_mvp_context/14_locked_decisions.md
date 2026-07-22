# Locked Decisions

File này là nguồn tham chiếu nhanh cho các quyết định thiết kế đã khóa.

## Vision

- Game xoay quanh chuẩn bị cho bão phóng xạ.
- Trải nghiệm ưu tiên là quyết định căng thẳng.
- Resource management đứng thứ hai.
- Shelter preparation đứng thứ ba.

## Time

- Dùng discrete time chunks.
- Mọi hành động quan trọng đều tiêu tốn thời gian.
- Search không instant.
- Multiplayer không dùng time skip tự do.

## Searching

- Quick Search.
- Deep Search.
- Ignore.
- Deep Search cho loot tốt hơn nhưng tốn thời gian và radiation hơn.

## Loot

- Không dùng traditional loot respawn.
- Loot đã lấy không tự quay lại.
- World events có thể tạo nguồn loot mới hợp lý.
- Location có thể exhausted.

## World Map

- Shelter gần trung tâm.
- Map chia thành district có identity riêng.
- Có multiple routes.
- Map thay đổi theo ngày.
- Người chơi không thể khám phá toàn bộ trong một lượt.

## Resources

- Mỗi tài nguyên quan trọng có ít nhất hai nguồn.
- Mỗi tài nguyên chiến lược có ít nhất hai công dụng.
- Total world resources khoảng 150–200% mức Safe.
- Player không thể tiếp cận tất cả.

## Survival Strategies

Có ít nhất ba hướng:

1. Strong Shelter.
2. Technical Shelter.
3. Manual Survival.

Không có hướng duy nhất bắt buộc.

## Dependencies

- Không có unique mandatory item.
- Soft lock được chấp nhận.
- Hard lock cần tránh.
- Generator optional.
- Radio optional.
- Decontamination room optional.
- Salvage giúp phục hồi một phần sai lầm.

## Storm

- Storm là final exam.
- Storm không tạo game hoàn toàn mới.
- Failure phải truy ngược được về quyết định trước đó.

## MVP

- 7 preparation days.
- Day 8 warning.
- 2–3 storm days.
- 8–12 expeditions.
- 4–6 giờ một lượt.
- 12–16 major locations.
- 15–25 minor locations hoặc events.

## AI Constraint

AI không được tự thay đổi các quyết định trong file này trừ khi người dùng yêu cầu rõ ràng.
