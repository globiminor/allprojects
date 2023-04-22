using Basics.Geom;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Basics.Forms
{
  public static class DrawUtils
  {
    public static void DrawLine(Graphics graphics, Polyline line, Pen pen)
    {
      if (line == null)
      { return; }

      GraphicsPath pntList = GetPath(line);
      if (pntList == null)
      { return; }

      graphics.DrawPath(pen, pntList);
    }

    public static void DrawArea(Graphics graphics, Surface area, Brush brush)
    {
      if (area == null)
      { return; }

      GraphicsPath path = new GraphicsPath();
      foreach (var polyline in area.Border)
      {
        GraphicsPath part = GetPath(polyline);
        if (part != null)
        { path.AddPath(part, false); }
      }

      if (path.PointCount > 0)
      { graphics.FillPath(brush, path); }
    }

    private static PointF GetPoint(IPoint p)
    {
      PointF f = new PointF((float)p.X, (float)p.Y);
      return f;
    }

    private static GraphicsPath GetPath(Polyline line)
    {
      GraphicsPath path = new GraphicsPath();

      foreach (var seg in line.EnumSegments())
      {
        if (seg is Line l)
        {
          path.AddLine(GetPoint(l.Start), GetPoint(l.End));
        }
        else if (seg is Bezier b)
        {
          path.AddBezier(GetPoint(b.Start), GetPoint(b.P1), GetPoint(b.P2), GetPoint(b.End));
        }
        else if (seg is Arc a)
        {
          if (a.Radius > 0)
          {
            float r = (float)a.Radius;
            RectangleF box = new RectangleF((float)a.Center.X - r, (float)a.Center.Y - r, 2 * r, 2 * r);
            path.AddArc(box, (float)(180 * a.DirStart / Math.PI), (float)(180 * a.Angle / Math.PI));
          }
        }
      }
      if (path.PointCount == 0)
      { return null; }
      return path;
    }

  }
}
