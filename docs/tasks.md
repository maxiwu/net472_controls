**CustomDataGrid**

Implementation Task List

*Version 1.0 \| 8 Phases \| .NET Framework 4.7.2*

**Phase Overview**

|        |                    |                                           |
|--------|--------------------|-------------------------------------------|
| **\#** | **Phase**          | **Deliverable**                           |
| **1**  | Contracts          | All interfaces, enums, EventArgs          |
| **2**  | Models             | GridGroupRow, GridItemRow, action classes |
| **3**  | Collection         | FlatRowCollection — virtual IList engine  |
| **4**  | Default DataSource | InMemoryGridDataSource                    |
| **5**  | Control Shell      | GridControl + GridHeaderBar               |
| **6**  | Columns            | All 6 column types                        |
| **7**  | Styles & Selectors | Row/cell selectors, Generic.xaml themes   |
| **8**  | Sample App         | Working MVVM demo                         |

**Phase 1 — Contracts**

> *All interfaces. No implementation. Everything else depends on this
> phase.*

**Task 1.1 — SelectionState.cs**

- Define enum: Deselected, PartiallySelected, FullySelected

- Add XML doc comments explaining each state

**Task 1.2 — RowKind.cs**

- Define enum: Group, Item

**Task 1.3 — IGridRow.cs**

- Properties: RowKind Kind, bool IsEnabled, bool IsHighlighted

- XML doc: document that disabled rows cannot be selected, highlighted,
  or edited

- XML doc: document that CellEdit events are unreachable for disabled
  rows

**Task 1.4 — IGridDataSource.cs**

- int GroupCount { get; }

- int GetItemCount(int groupIndex)

- GridGroupRow GetGroup(int groupIndex)

- GridItemRow GetItem(int groupIndex, int itemIndex)

- IList\<GridItemRow\> GetItems(int groupIndex, int startIndex, int
  count)

- event EventHandler\<GroupChangedEventArgs\> GroupChanged

- event EventHandler\<ItemChangedEventArgs\> ItemChanged

- event EventHandler DataReset

**Task 1.5 — IGridCollapseHint.cs**

- void OnGroupCollapsed(int groupIndex)

- XML doc: optional interface, implement to release paged data on group
  collapse

**Task 1.6 — IGridRowAction.cs**

- string Label { get; }

- ImageSource Icon { get; } // nullable — use XML doc

- ICommand Command { get; } // CommandParameter = IGridRow

- bool IsSeparator { get; } // visual divider, no command

**Task 1.7 — IGridHeaderAction.cs**

- string Label { get; }

- ImageSource Icon { get; } // nullable

- ICommand Command { get; }

- bool IsEnabled { get; }

**Task 1.8 — Events/GroupExpandedEventArgs.cs**

- Extend RoutedEventArgs

- GridGroupRow Group { get; }

- int GroupIndex { get; }

- Constructor: (RoutedEvent, GridGroupRow, int)

**Task 1.9 — Events/GroupCollapsedEventArgs.cs**

- Same structure as GroupExpandedEventArgs

**Task 1.10 — Events/SelectedRowChangedEventArgs.cs**

- Extend RoutedEventArgs

- IGridRow OldRow { get; } // nullable

- IGridRow NewRow { get; } // nullable

- Constructor: (RoutedEvent, IGridRow, IGridRow)

**Task 1.11 — Events/SelectedRowsChangedEventArgs.cs**

- Extend RoutedEventArgs

- IList\<IGridRow\> AddedRows { get; }

- IList\<IGridRow\> RemovedRows { get; }

- Constructor: null-safe (default to empty List\<IGridRow\>)

**Task 1.12 — Events/CellEditCommittedEventArgs.cs**

- Extend RoutedEventArgs

- IGridRow Row { get; }

- string ColumnName { get; }

- object OldValue { get; }

- object NewValue { get; }

- XML doc: value types per column — string/string/bool/N/A/N/A

**Task 1.13 — Events/CellEditCancelledEventArgs.cs**

- Extend RoutedEventArgs

- IGridRow Row { get; }

- string ColumnName { get; }

**Task 1.14 — Events/GroupChangedEventArgs.cs**

- int GroupIndex { get; }

- GroupChangeKind Kind: Added, Removed, Updated

**Task 1.15 — Events/ItemChangedEventArgs.cs**

