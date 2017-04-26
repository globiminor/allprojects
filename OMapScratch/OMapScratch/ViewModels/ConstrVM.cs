using System;
using System.Collections.Generic;

namespace OMapScratch.ViewModels
{
  public interface IConstrView
  {
    IMapView MapView { get; }
    void SetConstrColor(ColorRef color);
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

      public ConstrVm ConstrVm { get { return _constrVm; } }

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
      float? _declination;
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
          float declination = _declination ?? (_declination = ConstrVm.GetDeclination()).Value;
          float dx = End.X - OrigPosition.X;
          float dy = End.Y - OrigPosition.Y;
          double l = System.Math.Sqrt(dx * dx + dy * dy);
          double angle = System.Math.Atan2(-dy, dx);
          double azi = 90 - declination - angle * 180 / System.Math.PI;
          azi = azi % 360;
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

    private class MeasureDirAction : IPointAction, IAction
    {
      private readonly IMapView _mapView;
      private readonly Pnt _position;
      private readonly ConstrVm _constrVm;
      private readonly bool _to;
      public MeasureDirAction(IMapView mapView, Pnt position, ConstrVm constrVm, bool to)
      {
        _mapView = mapView;
        _position = position;
        _constrVm = constrVm;
        _to = to;
      }

      public string Description
      {
        get
        {
          return _to ?
            "Point the device towards the target and click on map when ready" :
            "Point the device towards the selected map position and click on map when ready";
        }
      }
      void IAction.Action()
      {
        if (_constrVm._view.MapView.MapVm.GetDeclination() == null)
        {
          _mapView.SetNextPointAction(null);
          _mapView.ShowText("Declination is not known. Please set orientation first", success: false);
          return;
        }
        _mapView.StartCompass(hide: true);
        _mapView.SetNextPointAction(this);
      }

      void IPointAction.Action(Pnt pnt)
      {
        double? azi = _constrVm._view.MapView.MapVm.GetDeclination() - _constrVm.GetOrientation();
        if (azi == null)
        { return; }
        double a = (azi.Value) * System.Math.PI / 180;
        double l = 100;
        float dx = (float)(l * System.Math.Cos(a));
        float dy = (float)(l * System.Math.Sin(a));

        Curve direction = _to ?
          new Curve().MoveTo(_position.X, _position.Y).LineTo(_position.X + dx, _position.Y + dy) :
          new Curve().MoveTo(_position.X + dx, _position.Y + dy).LineTo(_position.X, _position.Y);

        _constrVm._constrs.Add(direction);
      }
    }

    private class SetLocationAction : ILocationAction, IAction
    {
      private readonly IMapView _mapView;
      private readonly Pnt _position;
      private readonly MapVm _mapVm;
      public SetLocationAction(IMapView mapView, Pnt position, MapVm mapVm)
      {
        _mapView = mapView;
        _position = position;
        _mapVm = mapVm;
      }

      public string WaitDescription { get { return "Waiting to receive next location"; } }
      public string SetDescription { get { return "Set location to selected map position"; } }
      void IAction.Action()
      { _mapView.SetNextLocationAction(this); }

      void ILocationAction.Action(Android.Locations.Location loc)
      {
        _mapVm.SynchLocation(_position, loc);
      }
    }

    private class SetOrientationAction : IPointAction, IAction
    {
      private readonly IMapView _mapView;
      private readonly Pnt _position;
      private readonly MapVm _mapVm;
      public SetOrientationAction(IMapView mapView, Pnt position, MapVm mapVm)
      {
        _mapView = mapView;
        _position = position;
        _mapVm = mapVm;
      }

      void IAction.Action()
      {
        _mapView.StartCompass(hide: true);
        _mapView.SetNextPointAction(this);
      }

      string IPointAction.Description { get { return "Orientate this device corresponding to the environment, and click on map when ready"; } }
      void IPointAction.Action(Pnt pnt)
      {
        float? decl = 90 - (_mapVm.CurrentOrientation + Views.Utils.GetSurfaceOrientation());
        decl = decl % 360;
        _mapVm.SetDeclination(decl);
        _mapView.StartCompass(hide: false);
      }
    }

    private class SetConstrColorAction : IAction, ISymbolAction
    {
      private readonly IMapView _mapView;
      private readonly ConstrVm _constrVm;
      public SetConstrColorAction(IMapView mapView, ConstrVm constrVm)
      {
        _mapView = mapView;
        _constrVm = constrVm;
      }
      void IAction.Action()
      { _mapView.SetGetSymbolAction(this); }

      public string Description { get { return "Click on Color for construction and edit lines"; } }
      bool ISymbolAction.Action(Symbol symbol, ColorRef color, out string message)
      {
        _constrVm._view.SetConstrColor(color);
        message = null;
        return true;
      }
    }

    private class ClearConstrAction : IAction
    {
      private readonly ConstrVm _constrVm;
      public ClearConstrAction(ConstrVm constrVm)
      {
        _constrVm = constrVm;
      }

      void IAction.Action()
      {
        _constrVm._constrs.Clear();
        _constrVm._view.PostInvalidate();
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

      return _view.MapView.MapVm.GetDeclination() ?? 0;
    }

    public Pnt GetCompassPoint()
    {
      IHasCompass action = _view.MapView.NextPointAction as IHasCompass;
      if (action == null)
      { return null; }

      return action.Position;
    }

    public float? GetOrientation()
    {
      return _view.MapView.MapVm.CurrentOrientation;
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
        new ContextAction(pos, new MeasureDirAction(mapView, pos, this, true)) { Name = "Meas. Dir. From" },
        new ContextAction(pos, new MeasureDirAction(mapView, pos, this, false)) { Name = "Meas. Dir. To" },
        new ContextAction(pos, new SetConstrColorAction(mapView, this)) { Name = "Set Constr. color" },
        new ContextAction(pos, new ClearConstrAction(this)) { Name = "Clear Constrs." },
        new ContextAction(pos, new SetLocationAction(mapView, pos, _view.MapView.MapVm)) { Name = "Set Location" },
        new ContextAction(pos, new SetOrientationAction(mapView, pos, _view.MapView.MapVm)) { Name = "Set Orientation" }
      };

      return actions;
    }
  }
}