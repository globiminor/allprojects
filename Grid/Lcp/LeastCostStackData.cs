﻿using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class LeastCostStackData : ILeastCostData
  {
    private readonly IList<LeastCostData> _leastCostData;
    private readonly Dictionary<LeastCostData, List<Teleport<LeastCostData>>> _teleports;
    private Dictionary<LeastCostData, Dictionary<IPoint, List<Teleport<LeastCostData>>>> _teleportsDict;
    public LeastCostStackData(IList<LeastCostData> leastCostData, Dictionary<LeastCostData, List<Teleport<LeastCostData>>> teleports)
    {
      _leastCostData = leastCostData;
      _teleports = teleports;
    }

    public double GetCost(double x, double y)
    {
      return _leastCostData[0].CostGrid.Value(x, y);
    }

    public Polyline GetPath(IPoint end)
    {
      Polyline path = GetPath(_leastCostData[0], end);
      return path;
    }

    public Polyline GetPath(LeastCostData lcd, IPoint end)
    {
      lcd.CostGrid.Extent.GetNearest(end.X, end.Y, out int ex, out int ey);

      LinkedList<Point> points = lcd.GetPathPoints(ex, ey);
      IPoint start = points.First.Value;
      double startCost = start.Z;
      if (startCost <= 0) // no teleports in path
      {
        lcd.DirGrid.Extent.GridToWorld(points);
        return Polyline.Create(points);
      }

      Teleport<LeastCostData> teleport = GetTeleport(lcd, start);

      Polyline from = GetPath(teleport.FromData, teleport.From);

      lcd.DirGrid.Extent.GridToWorld(points);
      foreach (var point in points)
      {
        from.Add(point);
      }
      return from;
    }


    private Teleport<LeastCostData> GetTeleport(LeastCostData lcd, IPoint to)
    {
      double toCost = to.Z;
      Dictionary<IPoint, List<Teleport<LeastCostData>>> teleports = GetTeleports(lcd);
      List<Teleport<LeastCostData>> candidates = teleports[to];

      Teleport<LeastCostData> match = null;
      double minOffset = double.MaxValue;
      foreach (var teleport in candidates)
      {
        double fromCost = toCost - teleport.Cost;

        LeastCostData fromData = teleport.FromData;
        IGrid<double> fromGrid = fromData.CostGrid;
        fromGrid.Extent.GetNearest(teleport.From, out int fromX, out int fromY);

        double offset = Math.Abs(fromGrid[fromX, fromY] - fromCost);
        if (offset < minOffset)
        {
          minOffset = offset;
          match = teleport;
        }
      }
      return match;
    }

    private Dictionary<IPoint, List<Teleport<LeastCostData>>> GetTeleports(LeastCostData lcd)
    {
      _teleportsDict = _teleportsDict ?? new Dictionary<LeastCostData, Dictionary<IPoint, List<Teleport<LeastCostData>>>>();
      if (!_teleportsDict.TryGetValue(lcd, out Dictionary<IPoint, List<Teleport<LeastCostData>>> pointTeleports))
      {
        List<Teleport<LeastCostData>> teleports = _teleports[lcd];

        pointTeleports = new Dictionary<IPoint, List<Teleport<LeastCostData>>>(new PointComparer());

        foreach (var teleport in teleports)
        {
          IPoint to = teleport.To;
          teleport.ToData.CostGrid.Extent.GetNearest(to, out int ix, out int iy);
          IPoint key = new Point2D(ix, iy);

          if (!pointTeleports.TryGetValue(key, out List<Teleport<LeastCostData>> pointPorts))
          {
            pointPorts = new List<Teleport<LeastCostData>>();
            pointTeleports.Add(key, pointPorts);
          }
          pointPorts.Add(teleport);
        }

        _teleportsDict.Add(lcd, pointTeleports);
      }
      return pointTeleports;
    }
  }

}
