using Basics.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Basics.Forms
{
  public class FilterHeaderCell : DataGridViewColumnHeaderCell
  {
    /// <summary>
    /// The ListBox used for all drop-down lists. 
    /// </summary>
    private static readonly FilterListBox _filterList = new FilterListBox();
    private static readonly FilterTextBox _filterTextBox = new FilterTextBox();
    private static readonly FilterDateBox _filterDateBox = new FilterDateBox();
    private static readonly FilterCheckBox _filterCheckBox = new FilterCheckBox();

    private static readonly CntCustomFilter _filterCustom = new CntCustomFilter();

    private string _filterText;
    private string _filterStatement;

    private object _filterClearValue;

    public static FilterHeaderCell CreateCore(DataGridViewColumn column)
    {
      FilterHeaderCell cell = new FilterHeaderCell(column.HeaderCell);
      column.HeaderCell = cell;
      return cell;
    }
    /// <summary>
    /// Initializes a new instance of the DataGridViewColumnHeaderCell 
    /// class and sets its property values to the property values of the 
    /// specified DataGridViewColumnHeaderCell.
    /// </summary>
    /// <param name="oldHeaderCell">The DataGridViewColumnHeaderCell to copy property values from.</param>
    public FilterHeaderCell(DataGridViewColumnHeaderCell oldHeaderCell)
    {
      base.ContextMenuStrip = oldHeaderCell.ContextMenuStrip;
      ErrorText = oldHeaderCell.ErrorText;
      Tag = oldHeaderCell.Tag;
      ToolTipText = oldHeaderCell.ToolTipText;
      Value = oldHeaderCell.Value;
      base.ValueType = oldHeaderCell.ValueType;

      // Use HasStyle to avoid creating a new style object
      // when the Style property has not previously been set. 
      if (oldHeaderCell.HasStyle)
      {
        Style = oldHeaderCell.Style;
      }

      // Copy this type's properties if the old cell is an auto-filter cell. 
      // This enables the Clone method to reuse this constructor. 
      if (oldHeaderCell is FilterHeaderCell filterCell)
      {
        FilteringEnabled = filterCell.FilteringEnabled;
        AutomaticSortingEnabled = filterCell.AutomaticSortingEnabled;
        DropDownListBoxMaxLines = filterCell.DropDownListBoxMaxLines;
        _currentDropDownButtonPaddingOffset =
            filterCell._currentDropDownButtonPaddingOffset;
      }
    }

    /// <summary>
    /// Initializes a new instance of the DataGridViewColumnHeaderCell 
    /// class. 
    /// </summary>
    public FilterHeaderCell()
    {
    }

    /// <summary>
    /// Creates an exact copy of this cell.
    /// </summary>
    /// <returns>An object that represents the cloned DataGridViewAutoFilterColumnHeaderCell.</returns>
    public override object Clone()
    {
      FilterHeaderCell clone = (FilterHeaderCell)base.Clone();

      clone.FilteringEnabled = FilteringEnabled;
      clone.AutomaticSortingEnabled = AutomaticSortingEnabled;
      clone.DropDownListBoxMaxLines = DropDownListBoxMaxLines;
      clone._currentDropDownButtonPaddingOffset = _currentDropDownButtonPaddingOffset;
      clone.ConvertSelectedValue = ConvertSelectedValue;

      clone._filterText = _filterText;
      clone._filterClearValue = _filterClearValue;
      clone.CaseSensitive = CaseSensitive;

      return clone;
    }

    public Func<object, string> ConvertSelectedValue;

    public object FilterClearValue
    {
      get { return _filterClearValue; }
      set { _filterClearValue = value; }
    }

    public bool CaseSensitive { get; set; }
    protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
    {
      Size size = base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);
      if (FilteringEnabled)
      { size.Height += _filterTextBox.Height + 3; }
      return size;
    }
    /// <summary>
    /// Called when the value of the DataGridView property changes
    /// in order to perform initialization that requires access to the 
    /// owning control and column. 
    /// </summary>
    protected override void OnDataGridViewChanged()
    {
      // Continue only if there is a DataGridView. 
      if (DataGridView == null)
      {
        return;
      }

      // Confirm that the data source meets requirements. 
      VerifyDataSource();

      // Add handlers to DataGridView events. 
      HandleDataGridViewEvents();

      // Call the OnDataGridViewChanged method on the base class to 
      // raise the DataGridViewChanged event.
      base.OnDataGridViewChanged();
    }

    /// <summary>
    /// Confirms that the data source, if it has been set, is a BindingSource.
    /// </summary>
    private void VerifyDataSource()
    {
      // Continue only if there is a DataGridView and its DataSource has been set.
      if (IsDataSourceNull()) { return; }

      // Throw an exception if the data source is not a BindingSource. 
      IBindingListView data = DataGridView.DataSource as IBindingListView;
      if (data == null)
      {
        throw new NotSupportedException(
            "The DataSource property of the containing DataGridView control " +
            "must be set to a BindingSource.");
      }

      if (!CheckAllowFiltering(data))
      {
        AutomaticSortingEnabled = false;
        FilteringEnabled = false;
      }
      if (OwningColumn != null)
      {
        // Ensure that the column SortMode property value is not Automatic.
        // This prevents sorting when the user clicks the drop-down button.
        if (OwningColumn.SortMode == DataGridViewColumnSortMode.Automatic)
        {
          OwningColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
        }
      }
    }

    private bool CheckAllowFiltering(IBindingListView data)
    {
      // Disable sorting and filtering for columns that can't make
      // effective use of them. 
      if (OwningColumn == null)
      { return true; }

      if (OwningColumn is DataGridViewImageColumn ||
          (OwningColumn is DataGridViewButtonColumn &&
          ((DataGridViewButtonColumn)OwningColumn).UseColumnTextForButtonValue) ||
          (OwningColumn is DataGridViewLinkColumn &&
          ((DataGridViewLinkColumn)OwningColumn).UseColumnTextForLinkValue))
      { }
      else
      { return true; }

      Type elemType = ListViewUtils.GetElementType(data);
      if (elemType == null)
      { return false; }
      PropertyInfo prop = elemType.GetProperty(OwningColumn.DataPropertyName);
      if (prop == null)
      { return false; }

      if (!typeof(string).IsAssignableFrom(prop.PropertyType))
      { return false; }

      return true;
    }

    private bool IsDataSourceNull()
    {
      bool isNull = DataGridView == null ||
        DataGridView.DataSource == null || DataGridView.DataSource == DBNull.Value;
      return isNull;
    }

    #region DataGridView events: HandleDataGridViewEvents, DataGridView event handlers, ResetDropDown, ResetFilter

    /// <summary>
    /// Add handlers to various DataGridView events, primarily to invalidate 
    /// the drop-down button bounds, hide the drop-down list, and reset 
    /// cached filter values when changes in the DataGridView require it.
    /// </summary>
    private void HandleDataGridViewEvents()
    {
      DataGridView.Scroll += DataGridView_Scroll;
      DataGridView.ColumnDisplayIndexChanged += DataGridView_ColumnDisplayIndexChanged;
      DataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
      DataGridView.ColumnHeadersHeightChanged += DataGridView_ColumnHeadersHeightChanged;
      DataGridView.SizeChanged += DataGridView_SizeChanged;
      DataGridView.DataSourceChanged += DataGridView_DataSourceChanged;
      DataGridView.DataBindingComplete += DataGridView_DataBindingComplete;

      // Add a handler for the ColumnSortModeChanged event to prevent the
      // column SortMode property from being inadvertently set to Automatic.
      DataGridView.ColumnSortModeChanged += DataGridView_ColumnSortModeChanged;

      DataGridView.CellToolTipTextNeeded += DataGridView_CellToolTipTextNeeded;
    }

    private void DataGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
      if (e.ColumnIndex != OwningColumn.Index || e.RowIndex != -1)
      { return; }

      e.ToolTipText = _filterStatement;
      //Point pos = Control.MousePosition;
      //Point loc = DataGridView.PointToClient(pos);
      //Rectangle rect = DataGridView.GetCellDisplayRectangle(OwningColumn.Index, -1, false);

      //if (rect.Contains(loc))
      //{
      //  Rectangle filterRect = GetFilterBounds(rect);
      //  if (filterRect.Contains(loc))
      //  {
      //  }
      //}
    }

    /// <summary>
    /// Invalidates the drop-down button bounds when the user scrolls horizontally.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A ScrollEventArgs that contains the event data.</param>
    private void DataGridView_Scroll(object sender, ScrollEventArgs e)
    {
      if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
      {
        ResetFilterControl();
      }
    }

    /// <summary>
    /// Invalidates the drop-down button bounds when the column display index changes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGridView_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
    {
      ResetFilterControl();
    }

    /// <summary>
    /// Invalidates the drop-down button bounds when a column width changes
    /// in the DataGridView control. A width change in any column of the 
    /// control has the potential to affect the drop-down button location, 
    /// depending on the current horizontal scrolling position and whether
    /// the changed column is to the left or right of the current column. 
    /// It is easier to invalidate the button in all cases. 
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A DataGridViewColumnEventArgs that contains the event data.</param>
    private void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
    {
      ResetFilterControl();
    }

    /// <summary>
    /// Invalidates the drop-down button bounds when the height of the column headers changes.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    private void DataGridView_ColumnHeadersHeightChanged(object sender, EventArgs e)
    {
      ResetFilterControl();
    }

    /// <summary>
    /// Invalidates the drop-down button bounds when the size of the DataGridView changes.
    /// This prevents a painting issue that occurs when the right edge of the control moves 
    /// to the right and the control contents have previously been scrolled to the right.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    private void DataGridView_SizeChanged(object sender, EventArgs e)
    {
      ResetFilterControl();
    }

    /// <summary>
    /// Invalidates the drop-down button bounds, hides the drop-down 
    /// filter list, if it is showing, and resets the cached filter values
    /// if the filter has been removed. 
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A DataGridViewBindingCompleteEventArgs that contains the event data.</param>
    private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
      if (e.ListChangedType == ListChangedType.Reset)
      {
        ResetFilterControl();
        ResetFilter();
      }
    }

    /// <summary>
    /// Verifies that the data source meets requirements, invalidates the 
    /// drop-down button bounds, hides the drop-down filter list if it is 
    /// showing, and resets the cached filter values if the filter has been removed. 
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    private void DataGridView_DataSourceChanged(object sender, EventArgs e)
    {
      VerifyDataSource();
      ResetFilterControl();
      ResetFilter();
    }

    private void ResetFilterControl()
    {
      ResetFilterControl(_filterTextBox);
      ResetFilterControl(_filterDateBox);
      ResetFilterControl(_filterList);
      ResetFilterControl(_filterCheckBox);
    }
    /// <summary>
    /// Invalidates the drop-down button bounds and hides the filter
    /// list if it is showing.
    /// </summary>
    private void ResetFilterControl(Control control)
    {
      if (_filterEditShowing)
      {
        HideFilterControl(control);
      }
    }

    /// <summary>
    /// Resets the cached filter values if the filter has been removed.
    /// </summary>
    private void ResetFilter()
    {
      if (DataGridView == null) return;
      IBindingListView source = DataGridView.DataSource as IBindingListView;
      if (source == null || String.IsNullOrEmpty(source.Filter))
      {
      }
    }

    /// <summary>
    /// Throws an exception when the column sort mode is changed to Automatic.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A DataGridViewColumnEventArgs that contains the event data.</param>
    private void DataGridView_ColumnSortModeChanged(object sender, DataGridViewColumnEventArgs e)
    {
      if (e.Column == OwningColumn &&
          e.Column.SortMode == DataGridViewColumnSortMode.Automatic)
      {
        throw new InvalidOperationException(
            "A SortMode value of Automatic is incompatible with " +
            "the DataGridViewAutoFilterColumnHeaderCell type. " +
            "Use the AutomaticSortingEnabled property instead.");
      }
    }

    #endregion DataGridView events

    protected override void Paint(
        Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
        int rowIndex, DataGridViewElementStates cellState,
        object value, object formattedValue, string errorText,
        DataGridViewCellStyle cellStyle,
        DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
      Rectangle rect;
      bool enabled = FilteringEnabled;
      if (enabled)
      {
        rect = GetFilterBounds(cellBounds);
        cellStyle.Padding = new Padding(0, 0, 0, rect.Height);
      }
      else
      {
        rect = new Rectangle();
        cellStyle.Padding = new Padding(0);
      }

      base.Paint(graphics, clipBounds, cellBounds, rowIndex,
          cellState, value, formattedValue,
          errorText, cellStyle, advancedBorderStyle, paintParts);

      if (!enabled ||
          (paintParts & DataGridViewPaintParts.ContentBackground) == 0)
      {
        return;
      }

      DrawFilter(graphics, rect);
    }

    protected virtual void DrawFilter(Graphics graphics, Rectangle rect)
    {
      if (!(OwningColumn is DataGridViewCheckBoxColumn))
      {
        DrawText(graphics, rect, _filterText);
      }
      else
      {
        DrawText(graphics, rect, null);
        if (!string.IsNullOrEmpty(_filterText))
        {
          Extensions.TryParse(_filterText, out CheckState state);

          CheckBoxState boxState;
          if (state == CheckState.Checked)
          { boxState = CheckBoxState.CheckedNormal; }
          else if (state == CheckState.Unchecked)
          { boxState = CheckBoxState.UncheckedNormal; }
          else if (state == CheckState.Indeterminate)
          { boxState = CheckBoxState.MixedNormal; }
          else
          { throw new NotImplementedException("Unhandled CheckState " + state); }
          Size size = CheckBoxRenderer.GetGlyphSize(graphics, boxState);
          Point loc = new Point(rect.X + rect.Width / 2 - size.Width / 2,
            rect.Y + rect.Height / 2 - size.Height / 2);
          CheckBoxRenderer.DrawCheckBox(graphics, loc, boxState);
        }
      }
    }
    protected void DrawText(Graphics graphics, Rectangle rect, string text, Brush brush = null)
    {
      //if (TextBoxRenderer.IsSupported)
      //{
      //    if (text != null)
      //    { text = text.Replace("&", "&&"); }
      //    TextBoxRenderer.DrawTextBox(graphics, rect, text, DataGridView.Font, TextBoxState.Normal);
      //}
      //else
      {
        graphics.FillRectangle(Brushes.White, rect);
        graphics.DrawRectangle(Pens.Gainsboro, rect);
        if (!string.IsNullOrWhiteSpace(text))
        {
          brush = brush ?? Brushes.Black;

          int margin = 2;
          Rectangle reduced = new Rectangle(rect.Left + margin, rect.Top + margin,
              rect.Width - margin, rect.Height - margin);
          graphics.DrawString(text, DataGridView.Font, brush, reduced);
        }
        //ControlPaint.DrawStringDisabled(graphics,_filterText, DataGridView.Font, 
        //  System.Drawing.Color.Black, rect, TextFormatFlags.Left ); 
      }
    }

    protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
    {
      // Continue only if the user did not click the drop-down button 
      // while the drop-down list was displayed. This prevents the 
      // drop-down list from being redisplayed after being hidden in 
      // the LostFocus event handler. 
      if (_filterEditShowing)
      {
        return;
      }
      if (Cursor.Current != Cursors.Default)
      {
        return;
      }

      Rectangle rect = GetFilterBounds(Rectangle.Empty);
      if (FilteringEnabled && rect.Contains(rect.X + e.X, e.Y))
      {
        if (e.Button == MouseButtons.Right && _filterCustom.Parent == null)
        {
          _filterCustom.InitColumn(OwningColumn, GetFilterValues());
          rect.Height = _filterCustom.Height;
          ShowFilterControl(_filterCustom, rect, 40);
        }
        else
        { InitFilter(rect); }
      }
      else
      {
        SortByColumn();
      }
      base.OnMouseDown(e);
    }

    private IList<object> GetFilterValues(int max = 100)
    {
      List<object> filterValues = new List<object>();
      if (OwningColumn is DataGridViewComboBoxColumn col)
      {
        if (col.DataSource is IEnumerable values)
        {
          Func<object, string> getDisplayValue = null;

          foreach (object value in values)
          {
            if (getDisplayValue == null)
            {
              if (col.DisplayMember != null)
              {
                MethodBase method = value.GetType().GetProperty(col.DisplayMember).GetGetMethod();
                getDisplayValue = (o) => $"{method.Invoke(o, new object[] { })}";
              }
              else
              {
                getDisplayValue = (o) => $"{o}";
              }
            }
            string name = getDisplayValue(value);
            if (!string.IsNullOrEmpty(name))
            {
              filterValues.Add(name);
            }
          }
        }
      }
      if (filterValues.Count == 0)
      {
        if (DataGridView.DataSource is IPartialListView partialList)
        {
          SortedDictionary<string, object> dict = new SortedDictionary<string, object>();
          foreach (object value in partialList.GetDistinctValues(OwningColumn.DataPropertyName, max))
          {
            string key = $"{value}";
            dict[key] = value;
          }
          foreach (var pair in dict)
          {
            filterValues.Add(pair.Value);
          }
        }
      }
      if (filterValues.Count == 0)
      {
        if (DataGridView.DataSource is IBindingListView sortList)
        {
          Func<object, string> getPropertyValue = null;
          SortedDictionary<string, object> dict = new SortedDictionary<string, object>();
          for (int i = 0; i < sortList.Count; i++)
          {
            object value = sortList[i];
            if (getPropertyValue == null)
            {
              MethodBase method = value.GetType().GetProperty(OwningColumn.DataPropertyName).GetGetMethod();
              getPropertyValue = (o) => $"{method.Invoke(o, new object[] { })}";
            }
            object propVal = getPropertyValue(value);
            string key = $"{propVal}";
            dict[key] = propVal;

            if (dict.Count >= max)
            { break; }
          }

          foreach (var pair in dict)
          {
            filterValues.Add(pair.Value);
          }
        }
      }

      return filterValues;
    }
    private void InitFilter(Rectangle rect)
    {
      if (OwningColumn is DataGridViewTextBoxColumn)
      {
        Type propertyType = GetPropertyType(OwningColumn);

        if (propertyType == typeof(DateTime))
        {
          _filterDateBox.Location = rect.Location;
          _filterDateBox.Text = _filterText;
          _filterDateBox.OwningCell = this;
          ShowFilterControl(_filterDateBox, rect);
        }
        else
        {
          _filterTextBox.Location = rect.Location;
          _filterTextBox.Text = _filterText;
          _filterTextBox.CaseSensitive = CaseSensitive;

          ShowFilterControl(_filterTextBox, rect);
        }
      }
      else if (OwningColumn is DataGridViewComboBoxColumn col)
      {
        _filterList.Init(col, _filterClearValue);
        _filterList.Text = _filterText;
        _filterList.CaseSensitive = CaseSensitive;

        ShowFilterControl(_filterList, rect);
      }
      else if (OwningColumn is DataGridViewCheckBoxColumn)
      {
        Extensions.TryParse(_filterText, out CheckState state);
        _filterCheckBox.CheckState = state;

        ShowFilterControl(_filterCheckBox, rect);
      }
      else if (typeof(string).IsAssignableFrom(GetPropertyType(OwningColumn)))
      {
        _filterTextBox.Location = rect.Location;
        _filterTextBox.Text = _filterText;
        _filterTextBox.CaseSensitive = CaseSensitive;

        ShowFilterControl(_filterTextBox, rect);
      }
    }

    /// <summary>
    /// Sorts the DataGridView by the current column if AutomaticSortingEnabled is true.
    /// </summary>
    private void SortByColumn()
    {
      // Continue only if the data source supports sorting. 
      IBindingListView sortList = DataGridView.DataSource as IBindingListView;
      if (sortList == null ||
          !sortList.SupportsSorting ||
          !AutomaticSortingEnabled)
      {
        return;
      }
      ListSortDescriptionCollection sorts =
        ListViewUtils.AdaptSort(sortList, OwningColumn.DataPropertyName,
        Control.ModifierKeys == Keys.Shift);

      foreach (DataGridViewColumn column in DataGridView.Columns)
      {
        FilterHeaderCell cell = column.HeaderCell as FilterHeaderCell;
        if (cell == null)
        { continue; }

        cell.SortGlyphDirection = SortOrder.None;
        foreach (ListSortDescription sort in sorts)
        {
          if (sort.PropertyDescriptor.Name == cell.OwningColumn.DataPropertyName)
          {
            cell.OwningColumn.SortMode = DataGridViewColumnSortMode.Programmatic;

            if (sort.SortDirection == ListSortDirection.Ascending)
            { cell.SortGlyphDirection = SortOrder.Ascending; }
            else if (sort.SortDirection == ListSortDirection.Descending)
            { cell.SortGlyphDirection = SortOrder.Descending; }
            break;
          }
        }
      }
    }

    #region drop-down list: Show/HideDropDownListBox, SetDropDownListBoxBounds, DropDownListBoxMaxHeightInternal

    /// <summary>
    /// Indicates whether dropDownListBox is currently displayed 
    /// for this header cell. 
    /// </summary>
    private bool _filterEditShowing;

    private void ShowFilterControl<T>(T filter, Rectangle rect, int addWidth = 0) where T : Control, IFilterControl
    {
      if (DataGridView == null) { return; }

      if (DataGridView.CurrentRow != null &&
          DataGridView.CurrentRow.IsNewRow)
      {
        DataGridView.CurrentCell = null;
      }

      filter.Location = rect.Location;
      filter.Width = rect.Width + addWidth;
      filter.Height = rect.Height;

      WireEvents(filter);

      filter.Visible = true;
      _filterEditShowing = true;

      if (filter.Parent != null)
      {
        throw new InvalidOperationException(string.Format(
          "ShowEditFilter has been called multiple times for {0} before HideDropDownListBox",
          filter.GetType()));
      }
      DataGridView.Controls.Add(filter);

      filter.UseWaitCursor = false;
      filter.Cursor = Cursors.Default;
      filter.Focus();
      DataGridView.InvalidateCell(this);
    }

    /// <summary>
    /// Hides the drop-down filter list. 
    /// </summary>
    public void HideFilterControl(Control control)
    {
      // Hide filter control, remove handlers from its events, and remove 
      // it from the DataGridView control. 
      _filterEditShowing = false;
      control.Visible = false;
      UnwireEvents(control);
      DataGridView.Controls.Remove(control);

      // Invalidate the cell so that the drop-down button will repaint
      // in the unpressed state. 
      DataGridView.InvalidateCell(this);
    }
    #endregion drop-down list

    #region FilterControl events

    /// <summary>
    /// Adds handlers to ListBox events for handling mouse
    /// and keyboard input.
    /// </summary>
    private void WireEvents(Control control)
    {
      control.LostFocus += FilterControl_LostFocus;
      control.Validated += FilterControl_Validated;
      control.KeyDown += FilterControl_KeyDown;
    }
    private void UnwireEvents(Control control)
    {
      control.LostFocus -= FilterControl_LostFocus;
      control.Validated -= FilterControl_Validated;
      control.KeyDown -= FilterControl_KeyDown;
    }

    void FilterControl_Validated(object sender, EventArgs e)
    {
      Control control = (Control)sender;
      if (_filterEditShowing)
      {
        UpdateFilter((IFilterControl)control);
      }
      HideFilterControl(control);
    }

    /// <summary>
    /// Hides the drop-down list when it loses focus. 
    /// </summary>
    private void FilterControl_LostFocus(object sender, EventArgs e)
    {
      IFilterControl filter = (IFilterControl)sender;
      if (!filter.Focused)
      {
        HideFilterControl((Control)sender);
      }
    }

    /// <summary>
    /// Handles the ENTER and ESC keys.
    /// </summary>
    private void FilterControl_KeyDown(object sender, KeyEventArgs e)
    {
      Control control = (Control)sender;
      switch (e.KeyCode)
      {
        case Keys.Enter:
          UpdateFilter((IFilterControl)control);
          HideFilterControl(control);
          break;
        case Keys.Escape:
          HideFilterControl(control);
          break;
      }
    }

    #endregion ListBox events

    #region filtering

    internal string GetFilterStatement()
    {
      return _filterStatement;
    }
    internal string GetFilterText()
    {
      return _filterText;
    }

    public void SetFilterStatement(string filterText, bool updateList)
    {
      _filterText = filterText;
      _filterStatement = filterText;
      if (updateList)
      {
        FilterData();
      }
    }
    private void UpdateFilter(IFilterControl filterControl)
    {
      if (!filterControl.ApplyFilter)
      { return; }

      string filterStatement = filterControl.GetFilterStatement(OwningColumn);

      string origText = _filterText;
      string origStatement = _filterStatement;
      try
      {
        _filterText = filterControl.FilterText;
        // Continue only if the selection has changed.
        if (filterStatement == _filterStatement)
        {
          return;
        }

        _filterStatement = filterStatement;

        // Cast the data source to an IBindingListView.
        FilterData();
      }
      catch
      {
        _filterStatement = origStatement;
        _filterText = origText;
        DataGridView.InvalidateCell(this);
        throw;
      }
    }

    private void FilterData()
    {
      IBindingListView data = DataGridView.DataSource as IBindingListView;

      if (data == null) throw new ArgumentNullException(nameof(data), 
        "DataSource is not an IBindingListView or does not support filtering");

      StringBuilder filterBuilder = new StringBuilder();
      foreach (DataGridViewColumn column in DataGridView.Columns)
      {
        FilterHeaderCell cell = column.HeaderCell as FilterHeaderCell;
        if (cell == null)
        { continue; }

        string colFilter = cell.GetFilterStatement();
        if (string.IsNullOrEmpty(colFilter))
        { continue; }

        if (filterBuilder.Length > 0)
        { filterBuilder.Append(" AND "); }

        filterBuilder.AppendFormat("({0})", colFilter);
      }

      data.Filter = filterBuilder.ToString();
      DataGridView.Invalidate();
    }

    /// <summary>
    /// Removes the filter from the BindingSource bound to the specified DataGridView. 
    /// </summary>
    /// <param name="dataGridView">The DataGridView bound to the BindingSource to unfilter.</param>
    public static void RemoveFilter(DataGridView dataGridView)
    {
      if (dataGridView == null)
      {
        throw new ArgumentNullException("dataGridView");
      }

      // Cast the data source to a BindingSource.
      IBindingListView data = dataGridView.DataSource as IBindingListView;

      // Confirm that the data source is a BindingSource that 
      // supports filtering.
      if (data == null ||
          //data.DataSource == null ||
          !data.SupportsFiltering)
      {
        throw new ArgumentException("The DataSource property of the " +
            "specified DataGridView is not set to a BindingSource " +
            "with a SupportsFiltering property value of true.");
      }

      // Ensure that the current row is not the row for new records.
      // This prevents the new row from being added when the filter changes.
      if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.IsNewRow)
      {
        dataGridView.CurrentCell = null;
      }

      // Remove the filter. 
      data.Filter = null;

      foreach (DataGridViewColumn column in dataGridView.Columns)
      {
        FilterHeaderCell cell = column.HeaderCell as FilterHeaderCell;
        if (cell == null)
        { continue; }

        cell._filterStatement = null;
        cell._filterText = null;
      }
    }

    #endregion filtering

    #region button bounds: DropDownButtonBounds, InvalidateDropDownButtonBounds, SetDropDownButtonBounds, AdjustPadding


    private Rectangle GetFilterBounds(Rectangle cellBounds)
    {
      // Retrieve the cell display rectangle, which is used to 
      // set the position of the drop-down button. 
      if (cellBounds.IsEmpty)
      {
        cellBounds = DataGridView.GetCellDisplayRectangle(
            ColumnIndex, -1, false);
      }
      Rectangle filterBounds = new Rectangle(cellBounds.X - 1, cellBounds.Y + cellBounds.Height - _filterTextBox.Height - 2,
             cellBounds.Width + 1, _filterTextBox.Height);
      return filterBounds;
    }


    /// <summary>
    /// Adjusts the cell padding to widen the header by the drop-down button width.
    /// </summary>
    /// <param name="newDropDownButtonPaddingOffset">The new drop-down button width.</param>
    private void AdjustPadding(Int32 newDropDownButtonPaddingOffset)
    {
      // Determine the difference between the new and current 
      // padding adjustment.
      Int32 widthChange = newDropDownButtonPaddingOffset -
          _currentDropDownButtonPaddingOffset;

      // If the padding needs to change, store the new value and 
      // make the change.
      if (widthChange != 0)
      {
        // Store the offset for the drop-down button separately from 
        // the padding in case the client needs additional padding.
        _currentDropDownButtonPaddingOffset =
            newDropDownButtonPaddingOffset;

        // Create a new Padding using the adjustment amount, then add it
        // to the cell's existing Style.Padding property value. 
        Padding dropDownPadding = new Padding(0, 0, widthChange, 0);
        Style.Padding = Padding.Add(
            InheritedStyle.Padding, dropDownPadding);
      }
    }

    /// <summary>
    /// The current width of the drop-down button. This field is used to adjust the cell padding.  
    /// </summary>
    private Int32 _currentDropDownButtonPaddingOffset;

    #endregion button bounds

    #region public properties: FilteringEnabled, AutomaticSortingEnabled, DropDownListBoxMaxLines

    /// <summary>
    /// Indicates whether filtering is enabled for the owning column. 
    /// </summary>
    private Boolean _filteringEnabledValue = true;

    /// <summary>
    /// Gets or sets a value indicating whether filtering is enabled.
    /// </summary>
    [DefaultValue(true)]
    public Boolean FilteringEnabled
    {
      get
      {
        // Return filteringEnabledValue if (there is no DataGridView
        // or if (its DataSource property has not been set. 
        if (IsDataSourceNull())
        {
          return _filteringEnabledValue;
        }

        // if (the DataSource property has been set, return a value that combines 
        // the filteringEnabledValue and BindingSource.SupportsFiltering values.
        IBindingListView data = DataGridView.DataSource as IBindingListView;
        if (data == null) throw new ArgumentNullException(nameof(data));
        return _filteringEnabledValue && data.SupportsFiltering;
      }
      set
      {
        // If filtering is disabled, remove the padding adjustment
        // and invalidate the button bounds. 
        if (!value)
        {
          AdjustPadding(0);
        }

        _filteringEnabledValue = value;
      }
    }

    /// <summary>
    /// Indicates whether automatic sorting is enabled. 
    /// </summary>
    private Boolean _automaticSortingEnabledValue = true;

    /// <summary>
    /// Gets or sets a value indicating whether automatic sorting is enabled for the owning column. 
    /// </summary>
    [DefaultValue(true)]
    public Boolean AutomaticSortingEnabled
    {
      get
      {
        return _automaticSortingEnabledValue;
      }
      set
      {
        _automaticSortingEnabledValue = value;
        if (OwningColumn != null)
        {
          if (value)
          {
            OwningColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
          }
          else
          {
            OwningColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
          }
        }
      }
    }

    /// <summary>
    /// The maximum number of lines in the drop-down list. 
    /// </summary>
    private Int32 _dropDownListBoxMaxLinesValue = 20;

    /// <summary>
    /// Gets or sets the maximum number of lines to display in the drop-down filter list. 
    /// The actual height of the drop-down list is constrained by the DataGridView height. 
    /// </summary>
    [DefaultValue(20)]
    public Int32 DropDownListBoxMaxLines
    {
      get { return _dropDownListBoxMaxLinesValue; }
      set { _dropDownListBoxMaxLinesValue = value; }
    }

    #endregion public properties

    public static Type GetPropertyType(DataGridViewColumn column)
    {
      Type propertyType = column.ValueType ?? 
        ListViewUtils.GetPropertyType(column.DataGridView?.DataSource, column.DataPropertyName);
      return propertyType;
    }

    public interface IFilterControl
    {
      string GetFilterStatement(DataGridViewColumn column);
      string FilterText { get; }
      bool Focused { get; }
      bool ApplyFilter { get; }
    }

    private interface ITextFilterControl : IFilterControl
    {
      string Text { get; set; }
      bool CaseSensitive { get; }
    }

    private static void GetFilterText(string text, out string filter, out string filterOp)
    {
      string filterText = text;
      filterOp = null;
      if (string.IsNullOrEmpty(filterText))
      {
        filter = null;
        return;
      }
      filterText = filterText.Trim();
      if (string.IsNullOrEmpty(filterText))
      {
        filter = null;
        return;
      }

      filterOp = "";

      char op = filterText[0];
      if (op == '\\')
      {
        filter = filterText.Substring(1);
        filterOp = "\\";
        return;
      }

      while (op == '=' || op == '>' || op == '<')
      {
        filterOp += op;
        filterText = filterText.Substring(1);
        if (filterText.Length > 0)
        { op = filterText[0]; }
        else
        { op = ' '; }
      }

      filter = filterText;
    }

    private static string GetTextFilterStatement(ITextFilterControl control, string text, DataGridViewColumn column)
    {
      string filter = ListViewUtils.GetTextFilterStatement(text, column.DataPropertyName,
        column.DataGridView.DataSource, control.CaseSensitive);
      if (string.IsNullOrWhiteSpace(filter))
      { control.Text = ""; }
      return filter;
    }

    private class FilterDateBox : Panel, ITextFilterControl
    {
      private readonly DateTextBox _textBox;
      private readonly MonthCalendar _calendar;
      public FilterDateBox()
      {
        Visible = false;
        BorderStyle = BorderStyle.None;
        TabStop = false;

        _textBox = new DateTextBox
        {
          BorderStyle = BorderStyle.FixedSingle,
          TabStop = false
        };
        _textBox.LostFocus += _textBox_LostFocus;
        _textBox.PreviewKeyDown += _textBox_PreviewKeyDown;
        _textBox.TextChanged += _textBox_TextChanged;
        _textBox.KeyDown += _textBox_KeyDown;
        Controls.Add(_textBox);

        _calendar = new MonthCalendar();
        _calendar.LostFocus += _calendar_LostFocus;
        _calendar.DateChanged += _calendar_DateSelected;
        _calendar.DateSelected += _calendar_DateSelected;
        _calendar.PreviewKeyDown += _calender_PreviewKeyDown;
        _calendar.KeyDown += _textBox_KeyDown;
        MouseDown += FilterDateBox_MouseDown;
        VisibleChanged += FilterDateBox_VisibleChanged;
        Resize += FilterDateBox_Resize;
      }

      bool IFilterControl.ApplyFilter { get { return true; } }
      bool ITextFilterControl.CaseSensitive { get { return true; } }
      public string FilterText { get { return _textBox.Text; } }

      public string GetFilterStatement(DataGridViewColumn column)
      {
        return GetTextFilterStatement(this, _textBox.Text, column);
      }

      private FilterHeaderCell _owningCell;

      public FilterHeaderCell OwningCell
      {
        get { return _owningCell; }
        set
        {
          _owningCell = value;
          _textBox.Text = _owningCell._filterText;
        }
      }

      void _calender_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
      {
        switch (e.KeyCode)
        {
          case Keys.Enter:
            OwningCell.UpdateFilter(this);
            OwningCell.HideFilterControl(this);
            break;
          case Keys.Escape:
            OwningCell.HideFilterControl(this);
            break;
        }
      }

      void _textBox_KeyDown(object sender, KeyEventArgs e)
      {
        switch (e.KeyCode)
        {
          case Keys.Enter:
            OwningCell.UpdateFilter(this);
            OwningCell.HideFilterControl(this);
            break;
          case Keys.Escape:
            OwningCell.HideFilterControl(this);
            break;
        }
      }

      void _textBox_TextChanged(object sender, EventArgs e)
      {
        GetFilterText(_textBox.Text, out string filter, out string filterOp);
        if (filterOp == "\\")
        {
          return;
        }
        if (DateTime.TryParse(filter, out DateTime date))
        {
          if (_calendar.SelectionStart != date)
          { _calendar.SelectionStart = date; }
        }
      }

      void _textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
      {
        e.IsInputKey = true;
      }

      private void _calendar_DateSelected(object sender, DateRangeEventArgs e)
      {
        GetFilterText(_textBox.Text, out string filter, out string filterOp);
        if (filterOp != "\\")
        {
          if (DateTime.TryParse(filter, out DateTime date))
          {
            if (_calendar.SelectionStart == date)
            { return; }
          }
        }
        _textBox.Text = _calendar.SelectionStart.ToShortDateString();
      }

      bool IFilterControl.Focused
      {
        get { return Focused || _textBox.Focused || _calendar.Focused; }
      }
      void _calendar_LostFocus(object sender, EventArgs e)
      {
        OnLostFocus(e);
      }
      protected override void OnValidated(EventArgs e)
      {
      }

      void _textBox_LostFocus(object sender, EventArgs e)
      {
        OnLostFocus(e);
      }

      void FilterDateBox_Resize(object sender, EventArgs e)
      {
        _textBox.Width = Width - _textBox.Height;
      }

      protected override void OnPaint(PaintEventArgs e)
      {
        base.OnPaint(e);
        Rectangle rect = e.ClipRectangle;
        int h = _textBox.Height;
        int l = rect.Right - h;
        int t = rect.Top;
        rect = new Rectangle(l, t, h, h);
        ButtonRenderer.DrawButton(e.Graphics, rect, System.Windows.Forms.VisualStyles.PushButtonState.Normal);
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        int m = h / 2;
        int w = h / 5;
        path.AddLine(l + m - w, t + m - w / 2, l + m + w, t + m - w / 2);
        path.AddLine(l + m + w, t + m - w / 2, l + m, t + m - w / 2 + w);
        path.CloseFigure();
        e.Graphics.FillPath(Brushes.Black, path);
      }

      private void FilterDateBox_VisibleChanged(object sender, EventArgs e)
      {
        if (!Visible)
        {
          _calendar.Hide();
        }
      }

      void FilterDateBox_MouseDown(object sender, MouseEventArgs e)
      {
        _calendar.Parent = Parent;
        _calendar.Location = new Point(Left, Bottom);
        _calendar.Show();
      }
    }

    private class FilterTextBox : TextBox, ITextFilterControl
    {
      public FilterTextBox()
      {
        Visible = false;
        BorderStyle = BorderStyle.FixedSingle;
        TabStop = false;
      }

      public bool CaseSensitive { get; set; }

      protected override bool IsInputKey(Keys keyData)
      {
        return true;
      }

      protected override bool ProcessKeyMessage(ref Message m)
      {
        return ProcessKeyEventArgs(ref m);
      }

      public string FilterText { get { return Text; } }

      public string GetFilterStatement(DataGridViewColumn column)
      {
        return GetTextFilterStatement(this, Text, column);
      }
      bool IFilterControl.ApplyFilter { get { return true; } }
    }

    private class FilterCheckBox : CheckBox, IFilterControl
    {
      private string _text;
      public FilterCheckBox()
      {
        Visible = false;
        TabStop = false;
        ThreeState = true;
        CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      }

      protected override bool IsInputKey(Keys keyData)
      {
        return true;
      }

      protected override bool ProcessKeyMessage(ref Message m)
      {
        return ProcessKeyEventArgs(ref m);
      }

      public string FilterText { get { return _text; } }
      bool IFilterControl.ApplyFilter { get { return true; } }

      public string GetFilterStatement(DataGridViewColumn column)
      {
        _text = null;
        if (CheckState == CheckState.Indeterminate)
        { return null; }

        string filter;
        Type propertyType = column.ValueType;
        if (propertyType.IsGenericType && typeof(Nullable<>) == propertyType.GetGenericTypeDefinition())
        {
          propertyType = propertyType.GetGenericArguments()[0];
        }

        if (propertyType == typeof(bool))
        {
          filter = string.Format("{0} = {1}", column.DataPropertyName, Checked);
          _text = CheckState.ToString();
        }
        else
        { //TODO
          filter = null;
        }
        return filter;
      }
    }

    private class FilterListBox : ComboBox, ITextFilterControl
    {
      private object _filterClearValue;
      private IEnumerable _dataSource;
      private Dictionary<string, string> _replaceDict;

      private MethodInfo _displayMethod = null;
      private MethodInfo _valueMethod = null;

      public FilterListBox()
      {
        Visible = false;
        IntegralHeight = true;
        TabStop = false;
      }

      public bool CaseSensitive { get; set; }

      protected override bool IsInputKey(Keys keyData)
      {
        return true;
      }

      protected override bool ProcessKeyMessage(ref Message m)
      {
        return ProcessKeyEventArgs(ref m);
      }

      public string FilterText
      {
        get { return Text; }
      }
      bool IFilterControl.ApplyFilter { get { return true; } }

      private void InitDataSource()
      {
        if (_dataSource != DataSource)
        {
          _replaceDict = null;
          _displayMethod = null;
          _valueMethod = null;

          _dataSource = DataSource as IEnumerable;
        }
      }
      private void GetValues(object o, out string display, out object value)
      {
        if (string.IsNullOrEmpty(DisplayMember))
        {
          display = $"{0}";
        }
        else
        {
          if (_displayMethod == null)
          { _displayMethod = o.GetType().GetProperty(DisplayMember).GetGetMethod(true); }

          display = $"{_displayMethod.Invoke(o, null)}";
        }
        if (string.IsNullOrEmpty(ValueMember))
        {
          if (string.IsNullOrEmpty(DisplayMember))
          { value = o; }
          else
          { value = display; }
        }
        else
        {
          if (_valueMethod == null)
          { _valueMethod = o.GetType().GetProperty(ValueMember).GetGetMethod(true); }

          value = _valueMethod.Invoke(o, null);
        }
      }

      private Dictionary<string, string> ReplaceDict
      {
        get
        {
          InitDataSource();

          if (_dataSource == null)
          {
            return null;
          }

          if (_replaceDict == null)
          {
            Dictionary<string, object> unsorted = new Dictionary<string, object>();
            foreach (object o in _dataSource)
            {
              GetValues(o, out string display, out object value);

              if (!unsorted.ContainsKey(display))
              {
                unsorted.Add(display, value);
              }
            }
            List<string> sorted = new List<string>(unsorted.Keys);
            sorted.Sort(SortLength);

            Dictionary<string, string> replaceDict = new Dictionary<string, string>();
            foreach (string s in sorted)
            {
              if (string.IsNullOrEmpty(s))
              { continue; }

              object value = unsorted[s];
              if (value == null)
              { continue; }

              if (value.GetType().IsEnum)
              { value = (int)value; }

              replaceDict.Add(s, value.ToString());
            }
            _replaceDict = replaceDict;
          }

          return _replaceDict;
        }
      }
      private int SortLength(string x, string y)
      {
        int d = x.Length.CompareTo(y.Length);
        if (d != 0)
        {
          return -d;
        }
        return x.CompareTo(y);
      }

      public string GetFilterStatement(DataGridViewColumn column)
      {
        if (SelectedItem == null || SelectedValue == null)
        {
          string text = Text;
          Dictionary<string, string> replace = ReplaceDict;
          if (replace != null)
          {
            foreach (var pair in replace)
            { text = text.Replace(pair.Key, pair.Value); }
          }
          return GetTextFilterStatement(this, text, column);
        }

        if (_filterClearValue != null &&
          SelectedItem == _filterClearValue)
        {
          return null;
        }

        object selectedValue = SelectedValue;
        FilterHeaderCell header = column.HeaderCell as FilterHeaderCell;
        if (header.ConvertSelectedValue != null)
        {
          selectedValue = header.ConvertSelectedValue(selectedValue);
        }
        else
        {
          GetValues(selectedValue, out string display, out selectedValue);
        }
        if (selectedValue.GetType().IsEnum)
        {
          selectedValue = (int)selectedValue;
        }

        if (typeof(string).IsInstanceOfType(selectedValue))
        {
          if (string.IsNullOrEmpty((string)selectedValue))
          { return string.Empty; }

          selectedValue = $"'{selectedValue}'";
        }
        else if (typeof(DateTime).IsInstanceOfType(selectedValue))
        {
          selectedValue = $"'{selectedValue}'";
        }

        string filter = string.Format("{0} = {1}", column.DataPropertyName, selectedValue);
        return filter;
      }

      public void Init(DataGridViewComboBoxColumn col, object filterClearValue)
      {
        _filterClearValue = null;

        object dataSource = col.DataSource;
        if (dataSource is IEnumerable values && filterClearValue != null)
        {
          Type listType = typeof(List<>).MakeGenericType(filterClearValue.GetType());
          object oList = Activator.CreateInstance(listType);
          IList list = (IList)oList;
          list.Add(filterClearValue);
          foreach (object value in values)
          { list.Add(value); }
          dataSource = list;
        }
        DataSource = dataSource;
        DisplayMember = col.DisplayMember;
        ValueMember = col.ValueMember;

        _filterClearValue = filterClearValue;
      }
    }

    private class DateTextBox : TextBox
    {
      protected override bool IsInputKey(Keys keyData)
      {
        return true;
      }

      protected override bool ProcessKeyMessage(ref Message m)
      {
        return ProcessKeyEventArgs(ref m);
      }
    }
  }
}
