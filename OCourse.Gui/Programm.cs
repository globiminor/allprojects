using System;
using System.Windows.Forms;
using Ocad;

namespace OCourse.Gui
{
  public static class Programm
  {
    private static bool _init;
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      //ChangeSymbol();
      Application.SetCompatibleTextRenderingDefault(false);
      Init();
      WdgOCourse frm = new WdgOCourse();
      frm.Args = args;
      Application.Run(frm);
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
