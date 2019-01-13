using Basics.Geom;

namespace Ocad.Symbol
{
  public class PointSymbol : BaseSymbol
  {
    public PointSymbol(int symbolNumber)
      :
        base(symbolNumber)
    { }

    public GeometryCollection GetSymbolGeometry(Setup setup)
    {
      GeometryCollection lst = new GeometryCollection();
      foreach (var graphics in Graphics)
      {
        IGeometry geom = graphics.MapGeometry.Project(setup.Map2Prj);
        lst.Add(geom);
      }
      return lst;
    }

  }
}