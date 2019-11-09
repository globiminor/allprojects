using Basics.Geom;
using Grid;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace TMap
{
  public class SymbolPartGrid : ISymbolPart
  {
    private readonly GridSymbolisation _sym;
    private GridMapData _data;

    public SymbolPartGrid(GridSymbolisation sym)
    {
      _sym = sym;
      IGrid<double> grd = new DataDoubleGrid(1, 1, typeof(double), 0, 0, 1);
      _data = GridMapData.FromData(grd);
    }
    void ISymbolPart.SetProperties(DataRow properties) { }

    public GridSymbolisation Symbolisation
    {
      get { return _sym; }
    }
    string ISymbolPart.GetDrawExpressions() => null;

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

    public void Draw(IGeometry geometry, IDrawable drawable)
    {
      drawable.DrawRaster(_data);
    }

    Color ISymbolPart.Color => Color.Gray;
  }
}
