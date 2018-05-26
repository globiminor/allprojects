using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Shape.Data
{
  internal static class AccessUtils
  {
    public static Box GetExtent(byte[] data)
    {
      double xMin = BitConverter.ToDouble(data, 4);
      double yMin = BitConverter.ToDouble(data, 12);
      double xMax = BitConverter.ToDouble(data, 20);
      double yMax = BitConverter.ToDouble(data, 28);

      return new Box(new Point2D(xMin, yMin), new Point2D(xMax, yMax));
    }

    const int _typeArea = 19;

    private class AccessArea : Area
    {
      private byte[] _bytes;
      public AccessArea(byte[] bytes)
      {
        _bytes = bytes;
      }
      public override IBox Extent
      {
        get
        {
          if (_bytes != null)
          { return GetExtent(_bytes); }
          return base.Extent;
        }
      }
      protected override PolylineCollection _lines
      {
        get
        {
          PolylineCollection border = base._lines;
          if (_bytes == null)
          { return border; }

          List<Polyline> lines = GetLines(_bytes);
          border.AddRange(lines);
          _bytes = null;

          return border;
        }
      }
    }
    public static IGeometry GetGeometry(byte[] bytes)
    {
      const int TypeLine = 10;
      int type = BitConverter.ToInt32(bytes, 0);
      if (type == _typeArea || type == 5 || type == 536870963)
      {
        return new AccessArea(bytes);
        //List<Polyline> lines = GetLines(bytes);
        //Area area = new Area();
        //area.Border.AddRange(lines);
        //return area;
      }
      else if (type == TypeLine)
      {
        List<Polyline> lines = GetLines(bytes);
        if (lines.Count == 1)
        {
          return lines[0];
        }
        return new PolylineCollection(lines);
      }
      throw new NotImplementedException();
    }

    private static List<Polyline> GetLines(byte[] bytes)
    {
      int nParts = BitConverter.ToInt32(bytes, 36);
      List<Polyline> lines = new List<Polyline>(nParts);

      int totalPoints = BitConverter.ToInt32(bytes, 40);
      List<int> startPoints = new List<int>(nParts + 1);
      for (int iPart = 0; iPart < nParts; iPart++)
      { startPoints.Add(BitConverter.ToInt32(bytes, 44 + iPart * 4)); }
      startPoints.Add(totalPoints);
      for (int iPart = 0; iPart < nParts; iPart++)
      {
        int prePoints = startPoints[iPart];
        int nPoints = startPoints[iPart + 1] - prePoints;
        Polyline line = new Polyline();
        for (int iPoint = prePoints; iPoint < prePoints + nPoints; iPoint++)
        {
          int pos = 40 + nParts * 4 + 4 + iPoint * 16;
          double x = BitConverter.ToDouble(bytes, pos);
          double y = BitConverter.ToDouble(bytes, pos + 8);
          Point2D p = new Point2D(x, y);
          p.X = x;
          p.Y = y;
          line.Add(p);
        }
        lines.Add(line);
      }
      return lines;
    }
  }
}
