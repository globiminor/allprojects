using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicsTest
{
  [TestClass]
  public class GeomTest
  {
    [TestMethod]
    public void CanFindClosestDistance()
    {
      Point2D p = new Point2D(2, 2);
      Line l = new Line(new Point2D(1, 5), new Point2D(2, 5));
      IPoint closest = GeometryOperator.ClosestPoint(p, l);
    }
  }
}
