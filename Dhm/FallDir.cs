using Basics.Geom;

namespace Dhm
{
  public class FallDir
  {
    Point2D _point;
    double _dir;

    public FallDir(Point2D p, double direction)
    {
      _point = p;
      _dir = direction;
    }

    public Point2D Point
    { get { return _point; } }
    public double Direction
    { get { return _dir; } }
  }
}
