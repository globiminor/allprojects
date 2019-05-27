using System.Collections.Generic;

namespace OCourse.Route
{
  public class CostMean : CostBase, ICostSectionlists
  {
    public CostMean(CostBase cost, List<CostSectionlist> group)
      :base(cost)
    { Group = group; }
    public List<CostSectionlist> Group { get; private set; }

    IEnumerable<CostSectionlist> ICostSectionlists.Costs
    { get { return Group; } }

  }
}
