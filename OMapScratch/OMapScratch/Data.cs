using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace OMapScratch
{
  public partial interface ISegment
  {
    Pnt From { get; }
    Pnt To { get; }

    void InitToText(StringBuilder sb);
    void AppendToText(StringBuilder sb);
  }

  public partial interface IDrawable
  {
    string ToText();
    IEnumerable<Pnt> GetVertices();
  }

  public partial class ColorRef
  {
    public string Id { get; set; }
  }

  public partial class MapVm
  {
    private Map _map;

    public event System.ComponentModel.CancelEventHandler Saving;
    public event EventHandler Saved;
    public event System.ComponentModel.CancelEventHandler Loading;
    public event EventHandler Loaded;

    public MapVm(Map map)
    {
      _map = map;
    }

    public IEnumerable<XmlImage> Images
    {
      get
      {
        if (_map.Images == null)
        { yield break; }

        foreach (XmlImage img in _map.Images)
        { yield return img; }
      }
    }

    public IEnumerable<Elem> Elems
    { get { return _map.Elems; } }

    internal void AddPoint(float x, float y, Symbol symbol, ColorRef color)
    { _map.AddPoint(x, y, symbol, color); }
    internal void CommitCurrentCurve()
    { _map.CommitCurrentCurve(); }
    internal void Undo()
    { _map.Undo(); }
    internal void Redo()
    { _map.Redo(); }

    public float[] GetOffset()
    { return _map.GetOffset(); }
    public float[] GetCurrentWorldMatrix()
    { return _map.GetCurrentWorldMatrix(); }
    public List<Symbol> GetSymbols()
    { return _map.GetSymbols(); }
    public List<ColorRef> GetColors()
    { return _map.GetColors(); }



    internal void Save()
    {
      if (Utils.Cancel(this, Saving))
      { return; }
      _map.Save();
      Saved?.Invoke(this, null);
    }

    public void Load(string configPath)
    {
      Save();

      XmlConfig config;
      using (TextReader r = new StreamReader(configPath))
      {
        Serializer.Deserialize(out config, r);
      }
      if (config != null)
      {
        if (Utils.Cancel(this, Loading))
        { return; }
        _map.Load(configPath, config);
        Loaded?.Invoke(this, null);
      }
    }
  }
  public partial class Map
  {
    private List<Elem> _elems;
    private List<Symbol> _symbols;
    private List<ColorRef> _colors;

    private Curve _currentCurve;
    private string _currentImagePath;

    private XmlConfig _config;
    private string _configPath;

    internal IList<XmlImage> Images
    {
      get { return _config?.Images; }
    }

    public IEnumerable<Elem> Elems
    {
      get
      {
        if (_elems == null)
        { yield break; }

        foreach (Elem elem in _elems)
        { yield return elem; }
      }
    }

    public float[] GetOffset()
    {
      XmlOffset offset = _config?.Offset;
      if (offset == null)
      { return null; }
      return new float[] { (float)offset.X, (float)offset.Y };
    }

    public float[] GetCurrentWorldMatrix()
    {
      if (string.IsNullOrEmpty(_currentImagePath))
      { return null; }

      string ext = Path.GetExtension(_currentImagePath);
      if (string.IsNullOrEmpty(ext) || ext.Length < 3)
      { return null; }

      string worldExt = $"{ext.Substring(0, 2)}{ext[ext.Length - 1]}w";
      string worldFile = Path.ChangeExtension(_currentImagePath, worldExt);

      if (!File.Exists(worldFile))
      { return null; }

      using (TextReader r = new StreamReader(worldFile))
      {
        float x00, x01, x10, x11, dx, dy;
        if (!float.TryParse(r.ReadLine(), out x00)) return null;
        if (!float.TryParse(r.ReadLine(), out x01)) return null;
        if (!float.TryParse(r.ReadLine(), out x10)) return null;
        if (!float.TryParse(r.ReadLine(), out x11)) return null;
        if (!float.TryParse(r.ReadLine(), out dx)) return null;
        if (!float.TryParse(r.ReadLine(), out dy)) return null;

        return new float[] { x00, x01, x10, x11, dx, dy };
      }
    }

    public void AddPoint(float x, float y, Symbol sym, ColorRef color)
    {
      if (_elems == null)
      { _elems = new List<Elem>(); }

      Pnt p = new Pnt { X = x, Y = y };

      SymbolType symTyp = sym.GetSymbolType();
      if (symTyp == SymbolType.Line)
      {
        if (_currentCurve == null)
        {
          _currentCurve = new Curve().MoveTo(x, y);
          _elems.Add(new Elem { Geometry = _currentCurve, Symbol = sym, Color = color });
        }
        else
        { _currentCurve.LineTo(x, y); }
      }
      else if (symTyp == SymbolType.Point)
      {
        CommitCurrentCurve();
        _elems.Add(new Elem { Geometry = p, Symbol = sym, Color = color });
      }
      else if (symTyp == SymbolType.Text)
      {
        _elems.Add(new Elem { Geometry = p, Symbol = sym, Color = color });
      }
      _redoElems = null;
      _redoSegments = null;
    }
    public void CommitCurrentCurve()
    {
      if (_currentCurve != null && _currentCurve.Count == 0 && _elems[_elems.Count - 1].Geometry == _currentCurve)
      { _elems.RemoveAt(_elems.Count - 1); }
      _currentCurve = null;
    }

    private List<ISegment> _redoSegments;
    private List<Elem> _redoElems;
    public void Undo()
    {
      if (_currentCurve != null)
      {
        int i = _currentCurve.Count - 1;
        if (i >= 0)
        {
          _redoSegments = _redoSegments ?? new List<ISegment>();
          _redoSegments.Add(_currentCurve[i]);
          _currentCurve.RemoveLast();
        }
        else
        { CommitCurrentCurve(); }
      }
      else if (_elems != null)
      {
        _redoSegments = null;
        _redoElems = _redoElems ?? new List<Elem>();
        int i = _elems.Count - 1;
        if (i >= 0)
        {
          _redoElems.Add(_elems[i]);
          _elems.RemoveAt(i);
        }
      }
    }

    public void Redo()
    {
      if (_redoSegments?.Count > 0 && _currentCurve != null)
      {
        int i = _redoSegments.Count - 1;
        _currentCurve.Append(_redoSegments[i]);
        _redoSegments.RemoveAt(i);
      }
      else if (_redoElems?.Count > 0 && _currentCurve == null)
      {
        int i = _redoElems.Count - 1;
        _elems.Add(_redoElems[i]);
        _redoElems.RemoveAt(i);
      }
    }

    public void Save()
    {
      if (_elems == null)
      { return; }
      string path = GetLocalPath(_config?.Data?.Scratch);
      if (path == null)
      { return; }

      using (var w = new StreamWriter(path))
      {
        Serializer.Serialize(XmlElems.Create(_elems), w);
        w.Flush();
        w.Close();
      }
    }

    public void Load(string configPath, XmlConfig config)
    {
      _currentImagePath = null;

      _configPath = configPath;
      _config = config;

      _colors = null;
      _symbols = null;
      _elems = null;
      LoadSymbols();
      _elems = LoadElems();
    }

    private List<Elem> LoadElems()
    {
      string elemsPath = VerifyLocalPath(_config?.Data?.Scratch);
      if (string.IsNullOrEmpty(elemsPath))
      { return null; }
      XmlElems xml;
      using (TextReader r = new StreamReader(elemsPath))
      { Serializer.Deserialize(out xml, r); }
      if (xml?.Elems == null)
      { return null; }

      Dictionary<string, ColorRef> colorDict = new Dictionary<string, ColorRef>();
      foreach (ColorRef c in GetColors())
      { colorDict[c.Id] = c; }

      Dictionary<string, Symbol> symbolDict = new Dictionary<string, Symbol>();
      foreach (Symbol s in GetSymbols())
      { symbolDict[s.Id] = s; }

      List<Elem> elems = new List<Elem>();
      foreach (XmlElem xmlElem in xml.Elems)
      {
        Elem elem = xmlElem.GetElem();
        Symbol sym;
        if (symbolDict.TryGetValue(xmlElem.SymbolId ?? "", out sym))
        { elem.Symbol = sym; }
        ColorRef clr;
        if (colorDict.TryGetValue(xmlElem.ColorId ?? "", out clr))
        { elem.Color = clr; }

        elems.Add(elem);
      }
      return elems;
    }
    private void LoadSymbols()
    {
      string symbolPath = VerifyLocalPath(_config?.Data?.Symbol);
      if (string.IsNullOrEmpty(symbolPath))
      { return; }
      XmlSymbols xml;
      using (TextReader r = new StreamReader(symbolPath))
      { Serializer.Deserialize(out xml, r); }

      _symbols = ReadSymbols(xml?.Symbols);
      _colors = ReadColors(xml?.Colors);
    }

    private List<Symbol> ReadSymbols(List<XmlSymbol> xmlSymbols)
    {
      if (xmlSymbols == null)
      { return null; }
      List<Symbol> symbols = new List<Symbol>();
      foreach (XmlSymbol xml in xmlSymbols)
      { symbols.Add(xml.GetSymbol()); }
      return symbols;
    }

    private List<ColorRef> ReadColors(List<XmlColor> xmlColors)
    {
      if (xmlColors == null)
      { return null; }
      List<ColorRef> colors = new List<ColorRef>();
      foreach (XmlColor xml in xmlColors)
      { colors.Add(xml.GetColor()); }
      return colors;
    }

    public List<Symbol> GetSymbols()
    {
      return _symbols ?? (_symbols = GetDefaultSymbols());
    }

    public List<ColorRef> GetColors()
    {
      return _colors ?? (_colors = GetDefaultColors());
    }

    private List<Symbol> GetDefaultSymbols()
    {
      return new List<Symbol> {
        new Symbol {
          Id = "L1",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 1 },
          }
        },
        new Symbol {
          Id = "L2",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 2 },
          }
        },
        new Symbol {
          Id = "L4",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 4 },
          }
        },
        new Symbol {
          Id = "L8",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 8 },
          }
        },
        new Symbol {
          Id = "KrG",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { Curve = new Curve().Circle(0,0,10) },
          }
        },
        new Symbol {
          Id = "KrK",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { Curve = new Curve().Circle(0,0,5) },
          }
        },
        new Symbol {
          Id = "RiG",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 3 , Curve = new Curve().Circle(0,0,16), }
          }
        },
        new Symbol {
          Id = "RiK",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 3 ,Curve = new Curve().Circle(0,0,8), }
          }
        },
        new Symbol {
          Id = "KzG",
          Curves = new List<SymbolCurve> {
            new SymbolCurve { LineWidth = 3,Curve = new Curve().MoveTo(-10,-10).LineTo(10,10) },
            new SymbolCurve { LineWidth = 3,Curve = new Curve().MoveTo(10,-10).LineTo(-10,10) }
          }
        },
        new Symbol {
          Id = "TxA",
          Text = "A"
        },
        new Symbol {
          Id = "Tx1",
          Text = "1"
        },
      };
    }

    private string GetLocalPath(string path)
    {
      if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(_configPath))
      { return null; }
      string dir = Path.GetDirectoryName(_configPath);
      if (!Directory.Exists(dir))
      { return null; }
      string fullPath = Path.Combine(dir, Path.GetFileName(path));
      return fullPath;
    }

    internal string VerifyLocalPath(string path)
    {
      string fullPath = GetLocalPath(path);
      if (fullPath == null || !File.Exists(fullPath))
      { return null; }
      return fullPath;
    }
  }

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
    [XmlAttribute("symbol")]
    public string Symbol { get; set; }
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
  }

  public class XmlElems
  {
    public List<XmlElem> Elems { get; set; }

    public static XmlElems Create(IEnumerable<Elem> elems)
    {
      XmlElems created = new XmlElems();
      created.Elems = new List<XmlElem>();
      foreach (Elem elem in elems)
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
      XmlElem created = new XmlElem();
      created.SymbolId = elem.Symbol?.Id;
      created.ColorId = elem.Color?.Id;
      created.Geometry = elem.Geometry.ToText();

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
      XmlSymbols created = new XmlSymbols();
      created.Symbols = new List<XmlSymbol>();
      foreach (Symbol symbol in symbols)
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
      XmlSymbol created = new XmlSymbol();
      created.Id = symbol.Id;
      created.Text = symbol.Text;
      created.Curves = new List<XmlSymbolCurve>();
      foreach (SymbolCurve curve in symbol.Curves)
      {
        created.Curves.Add(XmlSymbolCurve.Create(curve));
      }
      return created;
    }

    public Symbol GetSymbol()
    {
      Symbol sym = new Symbol();
      sym.Id = Id;
      sym.Text = Text;
      sym.Curves = new List<SymbolCurve>();
      if (Curves != null)
      {
        foreach (XmlSymbolCurve curve in Curves)
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
    [XmlAttribute("geom")]
    public string Geometry { get; set; }

    public static XmlSymbolCurve Create(SymbolCurve curve)
    {
      XmlSymbolCurve created = new XmlSymbolCurve();
      created.LineWidth = curve.LineWidth;
      created.Fill = curve.Fill;
      created.Stroke = curve.Stroke;
      IDrawable geom = curve.Curve;
      created.Geometry = geom?.ToText();
      return created;
    }

    internal SymbolCurve GetCurve()
    {
      SymbolCurve curve = new SymbolCurve();
      curve.LineWidth = LineWidth;
      curve.Fill = Fill;
      curve.Stroke = Stroke;

      curve.Curve = (Curve)DrawableUtils.GetGeometry(Geometry);
      return curve;
    }
  }

  public static class Serializer
  {
    public static void Serialize<T>(T obj, TextWriter writer)
    {
      XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      ns.Add(string.Empty, string.Empty);
      XmlSerializer ser = new XmlSerializer(typeof(T));

      ser.Serialize(writer, obj, ns);
    }

    public static void Deserialize<T>(out T obj, TextReader reader)
    {
      XmlSerializer ser = new XmlSerializer(typeof(T));
      object o = ser.Deserialize(reader);
      obj = (T)o;
    }
  }

  public static class DrawableUtils
  {
    public const string Point = "p";
    public const string Circle = "r";
    public const string MoveTo = "m";
    public const string LineTo = "l";
    public const string CubicTo = "c";

    internal static IDrawable GetGeometry(string geometry)
    {
      if (string.IsNullOrEmpty(geometry))
      { return null; }

      IList<string> parts = geometry.Split();
      IDrawable d = null;
      int i = 0;
      while (i < parts.Count)
      {
        string part = parts[i];
        if (string.IsNullOrEmpty(part))
        { i++; }
        else if (part == Point)
        {
          if (d != null)
          { throw new InvalidOperationException(); }
          Pnt p = new Pnt();
          p.X = float.Parse(parts[i + 1]);
          p.Y = float.Parse(parts[i + 2]);
          i += 3;
          d = p;
        }
        else if (part == Circle)
        {
          Curve c = (Curve)d ?? new Curve();
          c.Append(new Circle
          {
            Center = new Pnt { X = float.Parse(parts[i + 1]), Y = float.Parse(parts[i + 2]) },
            Radius = float.Parse(parts[i + 3])
          });
          i += 4;
          d = c;
        }
        else if (part == MoveTo)
        {
          if (d != null)
          { throw new InvalidOperationException(); }
          Curve c = new Curve();
          c.MoveTo(float.Parse(parts[i + 1]), float.Parse(parts[i + 2]));
          i += 3;
          d = c;
        }
        else if (part == LineTo)
        {
          Curve c = (Curve)d;
          c.LineTo(float.Parse(parts[i + 1]), float.Parse(parts[i + 2]));
          i += 3;
        }
        else if (part == CubicTo)
        {
          Curve c = (Curve)d;
          c.CubicTo(
            float.Parse(parts[i + 1]), float.Parse(parts[i + 2]),
            float.Parse(parts[i + 3]), float.Parse(parts[i + 4]),
            float.Parse(parts[i + 5]), float.Parse(parts[i + 6])
            );
          i += 3;
        }
        else
        {
          throw new NotImplementedException();
        }
      }
      return d;
    }
  }
  public class Elem
  {
    public IDrawable Geometry { get; set; }
    public Symbol Symbol { get; set; }
    public ColorRef Color { get; set; }
  }

  public partial class Pnt : IDrawable
  {
    public float X { get; set; }
    public float Y { get; set; }

    string IDrawable.ToText()
    {
      return $"{DrawableUtils.Point} {X:f1} {Y:f1}";
    }

    IEnumerable<Pnt> IDrawable.GetVertices()
    {
      yield return new Pnt { X = X, Y = Y };
    }
  }

  public partial class Lin : ISegment
  {
    public Pnt From { get; set; }
    public Pnt To { get; set; }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.LineTo} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Circle : ISegment
  {
    public Pnt Center { get; set; }
    public float Radius { get; set; }

    Pnt ISegment.To { get { return new Pnt { X = Center.X, Y = Center.Y + Radius }; } }
    Pnt ISegment.From { get { return new Pnt { X = Center.X, Y = Center.Y + Radius }; } }

    void ISegment.InitToText(StringBuilder sb)
    { }
    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.Circle} {Center.X:f1} {Center.Y:f1} {Radius:f1}"); }
  }

  public partial class Bezier : ISegment
  {
    public Pnt From { get; set; }
    public Pnt I0 { get; set; }
    public Pnt I1 { get; set; }
    public Pnt To { get; set; }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.CubicTo} {I0.X:f1} {I0.Y:f1} {I1.X:f1} {I1.Y:f1} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Curve : List<ISegment>, IDrawable
  {
    private float _tx, _ty;
    public Curve MoveTo(float x, float y)
    {
      _tx = x;
      _ty = y;
      return this;
    }

    public Pnt From
    {
      get
      {
        if (Count > 0)
        { return this[0].From; }
        return new Pnt { X = _tx, Y = _ty };
      }
    }
    public void Append(ISegment segment)
    {
      Add(segment);
      Pnt end = segment.To;
      _tx = end.X;
      _ty = end.Y;
    }

    public Curve LineTo(float x, float y)
    {
      AddSegment(new Lin { From = new Pnt { X = _tx, Y = _ty }, To = new Pnt { X = x, Y = y } });
      return this;
    }

    public Curve CubicTo(float tx0, float ty0, float tx1, float ty1, float x, float y)
    {
      AddSegment(new Bezier
      {
        From = new Pnt { X = _tx, Y = _ty },
        I0 = new Pnt { X = tx0, Y = ty0 },
        I1 = new Pnt { X = tx1, Y = ty1 },
        To = new Pnt { X = x, Y = y }
      });
      return this;
    }

    public Curve Circle(float centerX, float centerY, float radius)
    {
      AddSegment(new Circle { Center = new Pnt { X = centerX, Y = centerY }, Radius = radius });
      return this;
    }
    private void AddSegment(ISegment seg)
    {
      Add(seg);
      SetTo(seg.To);
    }

    string IDrawable.ToText()
    {
      if (Count <= 0)
      { return null; }

      StringBuilder sb = new StringBuilder();
      this[0].InitToText(sb);
      foreach (ISegment seg in this)
      { seg.AppendToText(sb); }

      return sb.ToString();
    }
    IEnumerable<Pnt> IDrawable.GetVertices()
    {
      if (Count <= 0)
      { yield break; }
      yield return this[0].From;
      foreach (ISegment seg in this)
      { yield return seg.To; }
    }

    private void SetTo(Pnt p)
    {
      _tx = p.X;
      _ty = p.Y;
    }
    public void RemoveLast()
    {
      if (Count <= 0)
      { return; }
      if (Count == 1)
      { SetTo(this[0].From); }
      else
      { SetTo(this[Count - 2].To); }
      RemoveAt(Count - 1);
    }
  }

  public class SymbolCurve
  {
    public float LineWidth { get; set; }
    public bool Fill { get; set; }
    public bool Stroke { get; set; }

    public Curve Curve { get; set; }
  }

  public enum SymbolType { Point, Line, Text }
  public class Symbol
  {
    public string Id { get; set; }

    public string Text { get; set; }
    public List<SymbolCurve> Curves { get; set; }

    public SymbolType GetSymbolType()
    {
      int nCurves = Curves?.Count ?? 0;
      if (nCurves == 0)
      { return SymbolType.Text; }

      if (nCurves == 1 && Curves[0].Curve == null)
      { return SymbolType.Line; }

      return SymbolType.Point;
    }
  }

  public static class Utils
  {
    public static bool Cancel(object sender, System.ComponentModel.CancelEventHandler cancelEvent)
    {
      if (cancelEvent == null)
      { return false; }
      System.ComponentModel.CancelEventArgs args = new System.ComponentModel.CancelEventArgs();
      cancelEvent.Invoke(sender, args);
      return args.Cancel;
    }
  }
}