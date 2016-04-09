using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Macro
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.ThreadException += Application_ThreadException;

      MacroWdg wdg = new MacroWdg();

      try
      {
        wdg.Init(Environment.GetCommandLineArgs());
      }
      catch(Exception e)
      {
        MessageBox.Show(e.Message);
        return;
      }
      Application.Run(wdg);
    }

    static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      MessageBox.Show(e.Exception.Message);
    }

  }
}