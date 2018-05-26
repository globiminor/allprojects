

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Basics.Window
{
  public interface IDataContext<T>
  {
    T DataContext { get; }
  }

  public static class Utils
  {
    public static void SetBinding<T>(DataGridComboBoxColumn col, IDataContext<T> parent, Func<T, string> getProperty)
    {
      Type ancestorType = parent.GetType();
      string prop = getProperty(parent.DataContext);

      col.ElementStyle = new Style { TargetType = typeof(ComboBox) };
      col.ElementStyle.Setters.Add(new Setter(ComboBox.ItemsSourceProperty,
        new Binding
        {
          Path = new PropertyPath($"{nameof(parent.DataContext)}.{prop}"),
          RelativeSource = new RelativeSource { AncestorType = ancestorType }
        }));

      col.EditingElementStyle = new Style { TargetType = typeof(ComboBox) };
      col.EditingElementStyle.Setters.Add(new Setter(ComboBox.ItemsSourceProperty,
        new Binding
        {
          Path = new PropertyPath($"{nameof(parent.DataContext)}.{prop}"),
          RelativeSource = new RelativeSource { AncestorType = ancestorType }
        }));
    }

    public static void SetErrorBinding(DataGridTextColumn col, string property)
    {
      col.Binding = new Binding(property) { ValidatesOnDataErrors = true };

      ToolTip tooltip = new ToolTip();
      tooltip.IsVisibleChanged += (s, e) =>
      {
        string ttp = null;
        if (tooltip.DataContext is IDataErrorInfo errInfo)
        {
          ttp = errInfo[property];
        }
        if (string.IsNullOrWhiteSpace(ttp))
        { ttp = "No Error"; }
        tooltip.Content = ttp;
      };
      col.CellStyle = new Style() { TargetType = typeof(DataGridCell) };
      col.CellStyle.Setters.Add(new Setter(DataGridCell.ToolTipProperty, tooltip));
    }

    public static T GetParent<T>(DependencyObject obj) where T : Visual
    {
      DependencyObject parent = obj;
      while (parent != null && !(parent is T))
      {
        parent = VisualTreeHelper.GetParent(parent);
      }
      return (T)parent;
    }

    public static void AllowUIToUpdate()
    {
      DispatcherFrame frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
      {
        frame.Continue = false;
        return null;
      }), null);

      Dispatcher.PushFrame(frame);
      //EDIT:
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                    new Action(delegate { }));
    }

    public static IEnumerable<T> GetChildren<T>(DependencyObject parent) where T : Visual
    {
      if (parent == null)
      { yield break; }

      if (parent is T)
      { yield return (T)parent; }

      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < count; i++)
      {
        foreach (T child in GetChildren<T>(VisualTreeHelper.GetChild(parent, i)))
        {
          yield return child;
        }
      }
    }

    private static readonly Dictionary<string, ImageSource> _smallIconCache = new Dictionary<string, ImageSource>();
    private static readonly Dictionary<string, ImageSource> _largeIconCache = new Dictionary<string, ImageSource>();

    public static ImageSource GetIcon(string path, bool smallIcon, bool isDirectory)
    {
      if (string.IsNullOrWhiteSpace(path))
      { return null; }
      string key;

      if (isDirectory)
      { key = "dir"; }
      else
      { key = System.IO.Path.GetExtension(path) ?? string.Empty; }

      Dictionary<string, ImageSource> cache = smallIcon ? _smallIconCache : _largeIconCache;
      if (!cache.TryGetValue(key, out ImageSource icon))
      {
        icon = GetIconCore(path, smallIcon, isDirectory);
        cache.Add(key, icon);
      }
      return icon;
    }

    private static ImageSource GetIconCore(string path, bool smallIcon, bool isDirectory)
    {
      uint flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
      if (smallIcon)
        flags |= Shell32.ShgfiSmallicon;


      uint attributes = isDirectory ? Shell32.FileAttributeDirectory : Shell32.FileAttributeNormal;

      Shell32.Shfileinfo shfi = new Shell32.Shfileinfo();
      if (IntPtr.Zero != Shell32.SHGetFileInfo(path, attributes, ref shfi, (uint)Marshal.SizeOf(typeof(Shell32.Shfileinfo)), flags))
      {
        return Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      }
      return null;
    }

    /// <summary>
    /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles using SHGetFileInfo. Code
    /// courtesy of MSDN Cold Rooster Consulting case study.
    /// </summary>
    static class Shell32
    {
      public const int MaxPath = 256;
      [StructLayout(LayoutKind.Sequential)]
      public struct Shfileinfo
      {
        public const int Namesize = 80;
        public readonly IntPtr hIcon;
        public readonly int iIcon;
        public readonly uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
        public readonly string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
        public readonly string szTypeName;
      };
      public const uint ShgfiIcon = 0x000000100;     // get icon
      public const uint ShgfiLinkoverlay = 0x000008000;     // put a link overlay on icon
      public const uint ShgfiLargeicon = 0x000000000;     // get large icon
      public const uint ShgfiSmallicon = 0x000000001;     // get small icon
      public const uint ShgfiUsefileattributes = 0x000000010;     // use passed dwFileAttribute
      public const uint FileAttributeNormal = 0x00000080;
      public const uint FileAttributeDirectory = 0x00000010;

      [DllImport("Shell32.dll")]
      public static extern IntPtr SHGetFileInfo(
          string pszPath,
          uint dwFileAttributes,
          ref Shfileinfo psfi,
          uint cbFileInfo,
          uint uFlags
          );
    }
    /// <summary>
    /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
    /// </summary>
    static class User32
    {
      /// <summary>
      /// Provides access to function required to delete handle. This method is used internally
      /// and is not required to be called separately.
      /// </summary>
      /// <param name="hIcon">Pointer to icon handle.</param>
      /// <returns>N/A</returns>
      [DllImport("User32.dll")]
      public static extern int DestroyIcon(IntPtr hIcon);
    }
  }
}
