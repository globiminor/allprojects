using System;
using System.Windows.Forms;

namespace Dhm 
{
  public class Dhm : TMap.ICommand
  {
    private WdgMain _wdg;
    private static bool _init;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.SetCompatibleTextRenderingDefault(false);
      Init();
      Application.Run(new WdgMain());
    }

    private static void Init()
    {
      if (_init == false)
      {
        Application.EnableVisualStyles();
        Application.ThreadException -= Application_ThreadException;
        Application.ThreadException += Application_ThreadException;
      }
      _init = true;
    }
    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
      string msg = Basics.Utils.GetMsg(e.Exception);
      MessageBox.Show(msg);
    }
    #region ICommand Members

    public void Execute(TMap.IContext context)
    {
      if (_wdg == null)
      {
        _wdg = new WdgMain();
        Init();
      }
      _wdg.TMapContext = context;
      if (context is IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
    }

    #endregion
  }
}