using Grid;
using Grid.Lcp;
using System;
using System.Windows.Forms;

namespace LeastCostPathUI
{
  public partial class WdgLeastCostPath : Form
  {
    private delegate void InitHandler(WdgLeastCostPath wdg);
    private static event InitHandler InitWdg;

    private readonly ConfigVm _lcpConfigVm;

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

      Load += (s, a) => { InitWdg?.Invoke(this); };
    }

    [STAThread]
    public static void Main(string[] arg)
    {
      Init();
      Application.Run(new WdgLeastCostPath());
    }

    private static bool _init;
    public static void Init()
    {
      if (_init == false)
      {
        Application.EnableVisualStyles();
        Application.ThreadException -= Application_ThreadException;
        Application.ThreadException += Application_ThreadException;

        AppDomain.CurrentDomain.UnhandledException += (s,exp)=> 
        {
          string msg = Basics.Utils.GetMsg(null);
          MessageBox.Show(msg);
        };
      }
      _init = true;
    }

    private static void Application_ThreadException(object sender,
      System.Threading.ThreadExceptionEventArgs e)
    {
      Exception exp = e.Exception;
      string msg = Basics.Utils.GetMsg(exp);
      MessageBox.Show(msg);
    }

    #region events


    private void BtnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    public ConfigVm Vm => _lcpConfigVm;
    private void BtnOK_Click(object sender, EventArgs e)
    {
      IGrid<double> grdHeight = _lcpConfigVm.HeightGrid;
      string grdVelo = _lcpConfigVm.VeloPath;
      double resol = _lcpConfigVm.Resolution;
      Steps step = _lcpConfigVm.StepsMode;

      Basics.Geom.IBox box = cntOutput.GetCalcBox();

      IDirCostModel<TvmCell> costProvider = new TerrainVeloModel(grdHeight, VelocityGrid.FromImage(grdVelo))
      { TvmCalc = _lcpConfigVm.TvmCalc };
      ILcpGridModel costModel = new LeastCostGrid<TvmCell>(costProvider, resol, box, step);

      cntOutput.Calc(costModel, grdHeight, grdVelo, ResetCalc);
    }
    private void ResetCalc()
    {

    }
    #endregion

  }
}
