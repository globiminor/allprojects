using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using P = Basics.Geom.Point2D;

namespace Basics.Geom
{
  public abstract class ArcCore : Curve, IArc, IMultipartSegment, ISegment<Arc>
  {
    #region nested classes
    private class _InnerCurve : InnerCurve
    {
      private readonly IPoint _center;
      private readonly double _radius;
      private readonly double _dirStart;
      private readonly double _angle;

      public _InnerCurve(IPoint center, double radius, double dirStart, double angle)
      {
        _center = center;
        _radius = radius;
        _dirStart = dirStart;
        _angle = angle;
      }

      public override Curve Complete(IPoint start, IPoint end)
      {
        return new Arc(start, end, _center, _radius, _dirStart, _angle);
      }
    }
    #endregion

    private IPoint _start;
    private IPoint _end;

    private readonly bool _isQuadrant; // indicates if it is a subpart
    private double? _maxOffset;

    protected ArcCore()
    { }
    protected ArcCore(IPoint start, IPoint end)
    {
      _start = start;
      _end = end;
    }
    protected ArcCore(bool isQuadrant)
    {
      _isQuadrant = isQuadrant;
    }

    protected abstract IPoint GetCenter();
    protected abstract double GetRadius();
    protected abstract double GetDirStart();
    protected abstract double GetAngle();

    public IPoint Center => GetCenter();
    public double Radius => GetRadius();
    public double DirStart => GetDirStart();
    public double Angle => GetAngle();

    public override IPoint Start => _start ?? (_start = ArcOp.GetStart(this));
    public override IPoint End => _end ?? (_end = ArcOp.GetEnd(this));

    public double Diameter => 2 * Radius;
    public new Box Extent => ArcOp.GetExtent(this);
    protected override IBox GetExtent() => Extent;

    protected override Curve CloneCore() => ArcOp.Clone(this);
    public new Arc Clone() => ArcOp.Clone(this);
    public IEnumerable<Bezier> EnumBeziers() => ArcOp.EnumBeziers(this);

    protected override IGeometry ProjectCore(IProjection projection) => ArcOp.Project(this, projection);
    public Arc Project(IProjection projection) => ArcOp.Project(this, projection);

