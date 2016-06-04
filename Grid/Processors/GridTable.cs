using Basics.Geom;
using Basics.Geom.Process;
using System;
using System.Collections.Generic;

namespace Grid.Processors
{
  public class GridTable : ISpatialTable<GridRow>
  {
    private readonly IGrid _grid;
    public GridTable(IGrid grid)
    {
      _grid = grid;
    }
    public IBox TileExtent { get; set; }

    IEnumerable<ISpatialRow> ISpatialTable.Search(IBox extent, string constraint)
    {
      foreach (ISpatialRow row in Search(extent))
      { yield return row; }
    }
    private IEnumerable<GridRow> Search(IBox extent)
    {
      double dx = TileExtent.Max.X - TileExtent.Min.X;
      double dy = TileExtent.Max.Y - TileExtent.Min.Y;
      int i0 = (int)Math.Floor((extent.Min.X - TileExtent.Min.X) / dx);
      int j0 = (int)Math.Floor((extent.Min.X - TileExtent.Min.Y) / dy);

      double x0 = TileExtent.Min.X + i0 * dx;
      double y0 = TileExtent.Min.Y + j0 * dy;

      GridExtent e = _grid.Extent;
      Box ge = new Box(new Point2D(e.X0, e.Y0), new Point2D(e.X0 + e.Dx * e.Nx, e.Y0 + e.Dy * e.Ny), verify: true);
      double xMin;
      for (int i = i0; (xMin = (x0 + (i - i0) * dx)) < extent.Max.X; i++)
      {
        if (xMin > ge.Max.X)
        { continue; }
        double xMax = xMin + dx;
        if (xMax < ge.Min.X)
        { continue; }

        double yMin;
        for (int j = j0; (yMin = (y0 + (j - j0) * dy)) < extent.Max.Y; j++)
        {
          if (yMin > ge.Max.Y)
          { continue; }
          double yMax = yMin + dy;
          if (yMax < ge.Min.Y)
          { continue; }

          Box rowExtent = new Box(new Point2D(xMin, yMin), new Point2D(xMax, yMax));
          yield return new GridRow(this, rowExtent);
        }
      }
    }
  }

  public class GridRow : ISpatialRow, IDisposable
  {
    private readonly Box _rowExtent;
    private readonly GridTable _grid;

    private BaseGrid _cashed;

    public GridRow(GridTable grid, Box rowExtent)
    {
      _grid = grid;
      _rowExtent = rowExtent;
    }
    ITable IRow.Table { get { return _grid; } }
    public IBox Extent { get { return _rowExtent; } }

    void IDisposable.Dispose()
    {
      if (_cashed != null)
      { _cashed = null; }
    }
  }
}
