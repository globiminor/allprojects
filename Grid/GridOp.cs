
using Basics.Geom;
using System;
using System.IO;

namespace Grid
{
  public static class GridOp
  {
    public static T Value<T>(this IGrid<T> grid, double x, double y)
    {
      grid.Extent.GetNearest(x, y, out int ix, out int iy);
      return grid[ix, iy]; // this[ix, iy];
    }

    public static T Min<T>(this IGrid<T> grid)
      where T : IComparable<T>
    {
      T m = default;
      bool first = true; ;
      for (int j = 0; j < grid.Extent.Ny; j++)
      {
        for (int i = 0; i < grid.Extent.Nx; i++)
        {
          T t = grid[i, j];
          if (first)
          {
            m = t;
            first = false;
          }
          else if (t.CompareTo(m) < 0)
            m = t;
        }
      }
      return m;
    }

    public static double Min(this IGrid<double> grid)
    {
      double m = double.MaxValue;
      for (int j = 0; j < grid.Extent.Ny; j++)
      {
        for (int i = 0; i < grid.Extent.Nx; i++)
        {
          double t = grid[i, j];
          if (double.IsNaN(t))
          { continue; }
          if (t < m)
            m = t;
        }
      }
      return m;
    }

    public static T Max<T>(this IGrid<T> grid)
      where T : IComparable<T>
    {
      T m = default;
      bool first = true; ;
      for (int j = 0; j < grid.Extent.Ny; j++)
      {
        for (int i = 0; i < grid.Extent.Nx; i++)
        {
          T t = grid[i, j];
          if (first)
          {
            m = t;
            first = false;
          }
          else if (t.CompareTo(m) > 0)
            m = t;
        }
      }
      return m;
    }
    public static double Max(this IGrid<double> grid)
    {
      double m = double.MinValue;
      for (int j = 0; j < grid.Extent.Ny; j++)
      {
        for (int i = 0; i < grid.Extent.Nx; i++)
        {
          double t = grid[i, j];
          if (double.IsNaN(t))
          { continue; }
          if (t > m)
            m = t;
        }
      }
      return m;
    }

    public static IGrid<int> Abs(this IGrid<int> grid)
    {
      return new GridConvert<int, int>(grid, (g, ix, iy) => Math.Abs(g[ix, iy]));
    }
    public static IGrid<int> Mod(this IGrid<int> grid, int mod)
    {
      return new GridConvert<int, int>(grid, (g, ix, iy) => g[ix, iy] % mod);
    }
    public static IGrid<int> ToInt(this IGrid<double> grid)
    {
      return new GridConvert<double, int>(grid, (g, ix, iy) => (int)g[ix, iy]);
    }
    public static IGrid<int> Add(this IGrid<int> grid, int add)
    {
      return new GridConvert<int, int>(grid, (g, ix, iy) => g[ix, iy] + add);
    }
    public static IGrid<double> Add(this IGrid<double> grid, double add)
    {
      return new GridConvert<double, double>(grid, (g, ix, iy) => g[ix, iy] + add);
    }
    public static IGrid<double> Add(this IGrid<double> x, IGrid<double> y, GridExtent extent = null,
      EGridInterpolation interpol = EGridInterpolation.nearest)
    {
      if (interpol == EGridInterpolation.nearest)
      {
        return new GridFunc<double, double, double>(x, y, (vx, vy) => vx + vy, extent);
      }
      return new GridFunc<double, double, double>(x, y, (vx, vy) => vx + vy, extent,
        xValueFct: (g, xc, yc) => g.Value(xc, yc, interpol),
        yValueFct: (g, xc, yc) => g.Value(xc, yc, interpol));
    }

    public static IGrid<double> Sub(this IGrid<double> grid, double sub)
    {
      return new GridConvert<double, double>(grid, (g, ix, iy) => g[ix, iy] - sub);
    }

    public static IGrid<double> Mult(this IGrid<double> grid, double mult)
    {
      return new GridConvert<double, double>(grid, (g, ix, iy) => g[ix, iy] * mult);
    }
    public static IGrid<double> Div(this IGrid<double> grid, double div)
    {
      return new GridConvert<double, double>(grid, (g, ix, iy) => g[ix, iy] / div);
    }

    public static IGrid<double> Max(IGrid<double> x, IGrid<double> y, GridExtent extent = null)
    {
      return new GridFunc<double, double, double>(x, y, (vx, vy) => 
      {
        if (double.IsNaN(vx)) return vy;
        if (double.IsNaN(vy)) return vx;
        return Math.Max(vx, vy);
      }, extent);
    }


