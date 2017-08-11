
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

    public static double VegeHeight(List<Vector> pts)
    {
      double h = pts[pts.Count - 1][2] - pts[0][2];
      if (h > 19.2)
      { h = 19.2; }
      return 10 * h;
    }

    public static double Obstruction(List<Vector> pts)
    {
      int dens = 0;

      foreach (Vector pt in pts)
      {
        double d = pt[2] - pts[0][2];
        if (d > 0.3 && d < 2)
        { dens++; }
      }
      return 10 * dens;
    }

    public static DoubleGrid CreateGrid(TextReader reader, double res, Func<List<Vector>, double> grdFct)
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

        grd[ix, iy] = grdFct(pts);
      }
      return grd;
    }
  }
}