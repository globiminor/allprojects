using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Basics.Geom
{
  public interface ICurve : ITanParamGeometry
  {
    ICurve Invert();
    IEnumerable<ISegment> Segments { get; }
  }

  public interface ISegment : IGeometry, IParamGeometry
  {
    IPoint Start { get; }
    IPoint End { get; }

    double Length(IReadOnlyCollection<int> dimensions = null);
    double ParamAt(double distance, IReadOnlyCollection<int> dimensions = null);
    IPoint PointAt(double param);
    IPoint TangentAt(double param);
    new ISegment Project(IProjection prj);
    ISegment Subpart(double t0, double t1);

    ISegment Clone();
    ISegment Invert();
    IInnerSegment GetInnerSegment();
    IList<IPoint> Linearize(double maxOffset, IReadOnlyCollection<int> dimensions = null, bool includeFirstPoint = true);
  }
  public interface IMultipartSegment : IMultipartGeometry
  {
    double Parameter(ParamGeometryRelation split);
  }
  public interface ISegment<T> : ISegment
    where T: ISegment
  {
    new T Subpart(double t0, double t1);
  }

  public abstract class Curve : Geometry, ISegment, ICurve, IRelTanParamGeometry
  {
    public class GeometryEquality : IEqualityComparer<ISegment>
    {
      public bool Equals(ISegment x, ISegment y)
      {
        return x.EqualGeometry(y);
      }
      public int GetHashCode(ISegment x)
      {
        return x.Start.X.GetHashCode();
      }
    }
    private static readonly Box _paramBox;

    static Curve()
    {
      Point min = Point.Create_0(1);
      Point max = Point.Create_0(1);
      min[0] = 0;
      max[0] = 1;
      _paramBox = new Box(min, max);
    }

    IEnumerable<ISegment> ICurve.Segments
    { get { yield return this; } }

    public enum CurveAtType
    { Parameter, Distance }

    IInnerSegment ISegment.GetInnerSegment() => GetInnerCurve();
    public abstract InnerCurve GetInnerCurve();

    public override abstract bool EqualGeometry(IGeometry other);

    public IList<IPoint> Linearize(double offset, IReadOnlyCollection<int> dimensions = null, bool includeFirstPoint = true) 
      => LinearizeCore(offset, dimensions, includeFirstPoint);
    protected virtual IList<IPoint> LinearizeCore(double offset, IReadOnlyCollection<int> dimensions, bool includeFirstPoint)
    {
      List<IPoint> points = new List<IPoint>();
      if (includeFirstPoint)
      {
        points.Add(Start);
      }
      if (!IsLinear)
      {
        double d2 = offset * offset;

        IRelationGeometry paramGeom = this;
        double normed = paramGeom.NormedMaxOffset;
        double l2 = PointOp.Sub(End, Start).OrigDist2(dimensions);
        if (normed * normed * l2 > d2)
        {
          Curve c0 = Subpart(0, 0.5);
          Curve c1 = Subpart(0.5, 1);
          points.AddRange(c0.Linearize(offset, dimensions, false));
          points.AddRange(c1.Linearize(offset, dimensions, false));
        }
        else
        {
          points.Add(End);
        }
      }
      else
      {
        points.Add(End);
      }
      return points;
    }


    public abstract IPoint Start { get; }
    public abstract IPoint End { get; }

    public Curve Clone() => CloneCore();
    ISegment ISegment.Clone() => CloneCore();
    protected abstract Curve CloneCore();

    IPoint ISegment.PointAt(double param) => PointAt(param);
    public abstract Point PointAt(double param);

    IPoint ISegment.TangentAt(double param) => TangentAt(param);
    public abstract Point TangentAt(double param);

    public abstract double ParamAt(double distance, IReadOnlyCollection<int> dimensions = null);
    public abstract double Length(IReadOnlyCollection<int> dimensions = null);

    ISegment ISegment.Subpart(double t0, double t1) => Subpart(t0, t1);
    public Curve Subpart(double t0, double t1) => SubpartCurve(t0, t1);
    protected abstract Curve SubpartCurve(double t0, double t1);

    IBox IParamGeometry.ParameterRange
    { get { return _paramBox; } }

    Relation IRelationGeometry.GetRelation(IBox paramRange)
    {
      double min = paramRange.Min[0];
      double max = paramRange.Max[0];
      if (min > 1 || max < 0)
      { return Relation.Disjoint; }
      if (min < 0 || max > 1)
      { return Relation.Near; }
      else
      { return Relation.Intersect; }
    }

    public virtual IEnumerable<ParamGeometryRelation> CreateRelations(IParamGeometry other,
      TrackOperatorProgress trackProgress)
    {
      if (other is Curve o)
      {
        return o.CreateRelations(this, trackProgress);
      }
      else
      {
        return GeometryOperator.CreateRelations(this, other, trackProgress, calcLinearized: true);
      }
    }
    protected virtual IEnumerable<ParamGeometryRelation> CreateRelations(Curve other, TrackOperatorProgress trackProgress)
    {
      // falls diese Methode ausgefuhert wird existiert keine ueberschriebene Methode dazu 
      // und die Relation muss via Linearisierung berechnet werden.
      return GeometryOperator.CreateRelations(this, other, trackProgress, calcLinearized: true);
    }
    protected IList<ParamGeometryRelation> AddRelations(
      IList<ParamGeometryRelation> list, TrackOperatorProgress trackProgress)
    {
      if (list == null)
      { return null; }
      IList<ParamGeometryRelation> result = null;

      foreach (var rel in list)
      {
        if (rel.Intersection == null)
        { continue; }

        if (result == null)
        { result = new List<ParamGeometryRelation>(); }
        result.Add(rel);

        if (trackProgress != null)
        { trackProgress.OnRelationFound(this, rel); }
      }

      return result;
    }

    public abstract bool IsLinear { get; }

    double IRelationGeometry.NormedMaxOffset
    { get { return NormedMaxOffsetCore; } }
    protected abstract double NormedMaxOffsetCore { get; }

    IPoint IParamGeometry.PointAt(IPoint param)
    {
      return PointAt(param[0]);
    }
    IList<IPoint> ITangentGeometry.TangentAt(IPoint param)
    {
      return new IPoint[] { TangentAt(param[0]) };
    }

    #region IGeometry Members

    public override int Dimension
    { get { return Math.Min(Start.Dimension, End.Dimension); } }

    public override int Topology
    { get { return 1; } }

    //public virtual bool Intersects(IGeometry other)
    //{
    //  return Intersection(other) != null;
    //}

    public override bool IsWithin(IPoint point)
    {
      return false;
    }
    /*
    public GeometryCollection Intersection(Curve curve)
    {
      object pReturn;
      double df0, df1;

      if (IsMultipart(this) || IsMultipart(curve))
      { return GeometryOperator.Intersection(this, curve); }

      if (this is Line)
      {
        if (curve is Line)
        { pReturn = ((Line)this).CutLine((Line)curve, out df0, out df1, true); }
        else if (curve is Arc)
        { pReturn = ((Arc)curve).CutLine((Line)this, out df0, out df1); }
        else
        {
          pReturn = GeometryOperator.Intersection(this, curve);
        }
      }
      else if (this is Arc)
      {
        if (curve is Line)
        { pReturn = ((Arc)this).CutLine((Line)curve, out df0, out df1); }
        else if (curve is Arc)
        { pReturn = ((Arc)this).CutArc((Arc)curve); }
        else
        {
          pReturn = GeometryOperator.Intersection(this, curve);
        }
      }
      else
      {
        if (curve is Line)
        { pReturn = curve.Intersection(this); }
        else
        {
          pReturn = GeometryOperator.Intersection(this, curve);
        }
      }
      if (pReturn == null)
      { return null; }
      if (pReturn is GeometryCollection)
      { return (GeometryCollection)pReturn; }
      return new GeometryCollection(new IGeometry[] { (IGeometry)pReturn });
    }
    */

    /// <summary>
    /// precondition: this and other must both be convex
    /// </summary>
    /// <returns></returns>
    /*
    private void CutAnyAny(Curve other, double min0, double max0, double min1, double max1,
      int ident, ref GeometryCollection result)
    {
      if (max0 - min0 < 1e-6 || max1 - min1 < 1e-6)
      { return; }

      double[] dfList = CutAnyAny(LinApprox(min0, max0),
        other.LinApprox(min1, max1), ident);
      if (dfList == null)
      { return; }

      double df0 = min0 + (max0 - min0) * dfList[0];
      double df1 = min1 + (max1 - min1) * dfList[1];
      double ddf0, ddf1;
      int n = 0;

      IPoint cut;
      do
      {
        IPoint pPnt0 = PointAt(df0);
        IPoint pTgt0 = TangentAt(df0);
        IPoint pPnt1 = other.PointAt(df1);
        IPoint pTgt1 = other.TangentAt(df1);

        Line pLine0 = new Line(pPnt0, pPnt0.Add(pTgt0));
        Line pLine1 = new Line(pPnt1, pPnt1.Add(pTgt1));
        cut = pLine0.CutLine(pLine1, out ddf0, out ddf1, false);
        df0 += ddf0;
        df1 += ddf1;
        n++;
      } while ((Math.Abs(ddf0) > 1e-6 || Math.Abs(ddf1) > 1e-6) &&
        df0 >= min0 && df0 <= max0 &&
        df1 >= min1 && df1 <= max1 && n < 128);

      if (Math.Abs(ddf0) > 1e-6 || Math.Abs(ddf1) > 1e-6 ||
        ((ident & 3) != 0 && Math.Abs(df0 - min0) < 2e-6) ||
        ((ident & 12) != 0 && Math.Abs(max0 - df0) < 2e-6))
      {
        double dMean0 = (min0 + max0) / 2.0;
        double dMean1 = (min1 + max1) / 2.0;
        CutAnyAny(other, min0, dMean0, min1, dMean1, ident & 1, ref result);
        CutAnyAny(other, dMean0, max0, min1, dMean1, ident & 4, ref result);
        CutAnyAny(other, min0, dMean0, dMean1, max1, ident & 2, ref result);
        CutAnyAny(other, dMean0, max0, dMean1, max1, ident & 8, ref result);

        return;
      }

      if (result == null)
      { result = new GeometryCollection(); }
      result.Add(new CutPoint_(Point.Create(cut), this, df0, other, df1));

      if (((Start.X > End.X) == (Start.Y > End.Y)) ==
        ((other.Start.X > other.End.X) == (other.Start.Y > other.End.Y)))
      { // this and other continuous in same direction, may be there are multiple intersections !

        if (Start.X > End.X == other.Start.X > other.End.X)
        {
          CutAnyAny(other, min0, df0, min1, df1, ident | 8, ref result);
          CutAnyAny(other, df0, max0, df1, max1, ident | 1, ref result);
        }
        else
        {
          CutAnyAny(other, min0, df0, df1, max1, ident | 4, ref result);
          CutAnyAny(other, df0, max0, min1, df1, ident | 2, ref result);
        }
      }
    }

    private static double[] CutAnyAny(IList<IPoint> list0,
      IList<IPoint> list1, int ident)
    {
      double dSideAll = 0;
      bool bSameSide = true;

      SnapIdentPoints(list0, list1, ident);

      int iMaxPoint = list0.Count - 1;
      int iPos = 1;
      for (int i0 = 0; i0 <= iMaxPoint; i0++)
      {
        int i1 = i0 + 1;
        if (i1 > iMaxPoint)
        {
          i1 = 0;
          iPos = 3;
        }
        else if (i1 == iMaxPoint)
        { iPos = iPos | 2; }

        Line pLine = new Line(list0[i0], list0[i1]);

        double dSide;
        double[] dfList = pLine.Intersection(list1, iPos, ident, out dSide);
        iPos = 0;
        if (dfList != null)
        {
          if (i1 > 0)
          { dfList[0] = (dfList[0] + i0) / iMaxPoint; }
          else
          { dfList[0] = 1 - dfList[0]; }
          return dfList;
        }

        if (bSameSide == false)
        { } // do nothing
        else if (double.IsNaN(dSide))
        { bSameSide = false; }
        else if (dSideAll == 0)
        { dSideAll = dSide; }
        else if (dSide < 0 != dSideAll < 0)
        { bSameSide = false; }
      }

      if (bSameSide) // other line lies within this convec hull
      { return new double[] { 0.5, 0.5 }; }
      else // if you get here, the convex hulls do not intersect
      { return null; }
    }
    */
    internal static void SnapIdentPoints(IList<IPoint> list0,
      IList<IPoint> list1, int ident)
    {
      if (ident == 0)
      { return; }

      if ((ident & 1) != 0) // Start0 == Start1
      {
        if (!((ident & 6) == 0)) throw new InvalidOperationException($"Expected ident & 6 == 0, got {ident & 6}");
        list1[0] = list0[0];
      }
      else if ((ident & 2) != 0) // Start0 == End1
      { list1[list1.Count - 1] = list0[0]; }

      if ((ident & 4) != 0) // End0 == Start1
      {
        if (!((ident & 9) == 0)) throw new InvalidOperationException($"Expected ident & 9 == 0, got {ident & 9}");
        list1[0] = list0[list0.Count - 1];
      }
      else if ((ident & 8) != 0) // End0 == End1
      { list1[list1.Count - 1] = list0[list0.Count - 1]; }
    }

    public virtual IList<IPoint> LinApprox(double min, double max)
    {
      IPoint[] result = new IPoint[3];
      IPoint p0 = PointAt(min);
      IPoint p2 = PointAt(max);
      result[0] = p0;
      result[2] = p2;

      Line line0 = new Line(p0, PointOp.Add(p0, TangentAt(min)));
      Line line1 = new Line(p2, PointOp.Add(p2, TangentAt(max)));

      result[1] = (IPoint)line0.CutLine(line1).Intersection;

      return result;
    }

    //protected abstract double Parameter(ParamGeometryRelation split);
    public static List<double> GetSplitAts(ISegment curve, IList<ParamGeometryRelation> splits)
    {
      List<double> atList = new List<double>(splits.Count);
      foreach (var rel in splits)
      {
        ParamGeometryRelation r = rel.GetChildRelation(curve);
        double g;
        if (r == null)
        { g = rel.XParam[0]; }
        else
        {
          g = ((IMultipartSegment)curve).Parameter(r);
        }

        atList.Add(g);
      }
      atList.Sort();
      return atList;
    }

    public static List<ISegment> GetSplitSegments(ISegment curve, IEnumerable<double> atList)
    {
      List<ISegment> result = new List<ISegment>();
      ISegment part;
      double min = 0;
      foreach (var at in atList)
      {
        if (at < min)
        { throw new InvalidOperationException(); }
        if (at > 1)
        { throw new InvalidOperationException(); }

        if (min < at)
        { part = curve.Subpart(min, at); }
        else
        { part = null; }
        result.Add(part);

        min = at;
      }
      if (min < 1)
      { part = curve.Subpart(min, 1); }
      else
      { part = null; }
      result.Add(part);

      return result;
    }

    IBox IGeometry.Extent
    { get { return Extent; } }

    public virtual PointCollection Border
    {
      get
      {
        PointCollection pBorder = new PointCollection();
        pBorder.AddRange(new IPoint[] { Start, End });
        return pBorder;
      }
    }

    protected override IGeometry BorderGeom
    { get { return Border; } }
    IGeometry IGeometry.Border
    { get { return Border; } }

    #endregion

    internal static ISegment Create(IPoint start, IPoint end, IInnerSegment innerCurve)
    {
      if (innerCurve == null)
      { return new Line(start, end); }
      else
      { return innerCurve.Complete(start, end); }
    }

    ICurve ICurve.Invert() => InvertCore();
    ISegment ISegment.Invert() => InvertCore();
    protected abstract Curve InvertCore();

    ISegment ISegment.Project(IProjection prj) => (ISegment)ProjectCore(prj);
  }

  public interface IInnerSegment
  {
    ISegment Complete(IPoint start, IPoint end);
  }
  public abstract class InnerCurve : IInnerSegment
  {
    ISegment IInnerSegment.Complete(IPoint start, IPoint end)
    { return Complete(start, end); }
    public abstract Curve Complete(IPoint start, IPoint end);
  }
}
