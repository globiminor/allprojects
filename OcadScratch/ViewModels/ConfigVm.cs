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

    public double Lat
    {
      get { return _config.Offset.Latitude; }
      set { _config.Offset.Latitude = value; }
    }

    public double Lon
    {
      get { return _config.Offset.Longitude; }
      set { _config.Offset.Longitude = value; }
    }

    public double Meridiankonvergenz
    {
      get { return _config.Offset.Meridiankonvergenz; }
      set { _config.Offset.Meridiankonvergenz = value; }
    }

    public double Declination
    {
      get { return _config.Offset.Declination; }
      set
      {
        _config.Offset.Declination = value;
        Changed();
      }
    }


    protected override void Disposing(bool disposing)
    { }

    public void Init(string ocdFile)
    {
      using (OcadReader r = OcadReader.Open(ocdFile))
      {
        Setup s = r.ReadSetup();
        Point offset = s?.PrjTrans;

        OffsetX = offset?.X ?? 0;
        OffsetY = offset?.Y ?? 0;

        foreach (StringParamIndex idx in r.ReadStringParamIndices())
        {
          if (idx.Type == StringType.ScalePar)
          {
            string p = r.ReadStringParam(idx);
            ScalePar par = new ScalePar(p);
            int? gridAndZone = par.GridAndZone;
            TransferProjection prj = null;
            if (gridAndZone == 14001)
            {
              prj = new TransferProjection(new Ch1903(), Geographic.Wgs84());
            }
            else if (gridAndZone == 14002)
            {
              prj = new TransferProjection(new Ch1903_LV95(), Geographic.Wgs84());
            }

            if (prj != null)
            {
              IPoint wgs84 = prj.Project(offset);
              Lon = wgs84.X;
              Lat = wgs84.Y;

              double dNorth = 100;
              Point declMap = new Point2D(offset.X, offset.Y + dNorth);
              IPoint declWgs84 = prj.Project(declMap);

              double dLat = declWgs84.Y - Lat;
              double dLon = declWgs84.X - Lon;
              double fLon = System.Math.Cos(Lat / 180 * System.Math.PI);
              Meridiankonvergenz = (fLon * dLon / dLat) * 180 / System.Math.PI;

              CalcDeclination();
            }
          }
        }
      }
      Changed();
    }
    public void CalcDeclination()
    {
      CmdCalcDeclination cmd = new CmdCalcDeclination(Lat, Lon);
      cmd.Execute();
      Declination = cmd.Declination ?? Declination;
    }
  }
}
