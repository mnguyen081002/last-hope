# Time and Action System

## Quyết định đã khóa

Game sử dụng **thời gian theo khối hành động rời rạc**, không mô phỏng đồng hồ thời gian thực hoàn toàn.

Mỗi hành động làm thời gian thế giới tiến lên.

## Các hành động tiêu tốn thời gian

- Travel.
- Search.
- Deep Search.
- Break Lock.
- Salvage.
- Build.
- Craft.
- Repair.
- Treat.
- Decontaminate.
- Sleep.
- Special Interaction.

## Lý do sử dụng thời gian rời rạc

- Dễ cân bằng.
- Dễ hiển thị hậu quả.
- Giảm thời gian chờ.
- Phù hợp multiplayer.
- Dễ đồng bộ sự kiện.
- Dễ kiểm soát nhịp chơi.
- Cho phép người chơi hiểu rõ chi phí của quyết định.

## Searching

Tìm kiếm không diễn ra tức thời.

Mỗi container có thể cho người chơi chọn:

### Quick Search

- Mất ít thời gian.
- Phóng xạ thấp hơn.
- Có thể bỏ sót loot.
- Phù hợp khi cần rút nhanh.

### Deep Search

- Mất nhiều thời gian.
- Phóng xạ cao hơn.
- Tăng khả năng tìm loot hiếm hoặc loot ẩn.
- Có thể kích hoạt sự kiện.

### Ignore

- Không mất thời gian.
- Bỏ qua cơ hội loot.

## Time Budget của một ngày

Một ngày gồm các nhóm thời gian:

- Chuẩn bị.
- Di chuyển.
- Khám phá.
- Quay về.
- Khử nhiễm.
- Xây dựng.
- Chế tạo.
- Nghỉ ngơi.

Người chơi không thể tối đa hóa tất cả trong cùng một ngày.

## Expedition Categories

### Short Expedition

- 2–4 giờ trong game.
- Địa điểm gần.
- Loot cơ bản.
- Có thể còn thời gian xây dựng.
- Có thể thực hiện hai chuyến nhỏ.

### Medium Expedition

- 5–8 giờ.
- Tài nguyên chuyên dụng.
- Rủi ro trung bình.
- Chỉ còn thời gian cho công việc nhỏ tại nơi trú ẩn.

### Large Expedition

- 9–14 giờ.
- Gần như chiếm cả ngày.
- Tài nguyên chiến lược.
- Rủi ro cao.
- Có nguy cơ không kịp quay về.

## Công thức thời gian chuyến đi

```text
Tổng chuyến đi
=
Travel In
+
Exploration
+
Search
+
Interaction
+
Travel Out
+
Decontamination
+
Recovery
```

## Multiplayer Time

Thời gian thế giới là tài nguyên chung.

Khi người chơi chia đội:

- Mỗi nhóm có thể thực hiện hành động riêng.
- Đồng hồ tiến theo hệ thống đồng bộ.
- Hành động dài cần được giải quyết theo cùng đơn vị thời gian.
- Không dùng cơ chế một người ngủ để nhảy thời gian trong khi người khác đang hoạt động.

MVP nên ưu tiên hệ thống hành động theo lựa chọn và đồng bộ theo chunk thay vì thời gian trôi tự do.
