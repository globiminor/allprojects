
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Macro
{
  public class Wm
  {
    [DllImport("user32.dll")]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    internal static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, int lparam);


    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    public const int WM_SIZE = 0x0005;
    public const int WM_GETTEXT = 0x000D;
    public const int WM_GETTEXTLENGTH = 0x000E;
    public const int WM_SHOWWINDOW = 0x0018;
    public const int WM_CHILDACTIVATE = 0x0022;
    public const int WM_SETFONT = 0x0030;
    public const int WM_WINDOWPOSCHANGING = 0x0046;
    public const int WM_WINDOWPOSCHANGED = 0x0047;
    public const int WM_NOTIFY = 0x004E;
    public const int WM_NCCALCSIZE = 0x0083;
    public const int WM_INITDIALOG = 0x0110;
    public const int WM_USER = 0x0400;

    public const int OFN_READONLY = 0x00000001;
    public const int OFN_HIDEREADONLY = 0x00000004;
    public const int OFN_NOCHANGEDIR = 0x00000008;
    public const int OFN_ENABLEHOOK = 0x00000020;
    public const int OFN_NOVALIDATE = 0x00000100;
    public const int OFN_ALLOWMULTISELECT = 0x00000200;
    public const int OFN_PATHMUSTEXIST = 0x00000800;
    public const int OFN_CREATEPROMPT = 0x00002000;
    public const int OFN_EXPLORER = 0x00080000;
    public const int OFN_NODEREFERENCELINKS = 0x00100000;
    public const int OFN_ENABLESIZING = 0x00800000;

    public const int CDM_FIRST = (WM_USER + 100);
    public const int CDM_GETSPEC = (CDM_FIRST + 0x0000);
    public const int CDM_GETFILEPATH = (CDM_FIRST + 0x0001);
    public const int CDM_GETFOLDERPATH = (CDM_FIRST + 0x0002);

    public const int CDN_FIRST = unchecked((int)(0U - 601U));
    public const int CDN_INITDONE = (CDN_FIRST - 0x0000);
    public const int CDN_SELCHANGE = (CDN_FIRST - 0x0001);
    public const int CDN_FOLDERCHANGE = (CDN_FIRST - 0x0002);
    public const int CDN_SHAREVIOLATION = (CDN_FIRST - 0x0003);
    public const int CDN_FILEOK = (CDN_FIRST - 0x0005);

    public static string GetFileDialogFolder(string dialogTitle)
    {
      IList<WindowPtr> allWindows = Processor.GetChildWindows(IntPtr.Zero);

      foreach (var window in allWindows)
      {
        if (window.GetWindowText() == dialogTitle)
        {
          IList<WindowPtr> controls = Processor.GetChildWindows(window.HWnd);

          foreach (var control in controls)
          {
            string txt = control.GetWindowText();
            if (txt.Contains("Adresse:"))
            {
              return txt;
            }
          }
        }
      }
      return string.Empty;
    }

    public static void ShowOpenFileDlg()
    {
      OpenFileName ofn = new OpenFileName();

      ofn.structSize = Marshal.SizeOf(ofn);

      //      ofn.filter = "Log files\0*.log\0Batch files\0*.bat\0";

      ofn.file = new string(new char[256]);
      ofn.maxFile = ofn.file.Length;

      ofn.fileTitle = new string(new char[64]);
      ofn.maxFileTitle = ofn.fileTitle.Length;

      //      ofn.initialDir = "C:\\";
      ofn.title = "Open file called using platform invoke...";
      //      ofn.defExt = "txt";

      ofn.flags =
        OFN_READONLY | OFN_HIDEREADONLY | OFN_NOCHANGEDIR | OFN_NOVALIDATE |                                  // OFN_ALLOWMULTISELECT | 
        OFN_PATHMUSTEXIST | OFN_NODEREFERENCELINKS | OFN_EXPLORER | OFN_ENABLEHOOK | OFN_ENABLESIZING;

      ofn.hook = HandleDialogEvents;

      if (GetOpenFileName(ofn))
      {
        Console.WriteLine("Selected file with full path: {0}", ofn.file);
        Console.WriteLine("Selected file name: {0}", ofn.fileTitle);
        Console.WriteLine("Offset from file name: {0}", ofn.fileOffset);
        Console.WriteLine("Offset from file extension: {0}", ofn.fileExtension);
      }
    }

    public static string GetControlText(IntPtr hWnd)
    {

      // Get the size of the string required to hold the window title (including trailing null.) 
      Int32 titleSize = SendMessage(hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();

      // If titleSize is 0, there is no title so return an empty string (or null)
      if (titleSize == 0)
        return String.Empty;

      StringBuilder title = new StringBuilder(titleSize + 1);

      SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);

      return title.ToString();
    }

    private static IntPtr HandleDialogEvents(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
      int t = new List<int> {WM_SIZE,WM_SHOWWINDOW,WM_CHILDACTIVATE    ,WM_SETFONT,
        WM_WINDOWPOSCHANGING    ,WM_WINDOWPOSCHANGED,WM_NOTIFY ,WM_NCCALCSIZE    ,WM_INITDIALOG
      }.FirstOrDefault(x => x == msg);
      if (t == 0 && msg != 0x0128)
      { }
      if (msg == WM_NOTIFY)
      {
        OFNOTIFY notify = (OFNOTIFY)Marshal.PtrToStructure(lParam, typeof(OFNOTIFY));

        int cdn = (int)notify.hdr.code;
        if (cdn == CDN_INITDONE || cdn == CDN_SHAREVIOLATION || cdn == CDN_FOLDERCHANGE)
        { }
        else
        { }
        if (cdn == CDN_FOLDERCHANGE)
        { cdn = CDN_SELCHANGE; }
        if (cdn == CDN_SELCHANGE)
        {
          OpenFileName ofn = (OpenFileName)Marshal.PtrToStructure(notify.lpOFN, typeof(OpenFileName));
          IntPtr parent = Ui.GetParent(hWnd);

          // var ccs = Processor.GetChildWindows(parent);

          int sizeNeeded = (int)SendMessage(parent,                      // hWnd of window to receive message
            CDM_GETSPEC,                          // Msg (message to send)
            IntPtr.Zero,                          // wParam (additional info)
            IntPtr.Zero);                         // lParam (additional info)

          if (sizeNeeded != 0)
          {
            string str = new string(new char[256]);
            GCHandle gcHandle = GCHandle.Alloc(str, GCHandleType.Pinned);
            IntPtr pStr = gcHandle.AddrOfPinnedObject();
            int n = (int)SendMessage(parent, CDM_GETSPEC, new IntPtr(256), pStr);

            n = (int)SendMessage(parent, CDM_GETFILEPATH, new IntPtr(256), pStr);

            n = (int)SendMessage(parent, CDM_GETFOLDERPATH, new IntPtr(256), pStr);

            gcHandle.Free();

          }
        }
      }

      return IntPtr.Zero;
    }

  }

  [Serializable]
  [StructLayout(LayoutKind.Sequential)]
  internal struct WINDOWPLACEMENT
  {
    /// <summary>
    /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
    /// <para>
    /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
    /// </para>
    /// </summary>
    public int Length;

    /// <summary>
    /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
    /// </summary>
    public int Flags;

    /// <summary>
    /// The current show state of the window.
    /// </summary>
    public ShowWindowCommands ShowCmd;

    /// <summary>
    /// The coordinates of the window's upper-left corner when the window is minimized.
    /// </summary>
    public POINT MinPosition;

    /// <summary>
    /// The coordinates of the window's upper-left corner when the window is maximized.
    /// </summary>
    public POINT MaxPosition;

    /// <summary>
    /// The window's coordinates when the window is in the restored position.
    /// </summary>
    public RECT NormalPosition;

    /// <summary>
    /// Gets the default (empty) value.
    /// </summary>
    public static WINDOWPLACEMENT Default
    {
      get
      {
        WINDOWPLACEMENT result = new WINDOWPLACEMENT();
        result.Length = Marshal.SizeOf(result);
        return result;
      }
    }
  }

  public enum ShowWindowCommands
  {
    /// <summary>
    /// Hides the window and activates another window.
    /// </summary>
    Hide = 0,
    /// <summary>
    /// Activates and displays a window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when displaying the window
    /// for the first time.
    /// </summary>
    Normal = 1,
    /// <summary>
    /// Activates the window and displays it as a minimized window.
    /// </summary>
    ShowMinimized = 2,
    /// <summary>
    /// Maximizes the specified window.
    /// </summary>
    Maximize = 3, // is this the right value?
    /// <summary>
    /// Activates the window and displays it as a maximized window.
    /// </summary>      
    ShowMaximized = 3,
    /// <summary>
    /// Displays a window in its most recent size and position. This value
    /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
    /// the window is not activated.
    /// </summary>
    ShowNoActivate = 4,
    /// <summary>
    /// Activates the window and displays it in its current size and position.
    /// </summary>
    Show = 5,
    /// <summary>
    /// Minimizes the specified window and activates the next top-level
    /// window in the Z order.
    /// </summary>
    Minimize = 6,
    /// <summary>
    /// Displays the window as a minimized window. This value is similar to
    /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
    /// window is not activated.
    /// </summary>
    ShowMinNoActive = 7,
    /// <summary>
    /// Displays the window in its current size and position. This value is
    /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
    /// window is not activated.
    /// </summary>
    ShowNA = 8,
    /// <summary>
    /// Activates and displays the window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when restoring a minimized window.
    /// </summary>
    Restore = 9,
    /// <summary>
    /// Sets the show state based on the SW_* value specified in the
    /// STARTUPINFO structure passed to the CreateProcess function by the
    /// program that started the application.
    /// </summary>
    ShowDefault = 10,
    /// <summary>
    ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
    /// that owns the window is not responding. This flag should only be
    /// used when minimizing windows from a different thread.
    /// </summary>
    ForceMinimize = 11
  }


  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
      Left = left;
      Top = top;
      Right = right;
      Bottom = bottom;
    }

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
      get { return Left; }
      set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
      get { return Top; }
      set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
      get { return Bottom - Top; }
      set { Bottom = value + Top; }
    }

    public int Width
    {
      get { return Right - Left; }
      set { Right = value + Left; }
    }

    public System.Drawing.Point Location
    {
      get { return new System.Drawing.Point(Left, Top); }
      set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
      get { return new System.Drawing.Size(Width, Height); }
      set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r)
    {
      return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(System.Drawing.Rectangle r)
    {
      return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
      return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
      return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
      return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
      if (obj is RECT)
        return Equals((RECT)obj);
      else if (obj is System.Drawing.Rectangle)
        return Equals(new RECT((System.Drawing.Rectangle)obj));
      return false;
    }

    public override int GetHashCode()
    {
      return ((System.Drawing.Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
  }


  [StructLayout(LayoutKind.Sequential)]
  public struct POINT
  {
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public static implicit operator System.Drawing.Point(POINT p)
    {
      return new System.Drawing.Point(p.X, p.Y);
    }

    public static implicit operator POINT(System.Drawing.Point p)
    {
      return new POINT(p.X, p.Y);
    }
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0, CharSet = CharSet.Auto)]
  internal struct NMHDR
  {
    public IntPtr hwndFrom;
    public uint idFrom;
    public uint code;
  }//NMHDR

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0, CharSet = CharSet.Auto)]
  internal struct OFNOTIFY
  {
    //[MarshalAs(UnmanagedType.Struct)]
    public NMHDR hdr;
    public IntPtr lpOFN;
    public string pszFile;
  }//OFNOTIFY

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public class OpenFileName
  {
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;

    public string filter = null;
    public string customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;

    public string file = null;
    public int maxFile = 0;

    public string fileTitle = null;
    public int maxFileTitle = 0;

    public string initialDir = null;

    public string title = null;

    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;

    public string defExt = null;

    public IntPtr custData = IntPtr.Zero;
    public LPOFNHookProc hook;

    public string templateName = null;

    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
  }

  public delegate IntPtr LPOFNHookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