- int GroupIndex { get; }

- int ItemIndex { get; }

- ItemChangeKind Kind: Added, Removed, Updated

**Phase 2 — Models**

> *Concrete implementations of IGridRow. Must implement
> INotifyPropertyChanged.*

**Task 2.1 — GridGroupRow.cs**

- Implements IGridRow, INotifyPropertyChanged

- Properties: string Label, int TotalItemCount, bool IsExpanded

- SelectionState SelectionState { get; set; } — fires PropertyChanged

- bool IsEnabled, bool IsHighlighted — fire PropertyChanged

- IList\<GridItemRow\> Items — not ObservableCollection (perf)

- XML doc: IsEnabled=false blocks expand/collapse and all child rows are
  treated as disabled

**Task 2.2 — GridItemRow.cs**

- Implements IGridRow, INotifyPropertyChanged

- bool IsSelected { get; set; } — fires PropertyChanged

- bool IsEnabled { get; set; } — fires PropertyChanged

- bool IsHighlighted { get; set; } — fires PropertyChanged

- XML doc: disabled row rules (no select, no highlight, no edit,
  CellEdit unreachable)

**Task 2.3 — GridRowAction.cs**

- Implements IGridRowAction

- Constructor: (string label, ICommand command, ImageSource icon = null)

- Static factory: GridRowAction.Separator — sets IsSeparator=true, null
  command

**Task 2.4 — GridHeaderAction.cs**

- Implements IGridHeaderAction

- Constructor: (string label, ICommand command, ImageSource icon = null)

- IsEnabled backed by Command.CanExecute if not set explicitly

**Phase 3 — Collection**

> *The performance core. Must be correct before any UI work begins.*

**Task 3.1 — FlatRowCollection.cs — Skeleton**

- Implements IList\<IGridRow\>, INotifyCollectionChanged

- Constructor: (IGridDataSource source)

- Internal List\<GroupState\> struct tracking

- Rebuild() method called on construction and DataReset

**Task 3.2 — FlatRowCollection — Index Resolution**

- Implement Count property: groups + sum of expanded item counts

- Implement IGridRow this\[int index\] indexer

- Binary search over GroupState.FlatOffset — O(log G)

- Return GridGroupRow if index == group offset, else call
  \_source.GetItem()

**Task 3.3 — FlatRowCollection — Expand / Collapse**

- ExpandGroup(int groupIndex): incremental insert, fires Add
  CollectionChanged

- CollapseGroup(int groupIndex): incremental remove, fires Remove
  CollectionChanged

- SetExpanded(int groupIndex, bool expanded): public entry point

- Check group IsEnabled before allowing expand — no-op if disabled

- Update GroupState.FlatOffset for all groups after the changed one

**Task 3.4 — FlatRowCollection — SingleExpandMode**

- bool SingleExpandMode property

- int \_currentExpandedGroupIndex field, initialized to -1

- On SetExpanded(true): collapse previous if different index

- Call IGridCollapseHint.OnGroupCollapsed() via "as" cast after collapse

- EnforceSingleExpandMode(): collapse all but first expanded — called on
  mode toggle

**Task 3.5 — FlatRowCollection — DataSource Event Handlers**

- Subscribe to \_source.GroupChanged — targeted insert/remove in flat
  list

- Subscribe to \_source.ItemChanged — update item if group is expanded

- Subscribe to \_source.DataReset — call Rebuild(), fire Reset
  CollectionChanged

- Unsubscribe old source events when DataSource DP changes

**Task 3.6 — FlatRowCollection — Unit Tests**

- Test: Count with no groups expanded

- Test: Count with groups expanded

- Test: Indexer returns correct group row

- Test: Indexer returns correct item row

- Test: Expand fires correct CollectionChanged

- Test: Collapse fires correct CollectionChanged

- Test: SingleExpandMode collapses previous on expand

- Test: Disabled group cannot be expanded

- Test: EnforceSingleExpandMode collapses all but first

**Phase 4 — Default DataSource**

**Task 4.1 — InMemoryGridDataSource.cs**

- Implements IGridDataSource

- Constructor: (IList\<GridGroupRow\> groups)

- All IGridDataSource methods delegate to the list

- GetItems: returns list subrange via GetRange(startIndex, count)

