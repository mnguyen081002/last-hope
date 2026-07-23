# Location System Design

## Mục tiêu

Location System Design xác định cấu trúc chung cho mọi địa điểm trong Last Hope.

Tài liệu này không thiết kế chi tiết từng địa điểm cụ thể như hiệu thuốc, siêu thị hoặc trạm xăng.

Mục tiêu là tạo ra một bộ quy tắc chung để mọi địa điểm:

- Có vai trò rõ ràng.
- Có thời lượng khám phá dự kiến.
- Có rủi ro riêng.
- Có trạng thái trước và sau khi bị loot.
- Có lý do hợp lý để quay lại.
- Có thể thay đổi theo thế giới động.
- Không trở thành một chiếc hộp chứa loot dùng một lần.

---

# Định nghĩa địa điểm

Một địa điểm trong Last Hope không chỉ là một tòa nhà.

Nó là một đơn vị gameplay bao gồm:

```text
Không gian
+
Tài nguyên
+
Rủi ro
+
Thời gian
+
Thông tin
+
Quyết định
+
Trạng thái thế giới
```

Ví dụ, hiệu thuốc không chỉ có chức năng cung cấp thuốc.

Nó còn có thể tạo ra các quyết định:

- Phá cửa chính hay tìm lối vào phía sau.
- Dùng công cụ để mở kho hay quay lại sau.
- Tiếp tục tìm kiếm hay rời đi trước khi liều phóng xạ quá cao.
- Cứu người bị thương hay giữ lại thuốc.
- Mang thuốc về hay dành chỗ cho bộ lọc hiếm.

---

# Cấu trúc phân cấp

Mỗi địa điểm được chia thành ba cấp:

```text
Location
    ↓
Zone
    ↓
Search Point
```

---

## Location

Location là toàn bộ địa điểm.

Ví dụ:

```text
Hiệu thuốc
```

Location quản lý:

- Trạng thái tổng thể.
- Mức phóng xạ.
- Tình trạng loot.
- Sự kiện hiện tại.
- Nhóm NPC đang chiếm giữ.
- Lối vào và lối thoát.
- Các Zone bên trong.

---

## Zone

Zone là các khu vực lớn bên trong một Location.

Ví dụ tại hiệu thuốc:

- Khu bán hàng.
- Quầy thuốc.
- Phòng nhân viên.
- Kho phía sau.
- Tầng hầm.
- Mái nhà.

Mỗi Zone có thể có:

- Rủi ro riêng.
- Loot riêng.
- Điều kiện tiếp cận.
- Search Point riêng.
- Trạng thái riêng.

---

## Search Point

Search Point là điểm tương tác cụ thể.

Ví dụ:

- Ngăn kéo.
- Tủ thuốc.
- Két khóa.
- Tủ lạnh y tế.
- Ba lô.
- Thùng hàng.
- Tủ nhân viên.

Search Point là nơi người chơi thực hiện hành động tìm kiếm theo thời gian thực.

---

# Target Playtime

Mỗi địa điểm phải có một Target Playtime.

Target Playtime là thời gian chơi thực mà một người chơi trung bình cần để hoàn thành mục tiêu chính của địa điểm trong lần đầu ghé thăm.

Không nên chỉ mô tả:

> Đây là một địa điểm nhỏ.

Nên xác định rõ:

> Người chơi cần khoảng bao lâu để khám phá và hoàn thành mục tiêu chính?

---

## Thời lượng đề xuất

| Loại địa điểm      |    Lần đầu | Lần quay lại |
| ------------------ | ---------: | -----------: |
| Nhà dân nhỏ        |   4–7 phút |     1–3 phút |
| Cửa hàng nhỏ       |  6–10 phút |     2–4 phút |
| Trạm xăng          |  8–12 phút |     3–6 phút |
| Hiệu thuốc         | 10–16 phút |     4–8 phút |
| Siêu thị           | 18–28 phút |    6–12 phút |
| Trung tâm kỹ thuật | 20–30 phút |    8–15 phút |

Đây là mục tiêu cân bằng, không phải giới hạn cứng.

---

# Time Budget của địa điểm

Thời gian khám phá một địa điểm được tạo thành từ:

```text
Di chuyển bên trong
+
Tìm kiếm
+
Xử lý vật cản
+
Ra quyết định
+
Ứng phó sự kiện
+
Phân loại vật phẩm
+
Thoát khỏi địa điểm
```

Ví dụ với hiệu thuốc:

