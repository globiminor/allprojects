using Grid;
using Grid.Lcp;
using System;
using System.Windows.Forms;

namespace LeastCostPathUI
{
  public partial class WdgLeastCostPath : Form
  {
    private ConfigVm _lcpConfigVm;

    public WdgLeastCostPath()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
      _lcpConfigVm = new ConfigVm();
      cntConfig.ConfigVm = _lcpConfigVm;
    }

    [STAThread]
    public static void Main(string[] arg)
    {
      Application.ThreadException -= Application_ThreadException;
      Application.ThreadException += Application_ThreadException;
      Application.Run(new WdgLeastCostPath());
    }

    private static void Application_ThreadException(object sender,
      System.Threading.ThreadExceptionEventArgs e)
    {
      Exception exp = e.Exception;
      string msg = Basics.Utils.GetMsg(exp);
      MessageBox.Show(msg);
    }

    #region events


    private void btnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      IDoubleGrid grdHeight = _lcpConfigVm.HeightGrid;
      string grdVelo = _lcpConfigVm.VeloPath;
      double resol = _lcpConfigVm.Resolution;
      Steps step = _lcpConfigVm.StepsMode;
      ICostProvider costProvider = _lcpConfigVm.CostProvider;

      cntOutput.Calc(costProvider, grdHeight, grdVelo, resol, step, ResetCalc);
    }
    private void ResetCalc()
    {

    }
    #endregion

  }
}
