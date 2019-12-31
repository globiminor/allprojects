using Basics.Geom;
using System;
using System.Collections.Generic;
using System.IO;

namespace Grid
{
  public enum EGridInterpolation { nearest, bilinear };

  /// <summary>
  /// basic functions for grid calculations
  /// </summary>
  public class GridExtent : IGeometry
  {
    private int _nx;
    private int _ny;
    private double _x0; // upper left corner position
    private double _y0;
    private double _dx;
    private double _dy;

    private void Init(int nx, int ny, double x0, double y0, double dx, double dy)
    {
      _nx = nx;
      _ny = ny;
      _x0 = x0;
      _y0 = y0;
      _dx = dx;
      _dy = dy;
    }

    public bool EqualGeometry(IGeometry other)
    { return this == other; }

    public GridExtent(int nx, int ny, double x0, double y0, double dx, double dy)
    {
      Init(nx, ny, x0, y0, dx, dy);
    }

    public GridExtent(int nx, int ny, double x0, double y0, double dx)
    {
      Init(nx, ny, x0, y0, dx, -dx);
    }

    public bool GetNearest(IPoint p, out int ix, out int iy)
    { return GetNearest(p.X, p.Y, out ix, out iy); }
    public bool GetNearest(double x, double y, out int ix, out int iy)
    {
      ix = (int)Math.Floor((x - _x0) / _dx);
      iy = (int)Math.Floor((y - _y0) / _dy);
      return (ix >= 0 && ix < _nx && iy >= 0 && iy < _ny);
    }

    public void GridToWorld(IEnumerable<Point> gridPath)
    {
      foreach (var point in gridPath)
      {
        int x = (int)point.X;
        int y = (int)point.Y;
        Point p = CellLL(x, y);
        point.X = p.X;
        point.Y = p.Y;
      }
    }
    public void WorldToGrid(IEnumerable<Point> worldPath)
    {
      foreach (var point in worldPath)
      {
        GetNearest(point, out int ix, out int iy);
        point.X = ix;
        point.Y = iy;
      }
    }

    public void GetBilinear(double x, double y,
      out int ix, out int iy, out double dx, out double dy)
    {
      dx = (x - _x0) / _dx - 0.5;
      dy = (y - _y0) / _dy - 0.5;
      ix = (int)dx;
      iy = (int)dy;
      dx -= ix;
      dy -= iy;
    }

    private GridExtent()
    { }

    internal delegate string GetLine();
    public static GridExtent GetAsciiHeader(TextReader file)
    {
      return GetAsciiHeader(file.ReadLine);
    }
    internal static GridExtent GetAsciiHeader(GetLine getLine)
    {
      /* x0, y0 : lower left corner of lower left element of matrix */
      GridExtent extent = new GridExtent();

      using (new InvariantCulture())
      {
        int inx, iny, ix0, iy0, idx, inn;

        inx = 0;
        iny = 0;
        ix0 = 0;
        iy0 = 0;
        idx = 0;
        inn = 0;

        do
        {
          string line = getLine();
          string[] parts = line.Split();
          int n = parts.Length;
          if (n < 2)
          {
            return null;
          }

          string code = parts[0].ToLower();

          int i = 1;
          while (i < n && string.IsNullOrEmpty(parts[i]))
          {
            i++;
          }
          if (i >= n)
          {
            return null;
          }

          if (double.TryParse(parts[i], out double val) == false)
          {
            return null;
          }

          if (code.CompareTo("ncols") == 0)
          {
            extent._nx = (int)val;
            inx = 1;
          }
          else if (code.CompareTo("nrows") == 0)
          {
            extent._ny = (int)val;
            iny = 1;
          }
          else if (code.CompareTo("xllcorner") == 0)
          {
            extent._x0 = val;
            ix0 = 1;
          }
          else if (code.CompareTo("xllcenter") == 0)
          {
            extent._x0 = val;
            ix0 = 2;
          }
          else if (code.CompareTo("yllcorner") == 0)
          {
            extent._y0 = val;
            iy0 = 1;
          }
          else if (code.CompareTo("yllcenter") == 0)
          {
            extent._y0 = val;
            iy0 = 2;
          }
          else if (code.CompareTo("cellsize") == 0)
          {
            extent._dx = val;
            idx = 1;
          }
          else if (code.CompareTo("nodata_value") == 0)
          {
            extent.NN = val;
            inn = 1;
          }
        } while (inx == 0 || iny == 0 || ix0 == 0 ||
                 iy0 == 0 || idx == 0 || inn == 0);

        if (ix0 == 2)
        {
          extent._x0 -= extent._dx / 2;
        }
        if (iy0 == 2)
        {
          extent._y0 -= extent._dx / 2;
        }
        extent._y0 += extent._ny * extent._dx;
        extent._dy = -extent._dx;
      }
      return extent;
    }