    public IList<ParamGeometryRelation> CutLine(ILine line) => ArcOp.CutLine(this, line);

    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is IArc o))
      { return false; }

      return ArcOp.EqualGeometry(this, o);
    }
    public override IEnumerable<ParamGeometryRelation> CreateRelations(IParamGeometry other,
      TrackOperatorProgress trackProgress)
    {
      IEnumerable<ParamGeometryRelation> list;
      if (other is Curve o)
      { list = CreateRelations(o, trackProgress); }
      else
      { list = base.CreateRelations(other, trackProgress); }
      return list;
    }
    protected override IEnumerable<ParamGeometryRelation> CreateRelations(Curve other,
      TrackOperatorProgress trackProgress)
    {
      IList<ParamGeometryRelation> result;
      if (other is ILine line)
      {
        IList<ParamGeometryRelation> list = ArcOp.CutLine(this, line);
        result = AddRelations(list, trackProgress);
        return result;
      }
      if (other is IArc arc)
      {
        IList<ParamGeometryRelation> list = ArcOp.CutArc(this, arc);
        result = AddRelations(list, trackProgress);
        return result;
      }

      return base.CreateRelations(other, trackProgress);
    }

    double IMultipartSegment.Parameter(ParamGeometryRelation split) => Parameter(split);
    private double Parameter(ParamGeometryRelation split)
    {
      if (HasSubparts == false)
      { throw new InvalidProgramException(); }

      double angle = 0;
      foreach (var subpart in Subparts())
      {
        if (split.CurrentX.EqualGeometry(subpart))
        {
          double p = split.XParam[0];

          angle += p * subpart.Angle;

          return angle / Angle;
        }
        angle += subpart.Angle;
      }
      throw new InvalidProgramException();
    }

    public override InnerCurve GetInnerCurve()
    {
      return new _InnerCurve(Center, Radius, DirStart, Angle);
    }
    /// <summary>
    /// Punkt bei Parameter t
    /// </summary>
    /// <param name="t">Wert zwischen 0 und 1</param>
    /// <returns></returns>
    public override Point PointAt(double t) => ArcOp.PointAt(this, t);
    public override double ParamAt(double distance, IReadOnlyCollection<int> dimensions = null) => ArcOp.ParamAt(this, distance, dimensions);
    public override Point TangentAt(double t) => ArcOp.TangentAt(this, t);

    public override double Length(IReadOnlyCollection<int> dimensions = null) => ArcOp.Length(this, dimensions);
    public override bool IsLinear => false;
    protected override double NormedMaxOffsetCore => _maxOffset ?? (_maxOffset = ArcOp.GetNormedMaxOffsetCore(this)).Value;
    protected override Curve InvertCore() => ArcOp.Invert(this);

    protected override Curve SubpartCurve(double t0, double t1) => ArcOp.Subpart(this, t0, t1);
    public new Arc Subpart(double t0, double t1) => ArcOp.Subpart(this, t0, t1);

    public bool IsContinuous => true;
    public bool HasSubparts
    {
      get
      {
        if (_isQuadrant)
        { return false; }

        //double qStart = Math.Floor(DirStart / (0.5 * Math.PI));
        //double qEnd = Math.Floor((DirStart + Angle) / (0.5 * Math.PI));
        //return qStart - qEnd != 0;
        return Math.Abs(Angle) > MaxMultiPartAngle;
      }
    }
    internal const double MaxMultiPartAngle = Math.PI / 2;

    protected override IList<IPoint> LinearizeCore(double offset, IReadOnlyCollection<int> dimensions, bool includeFirstPoint) 
      => ArcOp.LinearizeCore(this, offset, includeFirstPoint);
    public IList<ParamGeometryRelation> CutArc(IArc other) => ArcOp.CutArc(this, other);

    public IEnumerable<Arc> Subparts()
    {
      if (HasSubparts == false)
      { yield break; }

      int n = (int)Math.Ceiling(Math.Abs(Angle) / MaxMultiPartAngle);
      double dir = DirStart;
      double dA = Angle / n;
      for (int i = 0; i < n; i++)
      {
        yield return new Arc(Center, Radius, dir + i * dA, dA);
      }
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }
  }

  public class ArcWrap : ArcCore
  {
    public IArc Arc { get; }
    public ArcWrap(IArc arc)
    {
      Arc = arc;
    }
    protected override IPoint GetCenter() => Arc.Center;
    protected override double GetRadius() => Arc.Radius;
    protected override double GetDirStart() => Arc.DirStart;
    protected override double GetAngle() => Arc.Angle;
  }
  public class Arc : ArcCore
  {
    private readonly IPoint _center;
    private readonly double _radius;
    private readonly double _dirStart;
    private readonly double _angle;

    protected override IPoint GetCenter() => _center;
    protected override double GetRadius() => _radius;
    protected override double GetDirStart() => _dirStart;
    protected override double GetAngle() => _angle;

    internal Arc(IPoint start, IPoint end,
      IPoint center, double radius, double startDirection, double angle)
      : base(start, end)
    {
      _center = center;
      _radius = radius;
      _dirStart = startDirection;
      _angle = angle;
    }

    public Arc(IPoint center, double radius, double startDirection, double angle)
      : this(center, radius, startDirection, angle, isQuadrant: false)
    { }
    internal Arc(IPoint center, double radius, double startDirection, double angle, bool isQuadrant)
      : base(isQuadrant)
    {
      _center = center;
      _radius = radius;
      _dirStart = startDirection;
      _angle = angle;
    }

    public static ArcCore CastOrWrap(IArc arc)
    {
      return arc as ArcCore ?? new ArcWrap(arc);
    }
  }

  public static class ArcOp
  {
    public static Arc Clone(IArc arc)
    {
      return new Arc(Point.Create(arc.Center), arc.Radius, arc.DirStart, arc.Angle);
    }
    public static IEnumerable<Bezier> EnumBeziers(IArc arc)
    {
      double dir0;
      double dir1;

      if (Math.Abs(arc.Angle) < 2 * Math.PI - 0.1)
      {
        dir0 = arc.DirStart;
        if (arc.Angle < 0)
        { dir0 += arc.Angle; }
        dir0 = dir0 / (2 * Math.PI);
        dir0 = 4 * (dir0 - Math.Floor(dir0));

        dir1 = dir0 + 2 * Math.Abs(arc.Angle) / Math.PI;
      }
      else
      {
        dir0 = 0;
        dir1 = 4;
      }

      double x = arc.Center.X;
      double y = arc.Center.Y;
      double r = arc.Radius;

      int i = (int)dir0;

      List<Bezier> parts = new List<Bezier>();
      while (i <= dir1)
      {
        double t0 = 0;
        double t1 = 1;
        if (dir0 >= i && dir0 <= i + 1)
        { t0 = (dir0 - i); }

        if (dir1 >= i && dir1 <= i + 1)
        { t1 = dir1 - i; }

        if (Math.Abs(t0 - t1) > 1e-8)
        {
          Bezier quart = GetQuart(i, x, y, r);
          Bezier part = quart.Subpart(t0, t1);
          parts.Add(part);
        }
        i++;
      }
      if (arc.Angle < 0)
      {
        parts.Reverse();
        for (int iPart = 0; iPart < parts.Count; iPart++)
        { parts[iPart] = parts[iPart].Invert(); }
      }
      return parts;
    }
    private static Bezier GetQuart(int quart, double x, double y, double r)
    {
      int q = quart % 4;
      double t = 0.55 * r;

      if (q == 0) return new Bezier(new P(x, y + r), new P(x + t, y + r), new P(x + r, y + t), new P(x + r, y));
      if (q == 1) return new Bezier(new P(x + r, y), new P(x + r, y - t), new P(x + t, y - r), new P(x, y - r));
      if (q == 2) return new Bezier(new P(x, y - r), new P(x - t, y - r), new P(x - r, y - t), new P(x - r, y));
      if (q == 3) return new Bezier(new P(x - r, y), new P(x - r, y + t), new P(x - t, y + r), new P(x, y + r));

      return null;
    }

    public static bool EqualGeometry(IArc x, IArc y)
    {
      bool equal =
        PointOp.EqualGeometry(x.Center, y.Center) &&
        x.Radius == y.Radius &&
        x.DirStart == y.DirStart &&
        x.Angle == y.Angle;
      return equal;
    }

    public static Point GetStart(IArc arc)
    {
      Point start = arc.Center + (new P(arc.Radius * Math.Cos(arc.DirStart),
           arc.Radius * Math.Sin(arc.DirStart)));
      return start;
    }

    public static Point GetEnd(IArc arc)
    {
      Point end = arc.Center + new P(arc.Radius * Math.Cos(arc.DirStart + arc.Angle),
           arc.Radius * Math.Sin(arc.DirStart + arc.Angle));
      return end;
    }

    public static IList<IPoint> LinearizeCore(IArc arc, double offset, bool includeFirstPoint)
    {
      ArcCore a = Arc.CastOrWrap(arc);
      if (offset > arc.Radius)
      { return new IPoint[] { a.Start, PointAt(arc, 0.5), a.End }; }

      double phi = 2 * Math.Acos(1 - offset / arc.Radius);
      if (phi >= Math.Abs(arc.Angle))
      { return new IPoint[] { a.Start, PointAt(arc, 0.5), a.End }; }

      int n = (int)Math.Ceiling((Math.Abs(arc.Angle) / phi));
      List<IPoint> list = new List<IPoint>(n + 1);

      if (includeFirstPoint)
      { list.Add(a.Start); }

      double at = 1.0 / n;
      for (int i = 1; i < n; i++)
      { list.Add(PointAt(arc, at * i)); }

      list.Add(a.End);

      return list;
    }

    public static Box GetExtent(IArc arc)
    {
      ArcCore arc_ = Arc.CastOrWrap(arc);


      Box box = new Box(Point.Create(arc_.Start), Point.Create(arc_.End), verify: true);
      if (arc_.HasSubparts)
      {
        foreach (var subPart in arc_.Subparts())
        { box.Include(subPart.Extent); }
      }
      else
      {
        IPoint ds = PointOp.Sub(arc_.Start, arc_.Center);
        IPoint de = PointOp.Sub(arc_.End, arc_.Center);
        if (ds.X * de.X < 0 || ds.Y * de.Y < 0)
        {
          double f = arc_.Angle * MaxBoxFactor;
          IBox tanS = PointOp.Add(arc_.Start, f * PointOp.Ortho(ds)); // Point is also box!
          box.Include(tanS);
          IBox tanE = PointOp.Add(arc_.End, -f * PointOp.Ortho(de));
          box.Include(tanE);
        }
      }
      return box;
    }
    private static double _maxBoxFactor;
    private static double MaxBoxFactor => _maxBoxFactor > 0 ? _maxBoxFactor : (_maxBoxFactor = GetMaxBoxFactor());

    private static double GetMaxBoxFactor()
    {
      double a = ArcCore.MaxMultiPartAngle / 4;
      double tan = Math.Tan(a);
      double f = tan / a;
      return f / 4;
    }

    public static Point PointAt(IArc arc, double t)
    {
      double dDir = arc.DirStart + t * arc.Angle;
      return new P(arc.Center.X + arc.Radius * Math.Cos(dDir),
        arc.Center.Y + arc.Radius * Math.Sin(dDir));
    }

    public static double ParamAt(IArc arc, double distance, IEnumerable<int> dimensions = null)
    {
      return distance / Length(arc, dimensions);
    }

    public static double Length(IArc arc, IEnumerable<int> dimensions)
    {
      double l = Math.Abs(arc.Radius * arc.Angle);

      IEnumerable<int> dims = dimensions ?? GeometryOperator.GetDimensionsEnum(arc.Start);
      double dl2 = 0;
      foreach (var dim in dims)
      {
        if (dim > 2)
        {
          double dl = arc.Start[dim] - arc.End[dim];
          dl2 += dl * dl;
        }
      }

      if (dl2 > 0)
      {
        l = Math.Sqrt(l * l + dl2);
      }

      return l;
    }

    public static double GetNormedMaxOffsetCore(IArc arc)
    {
      ArcCore a = Arc.CastOrWrap(arc);
      // TODO: verify
      Point p = PointOp.Sub(a.End, a.Start);
      double p2 = p.OrigDist2();
      Point m = 2.0 * (PointAt(a, 0.5) - a.Start);
      double m2 = m.OrigDist2();

      double maxOffset = Math.Sqrt((m2 - p2) / p2);
      return maxOffset;
    }

    public static Arc Invert(IArc arc)
    {
      return new Arc(arc.Center, arc.Radius, arc.DirStart + arc.Angle, -arc.Angle);
    }

    public static Arc Project(IArc arc, IProjection projection)
    { // TODO
      // only valid if only scaled and rotation
      IPoint center = PointOp.Project(arc.Center, projection);
      ArcCore a = Arc.CastOrWrap(arc);
      IPoint start = PointOp.Project(a.Start, projection);
      IPoint end = PointOp.Project(a.End, projection);
      double radius = Math.Sqrt(PointOp.Dist2(center, start));
      double dirStart = Math.Atan2(start.Y - center.Y, start.X - center.X);
      return new Arc(start, end, center, radius, dirStart, arc.Angle);
    }

    public static Arc Subpart(IArc arc, double t0, double t1)
    {
      return new Arc(arc.Center, arc.Radius,
        arc.DirStart + t0 * arc.Angle, (t1 - t0) * arc.Angle);
    }

    public static IList<ParamGeometryRelation> CutLine(IArc arc, ILine line)
    {
      // (x - cx)^2 + (y - cy)^2 = r^2
      // x = sx + t*dx
      // y = sy + t*dy
      Point dir = PointOp.Sub(line.End, line.Start);

      Point ortho = new P(dir.Y, -dir.X);
      Point center = new P(arc.Center.X, arc.Center.Y);
      ParamGeometryRelation rel = LineOp.CutLine(line, new Line(center, center + ortho));

      IPoint cut = (IPoint)rel.IntersectionAnywhere;

      double drOtho2 = center.Dist2(cut);
      double dr2 = arc.Radius * arc.Radius;
      double dCut2 = dr2 - drOtho2;
      if (dCut2 < 0)
      { return new List<ParamGeometryRelation>(); }

      double f0 = rel.XParam[0];
      double f1 = Math.Sqrt(dCut2 / dir.OrigDist2());
      List<ParamGeometryRelation> list = new List<ParamGeometryRelation>();
      double l0 = f0 + f1;
      double l1 = f0 - f1;
      Point arc0 = LineOp.PointAt(line, l0);
      Point arc1 = LineOp.PointAt(line, l1);
      double phi0 = Math.Atan2(arc0.Y - center.Y, arc0.X - center.X);
      double phi1 = Math.Atan2(arc1.Y - center.Y, arc1.X - center.X);
      double a0 = GetA(arc, phi0);
      double a1 = GetA(arc, phi1);

      LineCore line_ = Line.CastOrWrap(line);
      ArcCore arc_ = Arc.CastOrWrap(arc);
      IPoint pl, pa;
      ParamGeometryRelation r;
      pl = Point.Create(new double[] { l0 });
      pa = Point.Create(new double[] { a0 });
      r = new ParamGeometryRelation(line_, pl, arc_, pa, Point.Create_0(line.Dimension));
      if (r.IsWithin)
      { r.Intersection = arc0; }
      list.Add(r);

      pl = Point.Create(new double[] { l1 });
      pa = Point.Create(new double[] { a1 });
      r = new ParamGeometryRelation(line_, pl, arc_, pa, Point.Create_0(line.Dimension));
      if (r.IsWithin)
      { r.Intersection = arc1; }
      list.Add(r);
      return list;
    }

    private static readonly double _math2PI = 2 * Math.PI;
    private static double GetA(IArc arc, double phi)
    {
      double startAngle = phi - arc.DirStart;
      while (startAngle >= _math2PI)
      {
        startAngle -= _math2PI;
      }
      while (startAngle <= -_math2PI)
      {
        startAngle += _math2PI;
      }

      while (startAngle < 0 && arc.Angle > 0)
      {
        startAngle += _math2PI;
      }
      while (startAngle > 0 && arc.Angle < 0)
      {
        startAngle -= _math2PI;
      }
      double a = startAngle / arc.Angle;

      return a;
    }
    public static IList<ParamGeometryRelation> CutArc(IArc x, IArc y)
    {
      IGeometry intersect;
      ArcCore xCore = Arc.CastOrWrap(x);
      ArcCore yCore = Arc.CastOrWrap(y);
      if (PointOp.Dist2(x.Center, y.Center) == 0 && x.Radius == y.Radius)
      { // same circles
        // TODO: verify angles 2 PI
        double thisMin = Math.Min(x.DirStart, x.DirStart + x.Angle);
        double thisMax = Math.Max(x.DirStart, x.DirStart + x.Angle);

        double otherMin = Math.Min(y.DirStart, y.DirStart + y.Angle);
        double otherMax = Math.Max(y.DirStart, y.DirStart + y.Angle);

        double max = Math.Min(thisMax, otherMax);
        double min = Math.Max(thisMin, otherMin);

        if (max < min)
        { return null; }
        if (min == max)
        {
          if (x.Angle > 0)
          { intersect = Point.CastOrWrap(xCore.Start); }
          else
          { intersect = Point.CastOrWrap(xCore.End); }
        }
        else
        {
          if (x.Angle > 0)
          { intersect = new Arc(Point.Create(x.Center), x.Radius, min, max - min); }
          else
          { intersect = new Arc(Point.Create(x.Center), x.Radius, max, min - max); }
        }
        ParamGeometryRelation rel = new ParamGeometryRelation(xCore, yCore, intersect);
        return new ParamGeometryRelation[] { rel };
      } // end ident circle

      // Apply cosinus law
      Point delta = PointOp.Sub(y.Center, x.Center);
      double dist2 = delta.OrigDist2();
      double this2 = x.Radius * x.Radius;
      double other2 = y.Radius * y.Radius;
      double cos = (this2 + other2 - dist2) / (2 * x.Radius * y.Radius);
      if (cos < -1)
      { return null; }
      if (cos > 1)
      { return null; }

      intersect = null;
      if (cos == 1)
      {
        double f = x.Radius / (x.Radius - y.Radius);
        intersect = x.Center + f * delta;
      }
      else if (cos == -1)
      {
        double f = x.Radius / (x.Radius + y.Radius);
        intersect = x.Center + f * delta;
      }

      if (intersect != null)
      {
        ParamGeometryRelation rel = new ParamGeometryRelation(xCore, yCore, intersect);
        return new ParamGeometryRelation[] { rel };
      }

      double dDist = Math.Sqrt(dist2); // now we need the real distance
      cos = (this2 + dist2 - other2) / (2 * x.Radius * dDist);
      double dAlpha = Math.Acos(cos);
      double dDir = Math.Atan2(delta.Y, delta.X);

      List<ParamGeometryRelation> list = new List<ParamGeometryRelation>();

      ParamGeometryRelation rel0 = GetParamGeometryRelation(xCore, yCore, dDir + dAlpha);
      if (rel0 != null)
      { list.Add(rel0); }

      ParamGeometryRelation rel1 = GetParamGeometryRelation(xCore, yCore, dDir - dAlpha);
      if (rel1 != null)
      { list.Add(rel1); }

      return list;
    }

    private static ParamGeometryRelation GetParamGeometryRelation(ArcCore x, ArcCore y, double xAngle)
    {
      // TODO check other arc
      double xA = GetA(x, xAngle);
      if (xA < 0 || xA > 1)
      {
        return null;
      }
      Point i = PointAt(x, xA);
      double yAngle = Math.Atan2(i.Y - y.Center.Y, i.X - y.Center.X);

      double yA = GetA(y, yAngle);
      if (yA < 0 || yA > 1)
      {
        return null;
      }
      return new ParamGeometryRelation(x, Point.Create(new[] { xA }), y, Point.Create(new[] { yA }), Point.Create(new[] { 0.0 }));
    }
    public static Point TangentAt(IArc arc, double t)
    {
      Point p = PointAt(arc, t) - arc.Center;
      Point tangent = new P(-p.Y, p.X);
      return arc.Angle * tangent;
    }
  }
}
