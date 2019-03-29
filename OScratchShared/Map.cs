
using Basics;
using System;
using System.Collections.Generic;
using System.IO;

namespace OMapScratch
{
  public partial class Map
  {
    private List<Elem> _elems;
    private List<Symbol> _symbols;
    private List<ColorRef> _colors;

    private Elem _currentCurve;
    private IGeoImage _currentGeoImage;
    private List<GeoImageViews> _images;

    private XmlConfig _config;
    private string _configPath;

    private XmlWorld _world;
    private float? _declination;

    public IGeoImage CurrentGeoImage => _currentGeoImage;

    private Stack<Operation> _undoOps = new Stack<Operation>();
    private Stack<Operation> _redoOps = new Stack<Operation>();

    private Stack<Operation> UndoOps => _undoOps ?? (_undoOps = new Stack<Operation>());
    internal IReadOnlyList<GeoImageViews> Images
    {
      get { return _images ?? (_images = GeoImage.CreateImages(_config?.Images, GetConfigDir())); }
    }

    public const string DefaultScratchImg = "ScratchImg.jpg";

    public const float DefaultSymbolScale = 1;
    public float SymbolScale
    {
      get
      {
        float scale = _config?.Data?.SymbolScale ?? 0;
        return scale > 0 ? scale : DefaultSymbolScale;
      }
    }

    public const float DefaultElemTextSize = 12;
    public float ElemTextSize
    {
      get
      {
        float size = _config?.Data?.ElemTextSize ?? 0;
        return size > 0 ? size : DefaultElemTextSize;
      }
    }

    public const float DefaultConstrTextSize = 1.6f;
    public float ConstrTextSize
    {
      get
      {
        float size = _config?.Data?.ConstrTextSize ?? 0;
        return size > 0 ? size : DefaultConstrTextSize;
      }
    }

    public const float DefaultConstrLineWidth = 0.1f;
    public float ConstrLineWidth
    {
      get
      {
        float size = _config?.Data?.ConstrLineWidth ?? 0;
        return size > 0 ? size : DefaultConstrLineWidth;
      }
    }

    public const float DefaultMinSearchDist = 10;
    public float MinSearchDistance
    {
      get
      {
        float search = _config?.Data?.Search ?? 0;
        return search > 0 ? search : DefaultMinSearchDist;
      }
    }

    public IEnumerable<Elem> Elems
    {
      get
      {
        if (_elems == null)
        { yield break; }

        foreach (var elem in _elems)
        { yield return elem; }
      }
    }

