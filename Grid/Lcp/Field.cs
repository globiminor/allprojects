
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class Field : IField
  {
    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString()
    {
      return $"X:{X} Y:{Y}";
    }
  }


  public class RestCostField : IRestCostField
  {
    public RestCostField(IField field)
    {
      X = field.X;
      Y = field.Y;
      IdDir = -1;
    }

    public int X { get; }
    public int Y { get; }

    public int IdDir { get; set; }
    public double Cost { get; set; }

    public double MinFullCost { get { return MinRestCost + Cost; } }
    public double MinRestCost { get; set; }

    public void SetCost(ICostField fromField, double cost, int idDir)
    {
      Cost = (fromField?.Cost ?? 0) + cost;
      IdDir = idDir;
    }

    public override string ToString()
    {
      return $"X:{X} Y:{Y} C:{Cost:N1} F:{MinFullCost:N1} D:{IdDir};";
    }
  }
  public class RestCostField<T> : RestCostField, ICostField<T>
  {
    public RestCostField(IField field, T pointInfo)
      : base(field)
    {
      PointInfo = pointInfo;
    }
    public T PointInfo { get; }
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