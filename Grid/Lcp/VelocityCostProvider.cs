using Basics.Geom;

namespace Grid.Lcp
{
  public class VelocityCostCalculator : IStepCostCalculator
  {
    double IStepCostCalculator.Calc(double dh, double vMean, double slope_2Mean,
                                           double distance)
    { return Calc(dh, vMean, slope_2Mean, distance); }

    public static double Calc(double dh, double vMean, double slope_2Mean,
                                           double distance)
    {
      if (vMean < 0.002)
      { return -distance * 1000; }

      double v = vMean;
      double dhPos = dh;
      if (dhPos < 0)
      { dhPos = 0; }

      double cost;
      cost = distance / v + dhPos * 7;

      if (v < 0.99)
      { // not road Road
        double slope2 = slope_2Mean;
        cost += slope2 * 7.5 * distance;
      }
      if (double.IsNaN(cost))
      { return -distance * 100; }
      return cost;
    }
  }
  public class VelocityCostProvider : ICostProvider<HeightVeloLcp>
  {
    public string FileFilter
    {
      get { return "velocity image files (*.tif)|*.tif"; }
    }

    private static VelocityCostCalculator _defaultCalc;
    public static VelocityCostCalculator DefaultCalculator
    { get { return _defaultCalc ?? (_defaultCalc = new VelocityCostCalculator()); } }
    private string _velocityName;
    private IDoubleGrid _velocityGrid;

    LeastCostPath ICostProvider.Build(IBox box, double dx, Steps step, string velocityData)
    { return Build(box, dx, step, velocityData); }
    public HeightVeloLcp Build(IBox box, double dx, Steps step, string velocityName)
    {
      HeightVeloLcp lcp = new HeightVeloLcp(box, dx, step);
      if (velocityName != _velocityName)
      {
        _velocityGrid = VelocityGrid.FromImage(velocityName);
        _velocityName = velocityName;
      }
      lcp.VelocityGrid = _velocityGrid;

      lcp.StepCostCalculator = DefaultCalculator;
      return lcp;
    }
  }
}
