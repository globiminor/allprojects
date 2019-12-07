using Microsoft.Win32;
using OcadScratch.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntSettings.xaml
  /// </summary>
  public partial class CntSettings : UserControl
  {
    public CntSettings()
    {
      DataContext = new ConfigVm();

      InitializeComponent();

      txtElemTextSize.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ElemTextSize)));

      txtConstrTextSize.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ConstrTextSize)));
      txtConstrLineWidth.SetBinding(TextBox.TextProperty, new Binding(nameof(DataContext.ConstrLineWidth)));
      clrConstr.SetBinding(Xceed.Wpf.Toolkit.ColorPicker.SelectedColorProperty, new Binding(nameof(DataContext.ConstrColor)));
    }

    public new ConfigVm DataContext
    {
      get { return (ConfigVm)base.DataContext; }
      set { base.DataContext = value; }
    }
  }
}
