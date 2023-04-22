using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocad
{
  public class GeoElement : Element
  {
    public abstract class Geom
    {
      internal Geom() { }
      public abstract IEnumerable<Coord> EnumCoords();
      public abstract IEnumerable<IPoint> EnumPoints();
      public Geom Project(IProjection prj) => ProjectCore(prj);
      public IBox Extent { get { return GetExtent(); } }
      public abstract IGeometry GetGeometry();

      protected abstract Geom ProjectCore(IProjection prj);
      protected abstract IBox GetExtent();

      public static Geom Create(object geom)
      {
        if (geom is Basics.Geom.Surface area) return new Surface(area);
        if (geom is Polyline l) return new Line(l);
        if (geom is PointCollection ps) return new Points(ps);
        if (geom is IPoint p) return new Point(p);

        throw new NotImplementedException("Unhandled type " + geom.GetType());
      }
    }
    public sealed class Point : Geom
    {
      public Point(IPoint pnt) { BaseGeometry = pnt; }
      public IPoint BaseGeometry { get; }

      public override IEnumerable<Coord> EnumCoords() => Coord.EnumCoords(BaseGeometry);
      public override IEnumerable<IPoint> EnumPoints()
      {
        yield return BaseGeometry;
      }

      protected override Geom ProjectCore(IProjection prj) => Project(prj);
      public new Point Project(IProjection prj) => new Point(prj.Project(BaseGeometry));

      protected override IBox GetExtent() => Basics.Geom.Point.CastOrWrap(BaseGeometry);
      public override IGeometry GetGeometry() => Basics.Geom.Point.CastOrWrap(BaseGeometry);
    }
    public sealed class Points : Geom
    {
      public Points(PointCollection pnts) { BaseGeometry = pnts; }
      public PointCollection BaseGeometry { get; }

      public override IEnumerable<Coord> EnumCoords() => Coord.EnumCoords(BaseGeometry);
      public override IEnumerable<IPoint> EnumPoints()
      {
        foreach (var pt in BaseGeometry)
          yield return pt;
      }

      protected override Geom ProjectCore(IProjection prj) => Project(prj);
      public new Points Project(IProjection prj) => new Points(BaseGeometry.Project(prj));
      protected override IBox GetExtent() => BaseGeometry.Extent;
      public override IGeometry GetGeometry() => BaseGeometry;
    }
    public sealed class Line : Geom
    {
      public Line(Polyline line) { BaseGeometry = line; }
      public Polyline BaseGeometry { get; }

      public override IEnumerable<Coord> EnumCoords() => Coord.EnumCoords(BaseGeometry);
      public override IEnumerable<IPoint> EnumPoints() => BaseGeometry.Points;

      protected override Geom ProjectCore(IProjection prj) => Project(prj);
      public new Line Project(IProjection prj) => new Line(BaseGeometry.Project(prj));
      protected override IBox GetExtent() => BaseGeometry.Extent;
      public override IGeometry GetGeometry() => BaseGeometry;
    }
    public sealed class Surface : Geom
    {
      public Surface(Basics.Geom.Surface area) { BaseGeometry = area; }
      public Basics.Geom.Surface BaseGeometry { get; }

      public override IEnumerable<Coord> EnumCoords() => Coord.EnumCoords(BaseGeometry);
      public override IEnumerable<IPoint> EnumPoints()
      {
        foreach (var border in BaseGeometry.Border)
          foreach (var pt in border.Points)
            yield return pt;
      }

      protected override Geom ProjectCore(IProjection prj) => Project(prj);
      public new Surface Project(IProjection prj) => new Surface(BaseGeometry.Project(prj));
      protected override IBox GetExtent() => BaseGeometry.Extent;
      public override IGeometry GetGeometry() => BaseGeometry;
    }

    public GeoElement(IPoint point) : this(new Point(point)) { Type = GeomType.point; }
    public GeoElement(PointCollection points) : this(new Points(points)) { }
    public GeoElement(Polyline line) : this(new Line(line)) { Type = GeomType.line; }
    public GeoElement(Basics.Geom.Surface area) : this(new Surface(area)) { Type = GeomType.area; }

    public GeoElement(Geom geometry)
    {
      Geometry = geometry;
    }

    [Obsolete("Use only internally")]
    public GeoElement() { }

    public Geom Geometry { get; set; }

    // public void SetGeometry(GeoElement elem) { Geometry = elem.Geometry; }
    //public void SetGeometry(IPoint point) { Geometry = new Point(point); }
    //public void SetGeometry(Polyline line) { Geometry = new Line(line); }
    //public void SetGeometry(PointCollection points) { Geometry = new Points(points); }
    //public void SetGeometry(Basics.Geom.Area area) { Geometry = new Area(area); }
    //public bool TrySetGeometry(IGeometry geom)
    //{
    //  if (geom is IPoint pt) SetGeometry(Point.CastOrCreate(pt));
    //  else if (geom is PointCollection pts) SetGeometry(pts);
    //  else if (geom is Polyline line) SetGeometry(line);
    //  else if (geom is Area area) SetGeometry(area);

    //  else
    //  {
    //    return false;
    //  }
    //  return true;

    //}

    internal override void InitGeometry(IList<Coord> coords, Setup setup)
    {
      Geometry = Coord.GetGeometry(Type, coords).Project(setup.Map2Prj);
    }
    internal override bool NeedsSetup => true;

    internal override IEnumerable<Coord> GetCoords(Setup setup)
    {
      return Geometry.Project(setup.Prj2Map).EnumCoords();
    }

    internal override int PointCountCore()
    {
      return Geometry.EnumCoords().Count();
    }

  }
  public class MapElement : Element
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

    public GeoElement.Geom GetMapGeometry()
    {
      return (_coords.Count == 0) ? null : Coord.GetGeometry(Type, _coords);
    }
    public void SetMapGeometry(GeoElement.Geom geom)
    {
      _coords.Clear();
      _coords.AddRange(geom.EnumCoords());
    }

    public void SetText(string text, System.Drawing.Graphics graphics = null, System.Drawing.Font font = null)
    {
      if (text == Text)
      { return; }
      if (graphics == null)
      {
        using (System.Drawing.Bitmap bmpFont = new System.Drawing.Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
        using (System.Drawing.Graphics tempGrp = System.Drawing.Graphics.FromImage(bmpFont))
        {
          SetText(text, tempGrp, font);
          return;
        }
      }

      if (font == null)
      {
        string fontName = (BaseSymbol as Symbol.TextSymbol)?.FontName ?? System.Drawing.FontFamily.GenericSansSerif.Name;
        using (System.Drawing.Font tempFont = new System.Drawing.Font(fontName, 12))
        {
          SetText(text, graphics, tempFont);
          return;
        }
      }

      string orig = Text;
      float origW = MeasureWidth(orig, graphics, font);
      float textW = MeasureWidth(orig, graphics, font);

      GeoElement.Geom geom = GetMapGeometry();
      PointCollection pts = (PointCollection)geom.GetGeometry();
      double f = textW / origW;
      double fullWidth = pts[2].X - pts[0].X;
      ((Point)pts[2]).X = pts[0].X + f * fullWidth;
      ((Point)pts[3]).X = pts[2].X;
      SetMapGeometry(GeoElement.Geom.Create(pts));
      Text = text;
    }

    private static float MeasureWidth(string text, System.Drawing.Graphics graphics, System.Drawing.Font font)
    {
      float w = 0;
      foreach (string part in text.Split('\r', '\n'))
      {
        w = Math.Max(w, graphics.MeasureString(part, font).Width);
      }
      return w;
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
    public Symbol.BaseSymbol BaseSymbol { get; set; }

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
      else if (geometry is Surface)
      {
        Surface pArea = geometry as Surface;
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
      foreach (var pSeg in line.EnumSegments())
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
