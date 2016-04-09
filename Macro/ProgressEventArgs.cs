
using System;

namespace Macro
{
  public delegate void ProgressEventHandler(object sender, ProgressEventArgs args);

  public class ProgressEventArgs : EventArgs
  {
    private string _msg;
    public ProgressEventArgs(string msg)
    {
      _msg = msg;
    }
    public string Message
    {
      get { return _msg; }
    }
  }
}
