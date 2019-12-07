using System;

namespace OMapScratch
{
  partial class MapVm
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

    private class FlipAction : IAction
    {
      private readonly Map _map;
      private readonly Elem _elem;
      public FlipAction(Map map, Elem elem)
      {
        _map = map;
        _elem = elem;
      }
      void IAction.Action()
      { Flip(); }
      public void Flip()
      { _map.Flip(_elem); }
    }

    private class ReshapeAction : IAction, IPointAction, IEditAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;

      private bool _first = true;
      private Curve _reshapeGeometry;
      private Curve _baseGeometry;

      public ReshapeAction(IMapView view, Map map, Elem elem, float position)
      {
        _view = view;
        _map = map;
        _elem = elem;
      }

      public string Description { get { return "Click the position of the next vertex"; } }

      private Curve BaseGeometry
      {
        get
        {
          if (_baseGeometry == null)
          {
            _baseGeometry = (Curve)_elem.Geometry;
            _elem.Geometry = _baseGeometry.Clone();
          }
          return _baseGeometry;
        }
      }

      private Curve ReshapeGeometry
      {
        get { return _reshapeGeometry ?? (_reshapeGeometry = new Curve()); }
      }

      void IAction.Action()
      { _view.SetNextPointAction(this); }

      void IPointAction.Action(Pnt pnt)
      { ReshapeLine(pnt); }
      public void ReshapeLine(Pnt next)
      {
        _map.Reshape(_elem, BaseGeometry, ReshapeGeometry, next, _first);
        _first = false;
        _view.SetNextPointAction(this);
      }

      public bool ShowDetail { get { return true; } }
    }

    private class InsertVertexAction : IAction, IPointAction, IEditAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      private readonly float _position;

      public InsertVertexAction(IMapView view, Map map, Elem elem, float position)
      {
        _view = view;
        _map = map;
        _elem = elem;
        _position = position;
      }

      public string Description { get { return "Click the new position of the vertex"; } }

      void IAction.Action()
      {
        if (InsertVertex())
        { _view.SetNextPointAction(this); }
      }
      public bool InsertVertex()
      {
        return _map.InsertVertex(_elem, _position);
      }

      void IPointAction.Action(Pnt pnt)
      { MoveVertex(pnt); }
      public void MoveVertex(Pnt next)
      {
        _map.MoveVertex(_elem, (int)_position + 1, next);
      }

      public bool ShowDetail { get { return true; } }
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

    [Obsolete("rename")]
    private class SetSymbolAction_ : IAction, ISymbolAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      public SetSymbolAction_(IMapView view, Map map, Elem elem)
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
        return _map.SetSymbol_(_elem, symbol, color, out message);
      }
    }

    private class EditTextAction : IAction, ITextAction
    {
      private readonly IMapView _view;
      private readonly Map _map;
      private readonly Elem _elem;
      public EditTextAction(IMapView view, Map map, Elem elem)
      {
        _view = view;
        _map = map;
        _elem = elem;
      }

      public string Description { get { return "Add/Edit text"; } }

      void IAction.Action()
      { _view.EditText(_elem.Text, this); }

      bool ITextAction.Action(string text) => EditText(text);
      public bool EditText(string text)
      {
        return _map.EditText(_elem, text);
      }
    }

  }
}
