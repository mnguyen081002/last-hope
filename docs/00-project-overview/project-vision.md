# Last Hope — Project Vision

## 1. Tổng quan

**Last Hope** là game sinh tồn theo campaign tuyến tính, trong đó người chơi bắt đầu từ một thế giới bình thường và phải thích nghi khi các thảm họa ngày càng nghiêm trọng liên tiếp xảy ra.

Mỗi Chapter tập trung vào một Disaster riêng, nhưng các quyết định, mối quan hệ, kiến thức và trạng thái thế giới có thể tiếp tục ảnh hưởng đến những Chapter sau.

Chapter đầu tiên và cũng là phạm vi MVP:

```text
Siêu Bão Mưa Đen
```

---

## 2. Player Fantasy

Người chơi không phải anh hùng có năng lực đặc biệt.

Người chơi là một người sống sót bình thường phải:

- Quan sát dấu hiệu bất thường.
- Thu thập thông tin.
- Lựa chọn tài nguyên cần ưu tiên.
- Khám phá trong điều kiện nguy hiểm.
- Chuẩn bị Shelter.
- Hợp tác với NPC hoặc người chơi khác.
- Sống sót qua thời điểm Disaster đạt đỉnh.

Player Fantasy cốt lõi:

> Biến một nơi trú ẩn mong manh thành cơ hội sống sót bằng sự chuẩn bị, hiểu biết và những quyết định khó khăn.

---

## 3. Trải nghiệm cốt lõi

Last Hope tập trung vào bốn trải nghiệm:

### Khám phá trong áp lực

Mỗi chuyến đi tiêu tốn:

- Thời gian.
- Thể lực.
- Trang bị.
- Sức chứa.
- Cơ hội thực hiện công việc tại Shelter.

Người chơi phải liên tục quyết định tiếp tục khám phá hay quay về.

### Quản lý tài nguyên có đánh đổi

Tài nguyên quan trọng phải có nhiều công dụng.

Ví dụ:

```text
Fuel
→
Generator
OR
Water Pump
OR
Water Purifier
```

Người chơi không thể tối ưu mọi hệ thống trong một lượt chơi.

### Chuẩn bị Shelter

Shelter là trung tâm sinh tồn có trạng thái thực tế.

Người chơi phải:

- Xây Module trong các Zone được cho phép.
- Vận hành thiết bị.
- Bảo trì hệ thống.
- Bảo vệ tài nguyên.
- Ứng phó với Hazard và Event.

Shelter được thiết kế sẵn với:

```text
Fixed Core Components
+
Predefined Zones
+
Buildable Slots / Areas
```

Người chơi không xây dựng tự do từ đất trống.

### Thế giới thay đổi theo thời gian

World Clock luôn chạy.

Trong khi người chơi hoạt động:

- Disaster tiếp tục tiến triển.
- Hazard thay đổi.
- Route có thể bị khóa.
- NPC di chuyển.
- Event có thể hết hạn.
- Shelter tiếp tục tiêu thụ tài nguyên.

---

## 4. Cấu trúc Campaign

Campaign diễn ra theo trình tự tuyến tính:

```text
Thế giới bình thường
↓
Dấu hiệu bất thường
↓
Thảm họa đầu tiên
↓
Hậu quả kéo dài
↓
Các thảm họa nghiêm trọng hơn
↓
Nguồn gốc chung dần được hé lộ
```

Campaign có thể tuyến tính về thứ tự Chapter, nhưng chiến lược sinh tồn trong mỗi Chapter phải phi tuyến.

Người chơi có thể lựa chọn:

- Location cần khám phá.
- NPC cần cứu.
- Module cần xây.
- Tài nguyên cần bảo vệ.
- Shelter cần duy trì hoặc từ bỏ.
- Mức độ rủi ro có thể chấp nhận.

---

## 5. Cấu trúc Disaster Chapter

Mỗi Chapter tuân theo cấu trúc chung:

```text
Normal
↓
Warning
↓
Escalation
↓
Peak
↓
Aftermath
```

### Normal

- Giới thiệu thế giới trước thảm họa.
- Thiết lập NPC, Location và Shelter.
- Gieo dấu hiệu bất thường.

### Warning

- Cho phép người chơi chuẩn bị.
- Cung cấp thông tin chưa hoàn chỉnh.
- Tạo các quyết định ưu tiên đầu tiên.

### Escalation

- Hazard tăng.
- Route và Location thay đổi.
- Tài nguyên trở nên khó tiếp cận.

### Peak

- Kiểm tra toàn bộ quá trình chuẩn bị.
- Hạn chế khả năng sửa sai bằng chuyến đi mới.
- Shelter và Resource Flow trở thành trọng tâm.

### Aftermath

- Đánh giá hậu quả.
- Cập nhật Persistent World State.
- Chuyển tiến trình sang Chapter tiếp theo.

---

## 6. Trụ cột thiết kế

```text
Exploration Under Pressure
Resource Trade-offs
Shelter Preparation
Information-Driven Decisions
Persistent Consequences
```

Mọi hệ thống và nội dung phải phục vụ ít nhất một trụ cột.

---

## 7. Câu hỏi chiến lược trung tâm

> Tôi nên mạo hiểm thêm bao nhiêu hôm nay để tăng khả năng sống sót khi thảm họa đạt đỉnh?

Câu hỏi này phải xuất hiện xuyên suốt:

- Chuẩn bị chuyến đi.
- Search.
- Carry Load.
- Route Selection.
- Shelter Task.
- Event.
- NPC Rescue.
- Disaster Forecast.

---

## 8. Single-player và Multiplayer

Single-player là phạm vi triển khai ban đầu.

Core Architecture phải hỗ trợ Multiplayer trong tương lai:

- Một World Clock chung.
- Không có đồng hồ riêng cho từng người.
- Không Pause World Clock trong gameplay.
- Không Fast Forward.
- Không Time Skip ngoài giấc ngủ.
- Shelter State và World State được chia sẻ.
- Player Condition và Inventory được quản lý riêng.
- Người chơi có thể chia vai trò giữa Exploration và Shelter.

MVP chưa triển khai networking hoàn chỉnh.

---

## 9. Định hướng dài hạn

Các Chapter sau có thể tập trung vào:

- Lũ và biến đổi địa hình.
- Thảm họa khí hậu bất thường.
- Thiên thạch.
- Phóng xạ.
- Sinh vật hoặc dịch bệnh không xác định.
- Sự cố quy mô toàn cầu.
- Xâm lược ngoài Trái Đất.

Mỗi Disaster phải tạo gameplay riêng nhưng sử dụng chung các framework:

- Time.
- Hazard.
- Shelter.
- Information.
- Event.
- Resource Flow.
- Progression.
- Outcome.

---

## 10. Tuyên bố tầm nhìn

> Last Hope là game sinh tồn nơi chiến thắng không đến từ may mắn hoặc sức mạnh vượt trội, mà đến từ khả năng quan sát, chuẩn bị và chấp nhận hậu quả của những lựa chọn không hoàn hảo.
