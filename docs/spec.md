**CustomDataGrid**

Requirements Specification

*Version 1.0 \| Platform: .NET Framework 4.7.2 \| Language: C# 7.3*

**1. Overview**

CustomDataGrid is a WPF control that extends the native DataGrid with
two-level hierarchical data (groups and items), rich column types,
MVVM-friendly bindings, inter-control communication via routed events,
and optional data virtualization support for up to 2,000,000 rows.

**2. Platform & Constraints**

|                      |                                               |
|----------------------|-----------------------------------------------|
| **Constraint**       | **Value**                                     |
| **Target Framework** | .NET Framework 4.7.2                          |
| **Language**         | C# 7.3 maximum                                |
| **UI Framework**     | WPF (Windows Presentation Foundation)         |
| **Base Control**     | System.Windows.Controls.DataGrid (subclassed) |
| **Architecture**     | MVVM — no code-behind required by consumers   |

> *C# 8+ features are prohibited: no default interface methods, no
> nullable reference types (?), no records, no switch expressions, no
> ranges/indices.*

**3. Data Model**

**3.1 Two-Level Hierarchy**

The control operates on a two-level data hierarchy:

- Group Row — a container with a label, expand/collapse state, and child
  items

- Item Row — the actual data rows, always belonging to a group

**3.2 Row States**

|                   |          |                                                               |
|-------------------|----------|---------------------------------------------------------------|
| **Property**      | **Type** | **Description**                                               |
| **IsEnabled**     | bool     | If false: row cannot be selected, highlighted, or edited      |
| **IsHighlighted** | bool     | Applies a fixed highlight color to the row                    |
| **IsSelected**    | bool     | Reflects selection state. Consumer may bind to IsHighlighted. |
| **IsExpanded**    | bool     | Group rows only — controls child visibility                   |

**3.3 Group Row Selection States**

|                       |                           |                                            |
|-----------------------|---------------------------|--------------------------------------------|
| **State**             | **Checkbox (if visible)** | **Condition**                              |
| **FullySelected**     | Tick ✓                    | Group + all enabled children selected      |
| **PartiallySelected** | Solid square ■            | Some but not all enabled children selected |
| **Deselected**        | Empty □                   | No children selected                       |

**4. Column Types**

|                       |                                 |                                                                                  |
|-----------------------|---------------------------------|----------------------------------------------------------------------------------|
| **Column**            | **Edit Control**                | **Notes**                                                                        |
| **TextColumn**        | TextBox                         | Read-only or editable. Shows plain text when not editing.                        |
| **CheckBoxColumn**    | CheckBox (native)               | Toggle value. Does not trigger row select/highlight.                             |
| **ComboBoxColumn**    | ComboBox (on edit only)         | Shows display string when not editing. ItemsSource at column level.              |
| **ButtonColumn**      | N/A                             | Executes ICommand. Does not trigger row select/highlight.                        |
| **ActionsMenuColumn** | Popup menu                      | Icon button opens lazy popup. Does not trigger row select/highlight.             |
| **SelectionColumn**   | CheckBox (tri-state for groups) | Shown when ShowSelectionColumn=true. Leftmost, fixed. Triggers select/highlight. |

**5. Click Behavior**

**5.1 What Triggers Selection**

|                                   |                 |                 |                           |
|-----------------------------------|-----------------|-----------------|---------------------------|
| **Click Target**                  | **Enters Edit** | **Selects Row** | **Highlights Row**        |
| **Editable cell**                 | Yes             | No              | No (preserved if already) |
| **Non-editable / read-only cell** | No              | Yes             | Yes                       |
| **Empty row space**               | No              | Yes             | Yes                       |
| **CheckBox data column**          | Toggles value   | No              | No                        |
| **Button column**                 | No              | No              | No                        |
| **Actions menu column**           | No              | No              | No                        |
| **Selection column checkbox**     | No              | Yes             | Yes                       |

**5.2 Edit Mode**

- Clicking away from an editing cell commits the edit.

- A row being edited is not selected unless it was already selected
  before editing.

- Multi-selected rows remain selected while another row is being edited.

**6. Selection Rules**

**6.1 Disabled Row Rules**

- A disabled row cannot be selected, highlighted, or edited.

- If a group row is disabled, all child item rows are treated as
  disabled (inherited, not overwritten).

- A disabled group row cannot be expanded or collapsed.

- Range selection silently skips disabled rows.

- SelectRowCommand and BeginEditRowCommand on a disabled row are no-ops.

- CellEditCommitted / CellEditCancelled events are unreachable for
  disabled rows — document this explicitly, no runtime guard needed.

**6.2 Group Row Selection Behavior**

- Selecting a group row selects the group and all enabled children.

- Deselecting one child moves the group to PartiallySelected.

- Deselecting all children moves the group to Deselected.

- Clicking a PartiallySelected group is treated as Deselected for click
  purposes — moves to FullySelected.

- Clicking a FullySelected group moves to Deselected.

**6.3 Disabled Row Becomes Selected**

If a row is selected and subsequently becomes disabled, it is not
automatically deselected. The consumer is responsible for managing this
state.

**7. SingleExpandMode**

A boolean dependency property on CustomDataGrid. Default: false.

|                 |                                                                                                                                 |
|-----------------|---------------------------------------------------------------------------------------------------------------------------------|
| **Value**       | **Behavior**                                                                                                                    |
| false (default) | Multiple groups can be expanded simultaneously.                                                                                 |
| true            | Only one group can be expanded at a time. Expanding a new group collapses the previous one first. ExpandAllCommand is disabled. |

- When toggled to true at runtime, all but the first expanded group are
  collapsed.

- FlatRowCollection checks IsEnabled before expanding a group.

