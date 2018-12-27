using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grid;
using Basics.Geom;
using TMap;

namespace Dhm
{
  internal class DisplayGrid
  {
    public string Name { get; }
    public IDoubleGrid Grid { get; }

    public DisplayGrid(IDoubleGrid grid, string name)
    {
      Name = name;
      Grid = grid;
    }
  }
  public class SetGridHeight : ITool
  {
    private IContext _context;
    private IDoubleGrid _grid;
    private List<DisplayGrid> _grids;
    private double _width;
    WdgSetGridHeight _wdg;
    public void Execute(IContext context)
    {
      _context = context;
      _grids = GetGrids();
      _grid = _grids[0].Grid;
      _width = 5;

      if (_wdg == null || _wdg.IsDisposed)
      {
        _wdg = new WdgSetGridHeight();
      }
      _wdg.SetData(this);
      if (_context is IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
    }

    internal IList<DisplayGrid> Grids
    {
      get { return _grids; }
    }
    public IDoubleGrid Grid
    {
      get { return _grid; }
      set { _grid = value; }
    }


    public double Breite
    {
      get { return _width; }
      set { _width = value; }
    }

    private List<DisplayGrid> GetGrids()
    {
      List<DisplayGrid> grids = new List<DisplayGrid>();
      foreach (var part in _context.Data.Subparts)
      {
        if (!(part is GridMapData gridMapData))
        { continue; }
        if (!(gridMapData.Data.BaseData is IDoubleGrid grd))
        { continue; }
        grids.Add(new DisplayGrid(grd, gridMapData.Name));
      }
      return grids;
    }
    public void ToolMove(Point pos, int mouse, Point start, Point end)
    {
      string info = GetInfo(pos);
      _wdg.SetInfo(info);
    }

    private string GetInfo(Point pos)
    {
      if (_grid == null)
      { return null; }
      if (!_grid.Extent.IsWithin(pos))
      { return null; }

      double h = _grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);
      string info = string.Format("{0:N1}, {1:N1}, {2:N2}", pos.X, pos.Y, h);
      return info;
    }

    public void ToolEnd(Point start, Point end)
    {
      if (_grid == null)
      { return; }

      Point d = end - start;
      double d2 = Math.Sqrt(d.OrigDist2());
      Point w = _width / 2 / d2 * d;
      w = new Point2D(w.Y, -w.X);
      double w2 = (_width * _width) / 4;
      Polyline line = Polyline.Create(new [] { start + w, start + w + d, start - w + d, start - w, start + w });
      Area area = new Area(line);
      IBox ext = area.Extent;

      _grid.Extent.GetNearest(ext.Min, out int xMin, out int yMax);
      _grid.Extent.GetNearest(ext.Max, out int xMax, out int yMin);

      for (int ix = xMin; ix <= xMax; ix++)
      {
        for (int iy = yMin; iy <= yMax; iy++)
        {
          Point cellCenter = _grid.Extent.CellCenter(ix, iy);
          Point center = cellCenter - start;
          double f0 = center.SkalarProduct(w) / w2;
          if (Math.Abs(f0) > 1)
          { continue; }
          double fl = center.SkalarProduct(d) / (d2 * d2);
          if (fl > 1 || fl < 0)
          { continue; }

          Point ps = start + f0 * w;
          double h0 = _grid.Value(ps.X, ps.Y, EGridInterpolation.bilinear);
          Point pe = end + f0 * w;
          double h1 = _grid.Value(pe.X, pe.Y, EGridInterpolation.bilinear);
          double h = fl * (h1 - h0) + h0;
          _grid[ix, iy] = h;
          //if (area.IsWithin(center))
          //{ }
        }
      }
    }
  }
}
