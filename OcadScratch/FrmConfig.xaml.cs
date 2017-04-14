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

    private void mniSave_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
