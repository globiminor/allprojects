using System;
using System.Windows.Forms;

namespace MacroUi
{
  static class Program
  {
    private static bool _init;
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      Application.SetCompatibleTextRenderingDefault(false);
      Init();

      MacroWdg wdg = new MacroWdg();
      try
      {
        wdg.Init(args); // args: Environment.GetCommandLineArgs()
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
        return;
      }

      Application.Run(wdg);
    }

    public static void Init()
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
  }
}