- IGridDataSource.OnGroupCollapsed() hint is called after collapsing —
  allowing paging implementations to release memory.

**8. Grid Header Bar**

A toolbar rendered above the grid. Defined by an
IList\<IGridHeaderAction\> bound to HeaderActions on CustomDataGrid.

- Each action exposes: Label, Icon (nullable), Command (ICommand),
  IsEnabled.

- Buttons are rendered right-aligned by default.

- Header actions are separate from row actions — different interface
  types.

**9. Actions Menu Column**

The ActionsMenuColumn renders an icon button per row. Clicking opens a
popup menu.

**9.1 Scope**

- GroupRowActions: IList\<IGridRowAction\> — same menu for all group
  rows.

- ItemRowActions: IList\<IGridRowAction\> — same menu for all item rows.

- Group actions and item actions are independent lists.

**9.2 IGridRowAction**

- Label: display text

- Icon: ImageSource (nullable)

- Command: ICommand — CommandParameter is the IGridRow instance

- IsSeparator: bool — renders a visual divider, no command

- Enable/disable: resolved via ICommand.CanExecute(IGridRow)

**9.3 Custom Template**

- ActionsMenuColumn.ActionsMenuTemplate: DataTemplate — DataContext is
  IGridRow.

- If null, the control uses its default popup menu UI.

**10. Data Virtualization**

The control provides an interface contract for data virtualization.
Implementation is the consumer's responsibility.

**10.1 IGridDataSource**

- GroupCount: int — total groups, must be known upfront.

- GetItemCount(int groupIndex): int — item count per group, known
  upfront.

- GetGroup(int groupIndex): GridGroupRow

- GetItem(int groupIndex, int itemIndex): GridItemRow

- GetItems(int groupIndex, int startIndex, int count):
  IList\<GridItemRow\> — bulk fetch.

- Events: GroupChanged, ItemChanged, DataReset — consumer fires when
  source mutates.

**10.2 IGridCollapseHint (optional)**

Separate opt-in interface. FlatRowCollection checks with "as" cast at
runtime.

> var hint = \_source as IGridCollapseHint;
>
> hint?.OnGroupCollapsed(groupIndex);

Allows paging implementations to release cached pages for a collapsed
group.

**10.3 InMemoryGridDataSource**

A default implementation backed by IList\<GridGroupRow\>. Provided free
— no paging or virtualization. Suitable for datasets that fit
comfortably in RAM.

**11. MVVM Binding Surface**

|                         |                                  |                             |
|-------------------------|----------------------------------|-----------------------------|
| **Property**            | **Type**                         | **Description**             |
| **DataSource**          | IGridDataSource                  | Primary data binding        |
| **SelectedRow**         | IGridRow                         | Two-way, single selection   |
| **SelectedRows**        | ObservableCollection\<IGridRow\> | Two-way, multi-selection    |
| **HeaderActions**       | IList\<IGridHeaderAction\>       | Toolbar buttons             |
| **GroupRowActions**     | IList\<IGridRowAction\>          | Actions menu for group rows |
| **ItemRowActions**      | IList\<IGridRowAction\>          | Actions menu for item rows  |
| **SingleExpandMode**    | bool                             | DP, default false           |
| **ShowSelectionColumn** | bool                             | DP, default false           |
| **IsReadOnly**          | bool                             | Global edit lock            |

**12. Commands (Inbound)**

Bindable ICommand DPs — other controls can drive the grid.

|                          |               |                                                          |
|--------------------------|---------------|----------------------------------------------------------|
| **Command**              | **Parameter** | **Behavior**                                             |
| **ScrollToRowCommand**   | IGridRow      | Scrolls viewport to row                                  |
| **SelectRowCommand**     | IGridRow      | Selects row. No-op if disabled.                          |
| **ExpandGroupCommand**   | GridGroupRow  | Expands group. No-op if disabled.                        |
| **CollapseGroupCommand** | GridGroupRow  | Collapses group. No-op if disabled.                      |
| **CollapseAllCommand**   | None          | Collapses all groups                                     |
| **ExpandAllCommand**     | None          | Expands all groups. Disabled when SingleExpandMode=true. |
| **BeginEditRowCommand**  | IGridRow      | Begins edit. No-op if disabled.                          |

**13. Routed Events (Outbound)**

All events use RoutingStrategy.Bubble. Other controls subscribe anywhere
in the visual tree.

|                         |                                                                       |
|-------------------------|-----------------------------------------------------------------------|
| **Event**               | **EventArgs Key Properties**                                          |
| **SelectedRowChanged**  | OldRow: IGridRow, NewRow: IGridRow                                    |
| **SelectedRowsChanged** | AddedRows: IList\<IGridRow\>, RemovedRows: IList\<IGridRow\>          |
| **GroupExpanded**       | Group: GridGroupRow, GroupIndex: int                                  |
| **GroupCollapsed**      | Group: GridGroupRow, GroupIndex: int                                  |
| **CellEditCommitted**   | Row: IGridRow, ColumnName: string, OldValue: object, NewValue: object |
| **CellEditCancelled**   | Row: IGridRow, ColumnName: string                                     |

> *RowAdded and RowDeleted are not routed events. Consumers observe
> IGridDataSource.GroupChanged and ItemChanged directly.*

**14. CellEditCommitted Value Types**

|                       |                   |                   |
|-----------------------|-------------------|-------------------|
| **Column Type**       | **OldValue Type** | **NewValue Type** |
| **TextColumn**        | string            | string            |
| **ComboBoxColumn**    | string (display)  | string (display)  |
| **CheckBoxColumn**    | bool              | bool              |
| **ButtonColumn**      | N/A (no edit)     | N/A (no edit)     |
| **ActionsMenuColumn** | N/A (no edit)     | N/A (no edit)     |
