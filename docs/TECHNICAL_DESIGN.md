# Technical Design

## Baseline

- Unity 6.5 `6000.5.4f1`.
- C#.
- 2D top-down.
- Keyboard/mouse truoc.
- Khong them package ben thu ba neu chua co ly do ro.

## Architecture principles

- Gameplay rules nam trong C# classes co the test.
- MonoBehaviour chi dieu phoi input, scene object va presentation.
- Khong tao abstraction tong quat cho he thong chua ton tai.
- Khong global mutable state.
- Bootstrap scene phai co the chay lai de tao `DayOne` sach.

## Current structure

```text
Assets/
  Art/
  Audio/
  Editor/
    FreshProjectBootstrap.cs
  Scenes/
    DayOne.unity
  Scripts/
    Core/
      DayOneRun.cs
    Gameplay/
      CameraFollow2D.cs
      DayOneDirector.cs
      DayOneInteractable.cs
      DirectionalSpriteAnimator.cs
      PlayerMotor.cs
  Tests/
    EditMode/
      DayOneRunTests.cs
```

## Runtime boundaries

### Core

`DayOneRun` la domain state. No khong biet Unity scene, GameObject, input hay sprite.

### Gameplay presentation

- `DayOneDirector`: noi input, interactables, HUD debug va `DayOneRun`.
- `DayOneInteractable`: gan id tuong tac vao scene object.
- `PlayerMotor`: di chuyen top-down bang Rigidbody2D.
- `DirectionalSpriteAnimator`: chon idle/walk theo huong.
- `CameraFollow2D`: camera theo player.

### Editor bootstrap

`FreshProjectBootstrap` tao lai scene `DayOne`, gan art co san, tao player, camera, shelter, map va interactables.

## Testing

Edit Mode tests phai bao ve rules quan trong:

- Thu tu objective ngay dau.
- Thoi gian/lieu chi tang ngoai shelter.
- Flow loot -> quay ve -> complete hoat dong.

## Near-term technical debt

- HUD dang la debug UI.
- Search loot chua co duration/cancel.
- Summary cuoi ngay chua co UI rieng.
- Reset/replay chua hoan thien trong runtime.
