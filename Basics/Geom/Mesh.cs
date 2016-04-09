using System;
using System.Collections;
using System.Collections.Generic;
using PntOp = Basics.Geom.PointOperator;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Mesh.
  /// </summary>
  public partial class Mesh
  {
    public delegate IPoint InsertHandler(Mesh mesh, Line insertLine, object insertLineInfo,
      MeshLine cutLine, double fCut);

    #region nested classes (see also MeshNested.cs)
    private class _IntersectInfo
    {
      public MeshLineEx Line;
      private double _vecProdStart;
      private double _vecProdEnd;
      public _IntersectInfo(MeshLineEx line, double vecProdStart, double vecProdEnd)
      {
        Line = line;
        _vecProdStart = vecProdStart;
        _vecProdEnd = vecProdEnd;
      }

      public double FactorCut()
      {
        double f = _vecProdStart / (_vecProdStart - _vecProdEnd);
        if (!(f >= 0 && f <= 1)) throw new InvalidProgramException(f.ToString());
        return f;
      }
    }
    #endregion
    // member definitions
    private double _fuzzy2;
    private double _fuzzy;
    private double _epsi = 1e-12;
    private PointBoxTree _listPoint;
    private bool _inserted;

    public Mesh()
    {
      _listPoint = new PointBoxTree();
    }

    public Mesh(int capacity)
    {
      _listPoint = new PointBoxTree(capacity);
    }

    public IEnumerable<MeshPoint> Points(IBox intersect)
    {
      foreach (PointBoxTree.TileEntry entry in _listPoint.Search(intersect))
      {
        MeshPoint p = entry.Value;
        yield return p;
      }
    }

    public IEnumerable<MeshLine> GetLinesAt(MeshPoint centralPoint)
    {
        MeshLineEx l0 = centralPoint.IndexLine;
        MeshLineEx l = l0;
        do
        {
          yield return l; 
          l = l._GetNextPointLine();
        } while (l.BaseLine != l0.BaseLine);
    }

    public IEnumerable<MeshLine> Lines(IBox intersect)
    {
      foreach (PointBoxTree.TileEntry entry in _listPoint.Search(intersect))
      {
        MeshPoint p = entry.Value;
        MeshLineEx l0 = p.IndexLine;
        MeshLineEx l = l0;
        do
        {
          if (l.IsReverse == false)
          { yield return l; }
          l = l._GetNextPointLine();
        } while (l.BaseLine != l0.BaseLine);
      }
    }

    public IEnumerable<Tri> Tris(IBox intersect)
    {
      foreach (PointBoxTree.TileEntry entry in _listPoint.Search(intersect))
      {
        MeshPoint p = entry.Value;
        MeshLineEx l0 = p.IndexLine;
        MeshLineEx l = l0;
        do
        {
          if (l.LeftTriEx != null &&
            l.LeftTriEx._GetLine(0).BaseLine == l.BaseLine)
          { yield return l.LeftTriEx; }
          l = l._GetNextPointLine();
        } while (l.BaseLine != l0.BaseLine);
      }
    }

    public bool Verify()
    {
      if (_listPoint.Count < 3)
      {
        return true;
      }
      int n = _listPoint.Count;
      List<MeshPoint> points = new List<MeshPoint>(n);
      foreach (BoxTree<MeshPoint>.TileEntry entry in _listPoint.Search(null))
      {
        MeshPoint p = entry.Value;
        points.Add(p);

        MeshLine l0 = p.IndexLine;
        IPoint end = l0.Start;

        for (MeshLine l = l0; l.End != end; l = l.GetNextPointLine())
        {
          end = l0.End;
          if (l.Start != p.Point)
          {
            return false;
          }
          Tri lTri = l.LeftTri;
          Tri rTri = l.RightTri;
          if (lTri == null && rTri == null)
          {
            return false;
          }
          for (int i = 0; i < 3; i++)
          {
            if (lTri != null)
            {
              MeshLine b = lTri[i];
              if (b.LeftTri != lTri)
              {
                return false;
              }
            }
            if (rTri != null)
            {
              MeshLine b = rTri[i];
              if (b.LeftTri != rTri)
              {
                return false;
              }
            }
          }
        }
      }
      return true;
    }
    public Box Extent
    {
      get { return _listPoint.Extent; }
    }
    public void InitSize(IEnumerable<IGeometry> elements)
    {
      _listPoint.InitSize(elements);
    }
    public double Fuzzy2
    {
      get { return _fuzzy2; }
    }
    public double Fuzzy
    {
      get { return _fuzzy; }
      set
      {
        if (value < _epsi)
        { value = _epsi; }
        _fuzzy = value;
        _fuzzy2 = value * value;
      }
    }

    public void Add(Polyline polyline, object lineType, InsertHandler insertFct)
    {
      MeshPoint p0 = null;
      foreach (IPoint p in polyline.Points)
      {
        Point2D p1 = (Point2D)Point.CastOrCreate(p);
        if (p0 != null)
        {
          if (p1.Dist2(p0.Point) > _fuzzy2)
          {
            MeshPoint next = InsertSeg(p0, p1, lineType, insertFct);
            p0 = next;
          }
        }
        else
        { p0 = Add(p1); }
      }
    }
    /// <summary>
    /// Adds point to the mesh if not coincident with existing point of mesh
    /// </summary>
    /// <param name="point"></param>
    /// <returns>null if new Point add, coincident point if not added</returns>
    public MeshPoint Add(IPoint point)
    {
      return Add(point, (MeshPoint)null);
    }

    /// <summary>
    /// returns Line starting in p0 and ending in p1 if existing,
    /// otherwise null
    /// </summary>
    public MeshLine FindPointLine(MeshPoint p0, Point2D p1)
    {
      MeshLineEx l0 = p0.IndexLine;
      MeshLineEx l = l0;
      do
      {
        if (l.End == p1)
        { return l; }
        l = l._GetNextPointLine();
      } while (l.BaseLine != l0.BaseLine);

      return null;
    }

    /// <summary>
    /// Find a tri, where point lies within tris circle
    /// </summary>
    private Tri FindCircle(Point2D point, out double radius2)
    {
      Tri tri0 = null;

      radius2 = -1;
      foreach (Tri tri in Tris(null))
      {
        double dr2 = point.Dist2(tri.Center) - tri.Radius2;

        if (dr2 < radius2 || radius2 < 0)
        {
          radius2 = dr2;
          tri0 = tri;
        }
        if (radius2 <= 0)
        { return tri0; }
      }
      return tri0;
    }


    private MeshLineEx FindNextLine(MeshLineEx l0, IPoint from, IPoint goal,
      IPoint dirGoal, ref double vecProdLeft, ref double vecProdRight)
    {
      if (!(vecProdLeft >= 0 && vecProdRight < 0)) throw new InvalidProgramException("Error in software design assumption");

      MeshLineEx l1 = l0._GetNextTriLine();
      if (l1 == null)
      {
        return null;
      }

      IPoint dirMean = PntOp.Sub(l1.End, from);
      double vecProdMean = PntOp.VectorProduct(dirGoal, dirMean);

      if (vecProdMean >= 0)
      {
        vecProdLeft = vecProdMean;
        return -l1;
      }
      else
      {
        vecProdRight = vecProdMean;
        return -(l1._GetNextTriLine());
      }
    }

    private MeshLineEx FindStartLine(MeshPoint from, IPoint goal,
      out Point dirGoal, out double vecProdLeft, out double vecProdRight)
    {
      IPoint p0 = from.Point;
      dirGoal = PntOp.Sub(goal, p0);
      if (dirGoal.OrigDist2() == 0)
      {
        vecProdLeft = 0;
        vecProdRight = 0;
        return from.IndexLine;
      }

      MeshLineEx l0 = from.IndexLine;
      double v0 = dirGoal.VectorProduct(PntOp.Sub(l0.EndPointEx.Point, p0));
      bool getNext = (v0 >= 0);

      MeshLineEx l1 = l0;
      double v1 = v0;
      do
      {
        v0 = v1;
        l0 = l1;

        if (getNext)
        { l1 = (-l0)._GetNextTriLine(); }
        else
        {
          if (l0.LeftTri != null)
          { l1 = -(l0._GetPreviousTriLine()); }
          else
          { l1 = null; }
        }

        if (l1 == null)
        {
          vecProdLeft = 0;
          vecProdRight = 0;

          if (getNext)
          { return -l0; }
          else
          { return l0; }
        }

        v1 = dirGoal.VectorProduct(PntOp.Sub(l1.EndPointEx.Point, p0));
      }
      while ((v1 >= 0) == getNext);

      if (getNext)
      {
        vecProdLeft = v0;
        vecProdRight = v1;
        return -(l1._GetNextTriLine());
      }
      else
      {
        vecProdRight = v0;
        vecProdLeft = v1;
        return -(l0._GetNextTriLine());
      }
    }

    private MeshLineEx FindLine(IPoint goal, MeshPoint start)
    {
      double vecProdLeft;
      double vecProdRight;
      Point dirGoal;

      MeshLineEx l0 = FindStartLine(start, goal,
        out dirGoal, out vecProdLeft, out vecProdRight);
      if (l0.LeftTri == null)
      {
        if (l0.Start == start.Point || l0.End == start.Point)
        { return l0; }
        double f = vecProdLeft / (vecProdLeft - vecProdRight);
        Point cut = l0.Start + f * PntOp.Sub(l0.End, l0.Start);
        if (cut.Dist2(start.Point) > dirGoal.OrigDist2())
        { return -l0; }
        return l0;
      }

      MeshLineEx l1 = l0;
      while (l1 != null && (PntOp.Sub(l1.End, l1.Start)).VectorProduct(PntOp.Sub(goal, l1.Start)) > 0)
      {
        l0 = l1;
        l1 = FindNextLine(l0, start.Point, goal, dirGoal, ref vecProdLeft, ref vecProdRight);
      }

      if (l1 != null)
      { return -l1; }
      else
      { return l0; }
    }


    private bool FindNextLine(ref MeshLineEx l0, Point2D goal)
    {
      double lGoal2, vecProd1, vecProd2;
      return FindNextLine(ref l0, goal, out lGoal2, out vecProd1, out vecProd2);
    }
    private bool FindNextLine(ref MeshLineEx l0, Point2D goal,
      out double lGoal2, out double vecProd1, out double vecProd2)
    {
      IPoint p0 = l0.Start;
      IPoint p1 = l0.End;
      IPoint p2;
      Point dir = PntOp.Sub(p1, p0);
      MeshLineEx l2;
      IPoint dirGoal = goal - p0;
      lGoal2 = Point2D.Dist2(p0, goal);

      vecProd1 = dir.VectorProduct(dirGoal);
      if (vecProd1 < 0)
      { // goal lies on right side of l0
        vecProd2 = double.NaN;
        l0 = -l0;
        return true;
      }

      l2 = l0._GetPreviousTriLine();
      if (l2 == null)
      { // goal lies on left side of l0 and l0.LeftTri == null
        vecProd2 = double.NaN;
        l0 = null;
        return true;
      }
      p2 = l2.Start;
      dir = PntOp.Sub(p2, p0);
      vecProd2 = dir.VectorProduct(dirGoal);
      if (vecProd2 > 0)
      { // goal lies on right side of l2
        l0 = -l2;
        return true;
      }

      dir = PntOp.Sub(p2, p1);
      dirGoal = goal - p1;
      if (dir.VectorProduct(dirGoal) < 0)
      { // goal lies on right side of l1
        l0 = -(l0.GetNextTriLine());
        return true;
      }

      // goal lies within l0.LeftTri
      return false;
    }

    private MeshLineEx FindLine_(Point2D goal, MeshPoint start)
    {
      MeshLineEx l0;
      MeshLineEx l1 = start.IndexLine;
      do
      {
        l0 = l1;
      }
      while (FindNextLine(ref l1, goal) && l1 != null);
      return l0;
    }


    /// <summary>
    /// add a colinear point while only parallel lines exist
    /// </summary>
    /// <param name="point">point to insert</param>
    /// <returns>Newly insert mesh point or coincident old meshpoint</returns>
    private MeshPoint AddParallel(IPoint point)
    {
      MeshPoint p0 = _listPoint[0].Value;
      int m = _listPoint.Count - 1;
      MeshPoint p1 = _listPoint[m].Value;
      Point d = PntOp.Sub(p1.Point, p0.Point);
      int index;
      MeshLineEx newLine;

      if (Math.Abs(d.X) > Math.Abs(d.Y))
      { index = 0; }
      else
      { index = 1; }

      if ((point[index] > p0.Point[index]) == (point[index] < p1.Point[index]))
      {

        for (int i = 1; i < _listPoint.Count; i++)
        {
          p1 = _listPoint[i].Value;
          if ((point[index] >= p0.Point[index]) == (point[index] <= p1.Point[index]))
          { // segment found
            if (Point2D.Dist2(point, p0.Point) < _fuzzy2)
            { return p0; }
            if (Point2D.Dist2(point, p1.Point) < _fuzzy2)
            { return p1; }

            MeshPoint p2 = new MeshPoint(point);
            newLine = new MeshLineEx(p2, p1);
            p0.IndexLine.EndPointEx = p2;
            p2.IndexLine = newLine;
            if (p1.IndexLine.IsReverse)
            { p1.IndexLine = -newLine; }

            _listPoint.Insert(i, p2);
            _inserted = true;
            return p2;
          }

          p0 = p1;
        }
        throw new InvalidOperationException("no segment found");
      }
      else
      { // point on either side of start or end
        if ((point[index] < p0.Point[index]) == (p0.Point[index] < p1.Point[index]))
        { // insert at start
          if (Point2D.Dist2(point, p0.Point) < _fuzzy2)
          { return p0; }
          else
          {
            MeshPoint p2 = new MeshPoint(point);
            newLine = new MeshLineEx(p2, p0);
            _listPoint.Insert(0, p2);
            _inserted = true;
            p2.IndexLine = newLine;
            return p2;
          }
        }
        else
        { // insert at end
          if (Point2D.Dist2(point, p1.Point) < _fuzzy2)
          { return p1; }
          else
          {
            MeshPoint p2 = new MeshPoint(point);
            newLine = new MeshLineEx(p1, p2);
            p1.IndexLine = newLine;
            p2.IndexLine = -newLine;
            return p2;
          }
        }
      }
    }

    public MeshLine FindLine(IPoint p)
    {
      Point box = Point.CastOrCreate(p);
      BoxTree<MeshPoint>.TileEntry entry = _listPoint.Near(box);
      return FindLine(p, entry.Value);
    }

    public MeshPoint InsertSeg(Point2D p0, Point2D p1, object lineInfo, InsertHandler insertFct)
    {
      MeshPoint start = Add(p0);
      return StartInsertSeg(start, p1, null, lineInfo, insertFct, TagType.Untagged | TagType.Tagged);
    }
    public MeshPoint InsertSeg(MeshPoint p0, Point2D p1, object lineInfo, InsertHandler insertFct)
    {
      return StartInsertSeg(p0, p1, null, lineInfo, insertFct, TagType.Untagged | TagType.Tagged);
    }
    public void InsertSeg(MeshPoint p0, MeshPoint p1, object lineInfo, InsertHandler insertFct)
    {
      StartInsertSeg(p0, p1.Point, p1, lineInfo, insertFct, TagType.Untagged | TagType.Tagged);
    }

    [Flags]
    private enum TagType
    {
      None = 0,
      Untagged = 1,
      Tagged = 2
    }
    private MeshPoint StartInsertSeg(MeshPoint p0, IPoint p1,
      MeshPoint m1, object lineInfo,
      InsertHandler insertFct, TagType allowedCross)
    {
      if (p0 == m1)
      { return m1; }

      _IntersectInfo untagged = null;
      _IntersectInfo tagged = null;

      double vecProdLeft;
      double vecProdRight;
      Point dirGoal;

      MeshLineEx l0 = FindStartLine(p0, p1, out dirGoal, out vecProdLeft, out vecProdRight);
      MeshLineEx l1 = l0;

      while (l1 != null && (PntOp.Sub(l1.End, l1.Start)).VectorProduct(PntOp.Sub(p1, l1.Start)) > 0)
      {
        if (!(allowedCross != 0)) throw new InvalidProgramException("Error in software design assumption");

        bool isReverse;
        if (l1.GetTag(out isReverse) == null)
        {
          if (untagged == null)
          {
            untagged = new _IntersectInfo(l1, vecProdLeft, vecProdRight);
            if ((allowedCross & TagType.Tagged) == 0)
            { break; }
          }
        }
        else
        {
          if (!((allowedCross & TagType.Tagged) == TagType.Tagged))
            throw new InvalidProgramException("Error in software design assumption");

          tagged = new _IntersectInfo(l1, vecProdLeft, vecProdRight);
          break;
        }

        l0 = l1;
        l1 = FindNextLine(l0, p0.Point, p1, dirGoal, ref vecProdLeft, ref vecProdRight);
      }

      if (tagged != null)
      { return InsertSeg(p0, p1, m1, lineInfo, tagged, insertFct); }

      if (untagged != null)
      {
        MeshLineEx cutLine = untagged.Line;
        IPoint p2 = insertFct(this, new Line(p0.Point, p1), lineInfo, cutLine,
          untagged.FactorCut());
        _inserted = false;
        MeshPoint m2 = Add(p2, cutLine);
        if (m2.Point == p2 && _inserted == false)
        { _listPoint.Add(m2); }
        StartInsertSeg(p0, m2.Point, m2, lineInfo, insertFct, TagType.None);
        return StartInsertSeg(m2, p1, m1, lineInfo, insertFct, TagType.Untagged);
      }

      if (m1 == null)
      {
        _inserted = false;
        m1 = Add(p1, -l0);
        if (m1.Point == p1 && _inserted == false)
        {
          _listPoint.Add(m1);
          return StartInsertSeg(p0, m1.Point, m1, lineInfo, insertFct, TagType.None);
        }
        else if (m1 == p0)
        { return m1; }
      }

      if (Point2D.Dist2(p1, l0.Start) < _fuzzy2)
      {
        if (l0.LeftTri != null || p0.Equals(l0.EndPointEx) == false)
        { l0 = -(l0._GetPreviousPointLine()); }
        else
        { l0 = -l0; }
      }
      else if (Point2D.Dist2(p1, l0.End) <= _fuzzy2)
      {
        if (l0.LeftTri != null)
        { l0 = -((-l0)._GetNextPointLine()); }
      }
      if (!(p0.Equals(l0.StartPointEx))) throw new InvalidProgramException("Error in software design assumption");
      if (!(Point2D.Dist2(p1, l0.End) < _fuzzy2)) throw new InvalidProgramException("Error in software design assumption");

      l0.SetTag(lineInfo, false);

      return m1;
    }

    private MeshPoint InsertSeg(MeshPoint p0, IPoint p1, MeshPoint m1, object lineInfo,
      _IntersectInfo intersect, InsertHandler insertFct)
    {
      MeshLineEx cutLine = intersect.Line;
      IPoint p2 = insertFct(this, new Line(p0.Point, p1), lineInfo, cutLine,
        intersect.FactorCut());
      bool isReverse;
      object cutInfo = cutLine.GetTag(out isReverse);

      MeshPoint pCut0, pCut1;
      if (isReverse == false)
      {
        pCut0 = cutLine.StartPointEx;
        pCut1 = cutLine.EndPointEx;
      }
      else
      {
        pCut0 = cutLine.EndPointEx;
        pCut1 = cutLine.StartPointEx;
      }

      cutLine.SetTag(null, isReverse);

      _inserted = false;
      MeshPoint m2 = Add(p2, cutLine);
      if (_inserted == false)
      { _listPoint.Add(m2); }

      StartInsertSeg(pCut0, m2.Point, m2, cutInfo, insertFct, TagType.Untagged);
      StartInsertSeg(m2, pCut1.Point, pCut1, cutInfo, insertFct, TagType.Untagged);

      StartInsertSeg(p0, m2.Point, m2, lineInfo, insertFct, TagType.Untagged);
      p0 = m2;
      return StartInsertSeg(p0, p1, m1, lineInfo, insertFct, TagType.Tagged | TagType.Untagged);
    }

    private MeshPoint AddFirstTri(IPoint newPoint)
    {
      int pntMax = _listPoint.Count;
      MeshPoint p0, p1, p2;
      MeshLineEx l0;
      Point2D center;

      p0 = _listPoint[0].Value;
      p1 = _listPoint[pntMax - 1].Value;

      center = Geometry.OutCircleCenter(p0.Point, p1.Point, newPoint);
      if (center == null)
      { return AddParallel(newPoint); }

      // check coincident with all points
      // assumption : parallel lines are sorted so, that each start point is equal to the
      // preceding endpoint
      for (int i = 0; i < pntMax; i++)
      {
        p2 = _listPoint[i].Value;
        if (Point2D.Dist2(newPoint, p2.Point) < _fuzzy2)
        { return p2; }
      }

      p2 = new MeshPoint(newPoint);
      l0 = new MeshLineEx(p2, p0);
      p2.IndexLine = l0;

      bool dir = (Geometry.TriArea(p0.Point, p1.Point, newPoint) > 0);
      for (int i = 1; i < pntMax; i++)
      {
        p1 = _listPoint[i].Value;
        MeshLineEx l1 = new MeshLineEx(p1, p2);
        if (dir)
        { new TriEx(p0.IndexLine, l1, l0); }
        else
        { new TriEx(-l0, -l1, -p0.IndexLine); }
        p0 = p1;
        l0 = -l1;
      }
      return p2;
    }

    /// <summary>
    /// Adds a point outside current mesh
    /// </summary>
    private MeshPoint AddBorder(IPoint point, MeshLineEx l)
    {
      MeshPoint pNew = new MeshPoint(point);
      MeshPoint p0, p1;
      if (!(l.LeftTri == null)) throw new InvalidProgramException();

      p0 = l.StartPointEx;
      p1 = l.EndPointEx;

      double triArea = Geometry.TriArea(p0.Point, p1.Point, point);
      MeshLineEx lt = l;
      while (triArea == 0)
      {
        double d0 = Point2D.Dist2(point, p0.Point);
        double d1 = Point2D.Dist2(point, p1.Point);

        double d2 = Point2D.Dist2(p0.Point, p1.Point);
        if (d0 < d2 && d1 < d2) // point lies between p0 and p1
        {
          break;
        }

        if (d0 <= d1)
        {
          lt = -(lt._GetNextPointLine());
        }
        else
        {
          lt = -lt;
          lt = lt._GetPreviousPointLine();
        }
        if (!(lt.LeftTri == null)) throw new InvalidProgramException();
        p0 = lt.StartPointEx;
        p1 = lt.EndPointEx;
        triArea = Geometry.TriArea(p0.Point, p1.Point, point);
        if (triArea < 0) throw new InvalidProgramException();
      }

      MeshLineEx l0, l1;
      l0 = new MeshLineEx(p0, pNew);
      l1 = new MeshLineEx(p1, pNew);

      pNew.IndexLine = -l0;

      new TriEx(lt, l1, -l0);

      lt.OptionChangeDiag(0);
      CompleteLeftBord(l0);
      CompleteRightBord(l1);

      return pNew;
    }

    private void CompleteLeftBord(MeshLineEx l0)
    {
      if (!(l0.LeftTriEx == null)) throw new InvalidProgramException();

      MeshLineEx l1 = l0._GetNextPointLine();
      MeshPoint p0 = l1.EndPointEx;
      MeshPoint p1 = l0.StartPointEx;
      MeshPoint p2 = l0.EndPointEx;

      if (Geometry.TriArea(p0.Point, p1.Point, p2.Point) <= 0)
      { return; }

      MeshLineEx l2 = new MeshLineEx(p0, p2);
      new TriEx(-l1, l0, -l2);

      (-l1).OptionChangeDiag(0);
      CompleteLeftBord(l2);
    }

    private void CompleteRightBord(MeshLineEx l0)
    {
      if (!(l0.RightTriEx == null)) throw new InvalidProgramException();

      MeshLineEx l1 = l0._GetPreviousPointLine();
      MeshPoint p0 = l1.EndPointEx;
      MeshPoint p1 = l0.StartPointEx;
      MeshPoint p2 = l0.EndPointEx;

      if (Geometry.TriArea(p0.Point, p2.Point, p1.Point) <= 0)
      { return; }

      MeshLineEx l2 = new MeshLineEx(p0, p2);
      new TriEx(l1, l2, -l0);

      l1.OptionChangeDiag(0);
      CompleteRightBord(l2);
    }

    public MeshPoint Add(IPoint point, MeshPoint nearPoint)
    {
      _inserted = false;
      MeshPoint p = Add_(point, nearPoint);
      if (p.Point == point && _inserted == false)
      { _listPoint.Add(p.Extent, p); }
      return p;
    }
    private MeshPoint Add_(IPoint point, MeshPoint nearPoint)
    {
      #region first point(s)
      if (_listPoint.Count == 0)
      { // add first point
        MeshPoint p0 = new MeshPoint(point);
        return p0;
      }
      else if (_listPoint.Count == 1)
      {
        // try add first line
        MeshPoint p0 = _listPoint[0].Value;

        if (Point2D.Dist2(point, p0.Point) < _fuzzy2)
        { return p0; }

        MeshPoint p1 = new MeshPoint(point);
        MeshLineEx line = new MeshLineEx(p0, p1);

        p0.IndexLine = line;
        p1.IndexLine = -line;
        return p1;
      }
      else
      {
        MeshLineEx line = _listPoint[0].Value.IndexLine;
        if (line.LeftTri == null && line.RightTri == null)
        { return AddFirstTri(point); }
      }
      #endregion

      // normal points
      if (nearPoint == null)
      {
        Point p = Point.CastOrCreate(point);
        nearPoint = _listPoint.Near(p).Value;
      }

      // _MeshLine l0 = FindLine_(point, nearPoint);
      MeshLineEx l0 = FindLine(point, nearPoint);
      return Add(point, l0);
    }

    private MeshPoint Add(IPoint point, MeshLineEx line)
    {
      TriEx tri0 = line.LeftTriEx;

      if (tri0 == null)
      { // point outside of current mesh
        if (Point2D.Dist2(point, line.Start) <= _fuzzy2)
        {
          _inserted = true;
          return line.StartPointEx;
        }
        if (Point2D.Dist2(point, line.End) <= _fuzzy2)
        {
          _inserted = true;
          return line.EndPointEx;
        }

        return AddBorder(point, line);
      }

      return Add(point, tri0);
    }

    private MeshPoint Add(IPoint point, TriEx tri0)
    {
      for (int i = 0; i < 3; i++)
      {
        if (Point2D.Dist2(point, tri0[i].Start) <= _fuzzy2)
        {
          _inserted = true;
          return tri0._GetLine(i).StartPointEx;
        }
      }

      MeshPoint newPoint = new MeshPoint(point);
      MeshLineEx l0 = new MeshLineEx(tri0._GetLine(0).StartPointEx, newPoint);
      MeshLineEx l1 = new MeshLineEx(tri0._GetLine(1).StartPointEx, newPoint);
      MeshLineEx l2 = new MeshLineEx(tri0._GetLine(2).StartPointEx, newPoint);

      newPoint.IndexLine = -l0;

      TriEx tri1 = new TriEx(tri0._GetLine(1), l2, -l1);
      TriEx tri2 = new TriEx(tri0._GetLine(2), l0, -l2);

      tri0.ClearCircle();
      tri0._SetLine(1, l1);
      tri0._SetLine(2, -l0);

      if (!tri0.Validate()) throw new InvalidProgramException();

      tri0[0].OptionChangeDiag(0);
      tri1[0].OptionChangeDiag(0);
      tri2[0].OptionChangeDiag(0);

      return newPoint;
    }

    //
    //    int M2MovePoint(Mesh2 *mesh,Coo2 *co2,int i,double x,double y)
    //    {
    //      int k,t;
    //      int l0,l1,l2;
    //
    //      co2->x[i] = x;
    //      co2->y[i] = y;
    //
    //      M2WarningMsg("***M2MovePoint : not completely implemented !\n");
    //      M2WarningMsg("***M2MovePoint : current Posistion : %12.1f %12.1f\n",x,y);
    //      t = mesh->ptri[i];
    //      if (t < 0) 
    //      {
    //        M2AddPoint(mesh,co2,i,-t);
    //      } 
    //      else 
    //      {
    //        k = M2GetTriPartPoint(mesh,t,i);
    //        if (k < 0) 
    //        {
    //          M2ErrorMsg("%i %.1f %.1f\n",i,co2->x[i],co2->y[i]);
    //        }
    //        l0 = M2GetTriLine(mesh,t,k);
    //        l2 = l0;
    //        do 
    //        {
    //          if (t > 0) 
    //          {
    //            l1 = M2GetNextLine(mesh,t,l2);
    //            M2OptionChangeDiag(mesh,l1,co2);
    //          }
    //          l2 = M2GetNextPointLine(mesh,l2);
    //          t = M2GetLeftTri(mesh,l2);
    //        } while (l2 != l0);
    //      }
    //      return 0;
    //    }
    //
    //    int M2BorderClean(Mesh2 *mesh,Coo2 *co2,double angle)
    //    {
    //      int i,l0,t;
    //      int e0,e1;
    //      int n;
    //      double sa,f;
    //      double dx0,dx1,dy0,dy1;
    //  
    //      sa = sin(angle);
    //      n = 0;
    //      for (i = 1; i < mesh->nl; i++) 
    //      {
    //        if (mesh->line.id[i] == 0) 
    //        {
    //          l0 = 0;
    //          if (mesh->line.lt[i] == 0) 
    //          {
    //            l0 = i;
    //          } 
    //          else if (mesh->line.rt[i] == 0) 
    //          {
    //            l0 = -i;
    //          }
    //          if (l0 != 0) 
    //          {
    //            t = M2GetLeftTri(mesh,-l0);
    //            e0 = M2GetStartPoint(mesh,l0);
    //            e1 = M2GetStartPoint(mesh,-l0);
    //            dx0 = co2->x[e0] - mesh->tri.x[t];
    //            dy0 = co2->y[e0] - mesh->tri.y[t];
    //            dx1 = co2->x[e1] - mesh->tri.x[t];
    //            dy1 = co2->y[e1] - mesh->tri.y[t];
    //            f = (dx0 * dy1 - dx1 * dy0) / mesh->tri.r2[t];
    //            if (f > 0. && f < sa) 
    //            {  /* remove triangle t , if possible */
    //              int l1,l2,e2;
    //              /*check for double border triangle */
    //              l1 = -M2GetNextLine(mesh,t,-l0);
    //              l2 = -M2GetNextLine(mesh,t,-l1);
    //              if (M2GetLeftTri(mesh,l1) != 0 && M2GetLeftTri(mesh,l2) != 0) 
    //              {
    //                /* validate ptri of edge points */
    //                e2 = M2GetStartPoint(mesh,l1);
    //            
    //                mesh->ptri[e0] = M2GetLeftTri(mesh,l1);
    //                mesh->ptri[e1] = M2GetLeftTri(mesh,l2);
    //                mesh->ptri[e2] = M2GetLeftTri(mesh,l2);
    //            
    //                M2GetTriPartPoint(mesh,M2GetLeftTri(mesh,l1),e0);
    //                M2GetTriPartPoint(mesh,M2GetLeftTri(mesh,l2),e1);
    //                M2GetTriPartPoint(mesh,M2GetLeftTri(mesh,l2),e2);
    //
    //                M2RmLine(mesh,-l0);
    //                n++;
    //                i --; /* check new line assigned to same line # */
    //              }
    //            }
    //          }
    //        }
    //      }
    //      M2StatusMsg("Dropped %i border triangles\n",n);
    //      return n;
    //    }
    //
    //    int M2DropPoint(Mesh2 *mesh,Coo2 *co,int p)
    //    {
    //      int i;
    //      int l,l0,l1,l2,ln;
    //      int t1,t2;
    //      int p1;
    //      double dx,dy,d2,d20;
    //
    //      if (mesh->ptri[p] <= 0) 
    //      {
    //        return mesh->ptri[p];
    //      }
    //
    //      /* find closest point and reset IDs */
    //      l0 = M2GetPointLine(mesh,p);
    //      l = l0;
    //      d20 = -1.;
    //      do 
    //      {
    //        M2SetLineId(mesh,l,0);
    //        /* check for border */
    //        if (M2GetLeftTri(mesh,l) == 0) 
    //        {
    //          return -1;
    //        }
    //        p1 = M2GetStartPoint(mesh,-l);
    //        dx = co->x[p1] - co->x[p];
    //        dy = co->y[p1] - co->y[p];
    //        d2 = dx * dx + dy * dy;
    //        if (d20 < 0. || d2 < d20) 
    //        {
    //          d20 = d2;
    //          ln = l;
    //        }
    //        l = M2GetNextPointLine(mesh,l);
    //      } while (l != l0);
    //
    //      t1 = M2GetLeftTri(mesh,ln);
    //      t2 = M2GetLeftTri(mesh,-ln);
    //      l1 = M2GetNextLine(mesh,t1,ln);
    //      l2 = M2GetPrevLine(mesh,t2,-ln);
    //
    //      /* "move" p to p0, calculate new out circles
    //         drop ln,l0,l2
    //      */
    //      mesh->ptri[p] = 0;
    //      return 1;
    //    }
    //
    //    int M2ListLines(Mesh2 *mesh,FILE *fout)
    //    {
    //      int i;
    //      MLine *l;
    //
    //      l = &mesh->line;
    //      if (fout == 0) fout = stdout;
    //      fprintf(fout,"   #     sp   ep   lt   rt\n");
    //      for (i = 1; i <= mesh->nl; i++) 
    //      {
    //        fprintf(fout,"%4i    %4i %4i  %4i %4i\n",
    //          i,l->sp[i],l->ep[i],l->lt[i],l->rt[i]);
    //      }
    //      return 0;
    //    }
    //
    //    int M2ListTri(Mesh2 *mesh,int i,FILE *fout)
    //    {
    //      int l0,l1,l2;
    //      if (fout == 0) fout = stdout;
    //      fprintf(fout,"   #     l0     l1     l2         s0     s1     s2");
    //      fprintf(fout,"        rt     rt     rt\n");
    //      fprintf(fout,"                                  e0     e1     e2");
    //      fprintf(fout,"        lt     lt     lt\n");
    //      l0 = mesh->tri.l0[i];
    //      l1 = mesh->tri.l1[i];
    //      l2 = mesh->tri.l2[i];
    //      fprintf(fout,"%6i  %6i %6i %6i    %6i %6i %6i    %6i %6i %6i\n",
    //        i,l0,l1,l2,M2GetStartPoint(mesh,l0),M2GetStartPoint(mesh,l1),
    //        M2GetStartPoint(mesh,l2),
    //        M2GetLeftTri(mesh,-l0),M2GetLeftTri(mesh,-l1),M2GetLeftTri(mesh,-l2));
    //      fprintf(fout,"                              %5i %5i %5i    %5i %5i %5i\n",
    //        M2GetStartPoint(mesh,-l0),M2GetStartPoint(mesh,-l1),
    //        M2GetStartPoint(mesh,-l2),
    //        M2GetLeftTri(mesh,l0),M2GetLeftTri(mesh,l1),M2GetLeftTri(mesh,l2));
    //      return 0;
    //    }
    //
    //    int M2ListTris(Mesh2 *mesh,FILE *fout)
    //    {
    //      int i;
    //      if (fout == 0) fout = stdout;
    //      fprintf(fout,"   #     l0    l1    l2\n");
    //      for (i = 1; i <= mesh->nt; i++) 
    //      {
    //        fprintf(fout,"%4i  %5i %5i %5i\n",
    //          i,mesh->tri.l0[i],mesh->tri.l1[i],mesh->tri.l2[i]);
    //      }
    //      return 0;
    //    }
    //
    //    int  (*m2StatusMsg)(char *format,va_list args) = 0;
    //    int (*m2WarningMsg)(char *format,va_list args) = 0;
    //    int   (*m2ErrorMsg)(char *format,va_list args) = 0;
    //
    //    int M2SetStatusMsgFct(int (*fct)(char *format,va_list args))
    //  {
    //    m2StatusMsg = fct;
    //    return 0;
    //  }
    //  int M2SetWarningMsgFct(int (*fct)(char *format,va_list args))
    //{
    //  m2WarningMsg = fct;
    //  return 0;
    //}
    //  int M2SetErrorMsgFct(int (*fct)(char *format,va_list args))
    //{
    //  m2ErrorMsg = fct;
    //  return 0;
    //}
    //  int M2StatusMsg(char *format,...)
    //{
    //  va_list args;
    //  if (m2StatusMsg) 
    //{
    //  va_start(args,format);
    //  m2StatusMsg(format,args);
    //  va_end(args);
    //}
    //  return 0;
    //}
    //
    //  int M2WarningMsg(char *format, ...)
    //{
    //  va_list args;
    //  va_start(args,format);
    //  if (m2WarningMsg) 
    //{
    //  m2WarningMsg(format,args);
    //} 
    //  else 
    //{
    //  vfprintf(stdout,format,args);
    //}
    //  va_end(args);
    //  return 0;
    //}
    //
    //  int M2ErrorMsg(char *format, ...)
    //{
    //  va_list args;
    //  va_start(args,format);
    //  if (m2ErrorMsg) 
    //{
    //  m2ErrorMsg(format,args);
    //} 
    //  else 
    //{
    //  vfprintf(stderr,format,args);
    //}
    //  va_end(args);
    //  return -1;
    //}
  }
}
