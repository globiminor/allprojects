
using Basics.Views;
using System.Collections.Generic;
using System.IO;

namespace Basics.Window.Browse
{
  public class BrowserVm : BaseVm
  {
    private string _directory;
    private string _fileName;

    private ContentVm _selected;

    public BindingListView<ContentVm> Items { get; private set; }
    public BindingListView<ContentVm> Devices { get; private set; }

    public string Filter { get; set; }

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

    public ContentVm Selected
    {
      get { return _selected; }
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
      _selected = content;

      if (content == null)
      { return false; }

      FileName = Path.GetFileName(content.FullPath);
      return true;
    }

    public static BrowserVm Create(string parentPath, string filter = null)
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
        BrowserVm newContent = new BrowserVm { Devices = devices, Items = items, _directory = parentPath };
        newContent.Filter = filter;
        newContent.TryLoadContent(parentPath, items);

        return newContent;
      }
      return null;
    }

    private static void LoadDevices(BindingListView<ContentVm> devices)
    {
      foreach (var drive in DriveInfo.GetDrives())
      {
        if (Directory.Exists(drive.Name))
        { devices.Add(new DriveContentVm { FullPath = drive.Name }); }
      }

      foreach (var device in PortableDeviceUtils.GetDevices())
      {
        { devices.Add(new DeviceContentVm { FullPath = device }); }
      }
    }

    private bool TryLoadContent(string path, BindingListView<ContentVm> contents)
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
        return TryLoadContent(new DeviceContentVm { FullPath = path }, contents);
      }
      if (parts.Count > 1)
      {
        List<string> dirs = new List<string>(parts);
        dirs.RemoveAt(dirs.Count - 1);
        return TryLoadContent(new DevEntryContentVm(dirs, new PdEntry { Name = parts[parts.Count - 1] }), contents);
      }
      return false;
    }

    private bool TryLoadContent(ContentVm content, BindingListView<ContentVm> contents)
    {
      if (!content.IsDirectory)
      { return false; }

      IEnumerable<ContentVm> enumContent = content?.GetContent();
      if (enumContent == null)
      { return false; }

      List<ContentVm> toSort = new List<ContentVm>();
      foreach (var child in enumContent)
      {
        if (Filter != null && !child.IsDirectory && !child.Name.EndsWith(Filter, System.StringComparison.InvariantCultureIgnoreCase))
        { continue; }

        toSort.Add(child);
      }
      toSort.Sort((x, y) =>
      {
        if (x == y)
        { return 0; }

        int d = x.IsDirectory.CompareTo(y.IsDirectory);
        if (d != 0)
        { return d; }

        d = x.Name.CompareTo(y.Name);
        return d;
      });

      bool orig = contents.RaiseListChangedEvents;
      try
      {

        contents.RaiseListChangedEvents = false;
        contents.Clear();
        foreach (var contentVm in toSort)
        {
          contents.Add(contentVm);
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
