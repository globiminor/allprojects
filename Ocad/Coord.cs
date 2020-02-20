using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Ocad
{
  public class Coord
  {

    public class CodePoint : Point2D
    {
      public CodePoint(double x, double y)
        : base(x, y)
      { }
      public Flags Flags { get; set; }
      protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
      public new CodePoint Project(IProjection projection)
      {
        IPoint p1 = base.Project(projection);
        CodePoint d = new CodePoint(p1.X, p1.Y) { Flags = Flags };
        return d;
      }
    }
    public class PartPoint : Point2D
    {
      public PartPoint(double x, double y)
        : base(x, y)
      { }
      public List<Coord> Angles { get; set; }
      protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
      public new PartPoint Project(IProjection projection)
      {
        IPoint p1 = base.Project(projection);
        // TODO: rotate angles
        PartPoint d = new PartPoint(p1.X, p1.Y) { Angles = new List<Coord>(Angles) };
        return d;
      }

      public static PartPoint Create(IList<Coord> coords)
      {
        Point p = coords[0].GetPoint();
        List<Coord> angles = new List<Coord>(coords);
        angles.RemoveAt(0);
        PartPoint pp = new PartPoint(p.X, p.Y) { Angles = angles };
        return pp;
      }
    }

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
      Flags remove = Flags.firstBezierPoint | Flags.secondBezierPoint | Flags.firstHolePoint;
      Flags c = Code() | remove;
      Flags code = c ^ remove;
      double x = GetGeomPart(_ix);
      double y = GetGeomPart(_iy);
      Point2D p = code == 0 ? new Point2D(x, y) : new CodePoint(x, y) { Flags = code };
      return p;
    }

    public bool IsBezier => (_ix & 1) == 1;
    public bool IsNewRing => (_iy & 2) == 2;

    public Flags Code()
    {
      int code = GetCodePart(_ix);
      return (Flags)(code | (GetCodePart(_iy) << 8));
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


    public static GeoElement.Geom GetGeometry(GeomType type, IList<Coord> coords)
    {
      if (type == GeomType.point || type == GeomType.unformattedText ||
        type == GeomType.formattedText)
      {
        if (coords.Count == 1)
        { return new GeoElement.Point(coords[0].GetPoint()); }
        else if (type == GeomType.point)
        { return new GeoElement.Point(PartPoint.Create(coords)); }
        else
        {
          PointCollection points = new PointCollection();
          foreach (var coord in coords)
          { points.Add(coord.GetPoint()); }
          return new GeoElement.Points(points);
        }
      }
      else if (type == GeomType.line || type == GeomType.lineText)
      {
        Polyline line = GetPolyline(coords);
        return new GeoElement.Line(line);
      }
      else if (type == GeomType.rectangle)
      {
        Polyline line = GetPolyline(coords);
        return new GeoElement.Line(line);
      }
      else if (type == GeomType.area)
      {
        Area pArea = GetArea(coords);
        return new GeoElement.Area(pArea);
      }
      else
      { // unknown geometry type
        if (coords.Count == 1)
        { return new GeoElement.Point(coords[0].GetPoint()); }
        Polyline line = GetPolyline(coords);
        return new GeoElement.Line(line);
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

    public static bool TryEnumCoords(IGeometry geom, out IEnumerable<Coord> coords)
    {
      if (geom is IPoint pt) coords = EnumCoords(pt);
      else if (geom is PointCollection pts) coords = EnumCoords(pts);
      else if (geom is Polyline line) coords = EnumCoords(line);
      else if (geom is Area area) coords = EnumCoords(area);

      else
      {
        coords = null;
        return false;
      }
      return true;
    }
    public static IEnumerable<Coord> EnumCoords(IPoint pt)
    {
      yield return Coord.Create(pt, Coord.Flags.none);
      if (pt is PartPoint ppt)
      {
        foreach (Coord angle in ppt.Angles)
        { yield return angle; }
      }
    }

    public static IEnumerable<Coord> EnumCoords(PointCollection pts)
    {
      foreach (var p in pts)
      { yield return Coord.Create(p, Coord.Flags.none); }
    }

    public static IEnumerable<Coord> EnumCoords(Polyline line)
    {
      foreach (var coord in EnumLineCoords(line, isInnerRing: false))
      { yield return coord; }
    }


    public static IEnumerable<Coord> EnumCoords(Area area)
    {
      bool isInnerRing = false;
      foreach (var border in area.Border)
      {
        foreach (var coord in EnumLineCoords(border, isInnerRing))
        { yield return coord; }
        isInnerRing = true;
      }
    }

    private static IEnumerable<Coord> EnumLineCoords(Polyline polyline, bool isInnerRing)
    {
      Coord coord;

      if (polyline.Points.Count < 1)
      { yield break; }

      if (isInnerRing)
      { coord = Coord.Create(polyline.Points[0], Coord.Flags.firstHolePoint); }
      else
      { coord = Coord.Create(polyline.Points[0], Coord.Flags.none); }
      yield return coord;

      foreach (var seg in polyline.EnumSegments())
      {
        if (seg is Bezier bezier)
        {
          coord = Coord.Create(bezier.P1, Coord.Flags.firstBezierPoint);
          yield return coord;

          coord = Coord.Create(bezier.P2, Coord.Flags.secondBezierPoint);
          yield return coord;
        }

        Flags remove = Flags.firstBezierPoint | Flags.secondBezierPoint | Flags.firstHolePoint;
        Flags c = ((seg.End as CodePoint)?.Flags ?? Flags.none) | remove;
        Flags code = c ^ remove;

        coord = Coord.Create(seg.End, code);
        yield return coord;
      }

    }

  }
}
