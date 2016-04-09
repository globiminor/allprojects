
namespace Ocad.StringParams
{
  /// <summary>
  /// Summary description for Template.
  /// </summary>
  public class TemplatePar : MultiParam
  {
    public const char OmegaKey = 'a';
    public const char PhiKey = 'b';
    public const char DimKey = 'd';
    public const char TransparentKey = 't';
    public const char X0Key = 'x';
    public const char Y0Key = 'y';
    public const char DxKey = 'u';
    public const char DyKey = 'v';
    public const char VisibleKey = 's';

    public TemplatePar()
    { }

    public TemplatePar(string stringParam)
      : base(stringParam)
    { }

    public double Omega
    {
      get { return GetDouble(OmegaKey); }
      set { SetParam(OmegaKey, value); }
    }

    public double Phi
    {
      get { return GetDouble(PhiKey); }
      set { SetParam(PhiKey, value); }
    }

    public double Dim
    {
      get { return GetDouble(DimKey); }
      set { SetParam(DimKey, value); }
    }

    public bool Transparent
    {
      get { return GetInt(TransparentKey) != 0; }
      set { SetParam(TransparentKey, value ? 1 : 0); }
    }
    public double X0
    {
      get { return GetDouble(X0Key); }
      set { SetParam(X0Key, value); }
    }
    public double Y0
    {
      get { return GetDouble(Y0Key); }
      set { SetParam(Y0Key, value); }
    }
    public double Dx
    {
      get { return GetDouble(DxKey); }
      set { SetParam(DxKey, value); }
    }
    public double Dy
    {
      get { return GetDouble(DyKey); }
      set { SetParam(DyKey, value); }
    }
    public bool Visible
    {
      get { return GetInt(VisibleKey) != 0; }
      set
      {
        SetParam('r', value ? 1 : 0);
        SetParam('s', value ? 1 : 0);
      }
    }

    public void Georeference(double dxx, double dxy, double dyx, double dyy,
      double xMin, double yMax, int nx, int ny, Setup setup)
    {
      double df = setup.Scale / 1000.0;
      Dx = dxx / df;
      Dy = -dyy / df;
      X0 = (xMin + nx / 2.0 * dxx - setup.PrjTrans.X) / df;
      Y0 = (yMax + ny / 2.0 * dyy - setup.PrjTrans.Y) / df;
    }
  }
}
