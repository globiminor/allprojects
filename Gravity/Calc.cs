using Basics.Geom;
using System;
using System.Collections.Generic;
using GeomOp = Basics.Geom.GeometryOperator;

namespace Gravity
{
  public class Calc
  {
    private double _epsi = 1.0e-10;

    /// <summary>
    /// false: all gravity values will be calculated
    /// true: only gz and gzz will be calculated
    /// </summary>
    public bool OnlyZ { get; set; } = true;
    public double Epsi { get { return _epsi; } set { _epsi = value; } }

    private void GetLineParam(out double y0, out double a, out bool a0,
      double x1, double y1, double x2, double y2)
    {
      a0 = false;
      double dy = y2 - y1;
      double dx = x2 - x1;
      if (Math.Abs(dx) < _epsi)
      {
        a0 = true;
        y0 = double.NaN;
        a = double.NaN;
      }
      else
      {
        a = dy / dx;
        y0 = y1 - a * x1;
      }
    }

    private bool GetPlaneParam(out double z0, out double b, out double c, out bool d0,
      IPoint c0, IPoint c1, IPoint c2)
    {
      double dx1 = c1.X - c0.X;
      double dy1 = c1.Y - c0.Y;
      double dz1 = c1.Z - c0.Z;

      double dx2 = c2.X - c0.X;
      double dy2 = c2.Y - c0.Y;
      double dz2 = c2.Z - c0.Z;
      double sp = dx1 * dx2 + dy1 * dy2 + dz1 * dz2;
      sp = sp * sp;
      double m = (dx1 * dx1 + dy1 * dy1 + dz1 * dz1) * (dx2 * dx2 + dy2 * dy2 + dz2 * dz2);

      if (Math.Abs(sp - m) <= _epsi)
      {
        z0 = double.NaN;
        b = double.NaN;
        c = double.NaN;
        d0 = false;
        return false;
      }
      b = dy1 * dz2 - dy2 * dz1;
      c = dz1 * dx2 - dz2 * dx1;
      double d = dx1 * dy2 - dx2 * dy1;
      if (Math.Abs(d) <= _epsi * (Math.Abs(b) + Math.Abs(c)))
      {
        d0 = true;
        z0 = double.NaN;
      }
      else
      {
        d0 = false;
        b = -b / d;
        c = -c / d;
        z0 = -b * c0.X - c * c0.Y + c0.Z;
      }
      return true;
    }

