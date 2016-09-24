
using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public static class MeshUtils
  {
    public static List<IMeshLine> GetPointLines(IMeshLine start, IComparer<IMeshLine> comparer)
    {
      IMeshLine t = start;
      IMeshLine p;

      List<IMeshLine> lines = new List<IMeshLine>();
      lines.Add(start);
      while ((p = t.GetPreviousTriLine()) != null)
      {
        t = p.Invers();
        if (comparer.Compare(start, t) == 0)
        { break; }

        lines.Add(t);
      }

      if (p == null)
      {
        lines.Add(null);
        lines.Reverse();
        t = start;
        while ((p = t.Invers().GetNextTriLine()) != null)
        {
          lines.Add(p);
          t = p;
        }
        lines.Reverse();
      }

      return lines;
    }

    public static Polyline GetContour(IMeshLine line, double fraction, IComparer<IMeshLine> comparer)
    {
      bool complete;
      Polyline start = GetContour(line, fraction, comparer, out complete);
      if (complete)
      { return start; }

      Polyline end = GetContour(line.Invers(), (1 - fraction), comparer, out complete);
      if (end == null)
      { return start; }

      end.Invert();
      if (start != null)
      {
        bool first = true;
        foreach (IPoint p in start.Points)
        {
          if (first)
          {
            first = false;
            continue;
          }
          end.Add(p);
        }
      }
      return null;
    }

    public static char GetChar(IMeshLine line, out double angle)
    {
      angle = Math.Atan2(line.End.Y - line.Start.Y, line.End.X - line.Start.X);
      char cAngle = GetChar(angle);
      return cAngle;
    }

    private static char GetChar(double angle)
    {
      char cAngle;
      if (angle < -7.0 / 8.0 * Math.PI)
      { cAngle = '\u2190'; }
      else if (angle < -5.0 / 8.0 * Math.PI)
      { cAngle = '\u2199'; }
      else if (angle < -3.0 / 8.0 * Math.PI)
      { cAngle = '\u2193'; }
      else if (angle < -1.0 / 8.0 * Math.PI)
      { cAngle = '\u2198'; }
      else if (angle < 1.0 / 8.0 * Math.PI)
      { cAngle = '\u2192'; }
      else if (angle < 3.0 / 8.0 * Math.PI)
      { cAngle = '\u2197'; }
      else if (angle < 5.0 / 8.0 * Math.PI)
      { cAngle = '\u2191'; }
      else if (angle < 7.0 / 8.0 * Math.PI)
      { cAngle = '\u2196'; }
      else
      { cAngle = '\u2190'; }

      return cAngle;
    }

    private static Polyline GetContour(IMeshLine line, double fraction, IComparer<IMeshLine> comparer, out bool complete)
    {
      Point p0 = Point.CastOrCreate(line.Start);
      IPoint p1 = line.End;
      double h0 = p0.Z;
      double h1 = p1.Z;

      if (h0 == h1)
      {
        complete = true;
        return null;
      }

      Polyline contour = new Polyline();
      double h = h0 + (h1 - h0) * fraction;

      Point pFraction2d = p0 + fraction * (p1 - p0);
      Point pFraction3d = new Point3D(pFraction2d.X, pFraction2d.Y, h);
      contour.Add(pFraction3d);

      IMeshLine t = null;
      while (!EqualLine(line, t, comparer))
      {
        if (t == null)
        { t = line; }
        IMeshLine next = t.GetNextTriLine();
        if (next == null)
        {
          complete = false;
          return contour;
        }

        double h2 = next.End.Z;
        double f1;
        if ((h2 >= h) == (h1 >= h))
        {
          f1 = (h - h0) / (h2 - h0);
          t = t.GetPreviousTriLine().Invers();
          h1 = h2;
          p1 = t.End;
        }
        else
        {
          f1 = (h - h2) / (h1 - h2);
          t = next.Invers();
          h0 = h2;
          p0 = Point.CastOrCreate(t.Start);
        }

        pFraction2d = p0 + f1 * (p1 - p0);
        pFraction3d = new Point3D(pFraction2d.X, pFraction2d.Y, h);
        contour.Add(pFraction3d);
      }
      complete = true;
      return contour;
    }

    private static bool EqualLine<T>(T x, T y, IComparer<T> comparer)
    {
      if (ReferenceEquals(x, y))
      { return true; }
      if (comparer == null)
      { return false; }
      return comparer.Compare(x, y) == 0;
    }
  }
}
