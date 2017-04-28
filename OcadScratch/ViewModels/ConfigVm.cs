using Basics.Geom;
using Basics.Geom.Projection;
using Basics.Views;
using Ocad;
using Ocad.StringParams;
using OcadScratch.Commands;
using OMapScratch;
using System.Collections.Generic;
using System.Linq;

namespace OcadScratch.ViewModels
{
  public class ConfigVm : NotifyListener
  {
    private readonly XmlConfig _config;
    private readonly List<ProjectionVm> _projections;
    private ProjectionVm _prj;
    private Dictionary<int, ProjectionVm> _projectionDict;

    public ConfigVm()
    {
      _config = new XmlConfig();
      _config.Offset = new XmlOffset();

      _projections = new List<ProjectionVm>
      {
        null,
        new ProjectionVm("CH1903", new Ch1903()) { OcadId = 14001 },
        new ProjectionVm("CH1903 LV95", new Ch1903_LV95()) { OcadId = 14002 },
      };
    }

    public ConfigVm(XmlConfig config)
    { _config = config; }

    public double OffsetX
    {
      get { return _config.Offset.X; }
      set { _config.Offset.X = value; }
    }

    public double OffsetY
    {
      get { return _config.Offset.Y; }
      set { _config.Offset.Y = value; }
    }

    public double? Lat
    {
      get { return _config.Offset.World?.Latitude; }
      set { _config.Offset.GetWorld().Latitude = value ?? 0; }
    }

    public double? Lon
    {
      get { return _config.Offset.World?.Longitude; }
      set { _config.Offset.GetWorld().Longitude = value ?? 0; }
    }

    public double? GeoMatrix00
    {
      get { return _config.Offset.World?.GeoMatrix00; }
      set { _config.Offset.GetWorld().GeoMatrix00 = value ?? 0; }
    }

    public double? GeoMatrix01
    {
      get { return _config.Offset.World?.GeoMatrix01; }
      set { _config.Offset.GetWorld().GeoMatrix01 = value ?? 0; }
    }

    public double? GeoMatrix10
    {
      get { return _config.Offset.World?.GeoMatrix10; }
      set { _config.Offset.GetWorld().GeoMatrix10 = value ?? 0; }
    }

    public double? GeoMatrix11
    {
      get { return _config.Offset.World?.GeoMatrix11; }
      set { _config.Offset.GetWorld().GeoMatrix11 = value ?? 0; }
    }

    public double? Declination
    {
      get { return _config.Offset.Declination; }
      set
      {
        _config.Offset.Declination = value;
        ChangedAll();
      }
    }

    public ProjectionVm Projection
    {
      get { return _prj; }
      set
      {
        _prj = value;
        ChangedAll();
      }
    }

    public bool CanGeo2Map
    { get { return _prj != null && _config.Offset.World != null; } }

    public bool CanMap2Geo
    { get { return _prj != null; } }

    public bool CanDeclination
    {
      get { return _prj != null || _config.Offset.World != null; }
    }

    private Dictionary<int, ProjectionVm> ProjectionDict
    {
      get
      {
        return _projectionDict ??
          (_projectionDict = _projections.Where(x => x != null).ToDictionary(x => x.OcadId));
      }
    }

    public List<ProjectionVm> Projections
    {
      get { return _projections; }
    }

    protected override void Disposing(bool disposing)
    { }

    public void Save(string configFile)
    {
      if (_config == null)
      { return; }
      using (System.IO.TextWriter w = new System.IO.StreamWriter(configFile))
      {
        Serializer.Serialize(_config, w);
      }
    }

    public void Init(string ocdFile)
    {
      int? gridAndZone = null;
      Point offset;
      using (OcadReader r = OcadReader.Open(ocdFile))
      {
        Setup s = r.ReadSetup();
        offset = s?.PrjTrans;

        OffsetX = offset?.X ?? 0;
        OffsetY = offset?.Y ?? 0;

        foreach (StringParamIndex idx in r.ReadStringParamIndices())
        {
          if (idx.Type == StringType.ScalePar)
          {
            string p = r.ReadStringParam(idx);
            ScalePar par = new ScalePar(p);
            gridAndZone = par.GridAndZone;
          }
        }
      }

      ProjectionVm prj;
      if (gridAndZone.HasValue && ProjectionDict.TryGetValue(gridAndZone.Value, out prj))
      {
        Projection = prj;
      }

      CalcWgs84();
      CalcDeclination();

      ChangedAll();
    }

    public void CalcMapCoord()
    {
      ProjectionVm prj = Projection;
      if (prj == null)
      { return; }

      XmlWorld w = _config.Offset.World;
      if (w == null)
      { return; }

      IPoint map = prj.Geo2Map.Project(new Point2D(w.Longitude, w.Latitude));
      OffsetX = map.X;
      OffsetY = map.Y;

      ChangedAll();
    }

    public void CalcWgs84()
    {
      ProjectionVm prj = Projection;
      if (prj == null)
      { return; }

      IPoint offset = new Point2D(_config.Offset.X, _config.Offset.Y);

      IPoint wgs84 = prj.Map2Geo.Project(offset);
      Lon = wgs84.X;
      Lat = wgs84.Y;

      double d = 0.001;
      IPoint dEast = prj.Geo2Map.Project(new Point2D(wgs84.X + d, wgs84.Y));
      IPoint dNorth = prj.Geo2Map.Project(new Point2D(wgs84.X, wgs84.Y + d));

      XmlWorld w = _config.Offset.GetWorld();

      w.GeoMatrix00 = (dEast.X - offset.X) / d;
      w.GeoMatrix01 = (dEast.Y - offset.Y) / d;
      w.GeoMatrix10 = (dNorth.X - offset.X) / d;
      w.GeoMatrix11 = (dNorth.Y - offset.Y) / d;

      //IPoint test = new Point2D(offset.X + 80, offset.Y + 30);
      //IPoint rr = prj.Project(test);

      //double xt = (rr.X - wgs84.X) * w.GeoMatrix00 + (rr.Y - wgs84.Y) * w.GeoMatrix10;
      //double yt = (rr.X - wgs84.X) * w.GeoMatrix01 + (rr.Y - wgs84.Y) * w.GeoMatrix11;

      ChangedAll();
    }
    public void CalcDeclination()
    {
      double lat;
      double lon;
      if (Lat.HasValue && Lon.HasValue)
      {
        lat = Lat.Value;
        lon = Lon.Value;
      }
      else if (_prj != null)
      {
        IPoint wgs = _prj.Map2Geo.Project(new Point2D(OffsetX, OffsetY));
        lat = wgs.Y;
        lon = wgs.X;
      }
      else
      { return; }

      CmdCalcDeclination cmd = new CmdCalcDeclination(lat, lon);
      cmd.Execute();
      Declination = cmd.Declination ?? Declination;
    }
  }
}
