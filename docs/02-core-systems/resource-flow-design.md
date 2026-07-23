# Resource Flow Design

## Mục tiêu

Resource Flow Design xác định cách tài nguyên di chuyển trong toàn bộ vòng đời của một lượt chơi.

Tài liệu này không mô tả từng địa điểm có bao nhiêu loot, mà trả lời các câu hỏi:

- Tài nguyên xuất hiện từ đâu?
- Người chơi lấy tài nguyên như thế nào?
- Tài nguyên được tiêu hao ra sao?
- Khi nào người chơi buộc phải ra ngoài?
- Thế giới thay đổi như thế nào theo thời gian?
- Vì sao người chơi quay lại một địa điểm?

---

# Mục tiêu thiết kế

Hệ thống Resource Flow phải đảm bảo:

- Người chơi luôn thiếu một số tài nguyên quan trọng.
- Không có địa điểm nào trở thành nơi "farm" vô hạn.
- Thế giới luôn thay đổi theo thời gian.
- Người chơi luôn có lý do để đưa ra quyết định.
- Loot không hồi sinh một cách vô lý.

---

# Nguyên tắc thiết kế

## 1. Loot không tự hồi sinh

Một vật phẩm đã bị lấy khỏi thế giới sẽ không tự xuất hiện lại.

Ví dụ:

- Nhà dân đã lấy hết đồ hộp.
- Hai ngày sau sẽ không tự xuất hiện thêm đồ hộp.

Điều này giúp thế giới có tính chân thực.

---

## 2. Thế giới vẫn tiếp tục vận động

Người chơi không phải thực thể duy nhất ảnh hưởng đến thế giới.

Trong khi người chơi đang ở nơi trú ẩn, thế giới vẫn có thể thay đổi.

Ví dụ:

- Người sống sót khác loot một cửa hàng.
- Đoàn cứu trợ đi qua.
- Một tòa nhà bị sập.
- Xe cứu thương gặp tai nạn.
- Một tuyến đường bị phong tỏa.
- Một kho hàng mới được phát hiện.

---

## 3. Địa điểm thay đổi, không phải loot hồi sinh

Lý do người chơi quay lại một địa điểm không phải vì loot xuất hiện lại.

Người chơi quay lại vì:

- Có khu vực mới được mở.
- Có sự kiện mới.
- Có NPC mới.
- Có tuyến đường mới.
- Có tài nguyên mới do sự kiện mang tới.
- Có mục tiêu mới.

---

## 4. Mỗi tài nguyên phải tạo ra quyết định

Một tài nguyên quan trọng phải có nhiều công dụng.

Ví dụ:

### Nước

Có thể dùng để:

- Uống.
- Khử nhiễm.
- Điều trị.
- Dự trữ trước bão.

### Linh kiện

Có thể dùng để:

- Sửa radio.
- Sửa máy phát.
- Sửa máy lọc.
- Chế tạo.
- Nâng cấp nơi trú ẩn.

Không nên có tài nguyên chỉ dùng cho một mục đích duy nhất.

---

# Dòng chảy tài nguyên

```text
Thế giới
        ↓
Người chơi phát hiện
        ↓
Thu thập
        ↓
Mang về nơi trú ẩn
        ↓
Kiểm tra nhiễm bẩn
        ↓
Khử nhiễm hoặc cách ly
        ↓
Lưu kho
        ↓
Tiêu dùng / Xây dựng / Chế tạo
        ↓
Tiêu hao hoặc hỏng
        ↓
Phát sinh nhu cầu mới
        ↓
Lập kế hoạch chuyến đi tiếp theo
```

Đây là vòng tuần hoàn chính của toàn bộ gameplay.

---

# Phân loại tài nguyên

## Tài nguyên hữu hạn

Đây là các tài nguyên gần như không thể tạo thêm.

Ví dụ:

- Đồ hộp.
- Thuốc.
- Thuốc chống phóng xạ.
- Linh kiện.
- Bộ lọc.
- Pin.
- Nhiên liệu.

Vai trò:

