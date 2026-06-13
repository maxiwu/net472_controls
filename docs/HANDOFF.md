# CustomDataGrid — Session Handoff

This file compacts the full design conversation into a single reference so work can
continue in Claude Code / VSCode. It captures every requirement, every decision, and
the exact state of the codebase at handoff.

---

## 1. What we are building

A WPF custom control: a `DataGrid` subclass with two-level hierarchical data
(groups containing items), rich column types, MVVM bindings, inter-control
communication via routed events, and an interface seam for optional data
virtualization up to ~2,000,000 rows.

---

## 2. Hard constraints

| Constraint | Value |
|---|---|
| Target framework | .NET Framework 4.7.2 |
| Language | C# 7.3 maximum (no C# 8+ features) |
| UI framework | WPF |
| Base control | `System.Windows.Controls.DataGrid` (subclassed) |
| Architecture | MVVM — consumers write no code-behind |
| Tests | NUnit 3.13.3 + Moq 4.18.4, run with NUnit |

**Forbidden (C# 8+):** default interface methods, nullable reference types (`?`),
records, switch expressions, ranges/indices (`[^1]`, `a..b`), `IAsyncEnumerable`.
**Allowed (C# 7.x):** tuples, local functions, `out` vars, `?.`, `??`, `nameof`,
expression-bodied members, pattern matching (`is`).

---

## 3. Data model

Two levels: **group rows** contain **item rows**. Groups expand/collapse.

Row state (on `IGridRow`): `Kind` (Group/Item), `IsEnabled`, `IsHighlighted`.
Item rows add `IsSelected`. Group rows add `IsExpanded`, `Label`, `TotalItemCount`,
`SelectionState` (tri-state), and `Items`.

---

## 4. Locked decisions (the full requirements contract)

### Column types
Text (read/edit), CheckBox (native), ComboBox (combo shown only when editing;
displays string otherwise), Button (ICommand), ActionsMenu (lazy popup), plus an
optional leftmost **selection checkbox column** (`ShowSelectionColumn`).

### SingleExpandMode (bool DP, default false)
- true → only one group expanded at a time; expanding a new group collapses the
  previous one first; `ExpandAllCommand` is **disabled** (decision: option A).
- Used as a performance lever: bounds in-memory items to one group's worth.
- Toggling to true at runtime collapses all but the first expanded group.
- After collapsing, calls `IGridCollapseHint.OnGroupCollapsed(index)` if the data
  source implements it.

### Disabled row rules
- Cannot be selected, highlighted, or edited.
- Disabled **group** cannot expand/collapse; **all its children inherit disabled**.
- Range-select **skips disabled rows silently**.
- A row already selected that later becomes disabled is **not auto-deselected**.
- `SelectRowCommand` / `BeginEditRowCommand` on a disabled row → **no-op**.
- `CellEditCommitted` / `CellEditCancelled` are **unreachable** for disabled rows
  (documented, no runtime guard needed).

### Selection
- Group row **can** be selected. Selecting a group selects the group + all enabled
  children.
- Group tri-state: **FullySelected** (tick), **PartiallySelected** (solid square),
  **Deselected** (empty).
- Deselect one child → group becomes PartiallySelected. Deselect all → Deselected.
- Click on a group: Deselected/Partial → **FullySelected**; Full → **Deselected**
  (decision: clicking partial selects all — option A).
- Consumer may bind one bool to both IsSelected and IsHighlighted (selected ⇒ highlighted).

### Click behavior (edit vs select)
- Editable cell click → enter **edit mode**, does NOT select/highlight the row.
- Non-editable cell / empty row space → **select + highlight**.
- CheckBox data column → toggle value, no select/highlight.
- Button column → run command, no select/highlight.
- Actions menu column → open menu, no select/highlight.
- Selection column checkbox → select/deselect + highlight.
- Click away during edit → **commit**.
- Highlight is **preserved** during edit if the row was selected before editing.
- Multi-select is **preserved** when editing a different row's cell.

### Highlight
Fixed color defined by the control style. `IsHighlighted` is a `bool`.

### Actions menu
- `GroupRowActions` and `ItemRowActions` are separate `IList<IGridRowAction>`,
  one menu shared by all rows of that kind.
- Per-row enable/disable via `ICommand.CanExecute(IGridRow)`.
- Optional custom UI via `ActionsMenuColumn.ActionsMenuTemplate` (DataContext = IGridRow).
- `IGridRowAction`: Label, Icon (nullable), Command (param = IGridRow), IsSeparator.
- Header actions are a **different** interface: `IGridHeaderAction`
  (Label, Icon, Command, IsEnabled).

### Inter-control communication
- **Outbound routed events** (Bubble): `SelectedRowChanged`, `SelectedRowsChanged`,
  `GroupExpanded`, `GroupCollapsed`, `CellEditCommitted`, `CellEditCancelled`.
- **No** `RowAdded` / `RowDeleted` routed events — consumers observe
  `IGridDataSource.GroupChanged` / `ItemChanged` instead (decision: after-the-fact,
  data source events are sufficient).
- **Inbound commands** (ICommand DPs): `ScrollToRowCommand`, `SelectRowCommand`,
  `ExpandGroupCommand`, `CollapseGroupCommand`, `CollapseAllCommand`,
  `ExpandAllCommand`, `BeginEditRowCommand`.

### CellEditCommitted value types
Text → string; ComboBox → string (display); CheckBox → bool; Button/ActionsMenu → N/A.

### Data virtualization
- The control owns `FlatRowCollection` + UI; the **consumer** owns the data source.
- `IGridDataSource`: counts known upfront, rows fetched on demand, change events.
- `IGridCollapseHint`: optional opt-in (separate interface because no default
  interface methods in C# 7.3).
- `InMemoryGridDataSource`: free default implementation.

---

## 5. Performance design

- `FlatRowCollection` is a **virtual `IList<IGridRow>` + INotifyCollectionChanged**.
  It holds **no item objects** — resolves them from `IGridDataSource` on demand.
- Indexer is **O(log G)** via binary search over per-group flat offsets.
- Expand/collapse uses **incremental Add/Remove CollectionChanged**, never Reset, so
  `VirtualizingStackPanel` keeps recycling containers.
- ComboBox ItemsSource set at **column level**, not per cell.
- ActionsMenu popup created **lazily** on first click.
- CheckBox uses native `DataGridCheckBoxColumn`.
- Memory ceiling with SingleExpandMode = groups + one group's items.

---

## 6. Project structure

```
CustomDataGrid/
  Contracts/            <-- PHASE 1 COMPLETE
    IGridRow.cs
    IGridDataSource.cs
    IGridCollapseHint.cs
    IGridRowAction.cs
    IGridHeaderAction.cs
    RowKind.cs
    SelectionState.cs
    ChangeKinds.cs            (GroupChangeKind, ItemChangeKind)
    Events/
      GroupChangedEventArgs.cs      (EventArgs)
      ItemChangedEventArgs.cs       (EventArgs)
      GroupExpandedEventArgs.cs     (RoutedEventArgs)
      GroupCollapsedEventArgs.cs    (RoutedEventArgs)
      SelectedRowChangedEventArgs.cs
      SelectedRowsChangedEventArgs.cs
      CellEditCommittedEventArgs.cs
      CellEditCancelledEventArgs.cs
  Models/               <-- PHASE 2 (next)
    GridGroupRow.cs
    GridItemRow.cs
    GridRowAction.cs
    GridHeaderAction.cs
  Collection/
    FlatRowCollection.cs        <-- PHASE 3 (the performance core; most tests)
  DataSources/
    InMemoryGridDataSource.cs   <-- PHASE 4
  Controls/
    CustomDataGrid.xaml(.cs)    <-- PHASE 5
    GridHeaderBar.xaml(.cs)
  Columns/             <-- PHASE 6
    CustomDataGridColumn.cs
    TextColumn.cs
    CheckBoxColumn.cs
    ComboBoxColumn.cs
    ButtonColumn.cs
    ActionsMenuColumn.cs
  Selectors/           <-- PHASE 7
    GridRowStyleSelector.cs
    GridCellStyleSelector.cs
  Themes/
    Generic.xaml
```

---

## 7. Build order

1. Contracts ✅ done
2. Models (`GridGroupRow`, `GridItemRow`, `GridRowAction`, `GridHeaderAction`)
3. `FlatRowCollection` (+ full NUnit suite — see Task 3.6 in Tasks doc)
4. `InMemoryGridDataSource`
5. Control shell (`CustomDataGrid`, `GridHeaderBar`): DPs, routed events, commands,
   selection logic, edit-commit-on-click-away
6. Columns (Text, CheckBox, ComboBox, Button, ActionsMenu, + built-in SelectionColumn)
7. Selectors + `Generic.xaml`
8. Sample app (see section 9)

---

## 8. Verification plan

**Tier 1 — NUnit (runs anywhere, no WPF):** FlatRowCollection (all index math,
expand/collapse, SingleExpandMode, disabled-group guard, data-source event
handling), InMemoryGridDataSource, models (INPC + state transitions), group
selection state machine. Use Moq to fake `IGridDataSource`.

**Tier 2 — compile check:** WPF control + columns + XAML build with `dotnet build`
(or VS) on Windows.

**Tier 3 — manual (sample app on Windows):** the MT-01..MT-14 plan below.

### FlatRowCollection unit tests (minimum)
Count (no expand / with expand), indexer returns correct group row, indexer returns
correct item row, expand fires correct Add, collapse fires correct Remove,
SingleExpandMode collapses previous on expand, disabled group cannot expand,
EnforceSingleExpandMode collapses all but first, DataReset rebuilds, GroupChanged/
ItemChanged update flat list correctly.

### Tier 3 manual test plan (run in sample app)
- **MT-01 Rendering:** grid + header + columns render; resize behaves.
- **MT-02 Expand/Collapse:** independent expand; SingleExpandMode collapses previous;
  toggling mode off allows multiple.
- **MT-03 Disabled group:** grayed; chevron does nothing; no select/highlight.
- **MT-04 Disabled item:** grayed; no select; editable cell won't edit; range-select skips.
- **MT-05 Click select vs edit:** editable→edit no select; non-editable→select+highlight;
  click-away commits; combo/checkbox/button/actions don't select; empty space selects.
- **MT-06 Selection column:** item check toggles select+highlight; group check selects
  all enabled children; deselect one child → solid square; click partial → all selected;
  click full → cleared; disabled checkbox grayed.
- **MT-07 Highlight:** select highlights; stays during edit; stays after commit if still
  selected; independent IsHighlighted works.
- **MT-08 Multi-select:** Ctrl+click multiple; editing unselected row keeps prior
  selection; Ctrl+click disabled skipped.
- **MT-09 Actions menu:** item menu vs group menu; separator renders; CanExecute disables
  items; click fires command with correct IGridRow; click-away closes.
- **MT-10 Header actions:** Add Group adds; Delete Selected removes; disabled grays.
- **MT-11 Cell edit types:** Text commit→string; Escape→cancel restores; combo shows text
  in read, combo in edit, commit→display string; checkbox→bool; button no edit.
- **MT-12 Routed events:** each of the 6 fires with correct args (observe in log panel).
- **MT-13 Commands:** ScrollToRow scrolls; SelectRow (enabled vs disabled no-op);
  ExpandGroup (enabled vs disabled no-op); CollapseAll; ExpandAll (off vs disabled when
  SingleExpandMode); BeginEdit (enabled vs disabled no-op).
- **MT-14 Performance:** load 2000×1000; rapid scroll smooth; expand 1000 items <500ms;
  SingleExpandMode repeated expand releases memory; collapse-all returns to baseline.

---

## 9. Sample application spec

**Layout:** grid left (~65%), live event log right (~35%). Header bar with Add Group /
Delete Selected, plus SingleExpandMode and ShowSelectionColumn toggles. ShowSelectionColumn = true.

**Sample row types:** `SampleGroupRow : GridGroupRow` adds `string Description`
(editable text), `string Status` (Enable/Disable). `SampleItemRow : GridItemRow`
adds `float X`, `float Y`.

**Columns:** group → Description (Text, editable), Status (ComboBox: Enable/Disable),
Actions (Copy, Modify). Item → X (float text), Y (float text), Actions (Copy, Modify, Delete).

**Sample data:** 10 groups ("Group 001".."Group 010"), 8–12 items each, X/Y random
floats 2dp. Group 003 disabled. Group 002 item index 4 disabled. Group 001 item
index 2 highlighted.

**Log panel:** `LogCategory` {Selection, Edit, Action}; `LogEntry`
{Timestamp, Category, Message}; `LogViewModel` with ObservableCollection, AddEntry,
Clear, EntryCount. Newest-first / auto-scroll. Category colors: SELECTION=blue,
EDIT=orange, ACTION=red. "Clear Log" button + entry count label.

**Logged events:** row selected / selection cleared / N rows selected; group
expanded/collapsed; description changed old→new; status changed old→new; X/Y changed
old→new; edit cancelled; copy/modify/delete (group + item); add group; delete selected
(g groups, i items).

---

## 10. State at handoff

- **Phase 1 (Contracts): COMPLETE.** 16 files written. Verified by compiling under
  Mono `langversion:7.2` (stricter than 7.3 → safe) against faithful WPF stubs +
  temporary model stubs, then reflecting the public API to confirm it matches spec.
  WPF-dependent files (ImageSource / RoutedEventArgs) compiled against stubs only;
  first real WPF compile happens on the Windows machine.
- **Next: Phase 2 — Models.** First phase with real NUnit-testable logic
  (INPC, group tri-state transitions, disabled inheritance).

### Sandbox caveat (no longer relevant on Windows)
This was authored in a Linux sandbox with no .NET SDK and no WPF assemblies, so Mono
was used for syntax/structure checks only. On Windows with the real SDK, do a full
`dotnet build` (or VS build) of the library, then `dotnet test` / NUnit for Tier 1.

---

## 11. Open items / watch list

- `IGridDataSource` and the routed event args reference `GridGroupRow` /
  `GridItemRow` (Phase 2). They will not fully compile until Phase 2 lands.
- `GridGroupRow.Items` is `IList<GridItemRow>` (NOT ObservableCollection) by design,
  for scale. Change notifications come through `IGridDataSource` events.
- When wiring `FlatRowCollection` offsets: after any expand/collapse, update
  `FlatOffset` for all groups after the changed one.
- `ExpandAllCommand.CanExecute` must return false when `SingleExpandMode == true`.
- Selection-column group checkbox is tri-state: `IsChecked = null` ⇒ PartiallySelected.
