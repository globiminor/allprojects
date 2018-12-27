using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Macro
{
  public class Processor
  {
    public event ProgressEventHandler Progress;
    private readonly int _sleep = 200;
    private Process _currentProc = null;

    public static Process InitProcess(string workDir, string execName)
    {
      string procName = Path.GetFileNameWithoutExtension(execName);
      Process[] processes = Process.GetProcessesByName(procName);
      Process proc;
      if (processes == null || processes.Length == 0)
      {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
          WorkingDirectory = Path.GetDirectoryName(workDir),
          FileName = execName
        };
        proc = Process.Start(startInfo);
      }
      else
      { proc = processes[0]; }
      return proc;
    }

    public IntPtr MainWindowHandle
    {
      get
      {
        if (_currentProc == null)
        { throw new InvalidOperationException("No process set"); }

        return _currentProc.MainWindowHandle;
      }
    }

    protected virtual void OnProgress(ProgressEventArgs args)
    {
      Progress?.Invoke(this, args);
    }

    public void Sleep(int milliseconds)
    {
      OnProgress(null);
      System.Threading.Thread.Sleep(milliseconds);
    }

    public void KillProcess(string process)
    {
      IList<Process> processes = Process.GetProcessesByName(process);
      _currentProc = null;

      processes[0].Kill();
    }

    public void SetForegroundProcess(string process)
    {
      IList<Process> processes = Process.GetProcessesByName(process);
      foreach (var proc in processes)
      {
        if (proc.MainWindowHandle != IntPtr.Zero)
        {
          SetForegroundProcess(proc);
          break;
        }
      }
    }

    public void SetForegroundProcess(Process process)
    {
      _currentProc = process;
      SetWindow(_currentProc.MainWindowHandle);
    }

    public void SetForegroundProcess()
    {
      if (_currentProc == null)
      { return; }
      SetWindow(_currentProc.MainWindowHandle);
    }

    private class WindowsHandler
    {
      private List<WindowPtr> _windows = new List<WindowPtr>();

      public void InitWindows(IntPtr? data = null)
      {
        _windows.Clear();
        Ui.EnumWindows(AddWindow, data ?? IntPtr.Zero);
      }

      public void InitChildWindows(IntPtr hWnd, IntPtr? data = null)
      {
        _windows.Clear();
        Ui.EnumChildWindows(hWnd, AddWindow, data ?? IntPtr.Zero);
      }

      private bool AddWindow(IntPtr hWnd, IntPtr data)
      {
        _windows.Add(new WindowPtr { HWnd = hWnd, Data = data });
        return true;
      }
      public IList<WindowPtr> Windows { get { return _windows; } }
    }

    public IntPtr SetForegroundWindow(string textStart)
    {
      IntPtr hWnd = IntPtr.Zero;
      WindowsHandler w = new WindowsHandler();
      w.InitWindows();
      foreach (var window in w.Windows)
      {
        if (window.GetWindowText().ToUpper().Contains(textStart.ToUpper()))
        {
          hWnd = window.HWnd;
          break;
        }
      }
      if (hWnd != IntPtr.Zero)
      {
        SetWindow(hWnd);
      }
      return hWnd;
    }

    public static IList<WindowPtr> GetChildWindows(IntPtr hWnd)
    {
      WindowsHandler w = new WindowsHandler();
      w.InitChildWindows(hWnd);
      return w.Windows;
    }

    private void SetWindow(IntPtr hWnd)
    {
      Ui.SetForegroundWindow(hWnd);
      IntPtr current = Ui.GetForegroundWindow();
      int nTrys = 0;
      while (current != hWnd && Ui.GetParent(current) != hWnd && nTrys < 8)
      {
        IntPtr currentThreadId = Ui.GetWindowThreadProcessId(current, out IntPtr currentProcId);

        IntPtr setThreadId = Ui.GetWindowThreadProcessId(hWnd, out IntPtr setProcId);

        if (currentThreadId == setThreadId)
        {
          return;
        }

        WindowsHandler w = new WindowsHandler();
        w.InitChildWindows(current, IntPtr.Zero);
        foreach (var pair in w.Windows)
        {
          if (pair.HWnd == hWnd)
          {
            return;
          }
        }
        string msg = string.Format("Soll: {0}; Ist: {1} {2}", hWnd, current, Ui.GetParent(hWnd));
        OnProgress(new ProgressEventArgs(msg));
        System.Threading.Thread.Sleep(_sleep);
        Ui.SetForegroundWindow(hWnd);
        System.Threading.Thread.Sleep(_sleep);
        current = Ui.GetForegroundWindow();
        nTrys++;
      }
    }


    private static List<byte> GetKeyBytes(char key)
    {
      short s = Ui.VkKeyScan(key);
      if (s == 0)
      { return null; } // Unhandled

      byte code = (byte)s;
      int state = s / 256;

      List<byte> all = new List<byte>();

      if ((state & 1) != 0)
      { all.Add(Ui.VK_SHIFT); }
      if ((state & 2) != 0)
      { all.Add(Ui.VK_CONTROL); }
      if ((state & 4) != 0)
      { all.Add(Ui.VK_ALT); }

      all.Add(code);

      return all;
    }

    public void SendCommands(char key, params byte[] states)
    {
      SendCommand(key, states);
    }
    public void SendCommand(char key, IList<byte> states)
    {
      List<byte> all = GetKeyBytes(key);

      int nState = states.Count;
      for (int iState = nState - 1; iState >= 0; iState--)
      {
        byte state = states[iState];
        if (all.IndexOf(state) < 0)
        { all.Insert(0, state); }
      }

      SendCode(all);
    }

    public void SendCodes(params byte[] codes)
    { SendCode(codes); }
    public void SendCode(IList<byte> codes)
    {
      int n = codes.Count;
      for (int i = 0; i < n; i++)
      {
        Ui.keybd_event(codes[i], 0, 0, IntPtr.Zero);
      }
      for (int i = n - 1; i >= 0; i--)
      {
        Ui.keybd_event(codes[i], 0, Ui.KEYEVENTF_KEYUP, IntPtr.Zero);
      }
    }

    public void SendKeys(string word)
    {
      foreach (var key in word)
      { SendKey(key); }
    }

    public void SendKey(char key)
    {
      List<byte> all = GetKeyBytes(key);

      if (all != null)
      { SendCode(all); }
    }

    private IntPtr WaitNewWindow(string text)
    {
      DateTime t0 = DateTime.Now;
      for (IntPtr hWnd = Ui.GetForegroundWindow(); true;
        hWnd = Ui.GetForegroundWindow())
      {
        StringBuilder title = new StringBuilder(256);
        Ui.GetWindowText(hWnd, title, 256);
        if (title.ToString().StartsWith(text))
        {
          return hWnd;
        }
        System.Threading.Thread.Sleep(_sleep);
        if ((DateTime.Now - t0).TotalSeconds > 60)
        { break; }
      }
      return IntPtr.Zero;
    }

    public Rectangle GetWindowRect()
    {
      IntPtr hWnd = Ui.GetForegroundWindow();
      return GetWindowRect(hWnd);
    }
    public static Rectangle GetWindowRect(IntPtr hWnd)
    {
      Rectangle rect = new Rectangle();
      // returns with rect.Width = MaxX; rect.Height = MaxY
      Ui.GetWindowRect(hWnd, ref rect);

      // Correct for Width, Height
      rect.Width = rect.Width - rect.Left;
      rect.Height = rect.Height - rect.Top;

      return rect;
    }
    public static string ForeGroundWindowName()
    {
      IntPtr hWnd = Ui.GetForegroundWindow();
      StringBuilder title = new StringBuilder(256);
      Ui.GetWindowText(hWnd, title, 256);

      return title.ToString();
    }
    private IntPtr WaitNewWindow(IntPtr hWnd)
    {
      while (Ui.GetForegroundWindow() == hWnd)
      {
        System.Threading.Thread.Sleep(200);
      }
      IntPtr nWnd = Ui.GetForegroundWindow();
      SetWindow(nWnd);
      return nWnd;
    }

    public void WaitForInputIdle()
    {
      if (_currentProc == null)
      { throw new InvalidOperationException("No current process set"); }
      WaitForInputIdle(_currentProc);
    }

    private void WaitForInputIdle(Process proc)
    {
      { proc.WaitForInputIdle(); }
    }

    public void WaitFileIdle(string fileName)
    {
      FileStream stream = null;
      while (stream == null)
      {
        try
        { stream = File.OpenRead(fileName); }
        catch
        {
          System.Threading.Thread.Sleep(_sleep);
        }
      }
      stream.Close();
    }
    public void WaitIdle()
    {
      if (_currentProc == null)
      { throw new InvalidOperationException("No current process set"); }
      WaitIdle(_currentProc);
    }
    private void WaitIdle(Process proc)
    {
      TimeSpan t0 = proc.TotalProcessorTime;
      TimeSpan dt = new TimeSpan(0, 0, 1);
      while (dt.TotalMilliseconds > 0)
      {
        System.Threading.Thread.Sleep(_sleep);
        TimeSpan t1 = proc.TotalProcessorTime;
        dt = t1 - t0;
        t0 = t1;
      }
      return;
    }


    #region maus
    public void ForegroundClick(int x, int y)
    {
      IntPtr foreGround = Ui.GetForegroundWindow();
      Rectangle rect = GetWindowRect(foreGround);
      int ix;
      int iy;

      if (x > 0)
      { ix = rect.Left + x; }
      else
      { ix = rect.Right + x; }

      if (y > 0)
      { iy = rect.Top + y; }
      else
      { iy = rect.Bottom + y; }

      MausClick((uint)ix, (uint)iy);
    }
    private void MausClick(uint x, uint y)
    {
      x = (uint)(x * 65535 / Screen.PrimaryScreen.Bounds.Width);
      y = (uint)(y * 65535 / Screen.PrimaryScreen.Bounds.Height);

      Ui.mouse_event(Ui.MOUSEEVENTF_ABSOLUTE | Ui.MOUSEEVENTF_MOVE, x, y, 0, IntPtr.Zero);
      Sleep(50);
      Ui.mouse_event(Ui.MOUSEEVENTF_ABSOLUTE | Ui.MOUSEEVENTF_MOVE | Ui.MOUSEEVENTF_LEFTDOWN,
        x, y, 0, IntPtr.Zero);
      Sleep(50);
      Ui.mouse_event(Ui.MOUSEEVENTF_ABSOLUTE | Ui.MOUSEEVENTF_LEFTUP,
        x, y, 0, IntPtr.Zero);
    }
    private void MausDoubleClick(uint x, uint y)
    {
      MausClick(x, y);
      MausClick(x, y);
    }

    #endregion
  }
}