- Wire CollectionChanged from each group's Items to fire
  GroupChanged/ItemChanged

- DataReset fired if the top-level groups list is replaced

**Phase 5 — Control Shell**

> *GridControl is a custom control extending ItemsControl directly — NOT
> DataGrid, NOT Selector / MultiSelector. See design doc §5.1 for the
> rationale. The selection model, click resolution, column-width sharing,
> header row, and virtualization configuration are all owned by this control.*

**Task 5.1 — GridControl.cs — Base class + default ControlTemplate**

- Class `GridControl : ItemsControl` (no DataGrid, no Selector).

- Static ctor: override `DefaultStyleKeyProperty` so Generic.xaml's
  default template is picked up.

- Default ControlTemplate (in Generic.xaml, wired up here): Grid with
  three rows — header bar (`GridHeaderBar`), column headers
  (`GridColumnHeadersPresenter`), body `ScrollViewer` containing the
  `ItemsPresenter`. See design doc §5.2.

- Override `ItemsPanel` to a `VirtualizingStackPanel` with
  `IsVirtualizing=True`, `VirtualizationMode=Recycling`,
  `ScrollUnit=Item`.

- Override `GetContainerForItemOverride` / `IsItemItsOwnContainerOverride`
  to return a `GridRowPresenter` (Task 5.8).

**Task 5.2 — GridControl.cs — Dependency Properties**

- Register all DPs listed in design doc §5.3, including the new
  `Columns` DP (`GridColumnCollection`).

- `DataSource` DP change handler: detach the old `FlatRowCollection`
  (unsubscribe from its `CollectionChanged`, unsubscribe the
  FlatRowCollection from the old data source), construct a new
  `FlatRowCollection(newSource)`, propagate `SingleExpandMode`, and
  set the control's `ItemsSource` to the new FlatRowCollection.

- `SingleExpandMode` DP change handler: propagate to the
  FlatRowCollection; if toggled true, call `EnforceSingleExpandMode()`.

- `ShowSelectionColumn` DP change handler: insert / remove the built-in
  selection column from the `Columns` collection (Task 6.7).

- `Columns` DP: initialize to an empty `GridColumnCollection` so XAML
  consumers can populate via `<GridControl.Columns>` without a null
  check.

**Task 5.3 — GridControl.cs — Routed Events**

- Register all six routed events with
  `EventManager.RegisterRoutedEvent` using `RoutingStrategy.Bubble`.

- Add CLR event wrappers (add/remove with `AddHandler` / `RemoveHandler`).

- Provide protected `RaiseSelectedRowChanged`,
  `RaiseGroupExpanded`, etc. helpers used by §5.6 selection logic and
  by `FlatRowCollection` callbacks.

**Task 5.4 — GridControl.cs — Inbound Commands**

- Register all inbound `ICommand` DPs from design doc §5.7.

- `ScrollToRowCommand`: resolve the row's flat index from the
  FlatRowCollection and scroll the body `ScrollViewer` to that index
  (item-mode scrolling — no DataGrid `ScrollIntoView`).

- `SelectRowCommand`: check `IsEnabled`; no-op on disabled rows;
  otherwise update `SelectedRow` / `SelectedRows`.

- `ExpandGroupCommand`: call `FlatRowCollection.SetExpanded(index, true)`;
  no-op if the group's `IsEnabled` is false.

- `CollapseGroupCommand`: same, with `false`.

- `CollapseAllCommand`: iterate groups, collapse all currently expanded.

- `ExpandAllCommand`: `CanExecute` returns `false` when
  `SingleExpandMode=true`.

- `BeginEditRowCommand`: check `IsEnabled`; no-op on disabled rows;
  otherwise enter edit mode on the first editable cell of that row.

**Task 5.5 — GridControl.cs — Selection / click logic**

- Hook `PreviewMouseLeftButtonDown` on the control (or on the row
  container — Task 5.8). Walk the visual tree from the click target to
  classify what was clicked: editable cell, non-editable cell, checkbox
  cell, button cell, actions cell, selection-column cell, group chevron,
  empty row space.

- Editable cell click: enter edit mode on that cell. Do **not**
  change selection. Preserve existing `SelectedRows`.

- Non-editable cell / empty row space: clear single-select (unless
  Ctrl / Shift held), then add the row to selection and highlight it.

