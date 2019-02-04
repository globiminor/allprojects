using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace OMapScratch
{
  [XmlRoot("oscratch")]
  public class XmlConfig
  {
    [XmlElement("offset")]
    public XmlOffset Offset { get; set; }

    [XmlElement("data")]
    public XmlData Data { get; set; }

    [XmlElement("image")]
    public List<XmlImage> Images { get; set; }
  }
  public class XmlData
  {
    [XmlAttribute("scratch")]
    public string Scratch { get; set; }
    [XmlAttribute("scratchimg")]
    public string ScratchImg { get; set; }
    [XmlAttribute("symbol")]
    public string Symbol { get; set; }

    private static string Format(float? value)
    {
      if (value == null)
      { return null; }
      return string.Format(CultureInfo.InvariantCulture, "{0}", value);
    }
    private static float? Parse(string value)
    {
      if (float.TryParse(value, out float v)) return v;
      return null;
    }


    [XmlIgnore]
    public float? SymbolScale { get; set; }
    [XmlAttribute("symbolscale")]
    public string SSymbolScale
    {
      get { return Format(SymbolScale); }
      set { SymbolScale = Parse(value); }
    }

    [XmlIgnore]
    public float? ConstrTextSize { get; set; }
    [XmlAttribute("constrtextsize_mm")]
    public string SConstrTextSize
    {
      get { return Format(ConstrTextSize); }
      set { ConstrTextSize = Parse(value); }
    }

    [XmlIgnore]
    public float? ConstrLineWidth { get; set; }
    [XmlAttribute("constrlinewidth_mm")]
    public string SConstrLineWidth
    {
      get { return Format(ConstrLineWidth); }
      set { ConstrLineWidth = Parse(value); }
    }

    [XmlIgnore]
    public float? ElemTextSize { get; set; }
    [XmlAttribute("elemtextsize_pt")]
    public string SElemTextSize
    {
      get { return Format(ElemTextSize); }
      set { ElemTextSize = Parse(value); }
    }

    [XmlIgnore]
    public float? Search { get; set; }
    [XmlAttribute("search")]
    public string SSearch
    {
      get { return Format(Search); }
      set { Search = Parse(value); }
    }

    [XmlAttribute("numberformat")]
    public string NumberFormat { get; set; }
    [XmlElement("constrcolor")]
    public XmlColor ConstrColor { get; set; }
  }
  public class XmlImage
  {
    [XmlAttribute("name")]
    public string Name { get; set; }
    [XmlAttribute("path")]
    public string Path { get; set; }
  }
  public class XmlOffset
  {
    [XmlAttribute("x")]
    public double X { get; set; }
    [XmlAttribute("y")]
    public double Y { get; set; }

    [XmlElement("world")]
    public XmlWorld World { get; set; }

    public XmlWorld GetWorld()
    {
      return World ?? (World = new XmlWorld());
    }

    [XmlAttribute("declination")]
    public string DeclText
    {
      get { return $"{Declination}"; }
      set
      {
        if (double.TryParse(value, out double decl))
        { Declination = decl; }
        else
        { Declination = null; }
      }
    }

    [XmlIgnore()]
    public double? Declination { get; set; }
  }

  public class XmlWorld
  {
    [XmlAttribute("lat")]
    public double Latitude { get; set; }
    [XmlAttribute("lon")]
    public double Longitude { get; set; }
    [XmlAttribute("geoMat00")]
    public double GeoMatrix00 { get; set; }
    [XmlAttribute("geoMat01")]
    public double GeoMatrix01 { get; set; }
    [XmlAttribute("geoMat10")]
    public double GeoMatrix10 { get; set; }
    [XmlAttribute("geoMat11")]
    public double GeoMatrix11 { get; set; }

    public XmlWorld Clone()
    {
      return (XmlWorld)MemberwiseClone();
    }
    //[XmlAttribute("gpsmintime")]
    //public double GpsMinTime { get; set; }
    //[XmlAttribute("gpsmindistance")]
    //public double GpsMinDistance { get; set; }
  }

  public class XmlElems
  {
    public List<XmlElem> Elems { get; set; }

    public static XmlElems Create(IEnumerable<Elem> elems)
    {
      XmlElems created = new XmlElems
      {
        Elems = new List<XmlElem>()
      };
      foreach (var elem in elems)
      { created.Elems.Add(XmlElem.Create(elem)); }

      return created;
    }
  }
  public class XmlElem
  {
    [XmlAttribute("Sid")]
    public string SymbolId { get; set; }
    [XmlAttribute("Cid")]
    public string ColorId { get; set; }
    [XmlAttribute("Geom")]
    public string Geometry { get; set; }

    public static XmlElem Create(Elem elem)
    {
      XmlElem created = new XmlElem
      {
        SymbolId = elem.Symbol?.Id,
        ColorId = elem.Color?.Id,
        Geometry = elem.Geometry.ToText()
      };

      return created;
    }
    public Elem GetElem()
    {
      Elem elem = new Elem { Geometry = DrawableUtils.GetGeometry(Geometry) };
      return elem;
    }
  }

  public class XmlSymbols
  {
    [XmlElement("Color")]
    public List<XmlColor> Colors { get; set; }
    [XmlElement("Symbol")]
    public List<XmlSymbol> Symbols { get; set; }

    public static XmlSymbols Create(IEnumerable<Symbol> symbols)
    {
      XmlSymbols created = new XmlSymbols
      { Symbols = new List<XmlSymbol>() };
      foreach (var symbol in symbols)
      { created.Symbols.Add(XmlSymbol.Create(symbol)); }

      return created;
    }
  }
  public partial class XmlColor
  {
    public XmlColor()
    { }
    public XmlColor(byte r, byte g, byte b)
    {
      Red = r;
      Green = g;
      Blue = b;
    }
    [XmlAttribute("Id")]
    public string Id { get; set; }

    [XmlAttribute("R")]
    public byte Red { get; set; }
    [XmlAttribute("G")]
    public byte Green { get; set; }
    [XmlAttribute("B")]
    public byte Blue { get; set; }

    public static XmlColor Create(ColorRef color)
    {
      XmlColor xml = new XmlColor { Id = color.Id };
      xml.SetEnvColor(color);
      return xml;
    }
    public ColorRef GetColor()
    {
      ColorRef color = new ColorRef { Id = Id };
      GetEnvColor(color);
      return color;
    }

  }
  public class XmlSymbol
  {
    [XmlAttribute("Id")]
    public string Id { get; set; }
    [XmlAttribute("Text")]
    public string Text { get; set; }
    [XmlElement("Curve")]
    public List<XmlSymbolCurve> Curves { get; set; }

    public static XmlSymbol Create(Symbol symbol)
    {
      XmlSymbol created = new XmlSymbol
      {
        Id = symbol.Id,
        Text = symbol.Text,
        Curves = new List<XmlSymbolCurve>()
      };
      foreach (var curve in symbol.Curves)
      {
        created.Curves.Add(XmlSymbolCurve.Create(curve));
      }
      return created;
    }

    public Symbol GetSymbol()
    {
      Symbol sym = new Symbol
      {
        Id = Id,
        Text = Text,
        Curves = new List<SymbolCurve>()
      };
      if (Curves != null)
      {
        foreach (var curve in Curves)
        { sym.Curves.Add(curve.GetCurve()); }
      }
      return sym;
    }
  }
  public class XmlSymbolCurve
  {
    [XmlAttribute("w")]
    public float LineWidth { get; set; }
    [XmlAttribute("f")]
    public bool Fill { get; set; }
    [XmlAttribute("s")]
    public bool Stroke { get; set; }
    [XmlAttribute("dash")]
    public string Dash { get; set; }
    [XmlAttribute("geom")]
    public string Geometry { get; set; }

    public static XmlSymbolCurve Create(SymbolCurve curve)
    {
      XmlSymbolCurve created = new XmlSymbolCurve
      {
        LineWidth = curve.LineWidth,
        Fill = curve.Fill,
        Stroke = curve.Stroke
      };
      if (curve.Dash != null)
      {
        StringBuilder sb = new StringBuilder();
        foreach (var f in curve.Dash.Intervals)
        { sb.Append($"{f:f1},"); }
        sb.Remove(sb.Length - 1, 1);
        if (curve.Dash.EndOffset != 0 || curve.Dash.StartOffset != 0)
        { sb.Append($";{curve.Dash.StartOffset:f1}"); }
        if (curve.Dash.EndOffset != 0)
        { sb.Append($";{curve.Dash.EndOffset:f1}"); }
        created.Dash = sb.ToString();
      }
      IDrawable geom = curve.Curve;
      created.Geometry = geom?.ToText();
      return created;
    }

    internal SymbolCurve GetCurve()
    {
      SymbolCurve curve = new SymbolCurve
      {
        LineWidth = LineWidth,
        Fill = Fill,
        Stroke = Stroke,
        Dash = DrawableUtils.GetDash(Dash),

        Curve = (Curve)DrawableUtils.GetGeometry(Geometry)
      };
      return curve;
    }
  }

  public class XmlRecents
  {
    [XmlElement("recent")]
    public List<string> Recents { get; set; }
  }

}
