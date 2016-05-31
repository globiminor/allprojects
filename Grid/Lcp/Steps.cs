using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Grid.Lcp
{
  public class Step : Dir
  {
    public readonly int Index;
    public readonly double Angle;
    public readonly double Distance;
    public Step(int index, int dx, int dy)
      : base(dx, dy)
    {
      Index = index;

      Distance = Math.Sqrt(dx * dx + dy * dy);
      Angle = Math.Atan2(dy, dx);
    }

    public static int CompareDistance(Step x, Step y)
    {
      return x.Distance.CompareTo(y.Distance);
    }
  }

  public class Steps
  {
    private static readonly int[,] _step16 = new int[,]
      {
        {-1, 2}, {1, 2},
        {-2, 1}, {-1, 1}, {0, 1}, {1, 1}, {2, 1},
        {-1, 0}, {1, 0},
        {-2, -1}, {-1, -1}, {0, -1}, {1, -1}, {2, -1},
        {-1, -2}, {1, -2}
      };

    private static readonly int[,] _step8 = new int[,]
      {
        {-1, 1}, {0, 1}, {1, 1},
        {-1, 0},           {1, 0},
        {-1, -1}, {0, -1}, {1, -1}
      };

    private static readonly int[,] _step4 = new int[,]
      {
        {0, 1},
        {-1, 0}, {1, 0},
        {0, -1}
      };

    public static Steps Step16
    {
      get
      {
        return new Steps((int[,])_step16.Clone());
      }
    }

    public static Steps Step8
    {
      get
      {
        return new Steps((int[,])_step8.Clone());
      }
    }

    public static Steps Step4
    {
      get
      {
        return new Steps((int[,])_step4.Clone());
      }
    }

    private int _count;
    private List<Step> _steps;
    private List<Step> _distSteps;
    private List<int>[] _cancelSteps;

    public Step GetStep(int i)
    {
      return _steps[i];
    }
    public IEnumerable<Step> GetDistSteps()
    {
      return _distSteps;
    }

    public Steps(int[,] steps)
    {
      _count = steps.GetLength(0);
      _steps = new List<Step>(_count);
      for (int i = 0; i < _count; i++)
      {
        _steps.Add(new Step(i, steps[i, 0], steps[i, 1]));
      }

      _distSteps = new List<Step>(_steps);
      _distSteps.Sort(Step.CompareDistance);

      _cancelSteps = new List<int>[_count];
      foreach (Step step in _steps)
      {
        List<int> cancelSteps = GetCancelIndexes(step);
        _cancelSteps[step.Index] = cancelSteps;
      }
    }
    public int Count
    {
      get { return _count; }
    }

    public DoubleBaseGrid this[IntGrid grd]
    {
      get
      {
        double[] angles = new double[_count];
        foreach (Step step in _steps)
        { angles[step.Index] = step.Angle; }
        return DoubleBaseGrid.Create(angles, grd);
      }
    }

    private class StepInfo
    {
      public int Ix;
      public int Iy;
      public int Dir;
      public double Cost;

      public StepInfo(int ix, int iy, int dir, double cost)
      {
        Ix = ix;
        Iy = iy;
        Dir = dir;
        Cost = cost;
      }
      public override string ToString()
      {
        string s = string.Format("{0,2}: {1} {2}; {3:N1}", Dir, Ix, Iy, NormedCost());
        return s;
      }

      public double NormedCost()
      {
        return Cost / Math.Sqrt(Ix * Ix + Iy * Iy);
      }
    }
    private class VVelo
    {
      public readonly double Vx;
      public readonly double Vy;
      public readonly double V2;

      public VVelo(double vx, double vy, double vv)
      {
        Vx = vx;
        Vy = vy;
        V2 = vv;
      }
      public override string ToString()
      {
        return string.Format("{0:N3}; {1:N3}; {2:N3}", Vx, Vy, V2);
      }

      public double GetV()
      {
        return Math.Sqrt(Vx * Vx + Vy * Vy);
      }
    }
    private class StepInfoBuilder
    {
      public readonly List<StepInfo> Candidates = new List<StepInfo>();
      private readonly List<VVelo> _velos;
      StepInfo _assigned = new StepInfo(0, 0, 0, 0);

      public StepInfoBuilder(DoubleGrid grid)
      {
        _velos = new List<VVelo>(grid.Extent.Nx * grid.Extent.Ny / 4);
      }
      public bool IsAssigned()
      {
        return (_assigned.Ix != 0 || _assigned.Iy != 0);
      }
      public override string ToString()
      {
        return _assigned.ToString();
      }
      public bool IsInitialized()
      {
        return Candidates.Count > 0;
      }
      public bool TryAssignDir()
      {
        if (Candidates.Count == 1)
        {
          if (_assigned != Candidates[0])
          {
            _assigned = Candidates[0];
          }
          return true;
        }
        return false;
      }

      public void Analyze(StepInfo step, DoubleGrid costGrid)
      {
        for (int iDir = 0; iDir < Candidates.Count; iDir++)
        {
          StepInfo candidate = Candidates[iDir];
          int ix = step.Ix - candidate.Ix;
          int iy = step.Iy - candidate.Iy;
          if (ix < 0 || ix >= costGrid.Extent.Nx ||
            iy < 0 || iy >= costGrid.Extent.Ny)
          {
            Candidates.RemoveAt(iDir);
            continue;
          }
          double cost = step.Cost - costGrid[ix, iy];
          if (cost <= 0)
          {
            Candidates.RemoveAt(iDir);
            continue;
          }
          else
          {
            candidate.Cost += cost;
          }
        }

      }

      public void Initialize(StepInfo step, List<StepInfo> pres, Dictionary<int, StepInfoBuilder> dirs)
      {
        foreach (StepInfo pre in pres)
        {
          int dx = step.Ix - pre.Ix;
          int dy = step.Iy - pre.Iy;

          if (Basics.Utils.Gcd(dx, dy) != 1)
          { continue; }

          bool isAssigned = false;
          foreach (StepInfoBuilder t in dirs.Values)
          {
            if (t.IsAssigned() && t._assigned.Ix == dx && t._assigned.Iy == dy)
            {
              isAssigned = true;
              break;
            }
          }
          if (!isAssigned)
          {
            Candidates.Add(new StepInfo(dx, dy, step.Dir, step.Cost - pre.Cost));
          }
        }
      }

      public StepInfo AssignDirFromMaxCost()
      {
        StepInfo max = null;
        foreach (StepInfo candidate in Candidates)
        {
          if (max == null || candidate.NormedCost() > max.NormedCost())
          { max = candidate; }
        }
        Candidates.Clear();
        Candidates.Add(max);
        _assigned = max;
        return max;
      }
      public void Assign(Step step, int dir)
      {
        Candidates.Clear();
        _assigned = new StepInfo(step.Dx, step.Dy, dir, 0);
        Candidates.Add(_assigned);
      }

      public VVelo GetMeanVelo()
      {
        double vX = 0;
        double vY = 0;
        double sumW = 0;
        foreach (VVelo velo in _velos)
        {
          double v = velo.GetV();
          double w = 1.0 / (1.0 + velo.V2) / v;
          vX += w * velo.Vx;
          vY += w * velo.Vy;
          sumW += w;
        }
        VVelo mean = new VVelo(vX / sumW, vY / sumW, 0);
        return mean;
      }
      public void AddVelo(DoubleGrid grid, int ix, int iy)
      {
        if (ix < 1 || iy < 1 || ix + 2 > grid.Extent.Nx || iy + 2 > grid.Extent.Ny)
        {
          return;
        }
        double cost0 = grid[ix, iy];
        double sumX = 0;
        double sumY = 0;
        for (int i = -1; i < 2; i++)
        {
          for (int j = -1; j < 2; j++)
          {
            double dCost = grid[ix + i, iy + j] - cost0;
            sumX += dCost * i;
            sumY += dCost * j;
          }
        }
        double vx = sumX / 6.0;
        double vy = sumY / 6.0;

        double vv = 0;
        for (int i = -1; i < 2; i++)
        {
          for (int j = -1; j < 2; j++)
          {
            double dCost = grid[ix + i, iy + j] - cost0;
            double v = dCost - i * vx - j * vy;
            vv += v * v;
          }
        }
        VVelo velo = new VVelo(vx, vy, vv);
        _velos.Add(velo);
      }
    }
    public static Steps ReverseEngineer(IntGrid dirGrid, DoubleGrid costGrid)
    {
      const int size = 1024;
      SortedList<double, IList<StepInfo>> costs = new SortedList<double, IList<StepInfo>>();
      Dictionary<int, StepInfoBuilder> dirs = new Dictionary<int, StepInfoBuilder>();
      LoadAnalyzeData(dirGrid, costGrid, dirs, costs, size);

      GridExtent extent = dirGrid.Extent;

      int nCost = costs.Count;
      List<StepInfo> pres = new List<StepInfo>(size);
      for (int iCost = 0; iCost < nCost; iCost++)
      {
        IList<StepInfo> steps = costs.Values[iCost];
        foreach (StepInfo step in steps)
        {
          StepInfoBuilder dir = dirs[step.Dir];
          if (dir.IsAssigned())
          { continue; }

          if (dir.IsInitialized())
          {
            dir.Analyze(step, costGrid);
          }
          else
          {
            dir.Initialize(step, pres, dirs);
          }
          dir.TryAssignDir();
        }

        foreach (StepInfo step in steps)
        {
          pres.Add(step);
        }
      }

      for (int ix = 0; ix < extent.Nx; ix++)
      {
        for (int iy = 0; iy < extent.Ny; iy++)
        {
          StepInfo step = new StepInfo(ix, iy, dirGrid[ix, iy], costGrid[ix, iy]);
          StepInfoBuilder dir = dirs[step.Dir];
          if (!dir.IsAssigned() && dir.Candidates.Count > 0)
          {
            dir.Analyze(step, costGrid);
            if (dir.TryAssignDir())
            {
              foreach (StepInfoBuilder other in dirs.Values)
              {
                if (other == dir)
                { continue; }
                for (int i = 0; i < other.Candidates.Count; i++)
                {
                  StepInfo otherCand = other.Candidates[i];
                  if (otherCand.Ix == dir.Candidates[0].Ix && otherCand.Iy == dir.Candidates[0].Iy)
                  {
                    other.Candidates.RemoveAt(i);
                  }
                }
              }
            }
          }
        }
      }

      List<int> stepIndices = new List<int>(dirs.Count - 1);
      foreach (int stepIndex in dirs.Keys)
      {
        if (stepIndex < 0 || stepIndex > dirs.Count)
        { continue; }
        stepIndices.Add(stepIndex);
      }
      stepIndices.Sort();
      List<StepInfoBuilder> sortedSteps = new List<StepInfoBuilder>(stepIndices.Count);
      foreach (int i in stepIndices)
      {
        sortedSteps.Add(dirs[i]);
      }
      foreach (KeyValuePair<int, StepInfoBuilder> pair in dirs)
      {
        StepInfoBuilder dir = pair.Value;
        if (dir.IsAssigned())
        { continue; }

        VVelo mean = dir.GetMeanVelo();
        double l = mean.GetV();
        Steps steps = Step16;
        double min = double.MaxValue;
        Step minStep = null;
        foreach (Step step in steps._steps)
        {
          double p = Math.Abs(1 - (step.Dx * mean.Vx + step.Dy * mean.Vy) / (l * step.Distance));
          if (p < min)
          {
            minStep = step;
            min = p;
          }
        }
        dir.Assign(minStep, pair.Key);
      }

      foreach (StepInfoBuilder step in sortedSteps)
      {
        if (step.IsAssigned())
        { continue; }

        step.AssignDirFromMaxCost();
      }
      int nSteps = sortedSteps.Count;
      int[,] result = new int[sortedSteps.Count, 2];
      for (int iStep = 0; iStep < nSteps; iStep++)
      {
        StepInfo step = sortedSteps[iStep].Candidates[0];
        if (step.Dir != iStep + 1) throw new InvalidOperationException(string.Format("Direction {0} is missing", iStep + 1));
        result[iStep, 0] = step.Ix;
        result[iStep, 1] = step.Iy;
      }
      return new Steps(result);
    }

    private static void LoadAnalyzeData(IntGrid dirGrid, DoubleGrid costGrid,
      Dictionary<int, StepInfoBuilder> dirs,
      SortedList<double, IList<StepInfo>> costs, int size)
    {
      GridExtent extent = dirGrid.Extent;

      for (int ix = 0; ix < extent.Nx; ix++)
      {
        for (int iy = 0; iy < extent.Ny; iy++)
        {
          double cost = costGrid[ix, iy];
          int dir = dirGrid[ix, iy];
          StepInfoBuilder list;
          if (dirs.TryGetValue(dir, out list) == false)
          {
            list = new StepInfoBuilder(costGrid);
            dirs.Add(dir, list);
          }
          list.AddVelo(costGrid, ix, iy);


          if (costs.Count < size || cost <= costs.Keys[size - 1])
          {
            IList<StepInfo> steps;
            if (costs.TryGetValue(cost, out steps) == false)
            {
              if (costs.Count == size)
              {
                costs.RemoveAt(size - 1);
              }
              steps = new List<StepInfo>();
              costs.Add(cost, steps);
            }
            steps.Add(new StepInfo(ix, iy, dir, cost));
          }
        }
      }
    }

    public bool[] GetCancelIndexes(Step step, bool[] cancelFields)
    {
      if (cancelFields == null)
      { cancelFields = new bool[_count]; }

      List<int> cancelSteps = _cancelSteps[step.Index];
      foreach (int cancelIndex in cancelSteps)
      {
        cancelFields[cancelIndex] = true;
      }
      return cancelFields;
    }

    private List<int> GetCancelIndexes(Step step)
    {
      double minAngle = 0;
      double maxAngle = 0;
      Point2D center = null;
      List<int> cancelSteps = new List<int>(_count / 2);

      foreach (Step s in _steps)
      {
        if (s.Distance <= step.Distance)
        { continue; }

        if (center == null)
        {
          center = new Point2D(step.Dx / step.Distance, step.Dy / step.Distance);
          IList<Point2D> points = new Point2D[]
            {
              new Point2D(step.Dx - 0.5, step.Dy - 0.5),
              new Point2D(step.Dx - 0.5, step.Dy + 0.5),
              new Point2D(step.Dx + 0.5, step.Dy - 0.5),
              new Point2D(step.Dx + 0.5, step.Dy + 0.5),
            };
          foreach (Point2D d in points)
          {
            double dist = Math.Sqrt(d.OrigDist2());
            double angle = center.VectorProduct(d) / dist;
            if (angle < minAngle) minAngle = angle;
            if (angle > maxAngle) maxAngle = angle;
          }
        }
        {
          if (s.Dx * step.Dx + s.Dy * step.Dy < 0)
          { continue; }

          double dist = s.Distance;
          double angle = center.VectorProduct(new Point2D(s.Dx, s.Dy)) / dist;
          if (angle > minAngle && angle < maxAngle)
          { cancelSteps.Add(s.Index); }
        }
      }
      return cancelSteps;
    }
  }
}