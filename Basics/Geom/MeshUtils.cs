
using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public static class MeshUtils
  {
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
        IMeshLine next = line.GetNextTriLine();
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
          t = line.GetPreviousTriLine().Invers();
          h1 = h2;
          p1 = t.End;
        }
        else
        {
          f1 = (h1 - h2) / (h - h2);
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
