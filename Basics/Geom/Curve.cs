using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom
{
  public interface ICurve : ITanParamGeometry
  {
    ICurve Invert();
    IEnumerable<Curve> Segments { get; }
  }
  public abstract class Curve : Geometry, ICurve, IRelTanParamGeometry
  {
    public class GeometryEquality : IEqualityComparer<Curve>
    {
      public bool Equals(Curve x, Curve y)
      {
        return x.EqualGeometry(y);
      }
      public int GetHashCode(Curve x)
      {
        return x.Start.X.GetHashCode();
      }
    }
    private static readonly Box _paramBox;

    static Curve()
    {
      Point min = Point.Create(1);
      Point max = Point.Create(1);
      min[0] = 0;
      max[0] = 1;
      _paramBox = new Box(min, max);
    }

    IEnumerable<Curve> ICurve.Segments
    { get { yield return this; } }

    public enum CurveAtType
    { Parameter, Distance }

    public abstract InnerCurve InnerCurve();

    public override abstract bool EqualGeometry(IGeometry other);

    public IList<IPoint> Linearize(double offset)
    {
      return Linearize(offset, true);
    }
    internal virtual IList<IPoint> Linearize(double offset, bool includeFirstPoint)
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
        double l2 = Point.Sub(End, Start).OrigDist2();
        if (normed * normed * l2 > d2)
        {
          Curve c0 = Subpart(0, 0.5);
          Curve c1 = Subpart(0.5, 1);
          points.AddRange(c0.Linearize(offset, false));
          points.AddRange(c1.Linearize(offset, false));
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
    protected abstract Curve Clone_();
    public Curve Clone()
    { return Clone_(); }
    public abstract Point PointAt(double param);
    public abstract Point TangentAt(double t);
    public abstract double ParamAt(double distance);
    public abstract double Length();

    public Curve Subpart(double t0, double t1)
    { return SubpartCurve(t0, t1); }
    protected abstract Curve SubpartCurve(double t0, double t1);

    IBox IParamGeometry.ParameterRange
    { get { return _paramBox; } }

    ParamRelate IRelationGeometry.Relation(IBox paramRange)
    {
      double min = paramRange.Min[0];
      double max = paramRange.Max[0];
      if (min > 1 || max < 0)
      { return ParamRelate.Disjoint; }
      if (min < 0 || max > 1)
      { return ParamRelate.Near; }
      else
      { return ParamRelate.Intersect; }
    }

    public virtual IList<ParamGeometryRelation> CreateRelations(IParamGeometry other,
      TrackOperatorProgress trackProgress)
    {
      if (other is Curve o)
      {
        return o.CreateRelations(this, trackProgress);
      }
      else
      {
        return GeometryOperator.CreateRelations(this, other, trackProgress, true);
      }
    }
    protected virtual IList<ParamGeometryRelation> CreateRelations(Curve other, TrackOperatorProgress trackProgress)
    {
      // falls diese Methode ausgefuhert wird existiert keine ueberschriebene Methode dazu 
      // und die Relation muss via Linearisierung berechnet werden.
      return GeometryOperator.CreateRelations(this, other, trackProgress, true);
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
        Debug.Assert((ident & 6) == 0);
        list1[0] = list0[0];
      }
      else if ((ident & 2) != 0) // Start0 == End1
      { list1[list1.Count - 1] = list0[0]; }

      if ((ident & 4) != 0) // End0 == Start1
      {
        Debug.Assert((ident & 9) == 0);
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

      Line line0 = new Line(p0, PointOperator.Add(p0, TangentAt(min)));
      Line line1 = new Line(p2, PointOperator.Add(p2, TangentAt(max)));

      result[1] = (IPoint)line0.CutLine(line1).Intersection;

      return result;
    }

    protected abstract IList<Curve> SplitCore(IList<ParamGeometryRelation> splits);
    public IList<Curve> Split(IList<ParamGeometryRelation> splits)
    { return SplitCore(splits); }
    protected abstract double Parameter(ParamGeometryRelation split);
    protected static IList<T> GetSplitArray<T>(T curve, IList<ParamGeometryRelation> splits) where T : Curve
    {
      List<double> paramList = new List<double>(splits.Count);
      foreach (var rel in splits)
      {
        ParamGeometryRelation r = rel.GetChildRelation(curve);
        double g;
        if (r == null)
        { g = rel.XParam[0]; }
        else
        { g = curve.Parameter(r); }

        paramList.Add(g);
      }
      paramList.Sort();

      List<T> result = new List<T>();
      T part;
      double min = 0;
      foreach (var param in paramList)
      {
        if (param < min)
        { throw new InvalidOperationException(); }
        if (param > 1)
        { throw new InvalidOperationException(); }

        if (min < param)
        { part = (T)curve.Subpart(min, param); }
        else
        { part = null; }
        result.Add(part);

        min = param;
      }
      if (min < 1)
      { part = (T)curve.Subpart(min, 1); }
      else
      { part = null; }
      result.Add(part);

      return result.ToArray();

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

    public new Curve Project(IProjection projection)
    { return (Curve)Project_(projection); }

    #endregion

    internal static Curve Create(IPoint start, IPoint end, InnerCurve innerCurve)
    {
      if (innerCurve == null)
      { return new Line(start, end); }
      else
      { return innerCurve.Complete(start, end); }
    }

    ICurve ICurve.Invert() { return Invert(); }
    public Curve Invert()
    { return Invert_(); }

    protected abstract Curve Invert_();
  }

  public abstract class InnerCurve
  {
    public abstract Curve Complete(IPoint start, IPoint end);
  }
}
