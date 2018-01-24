using Microsoft.Win32;
using OcadScratch.ViewModels;
using OMapScratch;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntSymbols.xaml
  /// </summary>
  public partial class CntSymbols : UserControl
  {
    private class ColorPanel : Panel
    {
      public ColorPanel()
      {
        DataContextChanged += (s, a) =>
        InvalidateVisual();
      }


      protected override void OnRender(DrawingContext dc)
      {
        base.OnRender(dc);
        ColorRef clr = DataContext as ColorRef;
        if (clr != null)
        {
          Color color = clr.Color;
          color.A = 255;
          Brush b = new SolidColorBrush(color);

          dc.DrawRectangle(b, null, new Rect(2, 2, ActualWidth - 4, ActualHeight - 4));
        }
      }
    }

    private class SymbolPanel : Panel
    {
      public SymbolPanel()
      {
        DataContextChanged += (s, a) => InvalidateVisual();
      }

      protected override void OnRender(DrawingContext dc)
      {
        base.OnRender(dc);
        Symbol sym = DataContext as Symbol;
        if (sym != null)
        {
          Color color = Colors.Red;
          color.A = 255;

          SymbolUtils.DrawSymbol(dc, sym, color, (float)ActualWidth, (float)ActualHeight, 1);
        }
      }
    }

    public CntSymbols()
    {
      InitializeComponent();

      StyleColors(grdColors);
      StyleSymbols(grdSymbols);

      grdColors.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Colors)) { Mode = BindingMode.OneWay });
      grdSymbols.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Symbols)) { Mode = BindingMode.OneWay });

      btnLoad.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(DataContext.IsInit)));
    }

    public new ConfigVm DataContext
    {
      get { return base.DataContext as ConfigVm; }
      set { base.DataContext = value; }
    }

    private void StyleColors(DataGrid grd)
    {
      grd.AutoGenerateColumns = false;
      ColorRef clr;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Color ID", Binding = new Binding(nameof(clr.Id)) };
        col.IsReadOnly = true;
        grd.Columns.Add(col);
      }
      {
        DataGridTemplateColumn col = new DataGridTemplateColumn { Header = "Color" };
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(ColorPanel));

        col.CellTemplate = new DataTemplate { VisualTree = factory };
        col.IsReadOnly = true;
        grd.Columns.Add(col);
      }
    }

    private void StyleSymbols(DataGrid grd)
    {
      grd.AutoGenerateColumns = false;
      grd.MinRowHeight = 40;
      Symbol sym;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Symbol ID", Binding = new Binding(nameof(sym.Id)) };
        col.IsReadOnly = true;
        grd.Columns.Add(col);
      }
      {
        DataGridTemplateColumn col = new DataGridTemplateColumn { Header = "Symbol" };
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(SymbolPanel));

        col.CellTemplate = new DataTemplate { VisualTree = factory };
        col.IsReadOnly = true;
        grd.Columns.Add(col);
      }
    }

    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = ".xml | *.xml";
      if (dlg.ShowDialog() != true)
      { return; }

      using (TextReader reader = new StreamReader(dlg.FileName))
      {
        XmlSymbols symbols; Basics.Serializer.Deserialize(out symbols, reader);
        DataContext.LoadSymbols(symbols);
      }
    }
  }
}
