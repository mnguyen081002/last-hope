# Inventory and Equipment System Design

## 1. Mục tiêu

Inventory and Equipment System xác định cách người chơi mang, sắp xếp, sử dụng và đánh đổi vật phẩm.

Hệ thống phải đảm bảo:

- Sức chứa tạo ra quyết định rõ ràng.
- Trang bị đang mặc và vật phẩm đang mang có vai trò khác nhau.
- Vật phẩm nặng, cồng kềnh hoặc dễ hỏng tạo chi phí vận chuyển.
- Inventory không trở thành thao tác quản lý dư thừa.
- Hệ thống hỗ trợ Single-player và Multiplayer.
- Có thể mở rộng cho nhiều Disaster.

---

# 2. Nguyên tắc thiết kế

## 2.1. Inventory dựa trên trọng lượng và thể tích

Mỗi vật phẩm có:

```text
weight
volume
stack_size
```

Người chơi bị giới hạn bởi:

```text
maximum_weight
maximum_volume
```

Một vật phẩm có thể nhẹ nhưng chiếm nhiều chỗ.

Ví dụ:

- Áo mưa nhẹ nhưng cồng kềnh.
- Bình nhiên liệu nặng và chiếm nhiều thể tích.
- Thuốc nhẹ và nhỏ.
- Tấm kim loại vừa nặng vừa cồng kềnh.

---

## 2.2. Không dùng inventory grid phức tạp trong MVP

MVP không yêu cầu người chơi xoay vật phẩm hoặc xếp hình trong ba lô.

Inventory hiển thị theo danh sách hoặc nhóm vật phẩm.

Giới hạn được kiểm soát bằng:

- Trọng lượng.
- Thể tích.
- Slot trang bị.
- Loại vật chứa.

---

## 2.3. Mọi vật phẩm mang theo đều có chi phí

Mang thêm vật phẩm làm tăng:

- Carry Load.
- Stamina tiêu hao.
- Thời gian di chuyển.
- Nguy cơ trong vùng ngập.
- Khó khăn khi leo trèo.
- Rủi ro mất vật phẩm.

Không tồn tại vật phẩm quan trọng không có chi phí vận chuyển.

---

# 3. Cấu trúc Inventory

Inventory của người chơi gồm bốn lớp.

```text
Equipment
Quick Access
Backpack
Carried Object
```

---

# 4. Equipment

Equipment là vật phẩm đang được mặc hoặc sử dụng trực tiếp.

Các slot của MVP:

```text
head
body
hands
feet
back
primary_tool
secondary_tool
```

Ví dụ:

| Slot           | Vật phẩm               |
| -------------- | ---------------------- |
| Head           | Đèn đội đầu, mũ bảo hộ |
| Body           | Áo mưa, áo giữ nhiệt   |
| Hands          | Găng tay               |
| Feet           | Ủng chống nước         |
| Back           | Ba lô                  |
| Primary Tool   | Xà beng, búa           |
| Secondary Tool | Đèn pin, radio         |

Trang bị không chiếm thể tích trong ba lô nhưng vẫn tính trọng lượng.

---

# 5. Quick Access

Quick Access chứa vật phẩm cần dùng ngay.

Số slot đề xuất:

```text
4
```

Ví dụ:

- Nước.
- Băng gạc.
- Đèn pin.
- Dây thừng.
- Thuốc.
- Dao đa năng.

Vật phẩm không nằm trong Quick Access cần thời gian lấy ra.

Trong tình huống nguy hiểm, thời gian này có thể tạo khác biệt.

---

# 6. Backpack

Backpack là kho chứa chính của người chơi.

Mỗi ba lô có:

```text
weight_capacity
volume_capacity
water_resistance
access_speed
special_slots
```

Ví dụ:

| Loại ba lô       | Đặc điểm              |
| ---------------- | --------------------- |
| Túi nhỏ          | Nhẹ, sức chứa thấp    |
| Ba lô dân dụng   | Cân bằng              |
| Ba lô chống nước | Bảo vệ vật phẩm tốt   |
| Ba lô khung      | Sức chứa lớn, nặng    |
| Túi dụng cụ      | Có slot công cụ riêng |

---

# 7. Carried Object

Một số vật phẩm quá lớn để cho vào ba lô.

Ví dụ:

- Tấm gỗ.
- Máy bơm.
- Máy phát nhỏ.
- Can nhiên liệu lớn.
- NPC bị thương.
- Thùng vật tư.

Khi mang vật lớn:

- Hai tay có thể bị chiếm.
- Không thể chạy.
- Leo trèo bị hạn chế.
- Tốc độ di chuyển giảm.
- Nguy cơ trong nước tăng.
- Có thể phải đặt vật xuống để tương tác.

Một người chơi chỉ mang được một Carried Object tại một thời điểm.

---

# 8. Thuộc tính vật phẩm

Mỗi item có các thuộc tính cơ bản:

```text
item_id
category
weight
volume
stack_size
condition
durability
contamination_state
water_resistance
value
tags
```

