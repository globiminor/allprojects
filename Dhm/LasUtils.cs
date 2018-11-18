
using Basics.Geom;
using Grid;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dhm
{
  public static class LasUtils
  {
    private class LasPoint
    {
      public float X { get; set; }
      public float Y { get; set; }
      public float Z { get; set; }
      public int Intesity { get; set; }

      public Vector ToVector(double x0, double y0)
      {
        Vector v = new Vector(4);
        v[0] = x0 + X;
        v[1] = y0 + Y;
        v[2] = Z;
        v[3] = Intesity;

        return v;
      }

      public static List<Vector> GetVectors(double x0, double y0, List<LasPoint> pts)
      {
        List<Vector> vectors = new List<Vector>(pts.Count);
        foreach (LasPoint pt in pts)
        { vectors.Add(pt.ToVector(x0, y0)); }
        return vectors;
      }
    }

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

    static byte[] _red = new byte[] { 0, 64, 128, 160, 192, 255 };
    static byte[] _green = new byte[] { 0, 64, 128, 160, 192, 224, 255 };
    static byte[] _blue = new byte[] { 0, 64, 128, 160, 192, 255 };

    public static void InitStructColors(byte[] r, byte[] g, byte[] b)
    {

      int idx = 1;
      for (int iHeight = 0; iHeight < _red.Length; iHeight++)
      {
        for (int iDens = 0; iDens < _green.Length; iDens++)
        {
          for (int iUndef = 0; iUndef < _blue.Length; iUndef++)
          {
            r[idx] = _red[iHeight];
            g[idx] = _green[iDens];
            b[idx] = _blue[iUndef];
            idx++;
          }
        }
      }
      r[0] = 255;
      g[0] = 255;
      b[0] = 255;
    }

    public static double Struct(int ix, int iy, Func<int, int, List<Vector>> getPts)
    {
      List<Vector> pts = getPts(ix, iy);

      Vector pA0 = getPts(ix + 1, iy)?[0];
      Vector p0A = getPts(ix, iy + 1)?[0];
      Vector pM0 = getPts(ix - 1, iy)?[0];
      Vector p0M = getPts(ix, iy - 1)?[0];

      Quads q = new Quads(pts[0], pA0, p0A, pM0, p0M);
      double vegMin = 0.3;
      double vegMax = 2.0;
      bool forest = false;
      List<double> vegHs = new List<double>(pts.Count);
      List<int> intens = new List<int>(pts.Count);
      foreach (Vector pt in pts)
      {
        double d = q.GetDh(pt);
        if (d < vegMin)
        { intens.Add((int)pt[3]); }
        else if (d < vegMax)
        { vegHs.Add(d); }
        else
        { forest = true; }
      }
      if (forest)
      { }
      if (vegHs.Count > 0)
      {
        double max = 0;
        double sum = 0;
        double sum2 = 0;
        int nDens = vegHs.Count;
        foreach (double d in vegHs)
        {
          max = Math.Max(d, max);
          sum += d;
          sum2 += d * d;
        }
        double m = sum / nDens;
        double dev = Math.Sqrt((sum2 - m * m) / nDens);

        int iGreen =
          nDens < 3 ? 6
          : nDens < 6 ? 5
          : nDens < 10 ? 4
          : nDens < 15 ? 3
          : nDens < 25 ? 2 : 1;

        int iRed;
        int iBlue;
        double mm = (vegMin + vegMax) / 2.0;
        if (m < mm)
        {
          iRed = Math.Min(iGreen, (int)((mm - m) / (mm - vegMin) * (_red.Length - 1)));
          iBlue = 0;
        }
        else
        {
          iRed = 0;
          iBlue = Math.Min(iGreen, (int)((m - mm) / (vegMax - mm) * (_blue.Length - 1)));
        }

        return 1 + iRed * _green.Length * _blue.Length + iGreen * _blue.Length + iBlue;
      }
      //if (forest)
      //{ return 0; }

      {
        int sumIntens = 0;
        foreach (int inten in intens)
        {
          sumIntens += inten;
        }
        int mean = sumIntens / intens.Count;

        int i = mean > 700 ? 0
          : mean > 600 ? 1
          : mean > 500 ? 2
          : mean > 400 ? 3
          : mean > 300 ? 4 : 5;

        int idx = 1 + (_red.Length - 1) * _green.Length * _blue.Length + i * _blue.Length + i;

        return idx;
      }
    }

    public static double Dom(int ix, int iy, Func<int, int, List<Vector>> getPts)
    {
      List<Vector> pts = getPts(ix, iy);

      List<Vector> pA0 = getPts(ix + 1, iy);
      List<Vector> p0A = getPts(ix, iy + 1);
      List<Vector> pM0 = getPts(ix - 1, iy);
      List<Vector> p0M = getPts(ix, iy - 1);

      if (pts == null || p0A == null || pA0 == null || pM0 == null || p0M == null)
      { return 0; }

      Vector p0 = pts[pts.Count - 1];

      double da = (p0A[p0A.Count - 1].Z + pA0[pA0.Count - 1].Z - 2 * p0.Z);
      double dm = (p0M[p0M.Count - 1].Z + pM0[pM0.Count - 1].Z - 2 * p0.Z);

      double d = Math.Sqrt(da * da + dm * dm);

      return 5 * d;
      //      int iColor = (int)Math.Min(max / 2, 15);
      //      int iStd = (int)Math.Min(15 * dev / max, 15);
      //      return Math.Min(m * 8, 255);
      //      return Math.Min(16 * iColor + iStd, 255);
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
      Dictionary<Cell, List<LasPoint>> ptsDict = new Dictionary<Cell, List<LasPoint>>(new CellComparer());

      string line;
      while ((line = reader.ReadLine()) != null)
      {
        IList<string> parts = line.Split();
        double x = double.Parse(parts[0]);
        double y = double.Parse(parts[1]);
        double z = double.Parse(parts[2]);
        int intens = int.Parse(parts[3]);

        int ix = (int)(x / res);
        int iy = (int)(y / res);

        LasPoint v = new LasPoint
        {
          X = (float)(x - ix * res),
          Y = (float)(y - iy * res),
          Z = (float)z,
          Intesity = intens
        };

        Cell key = new Cell { IX = ix, IY = iy };
        if (!ptsDict.TryGetValue(key, out List<LasPoint> pts))
        {
          pts = new List<LasPoint>();
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
        List<LasPoint> pts = pair.Value;
        pts.Sort((x, y) => { return x.Z.CompareTo(y.Z); });
      }
      foreach (var pair in ptsDict)
      {
        Cell c = pair.Key;
        int ix = c.IX - xMin;
        int iy = yMax - c.IY;

        grd[ix, iy] = grdFct(c.IX, c.IY, (tx, ty) =>
        {
          Cell key = new Cell { IX = tx, IY = ty };
          if (!ptsDict.TryGetValue(key, out List<LasPoint> pts))
          { return null; }
          return LasPoint.GetVectors(tx * res, ty * res, pts);
        });
      }

      return grd;
    }
  }
}