    public static double Value(this IGrid<double> grid, double x, double y, EGridInterpolation eInter)
    {
      if (eInter == EGridInterpolation.nearest)
      {
        return grid.Value(x, y);
      }
      else if (eInter == EGridInterpolation.bilinear)
      {
        double h00, h01, h10, h11;
        double h0, h1;

        grid.Extent.GetBilinear(x, y, out int ix, out int iy, out double dx, out double dy);

        h00 = grid[ix, iy];
        if (dx == 0)
        {
          if (dy == 0)
          {
            return h00;
          }
          h01 = grid[ix, iy + 1];
          return h00 + (h01 - h00) * dy;
        }
        if (dy == 0)
        {
          h10 = grid[ix + 1, iy];
          return h00 + (h10 - h00) * dx;
        }

        h01 = grid[ix, iy + 1];
        h10 = grid[ix + 1, iy];
        h11 = grid[ix + 1, iy + 1];

        h0 = h00 + (h10 - h00) * dx;
        h1 = h01 + (h11 - h01) * dx;
        return h0 + (h1 - h0) * dy;
      }
      else
      {
        throw new Exception("Unhandled Interpolation method " + eInter);
      }
    }

    public static double Slope2(this IGrid<double> grid, double x, double y)
    {
      Point3D vertical = Vertical(grid, x, y);
      return (vertical.X * vertical.X + vertical.Y * vertical.Y);
    }

    public static Point3D Vertical(this IGrid<double> grid, double x, double y)
    {
      grid.Extent.GetBilinear(x, y, out int ix, out int iy, out double dx, out double dy);

      Point3D p = Vertical(grid, ix, iy, dx, dy);
      return p;
    }
    public static Point3D Vertical(this IGrid<double> grid, int ix, int iy, double dx, double dy)
    {
      double h00 = grid[ix, iy];
      double h01 = grid[ix, iy + 1];
      double h10 = grid[ix + 1, iy];
      double h11 = grid[ix + 1, iy + 1];

      double h0 = h00 + (h10 - h00) * dx;
      double h1 = h01 + (h11 - h01) * dx;

      double dhy = h1 - h0;
      double dhx = (h10 + (h11 - h10) * dy) - (h00 + (h01 - h00) * dy);

      double h = h0 + dhy * dy;

      dhy /= grid.Extent.Dy;
      dhx /= grid.Extent.Dx;

      Point3D p = new Point3D(dhx, dhy, h);

      return p;
    }

    public static Polyline Profil(this IGrid<double> grid, Polyline line)
    {
      Polyline profile = new Polyline();
      IPoint pos;

      double sumDist = 0;
      double h;
      pos = line.Points[0];
      h = grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);
      profile.Add(new Point2D(0, h));
      foreach (var segment in line.EnumSegments())
      {
        double dist = segment.Length();
        sumDist += dist;

        pos = segment.End;
        h = grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);

