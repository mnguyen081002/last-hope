1. Mục tiêu

Tài liệu này chuyển toàn bộ thiết kế MVP Siêu Bão Mưa Đen thành kế hoạch triển khai có thứ tự, phụ thuộc, tiêu chí hoàn thành và điểm kiểm tra phạm vi.

Kế hoạch ưu tiên:

Kiểm chứng Core Loop.
Xây nền tảng kỹ thuật ổn định.
Hoàn thiện Vertical Slice.
Chỉ sản xuất toàn bộ nội dung sau khi Vertical Slice đạt yêu cầu.
Tránh mở rộng sang Multiplayer, Combat hoặc Campaign trước khi MVP hoàn thành. 2. Giả định triển khai

Baseline kế hoạch:

Team Size: 1 developer
Platform: PC
Mode: Single-player
Visual: Isometric 3D
Camera: Orthographic, fixed angle
Asset Pipeline: AI generation + manual cleanup
World Structure: Location và Shelter tách thành scene
Development Style: Data-driven

Thời gian ước tính:

Các mốc thời gian chỉ là baseline quản lý phạm vi, không phải cam kết cố định.

3. Quyết định đã khóa trước khi code

Trước Milestone 1 cần chốt:

Engine và phiên bản: Unity 6000.5.4f1
Ngôn ngữ lập trình.
Hệ thống input.
Hệ thống camera.
Định dạng Definition Data.
Cách chia Scene.
Cách serialize Save.
Công cụ quản lý source control.
Nền tảng build đầu tiên.

Không cần khóa ngay:

Multiplayer framework hoàn chỉnh.
Art style cuối cùng.
Toàn bộ UI.
Toàn bộ Recipe.
Campaign Chapter sau. 4. Nguyên tắc triển khai
4.1. Xây theo lát dọc

Mỗi Milestone phải tạo được một vòng chơi hoàn chỉnh nhỏ.

Không xây riêng toàn bộ:

Inventory.
Shelter.
Event.
NPC.

mà chưa kết nối chúng thành gameplay.

4.2. Data-driven từ đầu

Các nội dung sau không được hard-code trực tiếp trong gameplay logic:

Item.
Resource.
Recipe.
Location.
Route.
Hazard.
Event.
NPC.
Disaster Phase.
Shelter Module.

Code hệ thống xử lý Definition Data và Runtime State.

4.3. Một World State duy nhất

Mọi hệ thống quan trọng phải đọc và ghi qua Runtime World State.

Không lưu trạng thái quan trọng chỉ trong:

Scene object.
Animation.
UI.
Local interaction component.
4.4. Debug trước content

Các công cụ debug phải được xây cùng hệ thống:

Chỉnh World Clock.
Chuyển Disaster Phase.
Thêm Resource.
Kích hoạt Event.
Thay đổi Hazard.
Teleport.
Xem Runtime State.

Không chờ tới cuối mới làm Debug Tool.

4.5. Placeholder trước asset hoàn chỉnh

Trong P1–P4 sử dụng:

Primitive mesh.
Material đơn sắc.
Animation tối thiểu.
Icon tạm.
UI debug.

AI asset chỉ bắt đầu tích hợp có hệ thống sau khi Camera, Scale và Modular Grid được khóa.

5. Dependency Graph
   Project Foundation
   ↓
   World Clock + Runtime State
   ↓
   Item + Inventory
   ↓
   Interaction + Search
   ↓
   Route + Travel
   ↓
   Player Condition + Hazard
   ↓
   Shelter + Task
   ↓
   Event + Information
   ↓
   NPC
   ↓
   Disaster Timeline
   ↓
   Outcome
   ↓
   Full Content

Save System phải được tích hợp sớm, không để tới cuối.