- CheckBox / Button / ActionsMenu cell: dispatch to the cell's own
  handler; no selection change.

- Selection-column cell: toggle the row's `IsSelected` (standard
  multi-select rules with Ctrl / Shift); set highlight.

- Disabled rows: ignored entirely; range-select skips them silently.

- Group row click: tri-state machine — Deselected / Partial →
  FullySelected; Full → Deselected. Cascade: on FullySelected, set
  `IsSelected=true` on all enabled children; on Deselected, clear them.

- Child selection change: recompute parent `GridGroupRow.SelectionState`
  from enabled-children state (Deselected / Partial / Full).

- Multi-select is preserved when editing a different row.

**Task 5.6 — GridControl.cs — Edit commit on click-away**

- Track the current edit (row + column) in a private field.

- Hook `PreviewMouseDown` on the control; if the click lands outside
  the editing cell, commit the edit before the click is processed by
  Task 5.5's selection logic.

- Commit fires `CellEditCommitted` (Phase 1 event) with old / new
  values. Cancel (Escape) fires `CellEditCancelled`.

- Highlight state is preserved across commit if the row was selected
  before editing.

**Task 5.7 — Columns/GridColumn.cs + GridColumnCollection.cs (shared widths)**

- `GridColumn`: abstract `DependencyObject` (NOT `DataGridColumn`). DPs:
  `Width` (GridLength — fixed pixels or star), `MinWidth`, `MaxWidth`,
  `Header` (object), `IsEditable` (bool, default true), `ColumnName`
  (string — surfaced in `CellEditCommittedEventArgs`).

- `GridColumnCollection : ObservableCollection<GridColumn>` — the
  central shared list. Raises `CollectionChanged` when columns are
  added/removed and forwards each column's `Width` change up so
  consumers can observe a single "any width changed" signal.

- Widths broadcast **down** to header and rows via bindings + change
  notifications. Rows never measure up to publish a width. No
  `Grid.IsSharedSizeScope`, no `SharedSizeGroup` — see design doc §5.4.

- v1: fixed and star widths only. Star widths are resolved against the
  body viewport width during arrange. Column resize-by-drag is a
  future task, not v1.

**Task 5.8 — Controls/GridRowPresenter.cs + GridCellsPanel.cs**

- `GridRowPresenter : ContentControl` is the ItemContainer returned
  from `GridControl.GetContainerForItemOverride`. Its DataContext is
  an `IGridRow`. Its template hosts a `GridCellsPanel` plus a 1-row
  background for highlight / disabled / selection visual states.

- `GridCellsPanel : Panel` is a custom Panel modeled on
  `DataGridCellsPanel`. `MeasureOverride` does NOT measure children
  against content — it measures each cell against the corresponding
  `GridColumn.Width` from the shared `Columns` collection, then
  arranges horizontally by accumulated offsets.

- For star widths: `GridCellsPanel.MeasureOverride` takes the
  available width, sums fixed widths, distributes the remainder by
  star ratios. Cells receive their column's resolved width.

- Cells themselves are materialized by the cell template defined on
  each `GridColumn` (Phase 6).

- The panel subscribes to `Columns` change notifications and calls
  `InvalidateMeasure` so width changes propagate to every realized
  row without re-realizing them.

**Task 5.9 — Controls/GridColumnHeadersPresenter.cs (header outside scroll)**

- `Control` (or simple `Panel` subclass) bound to the same
  `Columns` collection as the body.

- Lives in row 1 of the GridControl template — **outside** the body
  `ScrollViewer` — so it does not scroll vertically.

- Internal `Canvas` / `TranslateTransform` whose `X` is bound to the
  body `ScrollViewer.HorizontalOffset` (negative offset) so headers
  track horizontal scroll.

- Renders each column's `Header` content with the column's resolved
  width — same width math as `GridCellsPanel`, so headers and rows
  align without any shared-size scope.

**Task 5.10 — Virtualization configuration checklist (verify in template)**

> *Each of these silently disables virtualization if wrong. They MUST be
> set on the default Generic.xaml template and confirmed at runtime via
> a Phase 5 sanity test.*

- [ ] `VirtualizingPanel.IsVirtualizing = True` on the GridControl.

