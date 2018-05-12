using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Grid.Lcp
{
  public delegate void StatusEventHandler(object sender, StatusEventArgs args);

  public class StatusEventArgs
  {
    private IGrid<double> _costGrid;
    private IGrid<int> _dirGrid;

    private int _currentStep;
    private int _totalStep;

    private int _currentX;
    private int _currentY;

    public StatusEventArgs(IGrid<double> costGrid, IGrid<int> dirGrid,
      int ix, int iy, int currentStep, int totalStep)
    {
      _costGrid = costGrid;
      _dirGrid = dirGrid;

      _currentX = ix;
      _currentY = iy;

      _currentStep = currentStep;
      _totalStep = totalStep;
    }
    public IGrid<double> CostGrid => _costGrid;
    public IGrid<int> DirGrid => _dirGrid;

    public int CurrentStep => _currentStep;
    public int TotalStep => _totalStep;

    public int CurrentX => _currentX;
    public int CurrentY => _currentY;

    public bool Cancel { get; set; }
  }

  public partial class LeastCostGrid
  {
    public event StatusEventHandler Status;
    private readonly Steps _steps;

    private readonly int _xMax, _yMax;
    private readonly double _x0, _y0, _dx, _dy;

    public Steps Steps => _steps;
    public IDirCostProvider DirCostProvider { get; }

    public double X0 => _x0;
    public double Y0 => _y0;
    public double Dx => _dx;
    public double Dy => _dy;

    public int XMax => _xMax;
    public int YMax => _yMax;

    public LeastCostGrid(IDirCostProvider dirCostProvider, double dx, IBox box = null, Steps steps = null)
    {
      box = box ?? dirCostProvider.Extent;
      _steps = steps ?? Steps.Step16;
      _x0 = box.Min.X;
      _dx = dx;
      _y0 = box.Max.Y;
      _dy = -dx;

      DirCostProvider = dirCostProvider;

      _xMax = (int)((box.Max.X - box.Min.X) / dx + 1);
      _yMax = (int)((box.Max.Y - box.Min.Y) / dx + 1);
    }

    protected void OnStatus(StatusEventArgs args)
    {
      Status?.Invoke(this, args);
    }

    public GridExtent GetGridExtent()
    {
      GridExtent e = new GridExtent(_xMax, _yMax, _x0, _y0, _dx);
      return e;
    }

    public void CalcCost(IPoint start, IPoint end,
      out IGrid<double> costGrid, out IGrid<int> dirGrid)
    {
      CalcCost(start, new IPoint[] { end },
        out costGrid, out dirGrid);
    }

    public void CalcCost(IPoint start, IList<IPoint> endList,
      out IGrid<double> costGrid, out IGrid<int> dirGrid, bool invers = false, double stopFactor = 1)
    {
      List<int[]> stopList = new List<int[]>(endList.Count);
      foreach (Point end in endList)
      {

        int[] stop = new int[] {
          (int)((end.X - _x0) / _dx),
          (int)((end.Y - _y0) / _dy)
        };
        stopList.Add(stop);
      }

      StopHandler stopHandler = new StopHandler(stopList, DirCostProvider.MinUnitCost * Dx);
      stopHandler.StopFactor = stopFactor;
      CalcCost(start, out costGrid, out dirGrid, invers, calcAdapter: stopHandler);

      // Export to image:
//      IGrid<double> reduced = BaseGrid.TryReduce(costGrid);
//      ImageGrid.GridToTif(DoubleGrid.ToIntGrid(reduced), "C:\\temp\\test2.tif");
    }
    public void CalcCost(IPoint start,
      out IGrid<double> costGrid, out IGrid<int> dirGrid, bool invers = false)
    {
      CalcCost(start, out costGrid, out dirGrid, invers, null);
    }

    public Polyline CalcCost(IPoint start, IPoint end, Polyline nearLine, double corridorWidth,
      out IGrid<double> costGrid, out IGrid<int> dirGrid)
    {
      CalcCorridor(start, end, nearLine, corridorWidth, out costGrid, out dirGrid);
      Polyline path = GetPath(dirGrid, Steps, costGrid, end);
      return path;
    }

    /// <summary>
    /// Determines from dir if a cells cost is not defined (dir[ix,iy] == 0) and 
    /// sets the value of the corresponding cell in cost to undefinedValue 
    /// </summary>
    /// <param name="cost"></param>
    /// <param name="dir"></param>
    /// <param name="undefinedValue"></param>
    public static void SetUndefinedCells(IGrid<double> cost, IGrid<int> dir, 
      double undefinedValue = double.NaN)
    {
      cost.Extent.NN = undefinedValue;
      for (int ix = 0; ix < dir.Extent.Nx; ix++)
      {
        for (int iy = 0; iy < dir.Extent.Ny; iy++)
        {
          if (dir[ix, iy] == 0)
          { cost[ix, iy] = undefinedValue; }
        }
      }
    }

    public bool EqualSettings(LeastCostGrid other)
    {
      if (other == null)
      { return false; }

      if (Steps.Count != other.Steps.Count)
      { return false; }

      if (X0 != other.X0 ||
        Y0 != other.Y0 ||
        XMax != other.XMax ||
        YMax != other.YMax ||
        Dx != other.Dx)
      { return false; }

      return true;
    }

    protected void GetXy(IField position, out double tx, out double ty)
    {
      tx = X0 + position.X * Dx;
      ty = Y0 + position.Y * Dy;
    }
    public static Polyline GetPath(IGrid<int> dirGrid, Steps step, IGrid<double> costGrid, IPoint end)
    {
      Polyline line = GetPath(dirGrid, step, costGrid, end, null);
      return line;
    }
    private static Polyline GetPath(IGrid<int> dirGrid, Steps step, IGrid<double> costGrid, IPoint end,
      Func<int, int, bool> stop)
    {
      dirGrid.Extent.GetNearest(end, out int ix, out int iy);
      Polyline line = GetGridPath(dirGrid, step, costGrid, ix, iy, stop);

      foreach (IPoint point in line.Points)
      {
        int x = (int)point.X;
        int y = (int)point.Y;
        Point p = dirGrid.Extent.CellLL(x, y);
        point.X = p.X;
        point.Y = p.Y;
      }
      return line;
    }

    private static Polyline GetGridPath(IGrid<int> dirGrid, Steps step, IGrid<double> costGrid,
      int endX, int endY, Func<int, int, bool> stopMethod)
    {
      int ix = endX;
      int iy = endY;

      Polyline path = new Polyline();

      int dir = dirGrid[ix, iy];
      double cost = 0;
      if (costGrid != null) cost = costGrid[ix, iy];

      Point3D p3 = new Point3D(ix, iy, cost);
      path.AddFirst(p3);
      while (dir > 0 && dir <= step.Count)
      {
        if (stopMethod != null && stopMethod(ix, iy))
        { break; }

        dir = dir - 1;
        Step s = step.GetStep(dir);
        ix -= s.Dx;
        iy -= s.Dy;

        dir = dirGrid[ix, iy];
        if (costGrid != null) cost = costGrid[ix, iy];

        p3 = new Point3D(ix, iy, cost);
        path.AddFirst(p3);
      }
      return path;
    }

    private class RouteInfo
    {
      public double Cost;
      public int X;
      public int Y;

      public RouteInfo(double cost, int x, int y)
      {
        Cost = cost;
        X = x;
        Y = y;
      }
      public static int CompareCost(RouteInfo x, RouteInfo y)
      {
        return x.Cost.CompareTo(y.Cost);
      }
    }

    private class PathStop
    {
      private readonly IGrid<int> _dirGrid;
      private readonly IDoubleGrid _stopGrid;
      private readonly Func<int, int, bool> _stop;

      private readonly int _dx;
      private readonly int _dy;

      public PathStop(IGrid<int> dirGrid, IDoubleGrid stopGrid)
      {
        _dirGrid = dirGrid;
        _stopGrid = stopGrid;

        if (dirGrid.Extent.EqualExtent(stopGrid.Extent))
        {
          _stop = EqualExtent;
        }
        else if (dirGrid.Extent.Dx == stopGrid.Extent.Dx)
        {
          _dx = (int)Math.Round((dirGrid.Extent.X0 - stopGrid.Extent.X0) / dirGrid.Extent.Dx);
          _dy = (int)Math.Round((dirGrid.Extent.Y0 - stopGrid.Extent.Y0) / dirGrid.Extent.Dx);
          _stop = EqualCell;
        }
        else
        {
          _stop = General;
        }
      }

      private bool General(int ix, int iy)
      {
        IPoint p = _dirGrid.Extent.CellLL(ix, iy);
        _stopGrid.Extent.GetNearest(p, out int x, out int y);
        return EqualExtent(x, y);
      }

      private bool EqualExtent(int ix, int iy)
      {
        return _stopGrid[ix, iy] > 0;
      }
      private bool EqualCell(int ix, int iy)
      {
        return _stopGrid[ix + _dx, iy - _dy] > 0;
      }

      public bool GetStop(int ix, int iy)
      {
        return _stop(ix, iy);
      }
    }
    public static RouteTable CalcBestRoutes(IGrid<double> sum,
      IGrid<double> fromCost, IGrid<int> fromDir, Steps fromStep,
      IGrid<double> toCost, IGrid<int> toDir, Steps toStep,
      double maxLengthFactor, double minDiffFactor, StatusEventHandler Status)
    {
      if (fromStep == null)
      { fromStep = Steps.ReverseEngineer(fromDir, fromCost); }
      if (toStep == null)
      { toStep = Steps.ReverseEngineer(toDir, toCost); }

      GridExtent e = sum.Extent;
      DataDoubleGrid routeGrid = new DataDoubleGrid(e.Nx, e.Ny, typeof(double), e.X0, e.Y0, e.Dx);
      IntGrid closeGrid = new IntGrid(e.Nx, e.Ny, typeof(byte), e.X0, e.Y0, e.Dx);

      double rClose = -1;

      double sumMin = DoubleGrid.Min(sum);
      double vMax = sumMin * maxLengthFactor;
      List<RouteInfo> cands = new List<RouteInfo>(e.Nx * 5);
      for (int iX = 1; iX < e.Nx - 1; iX++)
      {
        for (int iY = 1; iY < e.Ny - 1; iY++)
        {
          double v00 = sum[iX, iY];
          if (v00 > vMax) continue;

          double v_10 = sum[iX - 1, iY];
          double v10 = sum[iX + 1, iY];
          double v0_1 = sum[iX, iY - 1];
          double v01 = sum[iX, iY + 1];

          if ((v00 <= v_10 && v00 <= v10) &&
            (v00 <= v0_1 && v00 <= v01))
          {
            cands.Add(new RouteInfo(v00, iX, iY));
          }
        }
      }
      cands.Sort(RouteInfo.CompareCost);
      RouteTable bestRoutes = new RouteTable();
      int iCand = 0;
      int nCands = cands.Count;
      foreach (RouteInfo cand in cands)
      {
        if (Status != null)
        {
          StatusEventArgs args = new StatusEventArgs(null, null, cand.X, cand.Y, iCand, nCands);
          Status(null, args);
          iCand++;
        }

        IPoint candPrj = routeGrid.Extent.CellLL(cand.X, cand.Y);
        PathStop fromStop = new PathStop(fromDir, routeGrid);
        Polyline fromPath = GetPath(fromDir, fromStep, fromCost, candPrj, fromStop.GetStop);
        MakeGridPath(routeGrid.Extent, fromPath);

        PathStop toStop = new PathStop(toDir, routeGrid);
        Polyline toPath = GetPath(toDir, toStep, toCost, candPrj, toStop.GetStop);
        MakeGridPath(routeGrid.Extent, toPath);

        if (fromPath.Points.Count <= 1 && toPath.Points.Count <= 1)
        { continue; }

        IPoint fromLast = fromPath.Points.Last.Value;
        IPoint toLast = toPath.Points.Last.Value;
        IPoint fromFirst = fromPath.Points.First.Value;
        IPoint toFirst = toPath.Points.First.Value;

        double newCost = fromLast.Z + toLast.Z - (fromFirst.Z + toFirst.Z);

        double fromC = routeGrid[(int)fromFirst.X, (int)fromFirst.Y];
        double toC = routeGrid[(int)toFirst.X, (int)toFirst.Y];
        double optCost = toC - fromC;
        double f = 1;
        if (optCost != 0)
        {
          f = newCost / optCost;
        }
        if (f < maxLengthFactor && f > 0)
        {
          if (IsBlocked(fromPath, closeGrid) && IsBlocked(toPath, closeGrid))
          { continue; }

          // because of numerical differences between from- and to-grid :
          // synchronize with existing routes
          double dz = fromFirst.Z - routeGrid[(int)fromFirst.X, (int)fromFirst.Y];
          foreach (IPoint point in fromPath.Points)
          { point.Z = point.Z - dz; }

          double toLastZ = toLast.Z;
          foreach (IPoint point in toPath.Points)
          { point.Z = fromLast.Z + toLastZ - point.Z; }

          Polyline inv = toPath.Invert();
          LinkedListNode<IPoint> next = inv.Points.First.Next;
          while (next != null)
          {
            fromPath.Add(next.Value);
            next = next.Next;
          }

          Assign(fromPath, routeGrid, OrigValue);
          //Assign(to, routeGrid, delegate(double value) { return fromLast.Z + toLast.Z - value; });
          bestRoutes.AddRow(fromPath, cand.Cost, newCost - optCost);

          if (rClose < 0)
          {
            double dx = toFirst.X - fromFirst.X;
            double dy = toFirst.Y - fromFirst.Y;
            rClose = minDiffFactor * Math.Sqrt(dx * dx + dy * dy);
          }
          BlockGridLine(fromPath, rClose, closeGrid);
          // Block(to, rClose, closeGrid);
        }
      }

      return bestRoutes;
    }

    private static void MakeGridPath(GridExtent extent, Polyline path)
    {
      foreach (IPoint p in path.Points)
      {
        extent.GetNearest(p, out int ix, out int iy);
        p.X = ix;
        p.Y = iy;
      }
    }

    private static double OrigValue(double value)
    {
      return value;
    }

    protected static void BlockLine(Polyline line, double rClose, IntGrid closeGrid)
    {
      double gridSize = closeGrid.Extent.Dx;
      double r2;
      int ir;
      {
        double r = rClose / gridSize;
        ir = (int)r;
        r2 = r * r;
      }
      if (ir == 0)
      { return; }

      int x0 = -1;
      int y0 = -1;
      bool first = true;
      foreach (IPoint p in line.Points)
      {
        closeGrid.Extent.GetNearest(p, out int x1, out int y1);

        if (!first)
        { BlockSegment(closeGrid, x0, x1, y0, y1, r2, ir); }
        first = false;

        BlockPoint(x1, y1, closeGrid, ir, r2);

        x0 = x1;
        y0 = y1;
      }
    }

    private static void BlockGridLine(Polyline line, double rClose, IntGrid closeGrid)
    {
      double r2 = rClose * rClose;
      int ir = (int)rClose;
      if (ir == 0)
      { return; }

      int x0 = -1;
      int y0 = -1;
      bool first = true;
      foreach (IPoint p in line.Points)
      {
        int x1 = (int)p.X;
        int y1 = (int)p.Y;

        if (!first)
        { BlockSegment(closeGrid, x0, x1, y0, y1, r2, ir); }
        first = false;

        BlockPoint(x1, y1, closeGrid, ir, r2);

        x0 = x1;
        y0 = y1;
      }
    }

    private static void BlockSegment(IntGrid closeGrid, int x0, int x1,
      int y0, int y1, double r2, int ir)
    {
      int dx = x1 - x0;
      int dy = y1 - y0;

      int i0;
      int i1;
      int d;
      if (Math.Abs(dx) > Math.Abs(dy))
      {
        d = Math.Sign(dx);
        i0 = x0;
        i1 = x1;
      }
      else
      {
        d = Math.Sign(dy);
        i0 = y0;
        i1 = y1;
      }
      if (d != 0)
      {
        double di = i1 - i0;
        for (int i = i0 + d; i != i1; i += d)
        {
          double f = (i - i0) / di;
          double x = x0 + f * dx;
          double y = y0 + f * dy;
          BlockPoint((int)x, (int)y, closeGrid, ir, r2);
        }
      }
    }

    private static void BlockPoint(int x1, int y1, IntGrid closeGrid, int ir, double r2)
    {
      for (int ix = x1 - ir; ix <= x1 + ir; ix++)
      {
        if (ix < 0 || ix >= closeGrid.Extent.Nx)
        { continue; }

        int dx = ix - x1;
        for (int iy = y1 - ir; iy <= y1 + ir; iy++)
        {
          if (iy < 0 || iy >= closeGrid.Extent.Ny)
          { continue; }

          int dy = iy - y1;

          if (dx * dx + dy * dy > r2)
          { continue; }

          closeGrid[ix, iy] = 1;
        }
      }
    }

    private static bool IsBlocked(Polyline line, IntGrid closeGrid)
    {
      foreach (IPoint p in line.Points)
      {
        int x1 = (int)p.X;
        int y1 = (int)p.Y;

        if (closeGrid[x1, y1] == 0)
        { return false; }
      }
      return true;
    }

    public static void Assign(Polyline line, DataDoubleGrid routeGrid, Func<double, double> valueFct)
    {
      if (line.Points.Count == 0)
      {
        return;
      }
      IPoint first = line.Points.First.Value;
      int x1 = (int)first.X;
      int y1 = (int)first.Y;
      double z1 = valueFct(first.Z);

      foreach (IPoint p in line.Points)
      {
        int x0 = x1;
        int y0 = y1;
        double z0 = z1;
        x1 = (int)p.X;
        y1 = (int)p.Y;
        z1 = valueFct(p.Z);
        double zOrig = routeGrid[x1, y1];
        if (zOrig == 0)
        { routeGrid[x1, y1] = z1; }
        int dx = x1 - x0;
        int dy = y1 - y0;

        if (Math.Abs(dx) > 1)
        {
          int d = Math.Abs(dx) / dx;
          for (int x = x0 + d; x != x1; x += d)
          {
            double y = y0 + dy * (double)(x - x0) / dx;
            int yc = (int)Math.Ceiling(y);
            PutValue(x, yc, routeGrid, x0, y0, x1, y1, z0, z1);
            int yf = (int)Math.Floor(y);
            PutValue(x, yf, routeGrid, x0, y0, x1, y1, z0, z1);
          }
        }

        if (Math.Abs(dy) > 1)
        {
          int d = Math.Abs(dy) / dy;
          for (int y = y0 + d; y != y1; y += d)
          {
            double x = x0 + dx * (double)(y - y0) / dy;
            int xc = (int)Math.Ceiling(x);
            PutValue(xc, y, routeGrid, x0, y0, x1, y1, z0, z1);
            int xf = (int)Math.Floor(x);
            PutValue(xf, y, routeGrid, x0, y0, x1, y1, z0, z1);
          }
        }

      }
    }

    private static void PutValue(int x, int y, DataDoubleGrid grid, int x0, int y0, int x1, int y1, double z0, double z1)
    {
      if (grid[x, y] > 0)
      { return; }
      double l0;
      {
        int dx = x - x0;
        int dy = y - y0;
        l0 = Math.Sqrt(dx * dx + dy * dy);
      }
      double l;
      {
        int dx = x0 - x1;
        int dy = y0 - y1;
        l = Math.Sqrt(dx * dx + dy * dy);
      }
      grid[x, y] = z0 + (z1 - z0) * l0 / l;
    }

    private class CorridorGrid : BaseGrid<int>
    {
      private readonly IntGrid _blockGrid;
      public CorridorGrid(LeastCostGrid parent, Polyline nearLine, double corridorWidth)
        : this(nearLine, corridorWidth, parent.GetGridExtent())
      { }
      public CorridorGrid(Polyline nearLine, double corridorWidth, GridExtent extent)
        : base(extent)
      {
        _blockGrid = new IntGrid(extent.Nx, extent.Ny, typeof(byte),
          extent.X0, extent.Y0, extent.Dx);
        BlockLine(nearLine, corridorWidth, _blockGrid);
      }

      public override int this[int ix, int iy]
      {
        get
        {
          if (_blockGrid[ix, iy] == 0)
          { return 0; }

          return 1;
        }
        set { }
      }
    }

    protected virtual double CalcCost(ICostField startField, Step step, int iField,
      ICostField[] neighbors, bool invers)
    {
      List<double> xs = new List<double>();
      List<double> ys = new List<double>();

      foreach (int iNeighbor in Steps.GetInvolveds(step))
      {
        ICostField involved = iNeighbor < 0 ? startField : neighbors[iNeighbor];
        if (involved == null)
        { return double.MaxValue; }
        GetXy(involved , out double tx, out double ty);
        xs.Add(tx);
        ys.Add(ty);
      }


      return DirCostProvider.GetCost(xs, ys, Steps.GetWs(step), Dx, step.Distance * Dx, invers);
    }

    private class CorridorLcp : LeastCostGrid
    {
      private readonly LeastCostGrid _baseLcp;
      private readonly BaseGrid<int> _corridorGrid;
      public CorridorLcp(LeastCostGrid baseLcp, BaseGrid<int> corridorGrid)
        : base(baseLcp.DirCostProvider, baseLcp.Dx, baseLcp.GetGridExtent().Extent, baseLcp.Steps)
      {
        _baseLcp = baseLcp;
        _corridorGrid = corridorGrid;
      }

      protected override double CalcCost(ICostField startField, Step step, int iField, ICostField[] neighbors, bool invers)
      {
        ICostField nb = neighbors[iField];
        if (_corridorGrid[nb.X, nb.Y] == 0) { return 0; }

        return _baseLcp.CalcCost(startField, step, iField, neighbors, invers);
      }

      protected override ICostField InitField(IField position)
      {
        return _baseLcp.InitField(position);
      }
    }

    protected virtual ICostField InitField(IField field)
    {
      return field as RestCostField ?? new RestCostField(field);
    }

    protected void CalcCorridor(IPoint start, IPoint end,
      Polyline nearLine, double corridorWidth, out IGrid<double> costGrid, out IGrid<int> dirGrid)
    {
      BaseGrid<int> corridorGrid = new CorridorGrid(this, nearLine, corridorWidth);
      CorridorLcp corridorLcp = new CorridorLcp(this, corridorGrid);

      corridorLcp.CalcCost(start, new IPoint[] { end }, out costGrid, out dirGrid);
    }

    protected virtual void CalcStarting()
    { }
    protected virtual void CalcEnded()
    { }

    protected void CalcCost(IPoint start,
      out IGrid<double> costGrid, out IGrid<int> dirGrid, bool invers = false, ICostOptimizer calcAdapter = null)
    {
      try
      {
        CalcStarting();
        Calc calc = new Calc(this, start, invers, calcAdapter);
        calc.Process();

        costGrid = calc.CostGrid;
        dirGrid = calc.DirGrid;
      }
      finally
      {
        CalcEnded();
      }
    }

    protected virtual bool UpdateCost(ICostField field)
    {
      return false;
    }

    protected bool EqualSettingsCore(LeastCostGrid o)
    {
      LeastCostGrid other = o as LeastCostGrid;
      if (other == null)
      { return false; }

      if (Steps.Count != other.Steps.Count)
      { return false; }

      if (X0 != other.X0 ||
        Y0 != other.Y0 ||
        XMax != other.XMax ||
        YMax != other.YMax ||
        Dx != other.Dx)
      { return false; }

      return true;
    }

    private class Calc
    {
      private readonly LeastCostGrid _parent;
      private readonly IPoint _start;
      private readonly bool _invers;
      private readonly ICostOptimizer _costOptimizer;

      private readonly SortedList<ICostField, ICostField> _costList;
      private readonly Dictionary<IField, ICostField> _fieldList;

      private readonly IGrid<double> _costGrid;
      private readonly IGrid<int> _dirGrid;
      private readonly ICostField _startField;

      public Calc(LeastCostGrid parent, IPoint start, bool invers = false, ICostOptimizer costOptimizer = null)
      {
        _parent = parent;
        _start = start;
        _invers = invers;
        _costOptimizer = costOptimizer;

        LeastCostGrid p = _parent;

        _costGrid = new TiledDoubleGrid(p.XMax, p.YMax, typeof(float), p.X0, p.Y0, p.Dx);

        _dirGrid = new TiledIntGrid(p.XMax, p.YMax, typeof(sbyte), p.X0, p.Y0, p.Dx);

        _startField = _parent.InitField(new Field { X = (int)((start.X - p.X0) / p.Dx), Y = (int)((start.Y - p.Y0) / p.Dy) });
        _startField.SetCost(null, 0, idDir: -2);

        _costOptimizer?.Init(_startField);

        FieldComparer fieldComparer = new FieldComparer();
        _costList = new SortedList<ICostField, ICostField>(_costOptimizer?.GetCostComparer() ?? new CostComparer());
        _fieldList = new Dictionary<IField, ICostField>(fieldComparer);
      }

      public IGrid<double> CostGrid => _costGrid;
      public IGrid<int> DirGrid => _dirGrid;

      public void Process()
      {
        int i = 0;
        int n = _parent.XMax * _parent.YMax;
        StatusEventArgs args;

        ICostField processField = _startField;
        while (processField != null)
        {
          _costGrid[processField.X, processField.Y] = processField.Cost;
          _dirGrid[processField.X, processField.Y] = processField.IdDir + 1;
          args = new StatusEventArgs(_costGrid, _dirGrid, processField.X, processField.Y, i, n);
          _parent.OnStatus(args);
          if (args.Cancel || (_costOptimizer?.Stop(processField, _costList) ?? false))
          {
            _costList.Clear();
            _fieldList.Clear();
            return;
          }

          i++;
          GotoNeighbours(processField, _invers);

          if (_costList.Count > 0)
          {
            bool updated = true;
            while (updated)
            {
              processField = _costList.Values[0];
              _costList.RemoveAt(0);
              updated = _parent.UpdateCost(processField);
              if (updated)
              {
                _costList.Add(processField, processField);
              }
            }
            _fieldList.Remove(processField);
          }
          else
          {
            processField = null;
          }
        }
        _parent.OnStatus(new StatusEventArgs(_costGrid, _dirGrid, -1, -1, n, n));
      }

      private int GotoNeighbours(ICostField startField, bool invers)
      {
        _parent.InitField(startField);

        bool[] cancelFields = null;
        IReadOnlyList<Step> distSteps = _parent.Steps.GetDistSteps();
        int iStep = 0;
        ICostField[] fields = new ICostField[distSteps.Count];
        foreach (Step distStep in distSteps)
        {
          iStep++;
          Field key = new Field
          { X = startField.X + distStep.Dx, Y = startField.Y + distStep.Dy };

          // check range
          if (key.X < 0 || key.X >= _parent.XMax ||
              key.Y < 0 || key.Y >= _parent.YMax)
          {
            continue;
          }
          // check set
          if (_dirGrid[key.X, key.Y] != 0)
          {
            continue;
          }

          if (_fieldList.TryGetValue(key, out ICostField stepField) == false)
          {
            stepField = _parent.InitField(key);
            _costOptimizer?.AdaptCost(stepField);
            _fieldList.Add(stepField, stepField);
          }
          fields[distStep.Index] = stepField;
        }
        for (int iDistStep = 0; iDistStep < _parent.Steps.Count; iDistStep++)
        {
          Step distStep = distSteps[iDistStep];
          int iField = distStep.Index;
          ICostField stepField = fields[iField];
          if (stepField == null)
          { continue; }

          double cost = _parent.CalcCost(startField, distStep, iField, fields, invers);
          {
            if (cost < 0)
            {
              cancelFields = _parent.Steps.GetCancelIndexes(distStep, cancelFields);
              cost = Math.Abs(cost);
            }
            if (cancelFields != null && cancelFields[distStep.Index])
            { continue; }
          }
          if (stepField.IdDir < 0 || stepField.Cost > startField.Cost + cost)
          {
            // new minimum path found
            if (stepField.IdDir >= 0)
            {
              // remove current position
              _costList.Remove(stepField);
            }
            // add at new position
            stepField.SetCost(startField, cost, distStep.Index);
            _costList.Add(stepField, stepField);
          }
        }

        return 0;
      }
    }
  }

  public class LeastCostGrid<T> : LeastCostGrid
    where T: class
  {
    public LeastCostGrid(IDirCostProvider<T> dirCostProvider, double dx, IBox box = null, Steps steps = null) :
      base(dirCostProvider, dx, box, steps)
    { }

    public new IDirCostProvider<T> DirCostProvider => (IDirCostProvider<T>)base.DirCostProvider;

    protected override ICostField InitField(IField position)
    {
      RestCostField<T> init = position as RestCostField<T>;
      if (init == null)
      {
        GetXy(position, out double tx, out double ty);
        T cellInfo = DirCostProvider.InitCell(tx, ty, Dx);
        init = new RestCostField<T>(position, cellInfo);
      }
      return init;
    }

    protected override double CalcCost(ICostField startField, Step step, int iField,
      ICostField[] neighbors, bool invers)
    {
      Steps.GetPointInfos(startField, iField, neighbors, out IList<T> pointInfos, out IList<double> ws);
      if (pointInfos != null)
      {
        if (pointInfos.Count == 1)
        { return double.MaxValue; }
        return DirCostProvider.GetCost(pointInfos, ws, step.Distance * Dx, invers);
      }
      else
      {
        return base.CalcCost(startField,step, iField, neighbors, invers);
      }
    }
  }
}
