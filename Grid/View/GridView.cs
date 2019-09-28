using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid.View
{
  public class GridView
  {
    private class InfParam
    {
      public double F0 { get; set; }
      public double F1 { get; set; }
      public double Fd { get; set; }
      public double UInf { get; set; }
      public double VInf { get; set; }
      public bool Is0 { get; set; }

      public double Du { get; set; }
      public double Dv { get; set; }
    }

    private class ViewParam
    {
      private const double _epsi = 1.0e-8;

      private readonly IPoint _center;
      private readonly IPoint _plane;
      private readonly IPoint _uDir;
      private readonly IPoint _vDir;

      private readonly double _fx;
      private readonly double _fy;

      private double _det0;
      private double _det1;
      private double _det2;

      private double _detT;
      private double _detU0;
      private double _detU1;
      private double _detU2;
      private double _detV0;
      private double _detV1;
      private double _detV2;

      private InfParam _parX;
      private InfParam _parY;
      private InfParam _parZ;

      public ViewParam(IPoint center, double fx, double fy, IPoint plane, IPoint uDir, IPoint vDir)
      {
        _center = center;
        _plane = plane;
        _uDir = uDir;
        _vDir = vDir;

        _fx = fx;
        _fy = fy;

        CalcParams();
      }

      public IPoint Center => _center;

      private void CalcParams()
      {
        /* calculating parameters */
        Point b = PointOp.Sub(_center, _plane);

        _det0 = _uDir.Y * _vDir.Z - _uDir.Z * _vDir.Y;
        _det1 = _uDir.Z * _vDir.X - _uDir.X * _vDir.Z;
        _det2 = _uDir.X * _vDir.Y - _uDir.Y * _vDir.X;

        _detT = b.X * _det0 + b.Y * _det1 + b.Z * _det2;

        _detU0 = b.Y * _vDir.Z - b.Z * _vDir.Y;
        _detU1 = b.Z * _vDir.X - b.X * _vDir.Z;
        _detU2 = b.X * _vDir.Y - b.Y * _vDir.X;

        _detV0 = _uDir.Y * b.Z - _uDir.Z * b.Y;
        _detV1 = _uDir.Z * b.X - _uDir.X * b.Z;
        _detV2 = _uDir.X * b.Y - _uDir.Y * b.X;

        /* infinit plane
          m_Det0 * m_xc + m_Det1 * m_yc + m_Det2 * m_zc + d = 0 */
        double d = -(_det0 * _center.X + _det1 * _center.Y + _det2 * _center.Z);

        _parX = GetInfParams(_det0, _det1, _det2, d, new Point3D(1, 0, 0));
        _parY = GetInfParams(_det1, _det2, _det0, d, new Point3D(0, 1, 0));
        _parZ = GetInfParams(_det2, _det0, _det1, d, new Point3D(0, 0, 1));
      }
      private InfParam GetInfParams(double a, double b, double c, double d, Point3D p)
      {
        InfParam inf;
        if (Math.Abs(a) > _epsi)
        {
          inf = new InfParam();
          inf.F0 = -b / a;
          inf.F1 = -c / a;
          inf.Fd = -d / a;

          Point3D t = ProjectPoint(_center + p);
          inf.UInf = t.X;
          inf.VInf = t.Y;
          inf.Is0 = false;
        }
        else
        {
          Point3D t = ProjectPoint(_plane + p);
          inf = new InfParam();
          inf.Du = t.X;
          inf.Dv = t.Y;
          inf.Is0 = true;
        }
        return inf;
      }
      public Point3D ProjectPoint(Point p)
      {
        Point p0 = new Point3D(_fx * (p.X - _center.X), _fy * (p.Y - _center.Y), p.Z - _center.Z);
        //Point p0 = new Point3D(p.X - _center.X, -(p.Y - _center.Y), p.Z - _center.Z);
        double det = p0.X * _det0 + p0.Y * _det1 + p0.Z * _det2;
        if (Math.Abs(det) < 1.0e-8)
        {
          return null;
        }

        Point3D p1 = new Point3D
        {
          Z = -_detT / det,
          X = (p0.X * _detU0 + p0.Y * _detU1 + p0.Z * _detU2) / det,
          Y = (p0.X * _detV0 + p0.Y * _detV1 + p0.Z * _detV2) / det
        };
        return p1;
      }

      private Point3D GetPoint(Point3D px, Point3D py, Point3D pz)
      {
        double det = px[0] + py[0] + pz[0];
        if (Math.Abs(det) < 1.0e-8)
        {
          return null;
        }

        Point3D p1 = new Point3D
        {
          Z = -_detT / det,
          X = (px[1] + py[1] + pz[1]) / det,
          Y = (px[2] + py[2] + pz[2]) / det
        };
        return p1;

      }
      private void ProjectBox(Point[] ps, int i0, int j0, double h0, int i1, int j1, double h1)
      {
        double x0 = _fx * (i0 - _center.X);
        double y0 = _fy * (j0 - _center.Y);
        double z0 = h0 - _center.Z;

        double x1 = _fx * (i1 - _center.X);
        double y1 = _fy * (j1 - _center.Y);
        double z1 = h1 - _center.Z;

        Point3D px0 = new Point3D(x0 * _det0, x0 * _detU0, x0 * _detV0);
        Point3D py0 = new Point3D(y0 * _det1, y0 * _detU1, y0 * _detV1);
        Point3D pz0 = new Point3D(z0 * _det2, z0 * _detU2, z0 * _detV2);

        Point3D px1 = new Point3D(x1 * _det0, x1 * _detU0, x1 * _detV0);
        Point3D py1 = new Point3D(y1 * _det1, y1 * _detU1, y1 * _detV1);
        Point3D pz1 = new Point3D(z1 * _det2, z1 * _detU2, z1 * _detV2);

        ps[0] = GetPoint(px0, py0, pz0);
        ps[4] = GetPoint(px1, py0, pz0);
        ps[2] = GetPoint(px0, py1, pz0);
        ps[6] = GetPoint(px1, py1, pz0);
        ps[1] = GetPoint(px0, py0, pz1);
        ps[3] = GetPoint(px1, py0, pz1);
        ps[5] = GetPoint(px0, py1, pz1);
        ps[7] = GetPoint(px1, py1, pz1);
      }


      public Point3D ProjectPointX(double x, double y, double z, double x1, Point3D p0)
      {
        return ProjectPointDir(x, y, z, x1, _parX, p0);
      }

      public Point3D ProjectPointY(double x, double y, double z, double y1, Point3D p0)
      {
        return ProjectPointDir(y, z, x, y1, _parY, p0);
      }
      public Point3D ProjectPointZ(double x, double y, double z, double z1, Point3D p0)
      {
        return ProjectPointDir(z, x, y, z1, _parZ, p0);
      }
      private Point3D ProjectPointDir(double x, double y, double z, double x1, InfParam inf, Point3D p0)
      {
        Point3D p1;
        if (inf.Is0)
        {
          double b = (x1 - x) * p0.Z;
          p1 = new Point3D
          {
            Z = p0.Z, // t1 always > 0
            X = p0.X + b * inf.Du,
            Y = p0.Y + b * inf.Dv
          };
        }
        else
        {
          double b = -(inf.F0 * y + inf.F1 * z + inf.Fd);

          double at = p0.Z * (b + x);
          double au = (p0.X - inf.UInf) * (b + x);
          double av = (p0.Y - inf.VInf) * (b + x);

          p1 = new Point3D
          {
            Z = at / (b + x1),
            X = au / (b + x1) + inf.UInf,
            Y = av / (b + x1) + inf.VInf
          };
        }
        return p1;
      }

      private void MinMax(IEnumerable<Point3D> pts, out Point3D pMin, out Point3D pMax, int count = 0)
      {
        pMin = null;
        pMax = null;
        int n = 0;
        foreach (var p in pts)
        {
          n++;
          if (pMin == null)
          {
            pMin = new Point3D(p.X, p.Y, p.Z);
            pMax = new Point3D(p.X, p.Y, p.Z);
          }
          else
          {
            pMin.X = Math.Min(p.X, pMin.X);
            pMin.Y = Math.Min(p.Y, pMin.Y);
            pMin.Z = Math.Min(p.Z, pMin.Z);

            pMax.X = Math.Max(p.X, pMax.X);
            pMax.Y = Math.Max(p.Y, pMax.Y);
            pMax.Z = Math.Max(p.Z, pMax.Z);
          }
          if (n == count)
          { break; }
        }
      }
      public enum ExtentType
      {
        FullyHidden = -3,
        CellTooSmall = -2,
        NotVisible = -1,
        PointVisible = 0,
        NotFlatEnough = 1,
        FlatEnough = 2
      }
      public ExtentType BlockExtent(Scene scene,
        int i0, int j0, double h0, int i1, int j1, double h1, double dh,
        ref int um, ref int vm, ref double tm)
      {
        Point3D[] p = new Point3D[12];

        //p[0] = ProjectPoint(new Point3D(i0, j0, h0));
        ////p[4] = ProjectPointX(i0, j0, h0, i1, p[0]);
        ////p[2] = ProjectPointY(i0, j0, h0, j1, p[0]);
        ////p[6] = ProjectPointY(i1, j0, h0, j1, p[4]);
        ////p[1] = ProjectPointZ(i0, j0, h0, h1, p[0]);
        ////p[3] = ProjectPointZ(i0, j1, h0, h1, p[2]);
        ////p[5] = ProjectPointZ(i1, j0, h0, h1, p[4]);
        ////p[7] = ProjectPointZ(i1, j1, h0, h1, p[6]);
        //p[4] = ProjectPoint(new Point3D(i1, j0, h0));
        //p[2] = ProjectPoint(new Point3D(i0, j1, h0));
        //p[6] = ProjectPoint(new Point3D(i1, j1, h0));
        //p[1] = ProjectPoint(new Point3D(i0, j0, h1));
        //p[3] = ProjectPoint(new Point3D(i1, j0, h1));
        //p[5] = ProjectPoint(new Point3D(i0, j1, h1));
        //p[7] = ProjectPoint(new Point3D(i1, j1, h1));

        //Point3D[] t = new Point3D[8];
        ProjectBox(p, i0, j0, h0, i1, j1, h1);

        MinMax(p, out Point3D pMin, out Point3D pMax, 8);
        if (pMax.Z < 0)
        {
          return ExtentType.NotVisible;
        }
        if (pMin.Z < 0)
        {
          /* handle infinity points */
          /// TO DO : handle correctly !!!
          return ExtentType.NotFlatEnough;

          /*    k = np;
              uj = 0.;
              vj = 0.;
              for (i = 0; i < np; i++) {
                j = i + 1;
                if (j == np) {
                  j = 0;
                }
                if ((t[i] < 0) != (t[j] < 0)) { // check for line going to infinity
                  if (t[i] < 0) {
                    ui = u[i];
                    vi = v[i];
                    uj = u[j];
                    vj = v[j];
                  } else {
                    ui = u[j];  // ui,vi: 'infinit' values
                    vi = v[j];
                    uj = u[i];  // uj,vj:  real values
                    vj = v[i];
                  }

                  if (ui > uj) { // set new point on 'infinity' side of drawing rectangel
                    u[k] = -1;
                  } else if (ui < uj) {
                    u[k] = m_scene->getNx() + 1;
                  } else {
                    u[k] = ui;
                  }

                  if (vi > vj) {
                    v[k] = -1;
                  } else if (vi < vj) {
                    v[k] = m_scene->getNy() + 1;
                  } else {
                    v[k] = vi;
                  }
                  k ++;
                  if (k >= 12) {
                    printf("...%i\n",k);
                  }
                }
              }
              for (i = 0; i < np; i++) { // make sure that infinit values do not disturb
                if (t[i] < 0) {
                  u[i] = uj;  // uj,vj are real values, see above
                  v[i] = vj;
                }
              }
              np = k; */
        }

        if (pMax.X < 0 || pMin.X > scene.Nx ||
            pMax.Y < 0 || pMin.Y > scene.Ny)
        {
          return ExtentType.NotVisible;
        }

        /* remark : do not convert uMin to int, numerical reason (uMin < -MAX_INT)
           can lead to wrong results */
        int u0 = (int)Math.Max(0.5, pMin.X + 0.5);
        int u1 = (int)Math.Min(scene.Nx - 0.5, pMax.X + 0.5);
        int v0 = (int)Math.Max(0.5, pMin.Y + 0.5);
        int v1 = (int)Math.Min(scene.Ny - 0.5, pMax.Y + 0.5);

        int iu = u0;
        while (iu <= u1)
        {
          int iv = v0;
          while (iv <= v1)
          {
            if (scene.PointIsVisible(pMax.Z, iu, iv) == true)
            {
              // single point view ?
              if (u0 == u1 && v0 == v1 &&
                  scene.PointIsVisible(pMax.Z, u1, v1) == true)
              {
                um = u0;
                vm = v0;
                tm = pMax.Z;
                return ExtentType.PointVisible;
              }
              // cell large enough ?
              if (pMax.X - pMin.X < 0.5 && pMax.Y - pMin.Y < 0.5)
              {
                return ExtentType.CellTooSmall;
              }
              if (double.IsNaN(dh))
              {
                return ExtentType.NotFlatEnough;
              }
              // triangles flat enough ?
              if (dh == 0)
              {
                return ExtentType.FlatEnough;
              }
              double d = (h1 - h0) / dh;
              for (int i = 0; i < 8; i += 2)
              {
                if (Math.Abs(p[i + 1].X - p[i].X) > d)
                {
                  return ExtentType.NotFlatEnough;
                }
                if (Math.Abs(p[i + 1].Y - p[i].Y) > d)
                {
                  return ExtentType.NotFlatEnough;
                }
              }
              return ExtentType.FlatEnough; // flat enough !
            }
            iv++;
          }
          iu++;
        }
        return ExtentType.FullyHidden; // all points hidden by other points
      }
    }

    private readonly Pyramide _pyr;
    private Func<double, double, int> _argbFct;
    public Func<double, double, int> ArgbFct { get => _argbFct; set => _argbFct = value; }

    private readonly int _cacheAnd;
    private readonly Point3D[,] _cachePrjPnt;
    private readonly int[,] _cacheCurrentI;
    private readonly int[,] _cacheCurrentJ;

    private int _resol;
    public GridView(Pyramide pyr, Func<double, double, int> argbFct)
    {
      _pyr = pyr;
      _argbFct = argbFct;

      int cacheDim = 6;
      int cacheLen = 1 << cacheDim;
      _cacheAnd = cacheLen - 1;
      _cachePrjPnt = new Point3D[cacheLen, cacheLen];
      _cacheCurrentI = new int[cacheLen, cacheLen];
      _cacheCurrentJ = new int[cacheLen, cacheLen];

      _resol = 2;
    }

    public void Draw(Point3D center, ViewSystem vs, double focus, Scene scene, bool flushScene = true)
    {
      double l = ViewSystem.PIX_M / focus;
      Point lp = PointOp.Scale(1 / l, vs.P);

      GridExtent e = _pyr.Grid.Extent;
      double ix = (center.X - e.X0) / e.Dx;
      double iy = (center.Y - e.Y0) / e.Dy;
      Point3D c = new Point3D(ix, iy, center.Z);

      int footx = scene.Nx / 2;
      int footy = scene.Ny / 2;
      Point plane = c + lp - PointOp.Scale(footx, vs.U) - PointOp.Scale(footy, vs.V);
      ViewParam vp = new ViewParam(c, e.Dx, e.Dy, plane, vs.U, vs.V);

      DrawBlock(scene, vp, _pyr.ParentBlock, 0, 0, _pyr.NMax);
      if (flushScene)
      {
        scene.Flush();
      }
    }
    public int Resol
    {
      get { return _resol; }
      set
      {
        _resol = value < 2 ? 2 : value;
      }
    }

    private int DrawBlock(Scene scene, ViewParam vp, Pyramide.Block parentBlock,
      int i0, int j0, int n)
    {
      if (scene.Escape)
      { return -1; }

      if (parentBlock == null)
      { return 0; }

      if (!parentBlock.HasChildren() && n > 2)
      { return 0; }

      if (parentBlock.HMax == 0)
      { return 0; }

      int u = 0; int v = 0; double t = 0;
      ViewParam.ExtentType s = vp.BlockExtent(scene, i0, j0, parentBlock.HMin,
        i0 + n, j0 + n, parentBlock.HMax, parentBlock.Dh, ref u, ref v, ref t);
      if (s < 0)
      {
        return 0;
      }

      if (s == ViewParam.ExtentType.FlatEnough)
      {
        DrawCell(scene, vp, i0, j0, n);
        return 0;
      }

      int n2 = n / 2;
      if (s == ViewParam.ExtentType.PointVisible)
      {
        scene.SetNew(t, u, v);
        DrawPoint(scene, u, v, i0 + n2, j0 + n2);
        return 0;
      }

      if (n <= _resol)
      { // single Cell view
        return DrawFourCells(scene, vp, i0, j0, n2);
      }

      /* children */

      if (vp.Center.X < i0 + n2)
      {
        if (vp.Center.Y < j0 + n2)
        {
          DrawBlock(scene, vp, parentBlock.ChildNW, i0, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildSW, i0, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildNE, i0 + n2, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildSE, i0 + n2, j0 + n2, n2);
        }
        else
        {
          DrawBlock(scene, vp, parentBlock.ChildSW, i0, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildNW, i0, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildSE, i0 + n2, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildNE, i0 + n2, j0, n2);
        }
      }
      else
      {
        if (vp.Center.Y < j0 + n2)
        {
          DrawBlock(scene, vp, parentBlock.ChildNE, i0 + n2, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildSE, i0 + n2, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildNW, i0, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildSW, i0, j0 + n2, n2);
        }
        else
        {
          DrawBlock(scene, vp, parentBlock.ChildSE, i0 + n2, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildSW, i0, j0 + n2, n2);
          DrawBlock(scene, vp, parentBlock.ChildNE, i0 + n2, j0, n2);
          DrawBlock(scene, vp, parentBlock.ChildNW, i0, j0, n2);
        }
      }

      if (scene.Escape == true)
      {
        return -1;
      }
      return 0;
    }

    private int DrawFourCells(Scene scene, ViewParam vp, int i0, int j0, int n2)
    {
      if (vp.Center.X < i0 + n2)
      {
        if (vp.Center.Y < j0 + n2)
        {
          DrawCell(scene, vp, i0, j0, n2);
          DrawCell(scene, vp, i0, j0 + n2, n2);
          DrawCell(scene, vp, i0 + n2, j0, n2);
          DrawCell(scene, vp, i0 + n2, j0 + n2, n2);
        }
        else
        {
          DrawCell(scene, vp, i0, j0 + n2, n2);
          DrawCell(scene, vp, i0, j0, n2);
          DrawCell(scene, vp, i0 + n2, j0 + n2, n2);
          DrawCell(scene, vp, i0 + n2, j0, n2);
        }
      }
      else
      {
        if (vp.Center.Y < j0 + n2)
        {
          DrawCell(scene, vp, i0 + n2, j0, n2);
          DrawCell(scene, vp, i0 + n2, j0 + n2, n2);
          DrawCell(scene, vp, i0, j0, n2);
          DrawCell(scene, vp, i0, j0 + n2, n2);
        }
        else
        {
          DrawCell(scene, vp, i0 + n2, j0 + n2, n2);
          DrawCell(scene, vp, i0, j0 + n2, n2);
          DrawCell(scene, vp, i0 + n2, j0, n2);
          DrawCell(scene, vp, i0, j0, n2);
        }
      }
      return 0;
    }

    private int DrawCell(Scene scene, ViewParam vp, int i0, int j0, int n)
    {
      int i1 = i0 + n;
      int j1 = j0 + n;

      if (CachePos(vp, i0, j0, out Point3D p00) < 0) return 0;
      if (CachePos(vp, i0, j1, out Point3D p01) < 0) return 0;
      if (CachePos(vp, i1, j0, out Point3D p10) < 0) return 0;
      if (CachePos(vp, i1, j1, out Point3D p11) < 0) return 0;

      Point3D p0 = new Point3D();
      Point3D p1 = new Point3D();
      p0.X = Math.Min(Math.Min(Math.Min(p00.X, p01.X), p10.X), p11.X);
      if (p0.X >= scene.Nx) return 0;
      p1.X = Math.Max(Math.Max(Math.Max(p00.X, p01.X), p10.X), p11.X);
      if (p1.X < 0) return 0;
      p0.Y = Math.Min(Math.Min(Math.Min(p00.Y, p01.Y), p10.Y), p11.Y);
      if (p0.Y >= scene.Ny) return 0;
      p1.Y = Math.Max(Math.Max(Math.Max(p00.Y, p01.Y), p10.Y), p11.Y);
      if (p1.Y < 0) return 0;

      p1.Z = Math.Max(Math.Max(Math.Max(p00.Z, p01.Z), p10.Z), p11.Z);

      int iu0 = (int)Math.Max(0.5, p0.X + 0.5);
      int iv0 = (int)Math.Max(0.5, p0.Y + 0.5);
      int iu1 = (int)Math.Min(scene.Nx - 0.5, p1.X + 0.5);
      int iv1 = (int)Math.Min(scene.Ny - 0.5, p1.Y + 0.5);

      if (!IsAnyVisible(scene, p1.Z, iu0, iu1, iv0, iv1))
      {
        return 0;
      }
      /* drawnig*/

      if (iu0 == iu1 && iv0 == iv1 && scene.TrySetNew(p1.Z, iu0, iv0))
      {
        // drawing single pixel
        DrawPoint(scene, iu0, iv0, i0, j0);
        return 0;
      }

      //flat draw : check if center of diagonals are close enough,
      //  otherwise divide further
      if (n >= _resol && (Math.Abs(p00.X + p11.X - p01.X - p10.X) > 2 ||
                          Math.Abs(p00.Y + p11.Y - p01.Y - p10.Y) > 2))
      {
        return DrawFourCells(scene, vp, i0, j0, n / 2);
      }

      double[] xa0 = { i0, i1, i0 };
      double[] xa1 = { i1, i0, i1 };
      double[] ya0 = { j0, j0, j1 };
      double[] ya1 = { j1, j1, j0 };
      Point3D[] pa0 = { p00, p10, p01 };
      Point3D[] pa1 = { p11, p01, p10 };

      if (vp.Center.X < i0)
      {
        DrawTri(scene, xa0, ya0, pa0);
        DrawTri(scene, xa1, ya1, pa1);
      }
      else
      {
        DrawTri(scene, xa1, ya1, pa1);
        DrawTri(scene, xa0, ya0, pa0);
      }

      return 0;
    }

    private void MinMax(IEnumerable<double> vals, ref double min, ref double max, int count = 0)
    {
      int n = 0;
      foreach (var val in vals)
      {
        min = Math.Min(val, min);
        max = Math.Max(val, max);
        n++;
        if (n == count)
        { break; }
      }
    }
    private int DrawTri(Scene scene, double[] x, double[] y, Point3D[] q)
    {
      double u0 = q[0].X;
      double ur = q[1].X - u0;
      double us = q[2].X - u0;
      double v0 = q[0].Y;
      double vr = q[1].Y - v0;
      double vs = q[2].Y - v0;

      double vmax = Math.Max(Math.Max(q[0].Y, q[1].Y), q[2].Y);
      double vmin = Math.Min(Math.Min(q[0].Y, q[1].Y), q[2].Y);
      vmin = Math.Max(0, vmin - 1);
      double umax = Math.Max(Math.Max(q[0].X, q[1].X), q[2].X);
      double umin = Math.Min(Math.Min(q[0].X, q[1].X), q[2].X);

      double det = ur * vs - us * vr;
      double xr = x[1] - x[0];
      double xs = x[2] - x[0];
      double dxdu = (xr * vs - xs * vr) / det;
      double dxdv = (ur * xs - us * xr) / det;
      double xc = x[0] - dxdu * u0 - dxdv * v0;

      double yr = y[1] - y[0];
      double ys = y[2] - y[0];
      double dydu = (yr * vs - ys * vr) / det;
      double dydv = (ur * ys - us * yr) / det;
      double yc = y[0] - dydu * u0 - dydv * v0;

      double tr = q[1].Z - q[0].Z;
      double ts = q[2].Z - q[0].Z;
      double dtdu = (tr * vs - ts * vr) / det;
      double dtdv = (ur * ts - us * tr) / det;
      double tc = q[0].Z - dtdu * u0 - dtdv * v0;

      int tv = Math.Min(1 + (int)vmax, scene.Ny - 1);
      double dus, us0;
      if (Math.Abs(vs) >= 1.0e-6)
      {
        dus = -us / vs;
        us0 = dus * (v0 - tv) + u0;
      }
      else
      {
        us0 = umin - 1;
        dus = 0.0;
      }
      double dur, ur0;
      if (Math.Abs(vr) >= 1.0e-6)
      {
        dur = -ur / vr;
        ur0 = dur * (v0 - tv) + u0;
      }
      else
      {
        ur0 = umin - 1;
        dur = 0.0;
      }

      double durs, urs0;
      if (Math.Abs(vs - vr) >= 1.0e-6)
      {
        durs = (ur - us) / (vs - vr);
        urs0 = durs * (v0 + vs - tv) + us + u0;
      }
      else
      {
        urs0 = umin - 1;
        durs = 0.0;
      }

      while (tv >= vmin)
      {
        double[] u_ = new double[3];
        int i = 0;
        if ((q[0].Y <= tv) == (q[1].Y >= tv))
        {
          u_[i] = ur0;
          i++;
        }
        if ((q[0].Y <= tv) == (q[2].Y >= tv))
        {
          u_[i] = us0;
          i++;
        }
        if ((q[1].Y <= tv) == (q[2].Y >= tv))
        {
          u_[i] = urs0;
          i++;
        }

        if (i >= 2)
        {
          int i_ = i - 1;
          double tumin = u_[i_];
          double tumax = tumin;
          MinMax(u_, ref tumin, ref tumax, i_);
          int tu = Math.Min(scene.Nx - 1, (int)tumax + 1);
          tumin = Math.Max(0, tumin - 1);

          double tx = dxdu * tu + dxdv * tv + xc;
          double ty = dydu * tu + dydv * tv + yc;
          double tt = dtdu * tu + dtdv * tv + tc;
          while (tu >= tumin)
          {
            double h = scene.Fields[tu, tv];
            if (h == 0 ||
                (tu <= tumax && tu >= tumin && tt > h))
            {
              if (DrawPoint(scene, tu, tv, tx, ty) != 0)
              {
                scene.Fields[tu, tv] = tt; //m_scene->trySetNew(t[0],tu,tv) == true) {
              }
            }
            if (scene.Line[tu] == false)
            {
              if (tv + 1 < scene.Ny &&
                  scene.Fields[tu, tv + 1] == 0.0)
              {
                if (DrawPoint(scene, tu, tv + 1, tx + dxdv, ty + dydv) != 0)
                {
                  scene.Fields[tu, tv + 1] = tt;
                }
              }
              scene.Line[tu] = true;
            }
            tu--;
            tx -= dxdu;
            ty -= dydu;
            tt -= dtdu;
          }
        }
        tv--;
        us0 += dus;
        ur0 += dur;
        urs0 += durs;
      }
      scene.ResetLine((int)umin, (int)umax);
      return 0;
    }

    private bool IsAnyVisible(Scene scene, double z, int u0, int u1, int v0, int v1)
    {
      int iu = u0;
      while (iu <= u1)
      {
        int iv = v0;
        while (iv <= v1)
        {
          if (scene.PointIsVisible(z, iu, iv) == true)
          {
            return true;
          }
          iv++;
        }
        iu++;
      }
      return false;
    }

    int DrawPoint(Scene scene, int u, int v, double i, double j)
    {
      if (u < 0)
      { return 1; }

      int argb = ArgbFct(i, j);

      scene.DrawPoint(u, v, argb);

      return 1;
    }

    private int CachePos(ViewParam vp, int i0, int j0, out Point3D p)
    {
      int i = i0 & _cacheAnd;
      int j = j0 & _cacheAnd;
      if (_cacheCurrentI[i, j] == i0 &&
          _cacheCurrentJ[i, j] == j0)
      { /* get from Cache */
        p = _cachePrjPnt[i, j];
      }
      else
      { /* calculate new pos */
        double h = _pyr.Grid[i0, j0];
        if (double.IsNaN(h) || h <= 0)
        {
          p = null;
          return -1;
        }
        p = vp.ProjectPoint(new Point3D(i0, j0, h));
        if (p == null)
        {
          return -3;
        }

        _cachePrjPnt[i, j] = p;
        _cacheCurrentI[i, j] = i0;
        _cacheCurrentJ[i, j] = j0;
      }
      if (p.Z < 0)
      {
        return -2;
      }
      return 0;
    }
  }
}
