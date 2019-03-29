
using System;
using System.Collections.Generic;

namespace OMapScratch
{
  public static class DrawableUtils
  {
    public const string Point = "p";
    public const string DirPoint = "dp";
    public const string Circle = "r";
    public const string Arc = "a";
    public const string MoveTo = "m";
    public const string LineTo = "l";
    public const string CubicTo = "c";

    internal static Dash GetDash(string dash)
    {
      if (string.IsNullOrEmpty(dash))
      { return null; }
      IList<string> parts = dash.Split(';');
      if (parts.Count > 3)
      { return null; }
      IList<string> intervalls = parts[0].Split(',');
      if (intervalls.Count < 1)
      { return null; }

      List<float> ints = new List<float>();
      foreach (var i in intervalls)
      {
        if (!float.TryParse(i, out float intervall))
        { return null; }
        ints.Add(intervall);
      }

      Dash d = new Dash { Intervals = ints.ToArray() };
      if (parts.Count > 1 && float.TryParse(parts[1], out float t))
      { d.StartOffset = t; }
      if (parts.Count > 2 && float.TryParse(parts[2], out t))
      { d.EndOffset = t; }

      return d;
    }
    internal static IDrawable GetGeometry(string geometry)
    {
      if (string.IsNullOrEmpty(geometry))
      { return null; }

      try
      {
        IList<string> parts = geometry.Split();
        IDrawable d = null;
        int i = 0;
        while (i < parts.Count)
        {
          string part = parts[i];
          if (string.IsNullOrEmpty(part))
          { i++; }
          else if (part == Point)
          {
            if (d != null)
            { throw new InvalidOperationException(); }
            Pnt p = new Pnt
            {
              X = float.Parse(parts[i + 1]),
              Y = float.Parse(parts[i + 2])
            };
            i += 3;
            d = p;
          }
          else if (part == DirPoint)
          {
            if (d != null)
            { throw new InvalidOperationException(); }
            DirectedPnt p = new DirectedPnt
            {
              X = float.Parse(parts[i + 1]),
              Y = float.Parse(parts[i + 2]),
              Azimuth = (float)(float.Parse(parts[i + 3]) * Math.PI / 180)
            };
            i += 4;
            d = p;
          }
          else if (part == Circle)
          {
            Curve c = (Curve)d ?? new Curve();
            c.Append(new Circle
            {
              Center = new Pnt { X = float.Parse(parts[i + 1]), Y = float.Parse(parts[i + 2]) },
              Radius = float.Parse(parts[i + 3])
            });
            i += 4;
            d = c;
          }
          else if (part == Arc)
          {
            Curve c = (Curve)d ?? new Curve();
            c.Append(new Circle
            {
              Center = new Pnt { X = float.Parse(parts[i + 1]), Y = float.Parse(parts[i + 2]) },
              Radius = float.Parse(parts[i + 3]),
              Azimuth = float.Parse(parts[i + 4]) / 180 * (float)Math.PI,
              Angle = float.Parse(parts[i + 5]) / 180 * (float)Math.PI
            });
            i += 6;
            d = c;
          }
          else if (part == MoveTo)
          {
            if (d != null)
            { throw new InvalidOperationException(); }
            Curve c = new Curve();
            c.MoveTo(float.Parse(parts[i + 1]), float.Parse(parts[i + 2]));
            i += 3;
            d = c;
          }
          else if (part == LineTo)
          {
            Curve c = (Curve)d;
            c.LineTo(float.Parse(parts[i + 1]), float.Parse(parts[i + 2]));
            i += 3;
          }
          else if (part == CubicTo)
          {
            Curve c = (Curve)d;
            c.CubicTo(
              float.Parse(parts[i + 1]), float.Parse(parts[i + 2]),
              float.Parse(parts[i + 3]), float.Parse(parts[i + 4]),
              float.Parse(parts[i + 5]), float.Parse(parts[i + 6])
              );
            i += 3;
          }
          else
          {
            throw new NotImplementedException($"Unhandled key '{part}'");
          }
        }
        return d;
      }
      catch (Exception e)
      {
        throw new Exception($"Error parsing '{geometry}'", e);
      }
    }
  }
  public class SymbolCurve
  {
    public float LineWidth { get; set; }
    public bool Fill { get; set; }
    public bool Stroke { get; set; }

