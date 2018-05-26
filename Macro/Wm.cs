
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

    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    public const int WM_SIZE = 0x0005;
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

      foreach (WindowPtr window in allWindows)
      {
        if (window.GetWindowText() == dialogTitle)
        {
          IList<WindowPtr> controls = Processor.GetChildWindows(window.HWnd);

          foreach (WindowPtr control in controls)
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
