using OcadScratch.ViewModels;
using OMapScratch;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntWorkElem.xaml
  /// </summary>
  public partial class CntWorkElem : UserControl
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
        WorkElemVm wElem = DataContext as WorkElemVm;
        Elem elem = wElem?.Elem;
        if (elem != null)
        {
          Color color = elem.Color?.Color ?? Colors.Red;
          color.A = 255;
          Pen p = new Pen(new SolidColorBrush(color), 1);

          Canvas parent = (Canvas)Parent;
          SymbolUtils.DrawSymbol(dc, elem.Symbol, color, (float)parent.ActualWidth, (float)parent.ActualHeight, 2);
        }
      }
    }

    public CntWorkElem()
    {
      DataContext = new WorkElemVm();

      InitializeComponent();

      SymbolPanel pnl = new SymbolPanel();
      pnl.Margin = new System.Windows.Thickness(0);
      cnvSymbol.Children.Add(pnl);

      //Text = "{Binding SymbolId, UpdateSourceTrigger=Explicit}"
      txtSymbol.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.SymbolId)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

      //Text = "{Binding ColorId, UpdateSourceTrigger=Explicit}"
      txtColor.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ColorId)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

      //Text = "{Binding Text, UpdateSourceTrigger=Explicit}"
      txtText.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.Text)) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
    }

    public new WorkElemVm DataContext
    {
      get { return (WorkElemVm)base.DataContext; }
      set { base.DataContext = value; }
    }

    private void CnvSymbol_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      cnvSymbol.InvalidateVisual();
    }
  }
}
