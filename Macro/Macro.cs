using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Macro
{
  public class Macro
  {
    #region external

    [DllImport("user32.dll")]
    public static extern bool ToAscii(int VirtualKey, int ScanCode,
                                      byte[] lpKeyState, ref uint lpChar, int uFlags);

    [DllImport("user32")]
    private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr param);

    [DllImport("user32")]
    private static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, out IntPtr lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);
    [DllImport("user32")]
    internal static extern IntPtr DestroyWindow(IntPtr hWnd);
    [DllImport("user32")]
    internal static extern IntPtr CloseWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    internal static extern bool EndDialog(IntPtr hWnd, IntPtr result);
    [DllImport("user32")]
    internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32")]
    private static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int max);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32")]
    private static extern int SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32")]
    private static extern IntPtr GetParent(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
       IntPtr dwExtraInfo);
    [DllImport("user32.dll")]
    public static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy,
      uint dwData, IntPtr dwExtraInfo);
    #endregion

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

    #region constants
    private const int MOUSEEVENTF_LEFTDOWN = 0x00000002;
    private const int MOUSEEVENTF_LEFTUP = 0x00000004;
    private const int MOUSEEVENTF_MIDDLEDOWN = 0x00000020;
    private const int MOUSEEVENTF_MIDDLEUP = 0x00000040;
    private const int MOUSEEVENTF_MOVE = 0x00000001;
    private const int MOUSEEVENTF_ABSOLUTE = 0x00008000;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x00000008;
    private const int MOUSEEVENTF_RIGHTUP = 0x00000010;

    private const byte VK_CLEAR = 12;
    public const byte VK_RETURN = 13;

    public const byte VK_SHIFT = 16;
    public const byte VK_CONTROL = 17;
    public const byte VK_ALT = 0x12;
    private const byte VK_MENU = 18;
    private const byte VK_PAUSE = 19;
    private const byte VK_CAPITAL = 20;

    private const byte VK_ESCAPE = 27;

    private const byte VK_SPACE = 32;
    public const byte VK_PRIOR = 33;
    public const byte VK_NEXT = 34;
    private const byte VK_END = 35;
    private const byte VK_HOME = 36;
    private const byte VK_LEFT = 37;
    private const byte VK_UP = 38;
    private const byte VK_RIGHT = 39;
    private const byte VK_DOWN = 40;
    private const byte VK_SELECT = 21;
    private const byte VK_PRINT = 42;
    private const byte VK_EXECUTE = 43;
    private const byte VK_SNAPSHOT = 44;
    private const byte VK_INSERT = 45;
    private const byte VK_DELETE = 46;
    private const byte VK_HELP = 47;

    private const int KEYEVENTF_KEYUP = 0x2;
    #endregion

    public event ProgressEventHandler Progress;
    private int _sleep = 200;
    private Process _currentProc = null;

    private Dictionary<IntPtr, string> _windows;

    public static Process InitProcess(string workDir, string execName)
    {
      string procName = Path.GetFileNameWithoutExtension(execName);
      Process[] processes = Process.GetProcessesByName(procName);
      Process proc;
      if (processes == null || processes.Length == 0)
      {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WorkingDirectory = Path.GetDirectoryName(workDir);
        startInfo.FileName = execName;
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
      if (Progress != null)
      { Progress(this, args); }
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
      foreach (Process proc in processes)
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

    public IntPtr SetForegroundWindow(string textStart)
    {
      IntPtr hWnd = IntPtr.Zero;
      _windows = new Dictionary<IntPtr, string>();
      EnumWindows(AddWindow, IntPtr.Zero);
      foreach (KeyValuePair<IntPtr, string> pair in _windows)
      {
        if (pair.Value.ToUpper().Contains(textStart.ToUpper()))
        {
          hWnd = pair.Key;
          break;
        }
      }
      if (hWnd != IntPtr.Zero)
      {
        SetWindow(hWnd);
      }
      return hWnd;
    }

    public Dictionary<IntPtr, string> GetChildWindowsText(IntPtr hWnd)
    {
      Dictionary<IntPtr, string> children = new Dictionary<IntPtr, string>();
      _windows = children;
      EnumChildWindows(hWnd, AddWindow, IntPtr.Zero);
      return children;
    }

    private void SetWindow(IntPtr hWnd)
    {
      SetForegroundWindow(hWnd);
      IntPtr current = GetForegroundWindow();
      int nTrys = 0;
      while (current != hWnd && GetParent(current) != hWnd && nTrys < 8)
      {
        IntPtr currentProcId;
        IntPtr currentThreadId = GetWindowThreadProcessId(current, out currentProcId);

        IntPtr setProcId;
        IntPtr setThreadId = GetWindowThreadProcessId(hWnd, out setProcId);

        if (currentThreadId == setThreadId)
        {
          return;
        }

        _windows = new Dictionary<IntPtr, string>();
        EnumChildWindows(current, AddWindow, IntPtr.Zero);
        foreach (KeyValuePair<IntPtr, string> pair in _windows)
        {
          if (pair.Key == hWnd)
          {
            return;
          }
        }
        string msg = string.Format("Soll: {0}; Ist: {1} {2}", hWnd, current, GetParent(hWnd));
        OnProgress(new ProgressEventArgs(msg));
        System.Threading.Thread.Sleep(_sleep);
        SetForegroundWindow(hWnd);
        System.Threading.Thread.Sleep(_sleep);
        current = GetForegroundWindow();
        nTrys++;
      }
    }


    private static List<byte> GetKeyBytes(char key)
    {
      short s = VkKeyScan(key);
      if (s == 0)
      { return null; } // Unhandled

      byte code = (byte)s;
      int state = s / 256;

      List<byte> all = new List<byte>();

      if ((state & 1) != 0)
      { all.Add(VK_SHIFT); }
      if ((state & 2) != 0)
      { all.Add(VK_CONTROL); }
      if ((state & 4) != 0)
      { all.Add(VK_ALT); }

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
        keybd_event(codes[i], 0, 0, IntPtr.Zero);
      }
      for (int i = n - 1; i >= 0; i--)
      {
        keybd_event(codes[i], 0, KEYEVENTF_KEYUP, IntPtr.Zero);
      }
    }

    public void SendKeys(string word)
    {
      foreach (char key in word)
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
      for (IntPtr hWnd = GetForegroundWindow(); true;
        hWnd = GetForegroundWindow())
      {
        StringBuilder title = new StringBuilder(256);
        GetWindowText(hWnd, title, 256);
        if (title.ToString().StartsWith(text))
        {
          return hWnd;
        }
        System.Threading.Thread.Sleep(_sleep);
      }
      return IntPtr.Zero;
    }

    private bool AddWindow(IntPtr hWnd, IntPtr data)
    {
      StringBuilder title = new StringBuilder(256);
      GetWindowText(hWnd, title, 256);
      _windows.Add(hWnd, title.ToString());
      return true;
    }

    public Rectangle GetWindowRect()
    {
      IntPtr hWnd = GetForegroundWindow();
      return GetWindowRect(hWnd);
    }
    public static Rectangle GetWindowRect(IntPtr hWnd)
    {
      Rectangle rect = new Rectangle();
      // returns with rect.Width = MaxX; rect.Height = MaxY
      GetWindowRect(hWnd, ref rect);

      // Correct for Width, Height
      rect.Width = rect.Width - rect.Left;
      rect.Height = rect.Height - rect.Top;

      return rect;
    }
    public string ForeGroundWindowName()
    {
      IntPtr hWnd = GetForegroundWindow();
      StringBuilder title = new StringBuilder(256);
      GetWindowText(hWnd, title, 256);

      return title.ToString();
    }
    private IntPtr WaitNewWindow(IntPtr hWnd)
    {
      while (GetForegroundWindow() == hWnd)
      {
        System.Threading.Thread.Sleep(200);
      }
      IntPtr nWnd = GetForegroundWindow();
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
      IntPtr foreGround = GetForegroundWindow();
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

      mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, IntPtr.Zero);
      Sleep(50);
      mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN,
        x, y, 0, IntPtr.Zero);
      Sleep(50);
      mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP,
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
