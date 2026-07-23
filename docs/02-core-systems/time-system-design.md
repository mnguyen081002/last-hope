# Time System Design ## 1. Mục tiêu Time System xác định cách thời gian vận hành trong Last Hope. Hệ thống phải đảm bảo: - Thời gian là tài nguyên quan trọng. - Mọi hành động đều có chi phí thời gian. - Thế giới tiếp tục thay đổi theo thời gian. - Thiết kế hoạt động nhất quán cho cả Single-player và Multiplayer. - Không phụ thuộc vào cơ chế tăng tốc hoặc nhảy thời gian trong gameplay thông thường. --- # 2. Nguyên tắc cốt lõi ## 2.1. Một World Clock duy nhất Toàn bộ game sử dụng một World Clock. Tất cả hệ thống cùng đọc từ một nguồn thời gian: - Player - NPC - Shelter - Hazard - Event - Weather - Disaster - Day/Night Cycle Không tồn tại đồng hồ riêng cho từng người chơi. --- ## 2.2. World Clock luôn chạy Trong gameplay bình thường: - Không Pause World Clock. - Không Time Skip. - Không Time Acceleration. - Không cộng trực tiếp thời gian vào World Clock. Thời gian luôn tiến với tốc độ cố định. --- ## 2.3. Thời gian là chi phí cơ hội Mỗi hành động đều tiêu tốn thời gian. Ví dụ:

text
Đi khám phá
=
Không thể sửa Shelter

- Không thể xử lý nước
- Không thể nghỉ ngơi
  Người chơi luôn phải lựa chọn ưu tiên. --- ## 2.4. Thế giới không chờ người chơi Trong khi người chơi hoạt động: - Mưa tiếp tục. - Mực nước thay đổi. - NPC di chuyển. - Shelter tiêu hao tài nguyên. - Event tiếp tục đếm thời gian. - Disaster tiếp tục tiến triển. Không có hệ thống "đóng băng thế giới". --- # 3. Tốc độ thời gian ## 3.1. Tỷ lệ thời gian Đề xuất mặc định:
  text
  1 phút thực
  =
  5 phút trong game
  Có thể điều chỉnh sau khi prototype. --- ## 3.2. Một tốc độ duy nhất Gameplay sử dụng duy nhất một tốc độ thời gian. Không tồn tại: - Fast Forward. - Build Speed Time Skip. - Craft Time Skip. - Travel Skip. Mọi người chơi đều sử dụng cùng tốc độ thời gian. --- # 4. Chu kỳ ngày đêm Một ngày gồm bốn giai đoạn. ## Morning - Kiểm tra Shelter. - Chuẩn bị. - Lập kế hoạch. --- ## Afternoon - Khám phá. - Thu thập tài nguyên. - Thực hiện nhiệm vụ. --- ## Evening - Trở về Shelter. - Hoàn thành công việc còn lại. - Chuẩn bị cho đêm. --- ## Night - Tầm nhìn giảm. - Một số Hazard nguy hiểm hơn. - Một số Event chỉ xuất hiện ban đêm. - Người chơi có thể lựa chọn ngủ. --- # 5. Timed Action Mọi hành động đều có thời lượng. Ví dụ: | Hành động | Thời lượng | | ---------- | ---------- | | Di chuyển | Theo khoảng cách | | Tìm kiếm | Theo thời gian thực | | Leo trèo | Theo thời gian thực | | Xây dựng | Theo thời gian thực | | Điều trị | Theo thời gian thực | | Nghỉ ngắn | Theo thời gian thực | Không có hành động nào tự động hoàn thành bằng cách nhảy thời gian. --- # 6. Background System Một số hệ thống hoạt động liên tục mà không cần người chơi điều khiển. Ví dụ: - Máy lọc nước. - Máy phát điện. - Máy bơm. - Pin đang sạc. - Quần áo đang sấy. - NPC làm việc. Ví dụ:
  text
  14:00

Khởi động máy lọc nước

↓

Người chơi tiếp tục khám phá

↓

16:00

Máy lọc hoàn thành
Người chơi không cần đứng chờ. --- # 7. Search System Search luôn diễn ra theo thời gian thực. Ví dụ:
text
Bắt đầu tìm kiếm

↓

Loot xuất hiện dần

↓

Người chơi có thể dừng bất cứ lúc nào
Không sử dụng:
text
Search

↓

+30 phút

↓

Nhận loot
--- # 8. Build System Xây dựng diễn ra theo thời gian thực. Các công việc lớn được chia thành nhiều giai đoạn. Ví dụ:
text
Gia cố cửa

↓

Chuẩn bị vật liệu

↓

Lắp khung

↓

Gia cố

↓

