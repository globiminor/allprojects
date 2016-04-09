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
  }
}
