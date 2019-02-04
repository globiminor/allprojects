using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public abstract class LineCore : Curve, ILine, ISegment<Line>
  {
    protected abstract IPoint GetStart();
    protected abstract IPoint GetEnd();

    public override IPoint Start => GetStart();
    public override IPoint End => GetEnd();

    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is ILine o))
      { return false; }
      return LineOp.EqualGeometry(this, o);
    }

    public override int Dimension
    {
      get { return Math.Min(Start.Dimension, End.Dimension); }
    }

    public override int Topology
    {
      get { return 1; }
    }

    public new Box Extent => LineOp.GetExtent(this);
    protected override IBox GetExtent() => LineOp.GetExtent(this);
    protected override Curve CloneCore() => LineOp.Clone(this);
    public new Line Clone() => LineOp.Clone(this);

    protected override double NormedMaxOffsetCore => 0;
    public override bool IsLinear => true;

    public override InnerCurve GetInnerCurve() => null;

    public override Point PointAt(double t) => LineOp.PointAt(this, t);

    protected override Curve SubpartCurve(double t0, double t1) => LineOp.Subpart(this, t0, t1);
    public new Line Subpart(double t0, double t1) => LineOp.Subpart(this, t0, t1);

    protected override Curve InvertCore() => LineOp.Invert(this);
    public Line Invert() => LineOp.Invert(this);
    public override double Length() => LineOp.Length(this);
    public override Point TangentAt(double t) => LineOp.TangentAt(this, t);
    public override double ParamAt(double distance) => LineOp.ParamAt(this, distance);

    protected override IGeometry ProjectCore(IProjection projection) => LineOp.Project(this, projection);
    public Line Project(IProjection projection) => LineOp.Project(this, projection);

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


    public Point Cut(IBox box, ref double t) => LineOp.Cut(this, box, ref t);
    public ParamGeometryRelation CutLine(ILine line) => LineOp.CutLine(this, line);

    public Relation Intersect(ILine other, int xDim, int yDim, ref double u, ref double v, double? epsi = null) => LineOp.Intersect(this, other, xDim, yDim, ref u, ref v, epsi);

    public override IEnumerable<ParamGeometryRelation> CreateRelations(IParamGeometry other, TrackOperatorProgress trackProgress)
    {
      if (other is Curve o)
      {
        return CreateRelations(o, trackProgress);
      }
      return base.CreateRelations(other, trackProgress);
    }

    public double Distance2(IPoint point) => LineOp.Distance2(this, point);

    protected override IEnumerable<ParamGeometryRelation> CreateRelations(Curve other, TrackOperatorProgress trackProgress) => LineOp.CreateRelations(this, other, trackProgress);

    public override IList<IPoint> LinApprox(double t0, double t1) => LineOp.LinApprox(this, t0, t1);
  }
  public class LineWrap : LineCore
  {
    public ILine Line { get; }
    public LineWrap(ILine line)
    {
      Line = line;
    }
    protected override IPoint GetStart() => Line.Start;
    protected override IPoint GetEnd() => Line.End;
  }

  public class Line : LineCore
  {
    private readonly IPoint _start;
    private readonly IPoint _end;

    public Line()
    { }

    public Line(IPoint start, IPoint end)
    {
      _start = start;
      _end = end;
    }

    protected override IPoint GetStart() => _start;
    protected override IPoint GetEnd() => _end;

    public static LineCore CastOrWrap(ILine line)
    {
      return line as LineCore ?? new LineWrap(line);
    }
  }

  public static class LineOp
  {
    public static bool EqualGeometry(ILine x, ILine y)
    {
      bool equal = PointOp.EqualGeometry(x.Start, y.Start) && PointOp.EqualGeometry(x.End, y.End);
      return equal;

    }

    public static Line Clone(ILine line)
    {
      return new Line(Point.Create(line.Start), Point.Create(line.End));
    }

    public static Point PointAt(ILine line, double t, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(line);
      List<double> coords = new List<double>();
      foreach (int i in dimensions)
      {
        coords.Add(line.Start[i] + t * (line.End[i] - line.Start[i]));
      }
      Point p = Point.Create(coords.ToArray());
      return p;
    }

    public static double ParamAt(ILine line, double distance)
    {
      return distance / Length(line);
    }

    public static double Length(ILine line, IEnumerable<int> dimensions = null)
    {
      return Math.Sqrt(PointOp.Dist2(line.Start, line.End, dimensions));
    }

    public static IEnumerable<ParamGeometryRelation> CreateRelations(ILine l, Curve other, TrackOperatorProgress trackProgress)
    {
      if (other is ILine line)
      {
        ParamGeometryRelation rel = CutLine(l, line);
        if (rel.Intersection != null)
        {
          if (trackProgress != null)
          { trackProgress.OnRelationFound(l, rel); }

          yield return rel;
          yield break;
        }
      }
      if (other is IArc arc)
      {
        foreach (var rel in ArcOp.CutLine(arc, l))
        { yield return rel; }
        yield break;
      }

      IEnumerable<ParamGeometryRelation> baseRels = GeometryOperator.CreateRelations(Line.CastOrWrap(l), other, trackProgress, calcLinearized: true);
      if (baseRels != null)
      {
        foreach (var rel in baseRels)
        {
          yield return rel;
        }
      }
    }

    public static Relation Intersect(ILine x, ILine other, int xDim, int yDim, ref double u, ref double v, double? epsi = null)
    {
      epsi = epsi ?? Geometry.epsi;
      // this.PointAt(u) = other.PointAt(v)
      // TODO: Generalize for any LineType
      double x0 = x.Start[xDim];
      double x1 = other.Start[xDim];
      double y0 = x.Start[yDim];
      double y1 = other.Start[yDim];

      double a0 = x.End[xDim] - x0;
      double b0 = x.End[yDim] - y0;

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

    public static Line Subpart(ILine line, double t0, double t1)
    {
      Point p = PointOp.Sub(line.End, line.Start);
      return new Line(line.Start + (t0 * p), line.Start + (t1 * p));
    }

    #region IGeometry Members

    public static double Distance2(ILine line, IPoint point)
    {
      Point a = PointOp.Sub(line.End, line.Start);
      Point b = PointOp.Sub(point, line.Start);

      double scalarProd = a.SkalarProduct(b); // == |a| * |b| * cos phi
      if (scalarProd < 0)
      { return b.OrigDist2(); }

      double a2 = a.OrigDist2();
      if (scalarProd > a2)
      { return PointOp.Dist2(line.End, point); }

      if (a2 == 0)
      { return b.OrigDist2(); }

      double b2 = b.OrigDist2();
      return b2 - scalarProd * scalarProd / a2;
    }

    public static ParamGeometryRelation CutLine(ILine x, ILine y)
    {
      Point xAxis = PointOp.Sub(x.End, x.Start);
      OrthogonalSystem fullSystem = OrthogonalSystem.Create(new IPoint[] { xAxis });

      Axis nullAxis = null;

      IPoint yAxis = PointOp.Sub(y.End, y.Start);
      Axis axis = fullSystem.GetOrthogonal(yAxis);
      if (axis.Length2Epsi > 0)
      { fullSystem.Axes.Add(axis); }
      else
      { nullAxis = axis; }

      Point offset = PointOp.Sub(y.Start, x.Start);
      Axis offsetAxis = fullSystem.GetOrthogonal(offset);

      IList<double> factors = fullSystem.VectorFactors(offsetAxis.Factors);

      IPoint xParam = Point.Create(new double[] { factors[0] });
      IPoint yParam;
      if (nullAxis == null)
      { yParam = Point.Create(new double[] { -factors[1] }); }
      else
      { yParam = null; }

      ParamGeometryRelation rel = new ParamGeometryRelation(Line.CastOrWrap(x), xParam, Line.CastOrWrap(y), yParam, offsetAxis.Point);

      return rel;
    }

    public static Point TangentAt(ILine line, double t, IEnumerable<int> dimensions = null)
    {
      return PointOp.Sub(line.End, line.Start, dimensions);
    }
    public static IList<IPoint> LinApprox(ILine line, double t0, double t1)
    {
      return new IPoint[] { PointAt(line, t0), PointAt(line, t1) };

    }

    public static Point Cut(ILine line, IBox box, ref double t, IEnumerable<int> dimensions = null)
    {
      /*
          (x)     (a)     (x0)   (x1)  (xc)  (xc)
          ( ) + t*( )  =      or     or    or
          (y)     (b)     (yc)   (yc)  (y0)  (y1)

          and t (output) > t (input)               
      */

      double tNew = t - 1;
      List<double> pNew = null;
      IPoint start = line.Start;
      IPoint end = line.End;

      dimensions = dimensions ?? GeometryOperator.GetDimensions(line, box);
      List<double> coords = new List<double>();

      foreach (int iDim in dimensions)
      {
        for (int iSid = 0; iSid < 2; iSid++)
        {
          while (coords.Count <= iDim) coords.Add(0);
          double a = end[iDim] - start[iDim];

          if (a != 0)
          {

            if (iSid == 0)
            { coords[iDim] = box.Min[iDim]; }
            else
            { coords[iDim] = box.Max[iDim]; }

            double u = (coords[iDim] - start[iDim]) / a;
            if (u > t && (u < tNew || tNew < t))
            {
              bool bCut = true;
              foreach (int j in dimensions)
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
                coords[j] = tc;
              }
              if (bCut)
              {
                pNew = new List<double>(coords);
                tNew = u;
              }
            }
          }
        }
      }
      Point result = null;
      if (pNew != null)
      {
        while (pNew.Count < coords.Count) pNew.Add(0);
        result = Point.Create(pNew.ToArray());
        t = tNew;
      }
      return result;
    }

    public static Box GetExtent(ILine line)
    {
      return new Box(Point.Create(line.Start), Point.Create(line.End), true);
    }

    public static Line Invert(ILine line)
    {
      return new Line(line.End, line.Start);
    }

    public static Line Project(ILine line, IProjection projection)
    {
      return new Line(PointOp.Project(line.Start, projection),
        PointOp.Project(line.End, projection));
    }

    #endregion
  }

}
