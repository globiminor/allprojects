
using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public static class PointOperator
  {
    public static double VectorProduct(IPoint p0, IPoint p1)
    {
      double v = p0.X * p1.Y - p0.Y * p1.X;
      return v;
    }

    public static double OrigDist2(IPoint p0, IEnumerable<int> dimensions = null) => Dist2(p0, null, dimensions);
    public static double Dist2(IPoint p0, IPoint p1 = null, IEnumerable<int> dimensions = null)
    {
      IEnumerable<int> dims = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
      double dDist2 = 0;
      foreach (int i in dims)
      {
        double d = p0[i] - (p1?[i] ?? 0);
        dDist2 += d * d;
      }
      return dDist2;
    }

    public static Point Add(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
      List<double> sum = new List<double>();
      foreach (var dim in dimensions)
      { sum.Add(p0[dim] + p1[dim]); }

      return Point.Create(sum.ToArray());
    }
    public static Point Sub(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
      List<double> diff = new List<double>();
      foreach (var dim in dimensions)
      { diff.Add(p0[dim] - p1[dim]); }

      return Point.Create(diff.ToArray());
    }
    public static Point Neg(IPoint p0, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(p0);
      List<double> neg = new List<double>();
      foreach (var dim in dimensions)
      { neg.Add(-p0[dim]); }

      return Point.Create(neg.ToArray());
    }

    public static Point Scale(double f, IPoint p, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(p);
      List<double> scaled = new List<double>();
      foreach (var dim in dimensions)
      { scaled.Add(f * p[dim]); }

      return Point.Create(scaled.ToArray());
    }

    public static double SkalarProduct(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
      double prod = 0;
      foreach (var dim in dimensions)
      { prod += p0[dim] * p1[dim]; }
      return prod;
    }

    /// <summary>
    /// get factor f of skalar product p0 = f * p1
    /// </summary>
    /// <param name="p0">result of scalar product</param>
    /// <returns>factor</returns>
    public static double GetFactor(IPoint p, IPoint p0, IEnumerable<int> dimensions = null)
    {
      double dMin = 0;
      double f = 0;
      foreach (var dim in dimensions)
      {
        if (Math.Abs(p[dim]) > dMin)
        {
          f = p0[dim] / p[dim];
          dMin = Math.Abs(p[dim]);
        }
      }

      return f;
    }
  }
}
