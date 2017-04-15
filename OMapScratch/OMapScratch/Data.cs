using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace OMapScratch
{
  public interface IPointAction
  {
    string Description { get; }
    void Action(Pnt pnt);
  }
  public interface ISymbolAction
  {
    string Description { get; }
    bool Action(Symbol symbol, ColorRef color, out string message);
  }


  public interface IMapView
  {
    IPointAction NextPointAction { get; }

    void SetGetSymbolAction(ISymbolAction setSymbol);
    void SetNextPointAction(IPointAction actionWithNextPoint);
  }

  public partial interface ISegment
  {
    ISegment Clone();
    Pnt From { get; set; }
    Pnt To { get; set; }

    ISegment Project(IProjection prj);
    void InitToText(StringBuilder sb);
    void AppendToText(StringBuilder sb);
  }

  public partial interface IDrawable
  {
    string ToText();
    IEnumerable<Pnt> GetVertices();
    IDrawable Project(IProjection prj);
  }

  public interface IProjection
  {
    Pnt Project(Pnt pnt);
  }

  public partial class ColorRef
  {
    public string Id { get; set; }
  }

  public class ContextActions
  {
    public ContextActions(string name, Elem elem, Pnt position, List<ContextAction> actions)
    {
      Name = name;
      Elem = elem;
      Position = position;
      Actions = actions;
    }
    public string Name { get; set; }
    public Elem Elem { get; }
    public Pnt Position { get; }
    public List<ContextAction> Actions { get; }
  }
  public class ContextAction
  {
    public ContextAction(Pnt position)
    {
      Position = position;
    }
    public Pnt Position { get; }
    public string Name { get; set; }
    public Action Action { get; set; }
  }

  public partial class MapVm
  {
    private class MoveVertexAction : IPointAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly int _iVertex;
      public MoveVertexAction(Map map, Elem elem, int iVertex)
      {
        _map = map;
        _elem = elem;
        _iVertex = iVertex;
      }

      public string Description { get { return "Click the new position of the vertex"; } }
      void IPointAction.Action(Pnt pnt)
      { MoveVertex(pnt); }
      public void MoveVertex(Pnt next)
      {
        _map.MoveVertex(_elem, _iVertex, next);
      }
    }

    private class MoveElementAction : IPointAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly Pnt _origPosition;
      public MoveElementAction(Map map, Elem elem, Pnt origPosition)
      {
        _map = map;
        _elem = elem;
        _origPosition = origPosition;
      }

      public string Description { get { return "Click the new position of the element"; } }
      void IPointAction.Action(Pnt pnt)
      { MoveElement(pnt); }
      public void MoveElement(Pnt next)
      {
        Pnt diff = new Pnt(next.X - _origPosition.X, next.Y - _origPosition.Y);
        _map.MoveElement(_elem, diff);
      }
    }

    private class SetSymbolAction : ISymbolAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      public SetSymbolAction(Map map, Elem elem)
      {
        _map = map;
        _elem = elem;
      }

      public string Description { get { return "Select new color and symbol"; } }
      bool ISymbolAction.Action(Symbol symbol, ColorRef color, out string message)
      { return SetSymbol(symbol, color, out message); }

      public bool SetSymbol(Symbol symbol, ColorRef color, out string message)
      {
        return _map.SetSymbol(_elem, symbol, color, out message);
      }
    }

    private Map _map;
    private Pnt _currentLocalLocation;

    public event System.ComponentModel.CancelEventHandler Saving;
    public event EventHandler Saved;
    public event System.ComponentModel.CancelEventHandler Loading;
    public event EventHandler Loaded;

    public MapVm(Map map)
    {
      _map = map;
    }

    public bool HasGlobalLocation()
    {
      return _map.Config?.Offset?.World != null;
    }

    public Pnt GetCurrentLocalLocation()
    {
      return _currentLocalLocation;
    }
    public void SetCurrentLocation(double lat, double lon, double alt, double accuracy)
    {
      XmlWorld w = _map.Config?.Offset?.World;
      if (w == null)
      {
        _currentLocalLocation = null;
        return;
      }

      double dLat = lat - w.Latitude;
      double dLon = lon - w.Longitude;

      double x = w.GeoMatrix00 * dLon + w.GeoMatrix10 * dLat;
      double y = w.GeoMatrix01 * dLon + w.GeoMatrix11 * dLat;

      _currentLocalLocation = new Pnt((float)x, (float)-y);
    }

    public List<ContextActions> GetContextActions(IMapView view, float x0, float y0, float dx)
    {
      float dd = Math.Max(Math.Abs(dx), _map.MinSearchDistance);
      float xMin = x0 - dd;
      float yMin = y0 - dd;
      float xMax = x0 + dd;
      float yMax = y0 + dd;

      List<ContextActions> allActions = new List<ContextActions>();
      foreach (Elem elem in _map.Elems)
      {
        bool isLine = ((elem.Geometry as Curve)?.Count > 1);

        Pnt pre = null;
        Pnt elemPnt = null;

        List<ContextActions> vertexActionsList = new List<ContextActions>();
        int iVertex = 0;
        foreach (Pnt point in elem.Geometry.GetVertices())
        {
          if (point.X > xMin && point.X < xMax && point.Y > yMin && point.Y < yMax)
          {
            if (elemPnt == null)
            { elemPnt = point; }
            else
            {
              Pnt down = new Pnt { X = x0, Y = y0 };
              if (down.Dist2(elemPnt) > down.Dist2(point))
              { elemPnt = point; }
            }

            if (isLine)
            {
              List<ContextAction> vertexActions = new List<ContextAction>();
              vertexActions.Add(new ContextAction(elemPnt) { Name = "Delete", Action = () => _map.RemoveVertex(elem, iVertex) });
              MoveVertexAction moveAction = new MoveVertexAction(_map, elem, iVertex);
              vertexActions.Add(new ContextAction(elemPnt)
              {
                Name = "Move",
                Action = () => { view.SetNextPointAction(moveAction); }
              });

              vertexActionsList.Add(new ContextActions($"Vertex #{iVertex}", elem, new Pnt(point.X, point.Y), vertexActions));
            }
          }
          iVertex++;
        }
        if (elemPnt != null)
        {
          List<ContextAction> elemActions = new List<ContextAction>();
          elemActions.Add(new ContextAction(elemPnt) { Name = "Delete", Action = () => _map.RemoveElem(elem) });

          MoveElementAction moveAction = new MoveElementAction(_map, elem, elemPnt);
          elemActions.Add(new ContextAction(elemPnt)
          {
            Name = "Move",
            Action = () => { view.SetNextPointAction(moveAction); }
          });
          if (isLine)
          {
            elemActions.Add(new ContextAction(elemPnt) { Name = "Ins. Vert.", Action = () => _map.RemoveElem(elem) });
          }
          SetSymbolAction resymbolAction = new SetSymbolAction(_map, elem);
          elemActions.Add(new ContextAction(elemPnt)
          {
            Name = "Change Symbol",
            Action = () => { view.SetGetSymbolAction(resymbolAction); }
          });

          allActions.Add(new ContextActions("Elem", elem, elemPnt, elemActions));
        }
        allActions.AddRange(vertexActionsList);
      }

      return allActions;
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

    public float SymbolScale
    {
      get { return _map.SymbolScale; }
    }

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
    private abstract class Operation
    {
      public abstract void Undo();
      public void Redo(Map map, bool isFirst)
      {
        Redo(map._undoOps);
        if (isFirst)
        { map._redoOps.Clear(); }
      }
      protected abstract void Redo(Stack<Operation> ops);
    }

    private class MoveVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly int _vertexIndex;
      private readonly Pnt _newPosition;

      private ISegment _seg0;
      private ISegment _seg1;

      public MoveVertexOperation(Elem elem, int vertexIndex, Pnt newPosition)
      {
        _elem = elem;
        _vertexIndex = vertexIndex;
        _newPosition = newPosition;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          curve[0] = _seg0;
        }
        else if (_vertexIndex == curve.Count)
        {
          curve[_vertexIndex - 1] = _seg1;
        }
        else
        {
          curve[_vertexIndex - 1] = _seg0;
          curve[_vertexIndex] = _seg1;
        }
      }
      protected override void Redo(Stack<Operation> ops)
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          _seg0 = curve[0];
          ISegment clone = _seg0.Clone();
          clone.From = _newPosition;
          curve[0] = clone;
        }
        else if (_vertexIndex == curve.Count)
        {
          _seg1 = curve[_vertexIndex - 1];
          ISegment clone = _seg1.Clone();
          clone.To = _newPosition;
          curve[_vertexIndex - 1] = clone;
        }
        else
        {
          _seg0 = curve[_vertexIndex - 1];
          _seg1 = curve[_vertexIndex];

          ISegment clone0 = _seg0.Clone();
          clone0.To = _newPosition;

          ISegment clone1 = _seg1.Clone();
          clone1.From = _newPosition;

          curve[_vertexIndex - 1] = clone0;
          curve[_vertexIndex] = clone1;
        }
        ops.Push(this);
      }
    }

    private class RemoveVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly int _vertexIndex;

      private ISegment _seg0;
      private ISegment _seg1;

      public RemoveVertexOperation(Elem elem, int vertexIndex)
      {
        _elem = elem;
        _vertexIndex = vertexIndex;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          curve.Insert(0, _seg0);
        }
        else if (_vertexIndex == curve.Count + 1)
        {
          curve.Add(_seg1);
        }
        else
        {
          curve.RemoveAt(_vertexIndex - 1);
          curve.Insert(_vertexIndex - 1, _seg1);
          curve.Insert(_vertexIndex - 1, _seg0);
        }
      }
      protected override void Redo(Stack<Operation> ops)
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          _seg0 = curve[0];
          curve.RemoveAt(0);
        }
        else if (_vertexIndex == curve.Count)
        {
          _seg1 = curve[_vertexIndex - 1];
          curve.RemoveAt(_vertexIndex - 1);
        }
        else
        {
          _seg0 = curve[_vertexIndex - 1];
          _seg1 = curve[_vertexIndex];
          Lin add = new Lin { From = _seg0.From, To = _seg1.To };
          curve.RemoveAt(_vertexIndex - 1);
          curve.RemoveAt(_vertexIndex - 1);
          curve.Insert(_vertexIndex - 1, add);
        }
        ops.Push(this);
      }
    }

    private class MoveElementOperation : Operation
    {
      private readonly Elem _elem;
      private readonly Pnt _offset;

      private IDrawable _origGeom;

      public MoveElementOperation(Elem elem, Pnt offset)
      {
        _elem = elem;
        _offset = offset;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Geometry = _origGeom;
      }
      protected override void Redo(Stack<Operation> ops)
      {
        _origGeom = _elem.Geometry;
        Translation translate = new Translation(_offset);
        _elem.Geometry = _elem.Geometry.Project(translate);
        ops.Push(this);
      }
    }

    private class Translation : IProjection
    {
      private readonly Pnt _offset;
      public Translation(Pnt offset)
      { _offset = offset; }
      public Pnt Project(Pnt p)
      { return new Pnt(p.X + _offset.X, p.Y + _offset.Y); }
    }

    private class RemoveElementOperation : Operation
    {
      private readonly Elem _remove;
      private readonly IList<Elem> _elems;
      private int _idx;
      public RemoveElementOperation(IList<Elem> elems, Elem remove)
      {
        _remove = remove;
        _elems = elems;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        if (_idx < 0)
        { return; }
        _elems.Insert(_idx, _remove);
      }
      protected override void Redo(Stack<Operation> ops)
      {
        _idx = _elems.IndexOf(_remove);
        LastSuccess = (_idx >= 0);

        if (_idx < 0)
        { return; }

        _elems.RemoveAt(_idx);
        ops.Push(this);
      }
    }

    private class SetSymbolOperation : Operation
    {
      private readonly Elem _elem;
      private readonly Symbol _newSymbol;
      private readonly ColorRef _newColor;

      private Symbol _oldSymbol;
      private ColorRef _oldColor;

      public SetSymbolOperation(Elem elem, Symbol symbol, ColorRef color)
      {
        _elem = elem;
        _newSymbol = symbol;
        _newColor = color;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Symbol = _oldSymbol;
        _elem.Color = _oldColor;
      }
      protected override void Redo(Stack<Operation> ops)
      {
        _oldSymbol = _elem.Symbol;
        _oldColor = _elem.Color;

        _elem.Symbol = _newSymbol;
        _elem.Color = _newColor;

        ops.Push(this);
      }
    }

    private class AddPointOperation : Operation
    {
      private float _x;
      private float _y;
      private Elem _currentCurve;
      private bool _first;
      private Map _map;
      public AddPointOperation(Map map, float x, float y)
      {
        _x = x;
        _y = y;
        _map = map;

        _currentCurve = _map._currentCurve;
        _first = (_map._elems.Count == 0 || _map._elems[_map._elems.Count - 1] != _currentCurve);
      }
      protected override void Redo(Stack<Operation> ops)
      {
        if (_first)
        {
          ((Curve)_currentCurve.Geometry).MoveTo(_x, _y);
          _map._elems.Add(_currentCurve);
        }
        else
        { ((Curve)_currentCurve.Geometry).LineTo(_x, _y); }
        ops.Push(this);
      }
      public override void Undo()
      {
        if (_first)
        {
          if (_map._elems.Count > 0 && _map._elems[_map._elems.Count - 1] == _currentCurve)
          {
            _map._elems.RemoveAt(_map._elems.Count - 1);
            _map._currentCurve = null;
          }
          else
          {
            // ERROR!!!
          }
        }
        else
        { ((Curve)_currentCurve.Geometry).RemoveLast(); }
      }
    }
    private class AddElementOperation : Operation
    {
      private Elem _elem;
      private IList<Elem> _elems;
      private Elem _currentCurve;
      public AddElementOperation(Elem elem, IList<Elem> elems, Elem currentCurve)
      {
        _elem = elem;
        _elems = elems;
        _currentCurve = currentCurve;
      }

      public override void Undo()
      {
        if (_elems.Count > 0 && _elems[_elems.Count - 1] == _elem)
        {
          _elems.RemoveAt(_elems.Count - 1);
        }
        else
        {
          // ERROR!!!
        }
      }
      protected override void Redo(Stack<Operation> ops)
      {
        while (ops.Count > 0 && ops.Peek() is AddPointOperation)
        { ops.Pop(); }
        if (_currentCurve != null)
        {
          AddElementOperation addCurve = new AddElementOperation(_currentCurve, _elems, null);
          addCurve.Redo(ops);
          _currentCurve = null;
        }

        if (_elem == null)
        { return; }

        if (_elems.Count == 0 || _elem != _elems[_elems.Count - 1])
        { _elems.Add(_elem); }
        ops.Push(this);
      }
    }

    private List<Elem> _elems;
    private List<Symbol> _symbols;
    private List<ColorRef> _colors;

    private Elem _currentCurve;
    private string _currentImagePath;

    private XmlConfig _config;
    private string _configPath;

    private static readonly float _defaultMinSearchDist = 10;

    private Stack<Operation> _undoOps = new Stack<Operation>();
    private Stack<Operation> _redoOps = new Stack<Operation>();

    internal IList<XmlImage> Images
    {
      get { return _config?.Images; }
    }

    public float SymbolScale
    {
      get
      {
        float scale = _config?.Data?.SymbolScale ?? 1;
        if (scale <= 0) { scale = 1; }
        return scale;
      }
    }

    public float MinSearchDistance
    {
      get
      {
        float search = _config?.Data?.Search ?? 0;
        return search > 0 ? search : _defaultMinSearchDist;
      }
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

    internal XmlConfig Config
    { get { return _config; } }

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

    public void CommitCurrentCurve()
    {
      AddElementOperation op = new AddElementOperation(_currentCurve, _elems, null);
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
      _undoOps = new Stack<Operation>();
      _redoOps = new Stack<Operation>();
      LoadSymbols();
      List<Elem> elems = LoadElems();
      _elems = GetValidElems(elems);
    }

    private List<Elem> GetValidElems(IEnumerable<Elem> elems)
    {
      List<Elem> valids = new List<Elem>();
      foreach (Elem elem in elems)
      {
        if (elem?.Symbol == null)
        { continue; }
        if (elem.Geometry == null)
        { continue; }

        valids.Add(elem);
      }
      return valids;
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
    [XmlAttribute("symbolscale")]
    public float SymbolScale { get; set; }
    [XmlAttribute("search")]
    public float Search { get; set; }
    [XmlAttribute("numberformat")]
    public string NumberFormat { get; set; }
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
    //[XmlAttribute("gpsmintime")]
    //public double GpsMinTime { get; set; }
    //[XmlAttribute("gpsmindistance")]
    //public double GpsMinDistance { get; set; }

    [XmlAttribute("declination")]
    public string DeclText
    {
      get { return $"{Declination}"; }
      set
      {
        double decl;
        if (double.TryParse(value, out decl))
        { Declination = decl; }
        else
        { Declination = null; }
      }
    }

    [XmlIgnore()]
    public double? Declination { get; set; }
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

    public Pnt()
    { }
    public Pnt(float x, float y)
    {
      X = x;
      Y = y;
    }

    public Pnt Clone()
    {
      return new Pnt(X, Y);
    }

    public Pnt Trans(float[] matrix)
    {
      if (matrix == null)
      { return new Pnt(X, Y); }

      Pnt trans = new Pnt(matrix[0] * X + matrix[2], matrix[4] * Y + matrix[5]);
      return trans;
    }

    public Pnt Project(IProjection prj)
    {
      return prj.Project(this);
    }

    IDrawable IDrawable.Project(IProjection prj)
    { return Project(prj); }

    string IDrawable.ToText()
    {
      return $"{DrawableUtils.Point} {X:f1} {Y:f1}";
    }

    public float Dist2(Pnt other)
    {
      float dx = X - other.X;
      float dy = Y - other.Y;
      return (dx * dx + dy * dy);
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

    ISegment ISegment.Clone()
    { return Clone(); }
    public Lin Clone()
    {
      return new Lin { From = From.Clone(), To = To.Clone() };
    }

    public Lin Project(IProjection prj)
    {
      return new Lin { From = From.Project(prj), To = To.Project(prj) };
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.LineTo} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Circle : ISegment
  {
    public Pnt Center { get; set; }
    public float Radius { get; set; }

    Pnt ISegment.From
    {
      get { return new Pnt { X = Center.X, Y = Center.Y + Radius }; }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }
    Pnt ISegment.To
    {
      get { return new Pnt { X = Center.X, Y = Center.Y + Radius }; }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }

    ISegment ISegment.Clone()
    { return Clone(); }
    public Circle Clone()
    {
      return new Circle { Center = Center.Clone(), Radius = Radius };
    }

    public Circle Project(IProjection prj)
    {
      Pnt center = Center.Project(prj);
      Pnt start = new Pnt(Center.X + Radius, Center.Y).Project(prj);
      float dx = start.X - center.X;
      float dy = start.Y - center.Y;
      float radius = (float)Math.Sqrt(dx * dx + dy * dy);
      return new Circle { Center = center, Radius = radius };
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

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

    ISegment ISegment.Clone()
    { return Clone(); }
    public Bezier Clone()
    {
      return new Bezier { From = From.Clone(), I0 = I0.Clone(), I1 = I1.Clone(), To = To.Clone() };
    }

    public Bezier Project(IProjection prj)
    {
      return new Bezier { From = From.Project(prj), I0 = I0.Project(prj), I1 = I1.Project(prj), To = To.Project(prj) };
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

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

    public Curve Project(IProjection prj)
    {
      Curve projected = new Curve();
      foreach (ISegment seg in this)
      {
        projected.AddSegment(seg.Project(prj));
      }
      return projected;
    }
    IDrawable IDrawable.Project(IProjection prj)
    { return Project(prj); }
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