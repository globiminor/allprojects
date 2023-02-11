using System;
using Basics.Geom;
using Basics.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicsTest
{
  [TestClass]
  public class GeomTest
  {
    [TestMethod]
    public void CanFindClosestDistance()
    {
      {
        Line l = new Line(new Point2D(1, 5), new Point2D(4, 5));

        IPoint closest = GeometryOperator.ClosestPoint(new Point2D(2, 2), l);
        Assert.AreEqual(2, closest.X);
        Assert.AreEqual(5, closest.Y);

        closest = GeometryOperator.ClosestPoint(new Point2D(5, 2), l);
        Assert.AreEqual(4, closest.X);
        Assert.AreEqual(5, closest.Y);
      }
      {
        Arc arc = new Arc(new Point2D(2, 2), 3, 0, Math.PI / 2);
        IPoint closest = GeometryOperator.ClosestPoint(new Point2D(4, 3), arc);
        Assert.IsNotNull(closest);
      }

      Assert.Inconclusive("Add more tests");
    }

    [TestMethod]
    public void ValidateArcBox()
    {
      Arc arc = new Arc(new Point2D(0, 0), 1, Math.PI / 4, Math.PI / 2);
      Box b = arc.Extent;
      Assert.IsTrue(b.Max.Y >= 1);

      arc = new Arc(new Point2D(0, 0), 1, Math.PI / 4, Math.PI / 1.7);
      b = arc.Extent;
      Assert.IsTrue(b.Max.Y >= 1);

    }

    [TestMethod]
    public void CanAreaLineIntersect()
    {
      Polyline l = Polyline.Create(new Point2D[] { new Point2D(0, 1), new Point2D(2, 1) });
      Polyline c = new Polyline();
      c.Add(new Arc(new Point2D(1, 1), 0.8, 0, 2 * Math.PI));

      Area a = new Area(Polyline.Create(new Point2D[] { new Point2D(0, 0), new Point2D(4, 0), new Point2D(2, 4), new Point2D(0, 0) }));

      var rels = GeometryOperator.CreateRelations(a, l);
      var inter = GeometryOperator.Intersection(a, l);

      rels = GeometryOperator.CreateRelations(a, c);
    }

    [TestMethod]
    public void CanUseCallerMemberName()
    {
      using (NotifyMock mock = new NotifyMock())
      {
        string prop = null;
        mock.PropertyChanged += (s, e) =>
        {
          prop = e.PropertyName;
        };

        mock.Test = "Hallo";
        Assert.AreEqual(prop, nameof(NotifyMock.Test));
      }
    }


    private class NotifyMock : NotifyListener
    {
      protected override void Disposing(bool disposing)
      { }
      private string _test;
      public string Test
      {
        get { return _test; }
        set
        {
          _test = value;
          Changed();
        }
      }
    }
  }
}
