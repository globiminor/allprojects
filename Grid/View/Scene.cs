using System;
using System.Collections.Generic;
using System.Drawing;

namespace Grid.View
{
  public class Scene
  {
    private Bitmap _bitmap;
    private double[,] _fields;
    private int[,] _argbs;
    private bool[] _line;
    public int Nx { get; private set; }
    public int Ny { get; private set; }
    public bool Escape { get; set; }

    public Bitmap Bitmap => _bitmap;
    public void Resize(int nx, int ny)
    {
      _fields = new double[nx, ny];
      _line = new bool[nx];
      Nx = nx;
      Ny = ny;

      _bitmap?.Dispose();
      _bitmap = new Bitmap(nx, ny, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      _argbs = new int[nx, ny];
    }
    public void Fill(int argb)
    {
      for (int i = 0; i < Nx; i++)
      {
        for (int j = 0; j < Ny; j++)
        {
          _argbs[i, j] = argb;
        }
      }
    }
    public void Flush()
    {
      ImageGrid.GridToImage(_bitmap, (x, y) => _argbs[x, y]);
    }

    public double[,] Fields => _fields;
    private bool[] Line => _line;
    public void DrawPoint(int x, int y, int color)
    {
      _argbs[x, y] = color;
    }
    public void SetNew(double t, int u, int v)
    {
      _fields[u, v] = t;
    }
    public bool TrySetNew(double t, int u, int v)
    {
      if (t > _fields[u, v])
      {
        _fields[u, v] = t;
        return true;
      }
      else
      {
        return false;
      }
    }

    private int ResetLine(int xMin, int xMax)
    {
      xMin = Math.Max(0, xMin);
      xMax = Math.Min(xMax, Nx - 1);
      for (int i = xMin; i <= xMax; i++)
      {
        _line[i] = false;
      }
      return 0;
    }

    public bool PointIsVisible(double t, int u, int v)
    {
      if (t > _fields[u, v])
      {
        return true;
      }
      return false;
    }

    public void DrawTri(ViewSystem vs, Basics.Geom.IPoint p00, Basics.Geom.IPoint p10, Basics.Geom.IPoint p11, 
      Func<double, double, int> argbFct)
    {
      Basics.Geom.IPoint q00 = vs.ProjectLocalPoint(p00);
      if (q00.Z < 0)
        return;
      Basics.Geom.IPoint q10 = vs.ProjectLocalPoint(p10);
      if (q10.Z < 0)
        return;
      Basics.Geom.IPoint q11 = vs.ProjectLocalPoint(p11);
      if (q11.Z < 0)
        return;

      DrawTri(new[] { 0.0, 1.0, 1.0 }, new[] { 0.0, 0.0, 1.0 }, new[] { q00, q10, q11 }, argbFct);
    }
    public int DrawTri(double[] x, double[] y, Basics.Geom.IPoint[] qs, Func<double, double, int> argbFct)
    {
      double u0 = qs[0].X;
      double ur = qs[1].X - u0;
      double us = qs[2].X - u0;
      double v0 = qs[0].Y;
      double vr = qs[1].Y - v0;
      double vs = qs[2].Y - v0;

      double vmax = Math.Max(Math.Max(qs[0].Y, qs[1].Y), qs[2].Y);
      double vmin = Math.Min(Math.Min(qs[0].Y, qs[1].Y), qs[2].Y);
      vmin = Math.Max(0, vmin - 1);
      double umax = Math.Max(Math.Max(qs[0].X, qs[1].X), qs[2].X);
      double umin = Math.Min(Math.Min(qs[0].X, qs[1].X), qs[2].X);

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

      double tr = qs[1].Z - qs[0].Z;
      double ts = qs[2].Z - qs[0].Z;
      double dtdu = (tr * vs - ts * vr) / det;
      double dtdv = (ur * ts - us * tr) / det;
      double tc = qs[0].Z - dtdu * u0 - dtdv * v0;

      int tv = Math.Min(1 + (int)vmax, Ny - 1);
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
        if ((qs[0].Y <= tv) == (qs[1].Y >= tv))
        {
          u_[i] = ur0;
          i++;
        }
        if ((qs[0].Y <= tv) == (qs[2].Y >= tv))
        {
          u_[i] = us0;
          i++;
        }
        if ((qs[1].Y <= tv) == (qs[2].Y >= tv))
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
          int tu = Math.Min(Nx - 1, (int)tumax + 1);
          tumin = Math.Max(0, tumin - 1);

          double tx = dxdu * tu + dxdv * tv + xc;
          double ty = dydu * tu + dydv * tv + yc;
          double tt = dtdu * tu + dtdv * tv + tc;
          while (tu >= tumin)
          {
            double h = Fields[tu, tv];
            if (h == 0 ||
                (tu <= tumax && tu >= tumin && tt > h))
            {
              if (DrawPoint(tu, tv, tx, ty, argbFct) != 0)
              {
                Fields[tu, tv] = tt; //m_scene->trySetNew(t[0],tu,tv) == true) {
              }
            }
            if (Line[tu] == false)
            {
              if (tv + 1 < Ny &&
                  Fields[tu, tv + 1] == 0.0)
              {
                if (DrawPoint(tu, tv + 1, tx + dxdv, ty + dydv, argbFct) != 0)
                {
                  Fields[tu, tv + 1] = tt;
                }
              }
              Line[tu] = true;
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
      ResetLine((int)umin, (int)umax);
      return 0;
    }

    public int DrawPoint(int u, int v, double i, double j, Func<double,double,int> argbFct)
    {
      if (u < 0)
      { return 1; }

      int argb = argbFct(i, j);
      DrawPoint(u, v, argb);

      return 1;
    }


    private static void MinMax(IEnumerable<double> vals, ref double min, ref double max, int count = 0)
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

  }
}