- [ ] `VirtualizingPanel.VirtualizationMode = Recycling`. Recycling is
  mandatory at 2M rows; Standard causes GC churn on scroll.

- [ ] Host `ScrollViewer.CanContentScroll = True` (False disables
  virtualization completely).

- [ ] `VirtualizingPanel.ScrollUnit = Item` (lighter than Pixel and
  matches the row-indexed FlatRowCollection).

- [ ] `ItemsSource` is the `IList<IGridRow>`-implementing
  `FlatRowCollection` so VSP realizes only the visible index window.

- Add an automated sanity check (NUnit, run against a constructed
  GridControl in a hidden window or via `Loaded` callback): assert
  `VirtualizingPanel.GetIsVirtualizing(grid) == true`,
  `VirtualizingPanel.GetVirtualizationMode(grid) == Recycling`,
  `ScrollViewer.GetCanContentScroll(grid) == true`,
  `VirtualizingPanel.GetScrollUnit(grid) == Item`.

**Task 5.11 — Data-virtualization trap (CollectionView) — verification**

> *Setting `ItemsSource` wraps in a default `CollectionView`. Sort /
> filter / group on that view, or some Refresh paths, can enumerate the
> entire source and force IGridDataSource to materialize 2M rows. See
> design doc §5.9.*

- Document in `GridControl`'s XML doc that the default view must not
  have `SortDescriptions`, `Filter`, or `GroupDescriptions` applied,
  and that `Refresh()` should be avoided.

