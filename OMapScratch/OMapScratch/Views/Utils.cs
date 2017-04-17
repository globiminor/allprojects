
using Android.Graphics;
using Android.Views;

namespace OMapScratch.Views
{
  public static class Utils
  {
    public static float GetMmPixel(View view)
    {
      return GetMmPixel(view.Resources);
    }
    public static float GetMmPixel(Android.Content.Res.Resources res)
    {
      return Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Mm, 1, res.DisplayMetrics);
    }

    public static void DrawSymbol(Canvas canvas, Symbol sym, Color color, float width, float height, float scale)
    {
      Paint p = new Paint();
      p.Color = color;

      canvas.Save();
      try
      {
        canvas.Translate(width / 2, height / 2);
        canvas.Scale(scale, scale);

        SymbolType symTyp = sym.GetSymbolType();
        if (symTyp == SymbolType.Line)
        {
          float w = width / (2 * scale) * 0.8f;
          SymbolUtils.DrawLine(canvas, sym, null, 1, new Curve().MoveTo(-w, 0).LineTo(w, 0), p);
        }
        else if (symTyp == SymbolType.Point)
        {
          SymbolUtils.DrawPoint(canvas, sym, null, 1, new Pnt(0, 0), p);
        }
        else if (symTyp == SymbolType.Text)
        {
          Paint.FontMetrics mtr = p.GetFontMetrics();
          p.TextSize = height / 6;
          SymbolUtils.DrawText(canvas, sym.Text, null, 1, new Pnt { X = 0, Y = 0 }, p);
        }
        else
        { throw new System.NotImplementedException($"unknown SymbolType {symTyp}"); }
      }
      finally
      { canvas.Restore(); }
    }
  }
}