# CLAUDE.md — CustomDataGrid

Guardrails for working on this WPF control. Read `docs/HANDOFF.md` for the full
requirements contract and design decisions. Read `docs/tasks.md` for the task list.

## Non-negotiable constraints

- **Target: .NET Framework 4.7.2. Language: C# 7.3 maximum.**
- **Do NOT use C# 8+:** no default interface methods, no nullable reference types
  (`?` on reference types), no records, no switch expressions, no ranges/indices
  (`a..b`, `[^1]`), no `IAsyncEnumerable`.
- C# 7.x is fine: tuples, local functions, `out` vars, `?.`, `??`, `nameof`,
  expression-bodied members, `is` pattern matching.
- WPF, MVVM. Consumers must not need code-behind.
- Tests: **NUnit + Moq**, run with NUnit.

## Build & verify

- Library + sample: `dotnet build` (or build in Visual Studio).
- Unit tests: `dotnet test` (or the NUnit runner).
- WPF rendering / mouse behavior can't be unit-tested — those are covered by the
  manual MT-01..MT-14 plan in `docs/HANDOFF.md`, run in the sample app.

## Build order (do not skip ahead)

1. Contracts ✅ done — `src/CustomDataGrid/Contracts/`
2. Models — GridGroupRow, GridItemRow, GridRowAction, GridHeaderAction
3. **FlatRowCollection** — the performance core; write the full NUnit suite here
4. InMemoryGridDataSource
5. Control shell — CustomDataGrid, GridHeaderBar
6. Columns — Text, CheckBox, ComboBox, Button, ActionsMenu, built-in SelectionColumn
7. Selectors + Themes/Generic.xaml
8. Sample app (grid left + live log right)

## Core invariants (get these right)

- `FlatRowCollection` is a virtual `IList<IGridRow>` that holds **no item objects** —
  it resolves rows from `IGridDataSource` on demand. Indexer is **O(log G)** via
  binary search over per-group flat offsets.
- Expand/collapse fires **incremental Add/Remove** CollectionChanged, **never Reset**.
- **Disabled row:** no select, no highlight, no edit. Disabled **group**: no
  expand/collapse, and all children inherit disabled. Range-select skips disabled
  rows silently. `SelectRowCommand`/`BeginEditRowCommand` on disabled → no-op.
- **Group selection is tri-state:** Full (tick) / Partial (solid square) / Deselected.
  Click partial or deselected → Full; click full → Deselected.
- **Click:** editable cell → edit, no select; non-editable/empty → select+highlight;
  checkbox/button/actions → act, no select; selection column → select+highlight.
  Click-away during edit → commit. Highlight preserved during edit if row was selected.
  Multi-select preserved while editing another row.
- **SingleExpandMode=true:** one group open at a time (collapse previous first);
  `ExpandAllCommand` disabled; call `IGridCollapseHint.OnGroupCollapsed` after collapse.
- No `RowAdded`/`RowDeleted` events — consumers use `IGridDataSource.GroupChanged` /
  `ItemChanged`.
- CellEditCommitted value types: Text→string, ComboBox→string(display), CheckBox→bool.

## Style

- One public type per file, file name matches the type.
- XML doc comments on all public members (the Phase 1 files set the standard).
- `GridGroupRow.Items` is `IList<GridItemRow>`, deliberately not ObservableCollection.