6. Milestone 0 — Project Setup
   Thời lượng mục tiêu
   3–5 ngày
   Công việc
   Tạo repository.
   Thiết lập branch strategy.
   Tạo project engine.
   Thiết lập coding convention.
   Thiết lập folder structure.
   Thiết lập input cơ bản.
   Tạo camera isometric orthographic.
   Tạo test scene.
   Thiết lập logging.
   Thiết lập automated test foundation.
   Tạo build PC đầu tiên.
   Folder Structure đề xuất
   /Docs
   /Game
   /Core
   /Systems
   /Gameplay
   /Presentation
   /UI
   /Debug
   /Data
   /Definitions
   /Balance
   /Localization
   /Scenes
   /Shelters
   /Locations
   /Routes
   /Tests
   /Assets
   /Characters
   /Environment
   /Props
   /VFX
   /Audio
   /Tests
   /Unit
   /Integration
   /PlayMode

Tên folder có thể thay đổi theo engine, nhưng trách nhiệm phải được tách rõ.

Deliverable

Một build có:

Nhân vật placeholder.
Camera isometric.
Di chuyển trong test room.
Log và debug overlay cơ bản.
Exit Criteria
Build chạy ổn định.
Camera không xoay.
Nhân vật di chuyển đúng hướng màn hình.
Scale chuẩn được xác định.
Repository và quy trình commit hoạt động. 7. Milestone 1 — Technical Foundation
Thời lượng mục tiêu
2 tuần
Hệ thống
Definition Registry
Load Definition Data.
Tra cứu bằng ID.
Validate reference.
Phát hiện ID trùng.
Báo lỗi data khi khởi động.
Runtime World State
Khởi tạo Chapter.
Lưu World Time.
Lưu Player State.
Lưu Location, Route và Shelter State.
Persistent Flag.
World Clock
Tỷ lệ 1 phút thực = 5 phút game.
Không phụ thuộc framerate.
Day và Time of Day.
Pause chỉ dành cho debug, không phải gameplay.
Phase transition hook.
Simulation Tick
Frame Update.
Short Tick.
Long Tick.
Tick subscription.
Command Layer

Các Command đầu tiên:

MoveItem
StartTask
CancelTask
BeginTravel
StartSearch
StopSearch
StartSleep
Save Foundation
Serialize World State.
Load World State.
Save Version.
Autosave test.
Random Seed.
Debug Panel
World Clock.
Tick Speed debug.
Add Item.
View State.
Save.
Load.
Test bắt buộc
Clock không sai sau thời gian dài.
Save và Load giữ đúng World Time.
Tick không chạy hai lần.
Definition reference thiếu bị báo lỗi.
Seed không thay đổi sau Load.
Exit Criteria
Chạy được 24 giờ World Time mô phỏng không lỗi.
Save và Load khôi phục cùng Runtime State.
Không có gameplay system nào phụ thuộc trực tiếp UI.
Debug Panel có thể điều khiển World Clock. 8. Milestone 2 — Exploration Loop
Thời lượng mục tiêu
2–3 tuần
Phạm vi nội dung
1 Main Shelter placeholder
1 Route
1 Cửa hàng tiện lợi prototype
Hệ thống
Interaction
Detect interactable.
Interaction prompt.
Hold interaction.
Cancel interaction.
Interaction validation.
Item
Item Definition.
Item Instance.
Weight.
Volume.
Stack.
Condition.
Wet và Contamination field.
Inventory
Backpack.
Equipment.
Quick Slot.
Transfer.
Drop.
Carried Object.
Overload.
Search
Search Point.
Real-time progress.
Progressive reveal.
Cancel.
Persistent depletion.
Storage
Shelter storage.
Transfer Player ↔ Storage.
Capacity.
Item state preservation.
Route và Travel

Phiên bản đầu có thể dùng travel transition thay vì đi bộ toàn bộ khoảng cách.

Travel phải:

