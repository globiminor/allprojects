using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Basics.Window.Browse
{
  /// <summary>
  /// Interaktionslogik für FrmBrowser.xaml
  /// </summary>
  public partial class FrmBrowser : System.Windows.Window
  {
    private class SymbolPanel : Panel
    {
      public SymbolPanel()
      {
        DataContextChanged += (s, a) => InvalidateVisual();
      }

      protected override void OnRender(DrawingContext dc)
      {
        base.OnRender(dc);
        ContentVm wElem = DataContext as ContentVm;
        if (wElem == null)
        { return; }

        ImageSource icon = Utils.GetIcon(wElem?.FullPath, true, wElem.IsDirectory);
        if (icon == null)
        { return; }

        dc.DrawImage(icon, new Rect(0, 0, 16, 16));
      }
    }

    public FrmBrowser()
    {
      InitializeComponent();

      txtPath.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.DirectoryPath)));
      txtFileName.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.FileName)));
      btnUp.Click += (s, e) =>
      {
        DataContext?.SetContentParent();
      };

      grdDevices.AutoGenerateColumns = false;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Device", Binding = new Binding(nameof(ContentVm.Name)) };
        col.IsReadOnly = true;
        grdDevices.Columns.Add(col);
      }
      grdDevices.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Devices)));
      grdDevices.MouseDoubleClick += (s, e) =>
      {
        DataContext?.SetContent(grdDevices.CurrentItem as ContentVm);
      };


      grdFiles.AutoGenerateColumns = false;
      grdFiles.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Items)));
      {
        DataGridTemplateColumn col = new DataGridTemplateColumn { Header = "Symbol" };
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(SymbolPanel));
        col.CellTemplate = new DataTemplate { VisualTree = factory };
        col.IsReadOnly = true;
        grdFiles.Columns.Add(col);
      }
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(ContentVm.Name)) };

        //Basics.Window.FilterPanel.CreateHeader(col);

        col.IsReadOnly = true;
        grdFiles.Columns.Add(col);
      }
      grdFiles.MouseDoubleClick += (s, e) =>
      {
        DataContext?.SetContent(grdFiles.CurrentItem as ContentVm);
        if ((DataContext?.Selected?.IsDirectory ?? true) == false)
        {
          DialogResult = true;
          Close();
        }
      };
      grdFiles.SelectedCellsChanged += (s, e) =>
      {
        DataContext?.SetSelected(grdFiles.CurrentItem as ContentVm);
      };
    }

    public new BrowserVm DataContext
    {
      get { return (BrowserVm)base.DataContext; }
      set { base.DataContext = value; }
    }
  }
}
