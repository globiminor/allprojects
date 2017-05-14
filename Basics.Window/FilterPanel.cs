
using Basics.Views;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Basics.Window
{
  public class FilterPanel : StackPanel, INotifyPropertyChanged
  {
    private interface IFilterView
    {
      Visibility Visibility { get; set; }
      bool TryUpdateFilter(object source);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly TextBlock _lblHeader;
    private readonly Border _brdFilter;

    private IFilterView _filterView;

    private class ListFilter : IFilterView
    {
      private readonly ComboBox _lstFilter;
      public ListFilter(FilterPanel parent, Binding items)
      {
        _lstFilter = new ComboBox();
        _lstFilter.SetBinding(ComboBox.TextProperty,
          new Binding(nameof(parent.FilterText)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
        _lstFilter.DataContext = parent;
        _lstFilter.Visibility = Visibility.Collapsed;
        _lstFilter.Margin = new Thickness(0, 0, 0, 0);

        _lstFilter.SetBinding(ComboBox.ItemsSourceProperty, items);

        _lstFilter.IsEditable = true;
        _lstFilter.IsReadOnly = false;

        parent.Children.Add(_lstFilter);
      }

      public Visibility Visibility
      {
        get { return _lstFilter.Visibility; }
        set { _lstFilter.Visibility = value; }
      }
      bool IFilterView.TryUpdateFilter(object source)
      {
        ComboBox filter = source as ComboBox;
        if (filter == _lstFilter)
        {
          BindingExpression be = _lstFilter.GetBindingExpression(ComboBox.TextProperty);
          be.UpdateSource();
          return true;
        }
        return false;
      }

    }
    private class TextFilter : IFilterView
    {
      private readonly TextBox _txtFilter;
      public TextFilter(FilterPanel parent)
      {
        _txtFilter = new TextBox();
        _txtFilter.SetBinding(TextBox.TextProperty,
          new Binding(nameof(parent.FilterText)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
        _txtFilter.DataContext = parent;
        _txtFilter.Visibility = Visibility.Collapsed;
        _txtFilter.Margin = new Thickness(0, 0, 0, 0);
        parent.Children.Add(_txtFilter);
      }

      public Visibility Visibility
      {
        get { return _txtFilter.Visibility; }
        set { _txtFilter.Visibility = value; }
      }
      bool IFilterView.TryUpdateFilter(object source)
      {
        TextBox filter = source as TextBox;
        if (filter == _txtFilter)
        {
          BindingExpression be = _txtFilter.GetBindingExpression(TextBox.TextProperty);
          be.UpdateSource();
          return true;
        }
        return false;
      }
    }
    public static void CreateHeader(DataGridColumn col)
    {
      FrameworkElementFactory factory = new FrameworkElementFactory(typeof(FilterPanel));
      col.HeaderTemplate = new DataTemplate { VisualTree = factory };
    }

    public FilterPanel()
    {
      PreviewKeyDown += FilterPanel_PreviewKeyDown;

      int m = 5;
      Margin = new Thickness(-m, 0, -m, -3);

      _lblHeader = new TextBlock();
      _lblHeader.Text = "Header";
      _lblHeader.Margin = new Thickness(m, 0, m, 0);
      Children.Add(_lblHeader);

      _brdFilter = new Border();
      _brdFilter.BorderThickness = new Thickness(1);
      _brdFilter.BorderBrush = Brushes.Gainsboro;
      {
        TextBlock lblFilter = new TextBlock();
        lblFilter.SetBinding(TextBlock.TextProperty, new Binding(nameof(FilterText)));
        lblFilter.SetBinding(TextBlock.ToolTipProperty, new Binding(nameof(FilterToolTip)));
        lblFilter.DataContext = this;
        lblFilter.Margin = new Thickness(m, 0, m, 0);
        _brdFilter.Child = lblFilter;
      }
      Children.Add(_brdFilter);

      DataContextChanged += (s, a) =>
      {
        UpdateHeader();
        InvalidateVisual();
      };
    }

    private IFilterView FilterView
    {
      get
      {
        if (_filterView == null)
        {
          DataGridColumn column = GetColumn();
          DataGridComboBoxColumn lstCol = column as DataGridComboBoxColumn;
          if (lstCol != null)
          {
            var setters = lstCol.ElementStyle?.Setters;
            if (setters != null)
            {
              foreach (Setter setter in setters)
              {
                Binding binding = setter.Value as Binding;
                DependencyProperty prop = setter.Property;
                if (binding != null && prop == ComboBox.ItemsSourceProperty)
                {
                  _filterView = new ListFilter(this, binding);
                }
              }
            }
          }
          else
          {
            Type t = ListViewUtils.GetPropertyType(GetDataSource(), column.SortMemberPath);
            if (t == typeof(DateTime))
            { _filterView = new TextFilter(this); }
            else
            { _filterView = new TextFilter(this); }
          }
        }
        return _filterView;
      }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      Point p = e.GetPosition(this);
      if (p.Y > _lblHeader.ActualHeight)
      {
        HideAllEditFilters();

        FilterView.Visibility = Visibility.Visible;
        _brdFilter.Visibility = Visibility.Collapsed;

        DataGrid grd = GetDataGrid();
        grd.CurrentCellChanged -= Grd_CurrentCellChanged;
        grd.CurrentCellChanged += Grd_CurrentCellChanged;

        e.Handled = true;
        return;
      }
      base.OnMouseDown(e);
    }

    private void Grd_CurrentCellChanged(object sender, EventArgs e)
    {
      HideEditFilter();
    }

    private void HideAllEditFilters()
    {
      DataGrid grd = GetDataGrid();

      StringBuilder filterBuilder = new StringBuilder();
      foreach (FilterPanel filter in Utils.GetChildren<FilterPanel>(grd))
      {
        filter.HideEditFilter(grd);
      }
    }

    public void HideEditFilter(DataGrid grid = null)
    {
      if (_filterView?.Visibility == Visibility.Visible)
      { FilterView.Visibility = Visibility.Collapsed; }
      if (_brdFilter.Visibility != Visibility.Visible)
      { _brdFilter.Visibility = Visibility; }

      if (grid != null)
      { grid.CurrentCellChanged -= Grd_CurrentCellChanged; }
    }

    private void FilterPanel_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        if (FilterView.TryUpdateFilter(e.Source))
        {
          HideEditFilter();
        }
        e.Handled = true;
      }
    }

    private string _filterText;

    private DataGrid GetDataGrid()
    {
      return Utils.GetParent<DataGrid>(this);
    }

    private IBindingListView GetDataSource()
    {
      DataGrid grid = GetDataGrid();

      IBindingListView data = grid?.ItemsSource as IBindingListView;
      return data;
    }

    public string FilterText
    {
      get { return _filterText; }
      set
      {
        _filterText = value;

        DataGridColumn col = GetColumn();
        IBindingListView data = GetDataSource();

        if (data == null)
        { return; }

        FilterStatement = ListViewUtils.GetTextFilterStatement(_filterText, col.SortMemberPath, data, caseSensitive: true);

        FilterData();

        OnPropertyChanged();
      }
    }

    private void FilterData()
    {
      DataGrid grd = GetDataGrid();
      IBindingListView data = grd?.ItemsSource as IBindingListView;

      if (data == null)
      {
        throw new ArgumentNullException(nameof(data),
          "DataSource is not an IBindingListView or does not support filtering");
      }

      StringBuilder filterBuilder = new StringBuilder();
      foreach (FilterPanel filter in Utils.GetChildren<FilterPanel>(grd))
      {
        string colFilter = filter.FilterStatement;
        filter.HideEditFilter(grd);
        if (string.IsNullOrWhiteSpace(colFilter))
        { continue; }

        if (filterBuilder.Length > 0)
        { filterBuilder.Append(" AND "); }

        filterBuilder.AppendFormat("({0})", colFilter);
      }

      data.Filter = filterBuilder.ToString();
    }

    private string _filterStatement;
    public string FilterStatement
    {
      get { return _filterStatement; }
      set
      {
        _filterStatement = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(FilterToolTip));
      }
    }

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    public string FilterToolTip
    {
      get { return string.IsNullOrWhiteSpace(FilterStatement) ? "No Filter Active" : FilterStatement; }
    }

    private void UpdateHeader()
    {
      string column = GetColumn()?.Header as string;
      if (column == null)
      { return; }

      _lblHeader.Text = column;
    }

    private DataGridColumn GetColumn()
    {
      ContentPresenter columnPresenter = VisualTreeHelper.GetParent(this) as ContentPresenter;
      if (columnPresenter == null)
      { return null; }

      System.Windows.Controls.Primitives.DataGridColumnHeader header =
        columnPresenter.TemplatedParent as System.Windows.Controls.Primitives.DataGridColumnHeader;

      return header?.Column as DataGridColumn;
    }
  }

}