Tiêu thụ World Time thực.
Có thể bị ảnh hưởng bởi Carry Load.
Có Destination rõ.
Lưu Arrival State.
Gameplay Slice
Chuẩn bị tại Shelter
↓
Đi tới Cửa hàng
↓
Search
↓
Chọn Resource
↓
Bị giới hạn Weight và Volume
↓
Quay về Shelter
↓
Cất Resource
↓
Save và Load
Telemetry
Search duration.
Item collected.
Item left behind.
Inventory overload.
Travel duration.
Location revisit.
Resource stored.
Exit Criteria
Người chơi phải bỏ lại ít nhất một Resource có giá trị.
Search có thể dừng giữa chừng.
Location không hồi loot sau Load.
Item giữ nguyên Condition và Contamination.
Một vòng Expedition hoàn chỉnh hoạt động ổn định.
Inventory không cần grid puzzle để tạo quyết định.
Không triển khai trong Milestone này
Flood.
Build.
NPC.
Event phức tạp.
Art hoàn chỉnh. 9. Milestone 3 — Player Condition and Hazard
Thời lượng mục tiêu
2–3 tuần
Phạm vi nội dung
2 Route
2 Location
1 Disaster Timeline rút gọn
Hệ thống
Player Condition
Health.
Stamina.
Fatigue.
Hunger.
Thirst.
Body Temperature.
Status Effect.
Incapacitation cơ bản.
Equipment Protection
Áo mưa.
Ủng.
Găng tay.
Rope.
Ba lô chống nước.
Hazard
Rain.
Wet.
Flood Depth.
Current Strength.
Black Water Exposure.
Electrified Water cục bộ.
Route State
Dry.
Shallow.
Medium.
Deep.
Impassable.
Return Window

World Map hiển thị:

Travel Time.
Estimated Return Time.
Phase Change Risk.
Known Hazard.
Gameplay Slice
Route ngắn nhưng ngập
OR
Route dài nhưng an toàn

Người chơi phải thay đổi:

Route.
Equipment.
Carry Load.
Thời điểm quay về.
Exit Criteria
Flood thay đổi lựa chọn Route.
Equipment tạo đánh đổi rõ.
Exposure có cảnh báo và cách xử lý.
Route Closure không gây softlock.
Người chơi hiểu rủi ro chiều về.
Hazard State tồn tại sau Save và Load. 10. Milestone 4 — Shelter Loop
Thời lượng mục tiêu
3–4 tuần
Phạm vi Shelter
Ground Floor.
Upper Floor.
Entrance.
Utility Area.
Storage.
Upper Safe Area.
Hệ thống
Shelter State
Structural Integrity.
Water Intrusion.
Living Capacity.
Storage State.
Power State.
Occupants.
Build and Placement
Build Slot.
Placement validation.
Material delivery.
Construction progress.
Interruptible build.
Dismantle cơ bản.
Task System
Active Task.
Passive Task.
Pause.
Resume.
Cancel.
Resource reservation.
Module

Milestone này chỉ cần:

Flood Barrier.
Portable Pump.
Elevated Storage.
Water Purifier.
Battery Bank.
Power System
City Grid.
Generator hoặc Battery.
Power Demand.
Priority.
Allocation.
Water System
Clean Water.
Untreated Water.
Purification Batch.
Filter.
Contamination.
Sleep
Sleep validation.
Tick simulation.
Event interruption.
Resource consumption.
Test Scenario

Người chơi chỉ đủ Resource xây hai trong ba:

Pump
Elevated Storage
Water Purifier
Exit Criteria
Có ít nhất ba chiến lược Shelter khả thi.
Không Module nào giải quyết toàn bộ Peak.
Passive Task tiếp tục khi rời Shelter.
Sleep không bỏ qua Simulation.
Lower Floor có thể bị mất mà game vẫn tiếp tục.
Power Priority tạo đánh đổi thực tế. 11. Milestone 5 — Event, Information and NPC
Thời lượng mục tiêu
3–4 tuần
Event System
Event Definition.
Event Instance.
Trigger.
Discovery.
Soft Deadline.
Hard Deadline.
Resolution.
Expiration.
Persistent Consequence.
Information System
Intel Record.
Confidence.
Information Age.
Map marker.
Forecast.
Route Intel.
Event Intel.
NPC

