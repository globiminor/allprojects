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

    public static double CostPath_StepCost(double h0, double v0, double slope0_2,
                                           double h1, double v1, double slope1_2, double distance)
    {
      if (v0 < 0.002 || v1 < 0.002)
      { return -distance * 1000; }

      double v = (v0 + v1) / 2.0;
      double dhPos = h1 - h0;
      if (dhPos < 0)
      { dhPos = 0; }

      double cost;
      cost = distance / v + dhPos * 7;

      if (v < 0.99)
      { // not road Road
        double slope2 = (slope0_2 + slope1_2) / 2;
        cost += slope2 * 7.5 * distance;
      }
      if (double.IsNaN(cost))
      { return -distance * 100; }
      return cost;
    }
  }
}
