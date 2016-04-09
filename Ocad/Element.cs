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
    public enum Flags
    {
      none = 0,
      firstBezierPoint = 1,
      secondBezierPoint = 2,
      noLeftLine = 4,
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
      Coord coord = new Coord();

      coord._ix = (((int)point.X) << 8) | ((int)code % 256);
      coord._iy = (((int)point.Y) << 8) | ((int)code >> 8);

      return coord;
    }

    public Point2D GetPoint()
    {
      Point2D p = new Point2D();
      p.X = GetGeomPart(_ix);
      p.Y = GetGeomPart(_iy);
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
      return string.Format("{0,7} {1,7}   {2}   {3}",
        GetGeomPart(_ix), GetGeomPart(_iy),
        CodeString(GetCodePart(_ix)), CodeString(GetCodePart(_iy)));
    }
  }

  public class ElementV9 : Element
  {
    private int _color; // color or color index
    private int _lineWidth; // units = 0.01 mm
    private int _flags;

    public ElementV9(bool isGeometryProjected)
      : base(isGeometryProjected)
    { }

    public override ElementIndex GetIndex()
    {
      ElementIndex index = new ElementIndex(Geometry.Extent);

      index.Symbol = Symbol;
      int nText = TextCount();

      index.Length = 40 + 8 * PointCount() + nText;

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

    public int Color
    {
      get { return _color; }
      set { _color = value; }
    }
    public int LineWidth
    {
      get { return _lineWidth; }
      set { _lineWidth = value; }
    }
    public int Flags
    {
      get { return _flags; }
      set { _flags = value; }
    }
  }
  public class ElementV8 : Element
  {
    private double mdReservedHeight;

    public ElementV8(bool isGeometryProjected)
      : base(isGeometryProjected)
    { }

    public double ReservedHeight
    {
      get
      { return mdReservedHeight; }
      set
      { mdReservedHeight = value; }
    }

    public override ElementIndex GetIndex()
    {
      ElementIndex index = new ElementIndex(Geometry.Extent);

      index.Symbol = Symbol;
      index.Length = PointCount() + TextCount();

      index.ObjectType = (short)Type;
      index.Status = 1;
      index.Color = -1;

      return index;
    }
  }
  public abstract class Element
  {
    private int _symbol;
    private int _deleteSymbol;
    private IGeometry _geometry;
    private GeomType _type;
    private string _text;
    private bool _unicodeText;

    private double _angle;
    private bool _isProjected;

    private int _index;
    private double _height;

    public Element(bool isGeometryProjected)
    {
      _isProjected = isGeometryProjected;

      _text = "";
      _index = -1;
    }

    public bool IsGeometryProjected
    {
      get { return _isProjected; }
      set { _isProjected = value; }
    }
    public int Symbol
    {
      get { return _symbol; }
      set { _symbol = value; }
    }

    public GeomType Type
    {
      [System.Diagnostics.DebuggerStepThrough]
      get
      { return _type; }
      [System.Diagnostics.DebuggerStepThrough]
      set
      { _type = value; }
    }

    public bool UnicodeText
    {
      get
      { return _unicodeText; }
      set
      { _unicodeText = value; }
    }

    public int DeleteSymbol
    {
      get
      { return _deleteSymbol; }
      set
      { _deleteSymbol = value; }
    }

    public int PointCount()
    {
      return PointCount(_geometry);
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
      if (_text == null || _text.Length == 0)
      { return 0; }

      if (_unicodeText)
      { return _text.Length / 4 + 1; }
      else
      { return _text.Length / 8 + 1; }
    }

    public int Index
    {
      get { return _index; }
      set { _index = value; }
    }

    public IGeometry Geometry
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _geometry; }
      [System.Diagnostics.DebuggerStepThrough]
      set { _geometry = value; }
    }

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    public double Angle
    {
      get { return _angle; }
      set { _angle = value; }
    }

    public double Height
    {
      get { return _height; }
      set { _height = value; }
    }

    public override string ToString()
    {
      string s = string.Format(
        "Symbol      : {0,5}\n" +
        "Object Type : {1}\n" +
        "Text        : {2}\n" +
        "Angle [°]   : {3,5}\n" +
        "# of Points : {4,5}\n", _symbol, _type, _text,
        _angle * 180 / Math.PI, PointCount());
      //      n = element->nPoint;
      //      for (i = 0; i < n; i++) 
      //      {
      //        OCListCoord(fout,element->points + i);
      //      }

      return s;
    }

    public abstract ElementIndex GetIndex();

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
