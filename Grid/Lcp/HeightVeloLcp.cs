
using Basics.Geom;

namespace Grid.Lcp
{
  public class HeightVeloField : IField
  {
    public int X { get; set; }
    public int Y { get; set; }

    public int IdDir { get; set; }
    public double Cost { get; set; }
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
  }

}