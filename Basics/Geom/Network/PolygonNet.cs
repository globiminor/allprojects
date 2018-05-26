using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom.Network
{
  public class PolygonNet : IEnumerable<LineListPolygon>
  {
    private class BoxComparer : IComparer<BoxTree<TopologicalLine>.TileEntry>
    {
      public int Compare(BoxTree<TopologicalLine>.TileEntry x, BoxTree<TopologicalLine>.TileEntry y)
      {
        return x.Box.Max.X.CompareTo(y.Box.Max.X);
      }
    }
    private class DirRowComparer : IComparer<DirectedRow>
    {
      public int Compare(DirectedRow x, DirectedRow y)
      {
        IBox boxX = x.TopoLine.Line.Extent;
        IBox boxY = y.TopoLine.Line.Extent;
        // sort descending -> y before x
        return boxY.Max.X.CompareTo(boxX.Max.X);
      }
    }

    private class Tree : BoxTree<TopologicalLine>
    {
      BoxComparer _comparer;
      bool _excludeEqual;
      public Tree(BoxComparer comparer)
        : base(2)
      {
        _comparer = comparer;
      }

      public bool ExcludeEqual
      {
        get { return _excludeEqual; }
        set { _excludeEqual = value; }
      }
    }

    private Tree _spatialIndex;
    private List<LineListPolygon> _polyList;

    public static PolygonNet Create(IList<LineList> outerRingList, IBox outerRingsBox,
      IList<LineList> innerRingList, List<DirectedRow> innerLineList)
    {
      PolygonNet net = new PolygonNet();

      innerLineList.Sort(new DirRowComparer());

      List<LineListPolygon> polyList = new List<LineListPolygon>();
      net._polyList = polyList;

      foreach (LineList ring in outerRingList)
      {
        LineListPolygon poly = new LineListPolygon(ring);
        polyList.Add(poly);
      }
      foreach (LineList ring in innerRingList)
      {
        LineListPolygon poly = new LineListPolygon(ring, true);
        Debug.Assert(poly != null,"Make resharper happy");
      }

      net._spatialIndex = BuildSpatialIndex(outerRingsBox, outerRingList, innerLineList, new BoxComparer());
      AssignInnerRings(innerLineList, net._spatialIndex, outerRingsBox);

      return net;
    }

    private static void AssignInnerRings(IList<DirectedRow> innerLines, Tree tree, IBox outerRingsBox)
    {
      foreach (DirectedRow row in innerLines)
      {
        TopologicalLine line = row.TopoLine;
        LineListPolygon poly = null;
        if (line.RightPoly != null && line.RightPoly.IsInnerRing && line.RightPoly.Processed == false)
        { poly = line.RightPoly; }
        else if (line.LeftPoly != null && line.LeftPoly.IsInnerRing && line.LeftPoly.Processed == false)
        { poly = line.LeftPoly; }

        if (poly == null)
        { continue; }

        poly.Processed = true;
        IBox _tBox;
        _tBox = line.Line.Extent;
        if (_tBox.Max.X < outerRingsBox.Min.X || _tBox.Max.Y < outerRingsBox.Min.Y)
        { continue; }

        IPoint _qPntAssignInnerRings = new Point2D(_tBox.Max.X, line.YMax());

        // no unassigned line right of pnt exists, because we sorted the innerLines in this fashion
        TopologicalLine nearLine = NearestLine(tree, _qPntAssignInnerRings, true, out int side);
        if (nearLine == null)
        { continue; }
        if (side > 0)
        {
          if (nearLine.RightPoly != null) // else outside of outer rings
          { nearLine.RightPoly.Add(poly); }
        }
        else
        {
          Debug.Assert(side != 0, "error in software design assumption");
          if (nearLine.LeftPoly != null) // else outside of outer rings
          { nearLine.LeftPoly.Add(poly); }
        }
      }
    }

    private static Tree BuildSpatialIndex(IBox box,
      IList<LineList> outerRingList, IList<DirectedRow> innerLineList,
      BoxComparer comparer)
    {
      Tree tree = new Tree(comparer);

      // Add each line once to box tree
      tree.InitSize(new IGeometry[] { box });
      foreach (LineList ring in outerRingList)
      {
        foreach (DirectedRow row in ring.DirectedRows)
        {
          if (row.RightPoly != null && row.RightPoly.IsInnerRing == false)
          { tree.Add(row.TopoLine.Line.Extent, row.TopoLine); }
          else
          {
            Debug.Assert(row.LeftPoly != null && row.LeftPoly.IsInnerRing == false,
              "error in software design assumption");
            tree.Add(row.TopoLine.Line.Extent, row.TopoLine);
          }
        }
      }
      foreach (DirectedRow row in innerLineList)
      {
        if (row.RightPoly == null || row.LeftPoly == null)
        { tree.Add(row.TopoLine.Line.Extent, row.TopoLine); }
      }

      tree.Sort(comparer);

      return tree;
    }

    public LineListPolygon AssignCentroid(IPoint point, out TopologicalLine line, out int side)
    {
      line = NearestLine(_spatialIndex, point, false, out side);
      if (line == null)
      {
        return null;
      }
      LineListPolygon poly = null;
      if (side > 0)
      {
        poly = line.RightPoly;
      }
      else if (side < 0)
      {
        poly = line.LeftPoly;
      }
      if (poly != null)
      { poly.Centroids.Add(point); }

      return poly;
    }

    /// <summary>
    /// finds the line cutting the x-Axes through pnt closest to pnt on the lower side of pnt
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="pnt"></param>
    /// <param name="excludeEqual"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    private static TopologicalLine NearestLine(Tree tree, IPoint pnt,
      bool excludeEqual, out int side)
    {
      TopologicalLine nearestLine = null;
      side = 0;
      double xMax = tree.Extent.Max.X;

      Box searchBox = new Box(new Point2D(pnt.X, pnt.Y), new Point2D(xMax, pnt.Y));
      tree.ExcludeEqual = excludeEqual;
      IEnumerator<BoxTree<TopologicalLine>.TileEntry> enumerator =
        tree.Search(searchBox).GetEnumerator();

      double x1Nearest = 0;
      double y1Nearest = 0;

      while (enumerator.MoveNext())
      {
        if (xMax <= pnt.X)
        {
          side = 0;
          return nearestLine;
        }

        TopologicalLine currentLine = enumerator.Current.Value;
        IBox _tBox;
        if (excludeEqual)
        {
          _tBox = currentLine.Line.Extent;
          if (_tBox.Max.X == pnt.X)
          { continue; }
        }

        _tBox = new Box(new Point2D(pnt.X, pnt.Y), new Point2D(xMax, pnt.Y));

        foreach (Curve seg in currentLine.Line.Segments)
        {
          if (xMax <= pnt.X)
          {
            side = 0;
            return nearestLine;
          }
          IPoint _pnt0 = seg.Start;
          IPoint _pnt1 = seg.End;

          if (_pnt0.Y == pnt.Y)
          {
            if (_pnt1.Y == pnt.Y)
            {
              if (_pnt0.X < pnt.X != _pnt1.X < pnt.X)
              {
                side = 0;
                return currentLine;
              }
              else if (_pnt0.X < xMax)
              {
                side = 0;
                nearestLine = currentLine;
                xMax = _pnt0.X;
              }
              else if (_pnt1.X < xMax)
              {
                side = 0;
                nearestLine = currentLine;
                xMax = _pnt0.X;
              }
            }
            else if (NewLineSide(pnt, ref xMax, _pnt0, _pnt1, true, ref x1Nearest, ref y1Nearest, ref side))
            { nearestLine = currentLine; }
          }
          else if (_pnt1.Y == pnt.Y)
          {
            if (NewLineSide(pnt, ref xMax, _pnt1, _pnt0, false, ref x1Nearest, ref y1Nearest, ref side))
            { nearestLine = currentLine; }
          }
          else if (_pnt0.Y < pnt.Y != _pnt1.Y < pnt.Y)
          {
            double x = _pnt0.X + (_pnt1.X - _pnt0.X) * (pnt.Y - _pnt0.Y) / (_pnt1.Y - _pnt0.Y);
            if (CheckNewMax(x, pnt.X, ref xMax))
            {
              nearestLine = currentLine;
              side = _pnt0.Y.CompareTo(_pnt1.Y);
            }
          }
        }
        searchBox.Max.X = xMax;
      }
      return nearestLine;
    }

    private static bool NewLineSide(IPoint p, ref double xMax, 
      IPoint onLine, IPoint offLine, bool startOnLine,
      ref double nearX, ref double nearY, ref int onRightSide)
    {
      if (onLine.X < p.X || onLine.X > xMax)
      { return false; }
      else if (nearY == 0 || onLine.X < xMax)
      {
        nearX = offLine.X - onLine.X;
        nearY = offLine.Y - onLine.Y;
        xMax = onLine.X;
        onRightSide = ((nearY < 0) == startOnLine) ? 1 : -1;
        return true;
      }
      else if ((nearY > 0) == (offLine.Y > p.Y))
      {
        double nX = offLine.X - onLine.X;
        double nY = offLine.Y - onLine.Y;
        if ((nX * nearY - nY * nearX < 0) == (nY > 0))
        {
          nearX = nX;
          nearY = nY;
          onRightSide = ((nearY < 0) == startOnLine) ? 1 : -1;
          return true;
        }
      }
      return false;
    }

    private static bool CheckNewMax(double x, double xMin, ref double xMax)
    {
      if (x < xMin)
      { return false; }
      if (x > xMax)
      { return false; }
      xMax = x;
      return true;
    }


    public IEnumerator<LineListPolygon> GetEnumerator()
    {
      return _polyList.GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
  }
}
