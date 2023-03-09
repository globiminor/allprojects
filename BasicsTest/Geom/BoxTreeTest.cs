
using Basics.Geom;
using Basics.Geom.Index;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

    private List<Line> _lines;
    private BoxTree<Line> _lineTree;

    private List<Point2D> _searchPoints;

    private List<Point2D> Points => _points ?? (_points = InitPoints(_nPoints));
    private List<Line> Lines => _lines ?? (_lines = InitLines(_nPoints));
    private List<Point2D> SearchPoints => _searchPoints ?? (_searchPoints = InitSearchPoints(_nSearchPoints));

    private BoxTree<Point2D> PointTree => _pointTree ?? (_pointTree = BoxTree.Create(Points, (p) => p, nElem: _nElem));
    private BoxTree<Line> LineTree => _lineTree ?? (_lineTree = BoxTree.Create(Lines, (l) => l.Extent, nElem: _nElem));
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
    private List<Line> InitLines(int nLines = 10000, double lMin = 0.01, double lMax = 0.02)
    {
      List<Line> lines = new List<Line>();
      Random r = new Random();
      for (int i = 0; i < nLines; i++)
      {
        Point2D p0 = new Point2D(r.NextDouble(), r.NextDouble());
        double angle = r.NextDouble() * Math.PI * 2;
        double sin = Math.Sin(angle);
        double cos = Math.Cos(angle);
        double l = (lMax - lMin) * r.NextDouble() + lMin;
        Point d = new Point2D(sin * l, cos * l);
        lines.Add(new Line(p0, p0 + d));
      }
      return lines;
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
        GetMinDistByFunc(PointTree, searchPoint, (geom, s) => PointOp.Dist2(geom, s));
      }
    }


    [TestMethod]
    public void StatGetMinDistElemsByList1()
    {
      for (int i = 8; i < 5000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 1, (geom, searchPoint) => PointOp.Dist2(geom, searchPoint));

        Console.WriteLine();
      }
    }

    [TestMethod]
    public void StatGetMinDistElemsByList3()
    {
      for (int i = 8; i < 3000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 3, (geom, searchPoint) => PointOp.Dist2(geom, searchPoint));

        Console.WriteLine();
      }
    }

    [TestMethod]
    public void StatGetMinDistElemsByList15()
    {
      for (int i = 8; i < 1000; i = 2 * i)
      {
        IList<Point2D> points = InitPoints(i);
        CanGetMinDistElemsByList(points, SearchPoints, 65, flag: 31, (geom, searchPoint) => PointOp.Dist2(geom, searchPoint));

        Console.WriteLine();
      }
    }

    [TestMethod]
    public void StatGetLineMinDistElemsByList15()
    {
      for (int i = 8; i < 1000; i = 2 * i)
      {
        IList<Line> lines = InitLines(i);
        CanGetMinDistElemsByList(lines, SearchPoints, 65, flag: 18,
          (geom, searchPoint) =>
          {
            IPoint closest = GeometryOperator.GetClosestPoint(searchPoint, geom);
            double d2 = PointOp.Dist2(searchPoint, closest);
            return d2;
          });

        Console.WriteLine();
      }
    }


    [TestMethod]
    public void CanGetMinDistElemsByList()
    {
      CanGetMinDistElemsByList(Points, SearchPoints, 257, flag: 1, (geom, searchPoint) => PointOp.Dist2(geom, searchPoint));
    }
    private static void CanGetMinDistElemsByList<T>(IList<T> geoms, IList<Point2D> searchPoints, int maxElems,
      int flag, Func<T,IPoint,double> minFunc) where T: IGeometry
    {
      Random r = new Random();
      for (int i = 1; i < maxElems; i = 2 * i)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append($"nGeoms={geoms.Count}; nSearch={searchPoints.Count}; nElems={i,4}; ");
        bool calc = false;

        var pointTree = BoxTree.Create(geoms, (p) => p.Extent, nElem: i);
        if (flag / 16 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            GetMinDistByFunc(pointTree, searchPoint, minFunc);
          }
          w.Stop();
          sb.Append($"F: {w.ElapsedMilliseconds / 1000.0:N3};");
          calc= true;
        }
        if (flag / 4 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            double minD2 = double.MaxValue;
            T minP = default;
            foreach (var entry in pointTree.Search(null))
            {
              var geom = entry.Value;

              double d2 = minFunc(geom, searchPoint); 
              if (d2 < minD2)
              {
                minD2 = d2;
                minP = geom;
              }
            }
          }
          w.Stop();
          sb.Append($"N: {w.ElapsedMilliseconds / 1000.0:N3};");
          calc= true;
        }
        if (flag / 8 % 2 == 1)
        {
          Stopwatch w = Stopwatch.StartNew();

          foreach (var searchPoint in searchPoints)
          {
            double minD2 = double.MaxValue;
            T minP = default;
            foreach (var entry in pointTree.Search(new Box(new Point2D(-10, -10), new Point2D(10, 10))))
            {
              var geom = entry.Value;
              double d2 = minFunc(geom, searchPoint);
              if (d2 < minD2)
              {
                minD2 = d2;
                minP = geom;
              }
            }
          }
          w.Stop();
          sb.Append($"B: {w.ElapsedMilliseconds / 1000.0:N3};");
          calc= true;
        }
        if (calc)
        {
          Console.WriteLine(sb.ToString());
        }
      }
      if (flag / 2 % 2 == 1)
      {
        Stopwatch w = Stopwatch.StartNew();
        foreach (var searchPoint in searchPoints)
        {
          double minD2 = double.MaxValue;
          T minGeom = default;
          foreach (var geom in geoms)
          {
            double d2 = minFunc(geom, searchPoint);
            if (d2 < minD2)
            {
              minD2 = d2;
              minGeom = geom;
            }
          }
        }
        w.Stop();
        Console.WriteLine($"nGeoms={geoms.Count}; nSearch={searchPoints.Count}; simple list; S: {w.ElapsedMilliseconds / 1000.0:N3}");
      }
    }


    private static void GetMinDistByFunc<T>(BoxTree<T> boxTree, Point point, Func<T, IPoint, double> minFunc)
      where T : IGeometry
    {
      double minD2 = double.MaxValue;
      T nearest = default;
      BoxTreeMinDistSearch search = new BoxTreeMinDistSearch(point);
      foreach (TileEntry<T> entry in boxTree.Search(searcher: search))
      {
        T geom = entry.Value;
        double d2 = minFunc(geom, point);
        if (d2 < minD2)
        {
          minD2 = d2;
          nearest = geom;

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