Triển khai một NPC đầu tiên:

Nguyễn Minh

Chức năng:

Recruitment.
Consumption.
Trust đơn giản.
Shelter Task.
Expedition Support.
Event Chain.
Injury và Death State.
Event Slice
Storm Warning.
Black Rain Transition.
Neighbor Introduction.
Drain Backflow.
Storage Flood Risk.
Grid Failure.
Exit Criteria
Event có thể kích hoạt ngoài Scene.
Event có thể hết hạn khi người chơi không có mặt.
World Map chỉ hiển thị Intel đã biết.
Intel có thể lỗi thời.
NPC tạo lợi ích và Resource Pressure.
NPC Task tiếp tục theo World Clock.
Event State tồn tại sau Save và Load. 12. Milestone 6 — Disaster Vertical Slice
Thời lượng mục tiêu
3–5 tuần
Thời lượng chơi mục tiêu
60–90 phút
Nội dung
Shelter
Main Shelter phiên bản rút gọn.
Temporary Shelter đơn giản.
Location
Cửa hàng tiện lợi.
Gara điện nước.
Trường học.
Route
Commercial Route.
Residential Route.
Shortcut tùy chọn.
Disaster Phase
Warning.
Black Rain.
Escalation.
Peak.
Aftermath ngắn.
NPC
Nguyễn Minh.
Một NPC thứ hai nếu phạm vi cho phép.
Event
Storm Warning.
Black Rain Transition.
School Rescue.
Grid Failure.
Drain Backflow.
Pump Jam hoặc Storage Flood.
Outcome
Stable Survival.
Forced Evacuation.
Collapse.
Art Scope
Modular environment kit cơ bản.
AI-generated prop đã cleanup.
Một character base.
Lighting và rain VFX cơ bản.
Audio cảnh báo cơ bản.
Go/No-Go Gate

Chỉ chuyển sang sản xuất Full MVP khi:

Slice hoàn thành từ đầu tới cuối.
Có ít nhất hai chiến lược sống sót.
Không thể hoàn thành mọi mục tiêu.
Peak phản ánh chuẩn bị.
Save và Load ổn định.
Không có softlock.
Tester hiểu nguyên nhân Outcome.
Core Loop đủ hấp dẫn để chơi lại.

Nếu không đạt, thiết kế lại trước khi thêm content.

13. Milestone 7 — Full MVP Content Production
    Thời lượng mục tiêu
    8–12 tuần
    Location Production Order
    Khu nhà dân.
    Cửa hàng tiện lợi.
    Gara điện nước.
    Trường học.
    Hiệu thuốc.
    Trạm bơm.
    Trạm thời tiết.

Mỗi Location phải hoàn thành theo Definition of Done trước khi chuyển Location tiếp theo.

NPC Production Order
Nguyễn Minh.
Trần Mai.
Lê Hùng.
Phạm An.
Event Production Order
Global Main Event.
Shelter Critical Event.
Location Main Event.
NPC Event Chain.
Optional Event.
Narrative Event.
Shelter Content

Hoàn thiện:

Tám Zone.
Sáu Fixed Core Component.
Bảy Module.
Forced Evacuation.
Temporary Shelter.
Disaster Content

Hoàn thiện:

