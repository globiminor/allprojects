
using System;
using System.Collections.Generic;
using System.IO;

namespace Basics.Window.Browse
{
  public abstract class ContentVm
  {
    public string FullPath { get; set; }
    public abstract string Name { get; }
    public abstract bool IsDirectory { get; }

    public virtual bool Read(Stream stream)
    {
      return false;
    }

    public IEnumerable<ContentVm> GetContent()
    {
      return GetContentCore();
    }

    protected abstract IEnumerable<ContentVm> GetContentCore();

    protected static IEnumerable<ContentVm> GetContent(string directory)
    {
      foreach (string path in Directory.GetDirectories(directory))
      {
        yield return new DirContentVm { FullPath = path };
      }

      foreach (string path in Directory.GetFiles(directory))
      {
        yield return new FileContentVm { FullPath = path };
      }
    }
  }

  public class DriveContentVm : ContentVm
  {
    public override string Name { get { return FullPath; } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      return GetContent(FullPath);
    }
    public override bool IsDirectory { get { return true; } }
  }

  public class DeviceContentVm : ContentVm
  {
    public override string Name { get { return FullPath; } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      IList<string> dir = new[] { FullPath };
      foreach (PdEntry entry in PortableDeviceUtils.GetContent(dir))
      {
        yield return new DevEntryContentVm(dir, entry);
      }
    }
    public override bool IsDirectory { get { return true; } }
  }

  class DevEntryContentVm : ContentVm
  {
    private readonly List<string> _path;
    private readonly PdEntry _entry;
    public DevEntryContentVm(IList<string> directory, PdEntry entry)
    {
      _path = new List<string>(directory)
      { entry.Name };
      _entry = entry;

      string full = string.Empty;
      foreach (string dir in directory)
      {
        full = Path.Combine(full, dir);
      }
      FullPath = Path.Combine(full, entry.Name);
    }
    public override string Name { get { return _path[_path.Count - 1]; } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      foreach (PdEntry entry in PortableDeviceUtils.GetContent(_path))
      {
        yield return new DevEntryContentVm(_path, entry);
      }
    }
    public override bool IsDirectory { get { return string.IsNullOrWhiteSpace(Path.GetExtension(FullPath)); } }

    public override bool Read(Stream stream)
    {
      if (IsDirectory)
      { return false; }

      PortableDeviceUtils.TransferContent(_path[0], _entry.Id, stream);
      return true;
    }
  }

  public class FileContentVm : ContentVm
  {
    public override string Name { get { return Path.GetFileName(FullPath); } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      return null;
    }
    public override bool IsDirectory { get { return false; } }
  }

  public class DirContentVm : ContentVm
  {
    public override string Name { get { return Path.GetFileName(FullPath); } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      return GetContent(FullPath);
    }
    public override bool IsDirectory { get { return true; } }
  }
}
