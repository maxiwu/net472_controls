# CustomDataGrid

A WPF `DataGrid` subclass with two-level grouping, rich columns, MVVM bindings,
routed-event inter-control communication, and a data-virtualization seam for large
data sets (~2M rows). Targets **.NET Framework 4.7.2 / C# 7.3**.

## Start here
- `CLAUDE.md` — guardrails (read first in Claude Code)
- `docs/HANDOFF.md` — full requirements contract, decisions, verification & manual test plan
- `docs/spec.md` / `docs/design.md` / `docs/tasks.md` — the OpenSpec docs (Markdown)
- `docs/*.docx` — same docs in Word format

## Status
- Phase 1 (Contracts): **complete** — `src/CustomDataGrid/Contracts/`
- Next: Phase 2 (Models) → see `docs/tasks.md`

## Build order
Contracts → Models → FlatRowCollection (+NUnit) → InMemoryGridDataSource →
Control shell → Columns → Selectors/Themes → Sample app.
