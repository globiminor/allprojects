using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocad
{
  public class GeoElement:Element
  {
    public GeoElement(IGeometry geometry)
    {
      Geometry = geometry;
    }

    public IGeometry Geometry { get; set; }

    [Obsolete("use only internally")]
    public GeoElement() { }
    internal override void InitGeometry(IList<Coord> coords, Setup setup)
    {
      Geometry = Coord.GetGeometry(Type, coords).Project(setup.Map2Prj);
    }
    internal override bool NeedsSetup => true;

    internal override IEnumerable<Coord> GetCoords(Setup setup)
    {
      return Coord.EnumCoords(Geometry.Project(setup.Prj2Map));
    }

    internal override int PointCountCore()
    {
      return Coord.EnumCoords(Geometry).Count();
    }

  }
  public class MapElement:Element
  {
    private List<Coord> _coords;
    public MapElement(IEnumerable<Coord> coords)
    {
      _coords = new List<Coord>(coords);
    }

    public MapElement() { }
    internal override void InitGeometry(IList<Coord> coords, Setup setup)
    {
      _coords = new List<Coord>(coords);
    }
    internal override bool NeedsSetup => false;

    internal override IEnumerable<Coord> GetCoords(Setup setup)
    {
      return _coords;
    }

    internal override int PointCountCore()
    {
      return _coords.Count;
    }

    public IGeometry GetMapGeometry()
    {
      return (_coords.Count == 0) ? null : Coord.GetGeometry(Type, _coords);
    }

    public static explicit operator Symbol.SymbolGraphics(MapElement element)
    {
      Symbol.SymbolGraphics graphics = new Symbol.SymbolGraphics();

      if (element.Type == GeomType.area)
      { graphics.Type = Ocad.Symbol.SymbolGraphicsType.Area; }
      else if (element.Type == GeomType.line)
      {
        graphics.Type = Ocad.Symbol.SymbolGraphicsType.Line;
        if (element.LineWidth != 0)
        { graphics.LineWidth = element.LineWidth; }
        else
        { graphics.LineWidth = 1; } // TODO
      }
      else
      { return null; }

      if (element.Color != 0)
      { graphics.Color = element.Color; }
      else
      { graphics.Color = 1; } // TODO: get Symbol Color of element

      graphics.MapGeometry = element.GetMapGeometry();
      return graphics;
    }

  }
  public abstract class Element
  {
    public int Color { get; set; }       // color or color index
    public int LineWidth { get; set; }   // units = 0.01 mm
    public int Flags { get; set; }

    public Element()
    {
      Text = "";
      Index = -1;
    }

    internal abstract void InitGeometry(IList<Coord> coords, Setup setup);
    internal abstract bool NeedsSetup { get; }
    internal abstract IEnumerable<Coord> GetCoords(Setup setup);

    internal double ReservedHeight { get; set; } // Ocad8

    public int Symbol { get; set; }

    public ObjectStringType ObjectStringType { get; set; }
    public string ObjectString { get; set; }

    public GeomType Type { get; set; }

    public bool UnicodeText { get; set; }

    public int DeleteSymbol { get; set; }

    public int PointCount()
    { return PointCountCore(); }
    internal abstract int PointCountCore();

    internal IBox GetExent(Setup setup)
    {
      PointCollection coords = new PointCollection();
      coords.AddRange(GetCoords(setup).Select(x => x.GetPoint()));
      return coords.Extent;
    }
    internal static int PointCount(IGeometry geometry)
    {
      if (geometry is Point)
      { return 1; }
      else if (geometry is Polyline)
      {
        Polyline pLine = geometry as Polyline;
        return PointCount(pLine);
      }
      else if (geometry is Area)
      {
        Area pArea = geometry as Area;
        int nPoints = 0;
        foreach (var pLine in pArea.Border)
        { nPoints += PointCount(pLine); }
        return nPoints;
      }
      else if (geometry is PointCollection)
      {
        PointCollection pList = geometry as PointCollection;
        return pList.Count;
      }
      else
      {
        throw new Exception(
          "Unhandled geometry " + geometry.GetType());
      }
    }
    private static int PointCount(Polyline line)
    {
      int nPoints = 1;
      foreach (var pSeg in line.Segments)
      {
        if (pSeg is Bezier)
        { nPoints += 3; }
        else
        { nPoints += 1; }
      }
      return nPoints;
    }

    public int Index { get; set; }

    public string Text { get; set; }

    public double Angle { get; set; }

    public double Height { get; set; }

    public int ServerObjectId { get; set; }
    private DateTime? _creationDate;
    public DateTime CreationDate
    {
      get { return _creationDate ?? (_creationDate = DateTime.Now).Value; }
      set { _creationDate = value; }
    }
    private DateTime? _modifyDate;
    public DateTime ModificationDate
    {
      get { return _modifyDate ?? (_creationDate = CreationDate).Value; }
      set { _modifyDate = value; }
    }
    public int MultirepresenationId { get; set; }

    public override string ToString()
    {
      if (ObjectStringType == ObjectStringType.None)
      {
        string s = string.Format(
          "Symbol      : {0,5}\n" +
          "Object Type : {1}\n" +
          "Text        : {2}\n" +
          "Angle [°]   : {3,5}\n" +
          "# of Points : {4,5}\n", Symbol, Type, Text,
          Angle * 180 / Math.PI, PointCount());
        //      n = element->nPoint;
        //      for (i = 0; i < n; i++) 
        //      {
        //        OCListCoord(fout,element->points + i);
        //      }
        return s;
      }
      else
      {
        return $"Symbol: {Symbol,5}; ObjTyp: {Type}; CsTyp: {ObjectStringType}; CsTxt: {ObjectString}";
      }
    }
  }
}
