using Android.Graphics;
using Android.Widget;

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

    protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {

      base.OnLayout(changed, left, top, System.Math.Min(right, left + (bottom - top)), bottom);
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
}