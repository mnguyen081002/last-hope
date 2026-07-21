# Decision Log

Tai lieu nay chi chua quyet dinh da duoc project owner duyet. De xuat va gia dinh nam trong tai lieu rieng khong duoc coi la quyet dinh.

## DEC-010 - Product design baseline bao phong xa

- **Date:** 2026-07-20
- **Question:** Ban thiet ke gameplay bao phong xa moi duoc dung o cap do nao?
- **Decision:** `GAMEPLAY_DESIGN_BASE.md` la product design baseline cua Last Hope.
- **Status:** Approved
- **Rationale:** Baseline moi lam ro fantasy dai han: chuan bi shelter truoc bao, di/ve nha trong cac ngay huu han, va chiu hau qua trong giai doan bao.
- **Consequences:** Khong dua toan bo baseline vao code cung luc. Moi he thong can duoc mo theo lat cat playtest.
- **Approved by:** Project owner.

## DEC-011 - Rebuild implementation tu dau theo baseline moi

- **Date:** 2026-07-20
- **Question:** Tiep tuc sua vertical slice cu hay loai bo de xay lai theo baseline moi?
- **Decision:** Loai bo gameplay implementation, scene, data va test cua vertical slice cu. Giu Unity 6.5, project settings, tai lieu can thiet va art/audio co the tai su dung. Implementation moi bat dau bang `DayOne` voi cold open trong shelter.
- **Status:** Approved
- **Rationale:** Ban cu duoc xay quanh 6 chu ky ngan, 5 resource, 6 slot va 3 shelter module. Cau truc do khong con la nen phu hop cho baseline moi theo ngay chuan bi va giai doan bao.
- **Consequences:** M0-M6 cu la lich su va khong con la implementation hien hanh. Source moi dung `DayOneRun`, scene `DayOne`, va tests moi. He thong dai han chi duoc them theo tung lat cat sau playtest.
- **Supersedes:** Pham vi implementation cu, bao gom prototype spec va release checklist cu.
- **Approved by:** Project owner.

## DEC-012 - Day 1 la lat cat dau tien

- **Date:** 2026-07-21
- **Question:** Sau khi rebuild, bat dau gameplay o dau?
- **Decision:** Tap trung hoan thien Day 1 truoc: cold open, shelter checks, chuyen loot dau tien, lua chon di tiep/quay ve, dung filter va summary cuoi ngay.
- **Status:** Approved
- **Rationale:** Neu nguoi moi chua hieu ngay dau tien, them simulation dai han se lam kho xac dinh van de nam o core loop hay onboarding.
- **Consequences:** Backlog uu tien R1. R2 tro di chi mo sau khi R1 duoc playtest.
- **Approved by:** Project owner.
