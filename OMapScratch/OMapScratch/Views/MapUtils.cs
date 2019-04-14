
using Android.Graphics;

namespace OMapScratch.Views
{
  public class MapUtils
  {
    public static Matrix GetPairedMatrix(Matrix matrix, Matrix mapMatrix)
    {
      float d = 1000;
      float[] pts = new float[]
      {
        0, 0,
        d, 0,
        0, d
      };
      using (Matrix invMat = new Matrix())
      {
        matrix.Invert(invMat);
        invMat.MapPoints(pts);
      }
      mapMatrix.MapPoints(pts);

      float xx = (pts[2] - pts[0]) / d;
      float yx = (pts[3] - pts[1]) / d;
      float xy = (pts[4] - pts[0]) / d;
      float yy = (pts[5] - pts[1]) / d;

      using (Matrix invPairedMat = new Matrix())
      {
        invPairedMat.SetValues(new float[] { xx, xy, pts[0], yx, yy, pts[1], 0, 0, 1 });

        Matrix pairedMat = new Matrix();
        invPairedMat.Invert(pairedMat);

        return pairedMat;
      }
    }

    public static void DrawSymbol(Graphics canvas, Symbol sym, Color color, float width, float height, float scale)
    {
      using (GraphicsPaint p = canvas.CreatePaint())
      {
        p.Paint.Color = color;

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
}