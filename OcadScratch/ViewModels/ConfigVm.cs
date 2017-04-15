using Basics.Geom;
using Basics.Geom.Projection;
using Basics.Views;
using Ocad;
using Ocad.StringParams;
using OcadScratch.Commands;
using OMapScratch;

namespace OcadScratch.ViewModels
{
  public class ConfigVm : NotifyListener
  {
    private readonly XmlConfig _config;
    public ConfigVm()
    {
      _config = new XmlConfig();
      _config.Offset = new XmlOffset();
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
      get { return _config.Offset.World?.Declination; }
      set
      {
        _config.Offset.GetWorld().Declination = value;
        Changed();
      }
    }

    private int? _gridAndZone;

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

      _gridAndZone = gridAndZone;

      CalcWgs84();
    }

    public void CalcWgs84()
    {
      IPoint offset = new Point2D(_config.Offset.X, _config.Offset.Y);
      int? gridAndZone = _gridAndZone;

      Projection map = null;
      if (gridAndZone == 14001)
      {
        map = new Ch1903();
      }
      else if (gridAndZone == 14002)
      {
        map = new Ch1903_LV95();
      }

      if (map != null)
      {
        TransferProjection prj = new TransferProjection(map, Geographic.Wgs84());

        IPoint wgs84 = prj.Project(offset);
        Lon = wgs84.X;
        Lat = wgs84.Y;

        TransferProjection invPrj = new TransferProjection(Geographic.Wgs84(), map);

        double d = 0.001;
        IPoint dEast = invPrj.Project(new Point2D(wgs84.X + d, wgs84.Y));
        IPoint dNorth = invPrj.Project(new Point2D(wgs84.X, wgs84.Y + d));

        XmlWorld w = _config.Offset.GetWorld();

        w.GeoMatrix00 = (dEast.X - offset.X) / d;
        w.GeoMatrix01 = (dEast.Y - offset.Y) / d;
        w.GeoMatrix10 = (dNorth.X - offset.X) / d;
        w.GeoMatrix11 = (dNorth.Y - offset.Y) / d;

        //IPoint test = new Point2D(offset.X + 80, offset.Y + 30);
        //IPoint rr = prj.Project(test);

        //double xt = (rr.X - wgs84.X) * w.GeoMatrix00 + (rr.Y - wgs84.Y) * w.GeoMatrix10;
        //double yt = (rr.X - wgs84.X) * w.GeoMatrix01 + (rr.Y - wgs84.Y) * w.GeoMatrix11;

        CalcDeclination();
      }

      Changed();
    }
    public void CalcDeclination()
    {
      if (!Lat.HasValue || !Lon.HasValue)
      { return; }

      CmdCalcDeclination cmd = new CmdCalcDeclination(Lat.Value, Lon.Value);
      cmd.Execute();
      Declination = cmd.Declination ?? Declination;
    }
  }
}