| Hoạt động               | Thời gian dự kiến |
| ----------------------- | ----------------: |
| Quan sát và tìm lối vào |          1–2 phút |
| Khám phá khu bán hàng   |          3–5 phút |
| Tìm kiếm Search Point   |          3–5 phút |
| Xử lý cửa kho           |          1–3 phút |
| Phân loại loot          |          1–2 phút |
| Rời địa điểm            |            1 phút |
| Tổng                    |        10–18 phút |

Một địa điểm không nên kéo dài thời gian bằng cách đặt quá nhiều vật chứa vô nghĩa.

---

# Trạng thái khám phá

Địa điểm không nên chỉ có hai trạng thái:

```text
Chưa loot
Đã loot
```

Nên sử dụng nhiều trạng thái hơn.

---

## Unknown

Người chơi chưa biết địa điểm tồn tại.

---

## Discovered

Người chơi đã biết vị trí nhưng chưa tới.

---

## Entered

Người chơi đã bước vào địa điểm.

---

## Partially Explored

Người chơi đã khám phá một phần nhưng vẫn còn Zone chưa tiếp cận.

---

## Main Objective Completed

Người chơi đã hoàn thành mục tiêu chính của địa điểm.

---

## Fully Explored

Người chơi đã tiếp cận toàn bộ khu vực hiện có.

---

## Depleted

Phần lớn tài nguyên thông thường có giá trị đã bị lấy.

---

## Changed

Địa điểm đã thay đổi do sự kiện hoặc trạng thái thế giới.

---

## Inaccessible

Địa điểm tạm thời không thể tiếp cận.

---

## Destroyed

Địa điểm bị phá hủy vĩnh viễn hoặc không còn giá trị sử dụng ban đầu.

---

# Loot Depletion

Mỗi địa điểm có một mức độ cạn kiệt tài nguyên.

Ví dụ:

| Mức depletion | Ý nghĩa                                          |
| ------------: | ------------------------------------------------ |
|            0% | Chưa bị lấy tài nguyên                           |
|           25% | Đã loot một phần nhỏ                             |
|           50% | Nguồn tài nguyên chính đã giảm đáng kể           |
|           75% | Chỉ còn tài nguyên phụ hoặc khu vực khó tiếp cận |
|          100% | Không còn loot thông thường có giá trị           |

`100% depletion` không có nghĩa địa điểm hoàn toàn vô dụng.

Địa điểm vẫn có thể:

- Xuất hiện sự kiện.
- Cung cấp thông tin.
- Mở đường tắt.
- Chứa NPC.
- Trở thành nơi trao đổi.
- Có khu vực mới được mở.
- Có tài nguyên mới do sự kiện hợp lý.
- Trở thành mục tiêu nhiệm vụ.

---

# Mục tiêu của địa điểm

Mỗi địa điểm cần ít nhất một mục tiêu chính.

Địa điểm lớn nên có thêm mục tiêu phụ.

---

## Mục tiêu chính

Mục tiêu chính là lý do cơ bản khiến người chơi đến địa điểm.

Ví dụ:

- Tìm thuốc.
- Lấy nhiên liệu.
- Thu thập thức ăn.
- Tìm linh kiện.
- Sửa trạm quan trắc.
- Cứu người sống sót.

---

## Mục tiêu phụ

Mục tiêu phụ tạo thêm lựa chọn hoặc phần thưởng.

Ví dụ:

- Mở đường tắt.
- Tìm mã két.
- Khôi phục điện.
- Mở kho khóa.
- Tìm bản đồ.
- Giải cứu NPC.
- Tắt nguồn rò rỉ phóng xạ.
- Thiết lập điểm quan sát.

Một địa điểm tốt không chỉ có mục tiêu:

> Vào và lấy mọi thứ.

---

# Return Hook

Return Hook là lý do hợp lý khiến người chơi quay lại một địa điểm.

Không phải địa điểm nào cũng cần nhiều Return Hook.

Một số nơi chỉ cần một chuyến đi duy nhất nhưng đáng nhớ.

---

## Công cụ mới

Người chơi quay lại sau khi có:

- Xà beng.
- Máy cắt.
- Chìa khóa.
- Thiết bị điện.
- Bộ bảo hộ tốt hơn.
- Nguồn điện di động.

---

## Thông tin mới

Người chơi quay lại vì biết thêm:

- Có tầng hầm.
- Có kho bí mật.
- Mã két.
- Vị trí vật tư.
- NPC đang chờ.
- Lối vào thay thế.

