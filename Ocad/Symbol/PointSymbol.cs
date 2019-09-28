using Basics.Geom;

namespace Ocad.Symbol
{
  public class PointSymbol : BaseSymbol
  {
    public PointSymbol(int symbolNumber)
      :
        base(symbolNumber, SymbolType.Point)
    { }

    public GeometryCollection GetSymbolGeometry(Setup setup)
    {
      GeometryCollection lst = new GeometryCollection();
      foreach (var graphics in Graphics)
      {
        IGeometry geom = graphics.MapGeometry.Project(setup.Map2Prj).GetGeometry();
        lst.Add(geom);
      }
      return lst;
    }
    protected override int GetMainColorBase()
    {
      foreach (var graphics in Graphics)
      {
        return graphics.Color;
      }
      return -1;
    }
  }
}