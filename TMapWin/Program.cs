
using System;
using System.Windows.Forms;

namespace TMapWin
{
  class ProgramMap
  {
    [STAThread]
    static void Main()
    {
      Program.Run(new WdgMain());
    }
  }

  class Program3D
  {
    [STAThread]
    static void Main()
    {
      Program.Run(new WdgMain());
    }
  }

  class Program
  {
    public static void Run(Form startForm)
    {
      Application.ThreadException -= Application_ThreadException;
      Application.ThreadException += Application_ThreadException;
      Application.Run(startForm);      
    }
    private static void Application_ThreadException(object sender,
      System.Threading.ThreadExceptionEventArgs e)
    {
      Exception exp = e.Exception;
      string msg = Basics.Utils.GetMsg(exp);
      MessageBox.Show(msg);
    }
  }
}
