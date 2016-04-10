using System.Data;
using Basics.Geom;

namespace TMap
{
  public class SymbolPartArea : SymbolPart
  {
    public SymbolPartArea(DataRow templateRow)
      : base(templateRow)
    {
      DrawLevel = 2;
    }

    public override int Topology
    {
      get { return 2; }
    }

    public override void Draw(IGeometry area, DataRow properties, IDrawable drawable)
    {
      drawable.DrawArea((Area)area.Project(drawable.Projection), this);
    }

    public override double Size()
    {
      return 0;
    }
  }
}