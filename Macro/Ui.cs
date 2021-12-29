
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Macro
{
  public static class Ui
  {
    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    #region external

    [DllImport("user32.dll")]
    public static extern bool ToAscii(int VirtualKey, int ScanCode,
                                      byte[] lpKeyState, ref uint lpChar, int uFlags);

    [DllImport("user32")]
    internal static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr param);
    [DllImport("user32.dll")]
    internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32")]
    internal static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, out IntPtr lpdwProcessId);

    [DllImport("user32.dll")]
    internal static extern short VkKeyScan(char ch);
    [DllImport("user32")]
    internal static extern IntPtr DestroyWindow(IntPtr hWnd);
    [DllImport("user32")]
    internal static extern IntPtr CloseWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    internal static extern bool EndDialog(IntPtr hWnd, IntPtr result);
    [DllImport("user32")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32")]
    private static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32")]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int max);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32")]
    internal static extern int SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32")]
    public static extern IntPtr GetParent(IntPtr hWnd);
    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(IntPtr hWnd, ref System.Drawing.Rectangle rect);

    [DllImport("user32")]
    public static extern IntPtr GetFocus();

    [DllImport("user32.dll")]
    internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
    [DllImport("user32.dll")]
    public static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32.dll")]
    internal static extern void mouse_event(uint dwFlags, uint dx, uint dy,
      uint dwData, IntPtr dwExtraInfo);
    #endregion

    public const int MOUSEEVENTF_LEFTDOWN = 0x00000002;
    public const int MOUSEEVENTF_LEFTUP = 0x00000004;
    public const int MOUSEEVENTF_MIDDLEDOWN = 0x00000020;
    public const int MOUSEEVENTF_MIDDLEUP = 0x00000040;
    public const int MOUSEEVENTF_MOVE = 0x00000001;
    public const int MOUSEEVENTF_ABSOLUTE = 0x00008000;
    public const int MOUSEEVENTF_RIGHTDOWN = 0x00000008;
    public const int MOUSEEVENTF_RIGHTUP = 0x00000010;

    public const byte VK_CLEAR = 12;
    public const byte VK_RETURN = 13;

    public const byte VK_SHIFT = 16;
    public const byte VK_CONTROL = 17;
    public const byte VK_ALT = 0x12;
    public const byte VK_MENU = 18;
    public const byte VK_PAUSE = 19;
    public const byte VK_CAPITAL = 20;

    public const byte VK_ESCAPE = 27;

    public const byte VK_SPACE = 32;
    public const byte VK_PRIOR = 33;
    public const byte VK_NEXT = 34;
    public const byte VK_END = 35;
    public const byte VK_HOME = 36;
    public const byte VK_LEFT = 37;
    public const byte VK_UP = 38;
    public const byte VK_RIGHT = 39;
    public const byte VK_DOWN = 40;
    public const byte VK_SELECT = 21;
    public const byte VK_PRINT = 42;
    public const byte VK_EXECUTE = 43;
    public const byte VK_SNAPSHOT = 44;
    public const byte VK_INSERT = 45;
    public const byte VK_DELETE = 46;
    public const byte VK_HELP = 47;

    public const int KEYEVENTF_KEYUP = 0x2;
  }
}