---

## Trạng thái mới

Địa điểm đã thay đổi:

- Điện được khôi phục.
- Nước rút.
- Tường bị sập.
- Cửa bị phá.
- Nhóm NPC rời đi.
- Khu vực nhiễm xạ giảm.

---

## Sự kiện mới

Một sự kiện mới xuất hiện:

- Đoàn cứu trợ ghé qua.
- Xe cứu thương xuất hiện.
- Người sống sót phát tín hiệu.
- Nhóm khác chiếm địa điểm.
- Một cuộc trao đổi được thiết lập.

---

## Chức năng mới

Địa điểm có vai trò mới:

- Trạm trao đổi.
- Nơi trú tạm.
- Điểm quan sát.
- Đường tắt.
- Trạm phát radio.
- Nguồn điện phụ.

---

# Cấu trúc rủi ro

Mỗi địa điểm nên có nhiều hơn một loại rủi ro.

Chỉ tăng mức phóng xạ sẽ không đủ để tạo ra khác biệt giữa các địa điểm.

---

## Rủi ro môi trường

- Phóng xạ.
- Khí độc.
- Cháy.
- Điện giật.
- Ngập.
- Công trình sắp sập.
- Nhiễm bẩn bề mặt.

---

## Rủi ro thời gian

- Địa điểm ở xa.
- Mất nhiều thời gian tìm kiếm.
- Đường về có thể bị chặn.
- Sự kiện chỉ tồn tại trong thời gian ngắn.
- Trời sắp tối.
- Bão sắp tới.

---

## Rủi ro tài nguyên

- Hao bộ lọc.
- Hao nhiên liệu.
- Hỏng công cụ.
- Tốn nước khử nhiễm.
- Tốn thuốc điều trị.
- Mất độ bền trang bị.

---

## Rủi ro con người

- Người sống sót khác.
- Nhóm đối địch.
- Tranh chấp tài nguyên.
- Lừa đảo.
- NPC bị thương cần hỗ trợ.
- Người được cứu trở thành gánh nặng.

---

## Rủi ro thông tin

- Tin đồn không chính xác.
- Không biết địa điểm đã bị loot.
- Không biết lối thoát đã bị khóa.
- Không biết mức phóng xạ đã tăng.
- Dự báo không đầy đủ.

---

# Phần thưởng của địa điểm

Phần thưởng không chỉ là vật phẩm.

---

## Tài nguyên

Ví dụ:

- Thức ăn.
- Nước.
- Thuốc.
- Linh kiện.
- Nhiên liệu.
- Bộ lọc.
- Công cụ.

---

## Thông tin

Ví dụ:

- Bản đồ.
- Tọa độ.
- Mật mã.
- Dự báo.
- Tin về tuyến đường.
- Vị trí kho vật tư.

---

## Tiến trình

Ví dụ:

- Mở công trình mới.
- Mở recipe.
- Mở khu vực.
- Mở nâng cấp nơi trú ẩn.
- Mở chuỗi nhiệm vụ.

---

## Quan hệ

Ví dụ:

- Cứu NPC.
- Mở điểm trao đổi.
- Nhận hỗ trợ.
- Tạo đồng minh.
- Mở sự kiện mới.

---

## Tiện ích thế giới

Ví dụ:

- Mở đường tắt.
- Sửa trạm điện.
- Giảm phóng xạ.
- Tạo nơi trú tạm.
- Mở điểm quan sát.
- Khôi phục tín hiệu radio.

---

# Search Point

Không nên đặt quá nhiều Search Point chỉ để kéo dài thời gian.

Mỗi Search Point cần có mục đích rõ ràng.

---

## Search Point nhanh

Ví dụ:

- Ngăn kéo.
- Kệ.
- Túi.
- Hộp nhỏ.

Đặc điểm:

- Tìm nhanh.
- Ít loot.
- Rủi ro thấp.

---

## Search Point trung bình

Ví dụ:

- Tủ.
- Thùng hàng.
- Tủ lạnh.
- Cốp xe.

Đặc điểm:

- Mất thời gian vừa phải.
- Chứa loot phù hợp với địa điểm.

---

## Search Point đặc biệt

Ví dụ:

- Két.
- Kho khóa.
- Tủ y tế.
- Thùng cứu trợ.
- Tủ điện.

Đặc điểm:

- Cần công cụ, mã hoặc điều kiện.
- Có loot giá trị cao.
- Có thể tạo Return Hook.

