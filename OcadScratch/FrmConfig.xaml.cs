using OcadScratch.ViewModels;
using System.Collections.Specialized;
using System.Net;
using System.Windows;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für FrmPrepare.xaml
  /// </summary>
  public partial class FrmConfig : Window
  {
    public FrmConfig()
    {
      InitializeComponent();
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
  }
}
