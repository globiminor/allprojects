using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using Cards.Gui;
using Cards.Vm;

namespace Cards
{
  public static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.ThreadException += Application_ThreadException;
      FrmCards frm = new FrmCards();
      CardsVm cards = new CardsVm(frm);

      Spider s = new Spider();
      s.Init();
      DataContractSerializer ser = new DataContractSerializer(typeof(Spider));
      using (Stream writer = new FileStream(@"C:\daten\temp\spider.xml", FileMode.Create))
      {
        ser.WriteObject(writer, s);
      }

      using (Stream reader = new FileStream(@"C:\daten\temp\spider.xml", FileMode.Open))
      {
        Spider read = (Spider)ser.ReadObject(reader);
        if (read == null) throw new InvalidOperationException();
      }

      cards.Cards = s.CreateStand();
      Application.Run(frm);
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Exception exp = e.Exception;
      string msg = ExceptionUtils.ToString(exp);
      MessageBox.Show(msg, string.Format("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
