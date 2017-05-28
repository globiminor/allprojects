
using Android.Graphics;
using Android.Runtime;
using Android.Views;

namespace OMapScratch.Views
{
  public static class Utils
  {
    public static IMapView MapView { get; set; }
    public static void Try(System.Action action)
    {
      try
      {
        action();
      }
      catch (System.Exception e)
      {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Exception:");
        System.Exception c = e;
        while (c != null)
        {
          sb.AppendLine(c.Message);
          //sb.AppendLine(c.StackTrace);
          string pre = null;
          using (System.IO.TextReader r = new System.IO.StringReader(c.StackTrace))
          {
            for (string line = r.ReadLine(); line != null; line = r.ReadLine())
            {
              if (string.IsNullOrWhiteSpace(line))
              { continue; }
              if (!line.Contains("OMapScratch"))
              {
                pre = line;
                continue;
              }
              if (pre != null)
              { sb.AppendLine(pre); }
              sb.AppendLine(line);
              pre = null;
            }
          }
          c = c.InnerException;
          if (c != null)
          {
            sb.AppendLine();
            sb.AppendLine("[Inner Exception]");
          }
        }
        MapView?.ShowText(sb.ToString(), false);
      }
    }
    public static float GetMmPixel(View view)
    {
      return GetMmPixel(view.Resources);
    }
    public static float GetMmPixel(Android.Content.Res.Resources res)
    {
      return Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Mm, 1, res.DisplayMetrics);
    }

    public static float GetSurfaceOrientation()
    {
      IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
      SurfaceOrientation o = windowManager.DefaultDisplay.Rotation;

      float angle;
      if (o == SurfaceOrientation.Rotation0)
      { angle = 90; }
      else if (o == SurfaceOrientation.Rotation90)
      { angle = 180; }
      else if (o == SurfaceOrientation.Rotation180)
      { angle = 270; }
      else if (o == SurfaceOrientation.Rotation270)
      { angle = 0; }
      else
      { angle = 90; }
      return angle;
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