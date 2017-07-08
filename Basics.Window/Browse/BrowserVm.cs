
using Basics.Views;
using System.Collections.Generic;
using System.IO;

namespace Basics.Window.Browse
{
  public class BrowserVm : BaseVm
  {
    private string _directory;
    private string _fileName;

    public BindingListView<ContentVm> Items { get; private set; }
    public BindingListView<ContentVm> Devices { get; private set; }

    public string DirectoryPath
    {
      get { return _directory; }
      set
      {
        if (TryLoadContent(value, Items))
        {
          _directory = value;
        }

        Changed(null);
      }
    }

    public string FileName
    {
      get { return _fileName; }
      set { SetValue(ref _fileName, value); }
    }

    public bool SetContentParent()
    {
      string parent = Path.GetDirectoryName(DirectoryPath);
      DirectoryPath = parent;
      return DirectoryPath == parent;
    }

    public bool SetContent(ContentVm content)
    {
      if (!TryLoadContent(content, Items))
      { return false; }

      _directory = content.FullPath;
      _fileName = null;
      Changed(nameof(DirectoryPath));
      Changed(nameof(FileName));
      return true;
    }

    public bool SetSelected(ContentVm content)
    {
      if (content == null)
      { return false; }

      FileName = Path.GetFileName(content.FullPath);
      return true;
    }


    public static BrowserVm Create(string parentPath)
    {
      if (Directory.Exists(parentPath))
      {
        BindingListView<ContentVm> items = new BindingListView<ContentVm>();
        items.AllowNew = false;
        items.AllowRemove = false;
        items.AllowEdit = false;

        BindingListView<ContentVm> devices = new BindingListView<ContentVm>();
        devices.AllowNew = false;
        devices.AllowRemove = false;
        devices.AllowEdit = false;

        LoadDevices(devices);
        TryLoadContent(parentPath, items);
        BrowserVm newContent = new BrowserVm { Devices = devices, Items = items, _directory = parentPath };

        return newContent;
      }
      return null;
    }

    private static void LoadDevices(BindingListView<ContentVm> devices)
    {
      foreach (DriveInfo drive in DriveInfo.GetDrives())
      {
        if (Directory.Exists(drive.Name))
        { devices.Add(new DriveContentVm { FullPath = drive.Name }); }
      }

      foreach (string device in PortableDeviceUtils.GetDevices())
      {
        { devices.Add(new DeviceContentVm { FullPath = device }); }
      }
    }

    private static bool TryLoadContent(string path, BindingListView<ContentVm> contents)
    {
      if (Directory.Exists(path))
      {
        return TryLoadContent(new DirContentVm { FullPath = path }, contents);
      }
      IList<string> parts = path.Split(Path.DirectorySeparatorChar);
      if (parts.Count <= 0)
      { return false; }
      if (parts.Count == 1)
      {
        return TryLoadContent(new DeviceContentVm { FullPath = path}, contents);
      }
      if (parts.Count > 1)
      {
        List<string> dirs = new List<string>(parts);
        dirs.RemoveAt(dirs.Count - 1);
        return TryLoadContent(new DevEntryContentVm(dirs, parts[parts.Count - 1]), contents);
      }
      return false;
    }

    private static bool TryLoadContent(ContentVm content, BindingListView<ContentVm> contents)
    {
      IEnumerable<ContentVm> enumContent = content?.GetContent();
      if (enumContent == null)
      { return false; }

      bool orig = contents.RaiseListChangedEvents;
      try
      {

        contents.RaiseListChangedEvents = false;
        contents.Clear();

        foreach (ContentVm child in enumContent)
        {
          contents.Add(child);
        }
      }
      finally
      { contents.RaiseListChangedEvents = orig; }
      contents.ResetBindings();

      return true;
    }

    public BrowserVm ChangeContent(ContentVm content)
    {
      if (File.Exists(content.FullPath))
      { return this; }

      return Create(content.FullPath);
    }
  }
}
