# Building and Placement System Design

## 1. Mục tiêu

Hệ thống xác định cách người chơi:

- Đặt Module trong Shelter và ngoài thế giới.
- Phân loại Module theo môi trường đặt.
- Xây dựng.
- Tạm dừng.
- Tháo dỡ.
- Di chuyển vật thể.
- Kiểm tra điều kiện không gian.

Hệ thống phải hoạt động trong World Clock cố định.

---

## 2. Nguyên tắc thiết kế

- Shelter không phải là không gian tự do hoàn toàn.
- Shelter được thiết kế sẵn bởi hệ thống với cấu trúc và Zone cố định.
- Một số thành phần quan trọng là **Fixed Core Component** và không thể di chuyển hoặc tháo dỡ.
- Các thành phần này phục vụ cho Event, Hazard và Narrative.
- Người chơi chỉ có thể xây dựng trong các Zone hợp lệ được cho phép.
- Module được phân loại theo môi trường đặt: trong Shelter, ngoài Shelter, hoặc cả hai.
- Xây dựng diễn ra trực tiếp trong thế giới.
- Không nhảy thời gian để hoàn thành.
- Tiến độ được lưu.
- Module phải phù hợp vị trí.
- Lối đi và khả năng tương tác phải được bảo toàn.
- Việc bố trí phải tạo quyết định nhưng không trở thành game xây nhà phức tạp.

---

## 3. Shelter Structure Constraint

Shelter bao gồm:

```text
Fixed Core Components
+
Predefined Zones
+
Buildable Slots / Areas
```

### Fixed Core Components

Không thể:

- Di chuyển.
- Tháo dỡ.
- Thay đổi vị trí.

Ví dụ:

- Main Staircase.
- Structural Pillar.
- Drain Core.
- Electrical Backbone.
- Water Intake Point.

Các thành phần này:

- Là điểm neo cho Event.
- Là nguồn phát sinh Hazard.
- Là điểm kết nối hệ thống.

---

## 4. Module Classification

Module được chia thành ba loại chính:

### Shelter Module

- Chỉ đặt trong Shelter.
- Phụ thuộc vào Zone và Core Component.
- Ví dụ:

  - Elevated Storage.
  - Drying Station.
  - Battery Bank.

### Outdoor Module

- Chỉ đặt ngoài Shelter.
- Phụ thuộc vào địa hình, Hazard và môi trường.
- Ví dụ:

  - Barricade.
  - External Pump.
  - Flood Marker.
  - Temporary Light.

### Hybrid Module

- Có thể đặt cả trong và ngoài Shelter.
- Có yêu cầu khác nhau tùy môi trường.
- Ví dụ:

  - Portable Pump.
  - Communication Device.
  - Container.

---

## 5. Placement Mode

Placement Mode cho phép xem:

- Kích thước Module.
- Zone hợp lệ (nếu trong Shelter).
- Điều kiện môi trường (nếu ngoài Shelter).
- Điểm kết nối.
- Lối đi bị ảnh hưởng.
- Hazard tại vị trí.
- Yêu cầu Power hoặc Water.
- Các vùng bị khóa do Fixed Component.

World Clock tiếp tục chạy trong Placement Mode.

---

## 6. Placement Validation

Một vị trí hợp lệ khi:

```text
placement_type_valid == true
AND
zone_allowed == true (nếu trong Shelter)
AND
environment_allowed == true (nếu ngoài Shelter)
AND
space_clear == true
AND
access_path_valid == true
AND
support_requirement_met == true
AND
hazard_requirement_met == true
AND
not_overlapping_fixed_component == true
```

---

## 7. Placement Constraint

Module có thể yêu cầu:

- Gần tường.
- Gần cửa.
- Gần nguồn nước.
- Gần Power Connection.
- Trên nền chịu lực.
- Trên tầng cao.
- Ngoài khu sinh hoạt.
- Có thông gió.
- Có đường xả.
- Đặt trên mặt đất ổn định (đối với Outdoor Module).
- Không nằm trong vùng dòng chảy mạnh (đối với Outdoor Module).

Ngoài ra:

- Không được đặt lên hoặc che phủ Fixed Core Component.
- Không được làm mất khả năng truy cập tới Core Component.

Ví dụ:

```text
Generator
→
Không đặt trong Living Area
→
Cần thông gió
→
Cần khoảng trống bảo trì
```

---

## 8. Construction Phase

Mỗi công trình có các giai đoạn:

```text
Planning
Material Delivery
Assembly
Installation
Testing
Operational
```

Không phải mọi Module cần đủ sáu bước.

---

## 9. Material Delivery

Vật liệu phải được đưa tới vị trí xây.

Vật liệu có thể:

- Lấy từ Storage.
- Đặt gần công trình.
- Mang thủ công.
- Vận chuyển bởi nhiều người.

Nếu vật liệu bị mất hoặc hỏng, công trình dừng.

---

## 10. Active Construction

Xây dựng là Active Task.

Trong quá trình xây:

- World Clock tiếp tục.
- Builder không thể làm việc khác.
- Có thể bị gián đoạn.
- Hazard có thể tác động.
- Tool mất Durability.
- Fatigue tăng.