    internal void SetSize(int nx, int ny)
    {
      _nx = nx;
      _ny = ny;
    }
    public static GridExtent FromWorldFile(TextReader file)
    {
      GridExtent extent = new GridExtent();
      using (new InvariantCulture())
      {
        double d;
        extent._dx = Convert.ToDouble(file.ReadLine());
        d = Convert.ToDouble(file.ReadLine());
        if (d != 0)
        {
          throw new Exception("cannot handle rotated grid");
        }
        d = Convert.ToDouble(file.ReadLine());
        if (d != 0)
        {
          throw new Exception("cannot handle rotated grid");
        }
        extent._dy = Convert.ToDouble(file.ReadLine());
        extent._x0 = Convert.ToDouble(file.ReadLine());
        extent._y0 = Convert.ToDouble(file.ReadLine());
      }
      return extent;
    }

    public bool EqualExtent(GridExtent other)
    {
      if (_x0 == other._x0 && _y0 == other._y0 &&
        _nx == other._nx && _ny == other._ny && _dx == other._dx && _dy == other._dy)
      {
        return true;
      }
      return false;
    }
    public static GridExtent GetWorldFile(string name)
    {
      if (!File.Exists(name))
      { return null; }
      using (TextReader pFile = new StreamReader(name))
      {
        GridExtent pExtent = FromWorldFile(pFile);
        return pExtent;
      }
    }

    public int Nx
    {
      get { return _nx; }
    }
    public int Ny
    {
      get { return _ny; }
    }
    public double X0
    {
      get { return _x0; }
    }
    public double X1 => X0 + Nx * _dx;
    public double Y0
    {
      get { return _y0; }
    }
    public double Y1 => Y0 + Ny * _dy;

    public double Dx
    {
      get { return _dx; }
    }
    public double Dy
    {
      get { return _dy; }
    }

    /// <summary>
    /// gets the number that shall be treated as NaN
    /// </summary>
    public double NN { get; set; }

    public Point2D CellLL(int ix, int iy)
    {
      return new Point2D(_x0 + ix * _dx, _y0 + iy * _dy);
    }

    public Point2D CellCenter(int ix, int iy)
    {
      return new Point2D(_x0 + (ix + 0.5) * _dx, _y0 + (iy + 0.5) * _dy);
    }


    #region IGeometry Members

    public int Topology
    {
      get { return 2; }
    }

    public int Dimension
    {
      get { return 2; }
    }

    public Box Extent
    {
      get
      {
        return new Box(new Point2D(_x0, _y0),
          new Point2D(_x0 + _nx * _dx, _y0 + _ny * _dy), true);
      }
    }
    IBox IGeometry.Extent
    { get { return Extent; } }

    public bool IsWithin(IPoint point)
    {
      return Extent.IsWithin(point);
    }
    public bool Intersects(IGeometry geometry)
    {
      return BoxOp.Intersects(Extent, geometry);
    }
    public GeometryCollection Intersection(IGeometry geometry)
    {
      return Extent.Intersection(geometry);
    }

    public IGeometry Border
    { get { return null; } }

    public IGeometry Project(IProjection projection)
    {
      throw new NotImplementedException();
    }

    public IGeometry Split(IGeometry[] border)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
