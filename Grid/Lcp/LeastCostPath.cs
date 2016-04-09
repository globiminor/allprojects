using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Grid.Lcp
{
  public class StatusEventArgs
  {
    private DoubleGrid _costGrid;
    private IntGrid _dirGrid;

    private int _currentStep;
    private int _totalStep;

    private int _currentX;
    private int _currentY;

    private bool _cancel;

    public StatusEventArgs(DoubleGrid costGrid, IntGrid dirGrid,
      int ix, int iy, int currentStep, int totalStep)
    {
      _costGrid = costGrid;
      _dirGrid = dirGrid;

      _currentX = ix;
      _currentY = iy;

      _currentStep = currentStep;
      _totalStep = totalStep;
    }
    public DoubleGrid CostGrid
    { get { return _costGrid; } }
    public IntGrid DirGrid
    { get { return _dirGrid; } }

    public int CurrentStep
    { get { return _currentStep; } }
    public int TotalStep
    { get { return _totalStep; } }

    public int CurrentX
    { get { return _currentX; } }
    public int CurrentY
    { get { return _currentY; } }

    public bool Cancel
    {
      get { return _cancel; }
      set { _cancel = value; }
    }
  }

  /// <summary>
  /// Calculates cost for a step
  /// </summary>
  /// <param name="h0">Height at start point of step</param>
  /// <param name="v0">Velocity at start point of step</param>
  /// <param name="slope0_2">Square of slope at start point</param>
  /// <param name="h1">Height at end point of step</param>
  /// <param name="v1">Velocity at end point of step</param>
  /// <param name="slope1_2">Square of slope at end point</param>
  /// <param name="dist">Distance between start and end point of step</param>
  /// <returns>Cost for the step</returns>
  public delegate double StepCostHandler<T>(
    double h0, T v0, double slope0_2,
    double h1, T v1, double slope1_2,
    double dist);
  public delegate void StatusEventHandler(object sender, StatusEventArgs args);

  public class LeastCostPath : LeastCostPathBase<double>
  {
    public LeastCostPath(IBox box, double dx)
      : base(box, dx, Steps.Step16)
    { }
    public LeastCostPath(IBox box, double dx, Steps step)
      : base(box, dx, step)
    { }
  }

  public abstract class LeastCostPathBase
  {
    public event StatusEventHandler Status;
    protected Steps _step;

    protected int _xMax, _yMax;
    protected double _x0, _y0, _dx, _dy;
    private IDoubleGrid _heightGrid;
    private BaseGrid _veloBase;

    public Steps Step
    { get { return _step; } }

    public IDoubleGrid HeightGrid
    {
      get { return _heightGrid; }
      set { _heightGrid = value; }
    }

    protected BaseGrid VeloBase
    {
      get { return _veloBase; }
      set { _veloBase = value; }
    }

    protected LeastCostPathBase(IBox box, double dx)
      : this(box, dx, Steps.Step16)
    { }
    public LeastCostPathBase(IBox box, double dx, Steps step)
    {
      _step = step;
      _x0 = box.Min.X;
      _dx = dx;
      _y0 = box.Max.Y;
      _dy = -dx;

      _xMax = (int)((box.Max.X - box.Min.X) / dx + 1);
      _yMax = (int)((box.Max.Y - box.Min.Y) / dx + 1);
    }

    protected void OnStatus(StatusEventArgs args)
    {
      if (Status != null)
      {
        Status(this, args);
      }
    }

    public GridExtent GetGridExtent()
    {
      GridExtent e = new GridExtent(_xMax, _yMax, _x0, _y0, _dx);
      return e;
    }

    public void CalcCost(IPoint start,
      out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      CalcCost(start, out costGrid, out dirGrid, false);
    }
    public void CalcCost(IPoint start, IPoint end,
      out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      CalcCost(start, new IPoint[] { end },
        out costGrid, out dirGrid);
    }

    public void CalcCost(IPoint start, IList<IPoint> endList,
      out DoubleGrid costGrid, out IntGrid dirGrid)
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

      CalcCost(start, out costGrid, out dirGrid, false, stopList);
    }
    public void CalcCost(IPoint start,
      out DoubleGrid costGrid, out IntGrid dirGrid, bool invers)
    {
      CalcCost(start, out costGrid, out dirGrid, invers, null);
    }

    public Polyline CalcCost(IPoint start, IPoint end, Polyline nearLine, double corridorWidth,
      out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      CalcCorridor(start, end, nearLine, corridorWidth, out costGrid, out dirGrid);
      Polyline path = GetPath(dirGrid, Step, costGrid, end);
      return path;
    }

    protected abstract void CalcCorridor(IPoint start, IPoint end,
      Polyline nearLine, double corridorWidth, out DoubleGrid costGrid, out IntGrid dirGrid);

    protected abstract void CalcCost(IPoint start,
      out DoubleGrid costGrid, out IntGrid dirGrid, bool invers,
      IList<int[]> stop);

    public bool EqualSettings(LeastCostPathBase other)
    {
      bool equals = EqualSettingsCore(other);
      return equals;
    }
    protected abstract bool EqualSettingsCore(LeastCostPathBase o);

    private delegate bool StopMethod(int ix, int iy);

    public static Polyline GetPath(IntGrid dirGrid, Steps step, DoubleGrid costGrid, IPoint end)
    {
      Polyline line = GetPath(dirGrid, step, costGrid, end, null);
      return line;
    }
    private static Polyline GetPath(IntGrid dirGrid, Steps step, DoubleGrid costGrid, IPoint end,
      StopMethod stop)
    {
      int ix;
      int iy;
      dirGrid.Extent.GetNearest(end, out ix, out iy);
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

    private static Polyline GetGridPath(IntGrid dirGrid, Steps step, DoubleGrid costGrid,
      int endX, int endY, StopMethod stopMethod)
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
      private readonly IntGrid _dirGrid;
      private readonly DoubleGrid _stopGrid;
      private readonly StopMethod _stop;

      private readonly int _dx;
      private readonly int _dy;

      public PathStop(IntGrid dirGrid, DoubleGrid stopGrid)
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
        int x, y;
        _stopGrid.Extent.GetNearest(p, out x, out y);
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
    public static RouteTable CalcBestRoutes(DoubleGrid sum,
      DoubleGrid fromCost, IntGrid fromDir, Steps fromStep,
      DoubleGrid toCost, IntGrid toDir, Steps toStep,
      double maxLengthFactor, double minDiffFactor, StatusEventHandler Status)
    {
      if (fromStep == null)
      { fromStep = Steps.ReverseEngineer(fromDir, fromCost); }
      if (toStep == null)
      { toStep = Steps.ReverseEngineer(toDir, toCost); }

      GridExtent e = sum.Extent;
      DoubleGrid routeGrid = new DoubleGrid(e.Nx, e.Ny, typeof(double), e.X0, e.Y0, e.Dx);
      IntGrid closeGrid = new IntGrid(e.Nx, e.Ny, typeof(byte), e.X0, e.Y0, e.Dx);

      double rClose = -1;

      double sumMin = sum.Min();
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
        int ix, iy;
        extent.GetNearest(p, out ix, out iy);
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
        int x1;
        int y1;
        closeGrid.Extent.GetNearest(p, out x1, out y1);

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

    public delegate double DoubleFct(double value);
    public static void Assign(Polyline line, DoubleGrid routeGrid, DoubleFct valueFct)
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

    private static void PutValue(int x, int y, DoubleGrid grid, int x0, int y0, int x1, int y1, double z0, double z1)
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
  }

  public class LeastCostPathBase<T> : LeastCostPathBase
  {
    private class CorridorGrid : BaseGrid<T>, ILockable
    {
      private readonly BaseGrid<T> _velo;
      private readonly IntGrid _blockGrid;
      public CorridorGrid(LeastCostPathBase<T> parent, BaseGrid<T> velocityGrid, Polyline nearLine, double corridorWidth)
        : this(velocityGrid, nearLine, corridorWidth, parent.GetGridExtent())
      { }
      public CorridorGrid(BaseGrid<T> velocityGrid, Polyline nearLine, double corridorWidth, GridExtent extent)
        : base(extent)
      {
        _velo = velocityGrid;
        _blockGrid = new IntGrid(extent.Nx, extent.Ny, typeof(byte),
          extent.X0, extent.Y0, extent.Dx);
        BlockLine(nearLine, corridorWidth, _blockGrid);
      }

      public override T this[int ix, int iy]
      {
        get
        {
          if (_blockGrid[ix, iy] == 0)
          { return default(T); }

          IPoint center = _blockGrid.Extent.CellCenter(ix, iy);
          return _velo.Value(center.X, center.Y);
        }
        set { }
      }

      public void LockBits()
      {
        ILockable velo = _velo as ILockable;
        if (velo != null)
        { velo.LockBits(); }
      }

      public void UnlockBits()
      {
        ILockable velo = _velo as ILockable;
        if (velo != null)
        { velo.UnlockBits(); }
      }
    }

    public class Field
    {
      public int X, Y;
      public int IdDir;
      public double Cost;
      // performance :
      public double Height;
      public T Velocity;
      public double Slope2 = -1;

      #region IComparable Members

      public int CompareTo(Field field)
      {
        return 0;
      }

      #endregion
    }

    public class FieldCompare : IComparer<Field>
    {
      public int Compare(Field field0, Field field1)
      { return SCompare(field0, field1); }
      public static int SCompare(Field field0, Field field1)
      {
        if (field0.X < field1.X) return -1;
        else if (field0.X > field1.X) return 1;
        else return field0.Y - field1.Y;
      }
    }

    public class CostCompare : IComparer<Field>
    {
      #region IComparer Members

      public int Compare(Field field0, Field field1)
      {
        if (field0.Cost < field1.Cost) { return -1; }
        else if (field0.Cost > field1.Cost) { return 1; }
        else return FieldCompare.SCompare(field0, field1);
      }

      #endregion
    }

    public StepCostHandler<T> StepCost;

    private IntGrid _dirGrid;
    private SortedList<Field, object> _costList;
    private SortedList<Field, Field> _fieldList;

    public BaseGrid<T> VelocityGrid
    {
      get { return (BaseGrid<T>)VeloBase; }
      set { VeloBase = value; }
    }

    private int GotoNeighbours(Field startField, bool invers)
    {
      double startSlope2;
      Field newField;
      double tx, ty;

      tx = _x0 + startField.X * _dx;
      ty = _y0 + startField.Y * _dy;
      if (startField.Slope2 < 0) startField.Slope2 = HeightGrid.Slope2(tx, ty);
      startSlope2 = startField.Slope2;

      bool[] cancelFields = null;
      newField = new Field();
      foreach (Step s in _step.GetDistSteps())
      {
        double t;
        newField.X = startField.X + s.Dx;
        newField.Y = startField.Y + s.Dy;

        // check range
        if (newField.X < 0 || newField.X >= _xMax ||
            newField.Y < 0 || newField.Y >= _yMax)
        {
          continue;
        }
        // check set
        if (_dirGrid[newField.X, newField.Y] != 0)
        {
          continue;
        }

        Field stepField;
        if (_fieldList.TryGetValue(newField, out stepField) == false)
        {
          _fieldList.Add(newField, newField);
          stepField = newField;

          tx = _x0 + stepField.X * _dx;
          ty = _y0 + stepField.Y * _dy;
          stepField.Height = HeightGrid.Value(tx, ty, EGridInterpolation.bilinear);
          stepField.Velocity = VelocityGrid.Value(tx, ty);
          stepField.Slope2 = HeightGrid.Slope2(tx, ty);
          stepField.IdDir = -1;

          newField = new Field();
        }
        {
          double h0, h1, s0, s1;
          T v0, v1;
          if (invers)
          {
            h1 = startField.Height;
            v1 = startField.Velocity;
            s1 = startSlope2;
            h0 = stepField.Height;
            v0 = stepField.Velocity;
            s0 = stepField.Slope2;
          }
          else
          {
            h0 = startField.Height;
            v0 = startField.Velocity;
            s0 = startSlope2;
            h1 = stepField.Height;
            v1 = stepField.Velocity;
            s1 = stepField.Slope2;
          }

          t = StepCost(h0, v0, s0, h1, v1, s1, s.Distance * _dx);
          if (t < 0)
          {
            cancelFields = _step.GetCancelIndexes(s, cancelFields);
            t = Math.Abs(t);
          }
          if (cancelFields != null && cancelFields[s.Index])
          { continue; }
        }
        if (stepField.IdDir < 0 || stepField.Cost > startField.Cost + t)
        {
          // new minimum path found
          if (stepField.IdDir >= 0)
          {
            // remove current position
            _costList.Remove(stepField);
          }
          // add at new position
          stepField.Cost = startField.Cost + t;
          stepField.IdDir = s.Index;
          _costList.Add(stepField, stepField);
        }
      }

      return 0;
    }

    public LeastCostPathBase(IBox box, double dx)
      : this(box, dx, Steps.Step16) { }
    public LeastCostPathBase(IBox box, double dx, Steps step)
      : base(box, dx, step)
    {
      _step = step;
      _x0 = box.Min.X;
      _dx = dx;
      _y0 = box.Max.Y;
      _dy = -dx;

      _xMax = (int)((box.Max.X - box.Min.X) / dx + 1);
      _yMax = (int)((box.Max.Y - box.Min.Y) / dx + 1);
    }

    public void CalcCost(IPoint start,
      IDoubleGrid heightGrid, BaseGrid<T> velocityGrid,
      out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      CalcCost(start, heightGrid, velocityGrid, out costGrid, out dirGrid, false);
    }

    public void CalcCost(IPoint start, IPoint end,
      IDoubleGrid heightGrid, BaseGrid<T> velocityGrid,
      out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      CalcCost(start, new IPoint[] { end }, heightGrid, velocityGrid,
        out costGrid, out dirGrid);
    }

    public void CalcCost(IPoint start, IList<IPoint> endList,
      IDoubleGrid heightGrid, BaseGrid<T> velocityGrid,
      out DoubleGrid costGrid, out IntGrid dirGrid)
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

      HeightGrid = heightGrid;
      VelocityGrid = velocityGrid;
      CalcCost(start, out costGrid, out dirGrid, false, stopList);
    }

    public void CalcCost(IPoint start,
      IDoubleGrid heightGrid, BaseGrid<T> velocityGrid,
      out DoubleGrid costGrid, out IntGrid dirGrid, bool invers)
    {
      HeightGrid = heightGrid;
      VelocityGrid = velocityGrid;
      CalcCost(start, out costGrid, out dirGrid, invers, null);
    }

    public void AssignCost(Polyline line, int costDim, DoubleGrid heightGrid, BaseGrid<T> velocityGrid)
    {
      IPoint p = line.Points.First.Value;
      double h1 = heightGrid.Value(p.X, p.Y, EGridInterpolation.bilinear);
      T v1 = velocityGrid.Value(p.X, p.Y);
      double s1 = heightGrid.Slope2(p.X, p.Y);
      double costTotal = 0;
      p[costDim] = costTotal;
      foreach (Curve curve in line.Segments)
      {
        double h0 = h1;
        T v0 = v1;
        double s0 = s1;

        p = curve.End;

        h1 = heightGrid.Value(p.X, p.Y, EGridInterpolation.bilinear);
        v1 = velocityGrid.Value(p.X, p.Y);
        s1 = heightGrid.Slope2(p.X, p.Y);

        double cost = StepCost(h0, v0, s0, h1, v1, s1, curve.Length());
        costTotal += cost;
        p[costDim] = costTotal;
      }
    }

    protected override void CalcCorridor(IPoint start, IPoint end,
      Polyline nearLine, double corridorWidth, out DoubleGrid costGrid, out IntGrid dirGrid)
    {
      BaseGrid<T> corridorGrid = new CorridorGrid(this, VelocityGrid, nearLine, corridorWidth);
      LeastCostPathBase<T> corridorLcp = new LeastCostPathBase<T>(
        new Box(new Point2D(_x0, _y0), new Point2D(_xMax, _yMax)), _dx, Step);
      corridorLcp.VelocityGrid = corridorGrid;
      corridorLcp.HeightGrid = HeightGrid;

      corridorLcp.CalcCost(start, new IPoint[] { end }, out costGrid, out dirGrid);
    }
    protected override void CalcCost(IPoint start,
      out DoubleGrid costGrid, out IntGrid dirGrid, bool invers, IList<int[]> stop)
    {
      try
      {
        if (VeloBase is ILockable)
        { ((ILockable)VeloBase).LockBits(); }

        int i, n;
        CostCompare costComparer = new CostCompare();
        FieldCompare fieldComparer = new FieldCompare();

        _costList = new SortedList<Field, object>(costComparer);
        _fieldList = new SortedList<Field, Field>(fieldComparer);

        costGrid = new DoubleGrid(_xMax, _yMax, typeof(float),
          _x0, _y0, _dx);
        dirGrid = new IntGrid(_xMax, _yMax, typeof(sbyte), _x0, _y0, _dx);

        Field startField;
        _dirGrid = dirGrid;

        startField = new Field();
        startField.X = (int)((start.X - _x0) / _dx);
        startField.Y = (int)((start.Y - _y0) / _dy);
        startField.Height = HeightGrid.Value(start.X, start.Y, EGridInterpolation.bilinear);
        startField.Velocity = VelocityGrid.Value(start.X, start.Y);
        startField.IdDir = -2;
        startField.Cost = 0;

        i = 0;
        n = _xMax * _yMax;
        StatusEventArgs args;

        while (startField != null)
        {
          costGrid[startField.X, startField.Y] = startField.Cost;
          dirGrid[startField.X, startField.Y] = startField.IdDir + 1;
          args = new StatusEventArgs(costGrid, dirGrid, startField.X, startField.Y, i, n);
          OnStatus(args);
          if (args.Cancel || Stop(stop, startField))
          {
            _costList.Clear();
            _fieldList.Clear();
            return;
          }

          i++;
          GotoNeighbours(startField, invers);

          //startField = null;
          //foreach (Field field in _costList.Keys)
          //{
          //  startField = field;
          //  break;
          //}
          //if (startField != null)
          //{
          //  _costList.Remove(startField);
          //  _fieldList.Remove(startField);
          //}
          if (_costList.Count > 0)
          {
            startField = _costList.Keys[0];
            _costList.RemoveAt(0);
            _fieldList.Remove(startField);
          }
          else
          {
            startField = null;
          }
        }
        OnStatus(new StatusEventArgs(costGrid, dirGrid, -1, -1, n, n));
      }
      finally
      {
        if (VeloBase is ILockable)
        { ((ILockable)VeloBase).UnlockBits(); }
      }
    }

    private bool Stop(IList<int[]> stop, Field startField)
    {
      if (stop == null)
      { return false; }

      int i = stop.Count - 1;
      while (i >= 0)
      {
        int[] s = stop[i];
        if (startField.X == s[0] && startField.Y == s[1])
        { stop.RemoveAt(i); }
        i--;
      }

      return (stop.Count == 0);
    }

    protected sealed override bool EqualSettingsCore(LeastCostPathBase o)
    {
      LeastCostPathBase<T> other = o as LeastCostPathBase<T>;
      if (other == null)
      { return false; }

      if (Step.Count != other.Step.Count)
      { return false; }

      if (_x0 != other._x0 ||
        _y0 != other._y0 ||
        _xMax != other._xMax ||
        _yMax != other._yMax ||
        _dx != other._dx)
      { return false; }

      if (StepCost.Method != other.StepCost.Method)
      { return false; }

      return true;
    }
  }
}
