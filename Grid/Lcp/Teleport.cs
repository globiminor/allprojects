using Basics.Geom;

namespace Grid.Lcp
{
  public class Teleport
  {
    public static Teleport<T> Create<T>(IPoint from, T fromData,
      IPoint to, T toData, double cost)
    {
      return new Teleport<T>(from, fromData, to, toData, cost);
    }

    public Teleport(IPoint from, object fromData,
      IPoint to, object toData, double cost)
    {
      From = from;
      FromData = fromData;
      To = to;
      ToData = toData;
      Cost = cost;
    }
    public IPoint From { get; }
    public object FromData { get; }
    public IPoint To { get; }
    public object ToData { get; }
    public double Cost { get; }
  }

  public class Teleport<T> : Teleport
  {
    public Teleport(IPoint from, T fromCostModel,
      IPoint to, T toCostModel, double cost)
      : base(from, fromCostModel, to, toCostModel, cost)
    { }
    public new T FromData => (T)base.FromData;
    public new T ToData => (T)base.ToData;
  }

}
