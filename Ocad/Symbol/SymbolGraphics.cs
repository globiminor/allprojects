using Basics.Geom;
using System.Collections.Generic;
namespace Ocad.Symbol
{
  public class SymbolGraphicsCollection : List<SymbolGraphics>
  {
    public IBox Extent()
    {
      Box boxAll = null;
      foreach (var element in this)
      {
        IBox box = element.MapGeometry.Extent;
        if (boxAll == null)
        { boxAll = new Box(box); }
        else
        { boxAll.Include(box); }
      }

      return boxAll;
    }
  }

  public class SymbolGraphics
  {
    private SymbolGraphicsType _type;
    private bool _roundEnds;
    private int _color;
    private int _lineWidth;

    private GeoElement.Geom _geometry;

    public SymbolGraphicsType Type
    {
      get
      { return _type; }
      set
      { _type = value; }
    }
    public bool RoundEnds
    {
      get { return _roundEnds; }
      set { _roundEnds = value; }
    }

    public int Color
    {
      get { return _color; }
      set { _color = value; }
    }

    /// <summary>
    /// LineWidth in 0.01 mm
    /// </summary>
    public int LineWidth
    {
      get { return _lineWidth; }
      set { _lineWidth = value; }
    }

    /// <summary>
    /// Diameter of Circle / Dot in 0.01 mm
    /// </summary>
    public int Diameter
    {
      get
      {
        Polyline line;
        if (_geometry is GeoElement.Line l)
        { line = l.BaseGeometry; }
        else if (_geometry is GeoElement.Surface area)
        {
          if (area.BaseGeometry.Border.Count != 1)
          { return 0; }
          line = area.BaseGeometry.Border[0];
        }
        else
        { return 0; }

        if (line.Points.Count == 2 &&
            line.GetSegment(0) is IArc arc)
        {
          return (int)(2 * arc.Radius);
        }
        else
        { return 0; }
      }
    }

    public GeoElement.Geom MapGeometry
    {
      get { return _geometry; }
      set { _geometry = value; }
    }
  }
}