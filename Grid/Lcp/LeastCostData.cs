using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class LeastCostData : ILeastCostData
  {
    private Steps _steps;
    public LeastCostData(IGrid<double> costGrid, IGrid<int> dirGrid, Steps steps)
    {
      _costGrid = costGrid;
      _dirGrid = dirGrid;
      _steps = steps;
    }

    public IGrid<double> CostGrid => _costGrid;
    private readonly IGrid<double> _costGrid;
    public IGrid<int> DirGrid => _dirGrid;
    private readonly IGrid<int> _dirGrid;
    public Steps Steps => _steps ?? (_steps = Steps.ReverseEngineer(DirGrid, CostGrid));

    double ILeastCostData.GetCost(double x, double y) => CostGrid.Value(x, y);
    Polyline ILeastCostData.GetPath(IPoint end) => GetPath(end);
    public Polyline GetPath(IPoint end, Func<int, int, bool> stop = null)
    {
      LinkedList<Point> gridPath = GetPathPoints(end, stop);
      Polyline path = Polyline.Create(gridPath);
      return path;
    }

    public LinkedList<Point> GetPathPoints(IPoint end, Func<int, int, bool> stop = null)
    {
      DirGrid.Extent.GetNearest(end, out int ix, out int iy);
      LinkedList<Point> gridPath = GetPathPoints(ix, iy, stop);

      DirGrid.Extent.GridToWorld(gridPath);
      return gridPath;
    }

    public LinkedList<Point> GetPathPoints(int endX, int endY, Func<int, int, bool> stopMethod = null)
    {
      int ix = endX;
      int iy = endY;

      LinkedList<Point> path = new LinkedList<Point>();

      int dir = DirGrid[ix, iy];
      double cost = 0;
      if (CostGrid != null) cost = CostGrid[ix, iy];

      Point3D p3 = new Point3D(ix, iy, cost);
      path.AddFirst(p3);
      while (dir > 0 && dir <= Steps.Count)
      {
        if (stopMethod != null && stopMethod(ix, iy))
        { break; }

        dir = dir - 1;
        Step s = Steps.GetStep(dir);
        ix -= s.Dx;
        iy -= s.Dy;

        dir = DirGrid[ix, iy];
        if (CostGrid != null) cost = CostGrid[ix, iy];

        p3 = new Point3D(ix, iy, cost);
        path.AddFirst(p3);
      }
      return path;
    }

    public LeastCostData GetReduced()
    {
      IGrid<double> cost = BaseGrid.TryReduce(CostGrid);
      IGrid<int> dir = BaseGrid.TryReduce(DirGrid);


      CostDirGrid costDirGrid = new CostDirGrid(cost, dir, double.NaN);
      LeastCostData reduced = new LeastCostData(costDirGrid, dir, Steps);
      return reduced;
    }

    /// <summary>
    /// Determines from dir if a cells cost is not defined (dir[ix,iy] == 0) and 
    /// returns the value of the corresponding cell in cost as undefinedValue 
    /// </summary>
    private class CostDirGrid : IGrid<double>
    {
      private readonly IGrid<double> _fullCost;
      private readonly IGrid<int> _dir;
      private readonly double _undefinedValue;
      public CostDirGrid(IGrid<double> fullCost, IGrid<int> dir, double undefinedValue = double.NaN)
      {
        _fullCost = fullCost;
        _dir = dir;
        _undefinedValue = undefinedValue;
      }

      object IGrid.this[int ix, int iy] => this[ix, iy];
      public double this[int ix, int iy]
      {
        get
        {
          if (_dir[ix, iy] == 0)
          { return _undefinedValue; }
          return _fullCost[ix, iy];
        }
      }
      public GridExtent Extent => _fullCost.Extent;
      public Type Type => typeof(double);
    }
  }
}
