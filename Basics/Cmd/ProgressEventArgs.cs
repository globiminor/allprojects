using System;

namespace Basics.Cmd
{
  public class ProgressEventArgs : EventArgs
  {
    public double Progress { get; set; }
    public string Comment { get; set; }
    public bool KeepVisible { get; set; }
  }
}
