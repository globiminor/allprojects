
using Basics.Views;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Basics.Window
{
  public class ErrorPanel : StackPanel
  {
    public static void CreateGaga(DataGridTemplateColumn col)
    {
      FrameworkElementFactory factory = new FrameworkElementFactory(typeof(ErrorPanel));
      col.CellTemplate = new DataTemplate { VisualTree = factory };
    }

    private readonly TextBlock _lblHeader;
    public ErrorPanel()
    {
      int m = 5;
      Margin = new Thickness(-m, 0, -m, -3);

      _lblHeader = new TextBlock();
      _lblHeader.Margin = new Thickness(m, 0, m, 0);
      _lblHeader.SetBinding(TextBlock.TextProperty, new Binding("Path") { ValidatesOnDataErrors = true });
      _lblHeader.ToolTip = "Hallo";
      _lblHeader.ToolTipOpening += (s, e) =>
      {
        string ttp = null;
        if (ttp == null && _lblHeader.DataContext is IDataErrorInfo errInfo)
        {
          ttp = errInfo["Path"];
        }
        if (ttp == null)
        {
          ttp = _lblHeader.GetValue(TextBlock.TextProperty) as string;
        }
        if (ttp == null)
        {
          ttp = "..";
        }
        _lblHeader.ToolTip = ttp;
      };
      Children.Add(_lblHeader);

      //_brdFilter = new Border();
      //_brdFilter.BorderThickness = new Thickness(1);
      //_brdFilter.BorderBrush = Brushes.Gainsboro;
      //{
      //  TextBlock lblFilter = new TextBlock();
      //  lblFilter.SetBinding(TextBlock.TextProperty, new Binding(nameof(FilterText)));
      //  lblFilter.SetBinding(TextBlock.ToolTipProperty, new Binding(nameof(FilterToolTip)));
      //  lblFilter.DataContext = this;
      //  lblFilter.Margin = new Thickness(m, 0, m, 0);
      //  _brdFilter.Child = lblFilter;
      //}
      //Children.Add(_brdFilter);

      //DataContextChanged += (s, a) =>
      //{
      //  UpdateHeader();
      //  InvalidateVisual();
      //};
    }

  }
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
    private readonly CheckBox _chkFilter;

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
    private class CheckBoxFilter : IFilterView
    {
      private readonly CheckBox _chkFilter;
      public CheckBoxFilter(FilterPanel parent)
      {
        _chkFilter = new CheckBox();
        _chkFilter.IsThreeState = true;

        _chkFilter.SetBinding(CheckBox.IsCheckedProperty,
          new Binding(nameof(parent.FilterText)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit, Converter = new CheckConverter() });
        _chkFilter.DataContext = parent;
        _chkFilter.Visibility = Visibility.Collapsed;
        _chkFilter.Margin = new Thickness(0, 0, 0, 0);
        _chkFilter.HorizontalAlignment = HorizontalAlignment.Center;
        parent.Children.Add(_chkFilter);
      }

      public Visibility Visibility
      {
        get { return _chkFilter.Visibility; }
        set { _chkFilter.Visibility = value; }
      }
      bool IFilterView.TryUpdateFilter(object source)
      {
        CheckBox filter = source as CheckBox;
        if (filter == _chkFilter)
        {
          BindingExpression be = _chkFilter.GetBindingExpression(CheckBox.IsCheckedProperty);
          be.UpdateSource();
          return true;
        }
        return false;
      }
    }

    private class CheckConverter : IValueConverter
    {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
        if (value is string s)
        {
          if (s == "false")
          { return false; }
          if (s == "true")
          { return true; }
        }
        return null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
        if (value is bool b)
        {
          string query = b ? "true" : "false";
          return query;
        }
        return null;
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

      {
        _brdFilter = new Border();
        _brdFilter.BorderThickness = new Thickness(1);
        _brdFilter.BorderBrush = Brushes.Gainsboro;
        _brdFilter.Margin = new Thickness(0, 0, 0, 0);
        _brdFilter.DataContext = this;
        _brdFilter.SetBinding(Border.VisibilityProperty, new Binding(nameof(TextVisibility)) { Mode = BindingMode.OneWay });

        TextBlock lblFilter = new TextBlock();
        lblFilter.SetBinding(TextBlock.TextProperty, new Binding(nameof(FilterText)));
        lblFilter.SetBinding(TextBlock.ToolTipProperty, new Binding(nameof(FilterToolTip)));
        lblFilter.DataContext = this;
        lblFilter.Margin = new Thickness(m, 0, m, 0);
        _brdFilter.Child = lblFilter;
        Children.Add(_brdFilter);
      }
      {
        _chkFilter = new CheckBox();
        _chkFilter.DataContext = this;
        _chkFilter.HorizontalAlignment = HorizontalAlignment.Center;
        _chkFilter.DataContext = this;
        _chkFilter.SetBinding(CheckBox.VisibilityProperty, new Binding(nameof(CheckBoxVisibility)) { Mode = BindingMode.OneWay });
        _chkFilter.IsEnabled = false;

        Children.Add(_chkFilter);
      }

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
          if (column is DataGridComboBoxColumn lstCol)
          {
            var setters = lstCol.ElementStyle?.Setters;
            if (setters != null)
            {
              foreach (var o in setters)
              {
                Setter setter = (Setter)o;
                DependencyProperty prop = setter.Property;
                if (setter.Value is Binding binding && prop == ComboBox.ItemsSourceProperty)
                {
                  _filterView = new ListFilter(this, binding);
                }
              }
            }
          }
          else if (column is DataGridCheckBoxColumn)
          {
            _filterView = new CheckBoxFilter(this);
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

    public Visibility TextVisibility
    {
      get
      {
        if (CheckBoxVisibility == Visibility.Visible)
        {
          return Visibility.Collapsed;
        }
        if (_filterView?.Visibility == Visibility.Visible)
        {
          return Visibility.Collapsed;
        }

        return Visibility.Visible;
      }
    }
    public Visibility CheckBoxVisibility
    {
      get
      {
        if (!(_filterView is CheckBoxFilter))
        {
          return Visibility.Collapsed;
        }
        if (FilterView.Visibility == Visibility.Visible)
        {
          return Visibility.Collapsed;
        }
        if (string.IsNullOrWhiteSpace(FilterText))
        {
          return Visibility.Collapsed;
        }

        return Visibility.Visible;
      }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      Point p = e.GetPosition(this);
      if (p.Y > _lblHeader.ActualHeight)
      {
        HideAllEditFilters();

        FilterView.Visibility = Visibility.Visible;

        DataGrid grd = GetDataGrid();
        grd.CurrentCellChanged -= Grd_CurrentCellChanged;
        grd.CurrentCellChanged += Grd_CurrentCellChanged;

        e.Handled = true;
        OnPropertyChanged(nameof(TextVisibility));
        OnPropertyChanged(nameof(CheckBoxVisibility));
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
      UIElement headers = Utils.GetParent<System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter>(this);
      DataGrid grd = Utils.GetParent<DataGrid>(headers);

      foreach (var filter in Utils.GetChildren<FilterPanel>(headers))
      {
        filter.HideEditFilter(grd);
      }
    }

    public void HideEditFilter(DataGrid grid = null)
    {
      if (_filterView?.Visibility == Visibility.Visible)
      { FilterView.Visibility = Visibility.Collapsed; }
      {
        if (_filterView is CheckBoxFilter chkboxFilter && !string.IsNullOrWhiteSpace(FilterText))
        {
          _chkFilter.IsChecked = FilterText == "true" ? true : false;
        }

        OnPropertyChanged(nameof(TextVisibility));
        OnPropertyChanged(nameof(CheckBoxVisibility));
      }

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

    public static void ClearFilter(DataGrid grd)
    {
      if (!(grd?.ItemsSource is IBindingListView data))
      {
        throw new ArgumentNullException(nameof(data),
          "DataSource is not an IBindingListView or does not support filtering");
      }

      bool clearedAny = false;
      StringBuilder filterBuilder = new StringBuilder();
      foreach (var filter in Utils.GetChildren<FilterPanel>(grd))
      {
        if (!string.IsNullOrWhiteSpace(filter._filterText))
        {
          filter._filterText = null;
          filter._filterStatement = null;
          filter.OnPropertyChanged(nameof(FilterText));
          filter.OnPropertyChanged(nameof(FilterStatement));
          filter.OnPropertyChanged(nameof(FilterToolTip));

          filter.OnPropertyChanged(nameof(TextVisibility));
          filter.OnPropertyChanged(nameof(CheckBoxVisibility));
          clearedAny = true;
        }
      }
      if (clearedAny)
      { data.Filter = string.Empty; }
    }

    public static void ApplyFilter(DataGrid grd)
    {
      if (!(grd?.ItemsSource is IBindingListView data))
      {
        throw new ArgumentNullException(nameof(data),
          "DataSource is not an IBindingListView or does not support filtering");
      }

      StringBuilder filterBuilder = new StringBuilder();
      foreach (var filter in Utils.GetChildren<FilterPanel>(grd))
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
    private void FilterData()
    {
      DataGrid grd = GetDataGrid();

      ApplyFilter(grd);
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
      if (!(GetColumn()?.Header is string column))
      { return; }

      _lblHeader.Text = column;

      if (VisualTreeHelper.GetParent(this) is ContentPresenter depO && depO.HorizontalAlignment != HorizontalAlignment.Stretch)
      { depO.HorizontalAlignment = HorizontalAlignment.Stretch; }
    }

    private DataGridColumn GetColumn()
    {
      if (!(VisualTreeHelper.GetParent(this) is ContentPresenter columnPresenter))
      { return null; }

      System.Windows.Controls.Primitives.DataGridColumnHeader header =
        columnPresenter.TemplatedParent as System.Windows.Controls.Primitives.DataGridColumnHeader;

      return header?.Column as DataGridColumn;
    }
  }

}
