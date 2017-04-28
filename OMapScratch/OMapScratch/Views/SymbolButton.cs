using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public interface ISetModeHandler
  {
    void SetMode(MapButton button);
  }

  public abstract class MapButton : Button
  {
    public MapButton(Android.Content.Context context)
      : base(context)
    { }

    protected sealed override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {
      if (right - left > bottom - top)
      { SetWidth(bottom - top); }
      else
      { base.OnLayout(changed, left, top, right, bottom); }
    }

    public abstract void MapClicked(float x, float y);

    protected sealed override void OnDraw(Canvas canvas)
    {
      OnDrawCore(this, canvas);
    }
    internal abstract void OnDrawCore(MapButton canvasOwner, Canvas canvas);
  }

  public class ModeButton : MapButton
  {
    private MainActivity _context;
    public MapButton CurrentMode { get; set; }

    public ModeButton(MainActivity context, MapButton initMode)
      : base(context)
    {
      _context = context;
      CurrentMode = initMode;
    }

    public override void MapClicked(float x, float y)
    {
      CurrentMode.MapClicked(x, y);
    }

    internal override void OnDrawCore(MapButton thisButton, Canvas canvas)
    {
      CurrentMode.OnDrawCore(thisButton, canvas);
    }
  }
  public class EditButton : MapButton
  {
    private MainActivity _context;

    public EditButton(MainActivity context)
      : base(context)
    {
      _context = context;
    }
    private class Prj : IProjection
    {
      private float _d;
      public Prj(float d)
      { _d = d; }
      public Pnt Project(Pnt p)
      { return new Pnt(_d * (p.X - 0.5f), _d * (p.Y - 0.5f)); }
    }
    private Curve _arrow;
    private Curve Arrow
    {
      get
      {
        if (_arrow == null)
        {
          Curve raw = new Curve().MoveTo(0.3f, 0.1f).LineTo(0.3f, 0.65f).LineTo(0.43f, 0.52f).
            LineTo(0.63f, 0.9f).LineTo(0.7f, 0.87f).LineTo(0.52f, 0.5f).LineTo(0.7f, 0.5f).LineTo(0.3f, 0.1f);

          float d = System.Math.Min(Width, Height);
          _arrow = raw.Project(new Prj(d));
        }
        return _arrow;
      }
    }

    public override void MapClicked(float x, float y)
    {
      _context.MapView.InitContextMenus(x, y);
    }

    internal override void OnDrawCore(MapButton canvasOwner, Canvas canvas)
    {
      Paint p = new Paint();
      p.Color = Color.White;

      canvas.Save();
      try
      {
        float width = canvasOwner.Width;
        float height = canvasOwner.Height;
        canvas.Translate(width / 2, height / 2);
        SymbolUtils.DrawCurve(canvas, Arrow, null, 3, false, true, p);
      }
      finally
      { canvas.Restore(); }
    }
  }
  public class SymbolButton : MapButton
  {
    private MainActivity _context;

    public Symbol Symbol { get; set; }
    public ColorRef Color { get; set; }
    public SymbolButton(Symbol symbol, MainActivity context)
      : base(context)
    {
      _context = context;
      Symbol = symbol;
      Color = new ColorRef { Color = MapView.DefaultColor };
    }

    public override void MapClicked(float x, float y)
    {
      _context.MapView.AddPoint(Symbol, Color, x, y);
    }

    internal override void OnDrawCore(MapButton canvasOwner, Canvas canvas)
    {
      //base.OnDraw(canvas);
      Utils.DrawSymbol(canvas, Symbol, Color.Color, canvasOwner.Width, canvasOwner.Height, 2);
    }
  }

  public class ColorButton : Button
  {
    public ColorRef Color { get; }

    public ColorButton(MainActivity context, ColorRef color)
      : base(context)
    {
      Color = color;
      SetBackgroundColor(Color.Color);
    }

    protected sealed override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {
      if (right - left > bottom - top)
      { SetWidth(bottom - top); }
      else
      { base.OnLayout(changed, left, top, right, bottom); }
    }
  }
}