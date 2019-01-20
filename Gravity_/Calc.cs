using System.Collections.Generic;
using System.IO;

namespace Gravity
{
  class Calc
  {
# include <math.h>
# include <stdio.h>
# include <malloc.h>

# include "dpool.h"
# include "potenz.h"

    char vi;
    static char grA = 1; /* grA = 1 : all gravity values will be calculated
                        grA = 0 : only gz will be calculated */

    char GetGrA()
    {
      return grA;
    }

    void SetGrA(char c)
    {
      grA = c;
    }

    coo3 Subcoo3(coo3 x, coo3 y)
    {
      coo3 z;
      z.x = x.x - y.x;
      z.y = x.y - y.y;
      z.z = x.z - y.z;

      return z;
    }

    void GetLineParam(double* y0, double* a, char* a0,
      double x1, double y1, double x2, double y2)
    {
      double dx, dy;

      *a0 = 0;
      dy = y2 - y1;
      dx = x2 - x1;
      if (fabs(dx) < epsi)
        *a0 = 1;
      else
      {
        *a = dy / dx;
        *y0 = y1 - *a * x1;
      }
    }

    char GetPlaneParam(double* z0, double* b, double* c, char* d0,
      coo3 c0, coo3 c1, coo3 c2)
    {
      double dx1, dx2, dy1, dy2, dz1, dz2, sp, m, d;

      dx1 = c1.x - c0.x;
      dy1 = c1.y - c0.y;
      dz1 = c1.z - c0.z;

      dx2 = c2.x - c0.x;
      dy2 = c2.y - c0.y;
      dz2 = c2.z - c0.z;
      sp = dx1 * dx2 + dy1 * dy2 + dz1 * dz2;
      sp = sp * sp;
      m = (dx1 * dx1 + dy1 * dy1 + dz1 * dz1) * (dx2 * dx2 + dy2 * dy2 + dz2 * dz2);

      if (fabs(sp - m) <= epsi)
      {
        return (0);
      }
      *b = dy1 * dz2 - dy2 * dz1;
      *c = dz1 * dx2 - dz2 * dx1;
      d = dx1 * dy2 - dx2 * dy1;
      if (fabs(d) <= epsi * (fabs(*b) + fabs(*c)))
        *d0 = 1;
      else
      {
        *d0 = 0;
        *b = -(*b) / d;
        *c = -(*c) / d;
        *z0 = -(*b) * c0.x - (*c) * c0.y + c0.z;
      }
      return (1);
    }

    int NullGrav(grv_val* w)
    {
      w->gz = 0;
      w->gzz = 0;
      if (grA)
      {
        w->g = 0;
        w->gx = 0;
        w->gy = 0;
        w->gxx = 0;
        w->gyy = 0;
        w->gxy = 0;
        w->gxz = 0;
        w->gyz = 0;
      }
      return 0;
    }

    int AddGrav(grv_val* z, grv_val* x, grv_val* y)
    {
      z->gz = x->gz + y->gz;
      z->gzz = x->gzz + y->gzz;
      if (grA)
      {
        z->g = x->g + y->g;
        z->gx = x->gx + y->gx;
        z->gy = x->gy + y->gy;
        z->gxx = x->gxx + y->gxx;
        z->gyy = x->gyy + y->gyy;
        z->gxy = x->gxy + y->gxy;
        z->gxz = x->gxz + y->gxz;
        z->gyz = x->gyz + y->gyz;
      }
      return 0;
    }

    int GrvUnit(grv_val* w, double rho)
    {

      w->gz *= +gr * rho * 1.e - 03;
      if (vi)
      {
        w->gzz = 1.e20;
      }
      else
      {
        w->gzz *= -gr * rho * 10.;
      }
      if (grA)
      {
        w->g *= -gr * rho * 1.e - 08;
        w->gx *= +gr * rho * 1.e - 03;
        w->gy *= +gr * rho * 1.e - 03;
        if (vi)
        {
          w->gxx = 1.e20;
          w->gyy = 1.e20;
          w->gxy = 1.e20;
          w->gxz = 1.e20;
          w->gyz = 1.e20;
        }
        else
        {
          w->gxx *= -gr * rho * 10.;
          w->gyy *= -gr * rho * 10.;
          w->gxy *= -gr * rho * 10.;
          w->gxz *= -gr * rho * 10.;
          w->gyz *= -gr * rho * 10.;
        }
      }
      return 0;
    }

