using OcadScratch.ViewModels;
using System.Windows.Controls;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntWorkElem.xaml
  /// </summary>
  public partial class CntWorkElem : UserControl
  {
    public CntWorkElem()
    {
      InitializeComponent();
    }

    public new WorkElemVm DataContext
    {
      get { return (WorkElemVm)base.DataContext; }
      set { base.DataContext = value; }
    }
  }
}
