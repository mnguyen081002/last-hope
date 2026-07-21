# Hướng dẫn làm việc với repository

## Mục tiêu hiện tại

Mục tiêu trước mắt là tạo prototype nhỏ nhất để kiểm chứng vòng lặp:

> khám phá → thu thập → quay về → chuẩn bị nơi trú ẩn → chịu hậu quả của thảm họa.

Không xây một game sinh tồn hoàn chỉnh trước khi vòng lặp này được playtest và chứng minh là thú vị.

## Nguồn sự thật

Trước khi thay đổi code hoặc thiết kế, đọc tối thiểu:

1. `docs/PROJECT_CONTEXT.md`
2. `docs/MVP_SCOPE.md`
3. `docs/DECISIONS.md`
4. `docs/OPEN_QUESTIONS.md`

Ưu tiên thông tin theo thứ tự: quyết định `Approved` mới nhất trong `DECISIONS.md`, tài liệu phạm vi, rồi giả định làm việc. Không suy diễn rằng một đề xuất đã được duyệt.

## Nhãn thông tin bắt buộc

- **Fact**: thông tin đã được người dùng hoặc repository xác nhận.
- **Decision**: lựa chọn đã được người dùng duyệt và ghi trong `DECISIONS.md`.
- **Assumption**: giả định tạm thời để tiếp tục phân tích hoặc prototype.
- **Proposal**: phương án đang chờ duyệt.

Khi một giả định ảnh hưởng đáng kể đến gameplay, phải ghi rõ và xin duyệt trước khi triển khai.

## Quy trình quyết định thiết kế

Mỗi vấn đề quan trọng phải ghi theo schema:

```text
Question:
Goal:
Constraints:
Options:
Recommendation:
Why:
Risks:
Prototype:
Decision status: Proposed | Approved | Rejected | Superseded
```

Đưa ra 2–4 phương án thực sự khác nhau, phân tích trade-off, chi phí sản xuất và cách kiểm chứng. Chỉ cập nhật GDD như thiết kế chính thức sau khi quyết định được duyệt.

## Quy tắc phạm vi và triển khai

- Ưu tiên vertical slice có thể chơi và reset nhanh.
- Không tự mở rộng phạm vi; không thêm multiplayer, thế giới mở lớn hoặc nhiều thảm họa cho prototype đầu tiên.
- Không tạo abstraction tổng quát cho nhu cầu chưa tồn tại.
- Không thêm dependency nếu chưa nêu lý do và chi phí.
- Gameplay logic phải tách khỏi presentation đủ để test độc lập.
- Dữ liệu vật phẩm, công thức và sự kiện nên cấu hình tập trung, không hard-code rải rác.
- Tránh global mutable state; chỉ tối ưu hiệu năng sau khi đo.
- Randomness cần seed cố định khi việc tái hiện lỗi có giá trị.
- TODO phải nói rõ thiếu gì và vì sao chưa làm.
- Không dùng placeholder âm thầm trong code production.
- Theo convention của engine sau khi engine được duyệt; không áp kiến trúc backend máy móc vào game.

## Sau mỗi thay đổi

1. Chạy test hoặc kiểm tra phù hợp.
2. Tóm tắt file thay đổi và hành vi mới.
3. Nêu giới hạn, giả định và rủi ro còn lại.
4. Cập nhật tài liệu liên quan.
5. Nếu có quyết định mới được người dùng duyệt, ghi vào `docs/DECISIONS.md`.

## Tiêu chí prototype

Đánh giá prototype qua mức độ dễ hiểu, chất lượng lựa chọn, căng thẳng khi rời căn cứ, giá trị chiến thuật của hành trang/nâng cấp, khả năng giải thích thắng-thua và mong muốn chơi lại. Không đánh giá chủ yếu qua đồ họa, lượng nội dung, kích thước bản đồ hay độ chi tiết mô phỏng.
