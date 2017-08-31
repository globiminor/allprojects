
using Basics.Geom;
using Grid;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dhm
{
  public static class LasUtils
  {
    private class Cell
    {
      public int IX { get; set; }
      public int IY { get; set; }
    }
    private class CellComparer : IComparer<Cell>, IEqualityComparer<Cell>
    {
      public int Compare(Cell x, Cell y)
      {
        if (x == y)
        { return 0; }
        if (x == null)
        { return -1; }
        if (y == null)
        { return 1; }

        int d = x.IX.CompareTo(y.IX);
        if (d != 0)
        { return d; }

        d = x.IY.CompareTo(y.IY);
        if (d != 0)
        { return d; }

        return 0;
      }

      public bool Equals(Cell x, Cell y)
      { return Compare(x, y) == 0; }

      public int GetHashCode(Cell x)
      {
        return x.IX + 41 * x.IY;
      }
    }

    public static double VegeHeight(int ix, int iy, Func<int, int, List<Vector>> getPts)
    {
      List<Vector> pts = getPts(ix, iy);

      double h = pts[pts.Count - 1][2] - pts[0][2];
      if (h > 19.2)
      { h = 19.2; }
      return 10 * h;
    }

    public static double Obstruction(int ix, int iy, Func<int, int, List<Vector>> getPts)
    {
      int dens = 0;

      List<Vector> pts = getPts(ix, iy);

      Vector pA0 = getPts(ix + 1, iy)?[0];
      Vector p0A = getPts(ix, iy + 1)?[0];
      Vector pM0 = getPts(ix - 1, iy)?[0];
      Vector p0M = getPts(ix, iy - 1)?[0];

      Quads q = new Quads(pts[0], pA0, p0A, pM0, p0M);
      double min = 0.3;
      double max = 2.0;
      foreach (Vector pt in pts)
      {

        double dMax = pt[2] - q.Max;
        if (dMax > max)
        { continue; }
        double dMin = pt[2] - q.Min;
        if (dMin < min)
        { continue; }


        if (dMin < max && dMax > min)
        { dens++; }
        else
        {
          double d = q.GetDh(pt);
          if (d > min && d < max)
          { dens++; }
        }
      }
      return 10 * dens;
    }

    private class Quads
    {
      private readonly Vector _center;
      private readonly Vector _pA0;
      private readonly Vector _p0A;
      private readonly Vector _pM0;
      private readonly Vector _p0M;

      private double _max = double.NaN;
      private double _min = double.NaN;

      public Quads(Vector center, Vector pA0, Vector p0A, Vector pM0, Vector p0M)
      {
        _center = center;
        _pA0 = pA0;
        _p0A = p0A;
        _pM0 = pM0;
        _p0M = p0M;
      }

      public double Max
      {
        get
        {
          if (double.IsNaN(_max))
          {
            _max = _center[2];
            _max = Math.Max(_pA0?[2] ?? _max, _max);
            _max = Math.Max(_p0A?[2] ?? _max, _max);
            _max = Math.Max(_pM0?[2] ?? _max, _max);
            _max = Math.Max(_p0M?[2] ?? _max, _max);
          }
          return _max;
        }
      }

      public double Min
      {
        get
        {
          if (double.IsNaN(_min))
          {
            _min = _center[2];
            _min = Math.Min(_pA0?[2] ?? _min, _min);
            _min = Math.Min(_p0A?[2] ?? _min, _min);
            _min = Math.Min(_pM0?[2] ?? _min, _min);
            _min = Math.Min(_p0M?[2] ?? _min, _min);
          }
          return _min;
        }
      }

      private Point[] _normal;
      private Point Normal
      {
        get
        {
          return (_normal ?? (_normal = new[] { GetNormal() }))[0];
        }
      }
      private Point GetNormal()
      {
        List<Vector> vs = new List<Vector> {
          _pA0?.Sub(_center),
          _p0A?.Sub(_center),
          _pM0?.Sub(_center),
          _p0M?.Sub(_center),
        };

        Vector pre = vs[3];
        Point sumN = null;
        foreach (Vector v in vs)
        {
          Point3D n = pre?.VectorProduct3D(v);
          pre = v;

          if (n == null)
          { continue; }
          if (sumN == null)
          { sumN = n; }
          else
          { sumN = sumN + n; }
        }
        if (sumN == null)
        { return null; }
        return (1.0 / Math.Sqrt(sumN.OrigDist2())) * sumN;
      }

      public double GetDh(Vector p)
      {
        if (_center == p)
        { return 0; }

        return Normal?.SkalarProduct(p - _center) ?? (p[2] - _center[2]);
      }
    }

    public static DoubleGrid CreateGrid(TextReader reader, double res, Func<int, int, Func<int, int, List<Vector>>, double> grdFct)
    {
      Dictionary<Cell, List<Vector>> ptsDict = new Dictionary<Cell, List<Vector>>(new CellComparer());

      string line;
      while ((line = reader.ReadLine()) != null)
      {
        IList<string> parts = line.Split();
        Vector v = new Vector(4);
        v[0] = double.Parse(parts[0]);
        v[1] = double.Parse(parts[1]);
        v[2] = double.Parse(parts[2]);
        v[3] = int.Parse(parts[3]);

        int ix = (int)(v.X / res);
        int iy = (int)(v.Y / res);

        Cell key = new Cell { IX = ix, IY = iy };
        List<Vector> pts;
        if (!ptsDict.TryGetValue(key, out pts))
        {
          pts = new List<Vector>();
          ptsDict.Add(key, pts);
        }
        pts.Add(v);
      }

      int xMin = int.MaxValue, xMax = int.MinValue;
      int yMin = int.MaxValue, yMax = int.MinValue;
      foreach (Cell cell in ptsDict.Keys)
      {
        xMin = Math.Min(cell.IX, xMin);
        yMin = Math.Min(cell.IY, yMin);

        xMax = Math.Max(cell.IX, xMax);
        yMax = Math.Max(cell.IY, yMax);
      }

      DataDoubleGrid grd = new DataDoubleGrid(xMax - xMin + 1, yMax - yMin + 1, typeof(short), xMin * res, yMax * res, res, 0, 0.01);

      foreach (var pair in ptsDict)
      {
        Cell c = pair.Key;
        int ix = c.IX - xMin;
        int iy = yMax - c.IY;
        List<Vector> pts = pair.Value;
        pts.Sort((x, y) => { return x[2].CompareTo(y[2]); });
      }
      foreach (var pair in ptsDict)
      {
        Cell c = pair.Key;
        int ix = c.IX - xMin;
        int iy = yMax - c.IY;

        grd[ix, iy] = grdFct(c.IX, c.IY, (tx, ty) =>
        {
          Cell key = new Cell { IX = tx, IY = ty };
          List<Vector> pts;
          if (!ptsDict.TryGetValue(key, out pts))
          { return null; }
          return pts;
        });
      }

      return grd;
    }
  }
}