using Android.Graphics;
using Android.Widget;

namespace OMapScratch.Views
{
  public class SymbolButton : Button
  {
    public Symbol Symbol { get; set; }
    public ColorRef Color { get; set; }
    public SymbolButton(Symbol symbol, Android.Content.Context context)
      : base(context)
    {
      Symbol = symbol;
      Color = new ColorRef { Color = MapView.DefaultColor };
    }

    protected override void OnDraw(Canvas canvas)
    {
      //base.OnDraw(canvas);
      Paint p = new Paint();
      p.Color = Color.Color;

      canvas.Save();
      try
      {
        float scale = 2;
        canvas.Translate(Width / 2, Height / 2);
        canvas.Scale(scale, scale);
        Symbol sym = Symbol;
        SymbolType symTyp = sym.GetSymbolType();
        if (symTyp == SymbolType.Line)
        {
          float w = Width / (2 * scale) * 0.8f;
          SymbolUtils.DrawLine(canvas, sym, new Curve().MoveTo(-w, 0).LineTo(w, 0), p);
        }
        else if (symTyp == SymbolType.Point)
        {
          SymbolUtils.DrawPoint(canvas, sym, new Pnt { X = 0, Y = 0 }, p);
        }
        else if (symTyp == SymbolType.Text)
        {
          Paint.FontMetrics mtr = p.GetFontMetrics();
          p.TextSize = Height / 6;
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