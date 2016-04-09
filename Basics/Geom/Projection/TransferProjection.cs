
namespace Basics.Geom.Projection
{
  public class TransferProjection : IProjection
  {
    private Projection _prj0;
    private Projection _prj1;

    private Datum _datum;

    public TransferProjection(Projection prj0, Projection prj1)
    {
      _prj0 = prj0;
      _prj1 = prj1;

      _datum = prj0.Ellipsoid.Datum - prj1.Ellipsoid.Datum;
    }

    public IPoint Project(IPoint p)
    {
      IPoint p1 = _prj0.Prj2Gg(p);
      Point3D p3 = _prj0.Ellipsoid.Elliptic2Kartesic(new Point3D(p1.X, p1.Y, 0.0));
      p3 = _datum.Transfer(p3);
      p3 = _prj1.Ellipsoid.Kartesic2Elliptic(p3);
      p1 = p3;
      p1 = _prj1.Gg2Prj(p1);

      return p1;
    }
  }
}
