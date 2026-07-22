# Core Loop Specification

Đây là tài liệu quan trọng nhất của game.

Nếu Game Design Pillars trả lời:

> **"Game muốn mang lại trải nghiệm gì?"**

thì Core Loop trả lời:

> **"Trong 1 phút, 10 phút và 1 ngày, người chơi đang làm gì?"**

Nếu Core Loop tốt thì gần như toàn bộ hệ thống sau này đều xoay quanh nó.

---

# Mục tiêu của Core Loop

Người chơi luôn lặp lại vòng sau:

```text
Đánh giá tình hình
        ↓
Lập kế hoạch
        ↓
Chuẩn bị
        ↓
Ra ngoài
        ↓
Khám phá & thu thập
        ↓
Quyết định tiếp tục hay quay về
        ↓
Khử nhiễm
        ↓
Quản lý nơi trú ẩn
        ↓
Chuẩn bị ngày tiếp theo
```

Lưu ý:

> Không phải mọi ngày đều giống nhau.

Có ngày:

- Chỉ ở nhà xây dựng.

Có ngày:

- Chỉ đi một chuyến ngắn.

Có ngày:

- Đánh cược chuyến đi cuối cùng trước bão.

Core Loop phải cho phép cả ba.

---

# Chi tiết từng bước

## 1. Đánh giá tình hình

Ngay khi bắt đầu ngày, người chơi trả lời:

**Hiện tại mình đang ở trạng thái nào?**

Thông tin hiển thị:

- Còn bao nhiêu ngày trước bão.
- Dự báo mới.
- Liều phóng xạ.
- Máu.
- Đói/khát.
- Độ bền mặt nạ.
- Bộ lọc còn bao lâu.
- Máy lọc còn hoạt động không.
- Máy phát còn nhiên liệu không.
- Thiếu tài nguyên gì.

Không có quyết định nào ở bước này.

Chỉ là thu thập thông tin.

---

## 2. Lập kế hoạch

Đây là bước quan trọng nhất.

Ví dụ:

> Hôm nay thiếu nước.

↓

Đi siêu thị?

Hay nhà dân?

Hay sửa máy lọc trước?

↓

Nếu đi xa sẽ không đủ thời gian xây cửa.

↓

Nếu ở nhà thì thiếu nước.

↓

Người chơi quyết định.

Game **không nên gợi ý đáp án đúng**.

---

## 3. Chuẩn bị

Người chơi chọn:

- Mặt nạ nào.
- Bộ lọc.
- Thuốc.
- Vũ khí.
- Dụng cụ.
- Ba lô.
- Chừa bao nhiêu chỗ cho loot.

Đây là bước tạo ra rất nhiều quyết định nhỏ.

---

## 4. Ra ngoài

Bắt đầu chuyến đi.

Ở đây gameplay chuyển sang khám phá thời gian thực.

Người chơi:

- Di chuyển.
- Quan sát.
- Chọn đường.
- Tránh khu nguy hiểm.

---

## 5. Khám phá

Gameplay chính.

Người chơi:

- Mở cửa.
- Tìm vật chứa.
- Phá khóa.
- Thu thập.
- Kiểm tra phóng xạ.
- Gặp sự kiện.

Quan trọng:

**Không có "tìm kiếm miễn phí".**

Muốn tìm phải:

- Đứng lại.
- Mất thời gian.
- Nhận thêm phóng xạ.

---

## 6. Quyết định tiếp tục?

Đây là lúc game bắt đầu căng.

Ví dụ:

```text
18:20

Bộ lọc còn 25%

Ba lô còn 4 ô

Phía trước còn một cửa hàng

Về nhà mất 1 giờ
```

Người chơi phải chọn:

- Quay về.

Hoặc:

- Liều thêm.

Đây là khoảnh khắc quan trọng nhất của gameplay.

---

## 7. Trở về

Về không có nghĩa là kết thúc.

Người chơi còn phải:

- Khử nhiễm.
- Phân loại loot.
- Sửa đồ.
- Điều trị.

Đây là "chi phí sau chuyến đi".

---

## 8. Quản lý nơi trú ẩn

Sau khi xử lý loot, người chơi dùng tài nguyên để:

- Xây.
- Sửa.
- Nâng cấp.
- Dự trữ.
- Xử lý sự cố.

Đây là lúc loot biến thành khả năng sống sót.

---

## 9. Chuẩn bị ngày sau

Nếu còn thời gian:

- Sắp xếp đồ.
- Chế tạo.
- Kiểm tra dự báo.
- Chuẩn bị hành trang.

↓

Ngủ.

↓

Ngày mới.

---

# Điều quan trọng nhất của Core Loop

Có một vòng lặp lớn hơn mà ta chưa nói rõ:

```text
Chuẩn bị

↓

Mạo hiểm

↓

Mang tài nguyên về

↓

Biến tài nguyên thành khả năng sống sót

↓

Đối mặt với cơn bão

↓

Đánh giá kết quả

↓

Lượt chơi mới
```

Đây gọi là **Meta Loop**.

Nó bao trùm toàn bộ game.

---

# Hai vòng lặp cần tồn tại

## Micro Loop (30 giây – 2 phút)

```text
Di chuyển

↓

Tìm vật chứa

↓

Quyết định lấy gì

↓

Tiếp tục
```

Đây là vòng lặp diễn ra liên tục khi khám phá.

---

## Macro Loop (10–20 phút)

```text
Ra ngoài

↓

Thu thập

↓

Quay về

↓

Xây dựng

↓

Chuẩn bị

↓

Ra ngoài
```

Đây là vòng lặp của một ngày.

---

# Meta Loop (Toàn bộ lượt chơi)

```text
Ngày 1

↓

Ngày 2

↓

Ngày 3

↓

...

↓

Bão

↓

Thắng / Thua

↓

Lượt chơi mới
```
