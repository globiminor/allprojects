using System;

namespace Basics.Geom.Network
{
  public abstract class NetElement
  {
    #region nested classes
    protected class NNetPoint
    {
      IPoint _point;

      public NNetPoint(IPoint point)
      { _point = point; }

      public IPoint Point
      { get { return _point; } }
    }
    #endregion

    internal NetElement()
    { }

    public static NetElement Create(IGeometry geom)
    {
      if (geom is IPoint)
      { return new NetPoint((IPoint)geom); }
      else if (geom is Polyline)
      { return new DirectedRow((Polyline)geom, false); }
      else
      { throw new ArgumentException("Invalid geometry type " + geom.GetType()); }
    }

    public IPoint NetPoint
    { get { return NetPoint__.Point; } }
    protected abstract NNetPoint NetPoint__ { get; }

  }
}