        profile.Add(new Point2D(sumDist, h));
      }
      return profile;
    }

    public static Polyline GetContour(this IGrid<double> grid, double x0, double y0)
    {
      grid.Extent.GetBilinear(x0, y0, out int ix, out int iy, out double dx, out double dy);

      double hContour = grid.Value(x0, y0, EGridInterpolation.bilinear);
      if (double.IsNaN(hContour))
      { return null; }

      double h00 = grid[ix, iy];
      double h01 = grid[ix, iy + 1];

      IMeshLine start;
      double h0;
      double h1;
      if (h00.CompareTo(hContour) != h01.CompareTo(hContour))
      {
        h0 = h00;
        h1 = h01;
        start = new GridMeshLine(grid, ix, iy, Dir.N);
      }
      else if (h01.CompareTo(hContour) != (h1 = grid[ix + 1, iy + 1]).CompareTo(hContour))
      {
        h0 = h01;
        start = new GridMeshLine(grid, ix, iy + 1, Dir.E);
      }
      else if (h00.CompareTo(hContour) != (h1 = grid[ix + 1, iy]).CompareTo(hContour))
      {
        h0 = h00;
        start = new GridMeshLine(grid, ix + 1, iy, Dir.E);
      }
      else
      { return null; }

      if (h0 > h1)
      { start = start.Invers(); }

      double f = (hContour - h0) / (h1 - h0);
      return MeshUtils.GetContour(start, f, new GridMeshLine.Comparer());
    }

    public static IGrid<double> HillShading(this IGrid<double> grid, Func<Point3D, double> shadeFunction)
    {
      return new GridConvert<double, double>(grid, (g, ix, iy) =>
       {
         Point3D v = GridOp.Vertical(g, ix, iy, 0, 0);
         double s = shadeFunction(v);
         return s;
       });
    }

    public static void SaveASCII(this IGrid<double> grid, string name, string format)
    {
      using (TextWriter w = new StreamWriter(name, false))
      {
        w.WriteLine("NCOLS {0}", grid.Extent.Nx);
        w.WriteLine("NROWS {0}", grid.Extent.Ny);
        w.WriteLine("XLLCORNER {0}", grid.Extent.X0);
        w.WriteLine("YLLCORNER {0}", grid.Extent.Y0 + (grid.Extent.Ny - 1) * grid.Extent.Dy);
        w.WriteLine("CELLSIZE {0}", grid.Extent.Dx);
        w.WriteLine("NODATA_VALUE {0}", grid.Extent.NN);
        for (int iy = 0; iy < grid.Extent.Ny; iy++)
        {
          for (int ix = 0; ix < grid.Extent.Nx; ix++)
          {
            double val = grid[ix, iy];
            if (double.IsNaN(val)) val = grid.Extent.NN;
            w.Write("{0} ", val.ToString(format));
          }
          w.WriteLine();
        }
      }
    }

  }

  public class GridConvert<TI, TO> : IGrid<TO>
  {
    private readonly IGrid<TI> _sourceGrid;
    private readonly Func<IGrid<TI>, int, int, TO> _calc;

    public GridConvert(IGrid<TI> sourceGrid, Func<IGrid<TI>, int, int, TO> calcFct)
    {
      _sourceGrid = sourceGrid;
      _calc = calcFct;
    }
    public GridExtent Extent => _sourceGrid.Extent;
    Type IGrid.Type => typeof(TO);
    object IGrid.this[int ix, int iy] => this[ix, iy];
    public TO this[int ix, int iy]
    {
      get
      {
        TO target = _calc(_sourceGrid, ix, iy);
        return target;
      }
    }
  }

  public class GridFunc<TX, TY, TO> : IGrid<TO>
  {
    private readonly IGrid<TX> _x;
    private readonly IGrid<TY> _y;
    private readonly Func<TX, TY, TO> _calc;

    private readonly GridExtent _extent;
    private readonly bool _xExtentEquals;
    private readonly bool _yExtentEquals;

    private readonly Func<IGrid<TX>, double, double, TX> _xValueFct;
    private readonly Func<IGrid<TY>, double, double, TY> _yValueFct;

    public GridFunc(IGrid<TX> x, IGrid<TY> y, Func<TX, TY, TO> calcFct,
      GridExtent extent = null,
      Func<IGrid<TX>, double, double, TX> xValueFct = null,
      Func<IGrid<TY>, double, double, TY> yValueFct = null)
    {
      _x = x;
      _y = y;
      _calc = calcFct;

      _extent = extent ?? _x.Extent;

      _xExtentEquals = _extent.EqualExtent(_x.Extent);
      _yExtentEquals = _extent.EqualExtent(_y.Extent);

      _xValueFct = xValueFct;
      _yValueFct = yValueFct;
    }
    public GridExtent Extent => _extent;

    Type IGrid.Type => typeof(TO);
    object IGrid.this[int ix, int iy] => this[ix, iy];
    public TO this[int ix, int iy]
    {
      get
      {
        double? xCoord = null;
        double? yCoord = null;

        TX x = GetValue(_x, ix, iy, ref xCoord, ref yCoord, _xExtentEquals, _xValueFct);
        TY y = GetValue(_y, ix, iy, ref xCoord, ref yCoord, _yExtentEquals, _yValueFct);

        TO value = _calc(x, y);
        return value;
      }
    }
    private T GetValue<T>(IGrid<T> g, int ix, int iy, ref double? x, ref double? y, bool extentEquals,
      Func<IGrid<T>, double, double, T> valueFunc)
    {
      if (extentEquals)
        return g[ix, iy];

      x = x ?? _extent.X0 + ix * _extent.Dx;
      y = y ?? _extent.Y0 + iy * _extent.Dy;
      T value = valueFunc == null
        ? g.Value(x.Value, y.Value)
        : valueFunc(g, x.Value, y.Value);
      return value;
    }
  }

}
