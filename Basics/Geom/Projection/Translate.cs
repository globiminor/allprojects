
namespace Basics.Geom.Projection
{
  public class Translate : IProjection
  {
    private IPoint _translate;
    public Translate(IPoint translate)
    {
      _translate = translate;
    }

    public IPoint Project(IPoint point)
    {
      return PointOperator.Add(point, _translate);
    }
  }
}
