using System;
using System.Collections.Generic;
using Basics.Geom;
using Ocad;

namespace OCourse.Route
{
  internal class CostFromTo : CostBase, ICost
  {
    public class SectionComparer : IComparer<CostFromTo>, IEqualityComparer<CostFromTo>
    {
      public bool Equals(CostFromTo x, CostFromTo y)
      {
        return Compare(x, y) == 0;
      }
      public int GetHashCode(CostFromTo r)
      {
        double res = r.Resolution;
        if (res <= 0) return r.Start.X.GetHashCode() ^ r.End.Y.GetHashCode();
        int nsx = (int)Math.Round(r.Start.X / res);
        int ney = (int)Math.Round(r.End.Y / res);

        return nsx.GetHashCode() ^ ney.GetHashCode() ^ res.GetHashCode();
      }
      public int Compare(CostFromTo x, CostFromTo y)
      {
        if (x == y) { return 0; }
        int i;
        i = x.Resolution.CompareTo(y.Resolution);
        if (i != 0) return i;
        double r = x.Resolution;

        i = Compare(x.Start.X, y.Start.X, r);
        if (i != 0) return i;
        i = Compare(x.Start.Y, y.Start.Y, r);
        if (i != 0) return i;
        i = Compare(x.End.X, y.End.X, r);
        if (i != 0) return i;
        i = Compare(x.End.Y, y.End.Y, r);
        if (i != 0) return i;

        return 0;
      }
      private int Compare(double x, double y, double res)
      {
        int i = x.CompareTo(y);
        if (i == 0 || res <= 0)
        { return i; }

        int nx = (int)Math.Round(x / res);
        int ny = (int)Math.Round(y / res);
        i = nx.CompareTo(ny);
        return i;
      }
    }

    private readonly Control _from;
    private readonly Control _to;

    public IPoint Start { get; private set; }
    public IPoint End { get; private set; }
    public double Resolution { get; set; }
    private Polyline _route;

    public CostFromTo(Control ctrlFrom, Control ctrlTo,
      IPoint start, IPoint end, double resolution,
      double direct, double climb, Polyline route, double optimalLength, double optimalCost)
      :
      base(direct, climb, optimalLength, optimalCost)
    {
      _from = ctrlFrom;
      _to = ctrlTo;

      Start = start;
      End = end;
      Resolution = resolution;

      _route = route;
    }

    string ICost.Name
    {
      get { return Name ?? $"{_from.Name} -> {_to.Name}"; }
    }

    public Control From
    { get { return _from; } }
    public Control To
    { get { return _to; } }

    public Polyline Route
    { get { return _route; } }
  }
}
