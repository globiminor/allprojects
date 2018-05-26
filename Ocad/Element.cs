using System;
using Basics.Geom;

namespace Ocad
{
  /// <summary>
  /// Summary description for Element.
  /// </summary>
  [System.Diagnostics.DebuggerStepThrough]
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

    public int Ox
    {
      get
      { return _ix; }
    }
    public int Oy
    {
      get
      { return _iy; }
    }

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

    public bool IsBezier
    {
      get
      { return (_ix & 1) == 1; }
    }

    public bool IsNewRing
    {
      get
      { return (_iy & 2) == 2; }
    }

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
  }

  public class ElementV9 : Element
  {
    public ElementV9(bool isGeometryProjected)
      : base(isGeometryProjected)
    { }

    public int Color { get; set; }       // color or color index
    public int LineWidth { get; set; }   // units = 0.01 mm
    public int Flags { get; set; }

    public override ElementIndex GetIndex(OcadReader reader)
    {
      ElementIndex index = new ElementIndex(Geometry.Extent)
      { Symbol = Symbol };

      index.CalcElementLength(reader, this);

      index.ObjectType = (short)Type;
      index.Status = 1;
      if (Symbol != -2)
      { index.Color = -1; }
      else
      { index.Color = (short)Color; }

      return index;
    }

    public override int TextCount()
    { return TextCount(Text); }
    public static int TextCount(string text)
    {
      if (text.Length > 0)
      {
        int nText = ((text.Length * 2 / 64) + 1) * 64;
        return nText;
      }
      return 0;
    }
  }
  public class ElementV8 : Element
  {
    private double _reservedHeight;

    public ElementV8(bool isGeometryProjected)
      : base(isGeometryProjected)
    { }

    public double ReservedHeight
    {
      get
      { return _reservedHeight; }
      set
      { _reservedHeight = value; }
    }

    public override ElementIndex GetIndex(OcadReader reader)
    {
      ElementIndex index = new ElementIndex(Geometry.Extent)
      { Symbol = Symbol };
      index.CalcElementLength(reader, this);

      index.ObjectType = (short)Type;
      index.Status = 1;
      index.Color = -1;

      return index;
    }
  }
  public abstract class Element
  {
    public Element(bool isGeometryProjected)
    {
      IsGeometryProjected = isGeometryProjected;

      Text = "";
      Index = -1;
    }

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
        foreach (Polyline pLine in pArea.Border)
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
      foreach (Curve pSeg in line.Segments)
      {
        if (pSeg is Bezier)
        { nPoints += 3; }
        else
        { nPoints += 1; }
      }
      return nPoints;
    }

    public virtual int TextCount()
    {
      if (Text == null || Text.Length == 0)
      { return 0; }

      if (UnicodeText)
      { return Text.Length / 4 + 1; }
      else
      { return Text.Length / 8 + 1; }
    }
    public short ObjectStringCount()
    {
      if ((ObjectString?.Length ?? 0) == 0)
      { return 0; }
      return (short)new System.Text.UnicodeEncoding().GetByteCount(ObjectString);
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

    public abstract ElementIndex GetIndex(OcadReader reader);

    public static explicit operator Symbol.SymbolGraphics(Element element)
    {
      Symbol.SymbolGraphics pGraphics = new Symbol.SymbolGraphics();
      ElementV9 elem9 = element as ElementV9;

      if (element.Type == GeomType.area)
      { pGraphics.Type = Ocad.Symbol.SymbolGraphicsType.Area; }
      else if (element.Type == GeomType.line)
      {
        pGraphics.Type = Ocad.Symbol.SymbolGraphicsType.Line;
        if (elem9 != null && elem9.LineWidth != 0)
        { pGraphics.LineWidth = elem9.LineWidth; }
        else
        { pGraphics.LineWidth = 1; } // TODO
      }
      else
      { return null; }

      if (elem9 != null && elem9.Color != 0)
      { pGraphics.Color = elem9.Color; }
      else
      { pGraphics.Color = 1; } // TODO: get Symbol Color of element

      pGraphics.Geometry = element.Geometry;
      return pGraphics;
    }
  }
}