    int GrvLine0(grv_val* w,
      double x1, double y1, double z1, double x2, double y2, double z2,
      double y0, double a, char a0, double z0, double b, double c, char d0,
      double r1, double r2)
    /* this routine computes the gravity of a line belonging to
       a three dimensional volume according to the formulas of
       IGP-report 192, ETH Zurich, 1992 */
    {
      double rm, rm1, rm2, a1, bc1, bcaa1, bca, cba, c1, rc1,as,x0,p1,p2,qn1,qn2,qd1,qd2;
      double rn1, rn2, rd1, rd2, p, q, r, azby;
      char as0;

      vi = 0;
      NullGrav(w);

      rm = epsi * (r1 + r2);
      rm1 = epsi * r1;
      rm2 = epsi * r2;
      if (d0 == 0)
      {
        if (a0 == 0)
        {

          /* general case */

          a1 = a * a + 1.;
          cba = c - b * a;
          azby = a * z0 - b * y0;
          bc1 = b * b + c * c + 1.;
          bca = b + c * a;
          bcaa1 = sqrt(a1 + bca * bca);

          /* computing of p */

          if (fabs(y0) <= rm && fabs(z0) <= rm)
          {
            if (x1 > rm && x2 > rm)
              p = log(x2 / x1) / bcaa1;
            else
            {
              if (x1 < -rm && x2 < -rm)
                p = log(x1 / x2) / bcaa1;
              else
              {
                vi = 1;
                p = 0.;
              }
            }
          }
          else
          {
            p2 = (x2 + a * y2 + bca * z2) / bcaa1 + r2;
            if (p2 <= rm2)
              p2 = -(y0 * y0 + (z0 + c * y0) * (z0 + c * y0) + azby * azby) /
                  ((x2 + a * y2 + bca * z2) * bcaa1 * 2.);
            p1 = (x1 + a * y1 + bca * z1) / bcaa1 + r1;
            if (p1 <= rm1)
              p1 = -(y0 * y0 + (z0 + c * y0) * (z0 + c * y0) + azby * azby) /
                  ((x1 + a * y1 + bca * z1) * bcaa1 * 2.);
            p = log(p2 / p1) / bcaa1;
          }
          {
            double p_, p1_, p2_;
            double dx, dy, dz, dd;
            dx = x2 - x1;
            dy = y2 - y1;
            dz = z2 - z1;
            dd = sqrt(dx * dx + dy * dy + dz * dz);
            p2_ = (dx * x2 + dy * y2 + dz * z2) / dd + r2;
            p1_ = (dx * x1 + dy * y1 + dz * z1) / dd + r1;
            p_ = dx / dd * log(p2_ / p1_);

            if (fabs(p / p_ - 1.) > 1.e - 7)
            {
              printf("%14.8e %14.8e\n", p, p_);
            }
          }

          /* computing of q */

          if (grA)
          {
            qn2 = (a1 * z0 + cba * y0) * x2 + azby * y0;
            qd2 = y0 * r2;
            if (fabs(y0) <= rm && fabs(x2) <= rm2)
            {
              qn2 = a;
              qd2 = 1;
            }
            qn1 = (a1 * z0 + cba * y0) * x1 + azby * y0;
            qd1 = y0 * r1;
            if (fabs(y0) <= rm && fabs(x1) <= rm1)
            {
              qn1 = a;
              qd1 = 1.;
            }
            if (fabs(y0) <= rm && fabs(z0) <= rm)
              q = 0;
            else
              q = atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2) / a1;
          }

          /* computing of r */

          rn2 = (bc1 * y0 + cba * z0) * x2 - azby * z0;
          rd2 = z0 * r2;
          if (fabs(z0) <= rm && fabs(x2) <= rm2)
          {
            rn2 = b;
            rd2 = sqrt(c * c + 1);
          }
          rn1 = (bc1 * y0 + cba * z0) * x1 - azby * z0;
          rd1 = z0 * r1;
          if (fabs(z0) <= rm && fabs(x1) <= rm1)
          {
            rn1 = b;
            rd1 = sqrt(c * c + 1);
          }
          if (fabs(z0) <= rm && fabs(y0) <= rm)
            r = 0.;
          else
            r = atan2(rn2 * rd1 - rn1 * rd2, rn1 * rn2 + rd1 * rd2) / bc1;

          /* computing of the effects for the generall case */

          w->gz = (y0 + cba * z0 / bc1) * p - z0 * r;
          w->gzz = cba / bc1 * p - r;
          if (grA)
          {
            w->g = (z0 * y0 + z0 * z0 * cba / (2 * bc1) + y0 * y0 * cba / (2 * a1)) * p
                - y0 * y0 / 2 * q - z0 * z0 / 2 * r;
            w->gx = -(bca * y0 / a1 + (c * bca + a) * z0 / bc1) * p
                + a * y0 * q + b * z0 * r;
            w->gy = ((b * bca + 1) * z0 / bc1 - a * bca * y0 / a1) * p
                - y0 * q + c * z0 * r;
            w->gxx = (a * bca / a1 + b * (a + c * bca) / bc1) * p + q + (c * c + 1) * r;
            w->gyy = -(a * bca / a1 + c * (1 + b * bca) / bc1) * p - q - c * c * r;
            w->gxy = (-bca / a1 + c * (a + c * bca) / bc1) * p + a * q - b * c * r;
            w->gxz = -(a + c * bca) / bc1 * p + b * r;
            w->gyz = (1 + b * bca) / bc1 * p + c * r;
          }
        }
        else
        {

          /* a infinite */

          x0 = (x1 + x2) / 2.;
          c1 = c * c + 1.;
          rc1 = sqrt(c1);
          bc1 = b * b + c1;

          /* computing of p */

          if (fabs(x0) <= rm && fabs(z0) <= rm)
          {
            if (y1 > rm && y2 > rm)
              p = log(y2 / y1) / rc1;
            else
            {
              if (y1 < -rm && y2 < -rm)
                p = log(y1 / y2) / rc1;
              else
              {
                vi = 1;
                p = 0.;
              }
            }
          }
          else
          {
            p2 = (y2 + c * z2) / rc1 + r2;
            if (p2 <= rm2)
              p2 = -(x0 * x0 * c1 + z0 * z0) / ((y2 + c * z2) * rc1 * 2.);
            p1 = (y1 + c * z1) / rc1 + r1;
            if (p1 <= rm1)
              p1 = -(x0 * x0 * c1 + z0 * z0) / ((y1 + c * z1) * rc1 * 2.);
            p = log(p2 / p1) / rc1;
          }

          /* computing of q */

          if (grA)
          {
            qn2 = c * x0 * x0 - (z0 + b * x0) * y2;
            qd2 = x0 * r2;
            qn1 = c * x0 * x0 - (z0 + b * x0) * y1;
            qd1 = x0 * r1;
            if (fabs(x0) <= rm)
              q = 0.;
            else
              q = atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2);
          }

          /* computing of the effects for a to infinite */

          w->gz = (-x0 - b * z0 / bc1) * p;
          w->gzz = -b / bc1 * p;
          if (grA)
          {
            w->g = (-z0 * x0 - b * z0 * z0 / (2.* bc1) - b * x0 * x0 / 2.) * p - x0 * x0 / 2.* q;
            w->gx = -(c1 * z0 / bc1) * p - x0 * q;
            w->gy = (c * x0 + b * c * z0 / bc1) * p;
            w->gxx = b * c1 / bc1 * p;
            w->gyy = -b * c * c / bc1 * p;
            w->gxy = c * c1 / bc1 * p;
            w->gxz = -c1 / bc1 * p;
            w->gyz = b * c / bc1 * p;
          }
        }
      }
      else
      {
        if (grA)
        {

          /*  z0 to infinite */

          if (a0)
          {
            x0 = (x1 + x2) / 2.;
            if (fabs(y2 - y1) <= rm)
            {
              y0 = (y1 + y2) / 2.;
              if (fabs(c) > rm)
              {
            as  = -b / c;
                as0 = 0;
              }
              else
                as0 = 1;
              if (fabs(x0) <= rm && fabs(y0) <= rm)
              {
                if (z1 > rm && z2 > rm)
                  p = log(z2 / z1);
                else
                {
                  if (z1 < -rm && z2 < -rm)
                    p = log(z1 / z2);
                  else
                  {
                    p = 0.;
                    vi = 1;
                  }
                }
              }
              else
              {
                p2 = z2 + r2;
                if (p2 <= rm2) p2 = -(x0 * x0 + y0 * y0) / (z2 * 2.);
                p1 = z1 + r1;
                if (p1 <= rm1) p1 = -(x0 * x0 + y0 * y0) / (z1 * 2.);
                p = log(p2 / p1);
              }
              if (as0 == 0)
              {
                a1 = as* as + 1.;
                w->g = (-as*x0* x0 +2.* x0 * y0 + as*y0* y0)/ (2.* a1) * p;
                w->gx = -(as*x0 - y0)/ a1 * p;
                w->gy = (x0 + as*y0)/ a1 * p;
                w->gxx = -as/ a1 * p;
                w->gyy = as/ a1 * p;
                w->gxy = p / a1;
              }
            }
          }
        }
      }

