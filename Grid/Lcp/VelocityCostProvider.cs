using Basics.Geom;

namespace Grid.Lcp
{
  public class VelocityCostProvider : ICostProvider
  {
    public string FileFilter
    {
      get { return "velocity image files (*.tif)|*.tif"; }
    }

    private string _velocityName;
    private IDoubleGrid _velocityGrid;

    LeastCostPathBase ICostProvider.Build(IBox box, double dx, Steps step, string velocityData)
    { return Build(box, dx, step, velocityData); }
    public LeastCostPath Build(IBox box, double dx, Steps step, string velocityName)
    {
      LeastCostPath lcp = new LeastCostPath(box, dx, step);
      if (velocityName != _velocityName)
      {
        _velocityGrid = VelocityGrid.FromImage(velocityName);
        _velocityName = velocityName;
      }
      lcp.VelocityGrid = _velocityGrid;

      lcp.StepCost = CostPath_StepCost;
      return lcp;
    }

    public static double CostPath_StepCost(double dh, double vMean, double slope_2Mean,
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
}
