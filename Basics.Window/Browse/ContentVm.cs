
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
      foreach (string entry in PortableDeviceUtils.GetContent(dir))
      {
        yield return new DevEntryContentVm(dir, entry);
      }
    }
    public override bool IsDirectory { get { return true; } }
  }

  class DevEntryContentVm : ContentVm
  {
    private readonly List<string> _path;
    public DevEntryContentVm(IList<string> directory, string name)
    {
      _path = new List<string>(directory);
      _path.Add(name);

      string full = string.Empty;
      foreach (string dir in directory)
      {
        full = Path.Combine(full, dir);
      }
      FullPath = Path.Combine(full, name);
    }
    public override string Name { get { return _path[_path.Count - 1]; } }
    protected override IEnumerable<ContentVm> GetContentCore()
    {
      foreach (string entry in PortableDeviceUtils.GetContent(_path))
      {
        yield return new DevEntryContentVm(_path, entry);
      }
    }
    public override bool IsDirectory { get { return string.IsNullOrWhiteSpace(Path.GetExtension(FullPath)); } }
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
