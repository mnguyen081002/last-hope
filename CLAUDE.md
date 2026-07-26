CLAUDE.md
Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

Tradeoff: These guidelines bias toward caution over speed. For trivial tasks, use judgment.

1. Think Before Coding
   Don't assume. Don't hide confusion. Surface tradeoffs.

Before implementing:

State your assumptions explicitly. If uncertain, ask.
If multiple interpretations exist, present them - don't pick silently.
If a simpler approach exists, say so. Push back when warranted.
If something is unclear, stop. Name what's confusing. Ask. 2. Simplicity First
Minimum code that solves the problem. Nothing speculative.

No features beyond what was asked.
No abstractions for single-use code.
No "flexibility" or "configurability" that wasn't requested.
No error handling for impossible scenarios.
If you write 200 lines and it could be 50, rewrite it.
Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

3. Surgical Changes
   Touch only what you must. Clean up only your own mess.

When editing existing code:

Don't "improve" adjacent code, comments, or formatting.
Don't refactor things that aren't broken.
Match existing style, even if you'd do it differently.
If you notice unrelated dead code, mention it - don't delete it.
When your changes create orphans:

Remove imports/variables/functions that YOUR changes made unused.
Don't remove pre-existing dead code unless asked.
The test: Every changed line should trace directly to the user's request.
Backlog tracking dùng file local `docs/backlog/BACKLOG.md` (không dùng Jira). Cập nhật trạng thái item (Backlog → Ready → In Progress → Verify → Done) trong file này khi bắt đầu/hoàn thành công việc. Mô tả chi tiết item nằm ở `docs/mvp-product-backlog.md`.

Đầu mỗi session code: đọc `docs/backlog/BACKLOG.md` + `docs/backlog/CODEMAP.md` trước, không re-scan `Assets/` để nắm hiện trạng — hai file này đủ token-rẻ để biết cái gì đã có, ở đâu, test đến đâu. Cuối mỗi khối việc (mỗi sprint hoặc mỗi lần hệ thống mới hoàn thành): cập nhật cả hai file cùng lúc, commit message tham chiếu BL-ID. `CODEMAP.md` = bảng Hệ thống → file path → API chính → trạng thái test → ghi chú "chưa làm/mock".

Mọi implementation plan (kế hoạch code, không phải backlog item) lưu dưới dạng `.md` trong `docs/plans/YYYY-MM-DD-<slug>.md`, không chỉ trong `.claude/plans/`.

Cuối plan / implementation, phải xác định người dùng cần test gì trên hệ thống mới hoàn thành.

Trả lời bằng tiếng việt

Tự xác định khi nào cần commit và tự thực hiện commit.

Khi thiết kế placement hãy đọc: isometric-game-placement-rules.md
