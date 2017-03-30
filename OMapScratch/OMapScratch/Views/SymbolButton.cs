using Android.Graphics;
using Android.Widget;

namespace OMapScratch.Views
{
  public abstract class MapButton : Button
  {
    public MapButton(Android.Content.Context context)
      : base(context)
    { }

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

    private Curve _arrow;
    private Curve Arrow
    {
      get
      {
        if (_arrow == null)
        {
          int d = System.Math.Min(Width, Height);
          _arrow = new Curve().MoveTo(-d / 3f, d / 6f).LineTo(-d / 3f, -d / 3f).LineTo(d / 6f, -d / 3f).
            LineTo(0, -d / 6f).LineTo(d / 3f, d / 3f).LineTo(-d / 6f, 0).LineTo(-d / 3f, d / 6f);
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
        SymbolUtils.DrawCurve(canvas, Arrow, 3, false, true, p);
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
      Paint p = new Paint();
      p.Color = Color.Color;

      canvas.Save();
      try
      {
        float width = canvasOwner.Width;
        float height = canvasOwner.Height;

        float scale = 2;
        canvas.Translate(width / 2, height / 2);
        canvas.Scale(scale, scale);
        Symbol sym = Symbol;
        SymbolType symTyp = sym.GetSymbolType();
        if (symTyp == SymbolType.Line)
        {
          float w = width / (2 * scale) * 0.8f;
          SymbolUtils.DrawLine(canvas, sym, new Curve().MoveTo(-w, 0).LineTo(w, 0), p);
        }
        else if (symTyp == SymbolType.Point)
        {
          SymbolUtils.DrawPoint(canvas, sym, new Pnt { X = 0, Y = 0 }, p);
        }
        else if (symTyp == SymbolType.Text)
        {
          Paint.FontMetrics mtr = p.GetFontMetrics();
          p.TextSize = height / 6;
          SymbolUtils.DrawText(canvas, sym.Text, new Pnt { X = 0, Y = 0 }, p);
        }
        else
        { throw new System.NotImplementedException($"unknown SymbolType {symTyp}"); }
      }
      finally
      { canvas.Restore(); }
    }
  }
}