Bảy Phase.
Route transition.
Infrastructure State.
Forecast.
Peak Signal.
Aftermath.
Exit Criteria
Toàn bộ Chapter có thể hoàn thành.
Bảy Location hoạt động.
Bốn NPC có Outcome.
14 Main Event hoạt động.
Tối thiểu tám Optional Event hoàn thành.
Năm Outcome được tính đúng.
Không Resource bắt buộc nào phụ thuộc RNG duy nhất. 14. Milestone 8 — Balance and Integration
Thời lượng mục tiêu
4–6 tuần
Công việc
Cân bằng Travel Time.
Cân bằng Search Time.
Cân bằng Carry Load.
Cân bằng Water và Food.
Cân bằng Fuel và Power.
Cân bằng Module.
Cân bằng Hazard.
Cân bằng Event Deadline.
Cân bằng NPC Consumption.
Cân bằng Outcome Threshold.
Fix softlock.
Fix save migration.
Test Strategy

Chạy các hướng:

Resource First
Shelter First
Information First
NPC Rescue
Minimal Preparation
Forced Evacuation
Playtest Minimum
20 internal playthrough
10 external playthrough
Exit Criteria
Có ít nhất ba chiến lược thắng.
Không có Module không-tutorial nào được xây trong trên 90% lượt.
Không Location tùy chọn nào bắt buộc trong phần lớn lượt thắng.
Collapse không chủ yếu do thiếu thông tin.
Resource cuối Chapter không dư thừa nghiêm trọng.
Forced Evacuation hoạt động như một Outcome hợp lệ. 15. Milestone 9 — Polish and Release Candidate
Thời lượng mục tiêu
4–8 tuần
Art
Thay placeholder quan trọng.
Chuẩn hóa material.
Cleanup AI asset.
Tối ưu polygon.
LOD nếu cần.
Collider.
Wall fade.
Animation cleanup.
VFX mưa và ngập.
Lighting theo Phase.
Audio
Rain Layer.
Drain Warning.
Pump.
Electrical Hazard.
Structural Warning.
Radio Interference.
Event Alert.
Ambient theo District.
UI/UX
Tutorial.
Shelter Overview.
World Map.
Event Deadline.
Intel Confidence.
Inventory.
Power Allocation.
Outcome Report.
Settings.
Accessibility cơ bản.
Technical
Performance.
Memory.
Loading.
Save corruption handling.
Crash logging.
Build packaging.
Input remapping.
Resolution support.
Release Criteria
Không còn Blocker.
Chapter hoàn thành ổn định.
Save và Load đáng tin cậy.
Tutorial không giới thiệu hệ thống mới trong Peak.
Outcome Report chính xác.
Performance đạt target trên cấu hình mục tiêu.
Không có asset AI chưa được kiểm tra license và cleanup. 16. Definition of Done cho một hệ thống

Một hệ thống chỉ được xem là hoàn thành khi:

Definition Data tồn tại.
Runtime State tồn tại.
Command được validate.
Save và Load hoạt động.
Debug Tool hỗ trợ.
UI hiển thị trạng thái cần thiết.
Có automated test hoặc integration test phù hợp.
Có ít nhất một gameplay scenario sử dụng hệ thống.
Telemetry được ghi.
Không tạo softlock đã biết. 17. Definition of Done cho một Location

Một Location hoàn thành khi:

Blockout hoàn chỉnh.
Entrance hoạt động.
Zone hoạt động.
Search Point hoạt động.
Controlled Resource Placement hoàn chỉnh.
Depletion được lưu.
Hazard transition hoạt động.
Event Anchor hoạt động.
Return Hook hoạt động.
Alternative Access được kiểm tra.
Art và collider được cleanup.
Save và Load giữ đúng State.
Performance đạt yêu cầu. 18. Definition of Done cho một NPC

Một NPC hoàn thành khi:

NPC Definition.
Runtime State.
Recruitment.
Consumption.
Skill.
Trait.
Shelter Task.
Event Chain.
Injury và Death State.
Persistent Outcome.
Save và Load.
UI hoặc hội thoại tối thiểu.
Không phụ thuộc RNG để sống hoặc chết. 19. Definition of Done cho một Event

Một Event hoàn thành khi:

