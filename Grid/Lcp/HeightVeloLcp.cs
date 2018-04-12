
using Basics.Geom;

namespace Grid.Lcp
{
  public interface IStepCostCalculator
  {
    /// <summary>
    /// Calculates cost for a step
    /// </summary>
    /// <param name="dh">Height difference</param>
    /// <param name="vMean">mean Velocity</param>
    /// <param name="slope0_2Mean">mean Square of slope at start point</param>
    /// <param name="dist">Distance between start and end point of step</param>
    /// <returns>Cost for the step</returns>
    double Calc(double dh, double vMean, double slope_2Mean, double dist);
  }

  public class HeightVeloField : IField
  {
    public int X { get; set; }
    public int Y { get; set; }

    public int IdDir { get; set; }
    public double Cost { get; set; }

    public void SetCost(IField fromField, double cost, int idDir)
    {
      Cost = (fromField?.Cost ?? 0) + cost;
      IdDir = idDir;
    }

    public override string ToString()
    {
      return $"X:{X} Y:{Y} C:{Cost:N1} D:{IdDir}; H:{Height:N1}, V:{Velocity:N1}";
    }
    // performance :
    public double Height;
    public double Velocity;
    public double Slope2;

    private bool _initiated;
    public void Init(double tx, double ty, IDoubleGrid heightGrid, IGrid<double> velocityGrid)
    {
      if (_initiated)
      { return; }

      Height = heightGrid.Value(tx, ty, EGridInterpolation.bilinear);
      Velocity = velocityGrid.Value(tx, ty);
      Slope2 = heightGrid.Slope2(tx, ty);

      _initiated = true;
    }
  }

  public class HeightVeloLcp : LeastCostPath<HeightVeloField>
  {
    public HeightVeloLcp(IBox box, double dx, Steps step = null)
      : base(box, dx, step)
    { }

    protected override HeightVeloField InitField(IField position)
    {
      double tx = X0 + position.X * Dx;
      double ty = Y0 + position.Y * Dy;

      HeightVeloField init = position as HeightVeloField ??
        new HeightVeloField { X = position.X, Y = position.Y, IdDir = -1 };

      init.Init(tx, ty, HeightGrid, VelocityGrid);
      return init;
    }

    public IDoubleGrid HeightGrid { get; set; }
    public IGrid<double> VelocityGrid { get; set; }

    public IStepCostCalculator StepCostCalculator { get; set; } = VelocityCostProvider.DefaultCalculator;

    protected override double CalcCost(HeightVeloField startField, Step step, int iField,
      HeightVeloField[] neighbors, bool invers)
    {
      double dh, vMean, s2_Mean;
      dh = Steps.GetMaxDiff(startField.Height, iField, neighbors, (f) => f.Height, invers);
      vMean = 1.0 / Steps.GetMean(1.0 / startField.Velocity, iField, neighbors, (f) => 1.0 / f.Velocity); // stepField.Velocity;
      s2_Mean = Steps.GetMean(startField.Slope2, iField, neighbors, (f) => f.Slope2);

      double cost = StepCostCalculator.Calc(dh, vMean, s2_Mean, step.Distance * Dx);
      return cost;
    }

    protected override void CalcStarting()
    {
      if (VelocityGrid is ILockable)
      { ((ILockable)VelocityGrid).LockBits(); }
    }

    protected override void CalcEnded()
    {
      if (VelocityGrid is ILockable)
      { ((ILockable)VelocityGrid).UnlockBits(); }
    }

    public void CalcCost(Point2D point, IDoubleGrid heightGrid, IDoubleGrid velocityGrid,
      out DataDoubleGrid costGrid, out IntGrid dirGrid, bool inverse = false)
    {
      HeightGrid = heightGrid;
      VelocityGrid = velocityGrid;
      CalcCost(point, out costGrid, out dirGrid, inverse);
    }

    public void AssignCost(Polyline line, int costDim)
    {
      IPoint p = line.Points.First.Value;
      double h1 = HeightGrid.Value(p.X, p.Y, EGridInterpolation.bilinear);
      double v1 = 1.0 / VelocityGrid.Value(p.X, p.Y);
      double s1 = HeightGrid.Slope2(p.X, p.Y);
      double costTotal = 0;
      p[costDim] = costTotal;
      foreach (Curve curve in line.Segments)
      {
        double h0 = h1;
        double v0 = v1;
        double s0 = s1;

        p = curve.End;

        h1 = HeightGrid.Value(p.X, p.Y, EGridInterpolation.bilinear);
        v1 = 1.0 / VelocityGrid.Value(p.X, p.Y);
        s1 = HeightGrid.Slope2(p.X, p.Y);

        double cost = StepCostCalculator.Calc(h1 - h0, 2.0 / (v0 + v1), (s0 + s1) / 2.0, curve.Length());
        costTotal += cost;
        p[costDim] = costTotal;
      }
    }


    protected override bool EqualSettingsCore(LeastCostPath o)
    {
      HeightVeloLcp other = o as HeightVeloLcp;
      if (o == null)
      { return false; }
      if (StepCostCalculator != other.StepCostCalculator)
      { return false; }

      return base.EqualSettingsCore(o);
    }
  }

}