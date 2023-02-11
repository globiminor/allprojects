using Basics.Geom;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class RouteRecord
  {
    public LinkedList<Point> Route { get; set; }
    public double Delay { get; set; }
    public double TotalCost { get; set; }

    public RouteRecord(LinkedList<Point> route, double totalCost, double delay)
    {
      Route = route;
      TotalCost = totalCost;
      Delay = delay;
    }
  }
}
