# Resource Flow Design

## Dòng chảy tổng quát

```text
Thế giới
    ↓
Người chơi tìm thấy tài nguyên
    ↓
Mang về nơi trú ẩn
    ↓
Khử nhiễm hoặc xử lý
    ↓
Lưu trữ
    ↓
Tiêu hao, xây dựng hoặc sửa chữa
    ↓
Phát sinh nhu cầu mới
    ↓
Lập kế hoạch chuyến đi tiếp theo
```

## Phân loại tài nguyên theo nguồn

### Finite Resources

- Canned Food.
- Medicine.
- Filters.
- Fuel.
- Components.
- Electronics.
- Protective Equipment.

Không tự hồi sinh.

### Renewable Resources

Chỉ xuất hiện qua đầu tư:

- Rainwater.
- Filtered Water.
- Farming.
- Solar Power.
- Manual Power.

Trong MVP, các nguồn này giảm áp lực nhưng không thay thế khám phá.

### World-Generated Resources

Xuất hiện do sự kiện hợp lý:

- Aid Convoy.
- Crashed Truck.
- Exposed Basement.
- Dead Survivor Backpack.
- Collapsed Building.
- Emergency Cache.

Đây không phải loot respawn.

## Các tác nhân tiêu hao

### Player

- Food.
- Water.
- Medicine.
- Filters.
- Equipment Durability.

### Shelter

- Fuel.
- Filters.
- Components.
- Seal Material.
- Water.
- Electricity.

### Time

- Food spoilage.
- Deterioration.
- Environmental contamination.

Chỉ áp dụng có chọn lọc để tránh micromanagement.

### World

- NPC lấy tài nguyên.
- Địa điểm cháy.
- Đường bị phong tỏa.
- Tòa nhà sập.
- Bão khiến khu vực không thể tiếp cận.

## Loot Respawn

Không dùng loot respawn truyền thống.

Vật chứa đã loot không tự đầy lại.

Người chơi quay lại địa điểm vì:

- Có công cụ mới.
- Khu vực mới mở.
- Sự kiện mới.
- Basement lộ ra.
- Shortcut.
- NPC.
- Mission.
- Thiết bị đặc biệt.

## Location Lifecycle

```text
Undiscovered
Discovered
Visited
Surface Explored
Easy Loot Taken
Locked Area Remains
Main Objective Complete
New Event
Nearly Exhausted
Exhausted
Blocked or Destroyed
```

## Resource Budget Toàn Lượt

Tổng tài nguyên trong thế giới nên bằng khoảng:

> 150–200% mức Safe.

Người chơi không thể thu thập toàn bộ vì:

- Thời gian.
- Sức chứa.
- Phóng xạ.
- Công cụ.
- Đường bị chặn.
- Sự kiện.
- Loot bị mất.
- Quyết định ưu tiên.

## Nhu cầu tham chiếu

### Food

- Minimum: 7.
- Safe: 10.
- Comfortable: 14–16.

### Water

- Minimum: 11–13.
- Safe: 17–20.
- Comfortable: 24–28.

### Filters

- Minimum: 7–8.
- Safe: 10–12.
- Comfortable: 14–16.

### Fuel

- Minimum: 4–5.
- Safe: 7–8.
- Comfortable: 10–12.

### Components

- Minimum: 5.
- Safe: 9–12.
- Comfortable: 16+.

Các giá trị này là số liệu prototype, cần playtest.

## Nguyên tắc chống cạn map

- Một số location được phép cạn hoàn toàn.
- Toàn bộ map không được cạn trước bão.
- Ở mỗi giai đoạn phải còn ít nhất hai mục tiêu hợp lý.
- World events có thể mở nguồn mới.
- Không dùng respawn để vá lỗi content shortage.
