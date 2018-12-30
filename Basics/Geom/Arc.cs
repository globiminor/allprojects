using System;
using System.Collections.Generic;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  public class Arc : Curve, IMultipartSegment, ISegment<Arc>
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

    private readonly IPoint _center;
    private readonly double _radius;
    private readonly double _dirStart;
    private readonly double _angle;

    private IPoint _start;
    private IPoint _end;
    private readonly bool _isQuadrant; // indicates if it is a subpart
    private double _maxOffset = -1;

    private Arc(IPoint start, IPoint end,
      IPoint center, double radius, double startDirection, double angle)
    {
      _center = center;
      _radius = radius;
      _dirStart = startDirection;
      _angle = angle;

      _start = start;
      _end = end;
    }

    public Arc(IPoint center, double radius, double startDirection, double angle)
      : this(center, radius, startDirection, angle, isQuadrant: false)
    { }
    private Arc(IPoint center,
      double radius, double startDirection, double angle, bool isQuadrant)
    {
      _center = center;
      _radius = radius;
      _dirStart = startDirection;
      _angle = angle;

      _isQuadrant = isQuadrant;
    }

    protected override Curve CloneCore()
    { return Clone(); }
    public new Arc Clone()
    {
      return new Arc(Point.Create(_center), _radius, _dirStart, _angle);
    }

    public IEnumerable<Bezier> EnumBeziers()
    {
      double dir0;
      double dir1;

      if (Math.Abs(_angle) < 2 * Math.PI - 0.1)
      {
        dir0 = _dirStart;
        if (_angle < 0)
        { dir0 += _angle; }
        dir0 = dir0 / (2 * Math.PI);
        dir0 = 4 * (dir0 - Math.Floor(dir0));

        dir1 = dir0 + 2 * Math.Abs(_angle) / Math.PI;
      }
      else
      {
        dir0 = 0;
        dir1 = 4;
      }

      double x = Center.X;
      double y = Center.Y;
      double r = Radius;

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
      if (_angle < 0)
      {
        parts.Reverse();
        for (int iPart = 0; iPart < parts.Count; iPart++)
        { parts[iPart] = parts[iPart].Invert(); }
      }
      return parts;
    }
    private Bezier GetQuart(int quart, double x, double y, double r)
    {
      int q = quart % 4;
      double t = 0.55 * r;

      if (q == 0) return new Bezier(new Point2D(x, y + r), new Point2D(x + t, y + r), new Point2D(x + r, y + t), new Point2D(x + r, y));
      if (q == 1) return new Bezier(new Point2D(x + r, y), new Point2D(x + r, y - t), new Point2D(x + t, y - r), new Point2D(x, y - r));
      if (q == 2) return new Bezier(new Point2D(x, y - r), new Point2D(x - t, y - r), new Point2D(x - r, y - t), new Point2D(x - r, y));
      if (q == 3) return new Bezier(new Point2D(x - r, y), new Point2D(x - r, y + t), new Point2D(x - t, y + r), new Point2D(x, y + r));

      return null;
    }

    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is Arc o))
      { return false; }
      bool equal =
        _center.EqualGeometry(o._center) &&
        _radius == o._radius &&
        _dirStart == o._dirStart &&
        _angle == o._angle;
      return equal;
    }

    public override IList<ParamGeometryRelation> CreateRelations(IParamGeometry other,
      TrackOperatorProgress trackProgress)
    {
      IList<ParamGeometryRelation> list;
      if (other is Curve o)
      { list = CreateRelations(o, trackProgress); }
      else
      { list = base.CreateRelations(other, trackProgress); }
      return list;
    }
    protected override IList<ParamGeometryRelation> CreateRelations(Curve other,
      TrackOperatorProgress trackProgress)
    {
      IList<ParamGeometryRelation> result;
      if (other is Line line)
      {
        IList<ParamGeometryRelation> list = CutLine(line);
        result = AddRelations(list, trackProgress);
        return result;

      }
      if (other is Arc arc)
      {
        IList<ParamGeometryRelation> list = CutArc(arc);
        result = AddRelations(list, trackProgress);
        return result;
      }

      result = base.CreateRelations(other, trackProgress);
      return result;

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

          angle += p * subpart._angle;

          return angle / _angle;
        }
        angle += subpart._angle;
      }
      throw new InvalidProgramException();
    }

    public IPoint Center { get { return _center; } }

    public double Radius { get { return _radius; } }

    public override IPoint Start
    {
      get
      {
        if (_start == null)
        {
          _start = _center + (new Point2D(_radius * Math.Cos(_dirStart),
            _radius * Math.Sin(_dirStart)));
        }
        return _start;
      }
    }

    public override IPoint End
    {
      get
      {
        if (_end == null)
        {
          _end = _center + (new Point2D(_radius * Math.Cos(_dirStart + _angle),
            _radius * Math.Sin(_dirStart + _angle)));
        }
        return _end;
      }
    }

    protected override IList<IPoint> LinearizeCore(double offset, bool includeFirstPoint)
    {
      if (offset > _radius)
      { return new IPoint[] { Start, PointAt(0.5), End }; }

      double phi = 2 * Math.Acos(1 - offset / _radius);
      if (phi >= Math.Abs(_angle))
      { return new IPoint[] { Start, PointAt(0.5), End }; }

      int n = (int)Math.Ceiling((Math.Abs(_angle) / phi));
      List<IPoint> list = new List<IPoint>(n + 1);

      if (includeFirstPoint)
      { list.Add(Start); }

      double at = 1.0 / n;
      for (int i = 1; i < n; i++)
      { list.Add(PointAt(at * i)); }

      list.Add(End);

      return list;
    }

    public double Diameter
    {
      get
      { return 2 * _radius; }
    }
    public override IBox Extent
    {
      get
      {
        Box box = new Box(Point.Create(Start), Point.Create(End), true);
        if (HasSubparts)
        {
          foreach (var subPart in Subparts())
          { box.Include(subPart.Extent); }
        }
        return box;
      }
    }

    /// <summary>
    /// Punkt bei Parameter t
    /// </summary>
    /// <param name="t">Wert zwischen 0 und 1</param>
    /// <returns></returns>
    public override Point PointAt(double t)
    {
      double dDir = _dirStart + t * _angle;
      return new Point2D(_center.X + _radius * Math.Cos(dDir),
        _center.Y + _radius * Math.Sin(dDir));
    }

    public override double ParamAt(double distance)
    {
      return distance / Length();
    }

    public override double Length()
    {
      return Math.Abs(_radius * _angle);
    }

    public override bool IsLinear
    {
      get { return false; }
    }
    protected override double NormedMaxOffsetCore
    {
      get
      {
        if (_maxOffset < 0)
        {
          // TODO: verify
          Point p = PntOp.Sub(End, Start);
          double p2 = p.OrigDist2();
          Point m = 2.0 * (PointAt(0.5) - Start);
          double m2 = m.OrigDist2();

          _maxOffset = Math.Sqrt((m2 - p2) / p2);
        }
        return _maxOffset;
      }
    }

    protected override Curve InvertCore() => Invert();
    public Arc Invert()
    {
      return new Arc(_center, _radius, _dirStart + _angle, -_angle);
    }

    protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
    public Arc Project(IProjection projection)
    { // TODO
      // only valid if only scaled and rotation
      IPoint center = _center.Project(projection);
      IPoint start = Start.Project(projection);
      IPoint end = End.Project(projection);
      double radius = Math.Sqrt(PntOp.Dist2(center, start));
      double dirStart = Math.Atan2(start.Y - center.Y, start.X - center.X);
      return new Arc(start, end, center, radius, dirStart, _angle);
    }

    public new Arc Subpart(double t0, double t1)
    {
      return new Arc(_center, _radius,
        _dirStart + t0 * _angle, _dirStart + t1 * _angle);
    }
    protected override Curve SubpartCurve(double t0, double t1)
    { return Subpart(t0, t1); }


    public override InnerCurve InnerCurve()
    {
      return new _InnerCurve(_center, _radius, _dirStart, _angle);
    }

    public IList<ParamGeometryRelation> CutLine(Line line)
    {
      // (x - cx)^2 + (y - cy)^2 = r^2
      // x = sx + t*dx
      // y = sy + t*dy
      Point dir = PntOp.Sub(line.End, line.Start);

      Point ortho = new Point2D(dir.Y, -dir.X);
      Point center = new Point2D(_center.X, _center.Y);
      ParamGeometryRelation rel = line.CutLine(new Line(center, center + ortho));

      IPoint cut = (IPoint)rel.IntersectionAnywhere;

      double drOtho2 = center.Dist2(cut);
      double dr2 = _radius * _radius;
      double dCut2 = dr2 - drOtho2;
      if (dCut2 < 0)
      { return null; }

      double f0 = rel.XParam[0];
      double f1 = Math.Sqrt(dCut2 / dir.OrigDist2());
      List<ParamGeometryRelation> list = new List<ParamGeometryRelation>();
      double l0 = f0 + f1;
      double l1 = f0 - f1;
      IPoint arc0 = line.PointAt(l0);
      IPoint arc1 = line.PointAt(l1);
      double phi0 = Math.Atan2(arc0.Y - center.Y, arc0.X - center.X);
      double phi1 = Math.Atan2(arc1.Y - center.Y, arc1.X - center.X);
      double a0 = GetA(phi0);
      double a1 = GetA(phi1);

      IPoint pl, pa;
      ParamGeometryRelation r;
      pl = Point.Create(new double[] { l0 });
      pa = Point.Create(new double[] { a0 });
      r = new ParamGeometryRelation(line, pl, this, pa, Point.Create(line.Dimension));
      if (r.IsWithin)
      { r.Intersection = arc0; }
      list.Add(r);

      pl = Point.Create(new double[] { l1 });
      pa = Point.Create(new double[] { a1 });
      r = new ParamGeometryRelation(line, pl, this, pa, Point.Create(line.Dimension));
      if (r.IsWithin)
      { r.Intersection = arc1; }
      list.Add(r);
      return list;
    }

    private static readonly double _math2PI = 2 * Math.PI;
    private double GetA(double phi)
    {
      double startAngle = phi - _dirStart;
      while (startAngle >= _math2PI)
      {
        startAngle -= _math2PI;
      }
      while (startAngle <= -_math2PI)
      {
        startAngle += _math2PI;
      }

      while (startAngle < 0 && _angle > 0)
      {
        startAngle += _math2PI;
      }
      while (startAngle > 0 && _angle < 0)
      {
        startAngle -= _math2PI;
      }
      double a = startAngle / _angle;

      return a;
    }
    public IList<ParamGeometryRelation> CutArc(Arc other)
    {
      IGeometry intersect;
      if (PntOp.Dist2(_center, other._center) == 0 && _radius == other._radius)
      { // same circles
        // TODO: verify angles 2 PI
        double thisMin = Math.Min(_dirStart, _dirStart + _angle);
        double thisMax = Math.Max(_dirStart, _dirStart + _angle);

        double otherMin = Math.Min(other._dirStart, other._dirStart + other._angle);
        double otherMax = Math.Max(other._dirStart, other._dirStart + _angle);

        double max = Math.Min(thisMax, otherMax);
        double min = Math.Max(thisMin, otherMin);

        if (max < min)
        { return null; }
        if (min == max)
        {
          if (_angle > 0)
          { intersect = Start; }
          else
          { intersect = End; }
        }
        else
        {
          if (_angle > 0)
          { intersect = new Arc(Point.Create(_center), _radius, min, max - min); }
          else
          { intersect = new Arc(Point.Create(_center), _radius, max, min - max); }
        }
        ParamGeometryRelation rel = new ParamGeometryRelation(this, other, intersect);
        return new ParamGeometryRelation[] { rel };
      } // end ident circle

      // Apply cosinus law
      Point delta = PntOp.Sub(other._center, _center);
      double dist2 = delta.OrigDist2();
      double this2 = _radius * _radius;
      double other2 = other._radius * other._radius;
      double cos = this2 + other2 - 2 * _radius * other._radius - dist2;
      if (cos < -1)
      { return null; }
      if (cos > 1)
      { return null; }

      intersect = null;
      if (cos == 1)
      {
        double f = _radius / (_radius - other._radius);
        intersect = _center + f * delta;
      }
      else if (cos == -1)
      {
        double f = _radius / (_radius + other._radius);
        intersect = _center + f * delta;
      }

      if (intersect != null)
      {
        ParamGeometryRelation rel = new ParamGeometryRelation(this, other, intersect);
        return new ParamGeometryRelation[] { rel };
      }

      double dDist = Math.Sqrt(dist2); // now we need the real distance
      cos = this2 + dist2 - 2 * _radius * dDist - other2;
      double dAlpha = Math.Acos(cos);
      double dDir = Math.Atan2(delta.Y, delta.X);
      double angle = dDir + dAlpha;

      // TODO check other arc
      List<ParamGeometryRelation> list = new List<ParamGeometryRelation>();
      if (angle >= _dirStart == _dirStart + _angle >= angle)
      { list.Add(new ParamGeometryRelation(this, other, PointAt(angle - _dirStart / _angle))); }
      angle = dDir - dAlpha;
      if (angle >= _dirStart == _dirStart + _angle >= angle)
      { list.Add(new ParamGeometryRelation(this, other, PointAt(angle - _dirStart / _angle))); }

      return list;
    }

    public override Point TangentAt(double t)
    {
      Point p = PointAt(t) - _center;
      Point tangent = new Point2D(-p.Y, p.X);
      return _angle * tangent;
    }


    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get
      { return true; }
    }
    public bool HasSubparts
    {
      get
      {
        if (_isQuadrant)
        { return false; }

        double qStart = Math.Floor(_dirStart / (0.5 * Math.PI));
        double qEnd = Math.Floor((_dirStart + _angle) / (0.5 * Math.PI));
        return qStart - qEnd != 0;
      }
    }

    public IEnumerable<Arc> Subparts()
    {
      if (HasSubparts == false)
      { yield break; }

      if (_angle > 0)
      {
        double next = 0;
        double dirEnd = _dirStart + _angle;
        for (double dDir = _dirStart; dDir < dirEnd; dDir = next)
        {
          next = (1 + Math.Floor(dDir / (0.5 * Math.PI))) * 0.5 * Math.PI;
          next = Math.Min(next, dirEnd);
          yield return new Arc(_center, _radius, dDir, next - dDir, isQuadrant: true);
        }
      }
      else
      {
        double next = 0;
        double dirEnd = _dirStart + _angle;
        for (double dDir = _dirStart; dDir > dirEnd; dDir = next)
        {
          next = (Math.Ceiling(dDir / (0.5 * Math.PI)) - 1) * 0.5 * Math.PI;
          next = Math.Max(next, dirEnd);
          yield return new Arc(_center, _radius, dDir, next - dDir, isQuadrant: true);
        }
      }
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    #endregion
  }

}
