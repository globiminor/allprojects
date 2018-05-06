
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class TerrainVeloModel : IDirCostProvider<TvmPoint>
  {
    public IDoubleGrid HeightGrid { get; }
    public IGrid<double> VelocityGrid { get; }

    public TerrainVeloModel(IDoubleGrid heightGrid, IGrid<double> velocityGrid)
    {
      HeightGrid = heightGrid;
      VelocityGrid = velocityGrid;
    }

    public ITvmCalc TvmCalc { get; set; } = new TvmCalc();

    public TvmPoint InitCostInfos(double tx, double ty)
    {
      TvmPoint init = new TvmPoint();
      init.Init(tx, ty, HeightGrid, VelocityGrid);
      return init;
    }

    public double MinUnitCost { get { return TvmCalc.MinUnitCost; } }

    double IDirCostProvider.GetCost(IList<double> x, IList<double> y, IList<double> w, double distance, bool inverse)
    {
      List<TvmPoint> pts = new List<TvmPoint>(x.Count);
      for (int i = 0; i < x.Count; i++)
      {
        pts.Add(InitCostInfos(x[i], y[i]));
      }
      return GetCost(pts, w, distance, inverse);
    }

    public double GetCost(IList<TvmPoint> pts, IList<double> ws, double distance, bool inverse)
    {
      double dh = pts[pts.Count - 1].Height - pts[0].Height;
      if (inverse)
      { dh = -dh; }

      double vMean = 0;
      double s2_Mean = 0;
      double sumW = 0;
      for (int i = 0; i < pts.Count; i++)
      {
        double w = ws[i];
        vMean += w * (1 / pts[i].Velocity);
        s2_Mean += w * pts[i].Slope2;
        sumW += w;
      }
      vMean = sumW / vMean;
      s2_Mean = s2_Mean / sumW;

      double cost = TvmCalc.Calc(dh, vMean, s2_Mean, distance);
      return cost;
    }
  }

  /// <summary>
  /// Terrain-Velocity-modell cost calculator
  /// </summary>
  public class TvmCalc : ITvmCalc
  {
    public double MinUnitCost => 1.0;

    double ITvmCalc.Calc(double dh, double vMean, double slope_2Mean,
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

  // performance :
  /// <summary>
  /// Terrain-Velocity-modell point
  /// </summary>
  public class TvmPoint
  {
    public double Height { get; private set; }
    public double Velocity { get; private set; }
    public double Slope2 { get; private set; }

    public void Init(double tx, double ty, IDoubleGrid heightGrid, IGrid<double> velocityGrid)
    {
      Height = heightGrid.Value(tx, ty, EGridInterpolation.bilinear);
      Velocity = velocityGrid.Value(tx, ty);
      Slope2 = heightGrid.Slope2(tx, ty);
    }
  }
}