- Tạo áp lực dài hạn.
- Buộc người chơi phải lập kế hoạch.
- Tăng giá trị của mỗi chuyến đi.

---

## Tài nguyên tái tạo

Có thể tạo ra sau khi đầu tư.

Ví dụ:

- Hứng nước mưa.
- Trồng rau.
- Pin mặt trời.

Đặc điểm:

- Cần xây dựng.
- Cần bảo trì.
- Không miễn phí.
- Không thể thay thế hoàn toàn việc khám phá.

---

## Tài nguyên xuất hiện do thế giới thay đổi

Không phải respawn.

Ví dụ:

- Xe cứu trợ gặp nạn.
- Kho hàng bị lộ sau vụ sập.
- Người sống sót bỏ lại ba lô.
- Cửa kho trước đây bị khóa nay mở được.
- Nhận tọa độ mới qua radio.

Đây là cách bổ sung tài nguyên hợp lý mà không phá vỡ tính chân thực.

---

# Các tác nhân tiêu thụ tài nguyên

## Người chơi

Tiêu hao để:

- Ăn.
- Uống.
- Điều trị.
- Bảo hộ.
- Khám phá.

---

## Nơi trú ẩn

Tiêu hao để:

- Máy lọc hoạt động.
- Máy phát chạy.
- Sửa chữa.
- Nâng cấp.
- Duy trì độ kín.

Nơi trú ẩn phải là một "bể tiêu hao" liên tục.

---

## Thời gian

Một số tài nguyên giảm giá trị theo thời gian.

Ví dụ:

- Thức ăn tươi bị hỏng.
- Pin tự hao.
- Bộ lọc giảm hiệu quả.
- Thiết bị xuống cấp.

Không phải mọi tài nguyên đều cần cơ chế này.

---

## Thế giới

Thế giới cũng làm mất tài nguyên.

Ví dụ:

- Người sống sót khác lấy mất.
- Cháy kho hàng.
- Sập công trình.
- Mưa làm ô nhiễm.
- Phóng xạ tăng.

Điều này khiến người chơi không thể trì hoãn mọi quyết định.

---

# Hệ thống thế giới động

Last Hope sử dụng mô hình:

> **Dynamic World (Controlled)**

Thế giới thay đổi nhưng nằm trong phạm vi được thiết kế trước.

Không mô phỏng hàng trăm NPC.

Chỉ cần hệ thống sự kiện và trạng thái địa điểm.

---

## Mục tiêu

Tạo cảm giác:

- Người chơi không cô độc.
- Thế giới không đứng yên.
- Thời gian có giá trị.
- Thông tin có giá trị.

---

## Các thay đổi có thể xảy ra

Một địa điểm có thể:

- Bị loot một phần.
- Bị chiếm.
- Có vật tư cứu trợ.
- Bị cháy.
- Bị sập.
- Mở khu vực mới.
- Tăng phóng xạ.
- Trở thành tuyến đường mới.
- Xuất hiện NPC.

---

# Vòng đời của địa điểm

```text
Unknown
        ↓
Discovered
        ↓
Visited
        ↓
Partially Looted
        ↓
Main Loot Depleted
        ↓
World Event
        ↓
New Objective
        ↓
Changed
        ↓
Destroyed / Inaccessible
```

Địa điểm không chỉ có hai trạng thái "còn loot" hoặc "hết loot".

---

# Vì sao người chơi quay lại?

Không phải vì loot hồi sinh.

Các lý do hợp lệ:

- Có công cụ mới.
- Có thông tin mới.
- Có khu vực mới.
- Có NPC mới.
- Có sự kiện mới.
- Có đường đi mới.
- Có mục tiêu mới.
- Có vật tư mới do sự kiện mang tới.

---

# Vòng đời của tài nguyên

Ví dụ với bộ lọc:

```text
Tồn tại trong thế giới
        ↓
Được tìm thấy
        ↓
Được mang về
        ↓
Lưu kho
        ↓
Lắp vào mặt nạ
        ↓
Giảm hiệu quả
        ↓
Hỏng
        ↓
Thải bỏ
```

Ví dụ với nước:

```text
Nguồn nước
        ↓
Thu thập
        ↓
Lọc
        ↓
Lưu trữ
        ↓
Uống / Khử nhiễm
        ↓
Hết
```

Mỗi tài nguyên quan trọng đều nên có vòng đời rõ ràng.

---

# Điều gì buộc người chơi ra ngoài?

Người chơi không nên có thể ở yên trong nơi trú ẩn từ đầu đến cuối.

Các động lực khám phá:

- Thiếu thức ăn.
- Thiếu nước.
- Thiếu bộ lọc.
- Máy phát sắp hết nhiên liệu.
- Thiếu linh kiện.
- Công trình chưa hoàn thành.
- Có tín hiệu cứu trợ.
- Có kho hàng mới.
- Có sự kiện giới hạn thời gian.

Ở nhà vẫn là lựa chọn hợp lệ, nhưng không thể duy trì mãi.

---

# Thông tin cũng là tài nguyên

Thông tin giúp người chơi:

- Biết địa điểm nào còn tài nguyên.
- Biết tuyến đường bị chặn.
- Biết nơi nào vừa có cứu trợ.
- Biết bão thay đổi.
- Biết khu vực nào sắp nguy hiểm.

Nguồn thông tin:

- Radio.
- Người sống sót.
- Ghi chú.
- Bản đồ.
- Trạm quan trắc.

Thông tin tốt giúp giảm các chuyến đi lãng phí.

---

# Phân phối tài nguyên

Không thiết kế theo hướng:

```text
Địa điểm
        ↓
Loot
```

Mà theo hướng:

```text
Nhu cầu toàn lượt chơi
        ↓
Tổng lượng tài nguyên cần
        ↓
Dự phòng cho nhiều chiến lược
        ↓
Phân bổ theo khu vực
        ↓
Phân bổ theo địa điểm
        ↓
Phân bổ xuống từng Search Point
```

Đây là cách cân bằng gameplay từ trên xuống.

---

# Quy tắc phân phối

- Mỗi tài nguyên quan trọng có ít nhất 2 nguồn.
- Không địa điểm nào độc quyền tài nguyên sống còn.
- Không cần khám phá toàn bộ bản đồ để thắng.
- Mỗi lượt chơi sẽ bỏ lỡ một phần nội dung.

Điều này tạo replayability.

---

# Quy tắc cho mỗi chuyến đi

Mỗi chuyến đi phải trả lời được ba câu hỏi:

1. Người chơi đi vì mục tiêu gì?
2. Nếu thất bại sẽ mất gì?
3. Nếu thành công sẽ thay đổi kế hoạch sinh tồn như thế nào?

Nếu không trả lời được ba câu hỏi này thì chuyến đi chưa đủ ý nghĩa.

---

# Quy tắc cân bằng

Thế giới động không được biến thành RNG.

Luôn phải đảm bảo:

- Có nhiều nguồn cho tài nguyên sống còn.
- Không sự kiện nào làm game không thể thắng.
- Người chơi luôn có cơ hội dự đoán thông qua thông tin.
- Thất bại đến từ quyết định nhiều hơn từ may rủi.

---

# Phạm vi MVP

MVP chỉ cần:

- Hệ thống trạng thái địa điểm.
- Loot không hồi sinh.
- Sự kiện theo ngày.
- Sự kiện theo điều kiện.
- Một số sự kiện ngẫu nhiên có kiểm soát.
- Địa điểm thay đổi theo thời gian.
- NPC chỉ tồn tại dưới dạng sự kiện.
- Không mô phỏng thế giới đầy đủ.

---

# Quyết định đã chốt

- Sử dụng **Dynamic World (Controlled)**.
- Loot không respawn.
- Địa điểm có nhiều trạng thái.
- Thế giới tiếp tục thay đổi khi người chơi không có mặt.
- Người chơi quay lại vì mục tiêu mới, không phải vì loot mới.
- Mỗi tài nguyên có nhiều công dụng.
- Thông tin là một loại tài nguyên.
- Resource Flow được thiết kế từ **nhu cầu của cả lượt chơi**, sau đó mới phân bổ xuống từng địa điểm.
