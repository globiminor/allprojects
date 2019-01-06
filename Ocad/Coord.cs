using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Ocad
{
  public class Coord
  {
    [Flags]
    public enum Flags
    {
      none = 0,
      firstBezierPoint = 1,
      secondBezierPoint = 2,
      noLeftLine = 4,
      noLine = 8,
      cornerPoint = 256,
      firstHolePoint = 512,
      noRightLine = 1024,
      dashPoint = 2048
    }

    private int _ix;
    private int _iy;

    private Coord()
    { }

    public int Ox => _ix;
    public int Oy => _iy;

    public Coord(int ox, int oy)
    {
      _ix = ox;
      _iy = oy;
    }
    public static Coord Create(IPoint point, Flags code)
    {
      Coord coord = new Coord
      {
        _ix = (((int)point.X) << 8) | ((int)code % 256),
        _iy = (((int)point.Y) << 8) | ((int)code >> 8)
      };

      return coord;
    }

    public Point2D GetPoint()
    {
      Point2D p = new Point2D
      {
        X = GetGeomPart(_ix),
        Y = GetGeomPart(_iy)
      };
      return p;
    }

    public bool IsBezier => (_ix & 1) == 1;
    public bool IsNewRing => (_iy & 2) == 2;

    public int Code()
    {
      int code = GetCodePart(_ix);
      return code | (GetCodePart(_iy) << 8);
    }

    public static int GetGeomPart(int coord)
    {
      return coord >> 8;
    }

    public static int GetCodePart(int coord)
    {
      int i;
      i = coord % 256;
      if (i < 0)
      {
        i += 256;
      }
      return i;
    }

    public string CodeString(int code)
    {
      System.Text.StringBuilder str = new System.Text.StringBuilder(8);
      for (int i = 0; i < 8; i++)
      {
        if (((code >> i) & 1) == 1)
        { str[7 - i] = '1'; }
        else
        { str[7 - i] = '0'; }
      }
      return str.ToString();
    }

    public override string ToString()
    {
      int cx = GetCodePart(_ix);
      int cy = GetCodePart(_iy);

      return string.Format("{0,7} {1,7}   {2}   {3}  {4}",
        GetGeomPart(_ix), GetGeomPart(_iy),
        cx, cy, (Flags)(cx + 256 * cy));
    }


    public static IGeometry GetGeometry(GeomType type, IList<Coord> coords)
    {
      if (type == GeomType.point || type == GeomType.unformattedText ||
        type == GeomType.formattedText)
      {
        if (coords.Count == 1)
        { return coords[0].GetPoint(); }
        else
        {
          PointCollection points = new PointCollection();
          foreach (var coord in coords)
          { points.Add(coord.GetPoint()); }
          return points;
        }
      }
      else if (type == GeomType.line || type == GeomType.lineText)
      {
        Polyline line = GetPolyline(coords);
        return line;
      }
      else if (type == GeomType.rectangle)
      {
        Polyline line = GetPolyline(coords);
        return line;
      }
      else if (type == GeomType.area)
      {
        Area pArea = GetArea(coords);
        return pArea;
      }
      else
      { // unknown geometry type
        return null;
      }
    }

    public static Area GetArea(IList<Coord> coords)
    {
      int iPoint = 0;
      Area pArea = new Area(GetPolylineCore(coords, ref iPoint));
      while (iPoint < coords.Count) // read holes / inner rings
      { pArea.Border.Add(GetPolylineCore(coords, ref iPoint)); }
      return pArea;
    }
    public static Polyline GetPolyline(IList<Coord> coords)
    {
      int iPoint = 0;
      Polyline line = GetPolylineCore(coords, ref iPoint);

      if (coords.Count != iPoint)
      { throw new InvalidOperationException($"Expected Points {coords.Count} <> {iPoint}"); }

      return line;
    }
    private static Polyline GetPolylineCore(IList<Coord> coords, ref int iPoint)
    {
      int nPoints = coords.Count;
      Point p0;
      Polyline line = new Polyline();

      p0 = coords[iPoint].GetPoint();
      iPoint++;
      while (iPoint < nPoints)
      {
        Curve seg;
        Point p1;

        Coord coord1 = coords[iPoint];
        if (coord1.IsNewRing)
        {
          break;
        }

        iPoint++;
        if (coord1.IsBezier)
        {
          Coord coord2 = coords[iPoint];
          iPoint++;
          Coord coord3 = coords[iPoint];
          iPoint++;
          p1 = coord3.GetPoint();
          seg = new Bezier(p0, coord1.GetPoint(),
            coord2.GetPoint(), p1);
        }
        else
        {
          p1 = coord1.GetPoint();
          seg = new Line(p0, p1);
        }
        line.Add(seg);

        p0 = p1;
      }

      return line;
    }
  }
}
