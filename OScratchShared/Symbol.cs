
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

}
