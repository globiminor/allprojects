
using Basics.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Basics.Window
{
  public class FilterPanel : StackPanel
  {
    private readonly TextBlock _lbl;
    private readonly TextBox _txt;

    public static void CreateHeader(DataGridColumn col)
    {
      FrameworkElementFactory factory = new FrameworkElementFactory(typeof(FilterPanel));
      col.HeaderTemplate = new DataTemplate { VisualTree = factory };
    }

    public FilterPanel()
    {
      _lbl = new TextBlock();
      _lbl.Text = "Header";
      Children.Add(_lbl);

      _txt = new TextBox();
      _txt.SetBinding(InputBindingsManager.UpdatePropertySourceWhenEnterPressedProperty, "TextBox.Text");
      _txt.SetBinding(TextBox.TextProperty, new Binding(nameof(FilterText)));
      _txt.DataContext = this;
      Children.Add(_txt);

      DataContextChanged += (s, a) =>
      {
        UpdateHeader();
        InvalidateVisual();
      };
    }

    private string _filterText;
    public string FilterText
    {
      get { return _filterText; }
      set
      {
        _filterText = value;

        DataGridColumn col = GetColumn();
        if (col == null)
        { return; }

        UIElement parent = this;
        while (parent != null && !(parent is DataGrid))
        {
          parent = VisualTreeHelper.GetParent(parent) as UIElement;
        }
        DataGrid grid = parent as DataGrid;

        IBindingListView data = grid?.ItemsSource as IBindingListView;
        if (data == null)
        { return; }

        string filter = ListViewUtils.GetTextFilterStatement(_filterText, col.SortMemberPath, data, caseSensitive: true);
        data.Filter = filter; 
      }
    }

    private void UpdateHeader()
    {
      string column = GetColumn()?.Header as string;
      if (column == null)
      { return; }

      _lbl.Text = column;
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