    public Dash Dash { get; set; }

    public Curve Curve { get; set; }
  }
  public class Dash
  {
    public Dash Scale(float f)
    {
      Dash scaled = new Dash { Intervals = new float[Intervals.Length], StartOffset = f * StartOffset, EndOffset = f * EndOffset };
      for (int i = 0; i < Intervals.Length; i++)
      { scaled.Intervals[i] = f * Intervals[i]; }
      return scaled;
    }
    public float[] Intervals { get; set; }
    public float StartOffset { get; set; }
    public float EndOffset { get; set; }

    public double GetFactor(double fullLength)
    {
      double f = 1;
      if (fullLength > 0 && EndOffset != 0)
      {
        double sum = 0;
        foreach (var interval in Intervals)
        { sum += interval; }

        double l = fullLength - StartOffset + EndOffset;
        int d = (int)Math.Round(l / sum);
        if (d < 1)
        { d = 1; }
        f = l / (d * sum);
      }
      return f;
    }
    public IEnumerable<double> GetPositions(double fullLength = -1)
    {
      double f = GetFactor(fullLength);
      double pos = f * StartOffset;
      yield return pos;
      while (true)
      {
        foreach (var interval in Intervals)
        {
          pos += f * interval;
          yield return pos;
        }
      }
    }
  }


  public enum SymbolType { Point, Line, Text }
  public class Symbol
  {
    public string Id { get; set; }

    public string Text { get; set; }
    public List<SymbolCurve> Curves { get; set; }

    public SymbolType GetSymbolType()
    {
      int nCurves = Curves?.Count ?? 0;
      if (nCurves == 0)
      { return SymbolType.Text; }

      if (Curves[0].Curve == null || Curves[0].Dash != null)
      { return SymbolType.Line; }

      return SymbolType.Point;
    }
  }

  public static partial class SymbolUtils
  {
    private class Dash : IDisposable
    {
      private readonly IPaint _p;
      private bool _hasPathEffect;

      public Dash(SymbolCurve sym, float scale, float symbolScale, IPaint p, float fullLength)
      {
        _p = p;
        if (sym.Dash == null)
        { return; }

        if (sym.Dash.EndOffset != 0 && fullLength > 0)
        {
          float l = fullLength / symbolScale;
          scale = (float)(scale * sym.Dash.GetFactor(l));
        }

        int n = sym.Dash.Intervals.Length;
        float[] dash = new float[n];
        for (int i = 0; i < n; i++)
        { dash[i] = sym.Dash.Intervals[i] * scale; }

        p.SetDashEffect(dash, sym.Dash.StartOffset * scale);
        _hasPathEffect = true;
      }
      public void Dispose()
      {
        if (_hasPathEffect)
        {
          _p.ResetPathEffect();
          _hasPathEffect = false;
        }
      }
    }
    public static void DrawLine<T>(IGraphics<T> canvas, Symbol symbol, MatrixProps matrix, float symbolScale, Curve line, T p)
      where T : IPaint
    {
      float lineScale = matrix?.Scale * symbolScale ?? 1;
      bool drawn = false;
      foreach (var sym in symbol.Curves)
      {
        if (sym.Curve == null)
        {
          using (IPathMeasure m = canvas.GetPathMeasure(line))
          {
            float fullLength = m.Length;

            using (new Dash(sym, lineScale, symbolScale, p, fullLength))
            {
              DrawCurve(canvas, line, matrix?.Matrix, p, sym.LineWidth * lineScale, sym.Fill, sym.Stroke);
            }
            drawn = true;
          }
        }
        else if (sym.Dash != null && line.Count > 0)
        {
          using (IPathMeasure m = canvas.GetPathMeasure(line))
          {
            float l = m.Length;

            Symbol pntSym = new Symbol { Curves = new List<SymbolCurve> { sym }, };
            OMapScratch.Dash scaled = matrix != null ? sym.Dash.Scale(symbolScale) : sym.Dash;

            foreach (var dist in scaled.GetPositions(l))
            {
              if (dist > l)
              { break; }

              Lin tan = m.GetTangentAt((float)dist);

              DirectedPnt pnt = new DirectedPnt
              {
                X = tan.From.X,
                Y = tan.From.Y,
                Azimuth = (float)(Math.Atan2(tan.To.X - tan.From.X, tan.To.Y - tan.From.Y) + Math.PI / 2)
              };
              DrawPoint(canvas, pntSym, matrix, symbolScale, pnt, p);
            }
            drawn = true;
          }
        }
      }

      if (!drawn) // empty line with only symbols along line (i.e dotted line)
      {
        DrawCurve(canvas, line, matrix?.Matrix, p, lineScale, fill: false, stroke: true);
      }
    }

