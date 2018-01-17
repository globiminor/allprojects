using Basics;
using Basics.Views;
using OMapScratch;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OcadScratch.ViewModels
{
  public class MapVm : NotifyListener
  {
    private string _configPath;
    private Map _map;
    private BindingListView<WorkElemVm> _elems;
    private Basics.Geom.IProjection _globalPrj;
    private List<string> _symbolIds;
    private ConfigVm _configVm;

    public void Init(string scratchFile)
    {
      _elems = null;
      _globalPrj = null;
      _configPath = null;
      _symbolIds = null;

      XmlConfig config;
      Basics.Window.Browse.PortableDeviceUtils.Deserialize(scratchFile, out config);

      Map map = new Map();
      map.Load(scratchFile, config);

      _map = map;
      _configPath = scratchFile;
      ChangedAll();
    }

    public Map Map
    { get { return _map; } }
    public string ConfigPath
    {
      get { return _configPath; }
      set {
        _configPath = value;
        Changed();
        Changed(nameof(CanSave));
      }
    }
    
    public ConfigVm ConfigVm
    {
      get { return _configVm; }
      set
      {
        _configVm = value;
        Changed();
        Changed(nameof(CanSave));
      }
    }

    public bool CanSave
    {
      get { return _configVm != null; }
    }

    public Basics.Geom.IProjection GlobalPrj
    {
      get
      {
        if (_globalPrj == null)
        { _globalPrj = _map?.GetGlobalPrj(); }
        return _globalPrj;
      }
    }

    protected override void Disposing(bool disposing)
    { }

    public IList<string> SymbolIds
    {
      get
      {
        if (_symbolIds == null)
        {
          _symbolIds = new List<string>(Map.GetSymbols().Select(x => x.Id));
        }
        return _symbolIds;
      }
    }
    public IList<WorkElemVm> Elems
    {
      get
      {
        if (_map == null)
        { return null; }

        if (_elems == null)
        {
          BindingListView<WorkElemVm> elems = new BindingListView<WorkElemVm>();
          elems.AllowNew = false;
          elems.AllowRemove = false;
          foreach (Elem elem in _map.Elems)
          { elems.Add(new WorkElemVm(elem)); }
          _elems = elems;
          Changed();
        }
        return _elems;
      }
    }
  }
}
