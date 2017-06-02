using System;
using System.Windows.Media;

namespace OMapScratch
{
  partial class Map
  {
    private System.Collections.Generic.List<ColorRef> GetDefaultColors()
    {
      return new System.Collections.Generic.List<ColorRef>
      {
        new ColorRef { Id = "Bl", Color = Colors.Black },
        new ColorRef { Id = "Gy", Color = Colors.Gray },
        new ColorRef { Id = "Bw", Color = Colors.Brown },
        new ColorRef { Id = "Y", Color = Colors.Yellow },
        new ColorRef { Id = "G", Color = Colors.Green },
        new ColorRef { Id = "K", Color = Colors.Khaki },
        new ColorRef { Id = "R", Color = Colors.Red },
        new ColorRef { Id = "B", Color = Colors.Blue } };
    }
  }
  partial class ColorRef
  {
    public Color Color { get; set; }
  }
  partial class XmlColor
  {
    private void GetEnvColor(ColorRef color)
    {
      color.Color = new Color { R = Red, G = Green, B = Blue, A = 255 };
    }

    private void SetEnvColor(ColorRef color)
    {
      Red = color.Color.R;
      Green = color.Color.G;
      Blue = color.Color.B;
    }
  }
  public partial interface ISegment
  {
    void Init(PathFigure path, float[] matrix);
    void AppendTo(PathFigure path, float[] matrix);
  }

  public partial class Lin : ISegment
  {
    void ISegment.Init(PathFigure path, float[] matrix)
    {
      Pnt t = From.Trans(matrix);
      path.StartPoint = new System.Windows.Point(t.X, t.Y);
    }

    void ISegment.AppendTo(PathFigure path, float[] matrix)
    {
      Pnt t = To.Trans(matrix);
      path.Segments.Add(new LineSegment(new System.Windows.Point(t.X, t.Y), true));
    }
  }
  public partial class Circle
  {
    void ISegment.Init(PathFigure path, float[] matrix)
    {
      ISegment s = this;
      Pnt t = s.From.Trans(matrix);
      path.StartPoint = new System.Windows.Point(t.X, t.Y);
      //path.StartPoint = new System.Windows.Point(20, 0);
    }
    void ISegment.AppendTo(PathFigure path, float[] matrix)
    {
      ISegment s = this;

      double scale = matrix?[0] ?? 1;
      SweepDirection dir = Ang > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
      Pnt m = s.At(0.5f).Trans(matrix);
      path.Segments.Add(new ArcSegment(new System.Windows.Point(m.X, m.Y),
        new System.Windows.Size(scale * Radius, scale * Radius), 180, false, dir, true));

      Pnt t = s.To.Trans(matrix);

      path.Segments.Add(new ArcSegment(new System.Windows.Point(t.X, t.Y),
        new System.Windows.Size(scale * Radius, scale * Radius), 0, false, dir, true));

      //path.Segments.Add(new ArcSegment(new System.Windows.Point(20, 10),
      //  new System.Windows.Size(50, 20), 0, true, SweepDirection.Clockwise, true));

      // path.Segments.Add(new LineSegment(new System.Windows.Point(50,50), false));
    }
  }

  public partial class Bezier
  {
    void ISegment.Init(PathFigure path, float[] matrix)
    {
      Pnt t = From.Trans(matrix);
      path.Segments.Add(new LineSegment(new System.Windows.Point(t.X, t.Y), true));
    }
    void ISegment.AppendTo(PathFigure path, float[] matrix)
    {
      Pnt i0 = I0.Trans(matrix);
      Pnt i1 = I1.Trans(matrix);
      Pnt to = To.Trans(matrix);
      path.Segments.Add(new BezierSegment(new System.Windows.Point(i0.X, i0.Y),
        new System.Windows.Point(i1.X, i1.Y), new System.Windows.Point(to.X, to.Y), true));
    }
  }


  public static class SymbolUtils
  {
    private static float _fontSize = 12;

    public static void DrawSymbol(DrawingContext dc, Symbol symbol, Color color, float width, float height, float scale)
    {
      Brush br = new SolidColorBrush(color);
      SymbolType symTyp = symbol.GetSymbolType();
      if (symTyp == SymbolType.Point)
      {
        float[] matrix = GetMatrix(width, height, scale);

        foreach (SymbolCurve curve in symbol.Curves)
        {
          Pen pen = new Pen(br, curve.LineWidth * scale);
          DrawCurve(dc, curve.Curve, matrix, curve.LineWidth * scale, curve.Fill, curve.Stroke, pen);
        }
      }
      else if (symTyp == SymbolType.Text)
      {
        float[] matrix = GetMatrix(width, height, scale);
        DrawText(dc, symbol.Text, matrix, new Pnt(0, 0.5f * _fontSize), br);
      }

      else if (symTyp == SymbolType.Line)
      {
        //float w = width / (2 * scale) * 0.8f;
        float lineWidth = symbol.Curves[0].LineWidth;
        Pen pen = new Pen(br, lineWidth * scale);
        DrawCurve(dc, new Curve().MoveTo(3, height / 2).LineTo(width - 3, height / 2), 
          null,  lineWidth * scale, false, true, pen);
      }
    }

    public static void DrawLine(DrawingContext canvas, Symbol sym, float[] matrix, float symbolScale, Curve line, Pen p)
    {
      float lineScale = matrix?[0] / symbolScale ?? 1;
      foreach (SymbolCurve curve in sym.Curves)
      { DrawCurve(canvas, line, matrix, curve.LineWidth * lineScale, curve.Fill, curve.Stroke, p); }
    }

    public static void DrawPoint(DrawingContext canvas, Symbol sym, float[] matrix, float symbolScale, Pnt point, Pen p)
    {
      if (!string.IsNullOrEmpty(sym.Text))
      {
        DrawText(canvas, sym.Text, matrix, point, p.Brush);
        return;
      }

      Pnt t = point.Trans(matrix);
      foreach (SymbolCurve curve in sym.Curves)
      { DrawCurve(canvas, curve.Curve, matrix, curve.LineWidth, curve.Fill, curve.Stroke, p); }
    }

    public static void DrawText(DrawingContext canvas, string text, float[] matrix, Pnt point, Brush brush)
    {
      Pnt t = point.Trans(matrix);

      FormattedText fTxt = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
        new Typeface("Arial"), _fontSize * matrix[0], brush);
      fTxt.TextAlignment = System.Windows.TextAlignment.Center;
      canvas.DrawText(fTxt, new System.Windows.Point(t.X, t.Y));
    }

    public static void DrawCurve(DrawingContext canvas, Curve curve, float[] matrix, float lineWidth, bool fill, bool stroke, Pen p)
    {
      if (curve.Count == 0)
      {
        Pnt pt = curve.From;
        return;
      }

      PathGeometry geom = new PathGeometry();
      PathFigure path = new PathFigure();
      curve[0].Init(path, matrix);
      foreach (ISegment segment in curve)
      { segment.AppendTo(path, matrix); }

      p.Thickness = lineWidth;
      Brush br = null;
      Pen pn = null;
      if (fill && stroke)
      {
        br = p.Brush;
        pn = p;
      }
      else if (fill)
      { br = p.Brush; }
      else if (lineWidth > 0)
      { pn = p; }
      else
      { br = p.Brush; }

      geom.Figures.Add(path);

      canvas.DrawGeometry(br, p, geom);
    }

    private static float[] GetMatrix(float width, float height, float scale)
    {
      float[] matrix = new float[9];
      matrix[0] = scale;
      matrix[4] = -scale;
      matrix[2] = width / 2;
      matrix[5] = height / 2;

      return matrix;
    }
  }
}