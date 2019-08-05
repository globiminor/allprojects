using Basics.Views;
using System.IO;

namespace OCourse.ViewModels
{
  public class VeloModelVm : NotifyListener
  {
    protected override void Disposing(bool disposing)
    { }

    private string _mapName;
    public string MapName
    {
      get { return _mapName; }
      set
      {
        _mapName = value;
        Changed();
      }
    }

    public string Title { get; set; }

    public bool LoadSettings(string settingsFile)
    {
      if (!File.Exists(settingsFile))
      { return false; }

      return true;
    }
  }
}
