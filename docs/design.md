**CustomDataGrid**

Technical Design Document

*Version 2.0 \| .NET Framework 4.7.2 \| C# 7.3*

**Revision history**

- v1.0 — initial design; control subclasses `System.Windows.Controls.DataGrid`.
- v2.0 — control rebased on `ItemsControl` + `VirtualizingStackPanel`.
  Phases 1–4 (Contracts, Models, FlatRowCollection, InMemoryGridDataSource) are
  unchanged. Phases 5–7 are rewritten around an `ItemsControl`-based shell,
  with a custom row container, a custom cells panel, and a shared column-width
  collection. Rationale: our hardest requirements (custom selection,
  click-to-edit-vs-select, 2M-row virtualization, tri-state group selection)
  are exactly where DataGrid's built-in behavior fights us. See §5.1.

**1. Architecture Overview**

CustomDataGrid is structured in layers. Each layer has a single
responsibility and depends only on layers below it.

|                 |                                                   |
|-----------------|---------------------------------------------------|
| **Layer**       | **Contents**                                      |
| **Contracts**   | Interfaces, enums, EventArgs — no implementation  |
| **Models**      | GridGroupRow, GridItemRow, action implementations |
| **Collection**  | FlatRowCollection — virtual IList engine          |
| **DataSources** | InMemoryGridDataSource — default implementation   |
| **Controls**    | GridControl, GridHeaderBar, GridRowPresenter, GridCellsPanel, GridColumnHeadersPresenter — WPF UI |
| **Columns**     | Column definitions (GridColumn + 5 column types)  |
| **Selectors**   | Row and cell style selectors                      |
| **Themes**      | Generic.xaml — default control templates          |

**2. Project Structure**

> CustomDataGrid/
>
> Contracts/
>
> IGridRow.cs
>
> IGridDataSource.cs
>
> IGridCollapseHint.cs
>
> IGridRowAction.cs
>
> IGridHeaderAction.cs
>
> SelectionState.cs
>
> Events/
>
> GroupExpandedEventArgs.cs
>
> GroupCollapsedEventArgs.cs
>
> SelectedRowChangedEventArgs.cs
>
> SelectedRowsChangedEventArgs.cs
>
> CellEditCommittedEventArgs.cs
>
> CellEditCancelledEventArgs.cs
>
> Models/
>
> GridGroupRow.cs
>
> GridItemRow.cs
>
> GridRowAction.cs
>
> GridHeaderAction.cs
>
> Collection/
>
> FlatRowCollection.cs
>
> DataSources/
>
> InMemoryGridDataSource.cs
>
> Controls/
>
> GridControl.xaml
>
> GridControl.xaml.cs
>
> GridHeaderBar.xaml
>
> GridHeaderBar.xaml.cs
>
> GridRowPresenter.cs            // custom row container (ItemsControl ItemContainer)
>
> GridCellsPanel.cs              // custom Panel; arranges cells by shared widths
>
> GridColumnHeadersPresenter.cs  // header row above the body ScrollViewer
>
> Columns/
>
> GridColumn.cs                  // abstract base (DependencyObject, NOT DataGridColumn)
>
> GridColumnCollection.cs        // shared column-width collection
>
> TextColumn.cs
>
> CheckBoxColumn.cs
>
> ComboBoxColumn.cs
>
> ButtonColumn.cs
>
> ActionsMenuColumn.cs
>
> Selectors/
>
> GridRowStyleSelector.cs
>
> GridCellStyleSelector.cs
>
> Themes/
>
> Generic.xaml

**3. FlatRowCollection Design**

**3.1 Purpose**

FlatRowCollection is a virtual IList\<IGridRow\> that serves as the
ItemsSource for GridControl. It does not hold item objects in memory —
it resolves them from IGridDataSource on demand. WPF's
VirtualizingStackPanel only calls the indexer for rows currently in the
viewport.

**3.2 Internal State**

> private readonly IGridDataSource \_source;
>
> private readonly List\<GroupState\> \_groupStates;
>
> private bool \_singleExpandMode;
>
> private int \_currentExpandedGroupIndex = -1;
>
> private struct GroupState
>
> {
>
> public int FlatOffset; // index of group header in flat list
>
> public bool IsExpanded;
>
> public int LoadedItemCount; // items currently inserted into flat list
>
> }

**3.3 Index Resolution**

The indexer uses binary search over GroupState.FlatOffset — O(log G)
where G is group count. No full list scan needed.

