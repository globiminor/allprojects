using Basics.Views;
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

    public void CalcDeclination()
    {
      CmdCalcDeclination cmd = new CmdCalcDeclination(Lat, Lon);
      cmd.Execute();
      Declination = cmd.Declination ?? Declination;
    }
  }
}
