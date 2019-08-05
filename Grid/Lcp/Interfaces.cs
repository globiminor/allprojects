using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public interface ILcpCalc
  {
    double MinUnitCost { get; }
  }
  /// <summary>
  /// TerrainVelocityModell Cost Calculator
  /// </summary>
  public interface ITvmCalc : ILcpCalc
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

  public interface IDirCostModel
  {
    Basics.Geom.IBox Extent { get; }

    double MinUnitCost { get; }
    double GetCost(IList<double> x, IList<double> y, IList<double> w, 
      double cellSize, double distance, bool inverse);
  }
  public interface IDirCostModel<T> : IDirCostModel
  {
    T InitCell(double centerX, double centerY, double cellSize);
    double GetCost(IList<T> costInfos, IList<double> w, double distance, bool inverse);
  }

  public interface ITeleportProvider
  {
    IReadOnlyList<Teleport> GetTeleports();
  }

  public static class FieldOp
  {
    public static double GetDist2(IField x, IField y)
    {
      int dx = x.X - y.X;
      int dy = x.Y - y.Y;
      return dx * dx + dy * dy;
    }
  }
  public interface IField
  {
    int X { get; }
    int Y { get; }
  }
  public interface ICostField : IField
  {
    int IdDir { get; }
    double Cost { get; }
    void SetCost(ICostField fromField, double deltaCost, int idDir);
  }
  public interface ICostField<T> : ICostField
  {
    T PointInfo { get; }
  }
  public interface IRestCostField : ICostField
  {
    double MinRestCost { get; set; }
  }
  public interface ICostOptimizer
  {
    void Init<T>(IField startField, SortedList<T, T> costList);

    IComparer<ICostField> GetCostComparer();
    bool AdaptCost(ICostField stepField);
    bool Stop<T>(ICostField processField, SortedList<T, T> costList);
  }
}
