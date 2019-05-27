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

#pragma warning disable CS0672 // Member 'SymbolPartArea.Draw(IGeometry, DataRow, IDrawable)' overrides obsolete member 'SymbolPart.Draw(IGeometry, DataRow, IDrawable)'. Add the Obsolete attribute to 'SymbolPartArea.Draw(IGeometry, DataRow, IDrawable)'.
    public override void Draw(IGeometry area, DataRow properties, IDrawable drawable)
#pragma warning restore CS0672 // Member 'SymbolPartArea.Draw(IGeometry, DataRow, IDrawable)' overrides obsolete member 'SymbolPart.Draw(IGeometry, DataRow, IDrawable)'. Add the Obsolete attribute to 'SymbolPartArea.Draw(IGeometry, DataRow, IDrawable)'.
    {
      drawable.DrawArea((Area)area.Project(drawable.Projection), this);
    }

    public override double Size()
    {
      return 0;
    }
  }
}