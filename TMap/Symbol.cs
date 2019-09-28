using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Data;

namespace TMap
{
  /// <summary>
  /// Summary description for Symbol.
  /// </summary>
  public class Symbol : List<ISymbolPart>
  {
    private readonly int _topology;
    public Symbol(int topology)
    {
      _topology = topology;
    }

    public Symbol(ISymbolPart part)
      : this(part.Topology)
    {
      Add(part);
    }

    public static Symbol Create(int topology, bool withSymbolPart)
    {
      Symbol sym = new Symbol(topology);

      if (!withSymbolPart)
      { return sym; }

      if (sym._topology == 0)
      { sym.Add(new SymbolPartPoint()); }
      else if (sym._topology == 1)
      { sym.Add(new SymbolPartLine()); }
      else if (sym._topology == 2)
      { sym.Add(new SymbolPartArea()); }
      else
      { throw new Exception(string.Format("Unhandled topology {0}", sym._topology)); }

      return sym;
    }
    public int Topology
    {
      get { return _topology; }
    }
    public static Symbol DefaultPoint()
    {
      Symbol sym = Create(0, withSymbolPart: true);
      Polyline l = SquareLine();

      SymbolPartPoint part = (SymbolPartPoint)sym[0];
      part.SymbolLine = l;

      return sym;
    }

    public static Polyline SquareLine()
    {
      Polyline l = new Polyline();
      l.Add(new Point2D(-2, -2));
      l.Add(new Point2D(2, -2));
      l.Add(new Point2D(2, 2));
      l.Add(new Point2D(-2, 2));
      l.Add(new Point2D(-2, -2));
      return l;
    }

    /// <summary>
    /// Draws a default geometry to drawable
    /// </summary>
    /// <param name="drawable"></param>
    public void Draw(IDrawable drawable)
    {
      IGeometry geom;
      if (_topology == 0)
      { geom = new Point2D(0, 0); }
      else if (_topology == 1)
      {
        double d = (drawable.Extent.Max.X - drawable.Extent.Min.X) / 2.0;
        geom = Polyline.Create(new[]
          { new Point2D(-d + 4,0),
            new Point2D(-d + 20,0),
            new Point2D( d - 20,0),
            new Point2D( d - 4,0) });
      }
      else if (_topology == 2)
      {
        double d = (drawable.Extent.Max.X - drawable.Extent.Min.X) / 2.0;
        double h = (drawable.Extent.Max.Y - drawable.Extent.Min.Y) / 2.0;
        geom = new Area(Polyline.Create(new[]
          { new Point2D(-d + 4,-5),
            new Point2D( d - 4, -5),
            new Point2D( d - 4, 5),
            new Point2D(-d + 4, 5),
            new Point2D(-d + 4,-5)
          }));
      }
      else
      { throw new NotImplementedException("Unhandled topology " + _topology); }
      Draw(geom, null, drawable);
    }

    /// <summary>
    /// Draws geometry to drawable
    /// </summary>
    public void Draw(IGeometry geometry, DataRow properties, IDrawable drawable)
    {
      foreach (var pPart in this)
      {
        pPart.SetProperties(properties);
        drawable.BeginDraw(pPart, properties);
        pPart.Draw(geometry, drawable);
        drawable.EndDraw(pPart);
      }
    }

    public double Size()
    {
      double dSize = 0;
      foreach (var part in this)
      {
        double dSizePart = part.Size();
        if (dSizePart > dSize)
        { dSize = dSizePart; }
      }
      return dSize;
    }
  }
}
