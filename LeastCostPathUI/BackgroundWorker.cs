using System;
using System.Threading;
using System.Windows.Forms;

namespace Common.Gui
{
  public interface IWorker
  {
    bool Start();
    void Finish();
    void Finally();
  }
  public delegate void EventDlg<T>(object sender, T obj);

  public class BackgroundWorker
  {
    private readonly Control _parent;
    private readonly IWorker _worker;

    public BackgroundWorker(Control parent, IWorker worker)
    {
      _parent = parent;
      _worker = worker;
    }

    public void DoWork()
    {
      Thread t = new Thread(Run);
      t.Start();
    }
    private void Run()
    {
      try
      {
        bool success = _worker.Start();

        if (success)
        {
          _parent.Invoke(new MethodInvoker(_worker.Finish));
        }
      }
      catch (Exception exp)
      {
        EventDlg<Exception> expDlg = OnException;
        _parent.Invoke(expDlg, _parent, exp);
      }
      finally
      {
        _parent.Invoke(new MethodInvoker(_worker.Finally));
      }
    }
    private void OnException(object sender, Exception exp)
    {
      string msg = Basics.Utils.GetMsg(exp);
      MessageBox.Show(_parent, msg);
    }
  }
}
