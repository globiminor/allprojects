
namespace Basics.Geom.Projection
{
  public class Translate : IProjection
  {
    private readonly IPoint _translate;
    public Translate(IPoint translate)
    {
      _translate = translate;
    }

    public IPoint Project(IPoint point)
    {
      return PointOp.Add(point, _translate);
    }
  }
}
