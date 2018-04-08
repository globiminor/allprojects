using Basics.Geom;
using Grid.Lcp;

namespace OCourse.Route
{
  public abstract class SymLeastCostPath : LeastCostPath<HeightVeloField> //<IList<int>>
  {
    public SymLeastCostPath(IBox box, double dx)
      : base(box, dx)
    { }

    public SymLeastCostPath(IBox box, double dx, Steps step)
      : base(box, dx, step)
    { }
  }
}
