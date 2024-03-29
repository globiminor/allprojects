using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public class RelOp
  {
    public RelOp Flip() { throw new NotImplementedException(); }
  }
  /// <summary>
  /// Summary description for Geometry.
  /// </summary>
  public abstract class Geometry : IGeometry
  {
    public class Comparer : IComparer<IGeometry>
    {
      #region IComparer<IGeometry> Members
      private readonly IList<int> _calcDimensions;

      public Comparer(IList<int> calcDimensions)
      {
        _calcDimensions = calcDimensions;
      }

      int IComparer<IGeometry>.Compare(IGeometry x, IGeometry y)
      { return Compare(x, y, _calcDimensions); }
      public static int Compare(IGeometry x, IGeometry y, IList<int> calcDimensions)
      {
        if (x == null)
        {
          if (y == null)
          { return 0; }
          else
          { return -1; }
        }
        else if (y == null)
        { return 1; }

        int i;
        i = x.Topology - y.Topology;
        if (i != 0)
        { return i; }

        foreach (var iDim in calcDimensions)
        {
          double d;
          d = x.Extent.Min[iDim] - y.Extent.Min[iDim];
          if (d != 0) { return Math.Sign(d); }
          d = x.Extent.Max[iDim] - y.Extent.Max[iDim];
          if (d != 0) { return Math.Sign(d); }
        }

        System.Collections.IEnumerable xParts = ((IMultipartGeometry)x).Subparts();
        System.Collections.IEnumerable yParts = ((IMultipartGeometry)y).Subparts();
        if (xParts == null)
        {
          if (yParts == null)
          { return (Compare(x.Border, y.Border, calcDimensions)); }
          else
          { return 1; }
        }
        else if (yParts == null)
        { return -1; }

        System.Collections.IList yList = yParts as System.Collections.IList;

        if (!(xParts is System.Collections.IList xList))
        {
          if (yList == null)
          { }
          else
          { return -1; }
        }
        else if (yList == null)
        { return 1; }
        else
        {
          i = xList.Count - yList.Count;
          if (i != 0)
          { return i; }
        }

        System.Collections.IEnumerator xEnum = xParts.GetEnumerator();
        System.Collections.IEnumerator yEnum = yParts.GetEnumerator();

        while (xEnum.MoveNext())
        {
          if (yEnum.MoveNext() == false)
          { return 1; }
          i = Compare(xEnum.Current as IGeometry, yEnum.Current as IGeometry, calcDimensions);
          if (i != 0)
          { return i; }
        }
        if (yEnum.MoveNext())
        { return -1; }

        return 0;
      }
      #endregion
    }


    private static IProjection _toXY;

    public virtual bool EqualGeometry(IGeometry other)
    { return this == other; }

    public GeometryCollection Intersection(IGeometry other)
    {
      return GeometryOperator.Intersection(this, other);
    }

    public abstract bool IsWithin(IPoint point);

    public bool Intersects(IGeometry other)
    {
      return GeometryOperator.Intersects(this, other);
    }

    public static bool IsMultipart(IGeometry geom)
    {
      return geom is IMultipartGeometry multi && multi.HasSubparts;
    }

    public abstract int Dimension { get; }
    public abstract int Topology { get; }
    //public abstract bool Intersects(IBox box, IList<int> calcDimensions);
    protected abstract IGeometry ProjectCore(IProjection projection);

    IGeometry IGeometry.Project(IProjection projection)
    { return ProjectCore(projection); }
    protected abstract IGeometry BorderGeom { get; }
    IGeometry IGeometry.Border
    { get { return BorderGeom; } }
    public IBox Extent => GetExtent();
    protected abstract IBox GetExtent();

    public static double epsi = 1e-12;
    public static double Precision = 1e-4;

    /// <summary>
    /// Cut two lines
    /// </summary>
    /// <param name="s0">start point of 1. line</param>
    /// <param name="dir0">direction of 1. line</param>
    /// <param name="s1">start point of 2. line</param>
    /// <param name="dir1">direction of 2. line</param>
    /// <param name="f">factor of 1. line or square distance between parallel lines</param>
    /// <returns>intersection point or null if directions are parallel</returns>
    public static Point2D CutDirDir(IPoint s0, IPoint dir0, IPoint s1, IPoint dir1,
      out double f)
    /*
      (x1)     (a1)     (x2)      (a2)     (x)
      (  ) + t*(  )  =  (  )  + v*(  ) ==> ( )
      (y1)     (b1)     (y2)      (b2)     (y)

      return : Point2D : lines cut each other at Point (non parallel)
               null    : lines are parallel
                         t = square of distance between the parallels
    */
    {
      if (s0.Dimension != 2)
      { throw new InvalidOperationException("Dimension != 2"); }

      Point p = PointOp.Sub(s1, s0);
      double det;

      det = PointOp.VectorProduct(dir0, dir1); // -dir0.X * dir1.Y + dir0.Y * dir1.X;
      if (Math.Abs(det) > epsi * (Math.Abs(s0.X) + Math.Abs(s1.X) +
        Math.Abs(s0.Y) + Math.Abs(s1.Y)) / 1000.0)
      {
        f = p.VectorProduct(dir1) / det;
        return (Point2D)(s0 + PointOp.Scale(f, dir0));
      }
      else
      {
        // parallel lines 
        det = p.VectorProduct(dir0);
        f = (det * det) / PointOp.OrigDist2(dir0);
        return null;
      }
    }

    public static Point2D OutCircleCenter(IPoint p0, IPoint p1, IPoint p2)
    {
      Point s0 = 0.5 * PointOp.Add(p0, p1);
      Point dir0 = PointOp.Sub(p1, p0);
      Point s1 = 0.5 * PointOp.Add(p1, p2);
      Point dir1 = PointOp.Sub(p2, p1);
      return CutDirDir(s0, new Point2D(dir0.Y, -dir0.X), s1,
        new Point2D(dir1.Y, -dir1.X), out double f);
    }
    public static double TriArea(IPoint p0, IPoint p1, IPoint p2)
    {
      Point d0 = PointOp.Sub(p1, p0);
      Point d1 = PointOp.Sub(p2, p0);
      return d0.VectorProduct(d1);
    }

    public static IProjection ToXY
    {
      get
      {
        if (_toXY == null)
        { _toXY = new Projection.ToXY(); }
        return _toXY;
      }
    }
  }
}
