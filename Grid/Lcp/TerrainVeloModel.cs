
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class TerrainVeloModel : IDirCostModel<TvmCell>, ITeleportProvider, ILockable
  {
    public IGrid<double> HeightGrid { get; }
    public IGrid<double> VelocityGrid { get; }

    private readonly List<Teleport> _teleports;

    public TerrainVeloModel(IGrid<double> heightGrid, IGrid<double> velocityGrid)
    {
      HeightGrid = heightGrid;
      VelocityGrid = velocityGrid;

      _teleports = new List<Teleport>();
    }

    void ILockable.LockBits()
    {
      (VelocityGrid as ILockable).LockBits();
      (HeightGrid as ILockable)?.LockBits();
    }
    void ILockable.UnlockBits()
    {
      (VelocityGrid as ILockable).UnlockBits();
      (HeightGrid as ILockable)?.UnlockBits();
    }

    public Basics.Geom.IBox Extent
    {
      get
      {
        if (HeightGrid is ConstGrid<double>)
        { return VelocityGrid.Extent.Extent; }

        Basics.Geom.Box b = new Basics.Geom.Box(HeightGrid.Extent.Extent);
        Basics.Geom.IBox extent = (Basics.Geom.IBox)b.Intersection(VelocityGrid.Extent.Extent)[0];
        return extent;
      }
    }
    public List<Teleport> Teleports => _teleports;
    IReadOnlyList<Teleport> ITeleportProvider.GetTeleports() { return _teleports; }

    public ITvmCalc TvmCalc { get; set; } = new TvmCalc();

    public TvmCell InitCell(double centerX, double centerY, double cellSize)
    {
      TvmCell init = new TvmCell();
      init.Init(centerX, centerY, HeightGrid, VelocityGrid);
      return init;
    }

    public double MinUnitCost { get { return TvmCalc.MinUnitCost; } }

    double IDirCostModel.GetCost(IList<double> x, IList<double> y, IList<double> w,
      double cellSize, double distance, bool inverse)
    {
      List<TvmCell> pts = new List<TvmCell>(x.Count);
      for (int i = 0; i < x.Count; i++)
      {
        pts.Add(InitCell(x[i], y[i], cellSize));
      }
      return GetCost(pts, w, distance, inverse);
    }

    public double GetCost(IList<TvmCell> pts, IList<double> ws, double distance, bool inverse)
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
  /// Terrain-Velocity-modell cell
  /// </summary>
  public class TvmCell
  {
    public double Height { get; private set; }
    public double Velocity { get; private set; }
    public double Slope2 { get; private set; }

		public override string ToString()
		{
			return $"h:{Height:N1}; v:{Velocity:N2}; s:{System.Math.Sqrt(Slope2):N3}";
		}

		public void Init(double tx, double ty, IGrid<double> heightGrid, IGrid<double> velocityGrid)
    {
      Height = heightGrid.Value(tx, ty, EGridInterpolation.bilinear);
      Velocity = velocityGrid.Value(tx, ty);
      Slope2 = heightGrid.Slope2(tx, ty);
    }
  }

  public class BlockModell : IDirCostModel<BlockCell>
  {
    public BlockGrid BlockGrid { get; }
    public BlockModell(BlockGrid grid)
    {
      BlockGrid = grid;
    }

    Basics.Geom.IBox IDirCostModel.Extent { get { return BlockGrid.Extent.Extent; } }
    double IDirCostModel.MinUnitCost => 1;

    double IDirCostModel.GetCost(IList<double> x, IList<double> y, IList<double> w,
      double cellSize, double distance, bool inverse)
    {
      List<BlockCell> pts = new List<BlockCell>(x.Count);
      for (int i = 0; i < x.Count; i++)
      {
        pts.Add(InitCell(x[i], y[i], cellSize));
      }
      return GetCost(pts, w, distance, inverse);
    }

    public double GetCost(IList<BlockCell> pts, IList<double> ws, double distance, bool inverse)
    {
      throw new System.NotImplementedException();
    }

    public BlockCell InitCell(double x, double y, double cellSize)
    {
      return new BlockCell();
    }
  }

  public class BlockCell
  {

  }
}