using Basics.Geom;
using Basics.Geom.Process;
using System;
using System.Collections.Generic;

namespace Grid.Processors
{
  public class GridTable<T> : GridTable, ISpatialTable<GridRow<T>>
  {
    public GridTable(IGrid<T> grid)
      : base(grid)
    { }

    protected override GridRow CreateRow(Box extent)
    { return new GridRow<T>(this, extent); }
  }
  public class SimpleGridTable : GridTable
  {
    protected SimpleGridTable(IGrid grid)
      : base(grid)
    { }

    protected override GridRow CreateRow(Box extent)
    { return new GridRow(this, extent); }
  }
  public abstract class GridTable : ISpatialTable<GridRow>
  {
    private readonly IGrid _grid;
    protected GridTable(IGrid grid)
    {
      _grid = grid;
    }
    public IBox TileExtent { get; set; }

    public IBox Extent { get { return _grid.Extent.Extent.Clone(); } }
    public IGrid Grid { get { return _grid; } }

    IEnumerable<ISpatialRow> ISpatialTable.Search(IBox extent, string constraint)
    {
      foreach (ISpatialRow row in Search(extent))
      { yield return row; }
    }
    protected abstract GridRow CreateRow(Box extent);

    private IEnumerable<GridRow> Search(IBox extent)
    {
      if (TileExtent == null)
      {
        yield return CreateRow(new Box(Point.Create(extent.Min), Point.Create(extent.Max)));
        yield break;
      }
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
          yield return CreateRow(rowExtent);
        }
      }
    }
  }

  public class GridRow<T> : GridRow
  {
    public GridRow(GridTable<T> grid, Box rowExtent)
      : base(grid, rowExtent)
    { }

    public new IGrid<T> Grid
    {
      get { return (IGrid<T>)base.Grid; }
    }

    protected override IGrid CreateOffsetGrid(IGrid grid, int ox, int oy, int nx, int ny)
    {
      return new OffsetGrid<T>((IGrid<T>)grid, ox, oy, nx, ny);
    }

  }
  public class GridRow : ISpatialRow, IDisposable
  {
    private readonly Box _rowExtent;
    private readonly GridTable _table;

    private IGrid _cashed;

    public GridRow(GridTable grid, Box rowExtent)
    {
      _table = grid;
      _rowExtent = rowExtent;
    }
    ITable IRow.Table { get { return _table; } }
    public IBox Extent { get { return _rowExtent; } }

    protected virtual IGrid CreateOffsetGrid(IGrid grid, int ox, int oy, int nx, int ny)
    {
      return new OffsetGrid(grid, ox, oy, nx, ny);
    }
    public IGrid Grid
    {
      get
      {
        if (_cashed == null)
        {
          IExtractable extractable = _table.Grid as IExtractable;
          IGrid cashed;
          if (extractable != null)
          { cashed = extractable.Extract(_rowExtent); }
          else
          {
            int ox;
            int oy;
            GridExtent ext = BaseGrid.GetOffsetExtent(_table.Grid, _rowExtent, out ox, out oy);
            cashed = CreateOffsetGrid(_table.Grid, ox, oy, ext.Nx, ext.Ny);
          }
          _cashed = cashed;
        }
        return _cashed;
      }
    }

    void IDisposable.Dispose()
    {
      if (_cashed != null)
      {
        IDisposable disp = _cashed as IDisposable;
        if (disp != null)
        { disp.Dispose(); }

        _cashed = null;
      }
    }
  }
}
