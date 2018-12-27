using Basics.Views;
using System.Collections.Generic;
using System.IO;

namespace Grid.Lcp
{
  public class ConfigVm : NotifyListener
  {
    private enum ConfigMode { Unknown, TerrainVeloModel, MultiLayerModel };

    private const string _dhmExt = "*.asc";
    private const string _veloExt = "*.tif";
    private const double _resol = 4.0;

    private readonly List<Steps> _stepsModes =
      new List<Steps> { Steps.Step16, Steps.Step8, Steps.Step4 };

    private ConfigMode _configMode;
    private string _heightPath;
    private string _veloPath;
    private string _teleportPath;
    private double _resolution;
    private Steps _steps;
    private IDoubleGrid _grdHeight;

    private BindingListView<ConfigVm> _modelLevels;
    public BindingListView<ConfigVm> ModelLevels => _modelLevels ?? (_modelLevels = new BindingListView<ConfigVm>());

    public ConfigVm()
    {
      HeightPath = _dhmExt;
      VeloPath = _veloExt;
      Resolution = _resol;
      StepsMode = _stepsModes[0];

      _configMode = ConfigMode.TerrainVeloModel;
    }

    protected override void Disposing(bool disposing)
    { }

    public bool TerrainVeloModel
    {
      get { return _configMode == ConfigMode.TerrainVeloModel; }
      set
      {
        if (value)
        {
          _configMode = ConfigMode.TerrainVeloModel;
        }
        else if (_configMode == ConfigMode.TerrainVeloModel)
        {
          MultiLayerModel = true;
        }
        ChangedAll();
      }
    }

    public bool MultiLayerModel
    {
      get { return _configMode == ConfigMode.MultiLayerModel; }
      set
      {
        if (value)
        {
          _configMode = ConfigMode.MultiLayerModel;
        }
        else if (_configMode == ConfigMode.MultiLayerModel)
        {
          TerrainVeloModel = true;
        }
        ChangedAll();
      }
    }

    public ConfigVm AddLevel()
    {
      ConfigVm vm = new ConfigVm { Resolution = Resolution, StepsMode = StepsMode };
      ModelLevels.Add(vm);
      return vm;
    }

    public string HeightPath
    {
      get { return _heightPath; }
      set
      {
        using (ChangingAll())
        {
          _heightPath = value;
          _grdHeight = null;
          TerrainVeloModel = true;
        }
      }
    }
    public string VeloPath
    {
      get { return _veloPath; }
      set
      {
        using (Changing())
        { _veloPath = value; }
      }
    }
    public string VeloFileFilter
    { get { return $"{_veloExt} | {_veloExt}"; } }

    public string TeleportPath
    {
      get { return _teleportPath; }
      set
      {
        using (Changing())
        { _teleportPath = value; }
      }
    }


    public double Resolution
    {
      get { return _resolution; }
      set
      {
        using (Changing())
        {
          if (value > 0)
          { _resolution = value; }
        }
      }
    }
    public Steps StepsMode
    {
      get { return _steps; }
      set
      {
        using (Changing())
        { _steps = value; }
      }
    }

    private ITvmCalc _tvmCalc = new TvmCalc();
    public ITvmCalc TvmCalc
    {
      get { return _tvmCalc; }
      set
      {
        using (Changing())
        { _tvmCalc = value; }
      }
    }
    public string CostTypeName { get { return _tvmCalc?.GetType().Name; } }

    public IDoubleGrid HeightGrid
    {
      get
      {
        if (_grdHeight == null)
        {
          if (File.Exists(HeightPath))
          {
            _grdHeight = DataDoubleGrid.FromAsciiFile(HeightPath, 0, 0.01, typeof(double));
          }
        }
        return _grdHeight;
      }
    }

    public IEnumerable<Steps> StepsModes
    {
      get
      {
        foreach (var step in _stepsModes)
        { yield return step; }
      }
    }

    public void SetConfig(string dir, Config config)
    {
      if (config == null) return;

      using (ChangingAll())
      {
        bool setResol = false;
        if (!File.Exists(HeightPath) && !string.IsNullOrEmpty(config.Dhm))
        {
          _heightPath = Config.GetFullPath(dir, config.Dhm);
          setResol = true;
        }
        if (!File.Exists(VeloPath) && !string.IsNullOrEmpty(config.Velo))
        {
          _veloPath = Config.GetFullPath(dir, config.Velo);
          setResol = true;
        }
        if (setResol && config.Resolution > 0)
        { _resolution = config.Resolution; }
      }
    }
  }
}