Trigger hoạt động.
Discovery hoạt động.
Deadline hoạt động.
Ít nhất hai phản ứng hợp lý nếu là Critical Event.
Success, Failure và Expiration được xử lý.
Persistent Flag được lưu.
Event có thể chạy ngoài Scene.
Event hoạt động khi Sleep Simulation.
UI cảnh báo rõ.
Telemetry hoạt động. 20. Test Pyramid
Unit Test

Ưu tiên:

World Clock.
Resource calculation.
Inventory validation.
Power allocation.
Event condition.
Outcome evaluation.
Save serialization.
Integration Test

Ưu tiên:

Sleep + Event.
Travel + Route Closure.
Search + Save.
Pump + Power Loss.
NPC + Resource Consumption.
Storage + Contamination.
Playtest

Dùng để kiểm chứng:

Core Loop.
Tension.
Clarity.
Strategic diversity.
Fairness.
Replay value.

Automated test không thay thế playtest.

21. Asset Pipeline

Tạo tài liệu riêng:

11-art-direction-and-asset-pipeline.md

Pipeline tối thiểu:

AI Concept
↓
AI 3D Generation hoặc manual blockout
↓
Blender Cleanup
↓
Scale và Pivot
↓
Topology và UV check
↓
Material normalization
↓
Collider
↓
Engine Import
↓
Isometric Camera Review
↓
Performance Review

Asset gameplay-critical phải ưu tiên:

Scale chính xác.
Collider chính xác.
Silhouette rõ.
Pivot đúng.
Material nhất quán.

Không ưu tiên chi tiết bề mặt nhỏ không nhìn thấy từ camera.

22. Scope Cut Order

Khi vượt tiến độ, cắt theo thứ tự:

Optional Event thứ chín trở đi.
Alternative NPC dialog.
Advanced Signal Narrative.
Trạm thời tiết có nhiều biến thể.
Shortcut nâng cấp.
NPC Expedition AI nâng cao.
Crafting Recipe phụ.
Shelter Module ít dùng.
Temporary Shelter Upgrade phụ.

Không cắt:

World Clock.
Exploration Loop.
Inventory Decision.
Flood Route Change.
Shelter Preparation.
Peak Phase.
Save và Load.
Outcome.
Forced Evacuation tối thiểu. 23. Risk Register
Real-time Task gây chờ đợi

Kiểm soát bằng:

Task ngắn.
Task có thể gián đoạn.
Passive Task.
Nhiều việc song song.
NPC hỗ trợ.
Hệ thống quá phụ thuộc nhau

Kiểm soát bằng:

Command Layer.
Event Bus.
Definition Data.
Runtime State độc lập.
Integration Test.
Save quá phức tạp

Kiểm soát bằng:

Save từ Milestone 1.
Stable ID.
Versioning.
Không lưu reference Scene trực tiếp.
AI asset thiếu nhất quán

Kiểm soát bằng:

Style Guide.
Modular Kit.
Một scale chuẩn.
Một material convention.
Camera cố định.
Cleanup bắt buộc.
Content production quá lớn

Kiểm soát bằng:

Vertical Slice Gate.
Content Budget.
Definition of Done.
Không thêm Location trước khi Location hiện tại hoàn chỉnh. 24. Backlog Priority
P0 — Blocker Foundation
World Clock.
Runtime State.
Save.
Definition Registry.
Debug Panel.
P1 — Core Loop
Interaction.
Inventory.
Search.
Travel.
Storage.
Carry Load.
P2 — Disaster Gameplay
Player Condition.
Flood.
Black Water.
Route State.
Shelter Intrusion.
Power.
P3 — Content Framework
Event.
Information.
NPC.
Outcome.
Tutorial.
P4 — Production and Polish
Full Location.
Optional Event.
Narrative.
AI Asset.
Audio.
Final UI.

Không làm P4 trước khi các nhiệm vụ P0–P2 tương ứng đạt Exit Criteria.
