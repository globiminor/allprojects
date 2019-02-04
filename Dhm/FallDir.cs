using Basics.Geom;

namespace Dhm
{
  public class FallDir
  {
    private readonly IPoint _point;
    private readonly double _dir;

    public FallDir(IPoint p, double direction)
    {
      _point = p;
      _dir = direction;
    }

    public IPoint Point => _point;
    public double Direction => _dir;
  }
}
