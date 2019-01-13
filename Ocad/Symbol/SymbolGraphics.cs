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

    private IGeometry _geometry;

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
        if (_geometry is Polyline)
        { line = (Polyline)_geometry; }
        else if (_geometry is Area area)
        {
          if (area.Border.Count != 1)
          { return 0; }
          line = area.Border[0];
        }
        else
        { return 0; }

        if (line.Segments.Count == 1 &&
            line.Segments.First is Arc)
        {
          return (int)((Arc)line.Segments.First).Diameter;
        }
        else
        { return 0; }
      }
    }

    public IGeometry MapGeometry
    {
      get { return _geometry; }
      set { _geometry = value; }
    }
  }
}