Hoàn thành
Tiến độ được lưu. Người chơi có thể dừng giữa chừng. --- # 9. Shelter Activity Các công việc trong Shelter gồm hai loại. ## Active Task Cần người chơi trực tiếp thực hiện. Ví dụ: - Xây dựng. - Sửa chữa. - Điều trị. - Chế tạo thủ công. --- ## Passive Task Thiết bị tự vận hành. Ví dụ: - Lọc nước. - Máy phát điện. - Máy bơm. - Sạc pin. - Hệ thống chiếu sáng. --- # 10. Rest System ## Nghỉ ngắn Người chơi có thể nghỉ ngắn. Đặc điểm: - Không tăng tốc thời gian. - World Clock tiếp tục chạy. - Hồi một phần Stamina. - Hồi rất ít Fatigue. --- ## Ngủ Ngủ là ngoại lệ duy nhất của Time System. Người chơi có thể: - Ngủ theo số giờ. - Ngủ đến một thời điểm. Ví dụ:
text
22:00

↓

Ngủ đến 06:00
Game mô phỏng toàn bộ khoảng thời gian này. Bao gồm: - Shelter. - NPC. - Hazard. - Disaster. - Weather. - Resource Consumption. - Event. Sau đó World Clock chuyển sang:
text
06:00
--- ## Điều kiện ngủ Chỉ được ngủ khi: - Đang ở Shelter hoặc nơi an toàn. - Không có Hazard trực tiếp. - Không đang chiến đấu. - Không thực hiện hành động bắt buộc. --- ## Giấc ngủ có thể bị gián đoạn Ví dụ: - Shelter bị ngập. - Cháy. - NPC báo động. - Máy phát điện hỏng. - Event khẩn cấp. - Disaster chuyển Phase. Nếu bị đánh thức: - World Clock dừng tại thời điểm xảy ra sự kiện. - Người chơi quay lại điều khiển. --- # 11. Multiplayer Time Rule Tất cả người chơi dùng chung World Clock. Ví dụ:
text
Player A
Khám phá

Player B
Sửa Shelter

Player C
Quản lý kho
World Clock vẫn chạy bình thường. Không ai được phép: - Skip Time. - Fast Forward. - Pause World Clock. --- # 12. Multiplayer Sleep Nếu chỉ một người ngủ: - World Clock không tăng tốc. - Người đó ngủ theo thời gian thực. Nếu tất cả người chơi đều ngủ: Game mô phỏng thời gian ngủ. Ví dụ:
text
22:00

↓

Tất cả cùng ngủ

↓

World Clock chuyển tới thời điểm người đầu tiên thức dậy
Nếu xảy ra Event trong lúc ngủ: - Giấc ngủ kết thúc. - Người chơi bị đánh thức. - Gameplay tiếp tục bình thường. --- # 13. Event Deadline Mỗi Event có thể có thời hạn. Ví dụ:
text
14:00

NPC cầu cứu

↓

18:00

Event hết hạn
Deadline luôn dựa trên World Clock. --- # 14. Disaster Timeline Mỗi Disaster định nghĩa Timeline riêng. Cấu trúc:
text
Normal

↓

Warning

↓

Escalation

↓

Peak

↓

Aftermath
World Clock quyết định khi nào chuyển Phase. --- # 15. World Update Thế giới được cập nhật theo chu kỳ. Ví dụ: - Mỗi 10 phút trong game. - Mỗi giờ trong game. - Khi chuyển Phase. - Khi người chơi vào Location. Không cần cập nhật toàn bộ thế giới mỗi frame. --- # 16. Dữ liệu hệ thống ## World
text
world_time
day_index
time_of_day
current_phase
weather_state

## Timed Task

text
task_id
owner
start_time
duration
progress
interruptible
completion_effect

## Sleep

text
sleep_start
sleep_target
sleep_interrupted
wake_reason
--- # 17. Phạm vi MVP Triển khai: - Một World Clock. - Chu kỳ ngày đêm. - Timed Action. - Background Task. - Search thời gian thực. - Build thời gian thực. - Passive Machine. - Sleep System. - Event Deadline. - Disaster Timeline. Chưa triển khai: - Pause World Clock. - Time Skip trong gameplay. - Fast Forward ngoài giấc ngủ. - Đồng hồ riêng cho từng người chơi. --- # 18. Quyết định chốt - Toàn bộ game sử dụng một World Clock. - World Clock luôn chạy với tốc độ cố định. - Không sử dụng Time Skip trong gameplay. - Không sử dụng Time Acceleration trong gameplay. - Mọi hành động đều tiêu tốn thời gian thực. - Máy móc và hệ thống tự động hoạt động dưới dạng Background Task. - Search và Build luôn diễn ra theo thời gian thực. - Chỉ giấc ngủ mới được phép mô phỏng và chuyển World Clock tới thời điểm thức dậy. - Giấc ngủ có thể bị gián đoạn bởi Event hoặc Disaster. - Multiplayer luôn sử dụng một World Clock chung.
cập nhật memory như này nhé. Sau đó tiếp tục phần tiếp theo
