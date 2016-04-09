
namespace Basics.Geom.Projection
{
  public class ToXY : IProjection
  {
    #region IProjection Members

    public IPoint Project(IPoint point)
    {
      return new Point2D(point.X, point.Y);
    }

    #endregion
  }
}
