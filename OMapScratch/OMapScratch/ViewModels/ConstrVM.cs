using System.Collections.Generic;

namespace OMapScratch.ViewModels
{
  public interface IConstrView
  {
    IMapView MapView { get; }
    bool IsInStartArea(Pnt pnt);
    void PostInvalidate();
  }

  public interface ITouchAction : IPointAction
  {
    bool OnDown(float[] at);
    bool OnMove(float[] at);
    bool TryHandle(bool reInit);
  }

  public interface IHasCompass
  {
    Pnt Position { get; }
  }
  public interface IHasGeometries
  {
    IEnumerable<Curve> GetGeometries();
  }
  public interface IHasTexts
  {
    IEnumerable<Elem> GetTexts();
  }


  public class ConstrVm
  {
    private abstract class ConstrAction : IAction, ITouchAction, IHasCompass, IHasGeometries, IHasTexts
    {
      private readonly IMapView _mapView;
      private readonly Pnt _origPosition;
      private readonly ConstrVm _constrVm;

      private bool _initialized;
      private Pnt _end;

      protected ConstrAction(IMapView mapView, Pnt origPosition, ConstrVm constrVm)
      {
        _mapView = mapView;
        _origPosition = origPosition;
        _constrVm = constrVm;
      }

      void IAction.Action()
      { _mapView.SetNextPointAction(this); }
      Pnt IHasCompass.Position
      { get { return OrigPosition; } }
      public Pnt OrigPosition
      { get { return _origPosition; } }

      public Pnt End
      { get { return _end; } }

      void IPointAction.Action(Pnt pnt)
      { MoveElement(pnt); }
      public void MoveElement(Pnt next)
      { }

      public abstract string Description { get; }

      public bool TryHandle(bool reInit)
      {
        bool initialized = _initialized;
        if (reInit)
        { _initialized = false; }

        if (!initialized || _end == null)
        { return false; }

        _constrVm._constrs.Add(GetGeometry());
        return true;
      }

      public bool OnDown(float[] pnt)
      {
        if (_initialized)
        {
          _end = null;
        }
        else
        {
          Pnt end = new Pnt(pnt[0], pnt[1]);
          if (_constrVm._view.IsInStartArea(end))
          { _end = end; }
          _initialized = true;
        }
        _constrVm._view.PostInvalidate();
        return true;
      }

      public bool OnMove(float[] pnt)
      {
        if (_end != null)
        { _end = new Pnt(pnt[0], pnt[1]); }

        _constrVm._view.PostInvalidate();
        return true;
      }

      public IEnumerable<Curve> GetGeometries()
      {
        if (_end != null)
        {
          yield return GetGeometry();
        }
      }

      public abstract IEnumerable<Elem> GetTexts();
      protected abstract Curve GetGeometry();
    }


    private class LineAction : ConstrAction
    {
      public LineAction(IMapView mapView, Pnt origPosition, ConstrVm constrVm)
        : base(mapView, origPosition, constrVm)
      { }

      public override string Description { get { return "Start within circle and move to the end position of the construction line"; } }
      protected override Curve GetGeometry()
      {
        return new Curve().MoveTo(OrigPosition.X, OrigPosition.Y).LineTo(End.X, End.Y);
      }

      public override IEnumerable<Elem> GetTexts()
      {
        if (End != null)
        {
          float dx = End.X - OrigPosition.X;
          float dy = End.Y - OrigPosition.Y;
          double l = System.Math.Sqrt(dx * dx + dy * dy);
          double angle = System.Math.Atan2(-dy, dx);
          double azi = 90 - angle * 180 / System.Math.PI;
          if (azi < 0) { azi += 360; }

          System.Text.StringBuilder sb = new System.Text.StringBuilder();
          sb.AppendLine($"{l:N1} m");
          sb.Append($"{azi:N1}°");
          Elem elem = new Elem() { Symbol = new Symbol { Text = sb.ToString() }, Geometry = new Pnt(OrigPosition.X + dx / 2, OrigPosition.Y + dy / 2) };
          yield return elem;
        }
      }
    }

    private class CircleAction : ConstrAction
    {
      public CircleAction(IMapView mapView, Pnt origPosition, ConstrVm constrVm)
        : base(mapView, origPosition, constrVm)
      { }

      public override string Description { get { return "Start within displayed circle and move to the radius of the construction circle"; } }
      protected override Curve GetGeometry()
      {
        float dx = End.X - OrigPosition.X;
        float dy = End.Y - OrigPosition.Y;
        float r = (float)System.Math.Sqrt(dx * dx + dy * dy);
        return new Curve().Circle(OrigPosition.X, OrigPosition.Y, r);
      }

      public override IEnumerable<Elem> GetTexts()
      {
        if (End != null)
        {
          float dx = End.X - OrigPosition.X;
          float dy = End.Y - OrigPosition.Y;
          double l = System.Math.Sqrt(dx * dx + dy * dy);

          System.Text.StringBuilder sb = new System.Text.StringBuilder();
          sb.AppendLine($"{l:N1} m");
          Elem elem = new Elem() { Symbol = new Symbol { Text = sb.ToString() }, Geometry = new Pnt(OrigPosition.X + dx / 2, OrigPosition.Y + dy / 2) };
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

    public float? GetDeclination()
    {
      IHasCompass action = _view.MapView.NextPointAction as IHasCompass;
      if (action == null)
      { return null; }

      return 0;
    }

    public Pnt GetCompassPoint()
    {
      IHasCompass action = _view.MapView.NextPointAction as IHasCompass;
      if (action == null)
      { return null; }

      return action.Position;
    }

    public IEnumerable<Curve> GetGeometries()
    {
      foreach (Curve curve in _constrs)
      { yield return curve; }

      IHasGeometries action = _view.MapView.NextPointAction as IHasGeometries;
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
      IHasTexts action = _view.MapView.NextPointAction as IHasTexts;
      if (action != null)
      {
        foreach (Elem elem in action.GetTexts())
        {
          yield return elem;
        }
      }
    }


    public List<ContextAction> GetConstrActions(Pnt pos)
    {
      IMapView mapView = _view.MapView;

      List<ContextAction> actions = new List<ContextAction>
      {
        new ContextAction(pos, new LineAction(mapView, pos, this)) { Name = "Constr. Line" },
        new ContextAction(pos, new CircleAction(mapView, pos, this)) { Name = "Constr. Circle" },
        new ContextAction(pos, null) { Name = "Set Location" }
      };

      return actions;
    }
  }
}