      return 0;
    }

    int GrvLine(grv_val* w, coo3 s0, coo3 c0, coo3 c1,
      double b, double c, char d0, double rho)
    {
      double x1, x2, y1, y2, z1, z2, r1, r2, y0, a, z0;
      char a0;

      x1 = c0.x - s0.x;
      y1 = c0.y - s0.y;
      z1 = c0.z - s0.z;
      r1 = sqrt(x1 * x1 + y1 * y1 + z1 * z1);
      x2 = c1.x - s0.x;
      y2 = c1.y - s0.y;
      z2 = c1.z - s0.z;
      r2 = sqrt(x2 * x2 + y2 * y2 + z2 * z2);
      z0 = z1 - b * x1 - c * y1;
      GetLineParam(&y0, &a, &a0, x1, y1, x2, y2);
      GrvLine0(w, x1, y1, z1, x2, y2, z2, y0, a, a0, z0, b, c, d0, r1, r2);
      GrvUnit(w, rho);
      return 0;
    }

    int GrvLineAppAdd0(grv_val* w, double x1, double y1, double x2, double y2,
      double l1, double l2, double m1, double m2)
    {
      double y0, a, a1, at, p;
      char a0;

      GetLineParam(&y0, &a, &a0, x1, y1, x2, y2);

      if (!a0)
      {
        a1 = a * a + 1.;
        at = atan2(y1 * x2 - y2 * x1, x1 * x2 + y1 * y2);
        p = (l2 - l1 - (m2 - m1) / 2.) / a1;

        w->g = +(a * y2 * y2 - a * x2 * x2 + 2.* x2 * y2) / (2.* a1) * l2
              - (a * y1 * y1 - a * x1 * x1 + 2.* x1 * y1) / (2.* a1) * l1
              - 3./ 4.* (x2 - x1) * (y2 + y1)
              + y0 * y0 / (a1 * 2.) * at;

        w->gx = y0 * p;
        w->gy = a * y0 * p;
        w->gz = 0.;

        w->gxx = -a * p;
        w->gyy = a * p;
        w->gzz = 0.;
        w->gxy = p;
        w->gxz = 0.;
        w->gyz = 0.;
      }
      else
      {
        at = atan2(y1 * x2 - y2 * x1, x1 * x2 + y1 * y2);
        p = (l2 - l1 - (m2 - m1) / 2.);

        w->g = +x1 * x1 / 2.* at;

        w->gx = 0.;
        w->gy = -x1 * p;
        w->gz = 0.;

        w->gxx = 0.;
        w->gyy = 0.;
        w->gzz = 0.;
        w->gxy = 0.;
        w->gxz = 0.;
        w->gyz = 0.;
      }
      return 0;
    }

    int GrvGpPlane0(grv_val* w, coo3 s0, Gp* pl, int i0,
      double z0, double b, double c, char d0, coo3 co[])
    {
      grv_val v;
      double x1, x2, y1, y2, z1, z2, r1, r2, xs, ys, zs, rs, y0, a, rm;
      int i, k, sp, ep;
      char a0, vt;

      NullGrav(w);
      vt = 0;

      sp = pl->s[i0];

      k = pl->e[sp];
      xs = co[k].x - s0.x;
      ys = co[k].y - s0.y;
      zs = co[k].z - s0.z;
      rs = sqrt(xs * xs + ys * ys + zs * zs);
      if (d0 == 0)
      {
        rm = epsi * (fabs(b) + fabs(c)) / 2.;
        z0 = -b * xs - c * ys + zs;
        if (fabs(z0) <= rm) z0 = 0.;
      }

      x2 = xs;
      y2 = ys;
      z2 = zs;
      r2 = rs;
      ep = sp + pl->n[i0];
      for (i = sp + 1; i < ep; i++)
      {
        x1 = x2;
        y1 = y2;
        z1 = z2;
        r1 = r2;

        k = pl->e[i];
        x2 = co[k].x - s0.x;
        y2 = co[k].y - s0.y;
        z2 = co[k].z - s0.z;
        r2 = sqrt(x2 * x2 + y2 * y2 + z2 * z2);

        GetLineParam(&y0, &a, &a0, x1, y1, x2, y2);
        GrvLine0(&v, x1, y1, z1, x2, y2, z2,
              y0, a, a0, z0, b, c, d0, r1, r2);
        AddGrav(w, w, &v);
        vt = (vi | vt);
      }
      GetLineParam(&y0, &a, &a0, x2, y2, xs, ys);
      GrvLine0(&v, x2, y2, z2, xs, ys, zs,
            y0, a, a0, z0, b, c, d0, r2, rs);
      AddGrav(w, w, &v);
      vi = (vi | vt);
      return 0;
    }

    int GrvGpPlane(grv_val* w, coo3 s0, Gp* pl, int i0, coo3 co[], double rho)
    {
      double z0, b, c;
      int k, k1, sp;
      char d0;
      coo3 e[3];

      sp = pl->s[i0];
      for (k = sp; k < sp + 3; k++)
      {
        k1 = pl->e[k];
        e[k - sp] = co[k1];
      }
      while (!GetPlaneParam(&z0, &b, &c, &d0, e[0], e[1], e[2]))
      {
        k++;
        k1 = pl->e[k];
        e[2] = co[k1];
      }
      GrvGpPlane0(w, s0, pl, i0, z0, b, c, d0, co);
      GrvUnit(w, rho);
      return 0;
    }

    int GrvPlane0(grv_val* w, coo3 s0, coo3 co[], int n, double z0, double b, double c,
      char d0)
    {
      grv_val v;
      double x1, x2, y1, y2, z1, z2, r1, r2, xs, ys, zs, rs, y0, a, rm;
      int i;
      char a0, vt;

      NullGrav(w);
      vt = 0;

      xs = co[0].x - s0.x;
      ys = co[0].y - s0.y;
      zs = co[0].z - s0.z;
      rs = sqrt(xs * xs + ys * ys + zs * zs);
      if (d0 == 0)
      {
        rm = epsi * (fabs(b) + fabs(c)) / 2.;
        z0 = -b * xs - c * ys + zs;
        if (fabs(z0) <= rm) z0 = 0.;
      }

      x2 = xs;
      y2 = ys;
      z2 = zs;
      r2 = rs;
      for (i = 1; i < n; i++)
      {
        x1 = x2;
        y1 = y2;
        z1 = z2;
        r1 = r2;

        x2 = co[i].x - s0.x;
        y2 = co[i].y - s0.y;
        z2 = co[i].z - s0.z;
        r2 = sqrt(x2 * x2 + y2 * y2 + z2 * z2);

        GetLineParam(&y0, &a, &a0, x1, y1, x2, y2);
        GrvLine0(&v, x1, y1, z1, x2, y2, z2,
              y0, a, a0, z0, b, c, d0, r1, r2);
        AddGrav(w, w, &v);
        vt = (vi | vt);
      }
      GetLineParam(&y0, &a, &a0, x2, y2, xs, ys);
      GrvLine0(&v, x2, y2, z2, xs, ys, zs,
        y0, a, a0, z0, b, c, d0, r2, rs);
      AddGrav(w, w, &v);
      vi = (vi | vt);
      return 0;
    }

    int GrvPlane(grv_val* w, coo3 s0, coo3 co[], int n, double rho)
    {
      double z0, b, c;
      int i;
      char d0;

      i = 2;
      while (!GetPlaneParam(&z0, &b, &c, &d0, co[0], co[1], co[i]))
      {
        i++;
      }
      GrvPlane0(w, s0, co, n, z0, b, c, d0);
      GrvUnit(w, rho);
      return 0;
    }


    int GrvPlaneApp(grv_val* w, coo3 s0, coo3 m, double df, double rho)
    {
      coo3 d;
      double r, s;
      d = Subcoo3(m, s0);

      s = d.x * d.x + d.y * d.y;
      r = sqrt(s + d.z * d.z);

      w->g = log(d.z + r) * df;
      w->gx = d.x * d.z / (s * r) * df;
      w->gy = d.y * d.z / (s * r) * df;
      w->gz = df / r;
      w->gxx = d.z * df / (s * r) * (-1. + d.x * d.x * (1./ (r * r) + 2./ s));
      w->gyy = d.z * df / (s * r) * (-1. + d.y * d.y * (1./ (r * r) + 2./ s));
      w->gzz = d.z * df / (r * r * r);
      w->gxy = d.x * d.y * d.z * df / (s * r) * (1./ (r * r) + 2./ s);
      w->gxz = d.x * df / (r * r * r);
      w->gyz = d.y * df / (r * r * r);

      GrvUnit(w, rho);

      return 0;
    }


    double log_term(double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double rm, rm1, rm2, p, p1, p2;

      rm = epsi * (r1 + r2);
      rm1 = epsi * r1;
      rm2 = epsi * r2;

      if (fabs(y0) < rm && fabs(z0) < rm)
      {
        if (x1 > rm && x2 > rm)
        {
          p = log(x2 / x1);
        }
        else
        {
          if (x1 < -rm && x2 < -rm)
          {
            p = log(x1 / x2);
          }
          else
          {
            vi = 1;
            p = 0.;
          }
        }
      }
      else
      {
        p2 = x2 + r2;
        if (p2 < rm2) p2 = -(y0 * y0 + z0 * z0) / (x2 * 2.);
        p1 = x1 + r1;
        if (p1 < rm1) p1 = -(y0 * y0 + z0 * z0) / (x1 * 2.);
        p = log(p2 / p1);
      }
      return p;
    }


    double arc_term(double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double rm, q, qn1, qn2, qd1, qd2;

      rm = epsi * (r1 + r2);

      qn2 = z0 * x2;
      qd2 = y0 * r2;
      qn1 = z0 * x1;
      qd1 = y0 * r1;
      if (fabs(y0) < rm)
      {
        q = 0.;
      }
      else
      {
        q = atan2(qn2 * qd1 - qn1 * qd2, qn1 * qn2 + qd1 * qd2);
      }
      return q;
    }


    int GrvSideX(grv_val* w,
      double x1, double x2, double y0, double z0, double r1, double r2)
    {
      double p, q, r;

      p = log_term(x1, x2, y0, z0, r1, r2);
      r = arc_term(x1, x2, z0, y0, r1, r2);

      w->gz = -y0 * p + z0 * r;
      w->gzz = +r;
      if (grA)
      {
        q = arc_term(x1, x2, y0, z0, r1, r2);

        w->g = -z0 * y0 * p + y0 * y0 / 2 * q + z0 * z0 / 2 * r;
        w->gx = 0.;
        w->gy = -z0 * p + y0 * q;
        w->gxx = -q - r;
        w->gyy = +q;
        w->gxy = 0.;
        w->gxz = 0.;
        w->gyz = -p;
      }
      return 0;
    }

    int GrvSideY(grv_val* w,
      double y1, double y2, double x0, double z0, double r1, double r2)
    {
      double p, q;

      p = log_term(y1, y2, x0, z0, r1, r2);

      w->gz = -x0 * p;
      w->gzz = 0.;
      if (grA)
      {
        q = arc_term(y1, y2, x0, -z0, r1, r2);

        w->g = -z0 * x0 * p - x0 * x0 / 2.* q;
        w->gx = -z0 * p - x0 * q;
        w->gy = 0.;
        w->gxx = 0.;
        w->gyy = 0.;
        w->gxy = 0.;
        w->gxz = -p;
        w->gyz = 0.;
      }
      return 0;
    }

    int GrvSideZ(grv_val* w,
      double z1, double z2, double x0, double y0, double r1, double r2)
    {
      double p;

      p = log_term(z1, z2, x0, y0, r1, r2);

      w->g = -x0 * y0 * p;
      w->gx = -y0 * p;
      w->gy = -x0 * p;
      w->gz = 0.;
      w->gxx = 0.;
      w->gyy = 0.;
      w->gzz = 0.;
      w->gxy = -p;
      w->gxz = 0.;
      w->gyz = 0.;

      return 0;
    }

    int GrvQuader(grv_val* w, double x1, double y1, double z1,
      double x2, double y2, double z2, double rho)
    {
      grv_val v;
      double d111, d112, d121, d122, d211, d212, d221, d222;

      d111 = sqrt(x1 * x1 + y1 * y1 + z1 * z1);
      d112 = sqrt(x1 * x1 + y1 * y1 + z2 * z2);
      d121 = sqrt(x1 * x1 + y2 * y2 + z1 * z1);
      d122 = sqrt(x1 * x1 + y2 * y2 + z2 * z2);
      d211 = sqrt(x2 * x2 + y1 * y1 + z1 * z1);
      d212 = sqrt(x2 * x2 + y1 * y1 + z2 * z2);
      d221 = sqrt(x2 * x2 + y2 * y2 + z1 * z1);
      d222 = sqrt(x2 * x2 + y2 * y2 + z2 * z2);

      vi = 0;

      GrvSideX(w, x1, x2, y1, z1, d111, d211);
      GrvSideX(&v, x2, x1, y1, z2, d212, d112); AddGrav(w, w, &v);
      GrvSideX(&v, x2, x1, y2, z1, d221, d121); AddGrav(w, w, &v);
      GrvSideX(&v, x1, x2, y2, z2, d122, d222); AddGrav(w, w, &v);

      GrvSideY(&v, y1, y2, x1, z1, d111, d121); AddGrav(w, w, &v);
      GrvSideY(&v, y2, y1, x1, z2, d122, d112); AddGrav(w, w, &v);
      GrvSideY(&v, y2, y1, x2, z1, d221, d211); AddGrav(w, w, &v);
      GrvSideY(&v, y1, y2, x2, z2, d212, d222); AddGrav(w, w, &v);

      if (grA)
      {
        GrvSideZ(&v, z1, z2, x1, y1, d111, d112); AddGrav(w, w, &v);
        GrvSideZ(&v, z2, z1, x1, y2, d122, d121); AddGrav(w, w, &v);
        GrvSideZ(&v, z2, z1, x2, y1, d212, d211); AddGrav(w, w, &v);
        GrvSideZ(&v, z1, z2, x2, y2, d221, d222); AddGrav(w, w, &v);
      }

      GrvUnit(w, rho);
      return 0;
    }


    int GrvGpVol(grv_val* w, coo3 s0, Gp* vm, int i0, Gp* pl, coo3* co, double rho)
    {
      grv_val v;
      int j, j0, vt, ev;

      NullGrav(w);

      ev = vm->s[i0] + vm->n[i0];
      for (j = vm->s[i0]; j < ev; j++)
      {
        j0 = abs(vm->e[j]);
        if (vm->e[j] > 0)
          vt = 1;
        else
          vt = -1;

        GrvGpPlane(&v, s0, pl, j0, co, vt * rho);
        AddGrav(w, w, &v);
      }
      return 0;
    }


    int GrvPoint(grv_val* w, double x, double y, double z, double m)
    {
      double r, r2, r3, r5;

      r = sqrt(x * x + y * y + z * z);
      if (r > 0.)
      {
        vi = 0;
        r2 = r * r;
        r3 = r * r2;
        r5 = r2 * r3;
        w->gz = z / r3;
        w->gzz = 3.* z * z / r5 - 1./ r3;
        if (grA)
        {
          w->g = 1./ r;
          w->gx = x / r3;
          w->gy = y / r3;
          w->gxx = 3.* x * x / r5 - 1./ r3;
          w->gyy = 3.* y * y / r5 - 1./ r3;
          w->gxy = 3.* x * y / r5;
          w->gxz = 3.* x * z / r5;
          w->gyz = 3.* y * z / r5;
        }
      }
      else
      {
        vi = 1;
        w->gz = 1.e20;
        w->gzz = 1.e20;
        if (grA)
        {
          w->g = 1.e20;
          w->gx = 1.e20;
          w->gy = 1.e20;
          w->gxx = 1.e20;
          w->gyy = 1.e20;
          w->gzz = 1.e20;
          w->gxy = 1.e20;
          w->gxz = 1.e20;
          w->gyz = 1.e20;
        }
      }
      GrvUnit(w, m);
      return 0;
    }

    int Potenz(grv_val* w, coo3 s0, Gp* vm, Gp* pl, coo3* co, double* rho)
    {
      grv_val v;
      int i;

      NullGrav(w);

      for (i = 0; i < vm->lg; i++)
      {
        GrvGpVol(&v, s0, vm, i, pl, co, rho[i]);
        AddGrav(w, w, &v);
      }
      return 0;
    }

    char GeomInput0(TextReader fi, string comm, IList<Point> co, int* mco, Gp* pl, Gp* vm)
    {
      int i, j;

      if ("points3D" == comm)
      {
        while ((j = fscanf(fi, "%i", &i)) == 1)
        {
          if (*mco <= i)
          {
            *mco = i + 1000;
            *co = (coo3*)realloc(*co, *mco * sizeof(coo3));
          }
          fscanf(fi, "%lf %lf %lf", &(*co)[i].x, &(*co)[i].y, &(*co)[i].z);
        }
        return CORNERS_KNOWN;
      }

      if (strcmp("planes", comm) == 0)
      {
        GpInput(fi, pl, 100, 1000);
        return PLANES_KNOWN;
      }

      if (strcmp("volumes", comm) == 0)
      {
        GpInput(fi, vm, 100, 1000);
        return VOLUMES_KNOWN;
      }

      return 0;
    }

    int PotenzInput0(FILE* fi, char* comm, coo3** co, int* mco,
      Gp* pl, Gp* vm, double** rho, int* mrho)
    {
      int i;

      i = GeomInput0(fi, comm, co, mco, pl, vm);
      if (i > 0)
      {
        return i;
      }

      if (strcmp("densities", comm) == 0)
      {
        while (fscanf(fi, "%i", &i) == 1)
        {
          if (i >= *mrho)
          {
            *mrho = i + 1 + (i + 1) / 8;
            *rho = (double*)realloc(*rho, *mrho * sizeof(double));
          }
          fscanf(fi, "%lf", &((*rho)[i]));
        }
        return DENSITIES_KNOWN;
      }

      return 0;
    }

    int PotenzInput(FILE* fi, coo3** co, int* mco, Gp* pl, Gp* vm,
      double** rho, int* mrho)
    {
      char comm[78];
      int status;

      status = 0;
      while (fscanf(fi, "%s", comm) >= 0)
      {
        status = status | PotenzInput0(fi, comm, co, mco, pl, vm, rho, mrho);
      }

    }

    void InitGeom(int xco, coo3** co, Gp* pl, Gp* vm)
    {
      *co = (coo3*)malloc(xco * sizeof(coo3));
      GpAllocClear(vm, 6 * xco, 24 * xco);
      GpAllocClear(pl, 12 * xco, 36 * xco);
    }

    void InitPotenz(int nco, coo3** co, int* mco, Gp* pl, Gp* vm,
      double** rho, int* mrho)
    {
      InitGeom(nco, co, pl, vm);
      *mco = nco;
      *rho = (double*)malloc(vm->mg * sizeof(double));
      *mrho = vm->mg;
    }

  }
}