> public IGridRow this\[int index\]
>
> {
>
> get
>
> {
>
> // Binary search to find which group owns this flat index
>
> // If index == group offset -\> return group row
>
> // Else -\> return item row at (index - groupOffset - 1)
>
> }
>
> }

**3.4 Expand / Collapse**

Expand and collapse use incremental CollectionChanged (Add/Remove
range), never Reset. This preserves VirtualizingStackPanel container
recycling.

> // Expand: insert items after group header
>
> int insertAt = FlatIndexOf(groupIndex) + 1;
>
> for (int i = 0; i \< group.Items.Count; i++)
>
> \_flatList.Insert(insertAt + i, ...);
>
> RaiseCollectionChanged(Add, insertAt, count);
>
> // Collapse: remove items after group header
>
> int removeAt = FlatIndexOf(groupIndex) + 1;
>
> for (int i = 0; i \< loadedCount; i++)
>
> \_flatList.RemoveAt(removeAt);
>
> RaiseCollectionChanged(Remove, removeAt, count);

**3.5 SingleExpandMode**

> public void SetExpanded(int groupIndex, bool expanded)
>
> {
>
> if (expanded && \_singleExpandMode)
>
> {
>
> if (\_currentExpandedGroupIndex \>= 0
>
> && \_currentExpandedGroupIndex != groupIndex)
>
> {
>
> CollapseGroup(\_currentExpandedGroupIndex);
>
> var hint = \_source as IGridCollapseHint;
>
> if (hint != null)
>
> hint.OnGroupCollapsed(\_currentExpandedGroupIndex);
>
> }
>
> }
>
> if (expanded) ExpandGroup(groupIndex);
>
> else CollapseGroup(groupIndex);
>
> }

**4. Key Contract Interfaces**

**4.1 IGridRow**

> public interface IGridRow
>
> {
>
> RowKind Kind { get; } // Group \| Item
>
> bool IsEnabled { get; }
>
> bool IsHighlighted { get; }
>
> }

**4.2 IGridDataSource**

> public interface IGridDataSource
>
> {
>
> int GroupCount { get; }
>
> int GetItemCount(int groupIndex);
>
> GridGroupRow GetGroup(int groupIndex);
>
> GridItemRow GetItem(int groupIndex, int itemIndex);
>
> IList\<GridItemRow\> GetItems(int groupIndex, int startIndex, int
> count);
>
> event EventHandler\<GroupChangedEventArgs\> GroupChanged;
>
> event EventHandler\<ItemChangedEventArgs\> ItemChanged;
>
> event EventHandler DataReset;
>
> }

**4.3 IGridCollapseHint**

> // Optional — FlatRowCollection casts with "as", no runtime failure if
> absent
>
> public interface IGridCollapseHint
>
> {
>
> void OnGroupCollapsed(int groupIndex);
>
> }

**4.4 SelectionState Enum**

> public enum SelectionState
>
> {
>
> Deselected,
>
> PartiallySelected,
>
> FullySelected
>
> }

**5. GridControl**

**5.1 Base Class — ItemsControl, not DataGrid, not Selector/MultiSelector**

GridControl extends `System.Windows.Controls.ItemsControl` directly.

**Why not DataGrid.** The control's hardest requirements diverge from
DataGrid's defaults at every interaction point:

- Click on an editable cell must *not* select the row — DataGrid selects
  the row on every cell click.
- Group rows have tri-state selection driven by their children — DataGrid
  has no equivalent.
- Selecting a group selects all enabled children — DataGrid does not
  cascade selection.
- Range-select must silently skip disabled rows — DataGrid does not skip.
- Multi-select must be preserved while editing a different row — DataGrid
  collapses selection on edit.
- 2M-row virtualization with on-demand row fetching from IGridDataSource —
  works against DataGrid's row-realization assumptions and its built-in
  column-width measurement.

Inheriting DataGrid would mean overriding most of its row-realization,
selection, and edit pipeline, while still inheriting its column-width
machinery (which measures rows to size columns — incompatible with
2M-row virtualization, see §5.4).

**Why not Selector / MultiSelector.** Same fighting-the-base problem in
miniature. Selector and MultiSelector hard-wire `IsSelected` propagation,
keyboard selection, and `SelectedItem`/`SelectedItems` semantics that do
not match our rules (edit-doesn't-select, tri-state groups,
group-selects-children, disabled-skip, multi-select-preserved-during-edit).
Inheriting either of them means overriding most of it.

