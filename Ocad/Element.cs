using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Ocad
{
  sealed public partial class Element
  {
    public int Color { get; set; }       // color or color index
    public int LineWidth { get; set; }   // units = 0.01 mm
    public int Flags { get; set; }

    public Element(bool isGeometryProjected)
    {
      IsGeometryProjected = isGeometryProjected;

      Text = "";
      Index = -1;
    }

    internal double ReservedHeight { get; set; } // Ocad8

    public bool IsGeometryProjected { get; set; }
    public int Symbol { get; set; }

    public ObjectStringType ObjectStringType { get; set; }
    public string ObjectString { get; set; }

    public GeomType Type { get; set; }

    public bool UnicodeText { get; set; }

    public int DeleteSymbol { get; set; }

    public int PointCount()
    {
      return PointCount(Geometry);
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

    public IGeometry Geometry { get; set; }

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

    public static explicit operator Symbol.SymbolGraphics(Element element)
    {
      Symbol.SymbolGraphics pGraphics = new Symbol.SymbolGraphics();

      if (element.Type == GeomType.area)
      { pGraphics.Type = Ocad.Symbol.SymbolGraphicsType.Area; }
      else if (element.Type == GeomType.line)
      {
        pGraphics.Type = Ocad.Symbol.SymbolGraphicsType.Line;
        if (element.LineWidth != 0)
        { pGraphics.LineWidth = element.LineWidth; }
        else
        { pGraphics.LineWidth = 1; } // TODO
      }
      else
      { return null; }

      if (element.Color != 0)
      { pGraphics.Color = element.Color; }
      else
      { pGraphics.Color = 1; } // TODO: get Symbol Color of element

      pGraphics.Geometry = element.Geometry;
      return pGraphics;
    }
  }
}