---

## Search Point nguy hiểm

Ví dụ:

- Thùng nhiễm xạ.
- Tủ trong khu vực cháy.
- Phòng sắp sập.
- Container rò rỉ hóa chất.

Đặc điểm:

- Phần thưởng cao.
- Chi phí tiếp cận rõ ràng.
- Người chơi có thể chủ động bỏ qua.

---

# Quy tắc phân bố loot

Loot phải phù hợp với logic không gian.

Ví dụ tại hiệu thuốc:

- Thuốc thường ở khu bán hàng.
- Thuốc hiếm ở tủ khóa.
- Vật tư y tế ở kho.
- Hồ sơ và mật mã ở phòng nhân viên.
- Đồ cá nhân ở tủ nhân viên hoặc ba lô.

Không nên phân bố vật phẩm hoàn toàn ngẫu nhiên.

Một vật phẩm bất thường chỉ nên xuất hiện khi có ngữ cảnh hợp lý.

Ví dụ:

- Linh kiện xuất hiện vì một thợ sửa máy từng trú tại đó.
- Thức ăn xuất hiện trong phòng nghỉ nhân viên.
- Nhiên liệu xuất hiện trong xe giao hàng phía sau.

---

# Lối vào và lối thoát

Mỗi địa điểm phải có ít nhất:

- Một lối vào hợp lý.
- Một lối thoát hợp lý.

Địa điểm lớn nên có nhiều tuyến tiếp cận.

---

## Lối vào chính

Đặc điểm:

- Dễ thấy.
- Dễ tiếp cận.
- Có thể nguy hiểm hoặc bị kiểm soát.

---

## Lối vào thay thế

Ví dụ:

- Cửa sau.
- Cửa sổ.
- Mái nhà.
- Đường hầm.
- Tường bị sập.

Đặc điểm:

- Cần công cụ.
- Tốn thời gian.
- Ít rủi ro hơn hoặc dẫn thẳng tới khu vực có giá trị.

---

## Lối thoát khẩn cấp

Ví dụ:

- Cửa giao hàng.
- Cầu thang thoát hiểm.
- Cửa sổ.
- Đường hầm.
- Lối thông sang tòa nhà bên cạnh.

Lối thoát phải được xem là một phần của quyết định khám phá.

---

# Ví dụ cấu trúc đường đi

Ví dụ tuyến chính của hiệu thuốc:

```text
Cửa chính
    ↓
Khu bán hàng
    ↓
Quầy thuốc
    ↓
Kho phía sau
    ↓
Cửa giao hàng
```

Tuyến thay thế:

```text
Hẻm sau
    ↓
Cửa khóa
    ↓
Kho phía sau
```

Người chơi có thể lựa chọn:

- Đi qua cửa chính nhanh hơn nhưng phóng xạ cao.
- Đi vòng ra sau và dùng xà beng.
- Tiếp cận từ mái nhà nếu đã mở tuyến đường phù hợp.

---

# State Machine của địa điểm

Mỗi địa điểm lớn nên có một State Machine đơn giản.

Ví dụ:

```text
STATE_0 — Intact
        ↓
STATE_1 — Partially Looted
        ↓
STATE_2 — Survivor Occupied
        ↓
STATE_3 — Abandoned
        ↓
STATE_4 — Radiation Surge
        ↓
STATE_5 — Collapsed
```

Không phải mọi trạng thái đều bắt buộc xảy ra.

---

## Điều kiện chuyển trạng thái

Trạng thái địa điểm có thể thay đổi dựa trên:

- Ngày hiện tại.
- Người chơi đã ghé hay chưa.
- Mức depletion.
- Quyết định trước đó.
- Sự kiện toàn cục.
- NPC.
- Công cụ người chơi sở hữu.
- Mức phóng xạ.
- Xác suất có kiểm soát.

---

# Dữ liệu cơ bản của một địa điểm

Mỗi Location nên có các dữ liệu sau:

```text
location_id
display_name
region
distance_category
target_playtime
current_state
exploration_state
loot_depletion
radiation_level
contamination_level
zones
entrances
exits
active_event
occupying_group
main_objective
secondary_objectives
return_hooks
next_possible_transitions
```

---

# Template thiết kế địa điểm

