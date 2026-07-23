# Last Hope — Code Map

Bảng tra cứu nhanh: hệ thống nào đã có, ở file nào, API chính là gì, test tới đâu. Đọc file này (+ `BACKLOG.md`) đầu mỗi session thay vì quét lại `Assets/`. Cập nhật cùng commit mỗi khi thêm/đổi hệ thống — không để lệch code thực tế.

Quy ước cột "Test": ⬜ chưa có test · 🟡 có test một phần · ✅ có EditMode/PlayMode test bao phủ chính.

---

## Assembly map (dependency một chiều)

```
LastHope.Data ← LastHope.Core ← LastHope.Systems ← LastHope.Presentation / LastHope.UI / LastHope.DebugTools
Tests.EditMode / Tests.PlayMode → tham chiếu tất cả assembly trên
LastHope.EditorTools (Editor-only) → Core, Data, Presentation, DebugTools, Unity.InputSystem, URP Runtime
```

## Scene flow

`00_Boot` (BootLoader) → additive `10_GamePersistent` (services sống suốt phiên) → additive gameplay scene đầu (`90_TestSystems` ở Sprint 1).

---

## LastHope.Core

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Core/Logging/GameLog.cs` | `GameLog` (static) + `LogCategory` enum | `Info/Warn/Error(LogCategory, string)` | ⬜ |

## LastHope.Data

Chưa có class nào (asmdef trống, chờ S2).

## LastHope.Systems

Chưa có class nào (asmdef trống, chờ S2/S3).

## LastHope.Presentation

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/Presentation/Camera/CameraRig.cs` | `CameraRig` | Orthographic iso cố định (pitch 35.264°/yaw 45°), zoom qua Input System action "Zoom". `SetTarget(Transform)`, `SetInputActions(InputActionAsset)` | ⬜ (chỉ headless smoke test, chưa unit test) |
| `Assets/Game/Presentation/Player/PlayerController.cs` | `PlayerController` | CharacterController, di chuyển theo hướng camera (screen-relative), framerate-độc lập. `SpeedModifier` (hook cho Carry Load/Flood sau này), `SetCameraTransform`, `SetInputActions` | ⬜ |
| `Assets/Game/Presentation/Boot/BootLoader.cs` | `BootLoader` (MonoBehaviour, sống trong `00_Boot`) | Load `10_GamePersistent` rồi `90_TestSystems` (additive, tuần tự). Chưa load Definition Data / World State (sẽ nối vào `GameBootstrapper` ở S3) | ⬜ |
| `Assets/Game/Presentation/Boot/GamePersistentMarker.cs` | `GamePersistentMarker` | `DontDestroyOnLoad` cho root scene `10_GamePersistent`, chặn instance thứ 2 | ⬜ |

## LastHope.UI

Chưa có class nào (asmdef trống).

## LastHope.DebugTools

| File | Class | API chính | Test |
| --- | --- | --- | --- |
| `Assets/Game/DebugTools/Overlay/DebugOverlay.cs` | `DebugOverlay` | OnGUI overlay, toggle **F1**: FPS, world position, build version. Tự tìm GameObject tag "Player" nếu chưa gán. **Chưa hiển thị World Clock/State** (đó là `DebugPanel` ở S4, phím F2) | ⬜ |

## LastHope.EditorTools (Editor-only, không build vào Player)

| File | Class | API chính | Ghi chú |
| --- | --- | --- | --- |
| `Assets/Game/EditorTools/SceneSetup.cs` | `SceneSetup` | `[MenuItem] BuildAll()` — dựng lại `00_Boot`/`10_GamePersistent`/`90_TestSystems` từ code, đăng ký Build Settings | Chạy lại bất cứ khi nào cần tái tạo scene từ đầu (deterministic) |
| `Assets/Game/EditorTools/RenderPipelineSetup.cs` | `RenderPipelineSetup` | `[MenuItem] Setup()` — tạo `Assets/Settings/LastHope_URP.asset` + Renderer, gán Graphics+Quality, Linear color space | Đã chạy 1 lần, asset đã tồn tại — chạy lại thì tái sử dụng asset cũ (idempotent) |
| `Assets/Game/EditorTools/BuildScript.cs` | `BuildScript` | `[MenuItem] BuildWindowsDevelopment()` → `Builds/Windows/LastHope.exe`, Mono, Development build | Dùng làm smoke test nhanh sau mỗi sprint |

## Input

| File | Nội dung |
| --- | --- |
| `Assets/Input/GameControls.inputactions` | Action map "Gameplay": `Move` (Vector2, WASD composite), `Zoom` (Axis, scroll), `Interact` (Button, E — **chưa có code nào đọc action này**, chờ S5 Interaction System) |

## Data định nghĩa game (chưa có nội dung)

`Assets/StreamingAssets/Definitions/` — chỉ có `README.md` placeholder. JSON thật (`manifest.json`, `items_p1.json`, ...) sẽ thêm ở S2/S5.

## Render / Project settings đã cấu hình (S1)

- URP asset: `Assets/Settings/LastHope_URP.asset` (+ `LastHope_Renderer.asset`), gán vào `GraphicsSettings` + toàn bộ Quality level.
- Color space: Linear.
- Packages đã thêm: `com.unity.render-pipelines.universal@17.5.0`, `com.unity.inputsystem@1.20.0` (⚠ 1.11.2/1.12.0 lỗi compile với Unity 6000.5.4f1 — không hạ version), `com.unity.nuget.newtonsoft-json@3.2.1`, `com.unity.modules.physics@1.0.0`.
- Build Settings scenes (thứ tự): `00_Boot` → `10_GamePersistent` → `90_TestSystems`.

---

## Việc CHƯA làm (để tránh giả định nhầm khi đọc code)

- Không có Definition Registry / WorldState / Save / Command / EventBus / Tick — toàn bộ nằm ở M1 (S2–S4), chưa viết dòng nào.
- `DebugOverlay` (F1) là overlay tối thiểu Sprint 1, KHÔNG phải Debug Panel v1 (F2, sẽ thêm state tree + save/load ở S4) — hai file khác nhau, đừng nhầm.
- `PlayerController.SpeedModifier` tồn tại nhưng chưa có hệ thống nào set nó (Carry Load/Flood ở M2/P2).
- Input action "Interact" (E) đã khai báo trong `.inputactions` nhưng chưa có script nào subscribe.
- Chưa có Content JSON nào — mọi Location/Item/Route trong bảng baseline (`docs/plans/2026-07-24-mvp-coding-plan.md`) mới là số liệu dự kiến, chưa nhập vào file thật.
