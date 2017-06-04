using Basics;
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
  /// Interaktionslogik für FrmPrepare.xaml
  /// </summary>
  public partial class FrmConfig : Window
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

    public FrmConfig()
    {
      InitializeComponent();
      StyleImages(grdImages);

      StyleColors(grdColors);
      StyleSymbols(grdSymbols);

      txtElemTextSize.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ElemTextSize)));

      txtConstrTextSize.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ConstrTextSize)));
      txtConstrLineWidth.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ConstrLineWidth)));
      clrConstr.SetBinding(Xceed.Wpf.Toolkit.ColorPicker.SelectedColorProperty, new Binding(nameof(DataContext.ConstrColor)));

      grdImages.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Images)) { Mode = BindingMode.OneWay });
      grdColors.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Colors)) { Mode = BindingMode.OneWay });
      grdSymbols.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Symbols)) { Mode = BindingMode.OneWay });
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

    private void StyleImages(DataGrid grd)
    {
      grd.AutoGenerateColumns = false;
      XmlImage img;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Image Name", Binding = new Binding(nameof(img.Name)) };
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
      {
        DataGridTextColumn col = new DataGridTextColumn
        {
          Header = "Image Name",
        };
        Basics.Window.Utils.SetErrorBinding(col, nameof(img.Path));
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
    }

    public new ConfigVm DataContext
    {
      get { return (ConfigVm)base.DataContext; }
      set { base.DataContext = value; }
    }

    private void btnGetDeklination_Click(object sender, RoutedEventArgs e)
    {
      DataContext?.CalcDeclination();
    }

    private void mniInit_Click(object sender, RoutedEventArgs e)
    {
      string ocdFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "OCAD file";
        dlg.Filter = "*.ocd | *.ocd";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        ocdFile = dlg.FileName;
      }

      DataContext = DataContext ?? new ConfigVm();
      DataContext.Init(ocdFile);
    }

    private void mniLoad_Click(object sender, RoutedEventArgs e)
    {
      string configFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "O-Scratch Config File";
        dlg.Filter = "*.config | *.config";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        configFile = dlg.FileName;
      }

      DataContext = DataContext ?? new ConfigVm();
      XmlConfig config;
      using (TextReader r = new StreamReader(configFile))
      { Serializer.Deserialize(out config, r); }
      ConfigVm configVm = new ConfigVm(configFile, config);
      try
      {
        configVm.LoadSymbols();
      }
      finally
      { DataContext = configVm; }
    }


    private void mniSave_Click(object sender, RoutedEventArgs e)
    {
      string configFile;
      {
        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
        dlg.Title = "O-Scratch Config";
        dlg.Filter = "*.config | *.config";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        configFile = dlg.FileName;
      }
      DataContext.Save(configFile);
    }

    private void btnGetWgs_Click(object sender, RoutedEventArgs e)
    {
      DataContext = DataContext ?? new ConfigVm();
      DataContext.CalcWgs84();
    }

    private void btnGetKoord_Click(object sender, RoutedEventArgs e)
    {
      DataContext = DataContext ?? new ConfigVm();
      DataContext.CalcMapCoord();
    }

    private void btnConstr_Click(object sender, RoutedEventArgs e)
    {
    }
  }
}
