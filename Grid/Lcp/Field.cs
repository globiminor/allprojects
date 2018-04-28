
using System.Collections.Generic;

namespace Grid.Lcp
{
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
  public interface IRestCostField : ICostField
  {
    double MinRestCost { get; set; }
  }

  public class FieldComparer : IComparer<IField>, IEqualityComparer<IField>
  {
    int IComparer<IField>.Compare(IField x, IField y)
    { return Compare(x, y); }
    bool IEqualityComparer<IField>.Equals(IField x, IField y)
    { return Compare(x, y) == 0; }
    public static int Compare(IField x, IField y)
    {
      int d = x.X.CompareTo(y.X);
      if (d != 0) return d;

      d = x.Y.CompareTo(y.Y);
      return d;
    }

    int IEqualityComparer<IField>.GetHashCode(IField obj)
    { return obj.X + 239 * obj.Y; }
  }

  public class CostComparer : IComparer<ICostField>
  {
    #region IComparer Members

    public int Compare(ICostField x, ICostField y)
    {
      int d = x.Cost.CompareTo(y.Cost);
      if (d != 0) return d;
      return FieldComparer.Compare(x, y);
    }

    #endregion
  }

  public class MinFullCostComparer : IComparer<ICostField>
  {
    #region IComparer Members

    public int Compare(ICostField x, ICostField y)
    {
      double xCost = x.Cost + ((x as IRestCostField)?.MinRestCost ?? 0);
      double yCost = y.Cost + ((y as IRestCostField)?.MinRestCost ?? 0);
      int d = xCost.CompareTo(yCost);
      if (d != 0) return d;
      return FieldComparer.Compare(x, y);
    }

    #endregion
  }
}