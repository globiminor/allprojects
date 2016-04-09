using System;

namespace Basics.Geom
{
  public static class Units
  {
    private static double _arcGrad = Math.PI/180.0;

    public static void Rad2Gms(double rad, out int h, out int m, out double s)
    {
      double t;

      t = rad / Math.PI * 180.0;
      h = (int)t;
      t = Math.Abs(60.0 * (t - h));
      m = (int)t;
      s = 60.0 * (t - m);
    }
    public static double Gms2Rad(int h, int m, double s)
    {
      if (h >= 0)
      { return Math.PI / 180.0 * (Math.Abs(h) + (m + s / 60.0) / 60.0); }
      else
      { return -Math.PI / 180.0 * (Math.Abs(h) + (m + s / 60.0) / 60.0); }
    }

    public static double ARC_GRAD
    {
      get { return _arcGrad; }
    }
  }
}
