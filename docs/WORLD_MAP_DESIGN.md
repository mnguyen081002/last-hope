# World Map Design

## Mục tiêu

Bản đồ không chỉ dùng để tạo cảm giác khám phá.

Vai trò chính của World Map là tạo ra các quyết định:

- Đi đâu?
- Đi theo tuyến nào?
- Có đáng đi xa không?
- Có đủ thời gian quay về không?
- Có nên ghé thêm một địa điểm?
- Có nên mở đường tắt?
- Có nên quay lại địa điểm cũ?

Nguyên tắc cốt lõi:

> **World Map là công cụ tạo quyết định, không chỉ là không gian di chuyển.**

---

# 1. Cấu trúc tổng thể

Thế giới được chia thành:

```text
Thế giới
    ↓
Khu vực
    ↓
Địa điểm chính
    ↓
Điểm phụ và sự kiện trên đường
```

Ví dụ các khu vực:

- Khu dân cư.
- Khu thương mại.
- Khu y tế.
- Khu công nghiệp.
- Khu hạ tầng.
- Khu cứu hộ.
- Khu hành chính.

Mỗi khu vực có đặc điểm riêng về:

- Tài nguyên.
- Phóng xạ.
- Khoảng cách.
- Rủi ro.
- Cấu trúc tuyến đường.
- Giai đoạn phù hợp để khám phá.

---

# 2. Vị trí nơi trú ẩn

Nơi trú ẩn nên nằm gần trung tâm bản đồ.

Điều này giúp:

- Người chơi có nhiều hướng lựa chọn.
- Không có một tuyến đường luôn tối ưu.
- Các khu vực khác nhau cạnh tranh trực tiếp về thời gian.
- Người chơi có thể thực hiện chuyến ngắn hoặc chuyến dài.

Ví dụ sơ đồ tổng quát:

```text
                     [Trạm quan trắc]
                            |
                    [Khu công nghiệp]
                            |
[ Khu dân cư ] — [ Nơi trú ẩn ] — [ Khu thương mại ]
                            |
                      [Khu y tế]
                            |
                      [Khu cứu hộ]
```

Đây chỉ là cấu trúc logic, không phải bố cục level cuối cùng.

---

# 3. Phân loại khoảng cách

Khoảng cách không chỉ là số mét.

Nó thể hiện tổng chi phí của chuyến đi:

- Thời gian di chuyển.
- Liều phóng xạ.
- Hao mòn trang bị.
- Nguy cơ về muộn.
- Khả năng mang loot.
- Thời gian xây dựng bị mất.

## Địa điểm gần

Thời gian một chiều:

> Khoảng 20–40 phút trong game.

Đặc điểm:

- Rủi ro thấp.
- Tài nguyên phổ thông.
- Số lượng hạn chế.
- Phù hợp chuyến đi ngắn.
- Người chơi vẫn còn thời gian làm việc tại nơi trú ẩn.

Ví dụ:

- Nhà dân.
- Cửa hàng nhỏ.
- Xe bị bỏ lại.
- Kho dân cư.

## Địa điểm trung bình

Thời gian một chiều:

> Khoảng 60–90 phút trong game.

Đặc điểm:

- Có tài nguyên chuyên dụng.
- Rủi ro trung bình.
- Chiếm phần lớn một buổi.
- Có thể ghé nhiều địa điểm nếu lập tuyến tốt.

Ví dụ:

- Siêu thị.
- Hiệu thuốc nhỏ.
- Trạm xăng.
- Xưởng sửa chữa.

## Địa điểm xa

Thời gian một chiều:

> Khoảng 120–180 phút trong game.

Đặc điểm:

- Có tài nguyên quý hoặc độc nhất.
- Gần như chiếm cả ngày.
- Yêu cầu trang bị tốt.
- Có nguy cơ không kịp quay về.
- Làm mất phần lớn thời gian xây dựng.

Ví dụ:

- Bệnh viện.
- Trung tâm kỹ thuật.
- Kho cứu trợ.
- Trạm quan trắc.

---

# 4. Chi phí thực của một chuyến đi

Tổng chi phí chuyến đi được tính bằng:

```text
Thời gian đi
+
Thời gian khám phá
+
Thời gian tìm kiếm
+
Thời gian quay về
+
Thời gian khử nhiễm
+
Liều phóng xạ
+
Hao mòn trang bị
+
Cơ hội xây dựng bị mất
```

Do đó, địa điểm có nhiều loot chưa chắc là địa điểm hiệu quả nhất.

Ví dụ:

## Nhà dân

- Đi gần.
- Phóng xạ thấp.
- Ít loot.
- Vẫn còn thời gian xây dựng.

## Trung tâm kỹ thuật

- Đi xa.
- Phóng xạ cao.
- Nhiều linh kiện.
- Gần như mất cả ngày.
- Cần thêm thời gian khử nhiễm.

Cả hai đều có giá trị trong hoàn cảnh khác nhau.

---

# 5. Mỗi địa điểm cần một vai trò rõ ràng

Không nên có địa điểm cung cấp mọi thứ.

Ví dụ:

| Địa điểm           | Vai trò chính                  |
| ------------------ | ------------------------------ |
| Khu nhà dân        | Thức ăn, nước, vật liệu cơ bản |
| Siêu thị           | Nhu yếu phẩm số lượng lớn      |
| Hiệu thuốc         | Thuốc, mặt nạ, bộ lọc          |
| Trạm xăng          | Nhiên liệu, linh kiện          |
| Xưởng sửa chữa     | Kim loại, dụng cụ              |
| Trung tâm kỹ thuật | Linh kiện cao cấp, thiết bị    |
| Trạm quan trắc     | Thông tin về bão               |
| Kho cứu trợ        | Tài nguyên hỗn hợp giá trị cao |

Mỗi nơi nên giải quyết một số vấn đề nhưng đồng thời không giải quyết các vấn đề khác.

---

# 6. Không có địa điểm tốt nhất

Mỗi địa điểm cần có:

- Điểm mạnh.
- Điểm yếu.
- Điều kiện phù hợp.
- Rủi ro đặc trưng.

Ví dụ:

## Siêu thị

### Điểm mạnh

- Nhiều thức ăn.
- Nhiều nước.
- Có thể tìm thấy ba lô.

### Điểm yếu

- Dễ bị vét sạch.
- Nhiều khu vực mở.
- Loot dễ nhiễm bẩn.
- Có thể bị phong tỏa sớm.

## Nhà dân

### Điểm mạnh

- Gần.
- Ít phóng xạ.
- Có nhiều nguồn nhu yếu phẩm nhỏ.

### Điểm yếu

- Mỗi căn có ít loot.
- Không có nhiều tài nguyên kỹ thuật.
- Dễ mất giá trị sau vài chuyến.

## Trung tâm kỹ thuật

### Điểm mạnh

- Nhiều linh kiện.
- Có thiết bị đặc biệt.
- Có thể mở khóa nâng cấp cao cấp.

### Điểm yếu

- Xa.
- Phóng xạ cao.
- Nhiều khu vực khóa.
- Tốn nhiều thời gian khám phá.

---

# 7. Nhiều nguồn cho cùng tài nguyên

Không nên khóa tài nguyên quan trọng vào một địa điểm duy nhất.

Ví dụ bộ lọc có thể tìm thấy tại:

- Hiệu thuốc.
- Trung tâm kỹ thuật.
- Kho cứu trợ.
- Xe cứu thương.
- Một sự kiện cứu hộ.

Nhưng mỗi nguồn có đánh đổi khác nhau.

## Hiệu thuốc

- Xa.
- Bộ lọc gần như đảm bảo.
- Phóng xạ trung bình.
- Cần công cụ mở kho.

## Trạm xăng

- Gần hơn.
- Phóng xạ cao hơn.
- Bộ lọc không đảm bảo.
- Có thêm nhiên liệu và linh kiện.

## Kho cứu trợ

- Rất xa.
- Nhiều bộ lọc.
- Có thể bị NPC hoặc sự kiện lấy mất.

Người chơi phải chọn nguồn phù hợp với tình trạng hiện tại.

---

# 8. Hệ thống tuyến đường

Mỗi địa điểm quan trọng nên có nhiều hơn một tuyến tiếp cận.

Ví dụ đi tới hiệu thuốc:

## Tuyến chính

- Nhanh.
- Phóng xạ cao.
- Ít vật cản.