---

## 11. Construction Progress

```text
construction_progress
required_work
completed_work
```

Tiến độ không tự giảm khi dừng.

Tiến độ có thể bị mất nếu:

- Công trình bị ngập.
- Vật liệu bị phá.
- Zone bị sập.
- Placement trở nên không hợp lệ.

---

## 12. Multiple Builders

Nhiều người có thể xây cùng nhau.

Hiệu suất tăng không tuyến tính.

| Số người | Hiệu suất tổng |
| -------- | -------------- |
| 1        | 100%           |
| 2        | 170%           |
| 3        | 220%           |
| 4        | 250%           |

Một số công trình có giới hạn số người cùng thao tác.

---

## 13. Access Path

Module không được:

- Chặn cửa.
- Chặn cầu thang.
- Chặn lối thoát.
- Làm NPC không thể tới Task.
- Ngăn tiếp cận Module khác.
- Ngăn tiếp cận Fixed Core Component.

Hệ thống phải kiểm tra tuyến cơ bản giữa các điểm quan trọng.

---

## 14. Interaction Space

Mỗi Module có vùng thao tác.

Ví dụ:

```text
module_bounds
interaction_bounds
maintenance_bounds
```

Một Module có thể đặt vừa nhưng vẫn không hợp lệ nếu không có khoảng trống sử dụng.

---

## 15. Connection System

MVP sử dụng kết nối logic, không mô phỏng dây dẫn chi tiết.

Các loại kết nối:

```text
Power
Water Input
Water Output
Drain
Signal
```

Một số kết nối chỉ có thể lấy từ Fixed Core Component hoặc nguồn ngoài môi trường.

---

## 16. Move Module

### Portable Module

Có thể di chuyển sau khi tắt và tháo kết nối.

Ví dụ:

- Drying Station.
- Small Battery.
- Radio.
- Portable Pump.

### Fixed Module

Không thể di chuyển trực tiếp.

Cần tháo dỡ và xây lại.

### Core Component

Không thể:

- Di chuyển.
- Tháo dỡ.
- Thay thế.

---

## 17. Dismantle

Tháo dỡ là Active Task.

Không áp dụng cho Core Component.

Kết quả:

- Thu hồi một phần vật liệu.
- Có thể thu hồi toàn bộ linh kiện chính.
- Module mất trạng thái lắp đặt.
- Tool hao Durability.

---

## 18. World Placement

Module ngoài Shelter có thể:

- Bị nước cuốn.
- Bị NPC lấy.
- Bị hỏng.
- Trở thành chướng ngại.
- Thay đổi trạng thái theo Hazard.

Placement ngoài Shelter phải tính đến:

- Flood Depth.
- Current Strength.
- Terrain Stability.
- Accessibility.

---

## 19. Multiplayer Rule

- Placement Preview chỉ hiển thị cho người đang đặt.
- Vị trí hoàn tất được đồng bộ cho nhóm.
- Hai người không thể đặt chồng Module.
- Construction State là dữ liệu chung.
- Người khác có thể tham gia công trình đang xây.
- Dismantle Module chung phải là hành động có cảnh báo.
- Core Component không thể bị thay đổi bởi bất kỳ người chơi nào.

---

## 20. Dữ liệu hệ thống

### Placed Object

```text
placed_object_id
definition_id
placement_type
zone_id
position
rotation
build_state
condition
connections
interaction_bounds
owner_scope
is_core_component
```

### Construction Task

```text
construction_id
placed_object_id
required_materials
delivered_materials
required_work
completed_work
assigned_builders
required_tools
```

---

## 21. Phạm vi MVP

Triển khai:

- Placement Preview.
- Zone Validation.
- Environment Validation.
- Access Validation.
- Construction Progress.
- Material Delivery.
- Multiple Builders data.
- Shelter Module, Outdoor Module và Hybrid Module.
- Core Component System.
- Dismantle (không áp dụng cho Core).
- Logical Connection.
- World Placement cơ bản.

Chưa triển khai:

- Xây tự do ngoài lưới.
- Kết cấu vật lý chi tiết.
- Dây điện thủ công.
- Hệ thống ống nước trực quan phức tạp.
- Xây nhà từ nền móng.
- Terrain deformation.

---

## 22. Quyết định chốt

- Shelter được thiết kế sẵn với cấu trúc và Zone cố định.
- Có Core Component không thể thay đổi để phục vụ Event và Hazard.
- Module được phân loại theo môi trường đặt (Shelter, Outdoor, Hybrid).
- Người chơi có thể xây dựng cả trong và ngoài Shelter theo quy tắc riêng.
- Xây dựng diễn ra trong World Clock.
- Công trình có tiến độ và có thể bị gián đoạn.
- Module phải đáp ứng Zone, môi trường, không gian và đường tiếp cận.
- MVP dùng kết nối logic.
- Portable Module và Fixed Module có quy tắc khác nhau.
- Core Component là bất biến.
- Tháo dỡ chỉ thu hồi một phần vật liệu.
- Multiplayer chia sẻ Construction State.

---
