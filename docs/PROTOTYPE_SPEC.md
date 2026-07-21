# Prototype Specification

## Trang thai

**Decision (`DEC-011`):** Prototype spec hien tai la `DayOne`, khong phai six-cycle vertical slice cu.

## Player-facing goal

Trong ngay dau, nguoi choi can:

1. Nghe canh bao bao phong xa.
2. Hieu shelter thieu filter.
3. Roi shelter de tim filter.
4. Can nhac quay ve hay di xa lay them material.
5. Quay ve dung filter.
6. Xem summary ngay dau.

## Runtime state

`DayOneRun` quan ly:

- `Hour`
- `Exposure`
- `Filters`
- `Materials`
- `IsOutside`
- Current objective/step
- Completion state

## Interactions

- `radio`: mo canh bao.
- `storage`: cho thay kho du tru yeu.
- `filter_unit`: tao muc tieu tim filter va dung filter khi quay ve.
- `door`: roi shelter sau khi thong tin co ban da du.
- `near_loot`: nhan filter.
- `far_loot`: nhan material them voi chi phi thoi gian/lieu.
- `workbench`: ket thuc buoi toi sau khi ve nha.

## Rules

- Thoi gian va exposure chi tang khi `IsOutside = true`.
- Near loot cho filter can thiet.
- Far loot cho material bo sung.
- Nguoi choi co the hoan tat ngay sau khi co filter va quay ve.
- Debug HUD duoc phep ton tai trong R1 development, nhung can thay truoc playtest rong.

## Acceptance criteria ky thuat

- Edit Mode tests pass.
- Scene `DayOne` duoc bootstrap lai khong mat object chinh.
- Player co the di chuyen, tuong tac va hoan tat flow.
- Khong co missing reference trong scene sau bootstrap.

## Acceptance criteria playtest

Giong `GAME_START_PLAN.md`.
