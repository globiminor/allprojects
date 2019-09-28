using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using TMap;
using Point = Basics.Geom.Point;

namespace TMapWin.Div
{
  public class TMapGraphics : ILevelDrawable
  {
    #region nested classes
    private class _Projection : IProjection
    {
      #region IProjection Members
      readonly double _x;
      readonly double _y;
      public _Projection(Rectangle bounds)
      {
        _x = bounds.X + bounds.Width / 2.0;
        _y = bounds.Y + bounds.Height / 2.0;
      }

      public IPoint Project(IPoint point)
      {
        return new Point2D(point.X + _x, _y - point.Y);
      }

      #endregion
    }
    #endregion

    // member variables
    private Box _extent = null;

    private Dictionary<ISymbolPart, Pen> _symbolPens = new Dictionary<ISymbolPart, Pen>();
    private Dictionary<ISymbolPart, Brush> _symbolBrushes = new Dictionary<ISymbolPart, Brush>();
    private Graphics _graphics;
    private readonly IProjection _prj;

    public ToolHandler SelectionEnd;

    private bool _breakDraw;
    public bool BreakDraw
    {
      get { return _breakDraw; }
      set { _breakDraw = value; }
    }

    public TMapGraphics(Graphics graphics, Rectangle bounds)
    {
      _graphics = graphics;
      _prj = new _Projection(bounds);
      _extent = new Box(
        new Point2D(bounds.X, bounds.Y),
        new Point2D(bounds.X + bounds.Width, bounds.Y + bounds.Height));
    }

    public int DrawLevels
    {
      get { return 1; }
    }

    #region ITMapDrawable Members

    public void BeginDraw()
    {
      _graphics.Clear(Color.White);
    }

    public void BeginDraw(MapData data)
    {
    }
    public void BeginDraw(ISymbolPart symbolPart, System.Data.DataRow properties)
    {
      symbolPart.SetProperties(properties);
      if (_symbolPens.TryGetValue(symbolPart, out Pen p) == false)
      {
        p = new Pen(symbolPart.Color);
        _symbolPens.Add(symbolPart, p);
      }
      else { p.Color = symbolPart.Color; }

      if (_symbolBrushes.TryGetValue(symbolPart, out Brush b) == false)
      {
        b = new SolidBrush(symbolPart.Color);
        _symbolBrushes.Add(symbolPart, b);
      }
      else if (b is SolidBrush sb) { sb.Color = symbolPart.Color; }
    }

    public void Draw(MapData data)
    {
      throw new Exception("Not implemented");
    }

    public void Flush()
    {
      _graphics.Flush();
    }
    public void DrawLine(Polyline line, ISymbolPart symbolPart)
    {
      if (_symbolPens.ContainsKey(symbolPart) == false)
      {
        Pen p = new Pen(symbolPart.Color);
        _symbolPens.Add(symbolPart, p);
      }

      Pen pen = _symbolPens[symbolPart];
      DrawLine(_graphics, line, pen);
    }

    public static void DrawLine(Graphics graphics, Polyline line, Pen pen)
    {
      if (line == null)
      { return; }

      GraphicsPath pntList = GetPath(line);
      if (pntList == null)
      { return; }

      graphics.DrawPath(pen, pntList);
    }

    public void DrawArea(Area area, ISymbolPart symbolPart)
    {
      Brush brush = _symbolBrushes[symbolPart];
      DrawArea(_graphics, area, brush);
    }

    public static void DrawArea(Graphics graphics, Area area, Brush brush)
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

    public void DrawRaster(GridMapData grid)
    {
    }

    public IProjection Projection
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _prj; }
    }

    public Box Extent
    {
      get
      { return _extent; }
    }

    public void SetExtent(Point point, double scaleFactor)
    {
      if (_extent == null)
      { return; }

      Point max = Point.CastOrCreate(_extent.Max);
      Point d = (0.5 / scaleFactor) * (max - _extent.Min);
      if (point == null)
      { point = 0.5 * (_extent.Min + max); }
      SetExtent(new Box(point - d, point + d));
    }
    public void SetExtent(IBox proposedExtent)
    {
    }

    public void EndDraw(ISymbolPart symbolPart)
    {
      if (_symbolPens.TryGetValue(symbolPart, out Pen pen))
      {
        _symbolPens.Remove(symbolPart);
        pen.Dispose();
      }
    }
    public void EndDraw(MapData data)
    {
    }

    public void EndDraw()
    {
    }

    #endregion
  }
}
