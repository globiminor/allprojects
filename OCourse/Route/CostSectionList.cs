using Ocad;
using OCourse.Ext;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCourse.Route
{
  public class CostSectionlist : CostBase, IInfo, ICostSectionlists
  {
    private readonly SectionList _sectionList;
    private readonly List<CostBase> _parts;
    public CostSectionlist(CostBase sum, SectionList sectionList)
      : base(sum)
    {
      _sectionList = sectionList;
      if (sum is CostSum s)
      { _parts = new List<CostBase>(s.Parts); }
    }
    public SectionList Sections
    {
      get { return _sectionList; }
    }
    public IReadOnlyList<CostBase> Parts
    { get { return _parts; } }

    public static IEnumerable<CostSectionlist> GetCostSectionLists(
      IEnumerable<ViewModels.PermutationVm> permutations,
      RouteCalculator routeCalc, double resol)
    {
      foreach (var permut in permutations)
      {
        int iLeg = 0;
        string sLeg = string.Empty;
        SectionList sections = null;
        foreach (var next in permut.Permutation.NextControls)
        {
          if (next.Control == SectionList.LegStart)
          {
            if (sections != null)
            {
              iLeg++;
              sLeg = $".{iLeg}";
              CostSectionlist cost = routeCalc.GetCourseInfo(sections, resol);
              cost.Name = $"{permut.StartNr}{sLeg}";
              yield return cost;
              sLeg = $".{iLeg + 1}";
            }
            sections = null;
            continue;
          }
          if (next.Control.Code == ControlCode.TextBlock
            && next.Control.Text.StartsWith("where ", StringComparison.InvariantCultureIgnoreCase))
          { continue; }

          if (sections == null)
          { sections = new SectionList(next); }
          else
          { sections.Add(next); }
        }
        if (sections != null)
        {
          CostSectionlist cost = routeCalc.GetCourseInfo(sections, resol);
          cost.Name = $"{permut.StartNr}{sLeg}";
          yield return cost;
        }
      }
    }
    public static IEnumerable<CostSectionlist> GetUniqueCombs(IEnumerable<ICost> costs)
    {
      Dictionary<CostSectionlist, CostSectionlist> uniques = new Dictionary<CostSectionlist, CostSectionlist>();
      foreach (var cost in costs)
      {
        if (!(cost is ICostSectionlists sections))
        { continue; }

        foreach (var costSectionlist in sections.Costs)
        {
          if (!uniques.ContainsKey(costSectionlist))
          {
            uniques.Add(costSectionlist, costSectionlist);
          }
        }
      }
      return uniques.Keys;
    }
    IEnumerable<CostSectionlist> ICostSectionlists.Costs
    { get { yield return this; } }
    public string GetInfo()
    {
      StringBuilder sb = new StringBuilder();
      foreach (var c in _sectionList.Controls)
      {
        if (sb.Length > 0)
        { sb.Append(" "); }
        sb.Append(c.Name);
      }

      return sb.ToString();
    }
  }
}
