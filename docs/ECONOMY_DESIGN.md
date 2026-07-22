# Economy Design

## Mục tiêu

Economy phải tạo ra cảm giác:

> **“Mình luôn thiếu một chút.”**

Không phải:

- Luôn thiếu tất cả.
- Hoặc luôn dư tất cả.

Người chơi phải luôn có thứ cần ưu tiên.

---

# 1. Phân loại tài nguyên

Thay vì có 50 loại item ngay từ đầu, MVP chỉ nên có khoảng **10–15 loại tài nguyên cốt lõi**.

## A. Nhu yếu phẩm

| Tài nguyên | Công dụng                 |
| ---------- | ------------------------- |
| Thức ăn    | Duy trì sinh tồn          |
| Nước       | Uống, khử nhiễm, điều trị |

Đây là tài nguyên tiêu hao mỗi ngày.

---

## B. Vật liệu xây dựng

| Tài nguyên       | Công dụng              |
| ---------------- | ---------------------- |
| Gỗ               | Gia cố, sửa chữa       |
| Kim loại         | Công trình, thiết bị   |
| Vật liệu bịt kín | Tăng độ kín nơi trú ẩn |

---

## C. Linh kiện kỹ thuật

| Tài nguyên | Công dụng                          |
| ---------- | ---------------------------------- |
| Linh kiện  | Máy lọc, radio, máy phát, sửa chữa |
| Bộ lọc     | Mặt nạ, máy lọc khí                |
| Nhiên liệu | Máy phát điện                      |

---

## D. Y tế

| Tài nguyên           | Công dụng                  |
| -------------------- | -------------------------- |
| Thuốc                | Hồi máu, điều trị          |
| Thuốc chống phóng xạ | Giảm tác động của phóng xạ |

---

## E. Trang bị

Không tiêu hao trực tiếp:

- Mặt nạ.
- Bộ đồ bảo hộ.
- Ba lô.
- Dụng cụ.

---

# 2. Dòng chảy tài nguyên

Mỗi tài nguyên cần trả lời 4 câu hỏi.

Ví dụ với **Nước**:

### Nguồn

- Nhà dân.
- Siêu thị.
- Xe cứu hộ.

### Tiêu hao

- Uống.
- Khử nhiễm.
- Điều trị.

### Thiếu nước

- Mất máu.
- Không thể khử nhiễm hiệu quả.
- Không thể điều trị tốt.

### Dư nước

- Có thể dự trữ.
- Không mang được quá nhiều vì chiếm sức chứa.

Làm tương tự với tất cả tài nguyên.

---

# 3. Nguyên tắc cân bằng

Mỗi tài nguyên quan trọng phải có:

- Ít nhất **2 nguồn**.
- Ít nhất **2 công dụng**.

Ví dụ:

## Linh kiện

### Nguồn

- Trung tâm kỹ thuật.
- Trạm điện.

### Công dụng

- Radio.
- Máy lọc.
- Máy phát.
- Sửa đồ.

Không bao giờ có “linh kiện chỉ để sửa radio”.

---

# 4. Không để mọi tài nguyên đều khan hiếm

Nếu mọi thứ đều hiếm, người chơi không thể đưa ra quyết định.

Ví dụ:

- Thức ăn: hơi thiếu.
- Nước: vừa đủ.
- Gỗ: khá nhiều.
- Kim loại: hơi hiếm.
- Linh kiện: hiếm.
- Bộ lọc: rất hiếm.

Mỗi loại có mức độ khan hiếm khác nhau để tạo ưu tiên.

---

# 5. Chu trình tài nguyên

Một vòng lặp lý tưởng:

```text
Khám phá
    ↓
Mang tài nguyên về
    ↓
Xử lý (khử nhiễm nếu cần)
    ↓
Tiêu hao hoặc xây dựng
    ↓
Thiếu tài nguyên khác
    ↓
Lập kế hoạch chuyến đi mới
```

Nếu chu trình này bị đứt, ví dụ người chơi không bao giờ thiếu nước nữa, gameplay sẽ mất động lực.

---

# 6. Mức tiêu hao

Thay vì con số cụ thể ngay, hãy dùng mức tương đối.

| Tài nguyên           | Mức tiêu hao |
| -------------------- | ------------ |
| Thức ăn              | Cao          |
| Nước                 | Cao          |
| Thuốc                | Thấp         |
| Thuốc chống phóng xạ | Rất thấp     |
| Gỗ                   | Trung bình   |
| Kim loại             | Trung bình   |
| Linh kiện            | Thấp         |
| Bộ lọc               | Trung bình   |
| Nhiên liệu           | Trung bình   |

Sau này playtest mới chuyển thành số.

---

# 7. Loot không nên hoàn toàn ngẫu nhiên

Mỗi địa điểm nên có:

- **Loot đảm bảo**, phù hợp loại địa điểm.
- **Loot ngẫu nhiên**.
- **Loot hiếm**.

Ví dụ hiệu thuốc:

- Chắc chắn có thuốc.
- Có thể có bộ lọc.
- Hiếm khi có bộ đồ bảo hộ.

Điều này giúp người chơi lập kế hoạch thay vì cầu may.

---

# 8. Công trình cũng là “bể tiêu hao”

Nâng cấp nơi trú ẩn phải tiêu thụ tài nguyên đáng kể.

Ví dụ máy lọc khí cần:

- Kim loại.
- Linh kiện.
- Bộ lọc.

Sau đó còn cần:

- Điện.
- Bộ lọc thay thế.

Như vậy công trình tạo ra chi phí lâu dài, không chỉ chi phí xây ban đầu.

---

# 9. Nguyên tắc quan trọng nhất

Một tài nguyên chỉ thực sự có giá trị khi người chơi phải từ bỏ thứ khác để lấy nó.

Ví dụ:

- Mang thêm 5 chai nước → bớt chỗ cho kim loại.
- Mang thêm bộ lọc → không mang được nhiều thức ăn.
- Mang bộ đồ bảo hộ → giảm sức chứa.

Những đánh đổi này mới tạo nên quyết định thú vị.