## Tuyến dân cư

- Chậm hơn.
- Phóng xạ thấp.
- Có thể ghé thêm nhà dân.

## Tuyến hầm

- Nhanh.
- Tối.
- Có nguy cơ sập.
- Cần đèn pin.

Người chơi lựa chọn tuyến dựa trên:

- Thời gian.
- Bảo hộ.
- Liều hiện tại.
- Công cụ.
- Mục tiêu phụ.
- Trạng thái thế giới.

---

# 9. Tuyến nhiều địa điểm

Người chơi có thể lập tuyến ghé nhiều nơi trong một chuyến.

Ví dụ:

```text
Nơi trú ẩn
    ↓
Nhà dân
    ↓
Hiệu thuốc
    ↓
Trạm xăng
    ↓
Nơi trú ẩn
```

Ưu điểm:

- Tiết kiệm thời gian di chuyển.
- Khai thác nhiều nhóm tài nguyên.
- Tận dụng trang bị bảo hộ trong một chuyến.

Nhược điểm:

- Hành trang nhanh đầy.
- Nhận liều phóng xạ cao.
- Khó tính thời gian quay về.
- Trang bị hao mòn nhiều.
- Dễ phải bỏ lại loot hữu ích.

---

# 10. Bản đồ thay đổi theo thời gian

World Map không nên giữ nguyên trong toàn bộ lượt chơi.

Các thay đổi có thể gồm:

- Đường bị chặn.
- Cầu bị sập.
- Khu vực bị nhiễm nặng.
- Một địa điểm bị cướp.
- Một tòa nhà bị cháy.
- Đường tắt được mở.
- Một khu vực mới được phát hiện.
- Đoàn cứu trợ xuất hiện.
- Tín hiệu radio tiết lộ một địa điểm.

Ví dụ vòng đời tuyến đường:

## Ngày 1–2

- Tuyến chính mở.
- Phóng xạ thấp.
- Các địa điểm dễ tiếp cận.

## Ngày 3–4

- Một tuyến bị chặn.
- Phóng xạ tăng.
- Một đường tắt được phát hiện.

## Ngày 5–6

- Khu công nghiệp trở nên nguy hiểm.
- Khu thương mại bị vét nhiều.
- Một kho cứu trợ xuất hiện.

## Ngày cuối

- Một số tuyến không còn an toàn.
- Người chơi phải cân nhắc chuyến đi cuối.
- Thời gian quay về trở thành yếu tố sống còn.

---

# 11. Trạng thái địa điểm trên World Map

Mỗi địa điểm có thể có các trạng thái:

```text
Chưa biết
Đã nghe tin
Đã phát hiện
Có thể tiếp cận
Đã khám phá một phần
Còn khu vực khóa
Gần cạn tài nguyên
Có sự kiện mới
Đã hoàn thành
Bị phong tỏa
Bị phá hủy
```

Không nên chỉ dùng:

```text
Chưa loot
Đã loot
```

Vì một địa điểm có thể hết loot thường nhưng vẫn còn:

- Khu vực khóa.
- NPC.
- Thông tin.
- Thiết bị.
- Đường tắt.
- Mục tiêu sự kiện.

---

# 12. Mở khóa bản đồ

Không cần hiển thị toàn bộ địa điểm ngay từ đầu.

Người chơi có thể mở khóa thông tin bằng:

- Khám phá trực tiếp.
- Sửa radio.
- Tìm bản đồ.
- Gặp NPC.
- Đọc ghi chú.
- Leo lên điểm quan sát.
- Đến trạm quan trắc.
- Mở đường tắt.

Các mức thông tin có thể gồm:

## Chưa biết

Không hiển thị trên bản đồ.

## Nghe tin

Biết vị trí tương đối và loại địa điểm.

## Đã xác định

Biết khoảng cách, tuyến đường và tài nguyên chính.

## Đã khảo sát

Biết mức phóng xạ, lối vào và rủi ro.

Thông tin bản đồ cũng là một loại tài nguyên.

---

# 13. Cấu trúc khu vực đề xuất

## Khu dân cư

Đặc điểm:

- Gần nơi trú ẩn.
- Phóng xạ thấp đến trung bình.
- Nhiều địa điểm nhỏ.
- Nhu yếu phẩm phổ thông.
- Có thể mở đường tắt.

Địa điểm:

- Nhà dân.
- Chung cư.
- Cửa hàng tiện lợi.
- Trường học.
- Nhà kho nhỏ.

## Khu thương mại

Đặc điểm:

- Nhiều thức ăn và nước.
- Giá trị loot cao.
- Dễ bị vét sạch.
- Nhiều không gian mở.

Địa điểm:

- Siêu thị.
- Trung tâm mua sắm.
- Nhà hàng.
- Cửa hàng dụng cụ.

## Khu y tế

Đặc điểm:

- Thuốc và thiết bị y tế.
- Bộ lọc và mặt nạ.
- Phóng xạ hoặc ô nhiễm sinh học cao.

Địa điểm:

- Hiệu thuốc.
- Phòng khám.
- Bệnh viện.
- Xe cứu thương.

## Khu công nghiệp

Đặc điểm:

- Kim loại.
- Linh kiện.
- Dụng cụ.
- Nguy cơ môi trường cao.

Địa điểm:

- Xưởng sửa chữa.
- Nhà máy.
- Kho kỹ thuật.
- Trung tâm điện.

## Khu hạ tầng

Đặc điểm:

- Nhiên liệu.
- Điện.
- Thông tin kỹ thuật.
- Tuyến đường quan trọng.

Địa điểm:

- Trạm xăng.
- Trạm điện.
- Trạm nước.
- Hầm kỹ thuật.

## Khu cứu hộ

Đặc điểm:

- Tài nguyên hỗn hợp.
- NPC.
- Sự kiện.
- Thiết bị đặc biệt.

Địa điểm:

- Kho cứu trợ.
- Trạm cứu hỏa.
- Chốt sơ tán.
- Trung tâm khẩn cấp.

---

# 14. Số lượng địa điểm

Với lượt chơi khoảng 7 ngày chuẩn bị và 8–12 chuyến đi, bản đồ nên có:

- 12–16 địa điểm chính.
- 15–25 điểm phụ.
- 5–7 khu vực lớn.
- Một số sự kiện di động.

Người chơi chỉ nên ghé khoảng:

- 6–10 địa điểm chính trong một lượt.
- Một số điểm phụ trên đường.

Như vậy:

- Không cần khám phá toàn bộ bản đồ.
- Mỗi lượt chơi có tuyến khác nhau.
- Một số địa điểm được để dành cho lượt sau.
- Quyết định chọn nơi nào có ý nghĩa.

---

# 15. Phân cấp địa điểm

## Địa điểm nhỏ

Số lượng đề xuất:

> 6–10 địa điểm.

Đặc điểm:

- Khám phá nhanh.
- Thường dùng một lần.
- Giải quyết nhu cầu tức thời.

Ví dụ:

- Nhà dân.
- Xe cứu thương.
- Cửa hàng nhỏ.
- Chốt kiểm soát.

## Địa điểm trung bình

Số lượng đề xuất:

> 6–8 địa điểm.

Đặc điểm:

- Có nhiều phòng.
- Có khu vực khóa.
- Có thể quay lại.
- Có rủi ro đặc trưng.

Ví dụ:

- Hiệu thuốc.
- Trạm xăng.
- Chung cư.
- Xưởng sửa chữa.

## Địa điểm lớn

Số lượng đề xuất:

> 3–5 địa điểm.

Đặc điểm:

- Có nhiều tầng hoặc khu.
- Có mục tiêu chiến lược.
- Không thể hoàn thành trong một chuyến.
- Có thể thay đổi theo thời gian.

Ví dụ:

- Bệnh viện.
- Siêu thị lớn.
- Trung tâm kỹ thuật.
- Trạm quan trắc.
- Kho cứu trợ.

---

# 16. Điểm phụ trên đường

World Map không chỉ gồm các tòa nhà chính.

Điểm phụ có thể là:

- Xe bị bỏ lại.
- Ba lô của người sống sót.
- Chốt kiểm soát.
- Đống đổ nát.
- Trạm radio tạm.
- Hố phóng xạ.
- Đường hầm.
- Một nhóm người sống sót.
- Tai nạn xe cứu trợ.

