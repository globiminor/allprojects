using System;
using System.Windows.Forms;
using Ocad;
using OCourse.Gui;
using System.IO;

namespace OCourse
{
  public static class Common
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
      Application.Run(new WdgOCourse());
    }

    private static int NewSymbol(ElementIndex index)
    {
      int symbol = index.Symbol;
      if (symbol % 1000 == 1)
      {
        return symbol - 1;
      }
      else
      {
        return symbol;
      }
    }
    public static void ChangeSymbol()
    {
      OcadWriter w = OcadWriter.AppendTo(@"D:\daten\felix\OL\wm2010\Henriksåsen\Henriksåsen2008_v.ocd");
      try
      {
        w.ChangeSymbols(NewSymbol);
      }
      finally
      {
        w.Close();
      }
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
