using OcadScratch.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntGeoref.xaml
  /// </summary>
  public partial class CntGeoref : UserControl
  {
    public CntGeoref()
    {
      InitializeComponent();
    }

    public new ConfigVm DataContext
    {
      get { return base.DataContext as ConfigVm; }
      set { base.DataContext = value; }
    }

    private void BtnGetWgs_Click(object sender, RoutedEventArgs e)
    {
      DataContext?.CalcWgs84();
    }

    private void BtnGetKoord_Click(object sender, RoutedEventArgs e)
    {
      DataContext?.CalcMapCoord();
    }

    private void BtnGetDeklination_Click(object sender, RoutedEventArgs e)
    {
      DataContext?.CalcDeclination();
    }
  }
}