- Avoid binding any control or converter to `ItemsSource` in a way that
  enumerates it (`Count` accessors that walk the sequence, "any item
  matches", etc.).

- **Verification test (NUnit + Moq):** instrument a fake
  `IGridDataSource` with counters on `GetGroup` / `GetItem` /
  `GetItems`. Load a 1,000,000-row source. Scroll the GridControl
  through a small window. Assert that the total fetch count tracks the
  realized container count (tens), not the row count (millions).
  Failure here means data virtualization is silently broken — a hard
  regression.

**Task 5.12 — GridHeaderBar.xaml / .cs**

- UserControl with an `ItemsControl` bound to `HeaderActions`.

- Buttons right-aligned in a horizontal `StackPanel`.

- Each button: `Content=Label`, `Command=Command`, `IsEnabled=IsEnabled`,
  optional `Icon`.

- Independent of GridControl's body — lives in row 0 of the GridControl
  template.

**Phase 6 — Columns**

> *Cells are DataTemplate-driven, laid out by the GridCellsPanel built
> in Task 5.8 against the shared column widths from Task 5.7. Columns
> are plain DependencyObjects (the GridColumn base from Task 5.7), NOT
> DataGridColumn subclasses. None of the six column types extends a
> WPF DataGrid type. The six column types and their interaction
> behavior (read vs edit, click rules) carry over from v1 unchanged.*

**Task 6.1 — Columns/GridColumn.cs (base)**

> *Defined in Task 5.7. Recap here for the column-types phase.*

- Abstract `DependencyObject`. DPs: `Header`, `Width` (GridLength),
  `MinWidth`, `MaxWidth`, `IsEditable` (bool, default true),
  `ColumnName` (string — surfaced in CellEditCommitted EventArgs),
  `CellTemplate` (DataTemplate, read-mode), `CellEditingTemplate`
  (DataTemplate, edit-mode; optional).

- The GridCellsPanel uses `CellTemplate` to materialize each cell, then
  swaps to `CellEditingTemplate` when that row+column enters edit mode
  via `BeginEditRowCommand` or click-to-edit.

**Task 6.2 — Columns/TextColumn.cs**

- Extends `GridColumn`.

- Default `CellTemplate`: `TextBlock` bound to the cell value via the
  column's `Binding` DP.

- Default `CellEditingTemplate`: `TextBox` two-way bound to the same.

- On commit (focus loss / Enter / click-away): the cell raises
  `CellEditCommitted` on the parent GridControl with `string` old / new
  values. Escape raises `CellEditCancelled`.

**Task 6.3 — Columns/CheckBoxColumn.cs**

- Extends `GridColumn`.

- Single template: `CheckBox` two-way bound to the cell value (no
  separate edit template; checkbox toggles in place).

- On toggle, raises `CellEditCommitted` with `bool` old / new values.

- Does not trigger row selection or highlight. Click on the checkbox
  cell is intercepted by Task 5.5's click classification.

**Task 6.4 — Columns/ComboBoxColumn.cs**

- Extends `GridColumn`.

- `CellTemplate`: `TextBlock` showing the display string (read mode).

- `CellEditingTemplate`: `ComboBox` (edit mode only — the combo is NOT
  materialized for non-editing rows).

- DP: `ItemsSource` (`IEnumerable`) — set at the **column** level,
  shared across all rows. Per-cell `ItemsSource` would allocate per
  row and is prohibited.

- On selection commit, raises `CellEditCommitted` with the **display
  string** old / new values.

**Task 6.5 — Columns/ButtonColumn.cs**

- Extends `GridColumn`.

- `CellTemplate`: `Button`.

- DP: `Command` (`ICommand`) — `CommandParameter` is the `IGridRow`.
  Defined **once at the column level** and reused for every row.

- DP: `ButtonContent` (object) — label or icon.

- Does not trigger row selection or highlight.

**Task 6.6 — Columns/ActionsMenuColumn.cs**

- Extends `GridColumn`.

- `CellTemplate`: an icon `Button` (ellipsis / hamburger) — the
  trigger. The popup is **not** in the cell template; it is
  instantiated lazily.

- On click, build the popup on first use, then reuse the same popup
  instance for that container (and reset its DataContext on each open
  to the current row).

- Popup `ItemsControl` is bound to `GroupRowActions` or
  `ItemRowActions` from the parent GridControl, selected by the row's
  `Kind`.

- Each menu item: `Label`, `Icon`, `Command` (`CommandParameter =
  IGridRow`), separator support.

- DP: `ActionsMenuTemplate` (`DataTemplate`) — if set, replaces the
  default popup.

- Does not trigger row selection or highlight.

> **Performance rules — read these before implementing:**
>
> - **Shared command instances.** `GroupRowActions` /
>   `ItemRowActions` are populated with shared `IGridRowAction`
>   instances defined once at the GridControl level. The sample app
>   has three item commands (Copy, Modify, Delete). At 2M rows that
>   is still **three** command objects — never construct per-row
>   commands. 2M × 3 ≈ 6M objects would consume hundreds of MB and
>   trigger constant GC.
>
> - **Lazy popup.** The popup is built on first click on a given
>   cell, not eagerly for every row.
>
> - **CanExecute only on open.** Call `ICommand.CanExecute(row)`
>   when a popup opens for that row. Never evaluate `CanExecute`
>   across all rows (e.g. to drive per-row icon visibility), and
>   never bind a row-cell property to `CanExecute`. WPF's
>   `CommandManager.RequerySuggested` fires aggressively; a per-row
>   binding turns each requery into a 2M-row walk.

**Task 6.6a — ActionsMenu CanExecute verification (NUnit)**

- Wrap the three sample item commands' `CanExecute` in counters.

- Construct a GridControl with the sample data source (10 groups, all
  expanded — or scale to 100k synthetic rows for the perf assert).

- Force scroll across the viewport repeatedly.

- Assert: total `CanExecute` invocation count tracks the **realized
  container** count (tens), not the row count (thousands /
  millions). A failure here is a hard regression — perf budget cannot
  absorb it.

**Task 6.7 — Built-in SelectionColumn**

- Inserted into / removed from the `Columns` collection by the
  `ShowSelectionColumn` DP change handler (Task 5.2). Leftmost,
  non-reorderable.

- Item rows: standard `CheckBox` two-way bound to
  `GridItemRow.IsSelected`.

- Group rows: tri-state `CheckBox` (`IsChecked = null` for
  `PartiallySelected`).

- Clicking the cell triggers Task 5.5's selection-column path:
  select + highlight, multi-select via Ctrl / Shift.

- Disabled rows: `CheckBox.IsEnabled=false`.

**Phase 7 — Styles & Selectors**

> *Row and cell styles target the custom row container
> (GridRowPresenter, Task 5.8) and the custom cells panel
> (GridCellsPanel, Task 5.8), NOT DataGridRow / DataGridCell. Generic.xaml
> also carries the GridControl ControlTemplate from §5.2 and the column
> headers' template from Task 5.9.*

**Task 7.1 — Selectors/GridRowStyleSelector.cs**

- Extends `StyleSelector`.

- `SelectStyle` switches on the `IGridRow.Kind` (Group vs Item) and
  returns `GroupRowStyle` for `GridGroupRow` instances, `ItemRowStyle`
  for `GridItemRow`.

- Applied via `GridControl.ItemContainerStyleSelector`, so each
  realized `GridRowPresenter` gets the right style.

**Task 7.2 — Cell visual states (attached properties + triggers)**

> *Design change (2026-06-14): the original plan was a
> `Selectors/GridCellStyleSelector : StyleSelector`. That was dropped — a
> `StyleSelector` is stateless and evaluated only once when a cell container
> is created, so it cannot react to selection / disable / edit changes that
> happen afterward, and a cell `ContentControl` exposes no row-disabled /
> highlighted / editing properties for it to switch on. Cell visual states
> are instead driven by attached properties set by `GridCellsPanel`, with
> triggers in the shared cell `Style`.*

- `Controls/GridCell.cs` defines attached properties set by `GridCellsPanel`
  on each cell `ContentControl`:
  - `GridCell.IsRowDisabled` — `true` when the cell's row is disabled, i.e.
    the row's `IsEnabled=false` **or** its owning group is disabled.
  - `GridCell.IsRowHighlighted` — mirrors the row's `IsHighlighted`.
  - `GridCell.IsEditing` — `true` while this specific cell is in edit mode.

- `GridCellsPanel` sets these when it builds cells and refreshes them on
  `GridControl.EditStateChanged` and on the row's `PropertyChanged`
  (`IsEnabled` / `IsHighlighted`), so the states react live.

- The shared cell `Style` in `Generic.xaml` carries the
  disabled / highlighted / editing trigger setters (the former
  `DisabledCellStyle` / `HighlightedCellStyle` / `EditingCellStyle`).

**Task 7.3 — Themes/Generic.xaml**

- `GridControl` default `ControlTemplate`: three-row Grid (header bar,
  column headers, body ScrollViewer with ItemsPresenter). ItemsPanel
  template = `VirtualizingStackPanel` with
  `IsVirtualizing=True`, `VirtualizationMode=Recycling`,
  `ScrollUnit=Item`. Body `ScrollViewer.CanContentScroll=True`. See
  design doc §5.2 and Task 5.10's checklist.

- `GridRowPresenter` default style + template: 1-row container hosting
  a `GridCellsPanel`; visual states for `IsSelected`, `IsHighlighted`,
  `IsEnabled`, and `IsEditing`.

- `GroupRowStyle` (for GridGroupRow rows): expand / collapse chevron
  bound to `IsExpanded`, label, optional tri-state checkbox bound to
  `SelectionState`. The chevron handler routes to
  `ExpandGroupCommand` / `CollapseGroupCommand` (Task 5.4).

- `ItemRowStyle` (for GridItemRow rows): standard row with triggers
  for highlight / disabled / selected.

- `DisabledRowStyle`: grayed background, blocked interaction via
  `IsHitTestVisible=false`.

- `HighlightStyle`: fixed highlight color via trigger on
  `IsHighlighted`. Color is a `Brush` resource so themes can override
  it.

- `SelectionStyle`: selection brush applied on `IsSelected=true`.

- `EditingStyle`: highlight is preserved during edit if the row was
  already highlighted (i.e. selected) before editing started.

- `GridColumnHeadersPresenter` default style + template: rendered
  outside the body ScrollViewer; horizontal offset bound to body
  `ScrollViewer.HorizontalOffset` so headers track horizontal scroll.
  Header cells are laid out by the same column widths as the rows.

**Phase 8 — Sample Application**

The sample app is a working MVVM demo with the grid on the left and a
live event log on the right. It exercises every column type, both action
menus, and every routed event.

**8.0 — Layout**

- Two-column window: GridControl on the left (~65% width), event log
  panel on the right (~35% width).

- Header bar above the grid with Add Group and Delete Selected buttons.

- A SingleExpandMode toggle and a ShowSelectionColumn toggle in the
  header area.

**Task 8.1 — Sample Row Types**

- SampleGroupRow : GridGroupRow — adds string Description (editable
  text), string Status (Enable / Disable).

- SampleItemRow : GridItemRow — adds float X, float Y.

- Both raise PropertyChanged so edits flow back to the log.

**Task 8.2 — Sample Columns**

- Group row columns: Description (TextColumn, editable), Status
  (ComboBoxColumn, editable; options Enable / Disable), Actions
  (ActionsMenuColumn).

- Item row columns: X (TextColumn, float, editable), Y (TextColumn,
  float, editable), Actions (ActionsMenuColumn).

- Group-only columns render blank on item rows and vice versa (standard
  two-level behavior).

**Task 8.3 — SampleDataSource.cs**

- Implements IGridDataSource via InMemoryGridDataSource.

- 10 groups named "Group 001"..."Group 010", each with 8-12 items
  (randomized).

- Item X / Y values: random floats to 2 decimal places.

- Group 003: IsEnabled = false (tests disabled group).

- Group 002, item index 4: IsEnabled = false (tests disabled item).

- Group 001, item index 2: IsHighlighted = true (tests highlight).

**Task 8.4 — Action Menus**

- GroupRowActions: Copy, Modify.

- ItemRowActions: Copy, Modify, Delete.

- Each action command logs to the event log; Delete removes the item
  from the data source.

**Task 8.5 — LogEntry.cs + LogViewModel.cs**

- LogCategory enum: Selection, Edit, Action.

- LogEntry: DateTime Timestamp, LogCategory Category, string Message.

- LogViewModel: ObservableCollection\<LogEntry\> Entries,
  AddEntry(category, message), Clear(), EntryCount.

- Entries are newest-first (insert at index 0) or auto-scrolled to
  newest.

**Task 8.6 — MainViewModel.cs — Wiring**

- Expose DataSource, SelectedRow, SelectedRows, HeaderActions,
  GroupRowActions, ItemRowActions.

- SingleExpandMode and ShowSelectionColumn toggle commands.

- Subscribe to grid routed events and translate each into a log entry:

  - SelectedRowChanged -\> SELECTION "Row selected: {label}" or
    "Selection cleared".

  - SelectedRowsChanged -\> SELECTION "{n} rows selected
    (+{added}/-{removed})".

  - GroupExpanded / GroupCollapsed -\> EXPAND (logged under Action
    category).

  - CellEditCommitted -\> EDIT "{row} -\> {column} changed: {old} -\>
    {new}".

  - CellEditCancelled -\> EDIT "Edit cancelled: {row} / {column}".

  - Header Add Group -\> ACTION "New group added: {label}".

  - Header Delete Selected -\> ACTION "Deleted selected: {g} groups, {i}
    items".

  - Row Copy / Modify / Delete -\> ACTION with the row identity.

**Task 8.7 — MainWindow.xaml**

- Grid column (left): GridControl with the sample columns,
  ShowSelectionColumn=True, header bar bound to HeaderActions.

- Log column (right): scrollable ItemsControl/ListBox bound to
  LogViewModel.Entries.

- Each log entry: fixed-width timestamp, category badge (color-coded),
  message text.

- Category colors: SELECTION=blue, EDIT=orange, ACTION=red.

- Newest entry auto-scrolls into view; "Clear Log" button; entry count
  label ("42 events").

**Task 8.8 — Integration Verification (manual, sample app)**

- CellEditCommitted fires with correct old/new values for Description
  (string), Status (display string), X/Y (float text).

- Disabled group (Group 003) blocks expand and child selection.

- Disabled item (Group 002 / item 4) cannot be selected or edited;
  range-select skips it.

- Highlighted item (Group 001 / item 2) shows highlight without being
  selected.

- SingleExpandMode collapses the previously expanded group on new
  expand.

- ExpandAllCommand is disabled when SingleExpandMode=true.

- Group tri-state checkbox transitions Deselected -\> Full -\>
  Deselected and shows partial square correctly.

- SelectedRowsChanged fires with correct added/removed rows and logs
  them.

- GroupExpanded / GroupCollapsed routed events bubble to the Window and
  log.

- ActionsMenu CanExecute disables menu items correctly; Delete removes
  the item and logs it.

- Highlight is preserved during cell edit if the row was selected before
  editing.

- Multi-select is preserved when editing a different row's cell.

- Every logged event appears in the right-hand panel with the correct
  category color and newest-first ordering.

> *This manual verification list mirrors the full Tier 3 manual test
> plan (MT-01 .. MT-14) captured in the handoff notes.*
