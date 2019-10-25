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
      public enum ExtentType
      {
        FullyHidden = -3,
        CellTooSmall = -2,
        NotVisible = -1,
        PointVisible = 0,
        NotFlatEnough = 1,
        FlatEnough = 2
      }

      private const double _epsi = 1.0e-8;

      private readonly IPoint _center;

      private readonly double _fx;
      private readonly double _fy;
      private readonly ViewSystem _vs;

      public ViewParam(IPoint center, double fx, double fy, ViewSystem vs)
      {
        _center = center;

        _fx = fx;
        _fy = fy;
        _vs = vs;
      }

      public IPoint Center => _center;
      public Point3D ProjectGridPoint(Point p)
      {
        Point p0 = new Point3D(_fx * (p.X - _center.X), _fy * (p.Y - _center.Y), p.Z - _center.Z);
        return _vs.ProjectLocalPoint(p0);
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
          Z = -_vs.DetT / det,
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

        Point3D px0 = new Point3D(x0 * _vs.Det0, x0 * _vs.DetU0, x0 * _vs.DetV0);
        Point3D py0 = new Point3D(y0 * _vs.Det1, y0 * _vs.DetU1, y0 * _vs.DetV1);
        Point3D pz0 = new Point3D(z0 * _vs.Det2, z0 * _vs.DetU2, z0 * _vs.DetV2);

        Point3D px1 = new Point3D(x1 * _vs.Det0, x1 * _vs.DetU0, x1 * _vs.DetV0);
        Point3D py1 = new Point3D(y1 * _vs.Det1, y1 * _vs.DetU1, y1 * _vs.DetV1);
        Point3D pz1 = new Point3D(z1 * _vs.Det2, z1 * _vs.DetU2, z1 * _vs.DetV2);

        ps[0] = GetPoint(px0, py0, pz0);
        ps[4] = GetPoint(px1, py0, pz0);
        ps[2] = GetPoint(px0, py1, pz0);
        ps[6] = GetPoint(px1, py1, pz0);
        ps[1] = GetPoint(px0, py0, pz1);
        ps[3] = GetPoint(px1, py0, pz1);
        ps[5] = GetPoint(px0, py1, pz1);
        ps[7] = GetPoint(px1, py1, pz1);
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
      public ExtentType BlockExtent(Scene scene,
        int i0, int j0, double h0, int i1, int j1, double h1, double dh,
        ref int um, ref int vm, ref double tm)
      {
        Point3D[] p = new Point3D[12];

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

    public void Draw(Point3D center, ViewSystem vs, Scene scene, bool flushScene = true)
    {
      GridExtent e = _pyr.Grid.Extent;
      double ix = (center.X - e.X0) / e.Dx;
      double iy = (center.Y - e.Y0) / e.Dy;
      Point3D c = new Point3D(ix, iy, center.Z);
      ViewParam vp = new ViewParam(c, e.Dx, e.Dy, vs);

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
        scene.DrawPoint(u, v, i0 + n2, j0 + n2, ArgbFct);
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
        scene.DrawPoint(iu0, iv0, i0, j0, ArgbFct);
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
        scene.DrawTri(xa0, ya0, pa0, ArgbFct);
        scene.DrawTri(xa1, ya1, pa1, ArgbFct);
      }
      else
      {
        scene.DrawTri(xa1, ya1, pa1, ArgbFct);
        scene.DrawTri(xa0, ya0, pa0, ArgbFct);
      }

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
        p = vp.ProjectGridPoint(new Point3D(i0, j0, h));
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