```md
# Location: [Tên địa điểm]

## 1. Vai trò

## 2. Mục tiêu chính

## 3. Mục tiêu phụ

## 4. Khu vực bản đồ

## 5. Khoảng cách từ nơi trú ẩn

## 6. Target Playtime

## 7. Quy mô

## 8. Các Zone

## 9. Search Point

## 10. Lối vào

## 11. Lối thoát

## 12. Rủi ro chính

## 13. Rủi ro phụ

## 14. Loot chính

## 15. Loot phụ

## 16. Loot đặc biệt

## 17. Công cụ cần thiết

## 18. Trạng thái ban đầu

## 19. Các trạng thái có thể chuyển đổi

## 20. Điều kiện chuyển trạng thái

## 21. Sự kiện có thể xảy ra

## 22. Return Hook

## 23. Điều kiện Depleted

## 24. Giá trị sau khi Depleted

## 25. Vai trò trong Resource Flow

## 26. Ghi chú cân bằng
```

---

# Quy tắc chất lượng

Một địa điểm chỉ được xem là hoàn thiện khi trả lời được các câu hỏi:

1. Người chơi đến đây để làm gì?
2. Địa điểm khác biệt với nơi khác ở đâu?
3. Người chơi mất bao lâu tại đây?
4. Rủi ro chính là gì?
5. Quyết định quan trọng là gì?
6. Loot có phù hợp với bối cảnh không?
7. Có nhiều lối tiếp cận không?
8. Khi loot hết, địa điểm còn giá trị gì?
9. Có lý do hợp lý để quay lại không?
10. Địa điểm thay đổi theo thời gian như thế nào?
11. Địa điểm đóng vai trò gì trong Resource Flow?
12. Người chơi có thể bỏ qua địa điểm này không?

---

# Phân loại quy mô địa điểm

## Địa điểm chính

Số lượng đề xuất:

```text
8–10 địa điểm
```

Đặc điểm:

- Nhiều Zone.
- Nhiều trạng thái.
- Có sự kiện riêng.
- Có Return Hook.
- Có tài nguyên quan trọng.
- Có thời lượng khám phá dài.

Ví dụ:

- Siêu thị.
- Hiệu thuốc.
- Trung tâm kỹ thuật.
- Trạm quan trắc.
- Phòng khám.
- Kho cứu trợ.

---

## Địa điểm phụ

Số lượng đề xuất:

```text
10–15 địa điểm
```

Đặc điểm:

- Quy mô nhỏ.
- Một mục tiêu chính.
- Thời lượng ngắn.
- Ít trạng thái hơn.
- Có thể cung cấp nguồn tài nguyên thay thế.

Ví dụ:

- Nhà dân.
- Cửa hàng tiện lợi.
- Gara.
- Văn phòng.
- Trạm xe buýt.
- Nhà kho nhỏ.

---

## Điểm cơ hội

Điểm cơ hội xuất hiện thông qua sự kiện thế giới.

Ví dụ:

- Xe cứu thương bị lật.
- Xe tải cứu trợ.
- Ba lô bị bỏ lại.
- Trại tạm.
- Thi thể.
- Tín hiệu khẩn cấp.
- Thùng hàng rơi.

Đặc điểm:

- Có thời hạn.
- Quy mô nhỏ.
- Giá trị cao.
- Thay đổi giữa các lượt chơi.

---

# Phạm vi MVP

MVP không cần mọi địa điểm đều có State Machine phức tạp.

MVP nên ưu tiên:

- 3–5 địa điểm chính hoàn chỉnh.
- 5–8 địa điểm phụ.
- Một số điểm cơ hội.
- Loot depletion được lưu lại.
- Zone bị khóa.
- Return Hook đơn giản.
- Sự kiện theo ngày hoặc điều kiện.
- Nhiều lối vào cho một số địa điểm quan trọng.

Sau khi Core Loop được kiểm chứng, số lượng địa điểm có thể tăng lên.

---

# Quyết định đã chốt

- Sử dụng cấu trúc `Location → Zone → Search Point`.
- Mỗi địa điểm có Target Playtime.
- Loot không hồi sinh.
- Trạng thái khám phá và depletion được lưu lại.
- Địa điểm lớn có State Machine riêng.
- Người chơi quay lại vì công cụ, thông tin, sự kiện hoặc chức năng mới.
- Không bắt buộc mọi địa điểm đều có nhiều lần quay lại.
- Phần thưởng bao gồm tài nguyên, thông tin, tiến trình, quan hệ và tiện ích thế giới.
- Mỗi địa điểm phải có ít nhất một mục tiêu và một quyết định có ý nghĩa.
- MVP ưu tiên chất lượng địa điểm hơn số lượng.
