using System.Collections.Generic;

namespace OMapScratch.ViewModels
{
  public interface IConstrView
  {
    IMapView MapView { get; }
    void PostInvalidate();
  }

  public interface ITouchAction : IPointAction
  {
    bool OnDown(float[] at);
    bool OnMove(float[] at);
  }

  public class ConstrVm
  {
    private class LineAction : ITouchAction
    {
      private readonly Pnt _origPosition;
      private readonly IConstrView _view;

      private Pnt _end;

      public LineAction(Pnt origPosition, IConstrView view)
      {
        _origPosition = origPosition;
        _view = view;
      }

      public string Description { get { return "Start within circle and move to the end position of the construction line"; } }
      void IPointAction.Action(Pnt pnt)
      { MoveElement(pnt); }
      public void MoveElement(Pnt next)
      {
      }
      public bool OnDown(float[] pnt)
      {
        _end = new Pnt(pnt[0], pnt[1]);
        _view.PostInvalidate();
        return true;
      }
      public bool OnMove(float[] pnt)
      {
        if (_end != null)
        { _end = new Pnt(pnt[0], pnt[1]); }

        _view.PostInvalidate();
        return true;
      }

      public IEnumerable<Curve> GetGeometries()
      {
        yield return new Curve().Circle(_origPosition.X, _origPosition.Y, 10);
        yield return new Curve().Circle(_origPosition.X, _origPosition.Y, 50);
        yield return new Curve().MoveTo(_origPosition.X - 60, _origPosition.Y).LineTo(_origPosition.X + 60, _origPosition.Y);
        yield return new Curve().MoveTo(_origPosition.X, _origPosition.Y - 60).LineTo(_origPosition.X, _origPosition.Y + 60);

        if (_end != null)
        {
          yield return new Curve().MoveTo(_origPosition.X, _origPosition.Y).LineTo(_end.X, _end.Y);
        }
      }

      public IEnumerable<Elem> GetTexts()
      {
        if (_end != null)
        {
          float dx = _end.X - _origPosition.X;
          float dy = _end.Y - _origPosition.Y;
          double l = System.Math.Sqrt(dx * dx + dy * dy);

          string text = $"{l:N1} m";
          Elem elem = new Elem() { Symbol = new Symbol { Text = text }, Geometry = new Pnt(_origPosition.X + dx / 2, _origPosition.Y + dy / 2) };
          yield return elem;
        }
      }
    }

    private readonly IConstrView _view;
    private readonly List<Curve> _constrs;

    public ConstrVm(IConstrView view)
    {
      _view = view;
      _constrs = new List<Curve>();
    }

    public IEnumerable<Curve> GetGeometries()
    {
      foreach (Curve curve in _constrs)
      { yield return curve; }

      LineAction action = _view.MapView.NextPointAction as LineAction;
      if (action != null)
      {
        foreach (Curve curve in action.GetGeometries())
        {
          yield return curve;
        }
      }
    }

    public IEnumerable<Elem> GetTexts()
    {
      LineAction action = _view.MapView.NextPointAction as LineAction;
      if (action != null)
      {
        foreach (Elem elem in action.GetTexts())
        {
          yield return elem;
        }
      }
    }


    public ContextActions GetConstrActions(Pnt pos)
    {
      IMapView mapView = _view.MapView;

      List<ContextAction> actions = new List<ContextAction>
      {
        new ContextAction(pos) { Name = "Constr. Line", Action = () => { mapView.SetNextPointAction(new LineAction(pos, _view)); } },
        new ContextAction(pos) { Name = "Constr. Circle", Action = () => {  } },
        new ContextAction(pos) { Name = "Set GPS", Action = () => { } },
      };
      ContextActions constr = new ContextActions("Constr.", null, pos, actions);

      return constr;
    }
  }
}