using System;
using System.Text;

namespace Macro
{
  public class WindowPtr
  {
    public IntPtr HWnd { get; set; }
    public IntPtr Data { get; set; }

    public string GetWindowText()
    {
      StringBuilder title = new StringBuilder(256);
      Ui.GetWindowText(HWnd, title, 256);
      return title.ToString();
    }

    public override string ToString()
    {
      return $"{HWnd}: '{GetWindowText()}'";
    }
  }
}
