# Shelter Site and Relocation System Design

## 1. Mục tiêu

Hệ thống xác định:

- Điều kiện một vị trí có thể trở thành Shelter.
- Cách đánh giá vị trí.
- Cách thiết lập Shelter mới.
- Cách di chuyển tài nguyên và người.
- Hậu quả của việc bỏ Shelter cũ.

---

## 2. Shelter Site

Shelter Site không phải là không gian trống hoàn toàn.

Mỗi Site được thiết kế sẵn với:

```text
site_id
location_id
elevation
structural_condition
water_risk
accessibility
storage_potential
power_access
water_access
living_capacity
security
available_zones
core_components
```

---

## 3. Site Evaluation

Người chơi đánh giá vị trí dựa trên:

- Độ cao.
- Rủi ro ngập.
- Độ bền kết cấu.
- Khoảng cách tài nguyên.
- Tuyến đường.
- Khả năng phòng thủ.
- Nguồn điện.
- Nguồn nước.
- Không gian Module.
- Chất lượng Core Component.
- Khả năng sơ tán.

Không có Shelter Site tốt nhất tuyệt đối.

---

## 4. Shelter Type

### Main Shelter

- Có đầy đủ hệ thống.
- Có Core Component quan trọng.
- Lưu phần lớn tài nguyên.
- Có Module cố định.
- Chi phí chuyển cao.

### Temporary Shelter

- Thiết lập nhanh.
- Ít hoặc không có Core Component.
- Sức chứa thấp.
- Ít Module.
- Dùng cho chuyến đi xa hoặc sơ tán.

### Emergency Shelter

- Chỉ dùng trong thời gian ngắn.
- Không có hệ thống hoàn chỉnh.
- Không có Core Component quan trọng.
- Bảo vệ khỏi Hazard cục bộ.
- Không phù hợp lưu trữ lâu dài.

---

## 5. Discover Site

Shelter Site có thể được phát hiện qua:

- Exploration.
- NPC.
- Map.
- Event.
- Disaster Forecast.
- Information System.

Người chơi không biết đầy đủ chất lượng Site trước khi khảo sát.

---

## 6. Establish Shelter

Thiết lập Shelter mới yêu cầu:

```text
site_secured
basic_living_area
minimum_storage
water_access
safe_entry
```

Core Component của Site sẽ quyết định khả năng phát triển lâu dài.

---

## 7. Relocation Phase

Quá trình di dời gồm:

```text
Decision
↓
Site Preparation
↓
Priority Selection
↓
Transport
↓
Old Shelter Closure
↓
New Shelter Activation
```

---

## 8. Priority Selection

Người chơi không thể luôn mang toàn bộ tài sản.

Phải ưu tiên:

- Người.
- Nước.
- Thức ăn.
- Tool.
- Medicine.
- Blueprint.
- Equipment.
- Portable Module.
- Material.

Core Component không thể mang theo.

---

## 9. Transport

Tài nguyên phải được vận chuyển vật lý.

Phương thức:

- Mang cá nhân.
- Carried Object.
- Xe đẩy.
- Phương tiện.
- Nhiều người cùng mang.
- Nhiều chuyến.

Transport tiêu tốn:

- Thời gian.
- Stamina.
- Nhiên liệu.
- Durability.
- Cơ hội.

---

## 10. Relocation Risk

Rủi ro gồm:

- Route bị chặn.
- Hazard tăng.
- Vật phẩm bị mất.
- NPC bị tách nhóm.
- Shelter cũ hỏng trước khi hoàn tất.
- Shelter mới chưa sẵn sàng.
- Event hết hạn.

---

## 11. Abandoned Shelter

Shelter cũ chuyển thành Location State mới.

Có thể:

- Bị ngập.
- Bị NPC chiếm.
- Bị loot.
- Giữ lại vật phẩm chưa mang.
- Trở thành mục tiêu quay lại.
- Bị phá hủy.

Core Component vẫn tồn tại và có thể trở thành nguồn Event sau này.

---

## 12. Forced Relocation

Forced Relocation xảy ra khi:

- Shelter không còn an toàn.
- Disaster Forecast cho thấy vị trí sẽ bị mất.
- Structural Integrity quá thấp.
- Core Component bị hỏng nghiêm trọng.
- Tuyến tiếp cận sắp bị cắt.

Forced Relocation có ít thời gian chuẩn bị và tổn thất cao hơn.

---

## 13. Multiplayer Rule

- Shelter chính là quyết định của nhóm.
- Người chơi có thể chia đội chuẩn bị Site và vận chuyển.
- World Clock không dừng.
- Storage cũ và mới cùng tồn tại trong thời gian chuyển tiếp.
- Người chơi đang ở hai Shelter vẫn dùng cùng World State.

---

## 14. Phạm vi MVP

Triển khai:

- Một Main Shelter với Core Component cố định.
- Một Temporary Shelter.
- Một Site thay thế.
- Site Evaluation.
- Relocation theo nhiều chuyến.
- Abandoned Shelter State.
- Forced Evacuation.

Chưa triển khai:

- Mạng lưới nhiều căn cứ.
- Fast Travel giữa Shelter.
- Xây Shelter hoàn toàn mới từ đất trống.
- Caravan.
- Quản lý nhiều nhóm độc lập.

---

## 15. Quyết định chốt

- Shelter Site được thiết kế sẵn với cấu trúc và Core Component.
- Người chơi không thể tự do tạo Shelter từ đầu.
- Core Component là nền tảng cho Event và Hazard.
- Relocation là chuỗi hành động vật lý, không phải menu chuyển căn cứ.
- Người chơi không thể luôn mang toàn bộ tài sản.
- Shelter cũ tiếp tục tồn tại trong World State.
- Temporary Shelter hỗ trợ khám phá và sơ tán.
- Forced Relocation là hậu quả gameplay, không phải cutscene.