    // this routine computes the gravity of a line belonging to
    //   a three dimensional volume according to the formulas of
    //   IGP-report 192, ETH Zurich, 1992
    private GravityValues GetLineGravity(
      double x1, double y1, double z1, double x2, double y2, double z2,
      double y0, double a, bool a0, double z0, double b, double c, bool d0,
      double r1, double r2)
    {
      GravityValues w = new GravityValues(OnlyZ);

      double rm = _epsi * (r1 + r2);
      double rm1 = _epsi * r1;
      double rm2 = _epsi * r2;

      double p;
      if (d0 == false)
      {
        if (a0 == false)
        {

          /* general case */

          double a1 = a * a + 1.0;
          double cba = c - b * a;
          double azby = a * z0 - b * y0;
          double bc1 = b * b + c * c + 1.0;
          double bca = b + c * a;
          double bcaa1 = Math.Sqrt(a1 + bca * bca);

          /* computing of p */

          if (Math.Abs(y0) <= rm && Math.Abs(z0) <= rm)
          {
            if (x1 > rm && x2 > rm)
              p = Math.Log(x2 / x1) / bcaa1;
            else
            {
              if (x1 < -rm && x2 < -rm)
                p = Math.Log(x1 / x2) / bcaa1;
              else
              {
                w.IsInfinit = true;
                p = 0.0;
              }
            }
          }
          else
          {
            double p2 = (x2 + a * y2 + bca * z2) / bcaa1 + r2;
            if (p2 <= rm2)
              p2 = -(y0 * y0 + (z0 + c * y0) * (z0 + c * y0) + azby * azby) /
                  ((x2 + a * y2 + bca * z2) * bcaa1 * 2.0);
            double p1 = (x1 + a * y1 + bca * z1) / bcaa1 + r1;
            if (p1 <= rm1)
              p1 = -(y0 * y0 + (z0 + c * y0) * (z0 + c * y0) + azby * azby) /
                  ((x1 + a * y1 + bca * z1) * bcaa1 * 2.0);
            p = Math.Log(p2 / p1) / bcaa1;
          }
          {
            double p_, p1_, p2_;
            double dx, dy, dz, dd;
            dx = x2 - x1;
            dy = y2 - y1;
            dz = z2 - z1;
            dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            p2_ = (dx * x2 + dy * y2 + dz * z2) / dd + r2;
            p1_ = (dx * x1 + dy * y1 + dz * z1) / dd + r1;
            p_ = dx / dd * Math.Log(p2_ / p1_);

            if (Math.Abs(p / p_ - 1.0) > 1.0e-7)
            {
              Console.WriteLine($"{p:14.8e} {p_:14.8e}", p, p_);
            }
          }

          /* computing of r */
          double r;
          double rn2 = (bc1 * y0 + cba * z0) * x2 - azby * z0;
          double rd2 = z0 * r2;
          if (Math.Abs(z0) <= rm && Math.Abs(x2) <= rm2)
          {
            rn2 = b;
            rd2 = Math.Sqrt(c * c + 1);
          }
          double rn1 = (bc1 * y0 + cba * z0) * x1 - azby * z0;
          double rd1 = z0 * r1;
          if (Math.Abs(z0) <= rm && Math.Abs(x1) <= rm1)
          {
            rn1 = b;
            rd1 = Math.Sqrt(c * c + 1);
          }
          if (Math.Abs(z0) <= rm && Math.Abs(y0) <= rm)
            r = 0.0;
          else
            r = Math.Atan2(rn2 * rd1 - rn1 * rd2, rn1 * rn2 + rd1 * rd2) / bc1;

          /* computing of the effects for the generall case */

          w.Gz = (y0 + cba * z0 / bc1) * p - z0 * r;
          w.Gzz = cba / bc1 * p - r;
          if (!OnlyZ)
          {
            /* computing of q */
            double qn2 = (a1 * z0 + cba * y0) * x2 + azby * y0;
            double qd2 = y0 * r2;
            if (Math.Abs(y0) <= rm && Math.Abs(x2) <= rm2)
            {
              qn2 = a;
              qd2 = 1;
            }
            double qn1 = (a1 * z0 + cba * y0) * x1 + azby * y0;
            double qd1 = y0 * r1;
            if (Math.Abs(y0) <= rm && Math.Abs(x1) <= rm1)
            {
              qn1 = a;
              qd1 = 1.0;
            }
            double q;
            if (Math.Abs(y0) <= rm && Math.Abs(z0) <= rm)
              q = 0;
            else
              q = Math.Atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2) / a1;

            w.G = (z0 * y0 + z0 * z0 * cba / (2 * bc1) + y0 * y0 * cba / (2 * a1)) * p
                - y0 * y0 / 2 * q - z0 * z0 / 2 * r;
            w.Gx = -(bca * y0 / a1 + (c * bca + a) * z0 / bc1) * p
                + a * y0 * q + b * z0 * r;
            w.Gy = ((b * bca + 1) * z0 / bc1 - a * bca * y0 / a1) * p
                - y0 * q + c * z0 * r;
            w.Gxx = (a * bca / a1 + b * (a + c * bca) / bc1) * p + q + (c * c + 1) * r;
            w.Gyy = -(a * bca / a1 + c * (1 + b * bca) / bc1) * p - q - c * c * r;
            w.Gxy = (-bca / a1 + c * (a + c * bca) / bc1) * p + a * q - b * c * r;
            w.Gxz = -(a + c * bca) / bc1 * p + b * r;
            w.Gyz = (1 + b * bca) / bc1 * p + c * r;
          }
        }
        else
        {

          /* a infinite */

          double x0 = (x1 + x2) / 2.0;
          double c1 = c * c + 1.0;
          double rc1 = Math.Sqrt(c1);
          double bc1 = b * b + c1;

          /* computing of p */

          if (Math.Abs(x0) <= rm && Math.Abs(z0) <= rm)
          {
            if (y1 > rm && y2 > rm)
              p = Math.Log(y2 / y1) / rc1;
            else
            {
              if (y1 < -rm && y2 < -rm)
                p = Math.Log(y1 / y2) / rc1;
              else
              {
                w.IsInfinit = true;
                p = 0.0;
              }
            }
          }
          else
          {
            double p2 = (y2 + c * z2) / rc1 + r2;
            if (p2 <= rm2)
              p2 = -(x0 * x0 * c1 + z0 * z0) / ((y2 + c * z2) * rc1 * 2.0);
            double p1 = (y1 + c * z1) / rc1 + r1;
            if (p1 <= rm1)
              p1 = -(x0 * x0 * c1 + z0 * z0) / ((y1 + c * z1) * rc1 * 2.0);
            p = Math.Log(p2 / p1) / rc1;
          }

          /* computing of the effects for a to infinite */

          w.Gz = (-x0 - b * z0 / bc1) * p;
          w.Gzz = -b / bc1 * p;
          if (!OnlyZ)
          {
            /* computing of q */
            double qn2 = c * x0 * x0 - (z0 + b * x0) * y2;
            double qd2 = x0 * r2;
            double qn1 = c * x0 * x0 - (z0 + b * x0) * y1;
            double qd1 = x0 * r1;
            double q;
            if (Math.Abs(x0) <= rm)
              q = 0.0;
            else
              q = Math.Atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2);

            w.G = (-z0 * x0 - b * z0 * z0 / (2.0 * bc1) - b * x0 * x0 / 2.0) * p - x0 * x0 / 2.0 * q;
            w.Gx = -(c1 * z0 / bc1) * p - x0 * q;
            w.Gy = (c * x0 + b * c * z0 / bc1) * p;
            w.Gxx = b * c1 / bc1 * p;
            w.Gyy = -b * c * c / bc1 * p;
            w.Gxy = c * c1 / bc1 * p;
            w.Gxz = -c1 / bc1 * p;
            w.Gyz = b * c / bc1 * p;
          }
        }
      }
      else
      {
        if (!OnlyZ)
        {

          /*  z0 to infinite */

          if (a0)
          {
            double x0 = (x1 + x2) / 2.0;
            if (Math.Abs(y2 - y1) <= rm)
            {
              y0 = (y1 + y2) / 2.0;
              if (Math.Abs(x0) <= rm && Math.Abs(y0) <= rm)
              {
                if (z1 > rm && z2 > rm)
                  p = Math.Log(z2 / z1);
                else
                {
                  if (z1 < -rm && z2 < -rm)
                    p = Math.Log(z1 / z2);
                  else
                  {
                    p = 0.0;
                    w.IsInfinit = true;
                  }
                }
              }
              else
              {
                double p2 = z2 + r2;
                if (p2 <= rm2) p2 = -(x0 * x0 + y0 * y0) / (z2 * 2.0);
                double p1 = z1 + r1;
                if (p1 <= rm1) p1 = -(x0 * x0 + y0 * y0) / (z1 * 2.0);
                p = Math.Log(p2 / p1);
              }

              if (Math.Abs(c) > rm)
              {
                double as_ = -b / c;
                double a1 = as_ * as_ + 1.0;
                w.G = (-as_ * x0 * x0 + 2.0 * x0 * y0 + as_ * y0 * y0) / (2.0 * a1) * p;
                w.Gx = -(as_ * x0 - y0) / a1 * p;
                w.Gy = (x0 + as_ * y0) / a1 * p;
                w.Gxx = -as_ / a1 * p;
                w.Gyy = as_ / a1 * p;
                w.Gxy = p / a1;
              }
              else
              { }
            }
          }
        }
      }

