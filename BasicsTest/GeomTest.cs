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
      Point2D p = new Point2D(2, 2);
      Line l = new Line(new Point2D(1, 5), new Point2D(2, 5));
      IPoint closest = GeometryOperator.ClosestPoint(p, l);
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