    public bool RemoveElem(Elem elem)
    {
      RemoveElementOperation op = new RemoveElementOperation(_elems, elem);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool RemoveVertex(Elem elem, int idx)
    {
      RemoveVertexOperation op = new RemoveVertexOperation(elem, idx);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool InsertVertex(Elem elem, float position)
    {
      InsertVertexOperation op = new InsertVertexOperation(elem, position);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool Flip(Elem elem)
    {
      FlipOperation op = new FlipOperation(elem);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool Split(Elem elem, float position)
    {
      SplitOperation op = new SplitOperation(_elems, elem, position);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool MoveVertex(Elem elem, int idx, Pnt newPosition)
    {
      MoveVertexOperation op = new MoveVertexOperation(elem, idx, newPosition);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool MoveElement(Elem elem, Pnt offset)
    {
      MoveElementOperation op = new MoveElementOperation(elem, offset);
      op.Redo(this, true);
      return op.LastSuccess;
    }

    public bool SetRotationElement(Elem elem, float azimuth)
    {
      SetRotationElementOperation op = new SetRotationElementOperation(elem, azimuth);
      op.Redo(this, true);
      return op.LastSuccess;
    }


    public bool SetSymbol(Elem elem, Symbol symbol, ColorRef color, out string message)
    {
      bool isElemLine = elem.Symbol.GetSymbolType() == SymbolType.Line;
      bool isSymbolLine = symbol.GetSymbolType() == SymbolType.Line;
      if (isElemLine != isSymbolLine)
      {
        message = isElemLine ?
          "Cannot change to point symbol" :
          "Cannot change to line symbol";
        return false;
      }

      SetSymbolOperation op = new SetSymbolOperation(elem, symbol, color);
      op.Redo(this, true);
      message = null;
      return op.LastSuccess;
    }

    public void SynchLocation(Pnt mapCoord, double lat, double lon, double alt, double accuracy)
    {
      if (_world == null)
      {
        _world = _config?.Offset?.World?.Clone();
      }
      if (_world == null)
      {
        XmlWorld world = new XmlWorld
        { GeoMatrix11 = 111175 };
        world.GeoMatrix00 = Math.Cos(Math.PI * lat / 180) * world.GeoMatrix11;
        _world = world;
      }
      // dLon * m00 + dLat * m10 == map.X
      // dLon * m01 + dLat * m11 == map.Y
      double det = _world.GeoMatrix00 * _world.GeoMatrix11 - _world.GeoMatrix01 * _world.GeoMatrix10;
      double dLon = (-_world.GeoMatrix10 * mapCoord.Y - _world.GeoMatrix11 * mapCoord.X) / det;
      double dLat = (_world.GeoMatrix00 * mapCoord.Y - _world.GeoMatrix01 * mapCoord.X) / det;

      _world.Latitude = lat + dLat;
      _world.Longitude = lon + dLon;
    }

    internal XmlWorld World
    {
      get { return _world ?? _config?.Offset?.World; }
    }
    internal XmlConfig Config
    { get { return _config; } }

    public double[] GetOffset()
    {
      XmlOffset offset = _config?.Offset;
      if (offset == null)
      { return null; }
      return new double[] { offset.X, offset.Y };
    }
    public float? GetDeclination()
    {
      return _declination ?? (float?)_config?.Offset?.Declination;
    }
    public void SetDeclination(float? declination)
    {
      _declination = declination;
    }

    public double[] GetCurrentWorldMatrix()
    {
      return _currentGeoImage?.GetWorldMatrix();
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
          Curve curve = new Curve();
          _currentCurve = new Elem { Geometry = curve, Symbol = sym, Color = color };
        }
        AddPointOperation op = new AddPointOperation(this, x, y);
        op.Redo(this, true);
      }
      else if (symTyp == SymbolType.Point ||
        symTyp == SymbolType.Text)
      {
        AddElementOperation op = new AddElementOperation(new Elem { Geometry = p, Symbol = sym, Color = color }, _elems, _currentCurve);
        op.Redo(this, true);
      }
    }

    public void Reshape(Elem element, Curve baseGeometry, Curve reshapeGeometry, Pnt add, bool first)
    {
      ReshapeOperation op = new ReshapeOperation(element, baseGeometry, reshapeGeometry, add, first);
      op.Redo(this, true);
    }

    public void CommitCurrentOperation()
    {
      CommitOperation op = new CommitOperation();
      op.Redo(this, true);
      _currentCurve = null;
    }

    public void Undo()
    {
      if (_undoOps?.Count > 0)
      {
        Operation op = _undoOps.Pop();
        op.Undo();
        _redoOps = _redoOps ?? new Stack<Operation>();
        _redoOps.Push(op);
      }
    }

    public void Redo()
    {
      if (_redoOps?.Count > 0)
      {
        Operation op = _redoOps.Pop();
        _undoOps = _undoOps ?? new Stack<Operation>();
        op.Redo(this, false);
      }
    }

    public void Save(bool backup = false)
    {
      if (_elems == null)
      { return; }
      string path = GetLocalPath(_config?.Data?.Scratch);
      if (path == null)
      { return; }

      if (backup)
      { path = $"{path}.bck"; }

      using (var w = new StreamWriter(path))
      { Serializer.Serialize(XmlElems.Create(_elems), w); }
    }

    public void Load(string configPath, XmlConfig config)
    {
      Reset();

      _configPath = configPath;
      _config = config;

      LoadSymbols();
      List<Elem> elems = LoadElems();
      _elems = GetValidElems(elems);
    }

    private void Reset()
    {
      _currentGeoImage = null;
      _colors = null;
      _symbols = null;
      _elems = null;
      _images = null;
      _world = null;
      _declination = null;
      _undoOps = new Stack<Operation>();
      _redoOps = new Stack<Operation>();

      _configPath = null;
      _config = null;

    }

    private List<Elem> GetValidElems(IEnumerable<Elem> elems)
    {
      List<Elem> valids = new List<Elem>();
      if (elems != null)
      {
        foreach (var elem in elems)
        {
          if (elem?.Symbol == null)
          { continue; }
          if (elem.Geometry == null)
          { continue; }

          valids.Add(elem);
        }
      }
      return valids;
    }
    private List<Elem> LoadElems()
    {
      string elemsPath = VerifyLocalPath(_config?.Data?.Scratch);
      if (string.IsNullOrEmpty(elemsPath))
      { return null; }
      if (!Deserialize(elemsPath, out XmlElems xml))
      { return null; }
      if (xml?.Elems == null)
      { return null; }

      Dictionary<string, ColorRef> colorDict = new Dictionary<string, ColorRef>();
      foreach (var c in GetColors())
      { colorDict[c.Id] = c; }

      Dictionary<string, Symbol> symbolDict = new Dictionary<string, Symbol>();
      foreach (var s in GetSymbols())
      { symbolDict[s.Id] = s; }

      List<Elem> elems = new List<Elem>();
      foreach (var xmlElem in xml.Elems)
      {
        Elem elem = xmlElem.GetElem();
        if (symbolDict.TryGetValue(xmlElem.SymbolId ?? "", out Symbol sym))
        { elem.Symbol = sym; }
        if (colorDict.TryGetValue(xmlElem.ColorId ?? "", out ColorRef clr))
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
      Deserialize(symbolPath, out XmlSymbols xml);

      _symbols = ReadSymbols(xml?.Symbols);
      _colors = ReadColors(xml?.Colors);
    }

    private List<Symbol> ReadSymbols(List<XmlSymbol> xmlSymbols)
    {
      if (xmlSymbols == null)
      { return null; }
      List<Symbol> symbols = new List<Symbol>();
      foreach (var xml in xmlSymbols)
      { symbols.Add(xml.GetSymbol()); }
      return symbols;
    }

    private List<ColorRef> ReadColors(List<XmlColor> xmlColors)
    {
      if (xmlColors == null)
      { return null; }
      List<ColorRef> colors = new List<ColorRef>();
      foreach (var xml in xmlColors)
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

    public string GetConfigDir()
    {
      if (string.IsNullOrEmpty(_configPath))
      { return null; }
      string dir = Path.GetDirectoryName(_configPath);

      return dir;
    }
    internal string VerifyLocalPath(string path)
    {
      if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(_configPath))
      { return null; }
      string dir = Path.GetDirectoryName(_configPath);
      string fullPath = Path.Combine(dir, Path.GetFileName(path));

      return fullPath;
    }
  }
}