    private class ScalePrj : IProjection
    {
      private readonly float _scale;

      public ScalePrj(float scale)
      { _scale = scale; }

      public Pnt Project(Pnt pnt)
      {
        return new Pnt(pnt.X * _scale, pnt.Y * _scale);
      }
    }

    public static void DrawPoint<T>(IGraphics<T> canvas, Symbol sym, MatrixProps matrix, float symbolScale, Pnt point, T p)
      where T : IPaint
    {
      if (!string.IsNullOrEmpty(sym.Text))
      {
        DrawText(canvas, sym.Text, matrix, symbolScale, point, p);
        return;
      }
      canvas.Save();
      try
      {
        {
          Pnt t = matrix?.Matrix != null ? point.Project(new MatrixPrj(matrix.Matrix)) : point;
          canvas.Translate(t.X, t.Y);
        }
        float pntScale = matrix?.Scale * symbolScale ?? 1;
        canvas.Scale(1, -1);

        float azi = ((point as DirectedPnt)?.Azimuth ?? 0) + (matrix?.Rotate ?? 0);
        if (azi != 0)
        { canvas.Rotate_(azi); }

        ScalePrj prj = new ScalePrj(pntScale);
        foreach (var curve in sym.Curves)
        {
          DrawCurve(canvas, curve.Curve.Project(prj), null, p, curve.LineWidth * pntScale, curve.Fill, curve.Stroke);
        }
      }
      finally
      { canvas.Restore(); }
    }

    public static void DrawText<T>(IGraphics<T> canvas, string text, MatrixProps matrix, float symbolScale, Pnt point, T p)
      where T : IPaint
    {
      canvas.Save();
      try
      {
        p.SetStyle(fill: false, stroke: true);
        p.StrokeWidth = 0;
        p.TextAlignSetCenter();

        {
          Pnt t = matrix?.Matrix != null ? point.Project(new MatrixPrj(matrix.Matrix)) : point;
          canvas.Translate(t.X, t.Y);
        }
        if (matrix != null)
        {
          float f = matrix.Scale;
          canvas.Scale(f * symbolScale, f * symbolScale);
          float rot = matrix.Rotate;
          if (rot != 0)
          { canvas.Rotate_(-rot); }
        }
        canvas.DrawText(text, 0, 0 + p.TextSize / 2.5f, p);
      }
      finally
      { canvas.Restore(); }
    }

    public static void DrawCurve<T>(IGraphics<T> canvas, Curve curve, float[] matrix, T p, float lineWidth, bool fill, bool stroke)
      where T : IPaint
    {
      p.StrokeWidth = lineWidth;

      MatrixPrj prj = matrix != null ? new MatrixPrj(matrix) : null;

      if (curve.Count == 0)
      {
        Pnt pt = curve.From;
        pt = prj?.Project(pt) ?? pt;
        Lin lin = new Lin(new Pnt(pt.X - lineWidth, pt.Y), new Pnt(pt.X + lineWidth, pt.Y));
        DrawCurve(canvas, new Curve().Append(lin), null, p, lineWidth, fill, stroke);
        return;
      }

      if (fill && stroke)
      { p.SetStyle(fill: true, stroke: true); }
      else if (fill)
      { p.SetStyle(fill: true, stroke: false); }
      else if (lineWidth > 0)
      { p.SetStyle(fill: false, stroke: true); }
      else
      { p.SetStyle(fill: true, stroke: false); }

      canvas.DrawPath(curve, prj, p);
    }
  }

}
