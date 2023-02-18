
using Basics.Geom;
using Basics.Geom.Index;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BasicsTest.Geom
{
  [TestClass]
  public class BoxTreeTest
  {
    private int _nPoints = 10000;
    private int _nElem = 64;
    private int _nSearchPoints = 10000;

    private List<Point2D> _points;
    private BoxTree<Point2D> _pointTree;
    private List<Point2D> _searchPoints;

    private List<Point2D> Points => _points ?? (_points = InitPoints(_nPoints));
    private List<Point2D> SearchPoints => _searchPoints ?? (_searchPoints = InitSearchPoints(_nSearchPoints));

    private BoxTree<Point2D> PointTree => _pointTree ?? (_pointTree = BoxTree.Create(Points, (p) => p, nElem: _nElem));
    private List<Point2D> InitPoints(int nPoints = 10000)
    {
      List<Point2D> points = new List<Point2D>();
      Random r = new Random();
      points.Add(new Point2D(0, r.NextDouble()));
      points.Add(new Point2D(1, r.NextDouble()));
      points.Add(new Point2D(r.NextDouble(), 0));
      points.Add(new Point2D(r.NextDouble(), 1));
      for (int i = 4; i < nPoints; i++)
      {
        points.Add(new Point2D(r.NextDouble(), r.NextDouble()));
      }
      return points;
    }

    private List<Point2D> InitSearchPoints(int nPoints = 10000, double f = 1)
    {
      List<Point2D> points = new List<Point2D>();
      Random r = new Random();
      for (int i = 0; i < nPoints; i++)
      {
        double x0 = r.NextDouble() - 0.5;
        double y0 = r.NextDouble() - 0.5;
        points.Add(new Point2D(0.5 + f * x0, 0.5 + f * y0));
      }
      return points;
    }


    [TestMethod]
    public void A0_init()
    {
      Assert.IsNotNull(PointTree);
      Assert.IsNotNull(SearchPoints);
    }
    [TestMethod]
    public void CanCreateBoxTree()
    {
      BoxTree<Point2D> boxTree = PointTree;

      Assert.AreEqual(_nPoints, boxTree.Count);

      int n = boxTree.Search(null).Count();
      Assert.AreEqual(_nPoints, n);

      int n00 = boxTree.Search(new Box(new Point2D(0, 0), new Point2D(0.5, 0.5))).Count();
      int n10 = boxTree.Search(new Box(new Point2D(0.5, 0), new Point2D(1, 0.5))).Count();
      int n01 = boxTree.Search(new Box(new Point2D(0, 0.5), new Point2D(0.5, 1))).Count();
      int n11 = boxTree.Search(new Box(new Point2D(0.5, 0.5), new Point2D(1, 1))).Count();

      Assert.AreEqual(_nPoints, n00 + n01 + n10 + n11);
    }

    [TestMethod]
    public void CanGetMinDistByFunc()
    {
      foreach (var searchPoint in SearchPoints)
      {
        GetMinDistByFunc(PointTree, searchPoint);
      }
    }


    [TestMethod]
    public void StatGetMinDistElemsByList1()
    {
      for (int i = 8; i < 5000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 1);

        Console.WriteLine();
      }
    }

    [TestMethod]
    public void StatGetMinDistElemsByList3()
    {
      for (int i = 8; i < 3000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 3);

        Console.WriteLine();
      }
    }

    [TestMethod]
    public void StatGetMinDistElemsByList15()
    {
      for (int i = 8; i < 1000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 31);

        Console.WriteLine();
      }
    }


    [TestMethod]
    public void CanGetMinDistElemsByList()
    {
      CanGetMinDistElemsByList(Points, SearchPoints, 257, flag: 1);
    }
    private static void CanGetMinDistElemsByList(IList<Point2D> points, IList<Point2D> searchPoints, int maxElems,
      int flag)
    {
      Random r = new Random();
      for (int i = 1; i < maxElems; i = 2 * i)
      {
        var pointTree = BoxTree.Create(points, (p) => p, nElem: i);
        if (flag / 16 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            GetMinDistByFunc(pointTree, searchPoint);
          }
          w.Stop();
          Console.WriteLine($"FuncSearch:    nPoints={points.Count}; nSearch={searchPoints.Count}; nElems={i}; {w.ElapsedMilliseconds / 1000.0:N3}");
        }
        if (flag / 4 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            double minD2 = double.MaxValue;
            Point2D minP = null;
            foreach (var entry in pointTree.Search(null))
            {
              var point = entry.Value;
              double d2 = PointOp.Dist2(searchPoint, point);
              if (d2 < minD2)
              {
                minD2 = d2;
                minP = point;
              }
            }
          }
          w.Stop();
          Console.WriteLine($"NoBoxSearch  : nPoints={points.Count}; nSearch={searchPoints.Count}; nElems={i};     {w.ElapsedMilliseconds / 1000.0:N3}");
        }
        if (flag / 8 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            double minD2 = double.MaxValue;
            Point2D minP = null;
            foreach (var entry in pointTree.Search(new Box(new Point2D(-10, -10), new Point2D(10, 10))))
            {
              var point = entry.Value;
              double d2 = PointOp.Dist2(searchPoint, point);
              if (d2 < minD2)
              {
                minD2 = d2;
                minP = point;
              }
            }
          }
          w.Stop();
          Console.WriteLine($"WithBoxSearch: nPoints={points.Count}; nSearch={searchPoints.Count}; nElems={i};         {w.ElapsedMilliseconds / 1000.0:N3}");
        }
      }
      if (flag / 2 % 2 == 1)
      {
        Stopwatch w = Stopwatch.StartNew();
        foreach (var searchPoint in searchPoints)
        {
          double minD2 = double.MaxValue;
          Point2D minP = null;
          foreach (var point in points)
          {
            double d2 = PointOp.Dist2(searchPoint, point);
            if (d2 < minD2)
            {
              minD2 = d2;
              minP = point;
            }
          }
        }
        w.Stop();
        Console.WriteLine($"Simple List: nPoints={points.Count}; nSearch={searchPoints.Count}; simple list; {w.ElapsedMilliseconds / 1000.0:N3}");
      }
    }


    private static void GetMinDistByFunc(BoxTree<Point2D> boxTree, IPoint point)
    {
      double minD2 = double.MaxValue;
      IPoint nearest = null;
      BoxTreeMinDistSearch search = new BoxTreeMinDistSearch(point);
      foreach (BoxTree<Point2D>.TileEntry entry in boxTree.Search(searcher: search))
      {
        IPoint p = entry.Value;
        double d2 = PointOp.Dist2(point, p);
        if (d2 < minD2)
        {
          minD2 = d2;
          nearest = p;

          search.SetMinDist2(d2);
        }
      }
    }
  }

  public class DistInfo
  {
    public class Comparer : IComparer<DistInfo>
    {
      public int Compare(DistInfo x, DistInfo y)
      {
        if (x == y)
        { return 0; }

        int d = x.MinDist2.CompareTo(y.MinDist2);
        if (d != 0)
        { return d; }

        d = x.MaxDist2.CompareTo(y.MaxDist2);
        if (d != 0)
        { return d; }

        d = x.Key.CompareTo(y.Key);
        return 0;
      }
    }

    public DistInfo(double minDist2, double maxDist2, int key)
    {
      MinDist2 = minDist2;
      MaxDist2 = maxDist2;
      Key = key;
    }

    public double MinDist2 { get; }
    public double MaxDist2 { get; }
    public int Key { get; }

    public override string ToString()
    {
      return $"{MinDist2:N3} {MaxDist2:N3}";
    }
  }
}
