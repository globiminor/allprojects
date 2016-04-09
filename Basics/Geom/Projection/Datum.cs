
namespace Basics.Geom.Projection
{
  public class Datum
  {
    Point3D _center;
    Point3D _rotation;
    double _scale = 1.0;

    public static Datum operator -(Datum d0, Datum d1)
    {
      Datum d = new Datum();
      if (d0._center != null || d1._center != null)
      {
        if (d0._center == null)
        { d._center = (Point3D)(-d1._center); }
        else if (d1._center == null)
        { d._center = d0._center.Clone(); }
        else
        { d._center = (Point3D)(d0._center - d1._center); }
      }
      if (d0._rotation != null || d1._rotation != null)
      {
        if (d0._rotation == null)
        { d._rotation = (Point3D)(-d1._rotation); }
        else if (d1._rotation == null)
        { d._rotation = d0._rotation.Clone(); }
        else
        { d._rotation = (Point3D)(d0._rotation - d1._rotation); }
      }

      d._scale = d0._scale / d1._scale;

      return d;
    }

    public Point3D Center
    {
      get { return _center; }
    }
    public Point3D Rotation
    {
      get { return _rotation; }
    }

    public Point3D Transfer(Point3D s)
    {
      Point3D t = s;
      bool n = false;
      if (_center != null)
      {
        t = (Point3D)(t + _center);
        n = true;
      }
      if (_scale != 1)
      {
        t = (Point3D)(t + (_scale - 1) * s);
        n = true;
      }
      if (_rotation != null)
      {
        if (n == false)
        { t = t.Clone(); }
        t.X += -_rotation.Z * s.Y + _rotation.Y * s.Z;
        t.Y += _rotation.Z * s.X - _rotation.X * s.Z;
        t.Z += -_rotation.Y * s.X + _rotation.X * s.Y;
      }

      return t;
    }
    public class ITRS : Datum
    {
      public ITRS()
      {
        _center = new Point3D(0, 0, 0);
      }
    }
    public class ChTrs95 : Datum
    {
      public ChTrs95()
      {
        _center = new Point3D(0, 0, 0);
      }
    }
    public class Ch1903 : Datum
    {
      public Ch1903()
      {
        _center = new Point3D(674.374, 15.056, 405.346);
      }
    }
  }
}
