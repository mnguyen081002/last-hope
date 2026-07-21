# Game Start Plan

## Decision

**Decision (`DEC-011`, `DEC-012`):** Game bat dau bang cold open trong shelter va dua nguoi choi ra chuyen loot dau tien trong 60-90 giay.

## Muc tieu

Nguoi moi can hieu bang hanh dong:

- Bao phong xa dang den.
- Shelter chua san sang.
- Filter la muc tieu dau tien.
- Ra ngoai co loi nhung tang thoi gian/lieu.
- Quay ve moi bien loot thanh tien do.

## Flow 0:00 den 1:30

1. **Radio warning:** radio phat canh bao bao phong xa co the den trong vai ngay.
2. **Storage check:** kho cho thay du tru thap.
3. **Filter check:** he thong loc can filter.
4. **Door unlock:** chi mo cua khi nguoi choi da hieu nhu cau co ban.
5. **Near loot:** diem gan cho filter voi rui ro thap.
6. **Continue choice:** sau filter, nguoi choi co the quay ve hoac di xa lay material.
7. **Return home:** dung filter de ket thuc ngay.

## First playable loop

1. Lay filter o diem gan hoac di xa them de lay material.
2. Thoi gian va lieu chi tang khi o ngoai.
3. HUD khong ep quay ve; chi hien trang thai can thiet.
4. Khi ve shelter, filter duoc dung cho may loc.
5. Summary cuoi ngay cho thay loot, lieu va thoi gian.

## Acceptance criteria

- 4/5 nguoi moi noi dung rang bao dang den va shelter can chuan bi.
- 4/5 tu roi shelter trong 90 giay ma khong duoc giai thich mieng.
- Tat ca hieu diem loot gan an toan hon diem xa.
- It nhat 3/5 can nhac di tiep sau loot dau tien.
- Sau ngay 1, nguoi choi ke lai duoc mot trade-off giua loot, lieu va thoi gian.

## Implementation order

1. Hoan thien search duration/cancel.
2. Tao summary cuoi ngay.
3. Them reset/replay trong scene.
4. Thay HUD debug bang HUD onboarding ro rang.
5. Chay 5 playtest moi.
