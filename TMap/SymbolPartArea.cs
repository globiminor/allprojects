using System.Data;
using Basics.Geom;

namespace TMap
{
  public class SymbolPartArea : SymbolPart
  {
    public SymbolPartArea()
    {
      DrawLevel = 2;
    }

    public override int Topology
    {
      get { return 2; }
    }

    public override void Draw(IGeometry area, IDrawable drawable)
    {
      drawable.DrawArea((Area)area.Project(drawable.Projection), this);
    }
    public override string GetDrawExpressions() => null;

    public override double Size()
    {
      return 0;
    }
  }
}