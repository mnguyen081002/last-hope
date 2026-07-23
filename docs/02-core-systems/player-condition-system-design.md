# Player Condition System Design

## 1. Mục tiêu

Player Condition System xác định trạng thái thể chất của người chơi và cách các trạng thái này ảnh hưởng đến khả năng sinh tồn.

Hệ thống phải đảm bảo:

- Trạng thái nhân vật ảnh hưởng trực tiếp đến gameplay.
- Hậu quả tăng dần và có thể dự đoán.
- Không sử dụng quá nhiều chỉ số.
- Không biến game thành mô phỏng y tế chi tiết.
- Có thể dùng lại cho nhiều Disaster.
- Hỗ trợ Single-player và Multiplayer.

---

# 2. Chỉ số cốt lõi

MVP sử dụng các chỉ số sau:

```text
Health
Stamina
Fatigue
Hunger
Thirst
Body Temperature
```

Các trạng thái bổ sung:

```text
Injury
Status Effect
Carry Load
```

---

# 3. Health

Health đại diện cho tình trạng sống còn tổng thể.

Health giảm do:

- Chấn thương.
- Mất máu.
- Thiếu nước nghiêm trọng.
- Hạ thân nhiệt.
- Tiếp xúc Hazard.
- Không điều trị Injury.

| Health | Trạng thái   |
| -----: | ------------ |
| 76–100 | Ổn định      |
|  51–75 | Bị thương    |
|  26–50 | Nghiêm trọng |
|   1–25 | Nguy kịch    |
|      0 | Tử vong      |

Health thấp có thể làm giảm:

- Tốc độ di chuyển.
- Tốc độ thao tác.
- Stamina tối đa.
- Khả năng mang vật nặng.

Health không tự hồi nhanh.

Hồi phục cần:

- Điều trị.
- Nghỉ ngơi.
- Thức ăn và nước.
- Điều kiện Shelter phù hợp.
- Loại bỏ nguyên nhân gây sát thương.

---

# 4. Stamina

Stamina đại diện cho năng lượng tức thời.

Stamina giảm khi:

- Chạy.
- Leo trèo.
- Bơi.
- Di chuyển trong nước.
- Mang vật nặng.
- Dọn vật cản.
- Thực hiện công việc thể lực.

Stamina hồi khi:

- Đi chậm.
- Đứng yên.
- Nghỉ ngắn.
- Bỏ bớt hành lý.

Khi Stamina cạn:

- Không thể chạy.
- Leo trèo chậm.
- Khó chống lại dòng nước.
- Tăng nguy cơ ngã.
- Không thể thực hiện một số hành động nặng.

---

# 5. Fatigue

Fatigue là mức mệt mỏi tích lũy.

Fatigue tăng do:

- Hoạt động kéo dài.
- Thiếu ngủ.
- Làm việc ban đêm.
- Mang quá tải.
- Tiếp xúc lạnh.
- Thực hiện công việc nặng.

| Fatigue | Tác động                           |
| ------: | ---------------------------------- |
|    0–24 | Không ảnh hưởng                    |
|   25–49 | Stamina hồi chậm                   |
|   50–74 | Hành động chậm                     |
|   75–89 | Di chuyển chậm                     |
|  90–100 | Không thể thực hiện hành động nặng |

Fatigue giảm chủ yếu bằng giấc ngủ.

Nghỉ ngắn chỉ giảm một phần nhỏ.

---

# 6. Hunger

Hunger tạo áp lực tài nguyên dài hạn.

Hunger tăng theo:

- Thời gian.
- Hoạt động thể lực.
- Nhiệt độ cơ thể thấp.
- Injury hoặc Illness.

Hunger cao gây:

- Giảm Stamina tối đa.
- Fatigue tăng nhanh hơn.
- Health giảm nếu kéo dài.

MVP không mô phỏng dinh dưỡng chi tiết.

Thức ăn chỉ cần các thuộc tính:

```text
nutrition
weight
spoilage
contamination_state
```

---

# 7. Thirst

Thirst tạo áp lực cấp bách hơn Hunger.

Thirst tăng theo:

- Thời gian.
- Hoạt động thể lực.
- Nhiệt độ.
- Carry Load.
- Illness.

Thirst cao gây:

- Stamina hồi chậm.
- Giảm tốc độ hành động.
- Fatigue tăng.
- Health giảm nếu kéo dài.

Nguồn nước có thể ở các trạng thái:

```text
Clean
Untreated
Black Water Contaminated
```

