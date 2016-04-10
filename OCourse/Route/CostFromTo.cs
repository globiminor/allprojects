using System;
using Basics.Geom;
using Ocad;

namespace OCourse.Route
{
  internal class CostFromTo : CostBase
  {
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

    public Control From
    { get { return _from; } }
    public Control To
    { get { return _to; } }

    public Polyline Route
    { get { return _route; } }
  }
}
