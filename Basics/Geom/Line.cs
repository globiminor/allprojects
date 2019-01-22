using System;
using System.Collections.Generic;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Line
  /// </summary>
  public class Line : Curve, ISegment<Line>
  {
    private IPoint _start;
    private IPoint _end;

    static Line()
    {
      Point min = Point.Create(1);
      Point max = Point.Create(1);
      min.X = 0;
      max.X = 1;
    }
    public Line()
    { }

    public Line(IPoint start, IPoint end)
    {
      _start = start;
      _end = end;
    }

    protected override double NormedMaxOffsetCore
    { get { return 0; } }
    public override bool IsLinear
    { get { return true; } }

    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is Line o))
      { return false; }
      bool equal = Start.EqualGeometry(o.Start) && End.EqualGeometry(o.End);
      return equal;
    }

    private delegate double BoxFct(double x, double y);
    private class BoxPoint : Line, IPoint
    {
      private readonly BoxFct _fct;
      public BoxPoint(IPoint s, IPoint e, BoxFct fct)
      {
        _start = s;
        _end = e;
        _fct = fct;
      }
      public double X { get { return _fct(_start.X, _end.X); } set { } }
      public double Y { get { return _fct(_start.Y, _end.Y); } set { } }
      public double Z { get { return _fct(_start.Z, _end.Z); } set { } }
      public double this[int i] { get { return _fct(_start[i], _end[i]); } set { } }

      IPoint IPoint.Project(IProjection prj)
      {
        return Pnt().Project(prj);
      }
      public Point Pnt()
      {
        int dim = Dimension;
        Point p = Point.Create(dim);
        for (int i = 0; i < dim; i++)
        { p[i] = this[i]; }
        return p;
      }
    }
    public override InnerCurve GetInnerCurve()
    { return null; }

    public override IPoint Start
    {
      get { return _start; }
    }

    public override IPoint End
    {
      get { return _end; }
    }

    protected override Curve CloneCore()
    { return Clone(); }
    public new Line Clone()
    {
      return new Line(Point.Create(_start), Point.Create(_end));
    }

    public override Point PointAt(double t)
    {
      int iDim = Dimension;
      Point p = Point.Create(iDim);

      for (int i = 0; i < iDim; i++)
      {
        p[i] = _start[i] + t * (_end[i] - _start[i]);
      }
      return p;
    }
    public override double ParamAt(double distance)
    {
      return distance / Length();
    }

    public override double Length()
    {
      return Math.Sqrt(PntOp.Dist2(Start, End));
    }

    /// <summary>
    /// Findet Punkt wo
    /// this.PointAt(u) = other.PointAt(v)
    /// mit u > Startwert von u
    /// </summary>
    /// <returns>weitere Schnittstelle vorhanden</returns>
    public Relation Intersect(Line other, int xDim, int yDim, ref double u, ref double v)
    {
      if (other.GetType() != typeof(Line))
      {
        if (GetType() == typeof(Line))
        { return other.Intersect(this, xDim, yDim, ref v, ref u); }
      }
      // this.PointAt(u) = other.PointAt(v)
      // TODO: Generalize for any LineType
      double x0 = Start[xDim];
      double x1 = other.Start[xDim];
      double y0 = Start[yDim];
      double y1 = other.Start[yDim];

      double a0 = End[xDim] - x0;
      double b0 = End[yDim] - y0;

      double a1 = other.End[xDim] - x1;
      double b1 = other.End[yDim] - y1;

      double c0 = x1 - x0;
      double c1 = y1 - y0;
      double det = -a0 * b1 + b0 * a1;
      if (Math.Abs(det) > epsi * (Math.Abs(x0) + Math.Abs(x1) + Math.Abs(y0) + Math.Abs(y1)) / 1000.0)
      {
        u = (-c0 * b1 + c1 * a1) / det;
        v = (a0 * c1 - b0 * c0) / det;
        return Relation.Intersect;
      }
      else
      {
        /* parallel lines */
        det = -a0 * c1 + b0 * c0;
        v = (det * det) / (a0 * a0 + b0 * b0);
        if (Math.Abs(v) < epsi)
        { return Relation.Intersect; }
        return Relation.Disjoint;
      }
    }

    public new Line Subpart(double t0, double t1)
    {
      Point p = PntOp.Sub(_end, _start);
      return new Line(_start + (t0 * p), _start + (t1 * p));
    }
    protected override Curve SubpartCurve(double t0, double t1)
    { return Subpart(t0, t1); }

    #region IGeometry Members

    public override int Dimension
    {
      get { return Math.Min(Start.Dimension, End.Dimension); }
    }

    public override int Topology
    {
      get { return 1; }
    }

    public double Distance2(IPoint point)
    {
      Point a = PntOp.Sub(End, Start);
      Point b = PntOp.Sub(point, Start);

      double scalarProd = a.SkalarProduct(b); // == |a| * |b| * cos phi
      if (scalarProd < 0)
      { return b.OrigDist2(); }

      double a2 = a.OrigDist2();
      if (scalarProd > a2)
      { return PntOp.Dist2(End, point); }

      if (a2 == 0)
      { return b.OrigDist2(); }

      double b2 = b.OrigDist2();
      return b2 - scalarProd * scalarProd / a2;
    }

    public override IEnumerable<ParamGeometryRelation> CreateRelations(IParamGeometry other, TrackOperatorProgress trackProgress)
    {
      if (other is Curve o)
      {
        return CreateRelations(o, trackProgress);
      }
      return base.CreateRelations(other, trackProgress);
    }
    protected override IEnumerable<ParamGeometryRelation> CreateRelations(Curve other, TrackOperatorProgress trackProgress)
    {
      if (other is Line line)
      {
        ParamGeometryRelation rel = CutLine(line);
        if (rel.Intersection != null)
        {
          if (trackProgress != null)
          { trackProgress.OnRelationFound(this, rel); }

          yield return rel;
          yield break;
        }
      }
      if (other is Arc arc)
      {
        foreach (var rel in arc.CutLine(this))
        { yield return rel; }
        yield break;
      }

      IEnumerable<ParamGeometryRelation> baseRels = base.CreateRelations(other, trackProgress);
      if (baseRels != null)
      {
        foreach (var rel in baseRels)
        {
          yield return rel;
        }
      }
    }

    /*
    internal void CutAny(Curve line, double min0, double max0, double min1, double max1,
      int ident, ref GeometryCollection result)
    {
      if (max0 - min0 < 1e-6 || max1 - min1 < 1e-6)
      { return; }

      IList<IPoint> pList = line.LinApprox(min1, max1);
      if (ident != 0)
      { SnapIdentPoints(new IPoint[] { PointAt(min0), PointAt(max0) }, pList, ident); }

      double dSide;
      double[] dfList = Intersection(min0, max0, pList, 3, ident, out dSide);
      if (dfList == null)
      { return; }

      double dfThis;
      double dfAny = min1 + (max1 - min1) * dfList[1];
      double ddf;

      IPoint cut;
      do
      {
        IPoint point = line.PointAt(dfAny);
        IPoint tangent = line.TangentAt(dfAny);

        cut = CutLine(new Line(point, point.Add(tangent)), out dfThis, out ddf);
        dfAny += ddf;
      } while (Math.Abs(ddf) > 1e-6 && dfThis >= min0 && dfThis <= max0 &&
        dfAny >= min1 && dfAny <= max1);

      if (Math.Abs(ddf) > 1e-6 ||
        ((ident & 3) != 0 && Math.Abs(dfThis - min0) < 2e-6) ||
        ((ident & 12) != 0 && Math.Abs(max0 - dfThis) < 2e-6))
      {
        double dMean0 = (min0 + max0) / 2.0;
        double dMean1 = (min1 + max1) / 2.0;
        CutAny(line, min0, dMean0, min1, dMean1, ident & 1, ref result);
        CutAny(line, dMean0, max0, min1, dMean1, ident & 4, ref result);
        CutAny(line, min0, dMean0, dMean1, max1, ident & 2, ref result);
        CutAny(line, dMean0, max0, dMean1, max1, ident & 8, ref result);

        return;
      }

      if (result == null)
      { result = new GeometryCollection(); }
      result.Add(new CutPoint_(Point.Create(cut), this, dfThis, line, dfAny));

      if (ident != 0)
      { return; }

      if (((Start.X > End.X) == (Start.Y > End.Y)) ==
        ((line.Start.X > line.End.X) == (line.Start.Y > line.End.Y)))
      { // this and other continuous in same direction, may be there are multiple intersections !

        if (Start.X > End.X == line.Start.X > line.End.X)
        {
          CutAny(line, 0, dfThis, 0, dfAny, 8, ref result);
          CutAny(line, dfThis, 1, dfAny, 1, 1, ref result);
        }
        else
        {
          CutAny(line, 0, dfThis, dfAny, 1, 4, ref result);
          CutAny(line, dfThis, 1, 0, dfAny, 2, ref result);
        }
      }

    }

    internal double[] Intersection(IList<IPoint> convexList,
      int position, int ident, out double side)
    {
      return Intersection(0, 1, convexList, position, ident, out side);
    }

    internal double[] Intersection(double l0, double l1,
      IList<IPoint> convexList, int position, int ident, out double side)
    { // remark: because convexList is assumed to be convex, 
      // there are exactly 0 or 2 intersections !
      double df0 = 0;

      IPoint d0 = End.Sub(Start);
      int iMin = 0;
      int iMax = convexList.Count - 1;
      int iNSeg = iMax;

      if (ident != 0 && position != 0)
      {
        if ((ident & 1) != 0 && (position & 1) != 0)
        { iMin = 1; }
        if ((ident & 2) != 0 && (position & 1) != 0)
        { iMax--; }
        if ((ident & 4) != 0 && (position & 2) != 0)
        { iMin = 1; }
        if ((ident & 8) != 0 && (position & 2) != 0)
        { iMax--; }
      }

      int iNCut = 0;
      IPoint d2 = convexList[iMin].Sub(Start);
      double dv1 = d0.VectorProduct(d2);
      side = dv1;
      for (int i0 = iMin; i0 <= iMax; i0++)
      {
        double dv0 = dv1;

        int i1 = i0 + 1;
        if (i1 > iMax)
        {
          if (iMin > 0 || iMax < iNSeg)
          { continue; }
          i1 = 0;
        }

        d2 = convexList[i1].Sub(Start);
        dv1 = d0.VectorProduct(d2);

        if (dv0 < 0 != dv1 < 0 || dv0 == 0 || dv1 == 0)
        { // intersection found
          double dv = dv0 / (dv0 - dv1);
          IPoint cut;
          if (dv != 1)
          { cut = convexList[i0].Add(convexList[i1].Sub(convexList[i0]).Scale(dv)); }
          else
          { cut = convexList[i1]; }
          double df1 = d0.GetFactor(cut.Sub(Start));

          if (df1 < l0 || df1 > l1)
          {
            if (iNCut == 0)
            {
              df0 = df1;
              iNCut++;
              continue;
            }
            if ((df0 < l0 && df1 < l0) || df0 > l1 && df1 > l1)
            {
              side = double.NaN;
              return null;
            }
            df1 = l0;
          }

          if (i0 < iMax)
          { return new double[] { df1, (dv + i0) / iNSeg }; }
          else
          { return new double[] { df1, 1 - dv }; }
        }
        else
        { side += dv0 + dv1; }
      }
      return null;
    }
    */


    public ParamGeometryRelation CutLine(Line line)
    {
      Point xAxis = PntOp.Sub(End, Start);
      OrthogonalSystem fullSystem = OrthogonalSystem.Create(new IPoint[] { xAxis });

      Axis nullAxis = null;

      IPoint yAxis = PntOp.Sub(line.End, line.Start);
      Axis axis = fullSystem.GetOrthogonal(yAxis);
      if (axis.Length2Epsi > 0)
      { fullSystem.Axes.Add(axis); }
      else
      { nullAxis = axis; }

      Point offset = PntOp.Sub(line.Start, Start);
      Axis offsetAxis = fullSystem.GetOrthogonal(offset);

      IList<double> factors = fullSystem.VectorFactors(offsetAxis.Factors);

      IPoint xParam = Point.Create(new double[] { factors[0] });
      IPoint yParam;
      if (nullAxis == null)
      { yParam = Point.Create(new double[] { -factors[1] }); }
      else
      { yParam = null; }

      ParamGeometryRelation rel = new ParamGeometryRelation(this, xParam, line, yParam, offsetAxis.Point);

      return rel;

      //IPoint p = End.Sub(Start);
      //IPoint p0 = line.Start.Sub(Start);
      //IPoint p1 = line.End.Sub(Start);
      //f0 = 0;
      //f1 = 0;
      //double v0 = p.VectorProduct(p0);
      //double v1 = p.VectorProduct(p1);

      //if (v0 == 0)
      //{
      //  if (v1 == 0)
      //  { // coincident
      //    f0 = p.GetFactor(p0);
      //    f1 = p.GetFactor(p1);
      //    return null;
      //  }
      //  f0 = p.GetFactor(p0);
      //  f1 = 0;
      //  return line.Start;
      //}

      //if (v1 == 0)
      //{
      //  f0 = p.GetFactor(p1);
      //  f1 = 1;
      //}
      //if (validateInside)
      //{
      //  if ((v0 > 0) == (v1 > 0))
      //  { return null; }
      //  if (v1 == 0)
      //  { return null; }
      //}

      //if (v0 == v1)
      //{ // Parallel lines
      //  f0 = v0;
      //  f1 = v0;
      //  return null;
      //}
      //f1 = v0 / (v0 - v1);
      //if ((validateInside == false || (f1 > 0 && f1 < 1)) == false)
      //{ throw new InvalidProgramException("Cut point does not lie within segment"); }

      //Point d = Point.Sub(line.End, line.Start);
      //Point cut = line.Start + f1 * d;
      //f0 = p.GetFactor(cut - Start);
      //return cut;
    }

    public override Point TangentAt(double t)
    {
      return PntOp.Sub(End, Start);
    }
    public override IList<IPoint> LinApprox(double t0, double t1)
    {
      if (GetType() == typeof(Line))
      { return new IPoint[] { PointAt(t0), PointAt(t1) }; }

      return base.LinApprox(t0, t1);
    }

    public Point Cut(Box box, ref double t)
    {
      /*
          (x)     (a)     (x0)   (x1)  (xc)  (xc)
          ( ) + t*( )  =      or     or    or
          (y)     (b)     (yc)   (yc)  (y0)  (y1)

          and t (output) > t (input)               
      */

      double tNew = t - 1;
      Point pNew = null;
      Point pc;
      IPoint start = Start;
      IPoint end = End;
      int dim = Dimension;

      if (box.Dimension < dim)
      { dim = box.Dimension; }
      pc = Point.Create(dim);

      for (int iSide = 0; iSide < 2 * dim; iSide++)
      {
        int iDim = iSide / 2;
        double a = end[iDim] - start[iDim];

        if (a != 0)
        {
          if ((iSide & 1) == 0)
          { pc[iDim] = box.Min[iDim]; }
          else
          { pc[iDim] = box.Max[iDim]; }

          double u = (pc[iDim] - start[iDim]) / a;
          if (u > t && (u < tNew || tNew < t))
          {
            bool bCut = true;
            for (int j = 0; j < dim; j++)
            {
              if (j == iDim)
              { continue; }
              double b = end[j] - start[j];
              double tc = start[j] + b * u;
              if (tc < box.Min[j] || tc > box.Max[j])
              {
                bCut = false;
                break;
              }
              pc[j] = tc;
            }
            if (bCut)
            {
              pNew = pc.Clone();
              tNew = u;
            }
          }
        }
      }
      if (pNew != null)
      { t = tNew; }
      return pNew;
    }


    public override IBox Extent
    {
      get
      {
        return new Box(Point.Create(Start), Point.Create(End), true);
      }
    }

    public override PointCollection Border
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

    protected override Curve InvertCore() => Invert();
    public Line Invert()
    {
      return new Line(_end, _start);
    }

    protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
    public Line Project(IProjection projection)
    {
      return new Line(_start.Project(projection),
        _end.Project(projection));
    }

    #endregion
  }
}
