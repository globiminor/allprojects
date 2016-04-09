
namespace Basics.Geom.Network
{
  public class NetPoint : NetElement
  {
    private NNetPoint _point;

    public NetPoint(IPoint point)
    {
      _point = new NNetPoint(point);
    }

    protected override NNetPoint NetPoint__
    {
      get { return _point; }
    }

  }
}