**What we do instead.** A bare ItemsControl. We own the selection model
through `IsSelected` on rows (Phase 2) plus the `SelectedRows`
ObservableCollection (Phase 5 DPs, defined in Phase 1's event args).
Keyboard / arrow-key navigation is added explicitly via
`KeyboardNavigation` attached properties only if a v1 task surfaces a
concrete need; nothing is inherited "for free."

**5.2 Visual tree (default ControlTemplate)**

> \<Grid\>                                 // root
>
>   \<Grid.RowDefinitions\>
>
>     \<RowDefinition Height="Auto"/\>    // 0: header bar (GridHeaderBar)
>
>     \<RowDefinition Height="Auto"/\>    // 1: column-headers row
>
>     \<RowDefinition Height="\*"/\>      // 2: body
>
>   \</Grid.RowDefinitions\>
>
>   \<GridHeaderBar Grid.Row="0" ItemsSource="{Binding HeaderActions, ...}"/\>
>
>   \<GridColumnHeadersPresenter Grid.Row="1"
>
>       Columns="{TemplateBinding Columns}"
>
>       HorizontalOffset="{Binding HorizontalOffset, ElementName=PART\_Scroll}"/\>
>
>   \<ScrollViewer x:Name="PART\_Scroll" Grid.Row="2"
>
>       CanContentScroll="True"
>
>       HorizontalScrollBarVisibility="Auto"
>
>       VerticalScrollBarVisibility="Auto"\>
>
>     \<ItemsPresenter/\>
>
>   \</ScrollViewer\>
>
> \</Grid\>
>
> // ItemsPanel (overridden):
>
> \<ItemsPanelTemplate\>
>
>   \<VirtualizingStackPanel
>
>       VirtualizingPanel.IsVirtualizing="True"
>
>       VirtualizingPanel.VirtualizationMode="Recycling"
>
>       VirtualizingPanel.ScrollUnit="Item"/\>
>
> \</ItemsPanelTemplate\>
>
> // ItemContainer (overridden via GetContainerForItemOverride):
>
> // returns GridRowPresenter — a custom ContentControl whose Template hosts
>
> // a GridCellsPanel that lays out one cell per column using the shared widths.

**5.3 Dependency Properties**

|                         |                            |             |                                    |
|-------------------------|----------------------------|-------------|------------------------------------|
| **DP Name**             | **CLR Type**               | **Default** | **Notes**                          |
| **DataSource**          | IGridDataSource            | null        | Triggers FlatRowCollection rebuild |
| **Columns**             | GridColumnCollection       | empty       | Shared column-width source (§5.4)  |
| **SelectedRow**         | IGridRow                   | null        | Two-way                            |
| **SelectedRows**        | ObservableCollection       | empty       | Two-way                            |
| **HeaderActions**       | IList\<IGridHeaderAction\> | null        | Bound to GridHeaderBar             |
| **GroupRowActions**     | IList\<IGridRowAction\>    | null        | Passed to ActionsMenuColumn        |
| **ItemRowActions**      | IList\<IGridRowAction\>    | null        | Passed to ActionsMenuColumn        |
| **SingleExpandMode**    | bool                       | false       | Propagated to FlatRowCollection    |
| **ShowSelectionColumn** | bool                       | false       | Shows/hides leftmost checkbox col  |
| **IsReadOnly**          | bool                       | false       | Global edit lock                   |

**5.4 Column-width sharing**

This is the hard problem DataGrid was solving for us. The design:

- `GridColumnCollection` is an `ObservableCollection<GridColumn>` exposed
  on `GridControl.Columns`.
- Each `GridColumn` has a `Width` DependencyProperty (currently
  fixed/star — see "v1 scope" below) and is itself a `DependencyObject`
  so it can be the binding source for cell widths.
- `GridColumnHeadersPresenter` (the header row) and every realized
  `GridCellsPanel` (one per row container) both look up the same
  `GridColumn` instances and arrange themselves by those widths.
- Widths flow **down** from `Columns` to the header and rows via
  bindings + change notifications. Rows never measure **up** to publish
  a width back to the columns.

**Warning: do NOT use Grid.IsSharedSizeScope / SharedSizeGroup for
column alignment.** SharedSizeGroup forces WPF to measure every member
of the scope to determine the shared width — at 2M rows that realizes
every row, defeats virtualization, and freezes the UI. The pattern to
follow is the one `GridViewRowPresenter` uses internally: a shared
width collection consulted by each cells panel during arrange, with
change notifications when a width changes. No shared-size scope.

**v1 scope.** Fixed widths (`Width="180"`) and star widths
(`Width="*"`, `Width="2*"`) computed once at layout time from the
viewport width. Star widths are resolved by the GridCellsPanel against
the available width — not against rendered cell content. Column
resizing (mouse drag on a header divider, auto-fit-to-content) is a
later optional task, not v1. This keeps the design strictly
virtualization-safe: widths are never derived from cell content.

**5.5 Header row outside the scroll area**

The column-headers row lives **outside** the body ScrollViewer (it sits
in row 1 of the control template, the body sits in row 2). This means:

- The header row does not scroll vertically with the rows.
- The header's horizontal offset is bound to the body
  `ScrollViewer.HorizontalOffset` so columns still track horizontal
  scroll.
- The header consumes the same `Columns` collection as the body, so
  header and rows are guaranteed to align without any shared-size
  scope.

**5.6 Selection model**

State is kept on the rows (`IsSelected` on `GridItemRow`,
`SelectionState` on `GridGroupRow` — see Phase 2) and on the control
(`SelectedRow`, `SelectedRows`).

Click resolution, implemented on the ItemsControl (no DataGrid
overrides):

- **Editable cell click** → enter edit mode; do **not** change selection.
- **Non-editable cell / empty row space** → select + highlight; clear
  prior single-select unless modifier held.
- **CheckBox data column** → toggle bound value; no selection change.
- **Button column** → invoke command; no selection change.
- **ActionsMenu column** → open popup; no selection change.
- **Selection column** → toggle row's `IsSelected`; highlight; multi-select
  uses standard Ctrl / Shift modifiers.
- **Group row click** → tri-state state machine:
  Deselected → FullySelected; Partial → FullySelected; Full → Deselected.
  When transitioning to FullySelected, set `IsSelected = true` on all
  enabled children. When transitioning to Deselected, clear them.
- **Child deselect** → recompute parent's `SelectionState`
  (Deselected / Partial / Full) by inspecting enabled children only.
- **Disabled rows** → ignored by all click handlers; range-select skips
  them silently; `SelectRowCommand` and `BeginEditRowCommand` no-op.
- **Multi-select is preserved** when editing a different row's cell —
  the click that started the edit only enters edit mode, it does not
  collapse the selection.

**5.7 Routed events & commands**

The six routed events and the inbound commands carry over from v1
unchanged in shape, but are raised by `GridControl` (ItemsControl)
rather than via DataGrid overrides:

- Outbound (Bubble): `SelectedRowChanged`, `SelectedRowsChanged`,
  `GroupExpanded`, `GroupCollapsed`, `CellEditCommitted`,
  `CellEditCancelled`.
- Inbound (ICommand DPs): `ScrollToRowCommand`, `SelectRowCommand`,
  `ExpandGroupCommand`, `CollapseGroupCommand`, `CollapseAllCommand`,
  `ExpandAllCommand` (CanExecute=false when `SingleExpandMode`=true),
  `BeginEditRowCommand`.

`ScrollToRowCommand` is implemented by scrolling the body
`ScrollViewer` to the row index resolved from `FlatRowCollection`
(no DataGrid `ScrollIntoView` available). Item-mode scrolling means
the offset is row-indexed, which matches the FlatRowCollection
indexer directly.

**5.8 Virtualization config checklist**

Each of these settings silently disables virtualization if wrong. They
must all be set correctly on the GridControl ControlTemplate and the
default ItemsPanel:

- [ ] `VirtualizingPanel.IsVirtualizing = True` on the ItemsControl.
- [ ] `VirtualizingPanel.VirtualizationMode = Recycling`. Recycling is
  **mandatory** at this scale — Standard mode constructs and discards
  containers on every scroll, causing GC churn that visibly stutters at
  2M rows.
- [ ] Host `ScrollViewer.CanContentScroll = True`. When `False` the
  ScrollViewer scrolls by pixels and bypasses item-based virtualization
  entirely.
- [ ] `VirtualizingPanel.ScrollUnit = Item`. Item-mode is lighter than
  Pixel-mode and matches our row-indexed FlatRowCollection.
- [ ] `ItemsSource` is the `IList<IGridRow>`-implementing
  `FlatRowCollection`. WPF only realizes a visible index window when the
  source implements `IList` (the indexer is what VSP calls during
  measure / arrange).

**5.9 Data-virtualization trap (CollectionView)**

Setting `ItemsControl.ItemsSource` causes WPF to wrap the source in a
default `CollectionView`. Sorting, filtering, or grouping configured on
that view — and some Refresh paths — will **enumerate the entire
collection**. For our on-demand `IGridDataSource` that means loading
all 2M rows up front, which defeats data virtualization completely
(while UI virtualization still appears to be working — the trap is
silent).

Mitigation:

- Keep the default view free of `SortDescriptions`, `Filter`, and
  `GroupDescriptions`. If sort / filter / group is ever required, it
  must be applied at the data-source level, not on the view.
- Do not bind anything that enumerates `ItemsSource` (e.g. converters,
  `Count` accessors that walk the sequence, "any item matches" checks).
- Avoid `CollectionView.Refresh()` on the default view.

Verification (Phase 5 task): with an instrumented Moq fake of
`IGridDataSource` that counts `GetGroup` / `GetItem` / `GetItems`
calls, load a million-row source, scroll, and assert that the fetched
row count tracks the realized-container count (tens), not the row
count (millions).

**6. Performance Design**

**6.1 Memory model**

|                                            |                                                |
|--------------------------------------------|------------------------------------------------|
| **Scenario**                               | **Items in Memory**                            |
| **All groups collapsed**                   | G group objects only (~2MB for 10k groups)     |
| **SingleExpandMode, 1 group open**         | G groups + 1 group's items (bounded)           |
| **All groups expanded (in-memory source)** | All N items — ~200MB at 2M rows x 100 bytes    |
| **Paged source, SingleExpandMode**         | G groups + 1 page of items (~10k rows typical) |

**6.2 Virtualization rules**

- The ItemsPanel is `VirtualizingStackPanel`. Configuration is fixed by
  the checklist in §5.8.
- `CollectionChanged` never fires Reset during normal group
  expand/collapse — only targeted Add/Remove (Phase 3 invariant).
- `ComboBoxColumn.ItemsSource` is set at column level — not per cell.
- CheckBox cells are rendered from a simple `CheckBox` in a template;
  no per-row column object is constructed (we are not a DataGrid).

**6.3 ActionsMenu performance**

The ActionsMenu column is the most attractive footgun in the design.
Rules:

- **Shared commands.** `GroupRowActions` and `ItemRowActions` are
  collections of shared `IGridRowAction` instances defined **once** at
  the GridControl level. For the sample app there are three item
  commands total (Copy, Modify, Delete) — **not** three per row. The
  current row is passed as `ICommand.CommandParameter` (the
  `IGridRow`). Per-row `CanExecute(IGridRow)` returns the per-row
  enabled state.
- **Warning: never construct per-row command instances.** 2M rows × 3
  commands = ~6M command objects. Even at ~80 bytes each that is
  hundreds of megabytes of objects sitting in Gen 2, plus the GC churn
  of allocating them as containers recycle. The same three command
  instances must be reused for every row.
- **Lazy popup.** The actions Popup is built on first click, not
  eagerly per row. The cell template renders only the trigger button
  until then.
- **CanExecute only on open.** Evaluate `ICommand.CanExecute(row)`
  only when a popup is opened for that row. Never evaluate
  `CanExecute` across all rows (e.g. to drive per-row icon visibility),
  and never bind a row property to `CanExecute`. WPF's
  `CommandManager.RequerySuggested` fires aggressively (focus changes,
  keyboard input); a per-row binding turns each requery into a 2M-row
  walk.
- **Verification (Phase 6 task).** Wrap the three command instances'
  `CanExecute` in a counter, scroll hard with all groups expanded,
  and assert that the call count tracks realized container count
  (tens), not row count (millions). Test failure here is a hard
  regression — the perf budget cannot absorb it.

**7. C# 7.3 Compliance Notes**

|                                                |                                                |
|------------------------------------------------|------------------------------------------------|
| **Pattern**                                    | **C# 7.3 Approach**                            |
| **Default interface methods**                  | Separate opt-in interface (IGridCollapseHint)  |
| **Nullable reference types**                   | XML doc comments + naming convention           |
| **Records**                                    | Regular classes with INPC                      |
| **Switch expressions**                         | switch statements                              |
| **Range / index operators**                    | GetRange(start, count), list\[list.Count - 1\] |
| Allowed: tuples, local funcs, out vars, ?., ?? | All available in C# 7.x                        |
