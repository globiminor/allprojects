using System.ComponentModel;
using System.Data;
using System.Drawing;
using Grid;
using Basics.Geom;

namespace TMap
{
  public class SymbolPartGrid : ISymbolPart
  {
    private readonly GridSymbolisation _sym;
    private GridMapData _data;

    public SymbolPartGrid(GridSymbolisation sym)
    {
      _sym = sym;
      IDoubleGrid grd = new DataDoubleGrid(1, 1, typeof(double), 0, 0, 1);
      _data = GridMapData.FromData(grd);
    }

    public GridSymbolisation Symbolisation
    {
      get { return _sym; }
    }

    public object EditProperties
    {
      get { return _sym; }
    }

    [Browsable(false)]
    public int Topology
    {
      get { return 2; }
    }

    [Browsable(false)]
    public int DrawLevel
    {
      get { return 0; }
    }

    [Browsable(false)]
    public double Size()
    {
      return 0;
    }

    public DataRow TemplateRow
    {
      get { return null; }
    }

    public void Draw(IGeometry geometry, DataRow row, IDrawable drawable)
    {
      drawable.DrawRaster(_data);
    }

    public Color LineColor
    {
      get { return Color.Gray; }
    }
  }
}
