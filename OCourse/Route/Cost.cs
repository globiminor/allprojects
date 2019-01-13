using Ocad;
using OCourse.Ext;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCourse.Route
{
  public interface ICost
  {
    string Name { get; }
    double DirectLKm { get; }
    double DirectKm { get; }
    double Climb { get; }
    double OptimalLKm { get; }
    double OptimalKm { get; }
    double OptimalCost { get; }
  }

  public interface IInfo
  {
    string GetInfo();
  }

  public interface ICostSectionlists
  {
    IEnumerable<CostSectionlist> Costs { get; }
  }

  public class CostBase : ICost
  {
    private readonly double _direct;
    private readonly double _optimalLength;
    private readonly double _optimalCost;
    private readonly double _climb;

    private string _name;

    double ICost.DirectKm { get { return _direct / 1000.0; } }
    double ICost.DirectLKm { get { return (_direct + 10.0 * _climb) / 1000.0; } }
    double ICost.OptimalKm { get { return _optimalLength / 1000.0; } }
    double ICost.OptimalLKm { get { return (_optimalLength + 10.0 * _climb) / 1000.0; } }

    protected CostBase(CostBase cost)
          : this(cost.Direct, cost.Climb, cost.OptimalLength, cost.OptimalCost)
    { }
    protected CostBase(double direct, double climb, double optimalLength, double optimalCost)
    {
      _direct = direct;
      _climb = climb;

      _optimalLength = optimalLength;
      _optimalCost = optimalCost;
    }

    public double Direct
    { get { return _direct; } }

    public double OptimalCost
    { get { return _optimalCost; } }

    public double OptimalLength
    { get { return _optimalLength; } }

    public double Climb
    { get { return _climb; } }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
  }

  public class CostSum : CostBase
  {
    private readonly List<CostBase> _parts = new List<CostBase>();
    public CostSum(double direct, double climb, double optimalLength, double optimalCost)
      : base(direct, climb, optimalLength, optimalCost)
    { }

    public IReadOnlyList<CostBase> Parts { get { return _parts; } }

    public CostSum(CostBase cost)
      : base(cost)
    {
      _parts.Add(cost);
    }

    public static CostSum operator +(CostSum s0, CostBase s1)
    {
      CostSum sum = new CostSum(s0.Direct + s1.Direct, s0.Climb + s1.Climb,
        s0.OptimalLength + s1.OptimalLength, s0.OptimalCost + s1.OptimalCost);

      sum._parts.AddRange(s0._parts);
      sum._parts.Add(s1);

      return sum;
    }

    public static CostSum operator *(double f, CostSum section)
    {
      CostSum prod = new CostSum(f * section.Direct, f * section.Climb, f * section.OptimalLength, f * section.OptimalCost);

      return prod;
    }
  }

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

    internal static IEnumerable<CostSectionlist> GetCostSectionLists(
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