Nước không an toàn vẫn có thể uống, nhưng tạo rủi ro Status Effect.

---

# 8. Body Temperature

Body Temperature đại diện cho khả năng giữ ấm của nhân vật.

Nhiệt độ cơ thể giảm khi:

- Bị ướt.
- Ở trong nước lâu.
- Mặc quần áo ướt.
- Ở ngoài trời lạnh.
- Ngủ trong Shelter không đủ ấm.

Nhiệt độ cơ thể tăng khi:

- Thay quần áo khô.
- Ở gần nguồn nhiệt.
- Nghỉ tại Shelter.
- Dùng đồ uống hoặc vật phẩm giữ ấm.

| Trạng thái    | Tác động                    |
| ------------- | --------------------------- |
| Bình thường   | Không ảnh hưởng             |
| Lạnh nhẹ      | Stamina hồi chậm            |
| Lạnh          | Fatigue tăng                |
| Hạ thân nhiệt | Di chuyển và hành động chậm |
| Nguy kịch     | Health giảm                 |

---

# 9. Injury

Injury là trạng thái riêng, không chỉ là mất Health.

Mỗi Injury có:

```text
injury_type
severity
body_region
bleeding_rate
movement_modifier
action_modifier
treatment_required
recovery_time
```

Các Injury của MVP:

## Cut

- Có thể gây Bleeding.
- Cần băng bó.
- Có nguy cơ nhiễm bệnh nếu tiếp xúc nước đen.

## Bruise

- Giảm nhẹ hiệu quả hành động.
- Tự hồi theo thời gian.

## Sprain

- Giảm tốc độ di chuyển.
- Giảm khả năng leo trèo.
- Cần nghỉ ngơi.

## Fracture

- Hạn chế nghiêm trọng di chuyển.
- Cần nẹp.
- Chỉ xuất hiện trong sự kiện nghiêm trọng.

---

# 10. Status Effect

Status Effect là trạng thái tạm thời hoặc kéo dài do Hazard, Injury, Item hoặc Event gây ra.

Mỗi Status Effect có:

```text
status_id
source
severity
duration
stack_rule
effects
treatment
```

Status Effect của MVP:

```text
Wet
Cold
Bleeding
Sick
Black Water Exposure
Disoriented
Exhausted
```

---

# 11. Wet

Wet xuất hiện khi:

- Đi dưới mưa.
- Đi trong vùng ngập.
- Bơi.
- Ngã xuống nước.

Wet gây:

- Body Temperature giảm nhanh hơn.
- Quần áo nặng hơn.
- Fatigue tăng nhanh hơn.
- Vật phẩm không chống nước có nguy cơ hỏng.

Wet được xử lý bằng:

- Thay quần áo.
- Sấy khô.
- Nguồn nhiệt.
- Nghỉ trong Shelter.

---

# 12. Black Water Exposure

Black Water Exposure tăng khi:

- Tiếp xúc trực tiếp với nước đen.
- Có vết thương hở trong vùng ngập.
- Uống nước nhiễm.
- Sử dụng vật phẩm bị ngâm chưa xử lý.

Tác động:

- Tăng nguy cơ Sick.
- Giảm hiệu quả hồi phục.
- Làm Injury khó điều trị hơn.
- Có thể tạo hiệu ứng bất thường trong các Event đặc biệt.

Exposure phải tăng theo thời gian tiếp xúc, không gây hậu quả tức thời sau một lần chạm ngắn.

---

# 13. Disoriented

Disoriented có thể xuất hiện do:

- Nhiễu điện từ mạnh.
- Mất Stamina khi ở trong dòng nước.
- Thiếu ngủ nghiêm trọng.
- Event bất thường.

Tác động:

- Giảm khả năng định hướng.
- Tăng thời gian tương tác.
- Làm thông tin bản đồ tạm thời kém chính xác.
- Giảm khả năng phát hiện nguy hiểm.

Trạng thái này phải có thời lượng ngắn và phản hồi rõ ràng.

---

# 14. Carry Load

Carry Load được tính từ tổng trọng lượng vật phẩm đang mang.

```text
Carry Load
=
Current Weight
/
Maximum Carry Weight
```

| Tải trọng | Tác động                            |
| --------: | ----------------------------------- |
|     0–50% | Không ảnh hưởng                     |
|    51–75% | Stamina tiêu hao tăng nhẹ           |
|   76–100% | Di chuyển chậm                      |
| Trên 100% | Không thể chạy hoặc leo bình thường |

Trong vùng ngập, Carry Load còn làm tăng:

