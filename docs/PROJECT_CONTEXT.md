# Project Context

## Tam nhin

**Fact:** Last Hope la game sinh ton top-down real-time ve viec chuan bi mot shelter truoc bao phong xa.

Nguoi choi co thoi gian, suc khoe va suc chua huu han de ra ngoai tim tai nguyen, doc rui ro, quay ve xu ly loot va chuan bi shelter. Sau giai doan chuan bi, bao phong xa kiem tra nhung lua chon do bang hau qua ro rang.

## Cau hoi thiet ke trung tam

**Decision (`DEC-010`):** Moi he thong phai phuc vu cau hoi:

> Hom nay toi nen mao hiem them bao nhieu de tang co hoi song sot khi bao den?

## Huong rebuild hien tai

**Decision (`DEC-011`):** Implementation cu da bi loai bo. Du an bat dau lai tu lat cat nho nhat: `DayOne`.

Trang thai hien tai:

- Unity 6.5 `6000.5.4f1`, C#, 2D top-down.
- Scene hien tai: `Assets/Scenes/DayOne.unity`.
- Gameplay state hien tai: `DayOneRun`, tach khoi MonoBehaviour de test doc lap.
- Opening hien tai: cold open trong shelter.
- Core loop hien tai: nghe canh bao -> kiem tra kho/filter -> ra ngoai -> loot gan hoac xa -> quay ve -> dung filter -> ket thuc ngay.
- Production art da duoc gan lai o muc phuc vu doc gameplay: map, shelter, survivor animation, loot.
- Edit Mode tests hien tai dat `3/3`.

## Nguyen tac scope

**Decision:** Khong xay lai ban cu theo 6 chu ky. Khong mo rong thanh simulation lon truoc khi Day 1 duoc playtest.

**Assumption:** Lat cat tot tiep theo la hoan thien Day 1 de nguoi moi hieu duoc muc tieu, rui ro va lua chon di tiep/quay ve.

## Nguon su that

Doc theo thu tu:

1. `DECISIONS.md`
2. `GAMEPLAY_DESIGN_BASE.md`
3. `GAME_START_PLAN.md`
4. `MVP_SCOPE.md`
5. `BACKLOG.md`

## Ngoai pham vi hien tai

Combat hoan chinh, enemy AI phuc tap, procedural world, nhieu disaster, NPC simulation, faction/colony, vehicle, meta-progression, save dai han va mot world lon chua thuoc rebuild hien tai.
