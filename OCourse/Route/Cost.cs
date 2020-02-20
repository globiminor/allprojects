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
}