      return w;
    }

    private GravityValues GrvLine(IPoint s0, IPoint c0, IPoint c1,
      double b, double c, bool d0, double rho)
    {
      double x1 = c0.X - s0.X;
      double y1 = c0.Y - s0.Y;
      double z1 = c0.Z - s0.Z;
      double r1 = Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
      double x2 = c1.X - s0.X;
      double y2 = c1.Y - s0.Y;
      double z2 = c1.Z - s0.Z;
      double r2 = Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2);
      double z0 = z1 - b * x1 - c * y1;
      GetLineParam(out double y0, out double a, out bool a0, x1, y1, x2, y2);
      GravityValues w = GetLineGravity(x1, y1, z1, x2, y2, z2, y0, a, a0, z0, b, c, d0, r1, r2);
      w.ApplyDensity(rho);
      return w;
    }

    private GravityValues GrvLineAppAdd0(double x1, double y1, double x2, double y2,
      double l1, double l2, double m1, double m2)
    {
      GetLineParam(out double y0, out double a, out bool a0, x1, y1, x2, y2);

      GravityValues w = new GravityValues(OnlyZ);
      double p;
      if (!a0)
      {
        double a1 = a * a + 1.0;
        double at = Math.Atan2(y1 * x2 - y2 * x1, x1 * x2 + y1 * y2);
        p = (l2 - l1 - (m2 - m1) / 2.0) / a1;

        w.G = +(a * y2 * y2 - a * x2 * x2 + 2.0 * x2 * y2) / (2.0 * a1) * l2
              - (a * y1 * y1 - a * x1 * x1 + 2.0 * x1 * y1) / (2.0 * a1) * l1
              - 3.0 / 4.0 * (x2 - x1) * (y2 + y1)
              + y0 * y0 / (a1 * 2.0) * at;

        w.Gx = y0 * p;
        w.Gy = a * y0 * p;
        w.Gz = 0.0;

        w.Gxx = -a * p;
        w.Gyy = a * p;
        w.Gzz = 0.0;
        w.Gxy = p;
        w.Gxz = 0.0;
        w.Gyz = 0.0;
      }
      else
      {
        double at = Math.Atan2(y1 * x2 - y2 * x1, x1 * x2 + y1 * y2);
        p = (l2 - l1 - (m2 - m1) / 2.0);

        w.G = +x1 * x1 / 2.0 * at;

        w.Gx = 0.0;
        w.Gy = -x1 * p;
        w.Gz = 0.0;

        w.Gxx = 0.0;
        w.Gyy = 0.0;
        w.Gzz = 0.0;
        w.Gxy = 0.0;
        w.Gxz = 0.0;
        w.Gyz = 0.0;
      }
      return w;
    }

    private GravityValues GetPlaneGravity(IPoint s0, ISimpleArea pl,
      double z0, double b, double c, bool d0)
    {
      GravityValues w = new GravityValues(OnlyZ);

      IPoint p2 = null;
      double r2 = 0;
      foreach (var border in pl.Border)
      {
        foreach (var p in border.Points)
        {
          IPoint p1 = p2;
          double r1 = r2;

          p2 = PointOp.Sub(p, s0, GeomOp.DimensionXYZ);
          r2 = Math.Sqrt(PointOp.Dist2(p2, null, dimensions: GeomOp.DimensionXYZ));

          if (p1 == null)
          {
            if (d0 == false)
            {
              double rm = _epsi * (Math.Abs(b) + Math.Abs(c)) / 2.0;
              z0 = -b * p2.X - c * p2.Y + p2.Z;
              if (Math.Abs(z0) <= rm) z0 = 0.0;
            }
            continue;
          }

          GetLineParam(out double y0, out double a, out bool a0, p1.X, p1.Y, p2.X, p2.Y);
          GravityValues v = GetLineGravity(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z,
                y0, a, a0, z0, b, c, d0, r1, r2);
          w = w.Add(v);
        }
      }
      return w;
    }

    GravityValues GetPlaneGravity(IPoint s0, ISimpleArea pl, double rho)
    {
      IPoint[] e = new IPoint[3];

      foreach (var border in pl.Border)
      {
        foreach (var p in border.Points)
        {
          if (e[0] == null)
          {
            e[0] = p;
            continue;
          }
          e[1] = e[2];
          e[2] = p;
          if (e[1] == null)
          { continue; }

          if (GetPlaneParam(out double z0, out double b, out double c, out bool d0, e[0], e[1], e[2]))
          {
            GravityValues w = GetPlaneGravity(s0, pl, z0, b, c, d0);
            w.ApplyDensity(rho);
            return w;
          }
        }
      }
      throw new InvalidOperationException("No valid plane parameters found");
    }

    private GravityValues GrvPlane0(IPoint s0, IList<IPoint> co, int n, double z0, double b, double c,
      bool d0)
    {
      GravityValues w = new GravityValues(OnlyZ);

      double xs = co[0].X - s0.X;
      double ys = co[0].Y - s0.Y;
      double zs = co[0].Z - s0.Z;
      double rs = Math.Sqrt(xs * xs + ys * ys + zs * zs);
      if (d0 == false)
      {
        double rm = _epsi * (Math.Abs(b) + Math.Abs(c)) / 2.0;
        z0 = -b * xs - c * ys + zs;
        if (Math.Abs(z0) <= rm) z0 = 0.0;
      }

      double x2 = xs;
      double y2 = ys;
      double z2 = zs;
      double r2 = rs;
      for (int i = 1; i < n; i++)
      {
        double x1 = x2;
        double y1 = y2;
        double z1 = z2;
        double r1 = r2;

        x2 = co[i].X - s0.X;
        y2 = co[i].Y - s0.Y;
        z2 = co[i].Z - s0.Z;
        r2 = Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2);

        GetLineParam(out double y0, out double a, out bool a0, x1, y1, x2, y2);
        GravityValues v = GetLineGravity(x1, y1, z1, x2, y2, z2,
              y0, a, a0, z0, b, c, d0, r1, r2);
        w = w.Add(v);
      }
      {
        GetLineParam(out double y0, out double a, out bool a0, x2, y2, xs, ys);
        GravityValues v = GetLineGravity(x2, y2, z2, xs, ys, zs,
          y0, a, a0, z0, b, c, d0, r2, rs);
        w = w.Add(v);
      }
      return w;
    }

    GravityValues GrvPlane(IPoint s0, IList<IPoint> co, int n, double rho)
    {
      double z0, b, c;
      bool d0;

      int i = 2;
      while (!GetPlaneParam(out z0, out b, out c, out d0, co[0], co[1], co[i]))
      {
        i++;
      }
      GravityValues w = GrvPlane0(s0, co, n, z0, b, c, d0);
      w.ApplyDensity(rho);
      return w;
    }


    GravityValues GrvPlaneApp(IPoint s0, IPoint m, double df, double rho)
    {
      IPoint d = PointOp.Sub(m, s0);

      double s = d.X * d.X + d.Y * d.Y;
      double r = Math.Sqrt(s + d.Z * d.Z);
      GravityValues w = new GravityValues(OnlyZ);
      w.G = Math.Log(d.Z + r) * df;
      w.Gx = d.X * d.Z / (s * r) * df;
      w.Gy = d.Y * d.Z / (s * r) * df;
      w.Gz = df / r;
      w.Gxx = d.Z * df / (s * r) * (-1.0 + d.X * d.X * (1.0 / (r * r) + 2.0 / s));
      w.Gyy = d.Z * df / (s * r) * (-1.0 + d.Y * d.Y * (1.0 / (r * r) + 2.0 / s));
      w.Gzz = d.Z * df / (r * r * r);
      w.Gxy = d.X * d.Y * d.Z * df / (s * r) * (1.0 / (r * r) + 2.0 / s);
      w.Gxz = d.X * df / (r * r * r);
      w.Gyz = d.Y * df / (r * r * r);

      w.ApplyDensity(rho);

      return w;
    }

    double LogTerm(double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double rm = _epsi * (r1 + r2);
      double rm1 = _epsi * r1;
      double rm2 = _epsi * r2;

      double p;
      if (Math.Abs(y0) < rm && Math.Abs(z0) < rm)
      {
        if (x1 > rm && x2 > rm)
        {
          p = Math.Log(x2 / x1);
        }
        else
        {
          if (x1 < -rm && x2 < -rm)
          {
            p = Math.Log(x1 / x2);
          }
          else
          {
            p = double.PositiveInfinity;
          }
        }
      }
      else
      {
        double p2 = x2 + r2;
        if (p2 < rm2) p2 = -(y0 * y0 + z0 * z0) / (x2 * 2.0);
        double p1 = x1 + r1;
        if (p1 < rm1) p1 = -(y0 * y0 + z0 * z0) / (x1 * 2.0);
        p = Math.Log(p2 / p1);
      }
      return p;
    }


    private double ArcTerm(double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double rm = _epsi * (r1 + r2);

      double qn2 = z0 * x2;
      double qd2 = y0 * r2;
      double qn1 = z0 * x1;
      double qd1 = y0 * r1;

      double q;
      if (Math.Abs(y0) < rm)
      {
        q = 0.0;
      }
      else
      {
        q = Math.Atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2);
      }
      return q;
    }


    private GravityValues GetSideX(double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double p = LogTerm(x1, x2, y0, z0, r1, r2);
      double r = ArcTerm(x1, x2, z0, y0, r1, r2);

      GravityValues w = new GravityValues(OnlyZ);
      if (double.IsInfinity(p))
      { p = 0; w.IsInfinit = true; }

      w.Gz = -y0 * p + z0 * r;
      w.Gzz = +r;
      if (!OnlyZ)
      {
        double q = ArcTerm(x1, x2, y0, z0, r1, r2);

        w.G = -z0 * y0 * p + y0 * y0 / 2 * q + z0 * z0 / 2 * r;
        w.Gx = 0.0;
        w.Gy = -z0 * p + y0 * q;
        w.Gxx = -q - r;
        w.Gyy = +q;
        w.Gxy = 0.0;
        w.Gxz = 0.0;
        w.Gyz = -p;
      }
      return w;
    }

    GravityValues GetSideY(double y1, double y2, double x0, double z0, double r1, double r2)
    {
      double p = LogTerm(y1, y2, x0, z0, r1, r2);

      GravityValues w = new GravityValues(OnlyZ);
      if (double.IsInfinity(p))
      { p = 0; w.IsInfinit = true; }
      w.Gz = -x0 * p;
      w.Gzz = 0.0;
      if (!OnlyZ)
      {
        double q = ArcTerm(y1, y2, x0, -z0, r1, r2);

        w.G = -z0 * x0 * p - x0 * x0 / 2.0 * q;
        w.Gx = -z0 * p - x0 * q;
        w.Gy = 0.0;
        w.Gxx = 0.0;
        w.Gyy = 0.0;
        w.Gxy = 0.0;
        w.Gxz = -p;
        w.Gyz = 0.0;
      }
      return w;
    }

    GravityValues GetSideZ(double z1, double z2, double x0, double y0, double r1, double r2)
    {
      double p = LogTerm(z1, z2, x0, y0, r1, r2);
      GravityValues w = new GravityValues(OnlyZ);
      if (double.IsInfinity(p))
      { p = 0; w.IsInfinit = true; }

      w.G = -x0 * y0 * p;
      w.Gx = -y0 * p;
      w.Gy = -x0 * p;
      w.Gz = 0.0;
      w.Gxx = 0.0;
      w.Gyy = 0.0;
      w.Gzz = 0.0;
      w.Gxy = -p;
      w.Gxz = 0.0;
      w.Gyz = 0.0;

      return w;
    }

    public GravityValues GetQuaderGravity(double x1, double y1, double z1,
      double x2, double y2, double z2, double rho)
    {
      double d111 = Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
      double d112 = Math.Sqrt(x1 * x1 + y1 * y1 + z2 * z2);
      double d121 = Math.Sqrt(x1 * x1 + y2 * y2 + z1 * z1);
      double d122 = Math.Sqrt(x1 * x1 + y2 * y2 + z2 * z2);
      double d211 = Math.Sqrt(x2 * x2 + y1 * y1 + z1 * z1);
      double d212 = Math.Sqrt(x2 * x2 + y1 * y1 + z2 * z2);
      double d221 = Math.Sqrt(x2 * x2 + y2 * y2 + z1 * z1);
      double d222 = Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2);

      GravityValues w = GetSideX(x1, x2, y1, z1, d111, d211);
      w = w.Add(GetSideX(x2, x1, y1, z2, d212, d112));
      w = w.Add(GetSideX(x2, x1, y2, z1, d221, d121));
      w = w.Add(GetSideX(x1, x2, y2, z2, d122, d222));

      w = w.Add(GetSideY(y1, y2, x1, z1, d111, d121));
      w = w.Add(GetSideY(y2, y1, x1, z2, d122, d112));
      w = w.Add(GetSideY(y2, y1, x2, z1, d221, d211));
      w = w.Add(GetSideY(y1, y2, x2, z2, d212, d222));

      if (!OnlyZ)
      {
        w = w.Add(GetSideZ(z1, z2, x1, y1, d111, d112));
        w = w.Add(GetSideZ(z2, z1, x1, y2, d122, d121));
        w = w.Add(GetSideZ(z2, z1, x2, y1, d212, d211));
        w = w.Add(GetSideZ(z1, z2, x2, y2, d221, d222));
      }

      w.ApplyDensity(rho);
      return w;
    }


    public GravityValues GetVolumeGravity(IPoint s0, IVolume vm, double rho)
    {
      GravityValues w = new GravityValues(OnlyZ);

      foreach (var signedPlane in vm.Planes)
      {
        double vt;
        if (signedPlane.Sign > 0)
          vt = 1;
        else
          vt = -1;

        GravityValues v = GetPlaneGravity(s0, signedPlane.Area, vt * rho);
        w = w.Add(v);
      }
      return w;
    }

    public GravityValues GetPointGravity(double x, double y, double z, double m)
    {
      GravityValues w = new GravityValues(OnlyZ);
      double r = Math.Sqrt(x * x + y * y + z * z);
      if (r > 0.0)
      {
        double r2 = r * r;
        double r3 = r * r2;
        double r5 = r2 * r3;
        w.Gz = z / r3;
        w.Gzz = 3.0 * z * z / r5 - 1.0 / r3;
        if (!OnlyZ)
        {
          w.G = 1.0 / r;
          w.Gx = x / r3;
          w.Gy = y / r3;
          w.Gxx = 3.0 * x * x / r5 - 1.0 / r3;
          w.Gyy = 3.0 * y * y / r5 - 1.0 / r3;
          w.Gxy = 3.0 * x * y / r5;
          w.Gxz = 3.0 * x * z / r5;
          w.Gyz = 3.0 * y * z / r5;
        }
      }
      else
      {
        w.IsInfinit = true;
        w.Gz = 1.0e20;
        w.Gzz = 1.0e20;
        if (!OnlyZ)
        {
          w.G = 1.0e20;
          w.Gx = 1.0e20;
          w.Gy = 1.0e20;
          w.Gxx = 1.0e20;
          w.Gyy = 1.0e20;
          w.Gzz = 1.0e20;
          w.Gxy = 1.0e20;
          w.Gxz = 1.0e20;
          w.Gyz = 1.0e20;
        }
      }
      w.ApplyDensity(m);
      return w;
    }

    public GravityValues GetGravitySum(IPoint s0, Dictionary<int, IVolume> vms, IList<double> rho)
    {
      GravityValues w = new GravityValues(OnlyZ);

      foreach (var pair in vms)
      {
        IVolume vm = pair.Value;
        GravityValues v = GetVolumeGravity(s0, vm, rho[pair.Key]);
        w = w.Add(v);
      }
      return w;
    }
  }
}
