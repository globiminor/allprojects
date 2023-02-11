
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicsTest.Geom
{
  [TestClass]
  public class BoxTreeTest
  {
    [TestMethod]
    public void CanCreateBoxTree()
    {
      int nPoints = 10000;
      List<Point2D> points = new List<Point2D>();
      Random r = new Random();
      for (int i = 0; i < nPoints - 4; i++)
      {
        points.Add(new Point2D(r.NextDouble(), r.NextDouble()));
      }
      points.Add(new Point2D(0, r.NextDouble()));
      points.Add(new Point2D(1, r.NextDouble()));
      points.Add(new Point2D(r.NextDouble(), 0));
      points.Add(new Point2D(r.NextDouble(), 1));
      BoxTree<Point2D> boxTree = BoxTree.Create(points, (p) => p);

      Assert.AreEqual(nPoints, boxTree.Count);

      int n = boxTree.Search(null).Count();
      Assert.AreEqual(nPoints, n);

      int n00 = boxTree.Search(new Box(new Point2D(0,0), new Point2D(0.5, 0.5))).Count();
      int n10 = boxTree.Search(new Box(new Point2D(0.5, 0), new Point2D(1, 0.5))).Count();
      int n01 = boxTree.Search(new Box(new Point2D(0, 0.5), new Point2D(0.5, 1))).Count();
      int n11 = boxTree.Search(new Box(new Point2D(0.5, 0.5), new Point2D(1, 1))).Count();

      Assert.AreEqual(nPoints, n00 + n01 + n10 + n11);
    }
  }
}
