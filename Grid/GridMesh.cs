
using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid
{
  public class GridMesh : ITileMesh
  {
    private readonly IGrid<double> _baseGrid;
    private readonly Box _tileExtent;
    private GridMeshLine.Comparer _comparer;

    public GridMesh(IGrid<double> baseGrid)
    {
      _baseGrid = baseGrid;
      Point min = Point.Create(_baseGrid.Extent.Extent.Min);
      Point max = Point.Create(_baseGrid.Extent.Extent.Max);
      Point2D d = new Point2D(Math.Abs(_baseGrid.Extent.Dx) / 2.0, Math.Abs(_baseGrid.Extent.Dy) / 2.0);
      _tileExtent = new Box(min + d, max - d);
    }

    public IBox TileExtent
    { get { return _tileExtent; } }

    IComparer<IMeshLine> IMesh.LineComparer
    { get { return LineComparer; } }
    public GridMeshLine.Comparer LineComparer
    { get { return _comparer ?? (_comparer = new GridMeshLine.Comparer()); } }

    public IEnumerable<IMeshLine> Points
    {
      get
      {
        int nx = _baseGrid.Extent.Nx;
        int ny = _baseGrid.Extent.Ny;
        for (int ix = 0; ix < nx; ix++)
        {
          for (int iy = 0; iy < ny; iy++)
          {
            if (double.IsNaN(_baseGrid[ix, iy]))
            { continue; }

            foreach (var dir in Dir.MeshDirs)
            {
              int tx = ix + dir.Dx;
              if (tx < 0 || tx >= nx)
              { continue; }

              int ty = iy + dir.Dy;
              if (ty < 0 || ty >= ny)
              { continue; }

              if (double.IsNaN(_baseGrid[tx, ty]))
              { continue; }

              yield return new GridMeshLine(_baseGrid, ix, iy, dir);
              break;
            }
          }
        }
      }
    }
  }

  public class GridPoint : Point3D
  {
    private readonly IGrid<double> _grid;
    private readonly int _x;
    private readonly int _y;
    public GridPoint(IGrid<double> grid, int x, int y)
      : base(grid.Extent.X0 + (x + 0.5) * grid.Extent.Dx, grid.Extent.Y0 + (y + 0.5) * grid.Extent.Dy, grid[x, y])
    {
      _grid = grid;
      _x = x;
      _y = y;
    }
  }

  public class GridMeshLine : IMeshLine
  {
    public class Comparer : IComparer<GridMeshLine>, IComparer<IMeshLine>
    {
      int IComparer<IMeshLine>.Compare(IMeshLine x, IMeshLine y)
      { return Compare(x as GridMeshLine, y as GridMeshLine); }
      public int Compare(GridMeshLine x, GridMeshLine y)
      {
        if (x == null) { if (y == null) return 0; return 1; }
        if (y == null) return -1;

        int d = x._x0.CompareTo(y._x0);
        if (d != 0) return d;

        d = x._y0.CompareTo(y._y0);
        if (d != 0) return d;

        d = _dirCmp.Compare(x._dir, y._dir);
        return d;
      }
    }

    private static Dir.Comparer _dirCmp = new Dir.Comparer();

    private readonly IGrid<double> _grid;
    private readonly int _x0;
    private readonly int _y0;
    private readonly Dir _dir;

    private GridPoint _start;
    private GridPoint _end;

    public GridMeshLine(IGrid<double> grid, int x0, int y0, Dir dir)
    {
      _grid = grid;
      _x0 = x0;
      _y0 = y0;
      _dir = dir;
    }

    public override string ToString()
    {
      char cAngle = MeshUtils.GetChar(this, out double angle);

      return string.Format("{0},{1} + {2} ({3:N1}, {4:N1}, {5:N1} + {6:N1}, {7:N1}, {8:N1})",
        _x0, _y0, cAngle,
        Start.X, Start.Y, Start.Z,
        End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z);
    }

    public IGrid<double> BaseGrid => _grid;

    public Dir Dir => _dir;

    public IPoint Start
    {
      get { return _start ?? (_start = CreatePoint(_x0, _y0)); }
    }

    public IPoint End
    {
      get { return _end ?? (_end = CreateEndPoint()); }
    }

    IMeshLine IMeshLine.Invers()
    {
      GridMeshLine invers = new GridMeshLine(_grid, _x0 + _dir.Dx, _y0 + _dir.Dy, new Dir(-_dir.Dx, -_dir.Dy))
      {
        _start = _end,
        _end = _start
      };
      return invers;
    }
    IMeshLine IMeshLine.GetNextTriLine()
    {
      Dir nextDir = Dir.GetNextTriDir(_dir);
      int x1 = _x0 + _dir.Dx;
      int x2 = x1 + nextDir.Dx;
      if (x2 < 0 || x2 >= _grid.Extent.Nx)
      { return null; }

      int y1 = _y0 + _dir.Dy;
      int y2 = y1 + nextDir.Dy;
      if (y2 < 0 || y2 >= _grid.Extent.Ny)
      { return null; }
      GridMeshLine next = new GridMeshLine(_grid, x1, y1, nextDir) { _start = _end };

      return next;
    }

    IMeshLine IMeshLine.GetPreviousTriLine()
    {
      Dir preDir = Dir.GetPreTriDir(_dir);
      int x2 = _x0 - preDir.Dx;
      if (x2 < 0 || x2 >= _grid.Extent.Nx)
      { return null; }

      int y2 = _y0 - preDir.Dy;
      if (y2 < 0 || y2 >= _grid.Extent.Ny)
      { return null; }
      GridMeshLine next = new GridMeshLine(_grid, x2, y2, preDir) { _end = _start };

      return next;
    }


    private GridPoint CreateEndPoint()
    {
      GridPoint end = CreatePoint(_x0 + _dir.Dx, _y0 + _dir.Dy);
      return end;
    }
    private GridPoint CreatePoint(int ix, int iy)
    {
      GridPoint p = new GridPoint(_grid, ix, iy) { Z = _grid[ix, iy] };
      return p;
    }
  }
}
