# Need User Playtest

Danh sách gộp mọi hạng mục đã có code (trạng thái `Verify` trong `BACKLOG.md`) nhưng **chưa
được user tự chơi xác nhận bằng mắt**. Khi user playtest xong một mục: xoá dòng khỏi bảng này
+ đổi trạng thái tương ứng trong `BACKLOG.md` từ `Verify` sang `Done` trong cùng lúc.

Không liệt kê `BL-P3-03` (Build và Placement) — đang làm lại sang free placement, chưa tới
lúc playtest.

## P3 — Shelter Loop

Script chi tiết, từng bước: `docs/plans/2026-07-28-p3-test-scenarios.md` (Scenario A–G).

| BL-ID | Hạng mục | Xác nhận gì | Scenario |
| --- | --- | --- | --- |
| BL-P3-01 | Main Shelter blockout / Z-level tầng | Đi bộ qua vùng cầu thang tự đổi tầng (không bấm phím); tầng dưới hiện mờ không va chạm được, tầng trên ẩn hẳn; không nhấp nháy khi đi qua lại ranh giới | G |
| BL-P3-02 | Shelter State | Overview đúng trong `ShelterPanel` (Structural/Water/Clean-Untreated Water/Battery) | A–F |
| BL-P3-04 | Task System (Construction) | Xây dở rồi rời Shelter/Sleep — tiến độ vẫn chạy khi quay lại | B |
| BL-P3-05 | Water Intrusion | Ground Floor khoá chức năng khi Water Intrusion ≥ Deep, không kết thúc game | C |
| BL-P3-06 | Module: Flood Barrier | Giảm inflow, Durability tự hao mòn | A |
| BL-P3-07 | Module: Portable Pump | Cần điện mới bơm được, Pump Jam làm ngừng hoạt động | B, E |
| BL-P3-08 | Module: Elevated Storage | Miễn Storage Flood Risk khi đã xây | D |
| BL-P3-09 | Module: Water Purifier | Batch Untreated→Clean, Filter hao dần | A |
| BL-P3-10 | Module: Battery Bank | Không chặn test riêng — quan sát chung qua Scenario B |
| BL-P3-11 | Power System | Priority quyết định module nào có điện khi Grid mất | B |
| BL-P3-12 | Water System | Water Intake thụ động + Purifier batch | A |
| BL-P3-13 | Sleep Simulation | Ngủ hồi Fatigue + chữa Black Water Exposure, Sick tự tắt | F |
| BL-P3-14 | Event: Drain Backflow | Kích hoạt ở RouteClosure, giải quyết được qua nút "Xử lý" | E |
| BL-P3-15 | Event: Storage Flood Risk | Cảnh báo + mất đồ nếu không có Elevated Storage | D |
| BL-P3-16 | Event: Pump Jam | Kích hoạt khi Pump có điện, giải quyết qua nút "Sửa" | E |
| BL-P3-17 | Kịch bản 2-trong-3 | Khan hiếm vật liệu tự nhiên chỉ cho xây 2/3 Module chính | A |

Sau khi playtest xong toàn bộ bảng trên (trừ BL-P3-03), quay lại `BACKLOG.md` để chạy Gate P3
— vẫn còn BL-P3-03 (làm lại) và BL-P3-18 (Telemetry, chưa quyết định phạm vi) cần xử lý trước
khi đóng Gate hẳn.
