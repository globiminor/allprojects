using System;

namespace Basics.Geom.Projection
{
  public class Geographic : Projection
  {
    private static double _f = 180.0 / Math.PI;

    public override IPoint Gg2Prj(IPoint p)
    {
      return new Point2D(p.X * _f, p.Y * _f);
    }
    public override IPoint Prj2Gg(IPoint p)
    {
      return new Point2D(p.X / _f, p.Y / _f);
    }

    public static Geographic Wgs84()
    {
      Geographic wgs = new Geographic();
      Ellipsoid ell = new Ellipsoid.Wgs84();
      ell.Datum = new Datum.ITRS();
      ell.Datum.Center.X = -0;
      ell.Datum.Center.Y = 0;
      ell.Datum.Center.Z = 0.0;
      wgs.SetEllipsoid(ell);

      return wgs;
    }
  }
}
