using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public abstract class KlothoideCore : Curve, IKlothoide, ISegment<Klothoide>
  {
    private class _InnerCurve : InnerCurve
    {
      private readonly IPoint _k0;
      private readonly double _angle;
      private readonly double _a;
      private readonly double _l0;
      private readonly double _l1;

      public _InnerCurve(IPoint k0, double angle, double a, double l0, double l1)
      {
        _k0 = k0;
        _angle = angle;
        _a = a;
        _l0 = l0;
        _l1 = l1;
      }

      public override Curve Complete(IPoint start, IPoint end)
      {
        return new Klothoide(_k0, _angle, _a, _l0, _l1);
      }
    }

    private IPoint _ps;
    private IPoint _pe;

    private double _sin;
    private double _cos;
    private IPoint _k0;

    public sealed override IPoint Start => _ps ?? (_ps = PointAt(0));
    public sealed override IPoint End => _pe ?? (_pe = PointAt(1));

    public IPoint K0 => _k0 ?? GetK0();
    public double Angle => GetAngle();
    public double A => GetA();
    public double L0 => GetL0();
    public double L1 => GetL1();
    protected abstract IPoint GetK0();
    protected abstract double GetAngle();
    protected abstract double GetA();
    protected abstract double GetL0();
    protected abstract double GetL1();
    public override bool IsLinear => false;
    public override double Length(IReadOnlyCollection<int> dimensions = null) => KlothoideOp.Length(this, dimensions);
    protected override Curve InvertCore() => KlothoideOp.Invert(this);
    public Klothoide Invert() => KlothoideOp.Invert(this);

    protected override Curve CloneCore() => KlothoideOp.Clone(this);
    public new Klothoide Clone() => KlothoideOp.Clone(this);
    public override InnerCurve GetInnerCurve()
    {
      return new _InnerCurve(K0, Angle, A, L0, L1);
    }

    public override Point TangentAt(double param) => KlothoideOp.TangentAt(this, param);
    protected override Curve SubpartCurve(double t0, double t1) => KlothoideOp.Subpart(this, t0, t1);
    public new Klothoide Subpart(double t0, double t1) => KlothoideOp.Subpart(this, t0, t1);
    public override bool EqualGeometry(IGeometry other) => KlothoideOp.EqualGeometry(this, other as IKlothoide);
    protected override IBox GetExtent() => KlothoideOp.GetExtent(this);
    protected override IGeometry ProjectCore(IProjection projection) => KlothoideOp.Project(this, projection);
    protected override double NormedMaxOffsetCore => KlothoideOp.GetNormedMaxOffset(this);
    public override double ParamAt(double distance, IReadOnlyCollection<int> dimensions = null) => KlothoideOp.ParamAt(this, distance, dimensions);
    public override Point PointAt(double param)
    {
      IPoint local = GetLocalPointAt(param);

      if (_k0 == null)
      {
        double angle = GetAngle();
        _sin = Math.Sin(angle);
        _cos = Math.Sin(angle);
        _k0 = GetK0();
      }

      return new Point2D(_k0.X + _cos * local.X + _sin * local.Y,
           _k0.Y - _sin * local.X + _cos * local.Y);

    }
    private Point2D GetLocalPointAt(double param)
    {
      double l = L0 + param * (L1 - L0);
      double ft = (l * l) / (2 * A * A);
      double t0 = ft;
      double x0 = 1;
      double y0 = ft / 3;
      int i = 1;
      while (Math.Abs(t0) > epsi)
      {
        t0 = GetNextT(t0, ft, 2 * i);
        double dx = t0 / (4 * i + 1);
        t0 = GetNextT(t0, ft, 2 * i + 1);
        double dy = t0 / (4 * i + 3);
        if (i % 2 == 1)
        {
          x0 = x0 - dx;
          y0 = y0 - dy;
        }
        else
        {
          x0 = x0 + dx;
          y0 = y0 + dy;
        }

        i++;
      }

      return new Point2D(l * x0, l * y0);
    }
    private double GetNextT(double t0, double ft, int i)
    {
      return t0 * ft / i;
    }
  }
  public class KlothoideWrap : KlothoideCore
  {

    public IKlothoide Klothoide { get; }
    public KlothoideWrap(IKlothoide klothoide)
    {
      Klothoide = klothoide;
    }
    protected override IPoint GetK0() => Klothoide.K0;
    protected override double GetAngle() => Klothoide.Angle;
    protected override double GetA() => Klothoide.A;
    protected override double GetL0() => Klothoide.L0;
    protected override double GetL1() => Klothoide.L1;
  }
  public class Klothoide : KlothoideCore
  {
    private readonly IPoint _k0;
    private readonly double _angle;

    private readonly double _a;
    private readonly double _l0;
    private readonly double _l1;

    public Klothoide(IPoint k0, double angle,
      double a, double l0, double l1)
    {
      _k0 = k0;
      _angle = angle;
      _a = a;
      _l0 = l0;
      _l1 = l1;
    }

    protected override IPoint GetK0() => _k0;
    protected override double GetAngle() => _angle;

    protected override double GetA() => _a;
    protected override double GetL0() => _l0;
    protected override double GetL1() => _l1;
  }

  public static class KlothoideOp
  {
    public static Klothoide Clone(IKlothoide k)
    {
      return new Klothoide(k.K0, k.Angle, k.A, k.L0, k.L1);
    }
    public static KlothoideCore CastOrWrap(IKlothoide k) => k as KlothoideCore ?? new KlothoideWrap(k);
    public static Klothoide Invert(IKlothoide k)
    {
      return new Klothoide(k.K0, k.Angle, k.A, k.L1, k.L0);
    }

    public static double Length(IKlothoide k, IEnumerable<int> dimensions = null)
    {
      return Math.Abs(k.L1 - k.L0);
    }

    public static double ParamAt(IKlothoide k, double distance, IReadOnlyCollection<int> dimensions = null)
    {
      return (distance - k.L0) / (k.L1 - k.L0);
    }

    public static Point PointAt(IKlothoide k, double t)
    {
      KlothoideCore c = CastOrWrap(k);
      return c.PointAt(t);
    }
    public static Point TangentAt(IKlothoide k, double param)
    {
      double l = (param - k.L0) / (k.L1 - k.L0);
      double t = (l * l) / (2 * k.A * k.A);

      double angle = t + k.Angle;
      return new Point2D(Math.Cos(angle), Math.Sin(angle));
    }

    public static Klothoide Subpart(IKlothoide k, double t0, double t1)
    {
      double l0 = k.L0;
      double dl = k.L1 - k.L0;
      return new Klothoide(k.K0, k.Angle, k.A, l0 + t0 * dl, l0 + t1 * dl);
    }

    public static bool EqualGeometry(IKlothoide x, IKlothoide y)
    {
      if (x == null || y == null)
      { return false; }

      if (x.Angle != y.Angle
        || x.A != y.A
        || x.L0 != y.L0
        || x.L1 != y.L1)
      {
        return false;
      }
      if (!PointOp.EqualGeometry(x.K0, y.K0))
      { return false; }

      return true;
    }

    public static double RadiusAt(IKlothoide k, double par)
    {
      double l = k.L0 + par * (k.L1 - k.L0);
      double a = k.A;
      double r = a * a / l;
      return r;
    }

    public static Klothoide Project(IKlothoide k, IProjection projection)
    { // TODO
      // only valid if only scaled and rotation
      IPoint center = PointOp.Project(k.K0, projection);

      KlothoideCore a = KlothoideOp.CastOrWrap(k);
      double ds2 = PointOp.Dist2(a.Start, k.K0);
      double de2 = PointOp.Dist2(a.End, k.K0);
      double d2;
      IPoint p0;
      double ang0;
      if (ds2 >= de2)
      {
        d2 = ds2;
        p0 = PointOp.Project(a.Start, projection);
        ang0 = Math.Atan2(a.Start.Y - k.K0.Y, a.Start.X - k.K0.X);
      }
      else
      {
        d2 = de2;
        p0 = PointOp.Project(a.End, projection);
        ang0 = Math.Atan2(a.End.Y - k.K0.Y, a.End.X - k.K0.X);
      }
      double ang1 = Math.Atan2(p0.Y - center.Y, p0.X - center.X);

      double f = Math.Sqrt(PointOp.Dist2(p0, center) / PointOp.Dist2(a.Start, k.K0));
      Klothoide prj = new Klothoide(center, k.Angle + (ang1 - ang0), k.A / f, k.L0 / f, k.L1 / f);
      return prj;
    }

    public static Box GetExtent(IKlothoide k)
    {
      KlothoideCore a = KlothoideOp.CastOrWrap(k);

      double r0 = Math.Abs(RadiusAt(k, 0));
      double r1 = Math.Abs(RadiusAt(k, 1));
      double r = Math.Min(r0, r1);
      double r2 = r * r;

      IPoint s = a.Start;
      IPoint e = a.End;
      Point p = PointOp.Sub(e, s);
      double p2 = p.OrigDist2();
      Point c = PointOp.Add(s, 0.5 * p);
      double dl = Math.Sqrt(p2) / 2;
      if (r2 < p2 / 4)
      {
        Point d = new Point2D(dl, dl);
        return new Box(c - d, c + d);
      }
      double t = Math.Sqrt(r2 - p2 / 4);
      Box extent = BoxOp.Clone(Point.CastOrWrap(s));
      extent.Include(e);
      Point normal = t / (2 * dl) * new Point2D(p.Y, -p.X);
      Point cl = c + normal;
      // TODO: check sign of radius
      // Point cr = c - normal;
      // throw new NotImplementedException();
      if ((cl.X < s.X) != (cl.X < e.X))
      {
        double dy = (cl.Y < extent.Max.Y)
          ? cl.Y + r - extent.Max.Y
          : extent.Min.Y - (cl.Y - r);
        extent.Include((IPoint)new Point2D(cl.X, extent.Max.Y + dy));
        extent.Include((IPoint)new Point2D(cl.X, extent.Min.Y - dy));
      }
      if ((cl.Y < s.Y) != (cl.Y < e.Y))
      {
        double dx = (cl.X < extent.Max.X)
          ? cl.X + r - extent.Max.X
          : extent.Min.X - (cl.X - r);
        extent.Include((IPoint)new Point2D(extent.Max.X + dx, cl.Y));
        extent.Include((IPoint)new Point2D(extent.Min.X + dx, cl.Y));
      }

      return extent;
    }
    public static double GetNormedMaxOffset(IKlothoide k)
    {
      KlothoideCore a = KlothoideOp.CastOrWrap(k);

      double r0 = Math.Abs(RadiusAt(k, 0));
      double r1 = Math.Abs(RadiusAt(k, 1));
      double r = Math.Min(r0, r1);
      double r2 = r * r;

      Point p = PointOp.Sub(a.End, a.Start);
      double p2 = p.OrigDist2();
      if (r2 < p2 / 4)
      {
        return 0.5;
      }
      double maxOffset = (r - Math.Sqrt(r2 - p2 / 4)) / Math.Sqrt(p2);

      return maxOffset;
    }

  }
}
