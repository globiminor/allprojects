using System.Collections.Generic;
using Grid;
using Basics.Geom;
using Grid.Lcp;

namespace OCourse.Route
{
  public class SymLeastCostPath : LeastCostPathBase<IList<int>>
  {
    public SymLeastCostPath(IBox box, double dx)
      : base(box, dx)
    { }

    public SymLeastCostPath(IBox box, double dx, Steps step)
      : base(box, dx, step)
    { }
  }
}
