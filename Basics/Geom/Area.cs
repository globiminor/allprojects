using System;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Area.
  /// </summary>
  public class Area : Geometry, IGeometry
  {
    PolylineCollection _lines_;
    BoxTree<Polyline> _spatialIndex;

    public Area()
    {
      _lines_ = new PolylineCollection();
    }
    public Area(Polyline border)
    {
      _lines_ = new PolylineCollection();
      _lines_.Add(border);
    }
    public Area(PolylineCollection border)
    {
      _lines_ = border;
    }
    public Area(Polyline[] border)
    {
      _lines_ = new PolylineCollection(border);
    }
    #region IGeometry Members

    protected virtual PolylineCollection _lines
    {
      get { return _lines_; }
    }
    public override int Dimension
    {
      get { return _lines.Dimension; }
    }

    public override int Topology
    {
      get { return 2; }
    }

    private BoxTree<Polyline> SpatialIndex
    {
      get
      {
        if (_spatialIndex == null)
        {
          _spatialIndex = new BoxTree<Polyline>(2);
          _spatialIndex.InitSize(new IGeometry[] { Extent });
          foreach (var line in _lines)
          { _spatialIndex.Add(line.Extent, line); }
        }
        return _spatialIndex;
      }
    }

    public override bool IsWithin(IPoint p)
    {
      Point down = Point.Create(p);
      Line lineDown = new Line(p, down);

      int i = 0;

      foreach (var border in _lines)
      {
        Relation rel = border.Extent.RelationTo(p.Extent);
        if (rel == Relation.Disjoint)
        { continue; }

        if (border.Points.First.Value.EqualGeometry(border.Points.Last.Value) == false)
        { border.Add(border.Points.First.Value); }

        double? xPre = null;
        foreach (var curve in border.Segments)
        {
          IBox extent = curve.Extent;

          if (VerifyExtent(p, extent) == false)
          { continue; }

          int iNew = Intersects(p, lineDown, curve, extent, ref xPre);
          if (iNew < 0) // Touch
          { return true; }
          i += iNew;
        }
      }
      if ((i & 1) == 1)
      { return true; }
      else
      { return false; }
    }

    private int Intersects(IPoint isInside, Line lineDown, Curve curve, IBox extent,
      ref double? xPre)
    {
      if (curve is IMultipartGeometry multi && multi.HasSubparts)
      {
        int n = 0;
        foreach (var part in multi.Subparts())
        {
          IBox partExtent = part.Extent;
          Curve subCurve = (Curve)part;

          if (VerifyExtent(isInside, partExtent) == false)
          { continue; }

          int nNew = Intersects(isInside, lineDown, subCurve, partExtent, ref xPre);
          if (nNew < 0) // Touch
          { return nNew; }
          else
          { n += nNew; }
        }
        return n;
      }

      // Remark: Assert.IsTrue(isInside[dim.Y] >= extent.Min[dim.Y]);

      double x = isInside.X;
      if (extent.Min.X == x)
      {
        if (extent.Max.X == x)
        { // senkrechte Linie
          if (extent.Max.Y >= isInside.Y)
          {
            xPre = null;
            return -1;
          } // Touch
        }
        return GetFromPreviousSide(isInside, curve, ref xPre); // TODO
      }
      else if (x == extent.Max.X)
      {
        return GetFromPreviousSide(isInside, curve, ref xPre);
      }
      else // (extent.Min[dim.X] < x && x < extent.Max[dim.X])
      {
        if (isInside.Y > extent.Max.Y)
        { return 1; }

        lineDown.End.Y = extent.Min.Y;
        GeometryCollection intersects = curve.Intersection(lineDown);
        if (intersects != null)
        {
          if (intersects.Count > 1)
          { throw new InvalidOperationException("Not expecting multiple intersects"); }
          return intersects.Count;
        }
        else
        { return 0; }
      }
    }

    private int GetFromPreviousSide(IPoint isInside, Curve curve, ref double? xPre)
    {
      IPoint p, q;
      double x = isInside.X;

      if (curve.Start.X == x)
      {
        p = curve.Start;
        q = curve.End;
      }
      else
      { // Assert.IsTrue(curve.End[dim.X] == x)
        p = curve.End;
        q = curve.Start;
      }
      double d = p.Y - isInside.Y;
      if (d > 0)
      { return 0; }
      else if (d == 0)
      { return -1; } // touch

      if (xPre.HasValue)
      {
        int inside = 1;
        if (xPre.Value < x == q.X < x)
        { inside = 0; }
        xPre = null;
        return inside;
      }
      else
      {
        xPre = q.X;
        return 0;
      }
    }
    private bool VerifyExtent(IPoint p, IBox extent)
    {
      if (p.X < extent.Min.X || p.X > extent.Max.X)
      { return false; }

      if (p.Y < extent.Min.Y)
      { return false; }

      return true;
    }

    /// <summary>
    /// only valid if polygon is defined clockwise
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsWithin_clockwiseArea(IPoint p)
    {
      Point2D min = new Point2D(p.X, p.Y);
      Point2D max = new Point2D(Extent.Max.X, p.Y);
      IPoint near = null;
      bool onRightSide = false;

      Line searchLine = new Line(min, max);
      Box searchBox = new Box(min, max);

      foreach (var eLine in SpatialIndex.Search(searchBox))
      {
        Polyline line = eLine.Value;
        if (Point.Equals(line.Points.First.Value, line.Points.Last.Value) == false)
        { line.Add(line.Points.First.Value); }
        foreach (var eCurve in line.SpatialIndex.Search(searchBox))
        {
          Curve curve = eCurve.Value;
          bool within = IsWithinCurve(p, curve, max, searchLine, ref near, ref onRightSide);
          if (within)
          { return true; }
        }
      }
      return onRightSide;
    }

    private bool IsWithinCurve(IPoint p, Curve curve, Point2D max, Line searchLine,
      ref IPoint near, ref bool onRightSide)
    {
      bool within = false;
      if (curve is IMultipartGeometry parts && parts.HasSubparts)
      {
        foreach (var part in parts.Subparts())
        {
          Curve subCurve = (Curve)part;
          within = IsWithinCurve(p, subCurve, max, searchLine,
            ref near, ref onRightSide);
          if (within)
          { break; }
        }
      }
      else
      {
        within = IsWithinCurveSimple(p, max, ref near, ref onRightSide,
        searchLine, curve);
      }
      return within;
    }

    private bool IsWithinCurveSimple(IPoint p, Point2D max,
      ref IPoint near, ref bool onRightSide, Line searchLine, Curve curve)
    {
      if (curve.Start.Y == p.Y)
      {
        if (curve.End.Y == p.Y)
        {
          max.X = Math.Max(curve.Start.X, curve.End.Y);
          if (max.X <= p.X)
          { return true; }
        }
        else
        { OnRightSide(p, max, curve.Start, curve.End, true, ref near, ref onRightSide); }
      }
      else if (curve.End.Y == p.Y)
      { OnRightSide(p, max, curve.End, curve.Start, false, ref near, ref onRightSide); }
      else
      {
        GeometryCollection cutList = curve.Intersection(searchLine);

        if (cutList == null)
        { return false; }

        Point2D cut = (Point2D)cutList[0];
        System.Diagnostics.Debug.Assert(cut.X >= p.X && cut.X <= max.X);

        max.X = cut.X;
        onRightSide = (curve.Start.Y > p.Y);
      }
      if (max.X == p.X)
      { return true; }
      return false;
    }

    private void OnRightSide(IPoint p, IPoint max, IPoint onLine, IPoint offLine,
      bool startOnLine, ref IPoint near, ref bool onRightSide)
    {
      if (onLine.X < p.X || onLine.X > max.X)
      { return; }
      else if (near == null || onLine.X < max.X)
      {
        near = PointOperator.Sub(offLine, onLine);
        max.X = onLine.X;
        onRightSide = (near.Y < 0) == startOnLine;
      }
      else if ((near.Y > 0) == (offLine.Y > p.Y))
      {
        Point n = PointOperator.Sub(offLine, onLine);
        if ((n.VectorProduct(near) < 0) == (n.Y > 0))
        {
          near = n;
          onRightSide = (near.Y < 0) == startOnLine;
        }
      }
    }

    public override IBox Extent
    {
      get
      { return _lines.Extent; }
    }
    IBox IGeometry.Extent
    { get { return Extent; } }

    public PolylineCollection Border
    {
      get { return _lines; }
    }
    protected override IGeometry BorderGeom
    { get { return Border; } }
    IGeometry IGeometry.Border
    { get { return Border; } }

    private Area Project__(IProjection projection)
    {
      return new Area(_lines.Project(projection));
    }
    protected override Geometry Project_(IProjection projection)
    { return Project__(projection); }
    public new Area Project(IProjection projection)
    { return (Area)Project_(projection); }
    IGeometry IGeometry.Project(IProjection projection)
    { return Project(projection); }

    #endregion

    public void CloseBorders()
    {
      System.Collections.Generic.List<Polyline> invalidList = new System.Collections.Generic.List<Polyline>();
      foreach (var border in Border)
      {
        if (border.Points.Count == 0)
        {
          invalidList.Add(border);
          continue;
        }
        Point start = Point.CastOrCreate(border.Points.First.Value);
        if (start.Dist2(border.Points.Last.Value) > 1e-12)
        { border.Add(start); }
      }
      foreach (var invalid in invalidList)
      {
        Border.Remove(invalid);
      }
    }
  }
}
