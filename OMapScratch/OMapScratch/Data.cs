using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Basics;

namespace OMapScratch
{
  public interface IAction
  {
    void Action();
  }
  public interface IPointAction
  {
    string Description { get; }
    void Action(Pnt pnt);
  }
  public interface IEditAction : IPointAction
  {
    bool ShowDetail { get; }
  }

  public interface ISymbolAction
  {
    string Description { get; }
    bool Action(Symbol symbol, ColorRef color, out string message);
  }
  public interface IColorAction
  {
    string Description { get; }
    bool Action(ColorRef color);
  }

  public partial interface IMapView
  {
    MapVm MapVm { get; }
    IPointAction NextPointAction { get; }

    void SetGetSymbolAction(ISymbolAction setSymbol);
    void SetGetColorAction(IColorAction setSymbol);

    void SetNextPointAction(IPointAction actionWithNextPoint);
    void StartCompass(bool hide = false);

    void ShowText(string text, bool success = true);
  }

  public partial interface ISegment
  {
    ISegment Clone();
    Pnt From { get; set; }
    Pnt To { get; set; }

    ISegment Project(IProjection prj);
    Box GetExtent();
    float GetAlong(Pnt p);
    Pnt At(float t);
    IList<ISegment> Split(float t);

    void InitToText(StringBuilder sb);
    void AppendToText(StringBuilder sb);
  }

  public partial interface IDrawable
  {
    IBox Extent { get; }
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
    public ContextAction(Pnt position, IAction action)
    {
      Position = position;
      Action = action;
    }
    public Pnt Position { get; }
    public string Name { get; set; }
    public IAction Action { get; set; }

    public void Execute()
    { Action?.Action(); }
  }

  public partial class MapVm
  {
    private class DeleteElemAction : IAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      public DeleteElemAction(Map map, Elem elem)
      {
        _map = map;
        _elem = elem;
      }
      void IAction.Action()
      { DeleteElem(); }
      public void DeleteElem()
      {
        _map.RemoveElem(_elem);
      }
    }

