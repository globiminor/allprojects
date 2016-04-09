
using Basics.Geom;
using Basics.Views;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Basics;

namespace Grid.Lcp
{
  public interface ICostProvider
  {
    LeastCostPathBase Build(IBox box, double dx, Steps step, string velocityData);
    string FileFilter { get; }
  }

  public class ConfigVm : NotifyListener
  {
    private const string _dhmExt = "*.asc";
    private const string _veloExt = "*.tif";
    private const double _resol = 4.0;

    private readonly List<Steps> _stepsModes =
      new List<Steps> { Steps.Step16, Steps.Step8, Steps.Step4 };

    private string _heightPath;
    private string _veloPath;
    private double _resolution;
    private Steps _steps;
    private ICostProvider _costProvider;
    private DoubleGrid _grdHeight;

    public ConfigVm()
    {
      HeightPath = _dhmExt;
      VeloPath = _veloExt;
      Resolution = _resol;
      StepsMode = _stepsModes[0];

      SetCostProviderType(new VelocityCostProvider());
    }

    protected override void Disposing(bool disposing)
    { }

    public string HeightPath
    {
      get { return _heightPath; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        {
          _heightPath = value;
          _grdHeight = null;
        }
      }
    }
    public string VeloPath
    {
      get { return _veloPath; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        { _veloPath = value; }
      }
    }
    public double Resolution
    {
      get { return _resolution; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
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
        using (Changing(MethodBase.GetCurrentMethod()))
        { _steps = value; }
      }
    }
    public string CostProviderName { get { return CostProvider != null ? CostProvider.GetType().Name : null; } }
    public ICostProvider CostProvider
    {
      get { return _costProvider; }
    }

    public DoubleGrid HeightGrid
    {
      get
      {
        if (_grdHeight == null)
        {
          if (File.Exists(HeightPath))
          {
            _grdHeight = DoubleGrid.FromAsciiFile(HeightPath, 0, 0.01, typeof(double));
          }
        }
        return _grdHeight;
      }
    }

    public IEnumerable<Steps> StepsModes
    {
      get
      {
        foreach (Steps step in _stepsModes)
        { yield return step; }
      }
    }

    public void SetCostProviderType(ICostProvider costProvider)
    {
      using (Changing(this.GetPropertyName(x => x.CostProviderName)))
      { _costProvider = costProvider; }
    }

    public void SetConfig(string dir, Config config)
    {
      if (config == null) return;

      using (Changing())
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