- Thời gian di chuyển.
- Nguy cơ mất thăng bằng.
- Nguy cơ bị dòng nước cuốn.
- Stamina tiêu hao.

---

# 15. Action Efficiency

Action Efficiency là giá trị tổng hợp từ trạng thái nhân vật.

```text
Action Efficiency
=
Health Modifier
×
Fatigue Modifier
×
Injury Modifier
×
Temperature Modifier
×
Status Modifier
```

Action Efficiency ảnh hưởng:

- Search.
- Build.
- Repair.
- Treatment.
- Rescue.
- Climbing.
- Obstacle Clearing.

UI không cần hiển thị công thức.

Chỉ cần hiển thị:

```text
Normal
Reduced
Severely Reduced
Unavailable
```

---

# 16. Quan hệ giữa các trạng thái

Các trạng thái có thể tạo chuỗi hậu quả.

Ví dụ:

```text
Đi trong nước đen
↓
Wet
↓
Body Temperature giảm
↓
Fatigue tăng
↓
Stamina hồi chậm
↓
Khó chống dòng nước
↓
Nguy cơ Injury
```

Hệ thống phải cho người chơi cơ hội phản ứng trước khi chuỗi hậu quả trở nên không thể cứu vãn.

---

# 17. Incapacitation

Khi người chơi không còn khả năng hoạt động nhưng Health vẫn lớn hơn `0`, nhân vật chuyển sang trạng thái Incapacitated.

Nguyên nhân:

- Injury nghiêm trọng.
- Hạ thân nhiệt.
- Stamina cạn trong dòng nước mạnh.
- Event đặc biệt.

Trong Single-player:

- NPC có thể cứu nếu đủ điều kiện.
- Người chơi có thể mất thời gian.
- Một phần vật phẩm có thể bị mất.
- Nhân vật có thể tỉnh lại tại Shelter.

Trong Multiplayer:

- Đồng đội có thể sơ cứu.
- Đồng đội có thể kéo hoặc mang người bị thương.
- World Clock tiếp tục chạy.

Nếu không được cứu và Health giảm về `0`, nhân vật tử vong.

---

# 18. UI Requirement

UI phải hiển thị:

- Health.
- Stamina.
- Fatigue.
- Hunger.
- Thirst.
- Body Temperature.
- Carry Load.
- Injury.
- Status Effect.

Đề xuất:

- Health và Stamina dùng thanh.
- Fatigue, Hunger, Thirst và Temperature dùng trạng thái.
- Injury và Status Effect dùng biểu tượng kèm mô tả.
- Carry Load dùng phần trăm và cấp tải trọng.

---

# 19. Dữ liệu hệ thống

## Player Condition

```text
health
stamina
fatigue
hunger
thirst
body_temperature
current_weight
maximum_carry_weight
injuries
status_effects
action_efficiency
```

## Injury

```text
injury_id
type
severity
body_region
bleeding_rate
movement_modifier
action_modifier
recovery_time
treatment_state
```

## Status Effect

```text
status_id
source
severity
duration
stack_rule
modifiers
treatment
```

---

# 20. Phạm vi MVP

Triển khai:

- Health.
- Stamina.
- Fatigue.
- Hunger.
- Thirst.
- Body Temperature.
- Carry Load.
- Cut.
- Bruise.
- Sprain.
- Wet.
- Cold.
- Bleeding.
- Sick.
- Black Water Exposure.
- Disoriented.
- Incapacitation.

Chưa triển khai:

- Mô phỏng từng bộ phận cơ thể chi tiết.
- Tâm lý.
- Dinh dưỡng chuyên sâu.
- Hệ thống bệnh phức tạp.
- Nhiều loại thuốc chuyên biệt.
- Chấn thương vĩnh viễn.

---

# 21. Quyết định chốt

- Player Condition sử dụng sáu chỉ số cốt lõi.
- Injury là trạng thái độc lập với Health.
- Body Temperature là chỉ số quan trọng trong MVP Mưa Đen.
- Wet, Cold và Black Water Exposure là các trạng thái đặc thù chính.
- Carry Load ảnh hưởng trực tiếp đến di chuyển trong vùng ngập.
- Hậu quả tăng theo nhiều mức và phải có cảnh báo rõ.
- Không trạng thái nào được gây thất bại tức thời nếu người chơi chưa có cơ hội phản ứng.
- Multiplayer hỗ trợ Incapacitation và cứu đồng đội.
- MVP không mô phỏng y tế hoặc sinh lý quá chi tiết.
