
namespace Basics.Geom.Projection
{
  public abstract class Projection
  {
    private Ellipsoid _ellipsoid;

    public Ellipsoid Ellipsoid
    {
      get { return _ellipsoid; }
    }
    public virtual void SetEllipsoid(Ellipsoid ellipsoid)
    {
      _ellipsoid = ellipsoid;
    }
    public abstract IPoint Gg2Prj(IPoint p);
    public abstract IPoint Prj2Gg(IPoint p);
  }
}