    private class DeleteVertexAction : IAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly int _iVertex;
      public DeleteVertexAction(Map map, Elem elem, int iVertex)
      {
        _map = map;
        _elem = elem;
        _iVertex = iVertex;
      }
      void IAction.Action()
      { DeleteVertex(); }
      public void DeleteVertex()
      {
        _map.RemoveVertex(_elem, _iVertex);
      }
    }

    private class SplitAction : IAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly float _position;
      public SplitAction(Map map, Elem elem, float position)
      {
        _map = map;
        _elem = elem;
        _position = position;
      }
      void IAction.Action()
      { Split(); }
      public void Split()
      { _map.Split(_elem, _position); }
    }

    private class InsertVertexAction : IAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly float _position;
      public InsertVertexAction(Map map, Elem elem, float position)
      {
        _map = map;
        _elem = elem;
        _position = position;
      }
      void IAction.Action()
      { InsertVertex(); }
      public void InsertVertex()
      { _map.InsertVertex(_elem, _position); }
    }

    private class MoveVertexAction : IAction, IPointAction, IEditAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly int _iVertex;
      public MoveVertexAction(IMapView view, Map map, Elem elem, int iVertex)
      {
        _view = view;
        _map = map;
        _elem = elem;
        _iVertex = iVertex;
      }

      public string Description { get { return "Click the new position of the vertex"; } }

      void IAction.Action()
      { _view.SetNextPointAction(this); }
      void IPointAction.Action(Pnt pnt)
      { MoveVertex(pnt); }
      public void MoveVertex(Pnt next)
      {
        _map.MoveVertex(_elem, _iVertex, next);
      }

      public bool ShowDetail { get { return true; } }
    }

    private class MoveElementAction : IAction, IPointAction, IEditAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly Pnt _origPosition;
      public MoveElementAction(IMapView view, Map map, Elem elem, Pnt origPosition)
      {
        _view = view;
        _map = map;
        _elem = elem;
        _origPosition = origPosition;
      }

      public string Description { get { return "Click the new position of the element"; } }

      void IAction.Action()
      { _view.SetNextPointAction(this); }
      void IPointAction.Action(Pnt pnt)
      { MoveElement(pnt); }
      public void MoveElement(Pnt next)
      {
        Pnt diff = new Pnt(next.X - _origPosition.X, next.Y - _origPosition.Y);
        _map.MoveElement(_elem, diff);
      }

      public bool ShowDetail { get { return true; } }
    }

    private class RotateElementAction : IAction, IPointAction, IEditAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly Pnt _origPosition;
      public RotateElementAction(IMapView view, Map map, Elem elem, Pnt origPosition)
      {
        _view = view;
        _map = map;
        _elem = elem;
        _origPosition = origPosition;
      }

      public string Description { get { return "Click the new direction of the element relative to its position"; } }

      void IAction.Action()
      { _view.SetNextPointAction(this); }
      void IPointAction.Action(Pnt pnt)
      { RotateElement(pnt); }
      public void RotateElement(Pnt dir)
      {
        double azi = Math.Atan2(_origPosition.X - dir.X, _origPosition.Y - dir.Y);
        _map.SetRotationElement(_elem, (float)azi);
      }

      public bool ShowDetail { get { return false; } }
    }

    private class SetSymbolAction : IAction, ISymbolAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      public SetSymbolAction(IMapView view, Map map, Elem elem)
      {
        _view = view;
        _map = map;
        _elem = elem;
      }

      public string Description { get { return "Select new color and symbol"; } }

      void IAction.Action()
      { _view.SetGetSymbolAction(this); }

      bool ISymbolAction.Action(Symbol symbol, ColorRef color, out string message)
      { return SetSymbol(symbol, color, out message); }

      public bool SetSymbol(Symbol symbol, ColorRef color, out string message)
      {
        return _map.SetSymbol(_elem, symbol, color, out message);
      }
    }

    private readonly Map _map;
    private Pnt _currentLocalLocation;
    private float? _maxSymbolSize;

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
      return _map.World != null;
    }

    public Pnt GetCurrentLocalLocation()
    {
      return _currentLocalLocation;
    }

    public Pnt SetCurrentLocation(double lat, double lon, double alt, double accuracy)
    {
      XmlWorld w = _map.World;
      if (w == null)
      {
        _currentLocalLocation = null;
        return null;
      }

      double dLat = lat - w.Latitude;
      double dLon = lon - w.Longitude;

      double x = w.GeoMatrix00 * dLon + w.GeoMatrix10 * dLat;
      double y = w.GeoMatrix01 * dLon + w.GeoMatrix11 * dLat;

      _currentLocalLocation = new Pnt((float)x, (float)-y);
      return _currentLocalLocation;
    }

    public float? CurrentOrientation { get; private set; }
    public void SetCurrentOrientation(float? orientation)
    { CurrentOrientation = orientation; }

    public void SetDeclination(float? declination)
    { _map.SetDeclination(declination); }

    public List<ContextActions> GetContextActions(IMapView view, float x0, float y0, float dx)
    {
      float dd = Math.Max(Math.Abs(dx), _map.MinSearchDistance);
      Box box = new Box(new Pnt(x0 - dd, y0 - dd), new Pnt(x0 + dd, y0 + dd));

      List<ContextActions> allActions = new List<ContextActions>();
      foreach (Elem elem in _map.Elems)
      {
        Curve curve = elem.Geometry as Curve;

        Pnt elemPnt = null;

        List<ContextActions> vertexActionsList = new List<ContextActions>();
        Pnt down = new Pnt { X = x0, Y = y0 };
        int iVertex = 0;
        float? split = null;
        foreach (Pnt point in elem.Geometry.GetVertices())
        {
          if (box.Intersects(point))
          {
            if (elemPnt == null)
            { elemPnt = point; }
            else
            {
              if (down.Dist2(elemPnt) > down.Dist2(point))
              {
                elemPnt = point;
                if (iVertex > 0 && curve?.Count > iVertex)
                { split = iVertex; }
              }
            }

            if (curve != null)
            {
              List<ContextAction> vertexActions = new List<ContextAction>();
              if (curve.Count > 1)
              { vertexActions.Add(new ContextAction(point, new DeleteVertexAction(_map, elem, iVertex)) { Name = "Delete" }); }

              vertexActions.Add(new ContextAction(point, new MoveVertexAction(view, _map, elem, iVertex)) { Name = "Move" });

              if (iVertex > 0 && iVertex < curve.Count)
              { vertexActions.Add(new ContextAction(point, new SplitAction(_map, elem, iVertex)) { Name = "Split Elem" }); }

              vertexActionsList.Add(new ContextActions($"Vertex #{iVertex}", null, point, vertexActions));
            }
          }
          iVertex++;
        }
        if (curve != null)
        {
          for (int iSeg = 0; iSeg < curve.Count; iSeg++)
          {
            ISegment seg = curve[iSeg];
            Box extent = seg.GetExtent();
            if (box.Intersects(extent))
            {
              float along = seg.GetAlong(down);
              if (along > 0 && along < 1)
              {
                Pnt at = seg.At(along);
                if (elemPnt == null || down.Dist2(elemPnt) > down.Dist2(at))
                {
                  elemPnt = at;
                  split = iSeg + along;
                }
              }
            }
          }
        }
        if (elemPnt != null)
        {
          List<ContextAction> elemActions = new List<ContextAction>();
          elemActions.Add(new ContextAction(elemPnt, new DeleteElemAction(_map, elem)) { Name = "Delete" });

          elemActions.Add(new ContextAction(elemPnt, new MoveElementAction(view, _map, elem, elemPnt)) { Name = "Move", });
          if (curve != null && split != null)
          {
            float at = split.Value - (int)split.Value;
            float limit = 0.01f;
            if (at > limit && at < 1 - limit)
            {
              elemActions.Add(new ContextAction(elemPnt, new InsertVertexAction(_map, elem, split.Value)) { Name = "Insert Vertex" });
            }
            elemActions.Add(new ContextAction(elemPnt, new SplitAction(_map, elem, split.Value)) { Name = "Split" });
          }
          if (curve == null)
          {
            elemActions.Add(new ContextAction(elemPnt, new RotateElementAction(view, _map, elem, elemPnt)) { Name = "Rotate" });
          }
          elemActions.Add(new ContextAction(elemPnt, new SetSymbolAction(view, _map, elem)) { Name = "Change Symbol" });

          allActions.Add(new ContextActions("Elem", elem, elemPnt, elemActions));
        }
        allActions.AddRange(vertexActionsList);
      }

      return allActions;
    }

    public int ImageCount { get { return _map?.Images?.Count ?? 0; } }
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

    public ColorRef ConstrColor
    {
      get { return _map.Config?.Data?.ConstrColor?.GetColor(); }
    }
    /// <summary>
    /// Font size of Text Symbol Elements in pt.
    /// </summary>
    /// <returns></returns>
    public float ElemTextSize
    {
      get { return _map.ElemTextSize; }
    }

    /// <summary>
    /// Font size of construction text
    /// </summary>
    /// <returns></returns>
    public float ConstrTextSize
    {
      get { return _map.ConstrTextSize; }
    }

    public float ConstrLineWidth
    {
      get { return _map.ConstrLineWidth; }
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
    public float? GetDeclination()
    { return _map.GetDeclination(); }
    public float[] GetCurrentWorldMatrix()
    { return _map.GetCurrentWorldMatrix(); }
    public List<Symbol> GetSymbols()
    { return _map.GetSymbols(); }
    public List<ColorRef> GetColors()
    { return _map.GetColors(); }

    public float MaxSymbolSize
    {
      get
      {
        if ((_maxSymbolSize ?? 0) <= 0)
        {
          float maxSymbolSize2 = 0;
          foreach (Symbol sym in GetSymbols())
          {
            if (sym.Curves == null)
            { continue; }

            foreach (SymbolCurve curve in sym.Curves)
            {
              IBox ext = curve.Curve?.Extent;
              if (ext == null)
              { continue; }

              maxSymbolSize2 = Math.Max(maxSymbolSize2, ext.Min.Dist2());
              maxSymbolSize2 = Math.Max(maxSymbolSize2, ext.Max.Dist2());
            }
          }
          _maxSymbolSize = (float)Math.Sqrt(maxSymbolSize2);
        }
        return _maxSymbolSize.Value;
      }
    }

    internal void Save(bool backup = false)
    {
      if (EventUtils.Cancel(this, Saving))
      { return; }
      _map.Save(backup);
      if (!backup)
      { Saved?.Invoke(this, null); }
    }

    public void Load(string configPath)
    {
      Save();
      _maxSymbolSize = null;

      XmlConfig config;
      using (TextReader r = new StreamReader(configPath))
      { Serializer.TryDeserialize(out config, r); }
      if (config != null)
      {
        if (EventUtils.Cancel(this, Loading))
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

    private class InsertVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly float _position;

      private ISegment _orig;
      private int _nSplit;

      public InsertVertexOperation(Elem elem, float position)
      {
        _elem = elem;
        _position = position;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        int pos = (int)_position;

        Curve curve = (Curve)_elem.Geometry;
        for (int i = 0; i < _nSplit; i++)
        { curve.RemoveAt(pos); }
        curve.Insert(pos, _orig);
      }
      protected override void Redo(Stack<Operation> ops)
      {
        int pos = (int)_position;
        Curve curve = (Curve)_elem.Geometry;
        _orig = curve[pos];

        IList<ISegment> segs = _orig.Split(_position - pos);
        _nSplit = segs.Count;

        curve.RemoveAt(pos);
        for (int i = _nSplit - 1; i >= 0; i--)
        { curve.Insert(pos, segs[i]); }
        ops.Push(this);
      }
    }

    private class SplitOperation : Operation
    {
      private readonly IList<Elem> _elems;
      private readonly Elem _elem;
      private readonly float _position;

      private ISegment _orig;

      public SplitOperation(IList<Elem> elems, Elem elem, float position)
      {
        _elems = elems;
        _elem = elem;
        _position = position;
      }
      public float InsertLimit { get; set; } = 0.01f;
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        int pos = (int)_position;
        float d = _position - pos;

        Curve curve = (Curve)_elem.Geometry;
        Curve newCurve = (Curve)_elems[_elems.Count - 1].Geometry;
        _elems.RemoveAt(_elems.Count - 1);

        if (_orig != null)
        {
          curve.RemoveAt(curve.Count - 1);
          curve.Add(_orig);

          newCurve.RemoveAt(0);
        }
        curve.AddRange(newCurve);
      }
      protected override void Redo(Stack<Operation> ops)
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_position <= InsertLimit || _position >= curve.Count - InsertLimit)
        { return; }

        int pos = (int)_position;
        float d = _position - pos;

        Curve newCurve = new Curve();
        ISegment preSplit = null;
        if (d > InsertLimit && d < 1 - InsertLimit)
        {
          _orig = curve[pos];
          IList<ISegment> segs = _orig.Split(_position - pos);
          if (segs.Count == 2)
          {
            preSplit = (segs[0]);
            newCurve.Add(segs[1]);
          }
        }

        if (newCurve.Count == 0)
        {
          newCurve.Add(curve[pos]);
          pos++;
        }

        for (int i = pos + 1; i < curve.Count; i++)
        { newCurve.Add(curve[i]); }

        while (curve.Count > _position)
        { curve.RemoveAt(curve.Count - 1); }

        if (preSplit != null)
        { curve.Add(preSplit); }

        _elems.Add(new Elem(_elem.Symbol, _elem.Color, newCurve));

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

    private class SetRotationElementOperation : Operation
    {
      private readonly Elem _elem;
      private readonly float _newAzimuth;

      private Pnt _preGeom;

      public SetRotationElementOperation(Elem elem, float azimuth)
      {
        _elem = elem;
        _newAzimuth = azimuth;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Geometry = _preGeom;
      }
      protected override void Redo(Stack<Operation> ops)
      {
        _preGeom = _elem.Geometry as Pnt;
        if (_preGeom == null)
        { return; }

        _elem.Geometry = DirectedPnt.Create(_preGeom, _newAzimuth);
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
        LastSuccess = false;
        _oldSymbol = _elem.Symbol;
        _oldColor = _elem.Color;

        _elem.Symbol = _newSymbol;
        _elem.Color = _newColor;
        ops.Push(this);
        LastSuccess = true;
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

    private XmlWorld _world;
    private float? _declination;


    private Stack<Operation> _undoOps = new Stack<Operation>();
    private Stack<Operation> _redoOps = new Stack<Operation>();

    internal IList<XmlImage> Images
    {
      get { return _config?.Images; }
    }

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

    public bool InsertVertex(Elem elem, float position)
    {
      InsertVertexOperation op = new InsertVertexOperation(elem, position);
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
        XmlWorld world = new XmlWorld();
        world.GeoMatrix11 = 111175;
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

    public float[] GetOffset()
    {
      XmlOffset offset = _config?.Offset;
      if (offset == null)
      { return null; }
      return new float[] { (float)offset.X, (float)offset.Y };
    }
    public float? GetDeclination()
    {
      return _declination ?? (float?)_config?.Offset?.Declination;
    }
    public void SetDeclination(float? declination)
    {
      _declination = declination;
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
      _currentImagePath = null;
      _colors = null;
      _symbols = null;
      _elems = null;
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
        foreach (Elem elem in elems)
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
      if (!File.Exists(elemsPath))
      { return null; }
      XmlElems xml;
      Deserialize(elemsPath, out xml);
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
      Deserialize(symbolPath, out xml);

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
      if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(_configPath))
      { return null; }
      string dir = Path.GetDirectoryName(_configPath);
      string fullPath = Path.Combine(dir, Path.GetFileName(path));

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
    [XmlAttribute("constrtextsize_mm")]
    public float ConstrTextSize { get; set; }
    [XmlAttribute("constrlinewidth_mm")]
    public float ConstrLineWidth { get; set; }
    [XmlAttribute("elemtextsize_pt")]
    public float ElemTextSize { get; set; }
    [XmlAttribute("search")]
    public float Search { get; set; }
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
    [XmlAttribute("dash")]
    public string Dash { get; set; }
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
      curve.Dash = DrawableUtils.GetDash(Dash);

      curve.Curve = (Curve)DrawableUtils.GetGeometry(Geometry);
      return curve;
    }
  }

  public class XmlRecents
  {
    [XmlElement("recent")]
    public List<string> Recents { get; set; }
  }

  public static class DrawableUtils
  {
    public const string Point = "p";
    public const string DirPoint = "dp";
    public const string Circle = "r";
    public const string Arc = "a";
    public const string MoveTo = "m";
    public const string LineTo = "l";
    public const string CubicTo = "c";

    internal static Dash GetDash(string dash)
    {
      if (string.IsNullOrEmpty(dash))
      { return null; }
      IList<string> parts = dash.Split(';');
      if (parts.Count > 3)
      { return null; }
      IList<string> intervalls = parts[0].Split(',');
      if (intervalls.Count < 1)
      { return null; }

      List<float> ints = new List<float>();
      foreach (string i in intervalls)
      {
        float intervall;
        if (!float.TryParse(i, out intervall))
        { return null; }
        ints.Add(intervall);
      }

      Dash d = new Dash { Intervals = ints.ToArray() };
      float t;
      if (parts.Count > 1 && float.TryParse(parts[1], out t))
      { d.StartOffset = t; }
      if (parts.Count > 2 && float.TryParse(parts[2], out t))
      { d.EndOffset = t; }

      return d;
    }
    internal static IDrawable GetGeometry(string geometry)
    {
      if (string.IsNullOrEmpty(geometry))
      { return null; }

      try
      {
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
          else if (part == DirPoint)
          {
            if (d != null)
            { throw new InvalidOperationException(); }
            DirectedPnt p = new DirectedPnt();
            p.X = float.Parse(parts[i + 1]);
            p.Y = float.Parse(parts[i + 2]);
            p.Azimuth = (float)(float.Parse(parts[i + 3]) * Math.PI / 180);
            i += 4;
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
          else if (part == Arc)
          {
            Curve c = (Curve)d ?? new Curve();
            c.Append(new Circle
            {
              Center = new Pnt { X = float.Parse(parts[i + 1]), Y = float.Parse(parts[i + 2]) },
              Radius = float.Parse(parts[i + 3]),
              Azimuth = float.Parse(parts[i + 4]) / 180 * (float)Math.PI,
              Angle = float.Parse(parts[i + 5]) / 180 * (float)Math.PI
            });
            i += 6;
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
            throw new NotImplementedException($"Unhandled key '{part}'");
          }
        }
        return d;
      }
      catch (Exception e)
      {
        throw new Exception($"Error parsing '{geometry}'", e);
      }
    }
  }
  public class Elem
  {
    public Elem()
    { }
    public Elem(Symbol symbol, ColorRef color, IDrawable geometry)
    {
      Symbol = symbol;
      Color = color;
      Geometry = geometry;
    }
    public IDrawable Geometry { get; set; }
    public Symbol Symbol { get; set; }
    public ColorRef Color { get; set; }
  }

  public interface IBox
  {
    Pnt Min { get; }
    Pnt Max { get; }
  }
  public class Box : IBox
  {
    public Box(Pnt min, Pnt max)
    {
      Min = min;
      Max = max;
    }
    public Pnt Min { get; }
    public Pnt Max { get; }

    public void Include(Pnt pnt)
    {
      Min.X = Math.Min(Min.X, pnt.X);
      Min.Y = Math.Min(Min.Y, pnt.Y);
      Max.X = Math.Max(Max.X, pnt.X);
      Max.Y = Math.Max(Max.Y, pnt.Y);
    }

    public bool Intersects(IBox box)
    {
      if (box.Min.X > Max.X)
      { return false; }
      if (box.Min.Y > Max.Y)
      { return false; }
      if (box.Max.X < Min.X)
      { return false; }
      if (box.Max.Y < Min.Y)
      { return false; }

      return true;
    }
  }
  public class DirectedPnt : Pnt
  {
    public static DirectedPnt Create(Pnt p, float azimuth)
    {
      return new DirectedPnt(p.X, p.Y, azimuth);
    }
    public DirectedPnt()
    { }
    public DirectedPnt(float x, float y, float azimuth)
      : base(x, y)
    {
      Azimuth = azimuth;
    }
    public float Azimuth { get; set; }

    protected override string ToDrawableText()
    {
      return $"{DrawableUtils.DirPoint} {X:f1} {Y:f1} {Azimuth * 180 / Math.PI:f1}";
    }
  }
  public partial class Pnt : IDrawable, IBox
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

    IBox IDrawable.Extent { get { return this; } }

    public Pnt Clone()
    {
      return CloneCore();
    }

    protected virtual Pnt CloneCore()
    {
      Pnt clone = (Pnt)MemberwiseClone();
      return clone;
    }

    public Pnt Trans(float[] matrix)
    {
      Pnt trans = Clone();
      if (matrix == null)
      { return trans; }

      trans.X = matrix[0] * X + matrix[2];
      trans.Y = matrix[4] * Y + matrix[5];
      return trans;
    }

    Pnt IBox.Min { get { return this; } }
    Pnt IBox.Max { get { return this; } }

    public Pnt Project(IProjection prj)
    {
      return prj.Project(this);
    }

    IDrawable IDrawable.Project(IProjection prj)
    { return Project(prj); }

    string IDrawable.ToText()
    {
      return ToDrawableText();
    }
    protected virtual string ToDrawableText()
    {
      return $"{DrawableUtils.Point} {X:f1} {Y:f1}";
    }

    public float Dist2(Pnt other = null)
    {
      float dx = X - (other?.X ?? 0);
      float dy = Y - (other?.Y ?? 0);
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

    public Lin()
    { }
    public Lin(Pnt from, Pnt to)
    {
      From = from;
      To = to;
    }
    ISegment ISegment.Clone()
    { return Clone(); }
    public Lin Clone()
    {
      return new Lin { From = From.Clone(), To = To.Clone() };
    }

    IList<ISegment> ISegment.Split(float t)
    { return Split(t); }
    public Lin[] Split(float t)
    {
      Pnt split = At(t);
      Lin[] splits = new Lin[]
      {
        new Lin(From.Clone(), split),
        new Lin(split.Clone(), To.Clone())
      };
      return splits;
    }

    public Lin Project(IProjection prj)
    {
      return new Lin { From = From.Project(prj), To = To.Project(prj) };
    }

    public Pnt At(float t)
    {
      return new Pnt(From.X + t * (To.X - From.X), From.Y + t * (To.Y - From.Y));
    }

    public float GetAlong(Pnt p)
    {
      float l2 = From.Dist2(To);
      float scalar = (p.X - From.X) * (To.X - From.X) + (p.Y - From.Y) * (To.Y - From.Y);
      return scalar / l2;
    }

    public Box GetExtent()
    {
      Pnt min = From.Clone();
      Pnt max = From.Clone();

      Box box = new Box(min, max);
      box.Include(To);

      return box;
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

    public float? Azimuth { get; set; }
    public float? Angle { get; set; }

    Pnt ISegment.From
    {
      get { return new Pnt(GetStartX(), GetStartY()); }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }
    Pnt ISegment.To
    {
      get { return new Pnt(GetEndX(), GetEndY()); }
      set { Center = new Pnt(value.X, value.Y - Radius); }
    }

    private float Azi { get { return Azimuth ?? 0; } }
    private double Ang { get { return Angle ?? (2.0 * Math.PI); } }

    private float GetStartX()
    { return Center.X + (float)Math.Sin(Azi) * Radius; }
    private float GetStartY()
    { return Center.Y + (float)Math.Cos(Azi) * Radius; }

    private float GetEndX()
    { return Center.X + (float)Math.Sin(Azi + Ang) * Radius; }
    private float GetEndY()
    { return Center.Y + (float)Math.Cos(Azi + Ang) * Radius; }

    public Pnt At(float t)
    {
      double angle = Azi + t * Ang;
      double sin = Math.Sin(angle);
      double cos = Math.Cos(angle);

      double x = Center.X + sin * Radius;
      double y = Center.Y + cos * Radius;
      return new Pnt((float)x, (float)y);
    }

    IList<ISegment> ISegment.Split(float t)
    {
      return new ISegment[]
      {
        new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azi, Angle = t * Angle },
        new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azi + t * Angle, Angle = (1 - t) * Angle },
      };
    }

    float ISegment.GetAlong(Pnt p)
    {
      double f = (Math.PI / 2 - Math.Atan2(p.Y - Center.Y, p.X - Center.X)) / (2 * Math.PI);
      if (f < 0) { f += 1; }
      f = (f - Azi) / Ang;
      return (float)f;
    }
    public Box GetExtent()
    {
      Pnt min = new Pnt(Center.X - Radius, Center.Y - Radius);
      Pnt max = new Pnt(Center.X + Radius, Center.Y + Radius);

      return new Box(min, max);
    }

    ISegment ISegment.Clone()
    { return Clone(); }
    public Circle Clone()
    {
      return new Circle { Center = Center.Clone(), Radius = Radius, Azimuth = Azimuth, Angle = Angle };
    }

    public Circle Project(IProjection prj)
    {
      Pnt center = Center.Project(prj);
      Pnt start = new Pnt(GetStartX(), GetStartY()).Project(prj);
      float dx = start.X - center.X;
      float dy = start.Y - center.Y;
      float radius = (float)Math.Sqrt(dx * dx + dy * dy);
      Circle projected = new Circle { Center = center, Radius = radius };

      if (Azimuth != null && Angle != null)
      {
        projected.Azimuth = (float)Math.Atan2(dx, dy);
        projected.Angle = Angle;
      }
      return projected;
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    void ISegment.InitToText(StringBuilder sb)
    { }
    void ISegment.AppendToText(StringBuilder sb)
    {
      if (Azimuth == null || Angle == null)
      { sb.Append($" {DrawableUtils.Circle} {Center.X:f1} {Center.Y:f1} {Radius:f1}"); }
      else
      { sb.Append($" {DrawableUtils.Arc} {Center.X:f1} {Center.Y:f1} {Radius:f1} {Azimuth * Math.PI / 180:f1} {Angle * Math.PI / 180:f1}"); }
    }
  }

  public partial class Bezier : ISegment
  {
    public Pnt From { get; set; }
    public Pnt I0 { get; set; }
    public Pnt I1 { get; set; }
    public Pnt To { get; set; }

    public Bezier()
    { }
    public Bezier(Pnt from, Pnt i0, Pnt i1, Pnt to)
    {
      From = from;
      I0 = i0;
      I1 = i1;
      To = to;
    }
    ISegment ISegment.Clone()
    { return Clone(); }
    public Bezier Clone()
    {
      return new Bezier(From.Clone(), I0.Clone(), I1.Clone(), To.Clone());
    }

    public Bezier Project(IProjection prj)
    {
      return new Bezier(From.Project(prj), I0.Project(prj), I1.Project(prj), To.Project(prj));
    }
    ISegment ISegment.Project(IProjection prj)
    { return Project(prj); }

    public float GetAlong(Pnt p)
    {
      float a0 = new Lin(From, I0).GetAlong(p);
      float a1 = new Lin(I0, I1).GetAlong(p);
      float a2 = new Lin(I1, To).GetAlong(p);

      float b0 = Math.Max(0, Math.Min(1, a0));
      float b1 = Math.Max(0, Math.Min(1, a1));
      float b2 = Math.Max(0, Math.Min(1, a2));

      float at0 = b0 / 3;
      float at1 = (b1 + 1) / 3;
      float at2 = (b2 + 2) / 3;
      Pnt p0 = At(at0);
      Pnt p1 = At(at1);
      Pnt p2 = At(at2);

      float d0 = p0.Dist2(p);
      float d1 = p1.Dist2(p);
      float d2 = p2.Dist2(p);

      if (d0 <= d1 && d0 <= d2)
      { return at0; }
      if (d1 <= d0 && d1 <= d2)
      { return at1; }

      return at2;
    }

    IList<ISegment> ISegment.Split(float t)
    { return Split(t); }
    public Bezier[] Split(float t)
    {
      Pnt splt = At(t);
      float u = 1 - t;
      float dx0 = I0.X - From.X;
      float dy0 = I0.Y - From.Y;

      float dx1 = I1.X - From.X;
      float dy1 = I1.Y - From.Y;

      Bezier[] splits = new Bezier[]
      {
        new Bezier(From.Clone(), new Pnt(From.X + t * dx0, From.Y + t * dy0), new Pnt(From.X + t * dx1, From.Y + t * dy1), splt),
        new Bezier(splt.Clone(), new Pnt(splt.X + u * dx0, splt.Y + u * dy0), new Pnt(splt.X + u * dx1, splt.Y + u * dy1), To.Clone()),
      };
      return splits;
    }

    public Pnt At(float t)
    {
      float x;
      {
        float a0 = From.X;
        float a1 = 3 * (I0.X - From.X);
        float a3 = To.X - 3 * I1.X + a1 + 2 * a0;
        float a2 = To.X - a3 - a1 - a0;

        x = a0 + t * (a1 + t * (a2 + t * a3));
      }
      float y;
      {
        float a0 = From.Y;
        float a1 = 3 * (I0.Y - From.Y);
        float a3 = To.Y - 3 * I1.Y + a1 + 2 * a0;
        float a2 = To.Y - a3 - a1 - a0;

        y = a0 + t * (a1 + t * (a2 + t * a3));
      }

      return new Pnt(x, y);
    }

    public Box GetExtent()
    {
      Pnt min = From.Clone();
      Pnt max = From.Clone();

      Box box = new Box(min, max);
      box.Include(I0);
      box.Include(I1);
      box.Include(To);

      return box;
    }

    void ISegment.InitToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.MoveTo} {From.X:f1} {From.Y:f1}"); }

    void ISegment.AppendToText(StringBuilder sb)
    { sb.Append($" {DrawableUtils.CubicTo} {I0.X:f1} {I0.Y:f1} {I1.X:f1} {I1.Y:f1} {To.X:f1} {To.Y:f1}"); }
  }

  public partial class Curve : List<ISegment>, IDrawable
  {
    private Box _extent;

    private float _tx, _ty;
    public Curve MoveTo(float x, float y)
    {
      _tx = x;
      _ty = y;
      return this;
    }

    public IBox Extent
    {
      get
      {
        if (_extent == null)
        {
          if (Count == 0)
          { return From; }
          Box extent = this[0].GetExtent();
          foreach (ISegment seg in this)
          {
            Box segExtent = seg.GetExtent();
            extent.Include(segExtent.Min);
            extent.Include(segExtent.Max);
          }
          _extent = extent;
        }
        return _extent;
      }
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

      _extent = null;
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

    public Curve Circle(float centerX, float centerY, float radius, float? azimuth = null, float? angle = null)
    {
      Circle circle = new Circle { Center = new Pnt { X = centerX, Y = centerY }, Radius = radius, Azimuth = azimuth, Angle = angle };
      AddSegment(circle);
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

      _extent = null;
    }
  }

  public class SymbolCurve
  {
    public float LineWidth { get; set; }
    public bool Fill { get; set; }
    public bool Stroke { get; set; }

    public Dash Dash { get; set; }

    public Curve Curve { get; set; }
  }
  public class Dash
  {
    public Dash Scale(float f)
    {
      Dash scaled = new Dash { Intervals = new float[Intervals.Length], StartOffset = f * StartOffset, EndOffset = f * EndOffset };
      for (int i = 0; i < Intervals.Length; i++)
      { scaled.Intervals[i] = f * Intervals[i]; }
      return scaled;
    }
    public float[] Intervals { get; set; }
    public float StartOffset { get; set; }
    public float EndOffset { get; set; }

    public double GetFactor(double fullLength)
    {
      double f = 1;
      if (fullLength > 0 && EndOffset != 0)
      {
        double sum = 0;
        foreach (double interval in Intervals)
        { sum += interval; }

        double l = fullLength - StartOffset + EndOffset;
        int d = (int)Math.Round(l / sum);
        if (d < 1)
        { d = 1; }
        f = l / (d * sum);
      }
      return f;
    }
    public IEnumerable<double> GetPositions(double fullLength = -1)
    {
      double f = GetFactor(fullLength);
      double pos = f * StartOffset;
      yield return pos;
      while (true)
      {
        foreach (double interval in Intervals)
        {
          pos += f * interval;
          yield return pos;
        }
      }
    }
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

      if (Curves[0].Curve == null || Curves[0].Dash != null)
      { return SymbolType.Line; }

      return SymbolType.Point;
    }
  }

  public static class EventUtils
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