
namespace Basics.Geom
{
  public static class PointOperator
  {
    public static Point Add(IPoint point, IPoint add)
    {
      Point p = Point.CastOrCreate(point);
      Point a = Point.CastOrCreate(add);
      Point r = p + a;
      return r;
    }
    public static Point Sub(IPoint point, IPoint sub)
    {
      Point p = Point.CastOrCreate(point);
      Point a = Point.CastOrCreate(sub);
      Point r = p - a;
      return r;
    }

    public static Point Scale(IPoint point, double f)
    {
      Point p = Point.CastOrCreate(point);
      Point r = f*p;
      return r;
    }

    public static double SkalarProduct(IPoint point, IPoint mult)
    {
      Point p = Point.CastOrCreate(point);
      Point a = Point.CastOrCreate(mult);
      double r = p.SkalarProduct(a);
      return r;
    }
    public static double VectorProduct(IPoint point, IPoint mult)
    {
      Point p = Point.CastOrCreate(point);
      Point a = Point.CastOrCreate(mult);
      double r = p.VectorProduct(a);
      return r;      
    }

    public static double Dist2(IPoint point, IPoint dist)
    {
      Point p = Point.CastOrCreate(point);
      double r = p.Dist2(dist);
      return r;
    }

    public static double OrigDist2(IPoint point)
    {
      Point p = Point.CastOrCreate(point);
      double r = p.OrigDist2();
      return r;
    }

    public static double GetFactor(IPoint point, IPoint factor)
    {
      Point p = Point.CastOrCreate(point);
      Point a = Point.CastOrCreate(factor);
      double r = p.GetFactor(a);
      return r;
    }

  }
}
