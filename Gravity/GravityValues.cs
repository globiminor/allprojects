
namespace Gravity
{
  public class GravityValues
  {
    public static double Gr = 6.6732;

    public bool IsInfinit { get; internal set; }
    public bool OnlyZ { get; set; }

    public double G { get; internal set; }
    public double Gx { get; internal set; }
    public double Gy { get; internal set; }
    public double Gz { get; internal set; }
    public double Gxx { get; internal set; }
    public double Gyy { get; internal set; }
    public double Gzz { get; internal set; }
    public double Gxy { get; internal set; }
    public double Gxz { get; internal set; }
    public double Gyz { get; internal set; }

    public GravityValues(bool onlyZ)
    {
      OnlyZ = onlyZ;
    }

    public void ApplyDensity(double rho)
    {
      Gz *= +Gr * rho * 1.0e-03;
      if (IsInfinit)
      {
        Gzz = 1.0e20;
      }
      else
      {
        Gzz *= -Gr * rho * 10.0;
      }
      if (!OnlyZ)
      {
        G *= -Gr * rho * 1.0e-08;
        Gx *= +Gr * rho * 1.0e-03;
        Gy *= +Gr * rho * 1.0e-03;
        if (IsInfinit)
        {
          Gxx = 1.0e20;
          Gyy = 1.0e20;
          Gxy = 1.0e20;
          Gxz = 1.0e20;
          Gyz = 1.0e20;
        }
        else
        {
          Gxx *= -Gr * rho * 10.0;
          Gyy *= -Gr * rho * 10.0;
          Gxy *= -Gr * rho * 10.0;
          Gxz *= -Gr * rho * 10.0;
          Gyz *= -Gr * rho * 10.0;
        }
      }
    }

    public GravityValues Add(GravityValues y)
    {
      GravityValues z = new GravityValues(OnlyZ);
      z.Gz = Gz + y.Gz;
      z.Gzz = Gzz + y.Gzz;
      if (!OnlyZ && !y.OnlyZ)
      {
        z.G = G + y.G;
        z.Gx = Gx + y.Gx;
        z.Gy = Gy + y.Gy;
        z.Gxx = Gxx + y.Gxx;
        z.Gyy = Gyy + y.Gyy;
        z.Gxy = Gxy + y.Gxy;
        z.Gxz = Gxz + y.Gxz;
        z.Gyz = Gyz + y.Gyz;
      }
      z.IsInfinit = IsInfinit | y.IsInfinit;
      z.OnlyZ = OnlyZ | y.OnlyZ;
      return z;
    }

  }
}
