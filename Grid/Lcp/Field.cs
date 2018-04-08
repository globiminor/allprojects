
using System.Collections.Generic;

namespace Grid.Lcp
{
  public interface IField
  {
    int X { get; }
    int Y { get; }
    int IdDir { get; set; }
    double Cost { get; set; }
  }
  public class FieldCompare : IComparer<IField>
  {
    int IComparer<IField>.Compare(IField x, IField y)
    { return Compare(x, y); }
    public static int Compare(IField x, IField y)
    {
      int d = x.X.CompareTo(y.X);
      if (d != 0) return d;

      d = x.Y.CompareTo(y.Y);
      return d;
    }
  }

  public class CostCompare : IComparer<IField>
  {
    #region IComparer Members

    public int Compare(IField x, IField y)
    {
      int d = x.Cost.CompareTo(y.Cost);
      if (d != 0) return d;
      return FieldCompare.Compare(x, y);
    }

    #endregion
  }
}