Điểm phụ giúp:

- Làm chuyến đi bớt tuyến tính.
- Tạo cơ hội bất ngờ.
- Cho phép thay đổi kế hoạch giữa đường.
- Tạo tài nguyên mới có lý do hợp lý.

---

# 17. Thông tin hiển thị trước chuyến đi

Trước khi chọn địa điểm, người chơi cần thấy:

| Thông tin            | Ý nghĩa                  |
| -------------------- | ------------------------ |
| Khoảng cách          | Chi phí thời gian        |
| Thời gian dự kiến    | Giờ về ước tính          |
| Mức phóng xạ         | Liều dự kiến             |
| Loot chính           | Lý do ghé                |
| Trạng thái           | Đã khám phá hay chưa     |
| Điều kiện            | Dụng cụ hoặc tuyến đường |
| Nguy cơ phong tỏa    | Giá trị của việc đi sớm  |
| Sức chứa khuyến nghị | Khả năng mang loot       |

Không nên hiển thị toàn bộ loot chính xác.

Người chơi chỉ cần đủ thông tin để đưa ra quyết định.

---

# 18. Multiplayer trên World Map

Trong multiplayer, người chơi có thể:

- Đi cùng nhau.
- Chia đội.
- Một nhóm khám phá.
- Một nhóm ở nhà xây dựng.
- Gặp lại nhau tại một địa điểm.

World Map cần theo dõi:

- Vị trí từng người.
- Tuyến đường từng nhóm.
- Thời gian dự kiến.
- Trạng thái địa điểm.
- Loot chung.
- Sự kiện đang diễn ra.

Loot không nhân theo số người.

Đội đông có lợi thế:

- Tìm kiếm nhanh.
- Mang nhiều đồ.
- Xử lý rủi ro tốt.

Nhưng tiêu thụ nhiều hơn:

- Thức ăn.
- Nước.
- Bộ lọc.
- Trang bị.
- Thời gian khử nhiễm.

---

# 19. Tiêu chí đánh giá World Map

World Map đạt yêu cầu khi:

- Không có một tuyến đường luôn tốt nhất.
- Không có một địa điểm luôn được chọn đầu tiên.
- Mỗi tài nguyên quan trọng có ít nhất hai nguồn.
- Người chơi không thể khám phá toàn bộ bản đồ trong một lượt.
- Khoảng cách tạo đánh đổi với thời gian xây dựng.
- Người chơi có lý do mở đường tắt.
- Một số địa điểm thay đổi theo thời gian.
- Người chơi có thể thay đổi kế hoạch giữa chuyến đi.
- Những địa điểm đã cạn giá trị được thể hiện rõ.
- Lượt chơi sau có thể sử dụng tuyến khác.

---

# 20. Dữ liệu cần theo dõi khi playtest

- Địa điểm được chọn trong từng ngày.
- Tuyến đường được sử dụng.
- Thời gian trung bình mỗi chuyến.
- Liều phóng xạ nhận được.
- Số địa điểm ghé trong một chuyến.
- Địa điểm không bao giờ được chọn.
- Địa điểm luôn được ưu tiên.
- Đường tắt có được mở hay không.
- Người chơi có về muộn không.
- Người chơi có quay lại địa điểm cũ không.
- Lý do quay lại.
- Số địa điểm đã khám phá khi bão tới.

---

# 21. Quy tắc khóa cho World Map

1. Nơi trú ẩn nằm gần trung tâm.
2. Bản đồ chia thành khu vực có bản sắc tài nguyên riêng.
3. Mỗi tài nguyên quan trọng có nhiều nguồn.
4. Khoảng cách là tổng chi phí, không chỉ là thời gian đi bộ.
5. Mỗi địa điểm có vai trò và điểm yếu rõ ràng.
6. Loot không tự hồi sinh.
7. Địa điểm có thể thay đổi do thế giới và sự kiện.
8. Một số địa điểm dùng một lần, một số có nhiều lớp khám phá.
9. Người chơi không thể ghé mọi nơi trong một lượt.
10. Tuyến đường và đường tắt là một phần của gameplay.
11. Thông tin bản đồ có giá trị chiến lược.
12. World Map phải hỗ trợ cả đi đơn, đi nhóm và chia đội.
