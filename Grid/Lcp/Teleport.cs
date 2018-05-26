using Basics.Geom;

namespace Grid.Lcp
{
  public class Teleport
  {
    public Teleport(IPoint from, IDirCostModel fromCostModel,
      IPoint to, IDirCostModel toCostModel, double cost)
    {
      From = from;
      FromCostModel = fromCostModel;
      To = to;
      ToCostModel = toCostModel;
      Cost = cost;
    }
    public IPoint From { get; }
    public IDirCostModel FromCostModel { get; }
    public IPoint To { get; }
    public IDirCostModel ToCostModel { get; }
    public double Cost { get; }
  }

  public class Teleport<T> : Teleport
  {
    public Teleport(IPoint from, IDirCostModel<T> fromCostModel,
      IPoint to, IDirCostModel<T> toCostModel, double cost)
      : base(from, fromCostModel, to, toCostModel, cost)
    { }
    public new IDirCostModel<T> FromCostModel => (IDirCostModel<T>)base.FromCostModel;
    public new IDirCostModel<T> ToCostModel => (IDirCostModel<T>)base.ToCostModel;
  }

}