Không phải vật phẩm nào cũng cần dùng toàn bộ thuộc tính.

---

# 9. Nhóm vật phẩm

## Consumable

- Thức ăn.
- Nước.
- Thuốc.
- Pin.
- Nhiên liệu.

## Material

- Gỗ.
- Kim loại.
- Vải.
- Dây.
- Vật liệu chống thấm.
- Linh kiện.

## Tool

- Búa.
- Xà beng.
- Kìm.
- Dao đa năng.
- Đèn pin.
- Radio.

## Equipment

- Áo mưa.
- Ủng.
- Găng tay.
- Mũ bảo hộ.
- Ba lô.

## Device

- Máy bơm.
- Máy phát.
- Máy lọc nước.
- Bộ sạc.

## Quest or Information Item

- Bản đồ.
- Ghi chú.
- Chìa khóa.
- Mã khóa.
- Thiết bị lưu trữ dữ liệu.

---

# 10. Stack Rule

Chỉ vật phẩm đồng nhất mới được stack.

Hai item chỉ stack khi có cùng:

```text
item_id
condition
contamination_state
```

Ví dụ:

- Nước sạch không stack với nước chưa xử lý.
- Pin đầy không stack với pin đã dùng.
- Thực phẩm sạch không stack với thực phẩm bị ngâm nước đen.

---

# 11. Condition và Durability

## Condition

Condition đại diện cho chất lượng tổng thể.

Các mức:

```text
Good
Worn
Damaged
Broken
```

## Durability

Durability là giá trị số dùng để xác định khi nào item đổi Condition.

Độ bền giảm khi:

- Sử dụng.
- Bị ngâm nước.
- Tiếp xúc Hazard.
- Chịu va đập.
- Không được bảo quản đúng cách.

---

# 12. Water Resistance

Trong MVP Mưa Đen, vật phẩm có một trong ba mức:

```text
None
Resistant
Waterproof
```

## None

- Dễ bị hỏng khi ngâm nước.
- Có thể bị nhiễm bẩn.

## Resistant

- Chịu được mưa hoặc tiếp xúc ngắn.
- Không bảo vệ khi ngâm lâu.

## Waterproof

- Không bị ảnh hưởng bởi nước trong điều kiện thông thường.
- Vẫn có thể hỏng do va đập hoặc Hazard đặc biệt.

---

# 13. Contamination State

Vật phẩm có ba trạng thái:

```text
Clean
Wet
Black Water Contaminated
```

## Clean

Có thể sử dụng bình thường.

## Wet

Có thể cần sấy khô trước khi sử dụng.

## Black Water Contaminated

Có thể:

- Không dùng được ngay.
- Cần xử lý.
- Gây Status Effect.
- Làm ô nhiễm kho chứa sạch.

Không phải mọi vật phẩm đều có thể làm sạch.

---

# 14. Container Rule

Một số vật chứa có tác dụng bảo vệ item.

Ví dụ:

```text
Waterproof Bag
Tool Case
Medical Box
Fuel Container
```

Mỗi container có:

```text
allowed_item_tags
weight_capacity
volume_capacity
protection_level
access_speed
```

Container không tạo thêm sức chứa vô hạn.

Trọng lượng của item bên trong vẫn được tính.

---

# 15. Inventory Interaction

Người chơi có thể:

- Nhặt.
- Thả.
- Chuyển.
- Chia stack.
- Gán Quick Access.
- Trang bị.
- Thay thế.
- Sử dụng.
- Đặt vào container.
- Kiểm tra condition.

Mỗi thao tác quan trọng đều diễn ra trong World Clock.

Inventory không làm dừng thời gian trong Multiplayer.

---

# 16. Loot Decision

Khi phát hiện vật phẩm, người chơi phải đánh giá:

```text
Giá trị
vs
Trọng lượng
vs
Thể tích
vs
Tình trạng
vs
Mức cần thiết hiện tại
```

Không nên có hệ thống tự động nhặt toàn bộ loot.

Người chơi có thể để lại vật phẩm và quay lại sau.

---

# 17. Overload

Người chơi có thể vượt giới hạn trọng lượng trong phạm vi nhỏ.

Mức đề xuất:

```text
maximum_overload = 120%
```

Khi quá tải:

- Không thể chạy.
- Stamina tiêu hao nhanh.
- Không thể leo trèo bình thường.
- Di chuyển trong nước rất nguy hiểm.
- Action Efficiency giảm.

Trên giới hạn quá tải, người chơi không thể nhặt thêm.

---

# 18. Tool Usage

Tool có thể ảnh hưởng tới:

- Tốc độ hành động.
- Noise.
- Durability cost.
- Loại vật cản có thể xử lý.
- Rủi ro gây Injury.

Ví dụ:

| Tool        | Lợi ích                 | Chi phí                |
| ----------- | ----------------------- | ---------------------- |
| Xà beng     | Mở cửa nhanh            | Nặng, gây tiếng động   |
| Búa         | Xây dựng và phá vật cản | Hao độ bền             |
| Dao đa năng | Nhẹ, nhiều công dụng    | Hiệu quả thấp          |
| Đèn pin     | Cải thiện tầm nhìn      | Tốn pin                |
| Radio       | Nhận thông tin          | Bị ảnh hưởng bởi nhiễu |

---

# 19. Equipment Trade-off

Trang bị bảo vệ tốt hơn thường phải đánh đổi bằng:

- Trọng lượng.
- Thể tích.
- Tốc độ di chuyển.
- Stamina.
- Số slot.
- Khả năng tương tác.

Ví dụ:

Áo mưa nặng:

- Giảm Wet.
- Bảo vệ item tốt hơn.
- Tăng nhiệt và trọng lượng.

Ủng cao su:

- Giảm tiếp xúc nước.
- Di chuyển trong nước nông tốt hơn.
- Chạy chậm hơn trên mặt đất khô.

---

# 20. Vật phẩm nhiệm vụ và thông tin

Vật phẩm nhiệm vụ không nên hoàn toàn miễn phí về sức chứa.

Các vật phẩm nhỏ như chìa khóa hoặc ghi chú có thể dùng một mục riêng:

```text
Document Pouch
```

Giới hạn của Document Pouch nên đủ lớn để tránh quản lý phiền phức nhưng không vô hạn với thiết bị lớn.

Thiết bị thông tin như radio hoặc máy ghi dữ liệu vẫn tính trọng lượng và slot.

---

# 21. Shelter Storage

Vật phẩm mang về Shelter được chuyển vào kho.

Shelter Storage nên chia thành:

```text
Clean Storage
Wet Storage
Contaminated Storage
Fuel Storage
Large Object Storage
```

Đặt sai khu vực có thể gây:

- Lây nhiễm bẩn.
- Hư hỏng.
- Nguy cơ cháy.
- Giảm hiệu quả xử lý.

---

# 22. Multiplayer Inventory

Mỗi người chơi có Inventory riêng.

Shelter sử dụng kho chung.

Nguyên tắc:

- Không tự động chia loot.
- Người chơi chuyển item trực tiếp cho nhau.
- Có thể thả item xuống thế giới.
- Carried Object có thể cần hai người mang.
- Đồng đội có thể nhặt túi của người bị Incapacitated.

Một số vật lớn có thể yêu cầu:

```text
required_carriers = 2
```

Mang theo hai người giúp:

- Tăng tốc độ.
- Giảm Stamina tiêu hao.
- Cho phép vận chuyển vật phẩm đặc biệt.

---

# 23. UI Requirement

UI cần hiển thị:

- Current Weight.
- Maximum Weight.
- Current Volume.
- Maximum Volume.
- Equipment Slot.
- Quick Access.
- Item Condition.
- Water Resistance.
- Contamination State.
- Durability.
- So sánh item đang dùng và item mới.

Cảnh báo phải rõ khi:

- Quá tải.
- Item sắp hỏng.
- Item bị nhiễm.
- Container không phù hợp.
- Vật phẩm có nguy cơ hỏng do nước.

---

# 24. Dữ liệu hệ thống

## Inventory

```text
owner_id
current_weight
maximum_weight
current_volume
maximum_volume
equipment_slots
quick_access_slots
backpack_container
carried_object
items
```

## Item

```text
item_id
instance_id
category
weight
volume
stack_size
condition
durability
contamination_state
water_resistance
tags
container_id
```

## Container

```text
container_id
container_type
weight_capacity
volume_capacity
allowed_tags
protection_level
access_speed
items
```

---

# 25. Phạm vi MVP

Triển khai:

- Weight.
- Volume.
- Backpack.
- Equipment Slot.
- Quick Access.
- Carried Object.
- Stack.
- Condition.
- Durability.
- Water Resistance.
- Contamination State.
- Clean và Contaminated Storage.
- Multiplayer-compatible data structure.

Chưa triển khai:

- Inventory grid.
- Vật phẩm xoay và xếp hình.
- Hệ thống quần áo nhiều lớp phức tạp.
- Chế độ tự động tối ưu ba lô.
- Hàng chục loại container chuyên dụng.
- Hệ thống bảo hiểm hoặc quyền sở hữu loot.

---

# 26. Quyết định chốt

- Inventory sử dụng trọng lượng và thể tích.
- MVP không dùng inventory grid.
- Equipment, Quick Access, Backpack và Carried Object là bốn lớp riêng.
- Trang bị vẫn tính trọng lượng.
- Người chơi có thể quá tải tới giới hạn cho phép.
- Vật phẩm lớn phải được mang trực tiếp.
- Condition, Durability và Contamination được lưu theo từng item instance.
- Water Resistance là thuộc tính quan trọng của MVP Mưa Đen.
- Shelter Storage phân tách đồ sạch, ướt và nhiễm.
- Multiplayer dùng inventory cá nhân và kho Shelter chung.
- Hệ thống ưu tiên quyết định loot hơn thao tác sắp xếp.
